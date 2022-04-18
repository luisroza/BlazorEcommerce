namespace BlazorEcommerce.Client.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _http;

        public ProductService(HttpClient http)
        {
            _http = http;
        }

        public event Action ProductsChanged;
        public string Message { get; set; } = "Loading Products...";
        public int CurrentPage { get; set; } = 1;
        public int PageCount { get; set; } = 0;
        public string LastSearchText { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Product> AdminProducts { get; set; } = new List<Product>();

        public async Task GetProductsAsync(string? categoryUrl = null)
        {
            var result = categoryUrl == null
                ? await _http.GetFromJsonAsync<ServiceResponse<List<Product>>>("api/product/featured")
                : await _http.GetFromJsonAsync<ServiceResponse<List<Product>>>($"api/product/categories/{categoryUrl}");
            if (result != null && result.Data != null)
                Products = result.Data;

            CurrentPage = 1;
            PageCount = 0;

            if (Products.Count == 0)
                Message = "No products found";

            ProductsChanged.Invoke();
        }

        public async Task<ServiceResponse<Product>> GetProductAsync(int id)
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<Product>>($"api/product/{id}");
            return result;
        }

        public async Task SearchProducts(string searchText, int page)
        {
            LastSearchText = searchText;
            var result = await _http.GetFromJsonAsync<ServiceResponse<ProductSearchResult>>($"api/product/search/{searchText}/{page}");
            if (result != null && result.Data != null)
            {
                Products = result.Data.Products;
                CurrentPage = result.Data.CurrentPage;
                PageCount = result.Data.Pages;
            }
            if (Products.Count == 0) Message = "No products found";
            ProductsChanged.Invoke();
        }

        public async Task<List<string>> GetProductSearchSuggestions(string searchText)
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<List<string>>>($"api/product/search-suggestions/{searchText}");
            return result.Data;
        }

        public async Task GetAdminProducts()
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<List<Product>>>($"api/product/admin");
            AdminProducts = result.Data;
            CurrentPage = 1;
            PageCount = 0;
            if (AdminProducts.Count == 0)
                Message = "No products found.";
        }

        public async Task<Product?> CreateProduct(Product product)
        {
            var result = await _http.PostAsJsonAsync("api/product", product);
            return (await result.Content.ReadFromJsonAsync<ServiceResponse<Product>>()).Data;
        }

        public async Task<Product?> UpdateProduct(Product product)
        {
            var result = await _http.PutAsJsonAsync("api/product", product);
            return (await result.Content.ReadFromJsonAsync<ServiceResponse<Product>>()).Data;
        }

        public async Task DeleteProduct(Product product)
        {
            await _http.DeleteAsync($"api/product/{product.Id}");
        }
    }
}
