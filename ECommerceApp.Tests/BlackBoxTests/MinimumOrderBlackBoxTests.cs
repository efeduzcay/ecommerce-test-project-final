using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests.BlackBoxTests
{
    /// <summary>
    /// BLACK BOX Tests for the minimum order amount rule in OrderService.PlaceOrder.
    /// Minimum = 100₺. Uses Equivalence Partitioning + Boundary Value Analysis.
    ///
    /// Partitions:
    ///   P1: total < 100  → should be rejected
    ///   P2: total ≥ 100  → should be accepted
    /// </summary>
    [TestFixture]
    public class MinimumOrderBlackBoxTests
    {
        private Cart _cart = null!;
        private OrderService _orderService = null!;

        [SetUp]
        public void SetUp()
        {
            _cart         = new Cart();
            _orderService = new OrderService(minimumOrderAmount: 100m);
        }

        // ------------------------------------------------------------------
        // TC-20 | EP — total = 60₺ (well below min) should be rejected
        // Expected: FAIL (BUG B7: threshold is MinimumOrderAmount / 2 = 50,
        //                so 60 < 50 is false → order accepted incorrectly)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-20 [Black Box / EP] Order total below minimum should be rejected")]
        public void PlaceOrder_TotalBelowMinimum_ShouldThrow()
        {
            var snack = new Product(60, "Snack", 60m, stock: 10);
            _cart.AddItem(snack, 1);   // total = 60₺, min = 100₺

            // BUG B7: comparison uses /2 threshold (50), so 60 passes.
            Assert.Throws<MinimumOrderNotMetException>(
                () => _orderService.PlaceOrder(_cart),
                "Expected MinimumOrderNotMetException because total 60₺ < min 100₺.");
        }

        // ------------------------------------------------------------------
        // TC-21 | BVA — total exactly at minimum (100₺) should be accepted
        // Expected: PASS
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-21 [Black Box / BVA] Total equal to minimum should be accepted")]
        public void PlaceOrder_TotalEqualsMinimum_ShouldSucceed()
        {
            var product = new Product(61, "Boundary Item", 100m, stock: 5);
            _cart.AddItem(product, 1);   // total = 100₺ == min

            Assert.DoesNotThrow(() => _orderService.PlaceOrder(_cart));
            Assert.That(_orderService.Status, Is.EqualTo(OrderStatus.Placed));
        }
    }
}
