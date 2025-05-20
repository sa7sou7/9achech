using System.Text.Json.Serialization;

namespace WebApplication5.Dto
{
    public class ArticleSyncDto
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("Designation")]
        public string? Designation { get; set; }

        [JsonPropertyName("Désignation")]
        public string? DesignationFR { get; set; }

        [JsonPropertyName("description")] // Common alternative
        public string? Description { get; set; }

        [JsonPropertyName("name")] // Another common alternative
        public string? Name { get; set; }

        [JsonPropertyName("libelle")] // French term for "label" or "description"
        public string? Libelle { get; set; }

        [JsonPropertyName("article_name")] // Another possible alternative
        public string? ArticleName { get; set; }

        [JsonPropertyName("Famille")]
        public string? Famille { get; set; }

        [JsonPropertyName("PrixAchat")]
        public string? PrixAchat { get; set; }

        [JsonPropertyName("PrixVente")]
        public string? PrixVente { get; set; }

        public string GetDesignation() =>
            !string.IsNullOrWhiteSpace(Designation) ? Designation :
            !string.IsNullOrWhiteSpace(DesignationFR) ? DesignationFR :
            !string.IsNullOrWhiteSpace(Description) ? Description :
            !string.IsNullOrWhiteSpace(Name) ? Name :
            !string.IsNullOrWhiteSpace(Libelle) ? Libelle :
            !string.IsNullOrWhiteSpace(ArticleName) ? ArticleName :
            "No Designation";
    }
}