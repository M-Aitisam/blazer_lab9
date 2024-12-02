using System.Text.Json;

namespace blazer_lab9.Data
{
    public class CartService
    {
        private readonly string _cartFilePath;
        private readonly object _lock = new();

        public List<CartItem> CartItems { get; private set; } = new();
        public event Action? OnCartChanged;

        public CartService(string cartFilePath = "cart.json")
        {
            _cartFilePath = cartFilePath;
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

                SaveCart();
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
                    SaveCart();
                    NotifyCartChanged();
                }
            }
        }

        public decimal GetTotalPrice()
        {
            lock (_lock)
            {
                return CartItems.Sum(item => item.Product.Price * item.Quantity);
            }
        }

        public void SaveCart()
        {
            lock (_lock)
            {
                try
                {
                    var cartData = JsonSerializer.Serialize(CartItems);
                    File.WriteAllText(_cartFilePath, cartData);
                }
                catch (Exception ex)
                {
                    LogError($"Error saving cart: {ex.Message}");
                }
            }
        }

        public void LoadCart()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_cartFilePath))
                    {
                        var cartData = File.ReadAllText(_cartFilePath);
                        CartItems = JsonSerializer.Deserialize<List<CartItem>>(cartData) ?? new();
                        NotifyCartChanged();
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error loading cart: {ex.Message}");
                    CartItems = new(); // Reset to avoid issues
                }
            }
        }

        public void ClearCart()
        {
            lock (_lock)
            {
                CartItems.Clear();
                SaveCart();
                NotifyCartChanged();
            }
        }

        private void NotifyCartChanged() => OnCartChanged?.Invoke();

        private void LogError(string message)
        {
            // Replace with your preferred logging framework, e.g., ILogger.
            Console.WriteLine(message);
        }
    }

    public class CartItem
    {
        public Product Product { get; set; } = default!;
        public int Quantity { get; set; }
    }
}