using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests.IntegrationTests
{
    /// <summary>
    /// INTEGRATION Tests — tests the full end-to-end flow:
    ///   Product → Cart → OrderService → Payment
    /// Multiple components interact; tests verify the combined behaviour.
    /// </summary>
    [TestFixture]
    public class OrderIntegrationTests
    {
        private Cart _cart = null!;
        private OrderService _orderService = null!;

        [SetUp]
        public void SetUp()
        {
            _cart         = new Cart();
            _orderService = new OrderService();
        }

        // ------------------------------------------------------------------
        // TC-11 | Happy path — full flow: Product → Cart → Order → Payment
        // Expected: PASS
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-11 [Integration] Full flow: add product → place order → process payment should succeed")]
        public void FullFlow_AddProductPlaceOrderProcessPayment_ShouldSucceed()
        {
            var phone = new Product(10, "Smartphone", 8000m, 15);

            // Step 1: Add to cart
            _cart.AddItem(phone, 1);
            Assert.That(_cart.IsEmpty, Is.False, "Cart should not be empty after adding product.");

            // Step 2: Place order
            var orderMsg = _orderService.PlaceOrder(_cart);
            Assert.That(_orderService.Status, Is.EqualTo(OrderStatus.Placed));

            // Step 3: Pay
            var payMsg = _orderService.ProcessPayment(_cart.GetTotal());
            Assert.That(_orderService.Status, Is.EqualTo(OrderStatus.Paid));
            Assert.That(_orderService.PaidAmount, Is.EqualTo(8000m));
        }

        // ------------------------------------------------------------------
        // TC-12 | BUG B4 — PlaceOrder with empty cart should throw
        // Expected: FAIL (BUG B4 silently places order with empty cart)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-12 [Integration] Placing an order with an empty cart should throw InvalidOperationException")]
        public void PlaceOrder_EmptyCart_ShouldThrowInvalidOperationException()
        {
            // Cart is empty (no items added)
            // BUG B4: no guard for empty cart → status becomes Placed without error
            Assert.Throws<InvalidOperationException>(
                () => _orderService.PlaceOrder(_cart),
                "Expected InvalidOperationException for empty-cart order.");
        }

        // ------------------------------------------------------------------
        // TC-13 | BUG B5 — ProcessPayment with negative amount should throw
        // Expected: FAIL (BUG B5 accepts negative amount)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-13 [Integration] Processing payment with a negative amount should throw ArgumentException")]
        public void ProcessPayment_NegativeAmount_ShouldThrowArgumentException()
        {
            var tv = new Product(11, "TV", 5000m, 3);
            _cart.AddItem(tv, 1);
            _orderService.PlaceOrder(_cart);

            // BUG B5: negative amount is accepted, PaidAmount becomes negative
            Assert.Throws<ArgumentException>(
                () => _orderService.ProcessPayment(-100m),
                "Expected ArgumentException for negative payment amount.");
        }

        // ------------------------------------------------------------------
        // TC-23 | Integration — BUG B6: end-to-end flow with stock=0 product
        // Expected: FAIL (off-by-one stock check accepts qty=1 when stock=0)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-23 [Integration] Full flow with stock=0 product should be rejected before payment")]
        public void FullFlow_ProductOutOfStock_ShouldThrowAndPreventPayment()
        {
            var soldOut = new Product(20, "Sold Out Phone", 8000m, stock: 0);
            _cart.AddItem(soldOut, 1);

            // BUG B6: order is placed (no exception); status becomes Placed.
            Assert.Throws<OutOfStockException>(
                () => _orderService.PlaceOrder(_cart),
                "Expected OutOfStockException so that payment never happens.");
            Assert.That(_orderService.Status, Is.Not.EqualTo(OrderStatus.Placed),
                "Order should not be marked Placed when stock is insufficient.");
        }

        // ------------------------------------------------------------------
        // TC-24 | Integration — BUG B7: under-minimum order rejected end-to-end
        // Expected: FAIL (threshold halved → 60₺ passes the check)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-24 [Integration] Order total 60₺ under min 100₺ should be rejected")]
        public void FullFlow_TotalUnderMinimum_ShouldThrowMinimumOrderNotMet()
        {
            var customOrderService = new OrderService(minimumOrderAmount: 100m);
            var item = new Product(21, "Cheap Item", 60m, stock: 10);
            _cart.AddItem(item, 1);

            // BUG B7: comparison uses /2 threshold (50₺), so 60₺ passes.
            Assert.Throws<MinimumOrderNotMetException>(
                () => customOrderService.PlaceOrder(_cart),
                "Expected MinimumOrderNotMetException because 60₺ < 100₺.");
        }

        // ------------------------------------------------------------------
        // TC-25 | Integration — discount code applied through full order flow
        // Expected: FAIL (B2 int-division kills the percent; final total stays the same)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-25 [Integration] Order with valid SAVE10 code should pay 90% of subtotal")]
        public void FullFlow_WithDiscountCode_ShouldChargeDiscountedTotal()
        {
            var discountSvc = new DiscountService();
            var product = new Product(22, "Headset", 1000m, stock: 5);
            _cart.AddItem(product, 1);

            int percent = discountSvc.GetDiscountPercent("SAVE10");      // = 10
            decimal discounted = _cart.GetTotal(percent);                // BUG B2: = 1000, should be 900

            _orderService.PlaceOrder(_cart);
            _orderService.ProcessPayment(discounted);

            // BUG B2 causes the discount to vanish; paid stays at 1000₺.
            Assert.That(_orderService.PaidAmount, Is.EqualTo(900m),
                "10% discount on 1000₺ should leave 900₺ paid, not the full 1000₺.");
        }
    }
}
