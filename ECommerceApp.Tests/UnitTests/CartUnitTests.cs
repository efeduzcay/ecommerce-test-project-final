using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests.UnitTests
{
    /// <summary>
    /// WHITE BOX (Unit) Tests — internal implementation is known.
    /// Tests target individual methods of Cart with full knowledge of the code.
    /// </summary>
    [TestFixture]
    public class CartUnitTests
    {
        private Cart _cart = null!;
        private Product _productA = null!;
        private Product _productB = null!;

        [SetUp]
        public void SetUp()
        {
            _cart     = new Cart();
            _productA = new Product(1, "Laptop", 15000m, 10);
            _productB = new Product(2, "Mouse",    250m, 50);
        }

        // ------------------------------------------------------------------
        // TC-01 | BUG B1 — AddItem should increment quantity, not duplicate
        // Expected: FAIL (BUG B1 causes duplicate row → ItemCount = 2, not 1)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-01 [White Box] Adding same product twice should result in quantity=2, not 2 separate rows")]
        public void AddItem_SameProduct_ShouldIncrementQuantity()
        {
            _cart.AddItem(_productA, 1);
            _cart.AddItem(_productA, 1);

            // BUG B1: cart will have 2 rows (ItemCount=2) instead of 1 row with Quantity=2
            Assert.That(_cart.ItemCount, Is.EqualTo(1),
                "Expected only one cart line; quantity should be aggregated.");
            Assert.That(_cart.Items[0].Quantity, Is.EqualTo(2),
                "Expected quantity=2 on the single cart line.");
        }

        // ------------------------------------------------------------------
        // TC-02 | BUG B2 — GetTotal with discount should apply correctly
        // Expected: FAIL (BUG B2 causes discountFactor=1 due to int division)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-02 [White Box] 10% discount on 100₺ product should yield 90₺")]
        public void GetTotal_WithDiscount_ShouldCalculateCorrectly()
        {
            var cheapProduct = new Product(99, "TestItem", 100m);
            _cart.AddItem(cheapProduct, 1);

            decimal total = _cart.GetTotal(discountPercent: 10);

            // BUG B2: int division  10 / 100 = 0  →  discountFactor = 1  →  total stays 100
            Assert.That(total, Is.EqualTo(90m),
                "10% discount on 100₺ should produce 90₺, not 100₺.");
        }

        // ------------------------------------------------------------------
        // TC-03 | BUG B3 — RemoveItem of non-existent product should throw
        // Expected: FAIL (BUG B3 silently does nothing instead of throwing)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-03 [White Box] Removing a product that is not in the cart should throw ArgumentException")]
        public void RemoveItem_NonExistent_ShouldThrowException()
        {
            _cart.AddItem(_productA, 1);

            // BUG B3: no exception is thrown; method silently returns
            Assert.Throws<ArgumentException>(
                () => _cart.RemoveItem(_productB.Id),
                "Expected ArgumentException when removing a product not in cart.");
        }

        // ------------------------------------------------------------------
        // TC-04 | Happy path — adding a valid product should increase count
        // Expected: PASS
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-04 [White Box] Adding a valid product should set ItemCount to 1")]
        public void AddItem_ValidProduct_ShouldIncreaseItemCount()
        {
            _cart.AddItem(_productA, 1);

            Assert.That(_cart.ItemCount, Is.EqualTo(1));
        }

        // ------------------------------------------------------------------
        // TC-05 | Happy path — empty cart total should be 0
        // Expected: PASS
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-05 [White Box] Empty cart total should be zero")]
        public void GetTotal_EmptyCart_ShouldReturnZero()
        {
            decimal total = _cart.GetTotal();

            Assert.That(total, Is.EqualTo(0m));
        }
    }
}
