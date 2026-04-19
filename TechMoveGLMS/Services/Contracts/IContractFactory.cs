using TechMoveGLMS.Models.Entities;

namespace TechMoveGLMS.Services.Contracts
{
    // Factory Method Pattern - Creator interface
    public interface IContractFactory
    {
        Contract CreateContract(string serviceLevel, int clientId, DateTime startDate, DateTime endDate);
    }
}