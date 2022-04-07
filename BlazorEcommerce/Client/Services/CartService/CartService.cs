using Blazored.LocalStorage;

namespace BlazorEcommerce.Client.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;
        public event Action? OnChanged;

        public CartService(ILocalStorageService localStorage, HttpClient http)
        {
            _localStorage = localStorage;
            _http = http;
        }

        public async Task AddToCart(CartItem cartItem)
        {
            var cart = await _localStorage.GetItemAsync<List<CartItem>>("cart") ?? new List<CartItem>();

            var matchItem =
                cart.Find(i => i.ProductId == cartItem.ProductId && i.ProductTypeId == cartItem.ProductTypeId);

            if (matchItem == null)
                cart.Add(cartItem);
            else
                matchItem.Quantity += cartItem.Quantity;

            await _localStorage.SetItemAsync("cart", cart);
            OnChanged?.Invoke();
        }

        public async Task<List<CartItem>> GetCartItems()
        {
            return await _localStorage.GetItemAsync<List<CartItem>>("cart") ?? new List<CartItem>();
        }

        public async Task<List<CartProductResponse>> GetCartProducts()
        {
            var cartItems = await _localStorage.GetItemAsync<List<CartItem>>("cart");
            var response = await _http.PostAsJsonAsync("api/carts/products", cartItems);
            var cartProducts = await response.Content.ReadFromJsonAsync<ServiceResponse<List<CartProductResponse>>>();
            return cartProducts.Data;
        }

        public async Task RemoveProductFromCart(int productId, int productTypeId)
        {
            var cart = await _localStorage.GetItemAsync<List<CartItem>>("cart");
            if (cart == null) return;

            var cartItem = cart.Find(p => p.ProductId == productId && p.ProductTypeId == productTypeId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                await _localStorage.SetItemAsync("cart", cart);
                OnChanged.Invoke();
            }
        }

        public async Task UpdateQuantity(CartProductResponse product)
        {
            var cart = await _localStorage.GetItemAsync<List<CartItem>>("cart");
            if (cart == null) return;

            var cartItem = cart.Find(p => p.ProductId == product.ProductId && p.ProductTypeId == product.ProductTypeId);
            if (cartItem != null)
            {
                cartItem.Quantity = product.Quantity;
                await _localStorage.SetItemAsync("cart", cart);
            }
        }
    }
}