namespace ECommerceApp.Core
{
    public enum OrderStatus
    {
        Pending,
        Placed,
        Paid,
        Cancelled
    }

    /// <summary>
    /// Order &amp; payment service with intentional bugs for testing purposes.
    ///
    /// BUG LIST:
    ///   B4 – PlaceOrder: accepts empty cart without throwing
    ///   B5 – ProcessPayment: accepts negative amount without throwing
    ///   B6 – PlaceOrder: stock check uses off-by-one ( &gt; stock + 1 ),
    ///                    so quantity == stock + 1 and stock == 0 (qty 1) slip through
    ///   B7 – PlaceOrder: minimum order check uses (MinimumOrderAmount / 2)
    ///                    threshold, so orders well below the real minimum pass
    /// </summary>
    public class OrderService
    {
        private OrderStatus _status = OrderStatus.Pending;
        private decimal _paidAmount = 0m;
        private Cart? _cart;

        public OrderStatus Status => _status;
        public decimal PaidAmount => _paidAmount;
        public decimal MinimumOrderAmount { get; }

        public OrderService(decimal minimumOrderAmount = 100m)
        {
            if (minimumOrderAmount < 0)
                throw new ArgumentException("Minimum order amount cannot be negative.", nameof(minimumOrderAmount));
            MinimumOrderAmount = minimumOrderAmount;
        }

        // -------------------------------------------------------
        // BUG B4: Should throw InvalidOperationException for empty cart.
        //         Instead it silently sets status to Placed.
        // BUG B6: Stock check uses off-by-one (qty > stock + 1).
        //         qty=1 / stock=0 should be rejected but passes.
        // BUG B7: Minimum order check uses (MinimumOrderAmount / 2) threshold.
        //         Orders between half-min and min pass when they shouldn't.
        // -------------------------------------------------------
        public string PlaceOrder(Cart cart)
        {
            if (cart == null) throw new ArgumentNullException(nameof(cart));

            // BUG B4 — missing empty-cart guard:
            //   Correct: if (cart.IsEmpty) throw new InvalidOperationException("Cannot place order with empty cart.");

            // ---- Stock check (BUG B6) -----------------------------------
            foreach (var item in cart.Items)
            {
                // BUG B6 — off-by-one comparison:
                //   Correct: if (item.Quantity > item.Product.Stock)
                if (item.Quantity > item.Product.Stock + 1)
                {
                    throw new OutOfStockException(
                        item.Product.Id, item.Quantity, item.Product.Stock);
                }
            }

            // ---- Minimum order amount check (BUG B7) --------------------
            // Skip the check when cart is empty so that BUG B4 is still observable.
            if (!cart.IsEmpty)
            {
                var total = cart.GetTotal();

                // BUG B7 — threshold halved:
                //   Correct: if (total < MinimumOrderAmount)
                if (total < MinimumOrderAmount / 2)
                {
                    throw new MinimumOrderNotMetException(total, MinimumOrderAmount);
                }
            }

            _cart = cart;
            _status = OrderStatus.Placed;
            return $"Order placed successfully. Total: {cart.GetTotal():C2}";
        }

        // -------------------------------------------------------
        // BUG B5: Should throw ArgumentException for negative amount.
        //         Instead it happily "processes" the payment.
        // -------------------------------------------------------
        public string ProcessPayment(decimal amount)
        {
            if (_status != OrderStatus.Placed)
                throw new InvalidOperationException("Order must be placed before payment.");

            // BUG B5 — missing negative-amount guard:
            //   Correct: if (amount <= 0) throw new ArgumentException("Payment amount must be positive.", nameof(amount));

            _paidAmount = amount;
            _status = OrderStatus.Paid;
            return $"Payment of {amount:C2} processed successfully.";
        }

        public string CancelOrder()
        {
            if (_status == OrderStatus.Paid)
                throw new InvalidOperationException("Cannot cancel a paid order.");

            _status = OrderStatus.Cancelled;
            return "Order cancelled.";
        }

        public override string ToString() =>
            $"Order Status: {_status} | Paid: {_paidAmount:C2}";
    }
}
