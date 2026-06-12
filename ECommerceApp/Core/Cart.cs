namespace ECommerceApp.Core
{
    /// <summary>
    /// Represents a line item inside the shopping cart.
    /// </summary>
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }

        public CartItem(Product product, int quantity = 1)
        {
            Product = product;
            Quantity = quantity;
        }

        public decimal LineTotal => Product.Price * Quantity;
    }

    /// <summary>
    /// Shopping cart with intentional bugs for testing purposes.
    /// 
    /// BUG LIST:
    ///   B1 – AddItem: duplicate items added instead of incrementing quantity
    ///   B2 – GetTotal: integer division causes discount to always be 0
    ///   B3 – RemoveItem: silently fails when item is not in cart
    /// </summary>
    public class Cart
    {
        private readonly List<CartItem> _items = new();

        // Exposed for Gray Box / Reflection testing
        internal decimal _discountedTotal = 0m;

        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

        public int ItemCount => _items.Count;

        // -------------------------------------------------------
        // BUG B1: Should find existing item and increment quantity.
        //         Instead, always adds a brand-new CartItem.
        // -------------------------------------------------------
        public void AddItem(Product product, int quantity = 1)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));

            // BUG B1 — missing "find existing" check:
            //   Correct:
            //     var existing = _items.FirstOrDefault(i => i.Product.Id == product.Id);
            //     if (existing != null) { existing.Quantity += quantity; return; }
            _items.Add(new CartItem(product, quantity));   // always duplicates
        }

        // -------------------------------------------------------
        // BUG B2: discountPercent should use decimal arithmetic.
        //         int / int → 0 when percent < 100.
        // -------------------------------------------------------
        public decimal GetTotal(int discountPercent = 0)
        {
            decimal subtotal = _items.Sum(i => i.LineTotal);

            // BUG B2 — integer division:  10 / 100 = 0  (should be 10m / 100m = 0.10)
            decimal discountFactor = 1 - discountPercent / 100;   // int division bug
            _discountedTotal = subtotal * discountFactor;
            return _discountedTotal;
        }

        // -------------------------------------------------------
        // BUG B3: Should throw ArgumentException when item not found.
        //         Instead silently does nothing.
        // -------------------------------------------------------
        public void RemoveItem(int productId)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);

            // BUG B3 — no exception when item is missing:
            //   Correct: if (item == null) throw new ArgumentException($"Product {productId} not in cart.");
            if (item != null)
                _items.Remove(item);
            // silently returns when item is null
        }

        public void Clear() => _items.Clear();

        public bool IsEmpty => _items.Count == 0;

        public override string ToString()
        {
            if (IsEmpty) return "Cart is empty.";
            var lines = _items.Select(i => $"  {i.Product.Name} x{i.Quantity} = {i.LineTotal:C2}");
            return string.Join(Environment.NewLine, lines);
        }
    }
}
