namespace ECommerceApp.Core
{
    /// <summary>
    /// Resolves discount percentages from promotional codes.
    ///
    /// BUG LIST:
    ///   B8 – GetDiscountPercent: unknown / empty codes return 50% instead of 0%
    /// </summary>
    public class DiscountService
    {
        private readonly Dictionary<string, int> _codes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "SAVE10", 10 },
            { "SAVE20", 20 },
            { "WELCOME", 15 },
        };

        // -------------------------------------------------------
        // BUG B8: Unknown codes should yield 0% discount.
        //         Instead the fallback returns 50%, giving a huge
        //         unintended discount whenever the code is invalid.
        // -------------------------------------------------------
        public int GetDiscountPercent(string? code)
        {
            if (code != null && _codes.TryGetValue(code, out var percent))
                return percent;

            // BUG B8 — wrong fallback value:
            //   Correct: return 0;
            return 50;
        }

        public bool IsValidCode(string? code) =>
            code != null && _codes.ContainsKey(code);

        public IReadOnlyCollection<string> AvailableCodes => _codes.Keys;
    }
}
