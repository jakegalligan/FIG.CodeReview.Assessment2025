using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(string category = null, int page = 1, int pageSize = 10, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var allProducts = await _productService.GetAllProductsAsync();

            // Filter products
            var filteredProducts = new List<Product>();
            foreach (var product in allProducts)
            {
                bool includeProduct = true;

                if (!string.IsNullOrEmpty(category) && product.Category != category)
                    includeProduct = false;

                if (minPrice.HasValue && product.Price < minPrice.Value)
                    includeProduct = false;

                if (maxPrice.HasValue && product.Price > maxPrice.Value)
                    includeProduct = false;

                if (includeProduct)
                    filteredProducts.Add(product);
            }

            // Apply pagination
            var pagedProducts = filteredProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                Products = pagedProducts,
                TotalCount = filteredProducts.Count,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequest request)
        {
            // Manual validation instead of using data annotations
            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest("Product name is required");
            }

            if (request.Name.Length < 3)
            {
                return BadRequest("Product name must be at least 3 characters");
            }

            if (request.Price <= 0)
            {
                return BadRequest("Product price must be greater than 0");
            }

            if (string.IsNullOrEmpty(request.Category))
            {
                return BadRequest("Product category is required");
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
                InStock = request.InStock
            };

            var createdProduct = _productService.CreateProductAsync(product);

            return Ok(createdProduct);
        }

        [HttpGet("category/{categoryName}")]
        public async Task<IActionResult> GetProductsByCategory(string categoryName)
        {
            var allProducts = await _productService.GetAllProductsAsync();

            var categoryProducts = new List<Product>();
            for (int i = 0; i < allProducts.Count; i++)
            {
                if (allProducts[i].Category.ToLower() == categoryName.ToLower())
                {
                    categoryProducts.Add(allProducts[i]);
                }
            }

            return Ok(categoryProducts);
        }

        [HttpGet("search")]
        public IActionResult SearchProducts(string searchTerm)
        {
            var allProducts = _productService.GetAllProductsAsync();

            var matchingProducts = new List<Product>();
            foreach (var product in allProducts.Result)
            {
                if (product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    product.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    matchingProducts.Add(product);
                }
            }

            return Ok(matchingProducts);
        }
    }

    public class ProductCreateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public bool InStock { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public bool InStock { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> CreateProductAsync(Product product);
    }
}