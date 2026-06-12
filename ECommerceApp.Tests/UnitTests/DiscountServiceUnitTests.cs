using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests.UnitTests
{
    /// <summary>
    /// WHITE BOX (Unit) Tests for DiscountService.
    /// Uses Equivalence Partitioning over the (valid code) / (invalid code) /
    /// (empty / null) partitions.
    /// </summary>
    [TestFixture]
    public class DiscountServiceUnitTests
    {
        private DiscountService _discount = null!;

        [SetUp]
        public void SetUp() => _discount = new DiscountService();

        // ------------------------------------------------------------------
        // TC-14 | EP — valid code partition: "SAVE10" should return 10
        // Expected: PASS
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-14 [White Box / EP] Known code 'SAVE10' should yield 10% discount")]
        public void GetDiscountPercent_KnownCode_ReturnsExpectedPercent()
        {
            int percent = _discount.GetDiscountPercent("SAVE10");

            Assert.That(percent, Is.EqualTo(10),
                "Known promotional code should return its mapped discount percent.");
        }

        // ------------------------------------------------------------------
        // TC-15 | EP — invalid code partition: unknown codes should yield 0
        // Expected: FAIL (BUG B8 returns 50% as the fallback value)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-15 [White Box / EP] Unknown code 'FAKE123' should yield 0% discount, not 50%")]
        public void GetDiscountPercent_UnknownCode_ReturnsZero()
        {
            int percent = _discount.GetDiscountPercent("FAKE123");

            // BUG B8: fallback returns 50 instead of 0
            Assert.That(percent, Is.EqualTo(0),
                "Unknown promotional code should return 0%, not the bug fallback 50%.");
        }

        // ------------------------------------------------------------------
        // TC-16 | BVA — boundary "empty string" should be treated as invalid
        // Expected: FAIL (BUG B8 — same wrong fallback path)
        // ------------------------------------------------------------------
        [Test]
        [Description("TC-16 [White Box / BVA] Empty-string code should yield 0% discount")]
        public void GetDiscountPercent_EmptyString_ReturnsZero()
        {
            int percent = _discount.GetDiscountPercent(string.Empty);

            // BUG B8: fallback returns 50 instead of 0
            Assert.That(percent, Is.EqualTo(0),
                "Empty code should be treated as invalid and return 0%, not 50%.");
        }
    }
}
