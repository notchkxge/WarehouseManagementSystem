using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.Core.Data.Repositories;
using WarehouseAPI.Core.Models.Entities;

namespace WarehouseAPI.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductRepository _productRepository;

        public ProductsController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productRepository.GetAllAsync();
            return Ok(products);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            
            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // GET: api/Products/search?name=query
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string name)
        {
            var products = await _productRepository.SearchByNameAsync(name);
            return Ok(products);
        }

        // GET: api/Products/article/{articleNumber}
        [HttpGet("article/{articleNumber}")]
        public async Task<ActionResult<Product>> GetProductByArticleNumber(string articleNumber)
        {
            var product = await _productRepository.GetByArticleNumberAsync(articleNumber);
            
            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // GET: api/Products/warehouse/{warehouseId}
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsInWarehouse(int warehouseId)
        {
            var products = await _productRepository.GetProductsInWarehouseAsync(warehouseId);
            return Ok(products);
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            // Check if article number already exists
            var existingProduct = await _productRepository.GetByArticleNumberAsync(product.ArticleNumber);
            if (existingProduct != null)
            {
                return BadRequest("Product with this article number already exists.");
            }

            var createdProduct = await _productRepository.CreateAsync(product);
            
            return CreatedAtAction("GetProduct", new { id = createdProduct.Id }, createdProduct);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            // Check if product exists
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Check if article number is being changed to one that already exists
            if (product.ArticleNumber != existingProduct.ArticleNumber)
            {
                var productWithSameArticle = await _productRepository.GetByArticleNumberAsync(product.ArticleNumber);
                if (productWithSameArticle != null)
                {
                    return BadRequest("Product with this article number already exists.");
                }
            }

            await _productRepository.UpdateAsync(product);
            return NoContent();
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productRepository.DeleteAsync(id);
            
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}