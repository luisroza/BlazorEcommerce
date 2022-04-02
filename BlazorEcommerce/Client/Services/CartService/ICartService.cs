﻿namespace BlazorEcommerce.Client.Services.CartService
{
    public interface ICartService
    {
        event Action OnChanged;
        Task AddToCart(CartItem cartItem);
        Task<List<CartItem>> GetCartItems();
        Task<List<CartProductResponse>> GetCartProducts();
        Task RemoveProductFromCart(int productId, int productTypeId);
    }
}
