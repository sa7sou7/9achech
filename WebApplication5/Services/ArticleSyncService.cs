using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication5.Data;
using WebApplication5.Dto;
using WebApplication5.Models;

namespace WebApplication5.Services
{
    public class ArticleSyncService : IArticleSyncService
    {
        private readonly IApiService _apiService;
        private readonly AppDbContext _context;
        private readonly ILogger<ArticleSyncService> _logger;
        private readonly IConfiguration _config;
        private readonly int _maxRetries;
        private readonly int _retryDelayMs;

        public ArticleSyncService(
            IApiService apiService,
            AppDbContext context,
            ILogger<ArticleSyncService> logger,
            IConfiguration config)
        {
            _apiService = apiService;
            _context = context;
            _logger = logger;
            _config = config;
            _maxRetries = _config.GetValue("ExternalApi:MaxRetries", 3);
            _retryDelayMs = _config.GetValue("ExternalApi:RetryDelayMs", 1000);
        }

        private async Task<string?> GetDataWithRetryAsync(string endpoint)
        {
            for (int i = 0; i < _maxRetries; i++)
            {
                try
                {
                    return await _apiService.GetDataAsync(endpoint);
                }
                catch (Exception ex)
                {
                    if (i == _maxRetries - 1) throw;
                    _logger.LogWarning(ex, $"Retry {i + 1}/{_maxRetries}");
                    await Task.Delay(_retryDelayMs);
                }
            }
            return null;
        }

        public async Task<bool> SynchronizeArticlesAsync()
        {
            try
            {
                var endpoint = "https://cmc.crm-edi.info/apisif/public/api/v1/Articlesynchro";
                var json = await GetDataWithRetryAsync(endpoint);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogError("Empty API response");
                    return false;
                }

                // Log the raw JSON response for debugging
                _logger.LogInformation("Raw API Response: {Json}", json);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                var articleDtos = JsonSerializer.Deserialize<List<ArticleSyncDto>>(json, options)
                    ?? new List<ArticleSyncDto>();

                if (!articleDtos.Any())
                {
                    _logger.LogWarning("No articles found in API response");
                    return true; // No articles is not necessarily an error
                }

                var successCount = 0;
                var errorCount = 0;

                foreach (var dto in articleDtos)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(dto.Code))
                        {
                            _logger.LogWarning("Skipped article with empty code");
                            errorCount++;
                            continue;
                        }

                        // Log the DTO values to debug the designation
                        _logger.LogInformation(
                            "Processing article: Code={Code}, Designation={Designation}, DesignationFR={DesignationFR}, Famille={Famille}, PrixAchat={PrixAchat}, PrixVente={PrixVente}",
                            dto.Code, dto.Designation, dto.DesignationFR, dto.Famille, dto.PrixAchat, dto.PrixVente);

                        var article = new Article
                        {
                            Code = dto.Code.Trim(),
                            Designation = SanitizeDesignation(dto.GetDesignation()),
                            Famille = SanitizeFamille(dto.Famille),
                            PrixAchat = ParsePrice(dto.PrixAchat),
                            PrixVente = ParsePrice(dto.PrixVente),
                            StockQuantity = 0
                        };

                        var existing = await _context.Articles
                            .FirstOrDefaultAsync(a => a.Code == article.Code);

                        if (existing == null)
                        {
                            _context.Articles.Add(article);
                            _logger.LogInformation("Added new article: Code={Code}, Designation={Designation}, PrixAchat={PrixAchat}, PrixVente={PrixVente}",
                                article.Code, article.Designation, article.PrixAchat, article.PrixVente);
                            successCount++;
                        }
                        else if (NeedsUpdate(existing, article))
                        {
                            existing.Designation = article.Designation;
                            existing.Famille = article.Famille;
                            existing.PrixAchat = article.PrixAchat;
                            existing.PrixVente = article.PrixVente;
                            _logger.LogInformation("Updated existing article: Code={Code}, Designation={Designation}, PrixAchat={PrixAchat}, PrixVente={PrixVente}",
                                article.Code, article.Designation, article.PrixAchat, article.PrixVente);
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing article {dto.Code}");
                        errorCount++;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Sync completed: {successCount} updated, {errorCount} errors");
                return successCount > 0 || errorCount == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Article synchronization failed");
                return false;
            }
        }

        private string SanitizeDesignation(string designation)
        {
            if (string.IsNullOrWhiteSpace(designation))
                return "No Designation";

            var clean = designation.Trim();
            return clean.Length > 100 ? clean[..100] : clean;
        }

        private string SanitizeFamille(string? famille)
        {
            if (string.IsNullOrWhiteSpace(famille))
                return string.Empty;

            var clean = famille.Trim();
            return clean.Length > 50 ? clean[..50] : clean;
        }

        private decimal ParsePrice(string? priceStr)
        {
            if (string.IsNullOrWhiteSpace(priceStr))
                return 0;

            priceStr = priceStr.Replace(",", ".")
                              .Replace(" ", "")
                              .Replace("€", "");

            if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                return Math.Max(0, price);
            }

            _logger.LogWarning($"Failed to parse price value: {priceStr}");
            return 0;
        }

        private bool NeedsUpdate(Article existing, Article updated)
        {
            return existing.Designation != updated.Designation ||
                   existing.Famille != updated.Famille ||
                   existing.PrixAchat != updated.PrixAchat ||
                   existing.PrixVente != updated.PrixVente;
        }
    }
}