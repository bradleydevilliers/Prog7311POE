namespace TechMoveGLMS.Services.Pricing
{
    public class PremiumPricingStrategy : IPricingStrategy
    {
        private const decimal PREMIUM_MARKUP = 1.25m;
        
        public decimal CalculatePrice(decimal baseCost, string serviceLevel, decimal distance = 0, bool isPriority = false)
        {
            var price = baseCost * PREMIUM_MARKUP;
            if (isPriority) price += 50.00m;
            return price;
        }
    }
}