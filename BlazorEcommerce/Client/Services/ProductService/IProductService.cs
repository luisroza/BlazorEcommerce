namespace BlazorEcommerce.Client.Services.ProductService
{
    public interface IProductService
    {
        event Action ProductsChanged;
        string Message { get; set; }
        int CurrentPage { get; set; }
        int PageCount { get; set; }
        string LastSearchText { get; set; }
        List<Product> Products { get; set; }
        List<Product> AdminProducts { get; set; }
        Task GetProductsAsync(string? categoryUrl = null);
        Task<ServiceResponse<Product>> GetProductAsync(int id);
        Task SearchProducts(string searchText, int page);
        Task<List<string>> GetProductSearchSuggestions(string searchText);
        Task GetAdminProducts();
        Task<Product?> CreateProduct(Product product);
        Task<Product?> UpdateProduct(Product product);
        Task DeleteProduct(Product product);
    }
}
