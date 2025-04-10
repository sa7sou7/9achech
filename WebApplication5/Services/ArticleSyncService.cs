using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication5.Data;
using WebApplication5.Dto;
using WebApplication5.Models;
using Microsoft.Extensions.Logging;

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

            _maxRetries = int.TryParse(_config["ExternalApi:MaxRetries"], out var retries) ? retries : 3;
            _retryDelayMs = int.TryParse(_config["ExternalApi:RetryDelayMs"], out var delay) ? delay : 1000;
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
                    if (i == _maxRetries - 1)
                    {
                        _logger.LogError(ex, $"Failed to fetch data from {endpoint} after {_maxRetries} retries");
                        throw;
                    }
                    _logger.LogWarning(ex, $"Retry {i + 1}/{_maxRetries} for {endpoint}");
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
                if (json == null) return false;

                // Deserialize using ArticleSyncDto to match the API response
                var articleDtos = JsonSerializer.Deserialize<List<ArticleSyncDto>>(json);
                if (articleDtos == null) return false;

                foreach (var dto in articleDtos)
                {
                    var article = new Article
                    {
                        Code = dto.Code,
                        Designation = dto.Designation,
                        Famille = string.IsNullOrEmpty(dto.Famille) ? string.Empty : dto.Famille,
                        PrixAchat = decimal.TryParse(dto.PrixAchat, out var prixAchat) ? prixAchat : 0,
                        PrixVente = decimal.TryParse(dto.PrixVente, out var prixVente) ? prixVente : 0,
                        StockQuantity = 0 // Default to 0 since the API doesn't provide it
                    };

                    var exists = await _context.Articles.AnyAsync(a => a.Code == article.Code);
                    if (!exists)
                    {
                        _context.Articles.Add(article);
                    }
                    else
                    {
                        var existing = await _context.Articles.FirstAsync(a => a.Code == article.Code);
                        existing.Designation = article.Designation;
                        existing.Famille = article.Famille;
                        existing.PrixAchat = article.PrixAchat;
                        existing.PrixVente = article.PrixVente;
                        // Don't overwrite StockQuantity since it's managed manually
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing articles");
                return false;
            }
        }
    }
}