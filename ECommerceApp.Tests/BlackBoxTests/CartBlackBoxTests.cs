using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests.BlackBoxTests
{
    /// <summary>
    /// BLACK BOX Tests — tester has NO knowledge of internal implementation.
    /// Tests are designed purely from the specification / requirements.
    /// Input → Output boundaries are checked without inspecting source code.
    /// </summary>
    [TestFixture]
    public class CartBlackBoxTests
    {
        private Cart _cart = null!;

        [SetUp]
        public void SetUp() => _cart = new Cart();

        // ------------------------------------------------------------------
        // TC-06 | Happy path — adding a valid product
        // Expected: PASS
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-06 [Black Box] A valid product add should result in ItemCount = 1")]
        public void AddItem_ValidProduct_ItemCountIsOne()
        {
            var product = new Product(1, "Headphones", 500m, 20);

            _cart.AddItem(product, 1);

            Assert.That(_cart.ItemCount, Is.EqualTo(1));
        }

        // ------------------------------------------------------------------
        // TC-07 | Boundary — product with negative price should be rejected
        // Expected: FAIL (no price validation exists in Cart.AddItem)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-07 [Black Box] Adding a product with a negative price should throw an exception")]
        public void AddItem_NegativePriceProduct_ShouldThrowException()
        {
            // From spec: prices must be non-negative
            var badProduct = new Product(2, "FakeProduct", -10m, 5);

            // BUG: No validation in Cart.AddItem → item is added silently
            Assert.Throws<ArgumentException>(
                () => _cart.AddItem(badProduct, 1),
                "Expected rejection of product with negative price.");
        }

        // ------------------------------------------------------------------
        // TC-08 | Calculation check — cart total for 2 equal-price products
        // Expected: PASS
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-08 [Black Box] Two products at 50₺ each should total 100₺")]
        public void GetTotal_TwoProductsAt50_ReturnsWith100()
        {
            var p1 = new Product(3, "ItemA", 50m);
            var p2 = new Product(4, "ItemB", 50m);

            _cart.AddItem(p1, 1);
            _cart.AddItem(p2, 1);

            Assert.That(_cart.GetTotal(), Is.EqualTo(100m));
        }
    }
}
