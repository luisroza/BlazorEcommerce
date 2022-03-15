﻿namespace BlazorEcommerce.Client.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _http;
        public List<Category> Categories { get; set; } = new List<Category>();

        public CategoryService(HttpClient http)
        {
            _http = http;
        }

        public async Task GetCategoriesAsync()
        {
            var response = await _http.GetFromJsonAsync<ServiceResponse<List<Category>>>("api/categories");
            if (response != null && response.Data != null)
                    Categories = response.Data;
        }

        Task<ServiceResponse<Product>> ICategoryService.GetCategoryAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}