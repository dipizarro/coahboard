using System.Threading.Tasks;

namespace CoachBoard.Application.Interfaces;

public interface IBillingAccessService
{
    Task<bool> CanAccessProAsync();
}
