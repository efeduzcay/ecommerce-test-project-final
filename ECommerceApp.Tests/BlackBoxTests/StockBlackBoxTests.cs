using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests.BlackBoxTests
{
    /// <summary>
    /// BLACK BOX Tests for stock validation in OrderService.PlaceOrder.
    /// Uses Boundary Value Analysis around the (quantity vs. stock) boundary.
    ///
    /// Equivalence partitions:
    ///   P1: stock = 0           → any order should be rejected
    ///   P2: 0 < quantity ≤ stock → order should succeed
    ///   P3: quantity > stock    → order should be rejected
    /// </summary>
    [TestFixture]
    public class StockBlackBoxTests
    {
        private Cart _cart = null!;
        private OrderService _orderService = null!;

        [SetUp]
        public void SetUp()
        {
            _cart         = new Cart();
            _orderService = new OrderService(minimumOrderAmount: 0m); // disable min-order to isolate stock
        }

        // ------------------------------------------------------------------
        // TC-17 | BVA — stock=0 with quantity=1 must throw OutOfStockException
        // Expected: FAIL (BUG B6 off-by-one accepts qty=1 when stock=0)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-17 [Black Box / BVA] Order with stock=0 product should be rejected")]
        public void PlaceOrder_StockZero_ShouldThrowOutOfStockException()
        {
            var noStockProduct = new Product(50, "Sold Out Item", 200m, stock: 0);
            _cart.AddItem(noStockProduct, 1);

            // BUG B6: off-by-one comparison (qty > stock + 1) → 1 > 1 is false → accepted
            Assert.Throws<OutOfStockException>(
                () => _orderService.PlaceOrder(_cart),
                "Expected OutOfStockException because stock is 0.");
        }

        // ------------------------------------------------------------------
        // TC-18 | BVA — quantity = stock (exact match) should succeed
        // Expected: PASS
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-18 [Black Box / BVA] Quantity equal to stock should be accepted")]
        public void PlaceOrder_QuantityEqualsStock_ShouldSucceed()
        {
            var product = new Product(51, "Limited Item", 200m, stock: 5);
            _cart.AddItem(product, 5);

            Assert.DoesNotThrow(() => _orderService.PlaceOrder(_cart));
            Assert.That(_orderService.Status, Is.EqualTo(OrderStatus.Placed));
        }

        // ------------------------------------------------------------------
        // TC-19 | BVA — quantity = stock + 1 should be rejected
        // Expected: FAIL (BUG B6 off-by-one lets this slip through)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-19 [Black Box / BVA] Quantity = stock + 1 should be rejected")]
        public void PlaceOrder_QuantityOneOverStock_ShouldThrow()
        {
            var product = new Product(52, "Stock5 Item", 200m, stock: 5);
            _cart.AddItem(product, 6);

            // BUG B6: 6 > 5 + 1 = false → accepted incorrectly
            Assert.Throws<OutOfStockException>(
                () => _orderService.PlaceOrder(_cart),
                "Expected OutOfStockException — quantity exceeds stock by 1.");
        }
    }
}
