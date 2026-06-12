using System.Reflection;
using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests.GrayBoxTests
{
    /// <summary>
    /// GRAY BOX Tests — tester has partial knowledge of internals.
    /// Uses Reflection to inspect private/internal fields while still
    /// testing through the public API.
    /// </summary>
    [TestFixture]
    public class OrderGrayBoxTests
    {
        private Cart _cart = null!;
        private OrderService _orderService = null!;
        private Product _product = null!;

        [SetUp]
        public void SetUp()
        {
            _cart         = new Cart();
            _orderService = new OrderService();
            _product      = new Product(1, "Tablet", 3000m, 5);
        }

        // ------------------------------------------------------------------
        // Helper: read private/internal field via Reflection
        // ------------------------------------------------------------------
        private static T? GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType()
                           .GetField(fieldName,
                               BindingFlags.NonPublic |
                               BindingFlags.Instance);
            if (field == null)
                throw new InvalidOperationException(
                    $"Field '{fieldName}' not found on {obj.GetType().Name}");

            return (T?)field.GetValue(obj);
        }

        // ------------------------------------------------------------------
        // TC-09 | Gray Box — internal _status field should be "Placed" after PlaceOrder
        // Expected: PASS (the public flow works; internal state is inspected via Reflection)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-09 [Gray Box] Internal _status field should equal OrderStatus.Placed after PlaceOrder")]
        public void PlaceOrder_InternalStatusField_ShouldBePlaced()
        {
            _cart.AddItem(_product, 1);
            _orderService.PlaceOrder(_cart);

            // Use Reflection to read the private backing field
            var status = GetPrivateField<OrderStatus>(_orderService, "_status");

            Assert.That(status, Is.EqualTo(OrderStatus.Placed),
                "Internal _status should be 'Placed' after calling PlaceOrder.");
        }

        // ------------------------------------------------------------------
        // TC-10 | Gray Box — internal _discountedTotal should reflect 10% discount
        // Expected: FAIL (BUG B2 causes int-division, _discountedTotal = 3000 not 2700)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-10 [Gray Box] Internal _discountedTotal should be 2700₺ after 10% discount on 3000₺")]
        public void GetTotal_InternalDiscountedTotal_ShouldReflect10PercentDiscount()
        {
            _cart.AddItem(_product, 1);                // 3000₺ in cart
            _cart.GetTotal(discountPercent: 10);       // triggers internal calculation

            // Read the internal _discountedTotal field set by GetTotal()
            var discountedTotal = GetPrivateField<decimal>(_cart, "_discountedTotal");

            // BUG B2: int division  10 / 100 = 0  →  discountFactor = 1
            //          → _discountedTotal = 3000, not 2700
            Assert.That(discountedTotal, Is.EqualTo(2700m),
                "10% discount on 3000₺ should set _discountedTotal to 2700₺.");
        }

        // ------------------------------------------------------------------
        // TC-22 | Gray Box — internal _cart reference should match cart passed to PlaceOrder
        // Expected: PASS (verifies the service stores the same Cart instance internally)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-22 [Gray Box] Internal _cart field should reference the cart passed to PlaceOrder")]
        public void PlaceOrder_InternalCartReference_ShouldMatchPassedCart()
        {
            _cart.AddItem(_product, 2);   // total = 6000₺, well above min 100₺
            _orderService.PlaceOrder(_cart);

            var storedCart = GetPrivateField<Cart>(_orderService, "_cart");

            Assert.That(storedCart, Is.SameAs(_cart),
                "Internal _cart should hold the same reference passed to PlaceOrder.");
        }
    }
}
