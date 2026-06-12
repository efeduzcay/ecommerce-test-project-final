namespace ECommerceApp.Core
{
    /// <summary>
    /// Thrown when a cart item's quantity exceeds available product stock.
    /// </summary>
    public class OutOfStockException : InvalidOperationException
    {
        public int ProductId { get; }
        public int Requested { get; }
        public int Available { get; }

        public OutOfStockException(int productId, int requested, int available)
            : base($"Product {productId} is out of stock. Requested: {requested}, Available: {available}.")
        {
            ProductId = productId;
            Requested = requested;
            Available = available;
        }
    }

    /// <summary>
    /// Thrown when the cart total is below the configured minimum order amount.
    /// </summary>
    public class MinimumOrderNotMetException : InvalidOperationException
    {
        public decimal CartTotal { get; }
        public decimal RequiredMinimum { get; }

        public MinimumOrderNotMetException(decimal cartTotal, decimal requiredMinimum)
            : base($"Cart total {cartTotal:C2} is below required minimum {requiredMinimum:C2}.")
        {
            CartTotal = cartTotal;
            RequiredMinimum = requiredMinimum;
        }
    }
}
