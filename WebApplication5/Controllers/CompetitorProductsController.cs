using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication5.Dto;
using WebApplication5.Dtos;
using WebApplication5.Models;
using WebApplication5.Repositories;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitorProductsController : ControllerBase
    {
        private readonly ICompetitorProductRepository _competitorProductRepository;

        public CompetitorProductsController(ICompetitorProductRepository competitorProductRepository)
        {
            _competitorProductRepository = competitorProductRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompetitorProduct([FromBody] CompetitorProductDto competitorProductCreateDto)
        {
            var competitorProduct = new CompetitorProduct
            {
                VisitId = competitorProductCreateDto.VisitId,
                ProductName = competitorProductCreateDto.ProductName,
                Price = competitorProductCreateDto.Price,
                ImageUrl = competitorProductCreateDto.ImageUrl,
                Notes = competitorProductCreateDto.Notes
            };

            var createdCompetitorProduct = await _competitorProductRepository.CreateAsync(competitorProduct);
            var competitorProductDto = new CompetitorProductDto
            {
                Id = createdCompetitorProduct.Id,
                VisitId = createdCompetitorProduct.VisitId,
                ProductName = createdCompetitorProduct.ProductName,
                Price = createdCompetitorProduct.Price,
                ImageUrl = createdCompetitorProduct.ImageUrl,
                Notes = createdCompetitorProduct.Notes
            };

            return CreatedAtAction(nameof(CreateCompetitorProduct), new { id = createdCompetitorProduct.Id }, competitorProductDto);
        }
    }
}