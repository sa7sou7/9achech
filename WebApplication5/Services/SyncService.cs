using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication5.Data;
using WebApplication5.Dto;
using WebApplication5.Models;

namespace WebApplication5.Services
{
    public class SyncService : ISyncService
    {
        private readonly IApiService _apiService;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SyncService> _logger;

        public SyncService(
            IApiService apiService,
           AppDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<SyncService> logger)
        {
            _apiService = apiService;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<string> SynchronizeClientsAsync()
        {
            try
            {
                _logger.LogInformation("Starting client synchronization...");

                var clientsFromApi = await _apiService.GetDataAsync("v1/Tiers/CL");
                _logger.LogInformation($"Raw API response: {clientsFromApi}");

                if (string.IsNullOrEmpty(clientsFromApi))
                {
                    _logger.LogWarning("Empty response received from clients API");
                    return "No data received from API";
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var clients = JsonSerializer.Deserialize<List<ApiClientDto>>(clientsFromApi, options) ?? new List<ApiClientDto>();
                _logger.LogInformation($"Deserialized {clients.Count} clients from API response");

                if (!clients.Any())
                {
                    _logger.LogWarning("No clients found in API response");
                    return "No clients found in API response";
                }

                _logger.LogInformation($"Processing {clients.Count} clients...");

                int createdCount = 0;
                int updatedCount = 0;
                int skippedCount = 0;

                foreach (var apiClient in clients)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(apiClient?.Code))
                        {
                            _logger.LogWarning("Skipping client with null/empty Code");
                            skippedCount++;
                            continue;
                        }

                        var trimmedCode = apiClient.Code.Trim();
                        _logger.LogInformation($"Processing client with Code: {trimmedCode}");

                        var existingClient = await _context.Tiers.FirstOrDefaultAsync(t => t.Matricule == trimmedCode);
                        if (existingClient != null)
                        {
                            _logger.LogInformation($"Found existing client with Matricule: {trimmedCode}");
                            existingClient.Nom = apiClient.Rs?.Trim() ?? existingClient.Nom;
                            existingClient.Adresse = apiClient.Adresse?.Trim() ?? existingClient.Adresse;
                            existingClient.Ville = apiClient.Ville?.Trim() ?? existingClient.Ville;
                            existingClient.Delegation = apiClient.Delegation?.Trim() ?? existingClient.Delegation;
                            existingClient.SecteurActiv = apiClient.SecteurActiv?.Trim() ?? existingClient.SecteurActiv;
                            existingClient.Tel = apiClient.Tel?.Trim() ?? existingClient.Tel;
                            updatedCount++;
                        }
                        else
                        {
                            _logger.LogInformation($"Creating new client with Matricule: {trimmedCode}");
                            var newClient = new Tiers
                            {
                                Matricule = trimmedCode,
                                Nom = apiClient.Rs?.Trim() ?? "N/A",
                                Adresse = apiClient.Adresse?.Trim() ?? "N/A",
                                Ville = apiClient.Ville?.Trim(),
                                Delegation = apiClient.Delegation?.Trim(),
                                SecteurActiv = apiClient.SecteurActiv?.Trim(),
                                Tel = apiClient.Tel?.Trim(),
                                Statut = StatutProspect.Client,
                                Cin = string.Empty
                            };
                            _context.Tiers.Add(newClient);
                            createdCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing client {apiClient?.Code}");
                    }
                }

                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation($"Client sync completed. Created: {createdCount}, Updated: {updatedCount}, Skipped: {skippedCount}, Total changes: {changes}");
                return $"Clients synchronized successfully. Created: {createdCount}, Updated: {updatedCount}, Skipped: {skippedCount}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing clients");
                return $"Client synchronization failed: {ex.Message}";
            }
        }

