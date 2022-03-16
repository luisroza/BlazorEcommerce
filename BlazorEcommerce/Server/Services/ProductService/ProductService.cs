namespace BlazorEcommerce.Server.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly DataContext _context;

        public List<Product>? Products { get; set; }

        public ProductService(DataContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<List<Product>>> GetProductsAsync()
        {
            return new ServiceResponse<List<Product>>
            {
                Data = await _context.Products.Include(p => p.Variants).ToListAsync()
            };
        }

        public async Task<ServiceResponse<List<Product>>> GetProductsByCategoryAsync(string categoryUrl)
        {
            return new ServiceResponse<List<Product>>()
            {
                Data = await _context.Products.Where(p => p.Category!.Url.ToLower().Equals(categoryUrl.ToLower()))
                    .Include(p => p.Variants).ToListAsync()
            };
        }

        public async Task<ServiceResponse<Product>> GetProductAsync(int id)
        {
            var response = new ServiceResponse<Product>();
            var product = await _context.Products.Include(p => p.Variants)
                .ThenInclude(v => v.ProductType).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                response.Success = false;
                response.Message = "Sorry, product was not found";
            }
            else
            {
                response.Data = product;
            }

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
                Data = await _context.Products.Where(p => p.Featured)
                    .Include(p => p.Variants).ToListAsync()
            };
        }

        public async Task<ServiceResponse<List<Product>>> SearchProducts(string searchText)
        {
            return new ServiceResponse<List<Product>>
            {
                Data = await FindProductBySearchText(searchText)
            };
        }

        private async Task<List<Product>> FindProductBySearchText(string searchText)
        {
            return await _context.Products
                .Where(p => p.Title.ToLower().Contains(searchText.ToLower())
                            || p.Description.ToLower().Contains(searchText.ToLower()))
                .Include(p => p.Variants).ToListAsync();
        }
    }
}
