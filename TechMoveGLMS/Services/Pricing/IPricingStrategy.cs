namespace TechMoveGLMS.Services.Pricing
{
    // Strategy Pattern - Strategy interface
    public interface IPricingStrategy
    {
        decimal CalculatePrice(decimal baseCost, string serviceLevel, decimal distance = 0, bool isPriority = false);
    }
}