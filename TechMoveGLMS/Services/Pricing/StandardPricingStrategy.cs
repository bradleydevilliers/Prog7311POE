namespace TechMoveGLMS.Services.Pricing
{
    public class StandardPricingStrategy : IPricingStrategy
    {
        public decimal CalculatePrice(decimal baseCost, string serviceLevel, decimal distance = 0, bool isPriority = false)
        {
            return baseCost;
        }
    }
}