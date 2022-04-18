namespace BlazorEcommerce.Server.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly DataContext _context;
        private readonly HttpContextAccessor _httpContextAccessor;

        public List<Product>? Products { get; set; }

        public ProductService(DataContext context, HttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ServiceResponse<List<Product>>> GetProductsAsync()
        {
            return new ServiceResponse<List<Product>>
            {
                Data = await _context.Products!
                    .Where(p => !p.Deleted && p.Visible)
                    .Include(p => p.Variants.Where(v => !v.Deleted && v.Visible))
                    .ToListAsync()
            };
        }

        public async Task<ServiceResponse<List<Product>>> GetProductsByCategoryAsync(string categoryUrl)
        {
            return new ServiceResponse<List<Product>>()
            {
                Data = await _context.Products!
                    .Where(p => p.Category!.Url.ToLower().Equals(categoryUrl.ToLower()) && !p.Deleted && p.Visible)
                    .Include(p => p.Variants.Where(v => !v.Deleted && v.Visible))
                    .ToListAsync()
            };
        }

        public async Task<ServiceResponse<Product>> GetProductAsync(int productId)
        {
            var response = new ServiceResponse<Product>();
            Product? product;
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User.IsInRole("Admin"))
            {
                product = await _context.Products!
                    .Include(p => p.Variants.Where(v => !v.Deleted))
                    .ThenInclude(v => v.ProductType)
                    .FirstOrDefaultAsync(p => p.Id == productId && !p.Deleted);
            }
            else
            {
                product = await _context.Products!
                    .Include(p => p.Variants.Where(v => v.Visible && !v.Deleted))
                    .ThenInclude(v => v.ProductType)
                    .FirstOrDefaultAsync(p => p.Id == productId && !p.Deleted && p.Visible);
            }
            
            if (product == null)
            {
                response.Success = false;
                response.Message = "Sorry, product was not found";
            }
            else
                response.Data = product;

            return response;
        }

        public async Task<ServiceResponse<List<string>>> GetProductSearchSuggestions(string searchText)
        {
            var products = await FindProductBySearchText(searchText);
            var result = new List<string>();

            foreach (var product in products)
            {
                if (product.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(product.Title);
                }

                if (string.IsNullOrEmpty(product.Description)) continue;

                var punctuation = product.Description.Where(char.IsPunctuation)
                    .Distinct().ToArray();
                var words = product.Description.Split()
                    .Select(s => s.Trim(punctuation));

                foreach (var word in words)
                {
                    if (word.Contains(searchText, StringComparison.OrdinalIgnoreCase) && !result.Contains(word))
                        result.Add(word);
                }
            }

            return new ServiceResponse<List<string>> { Data = result };
        }

        public async Task<ServiceResponse<List<Product>>> GetFeaturedProducts()
        {
            return new ServiceResponse<List<Product>>()
            {
                Data = await _context.Products!
                    .Where(p => p.Featured && !p.Deleted && p.Visible)
                    .Include(p => p.Variants.Where(v => !v.Deleted && v.Visible))
                    .ToListAsync()
            };
        }

        public async Task<ServiceResponse<List<Product>>> GetAdminProducts()
        {
            return new ServiceResponse<List<Product>>()
            {
                Data = await _context.Products!
                    .Where(p => !p.Deleted)
                    .Include(p => p.Variants.Where(v => !v.Deleted))
                    .ThenInclude(v => v.ProductType)
                    .ToListAsync()
            };
        }

        public async Task<ServiceResponse<Product>> CreateProduct(Product product)
        {
            foreach (var variant in product.Variants)
            {
                //EF to not create the types
                variant.ProductType = null;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return new ServiceResponse<Product> { Data = product };
        }

        public async Task<ServiceResponse<Product>> UpdateProduct(Product product)
        {
            var dbProduct = await _context.Products.FindAsync(product.Id);
            if (dbProduct == null)
            {
                return new ServiceResponse<Product>
                {
                    Success = false,
                    Message = "Product not found"
                };
            }

            dbProduct.Title = product.Title;
            dbProduct.Description = product.Description;
            dbProduct.ImageUrl = product.ImageUrl;
            dbProduct.CategoryId = product.CategoryId;
            dbProduct.Visible = product.Visible;
            dbProduct.Featured = product.Featured;

            foreach (var variant in product.Variants)
            {
                var dbVariant = await _context.ProductVariants.SingleOrDefaultAsync(v =>
                    v.ProductId == variant.ProductId &&
                    v.ProductTypeId == variant.ProductTypeId);

                if (dbVariant == null)
                {
                    variant.ProductType = null;
                    _context.ProductVariants.Add(variant);
                }
                else
                {
                    dbVariant.ProductTypeId = variant.ProductTypeId;
                    dbVariant.Price = variant.Price;
                    dbVariant.OriginalPrice = variant.OriginalPrice;
                    dbVariant.Deleted = variant.Deleted;
                    dbVariant.Visible = variant.Visible;
                }
            }

            await _context.SaveChangesAsync();
            return new ServiceResponse<Product> { Data = product };
        }

        public async Task<ServiceResponse<bool>> DeleteProduct(int productId)
        {
            var dbProduct = await _context.Products.FindAsync(productId);
            if (dbProduct == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Product not found"
                };
            }

            dbProduct.Deleted = true;
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true };
        }

        public async Task<ServiceResponse<ProductSearchResult>> SearchProducts(string searchText, int page)
        {
            var pageResults = 2f;
            var pageCount = Math.Ceiling((await FindProductBySearchText(searchText)).Count / pageResults);
            var products = await _context.Products!
                .Where(p => p.Title.ToLower().Contains(searchText.ToLower())
                            || p.Description.ToLower().Contains(searchText.ToLower()) && !p.Deleted && p.Visible)
                .Include(p => p.Variants)
                .Skip((page - 1) * (int) pageResults).Take((int) pageResults).ToListAsync();

            return new ServiceResponse<ProductSearchResult>
            {
                Data = new ProductSearchResult()
                {
                    Products = products,
                    CurrentPage = page,
                    Pages = (int)pageCount
                }
            };
        }

        private async Task<List<Product>> FindProductBySearchText(string searchText)
        {
            return await _context.Products!
                .Where(p => p.Title.ToLower().Contains(searchText.ToLower())
                            || p.Description.ToLower().Contains(searchText.ToLower()) && !p.Deleted && p.Visible)
                .Include(p => p.Variants).ToListAsync();
        }
    }
}
