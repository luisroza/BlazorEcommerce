namespace BlazorEcommerce.Client.Services.CategoryService
{
    public interface ICategoryService
    {
        List<Category> Categories { get; set; }
        Task GetCategoriesAsync();
        Task<ServiceResponse<Product>> GetCategoryAsync(int id);
    }
}
