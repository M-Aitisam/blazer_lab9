using Blazored.LocalStorage;
using System.Text.Json;

namespace blazer_lab9.Data
{
    public class CartService
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly object _lock = new();
        private bool _isInitialized;

        public List<CartItem> CartItems { get; private set; } = new();
        public event Action? OnCartChanged;

        public CartService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public async Task InitializeCartAsync()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            await LoadCartFromLocalStorage();
        }

        public void AddToCart(Product product)
        {
            lock (_lock)
            {
                var existingItem = CartItems.FirstOrDefault(item => item.Product.Id == product.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    CartItems.Add(new CartItem { Product = product, Quantity = 1 });
                }

                SaveCartToLocalStorage();
                NotifyCartChanged();
            }
        }

        public void RemoveFromCart(Product product)
        {
            lock (_lock)
            {
                var existingItem = CartItems.FirstOrDefault(item => item.Product.Id == product.Id);
                if (existingItem != null)
                {
                    CartItems.Remove(existingItem);
                    SaveCartToLocalStorage();
                    NotifyCartChanged();
                }
            }
        }

        public decimal GetTotalPrice()
        {
            return CartItems.Sum(item => item.Product.Price * item.Quantity);
        }

        public void ClearCart()
        {
            CartItems.Clear();
            SaveCartToLocalStorage();
            NotifyCartChanged();
        }

        private async Task LoadCartFromLocalStorage()
        {
            var cartData = await _localStorageService.GetItemAsStringAsync("cartData");
            if (!string.IsNullOrEmpty(cartData))
            {
                CartItems = JsonSerializer.Deserialize<List<CartItem>>(cartData) ?? new();
                NotifyCartChanged();
            }
        }

        private async void SaveCartToLocalStorage()
        {
            var cartData = JsonSerializer.Serialize(CartItems);
            await _localStorageService.SetItemAsStringAsync("cartData", cartData);
        }

        private void NotifyCartChanged() => OnCartChanged?.Invoke();
    }

    public class CartItem
    {
        public Product Product { get; set; } = default!;
        public int Quantity { get; set; }
    }
}
