using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]

        public async Task<IActionResult> GetAllProducts()
        {

            var products = await _productService.GetAllEnabled();
            return products.Any() ? Ok(products) : NoContent();
           
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductModel.ProductRequest request)
        {

            var response = await _productService.Add(request);
            return CreatedAtAction(
                nameof(GetById), 
                new { id = response.Id },
                response
            );
        }


        [HttpPut("{id:guid}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ProductModel.ProductRequest request)
        {
            var response = await _productService.Update(id, request);
            return Ok(response);
        }

        [HttpPatch("{id:guid}")]
        [Authorize(Roles = "ADMIN")]

        public async Task<IActionResult> Disable([FromRoute] Guid id)
        {
            await _productService.Disable(id);
            return NoContent();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById ([FromRoute] Guid id)
        {
            var response = await _productService.GetById(id);
            return Ok(response);            
        }
    }
}
