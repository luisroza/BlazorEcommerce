﻿using BlazorEcommerce.Server.Services.ProductService;
using Microsoft.AspNetCore.Mvc;

namespace BlazorEcommerce.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<Product>>>> GetProducts()
        {
            var result = await _productService.GetProductsAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceResponse<Product>>> GetProduct(int id)
        {
            var result = await _productService.GetProductAsync(id);
            if (result == null)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("categories/{categoryUrl}")]
        public async Task<ActionResult<ServiceResponse<Product>>> GetProductsByCategories(string categoryUrl)
        {
            var result = await _productService.GetProductsByCategoryAsync(categoryUrl);
            if (result == null)
                return NotFound(result);
            return Ok(result);
        }
    }
}