        public async Task<string> SynchronizeCommercialsAsync()
        {
            try
            {
                _logger.LogInformation("Starting commercial synchronization...");

                var commercialsFromApi = await _apiService.GetDataAsync("v1/Tiers/RE");
                _logger.LogInformation($"Raw API response: {commercialsFromApi}");

                if (string.IsNullOrEmpty(commercialsFromApi))
                {
                    _logger.LogWarning("Empty response received from commercials API");
                    return "No data received from API";
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var commercials = JsonSerializer.Deserialize<List<ApiCommercialDto>>(commercialsFromApi, options) ?? new List<ApiCommercialDto>();
                _logger.LogInformation($"Deserialized {commercials.Count} commercials from API response");

                if (!commercials.Any())
                {
                    _logger.LogWarning("No commercials found in API response");
                    return "No commercials found in API response";
                }

                _logger.LogInformation($"Processing {commercials.Count} commercials...");

                int createdCount = 0;
                int updatedCount = 0;
                int skippedCount = 0;

                foreach (var apiCommercial in commercials)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(apiCommercial?.Cref))
                        {
                            _logger.LogWarning("Skipping commercial with null/empty Cref");
                            skippedCount++;
                            continue;
                        }

                        var trimmedCref = apiCommercial.Cref.Trim();
                        _logger.LogInformation($"Processing commercial with Cref: {trimmedCref}");

                        var existingCommercial = await _context.Commercials.FirstOrDefaultAsync(c => c.Cref == trimmedCref);
                        if (existingCommercial != null)
                        {
                            _logger.LogInformation($"Found existing commercial with Cref: {trimmedCref}");
                            existingCommercial.Cnom = apiCommercial.Cnom?.Trim() ?? existingCommercial.Cnom;
                            existingCommercial.CPrenom = apiCommercial.CPrenom?.Trim() ?? existingCommercial.CPrenom;
                            updatedCount++;
                        }
                        else
                        {
                            var user = await _userManager.FindByNameAsync(trimmedCref);
                            if (user == null)
                            {
                                user = new IdentityUser
                                {
                                    UserName = trimmedCref,
                                    Email = $"{trimmedCref}@company.com",
                                    EmailConfirmed = true
                                };
                                var result = await _userManager.CreateAsync(user, "DefaultPassword123!");
                                if (!result.Succeeded)
                                {
                                    _logger.LogError($"Failed to create user {trimmedCref}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                                    skippedCount++;
                                    continue;
                                }

                                if (!await _roleManager.RoleExistsAsync("Commercial"))
                                    await _roleManager.CreateAsync(new IdentityRole("Commercial"));
                                await _userManager.AddToRoleAsync(user, "Commercial");
                            }

                            _logger.LogInformation($"Creating new commercial with Cref: {trimmedCref}");
                            var newCommercial = new Commercial
                            {
                                Cref = trimmedCref,
                                Cnom = apiCommercial.Cnom?.Trim() ?? "N/A",
                                CPrenom = apiCommercial.CPrenom?.Trim() ?? "N/A",
                                UserId = user.Id
                            };
                            _context.Commercials.Add(newCommercial);
                            createdCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing commercial {apiCommercial?.Cref}");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Commercial sync completed. Created: {createdCount}, Updated: {updatedCount}, Skipped: {skippedCount}");
                return $"Commercials synchronized successfully. Created: {createdCount}, Updated: {updatedCount}, Skipped: {skippedCount}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing commercials");
                return $"Commercial synchronization failed: {ex.Message}";
            }
        }

        public async Task<string> SynchronizeSalesAsync()
        {
            try
            {
                _logger.LogInformation("Starting sales synchronization...");

                var salesFromApi = await _apiService.GetDataAsync("v1/Tiers/CA");
                _logger.LogInformation($"Raw API response: {salesFromApi}");

                if (string.IsNullOrEmpty(salesFromApi))
                {
                    _logger.LogWarning("Empty response received from sales API");
                    return "No data received from API";
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var sales = JsonSerializer.Deserialize<List<SaleDto>>(salesFromApi, options) ?? new List<SaleDto>();
                _logger.LogInformation($"Deserialized {sales.Count} sales from API response");

                if (!sales.Any())
                {
                    _logger.LogWarning("No sales found in API response");
                    return "No sales found in API response";
                }

                _logger.LogInformation($"Processing {sales.Count} sales...");

                int createdCount = 0;
                int updatedCount = 0;
                int skippedCount = 0;

                foreach (var saleDto in sales)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(saleDto?.DocRef) || string.IsNullOrWhiteSpace(saleDto?.DocTiers))
                        {
                            _logger.LogWarning($"Skipping sale with missing reference or client: {saleDto?.DocRef}");
                            skippedCount++;
                            continue;
                        }

                        var trimmedDocRef = saleDto.DocRef.Trim();
                        var trimmedDocTiers = saleDto.DocTiers.Trim();
                        _logger.LogInformation($"Processing sale with DocRef: {trimmedDocRef}, DocTiers: {trimmedDocTiers}");

                        var client = await _context.Tiers.FirstOrDefaultAsync(t => t.Matricule == trimmedDocTiers);
                        if (client == null)
                        {
                            _logger.LogWarning($"Client not found for sale {trimmedDocRef} (Client ID: {trimmedDocTiers})");
                            skippedCount++;
                            continue;
                        }

                        var existingSale = await _context.Sales.FirstOrDefaultAsync(s => s.DocRef == trimmedDocRef);
                        if (existingSale != null)
                        {
                            _logger.LogInformation($"Found existing sale with DocRef: {trimmedDocRef}");
                            existingSale.TiersId = client.Id;
                            existingSale.DocRepresentant = saleDto.DocRepresentant?.Trim() ?? existingSale.DocRepresentant;
                            existingSale.DocNetAPayer = decimal.TryParse(saleDto.DocNetaPayer, out var net) ? net : existingSale.DocNetAPayer;
                            existingSale.DocDate = DateTime.TryParse(saleDto.DocDate, out var date) ? date : existingSale.DocDate;
                            updatedCount++;
                        }
                        else
                        {
                            _logger.LogInformation($"Creating new sale with DocRef: {trimmedDocRef}");
                            var newSale = new Sale
                            {
                                DocRef = trimmedDocRef,
                                TiersId = client.Id,
                                DocRepresentant = saleDto.DocRepresentant?.Trim() ?? "N/A",
                                DocNetAPayer = decimal.TryParse(saleDto.DocNetaPayer, out var net) ? net : 0,
                                DocDate = DateTime.TryParse(saleDto.DocDate, out var date) ? date : DateTime.MinValue
                            };
                            _context.Sales.Add(newSale);
                            createdCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing sale {saleDto?.DocRef}");
                    }
                }

                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation($"Sales sync completed. Created: {createdCount}, Updated: {updatedCount}, Skipped: {skippedCount}, Total changes: {changes}");
                return $"Sales synchronized successfully. Created: {createdCount}, Updated: {updatedCount}, Skipped: {skippedCount}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing sales");
                return $"Sales synchronization failed: {ex.Message}";
            }
        }
    }
}