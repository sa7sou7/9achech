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
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleRepository _articleRepository;

        public ArticlesController(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetArticles()
        {
            var articles = await _articleRepository.GetAllAsync();
            return Ok(articles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticle(int id)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
                return NotFound();

            return Ok(article);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, [FromBody] ArticleDto articleDto)
        {
            if (id != articleDto.Id)
            {
                return BadRequest("Article ID mismatch.");
            }

            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            article.Code = articleDto.Code;
            article.Designation = articleDto.Designation;
            article.Famille = articleDto.Famille;
            article.PrixAchat = articleDto.PrixAchat;
            article.PrixVente = articleDto.PrixVente;
            article.StockQuantity = articleDto.StockQuantity;

            var updatedArticle = await _articleRepository.UpdateAsync(article);
            return Ok(updatedArticle);
        }
    }
}