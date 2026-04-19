namespace TechMoveGLMS.Services.Pricing
{
    public class EnterprisePricingStrategy : IPricingStrategy
    {
        private const decimal ENTERPRISE_DISCOUNT = 0.85m;
        private const decimal DISTANCE_RATE = 2.50m;
        
        public decimal CalculatePrice(decimal baseCost, string serviceLevel, decimal distance = 0, bool isPriority = false)
        {
            var price = baseCost * ENTERPRISE_DISCOUNT;
            if (distance > 0) price += distance * DISTANCE_RATE;
            return price;
        }
    }
}