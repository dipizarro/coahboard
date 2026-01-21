using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;

namespace CoachBoard.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(CoachBoardDbContext context) : base(context)
    {
    }
}
