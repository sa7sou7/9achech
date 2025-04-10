using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication5.Dto;
using WebApplication5.Models;
using WebApplication5.Repository;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IArticleSyncService _articleSyncService;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(
            IArticleRepository articleRepository,
            IArticleSyncService articleSyncService,
            ILogger<ArticlesController> logger)
        {
            _articleRepository = articleRepository;
            _articleSyncService = articleSyncService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreateArticle([FromBody] ArticleDto articleDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingArticle = await _articleRepository.GetArticleByCodeAsync(articleDto.Code);
            if (existingArticle != null)
                return BadRequest("An article with this code already exists.");

            var article = new Article
            {
                Code = articleDto.Code,
                Designation = articleDto.Designation,
                Famille = string.IsNullOrEmpty(articleDto.Famille) ? null : articleDto.Famille,
                PrixAchat = articleDto.PrixAchat,
                PrixVente = articleDto.PrixVente,
                StockQuantity = articleDto.StockQuantity
            };

            var createdArticle = await _articleRepository.CreateArticleAsync(article);
            articleDto.Id = createdArticle.Id;
            return CreatedAtAction(nameof(GetArticle), new { id = createdArticle.Id }, articleDto);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetArticle(int id)
        {
            var article = await _articleRepository.GetArticleByIdAsync(id);
            if (article == null) return NotFound();

            var articleDto = new ArticleDto
            {
                Id = article.Id,
                Code = article.Code,
                Designation = article.Designation,
                Famille = article.Famille ?? string.Empty,
                PrixAchat = article.PrixAchat,
                PrixVente = article.PrixVente,
                StockQuantity = article.StockQuantity
            };

            return Ok(articleDto);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllArticles()
        {
            var articles = await _articleRepository.GetAllArticlesAsync();
            var articleDtos = articles.Select(a => new ArticleDto
            {
                Id = a.Id,
                Code = a.Code,
                Designation = a.Designation,
                Famille = a.Famille ?? string.Empty,
                PrixAchat = a.PrixAchat,
                PrixVente = a.PrixVente,
                StockQuantity = a.StockQuantity
            });
            return Ok(articleDtos);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchArticles([FromQuery] string searchTerm)
        {
            var articles = await _articleRepository.SearchArticlesAsync(searchTerm);
            var articleDtos = articles.Select(a => new ArticleDto
            {
                Id = a.Id,
                Code = a.Code,
                Designation = a.Designation,
                Famille = a.Famille ?? string.Empty,
                PrixAchat = a.PrixAchat,
                PrixVente = a.PrixVente,
                StockQuantity = a.StockQuantity
            });
            return Ok(articleDtos);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] ArticleDto articleDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != articleDto.Id) return BadRequest("ID mismatch.");

            var existingArticle = await _articleRepository.GetArticleByIdAsync(id);
            if (existingArticle == null) return NotFound();

            var articleWithSameCode = await _articleRepository.GetArticleByCodeAsync(articleDto.Code);
            if (articleWithSameCode != null && articleWithSameCode.Id != id)
                return BadRequest("An article with this code already exists.");

            existingArticle.Code = articleDto.Code;
            existingArticle.Designation = articleDto.Designation;
            existingArticle.Famille = string.IsNullOrEmpty(articleDto.Famille) ? null : articleDto.Famille;
            existingArticle.PrixAchat = articleDto.PrixAchat;
            existingArticle.PrixVente = articleDto.PrixVente;
            existingArticle.StockQuantity = articleDto.StockQuantity;

            var updated = await _articleRepository.UpdateArticleAsync(existingArticle);
            return updated ? NoContent() : StatusCode(500);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var deleted = await _articleRepository.DeleteArticleAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("synchronize")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> SynchronizeArticles()
        {
            try
            {
                var success = await _articleSyncService.SynchronizeArticlesAsync();
                if (!success) return StatusCode(500, "Failed to synchronize articles.");
                return Ok("Articles synchronized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during article synchronization");
                return StatusCode(500, "An error occurred during synchronization.");
            }
        }
    }
}