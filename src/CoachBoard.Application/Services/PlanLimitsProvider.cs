using CoachBoard.Application.Interfaces;
using CoachBoard.Application.Models;
using CoachBoard.Domain.Enums;

namespace CoachBoard.Application.Services;

public class PlanLimitsProvider : IPlanLimitsProvider
{
    private const int UNLIMITED = -1;

    public PlanLimits GetLimits(SubscriptionPlan plan)
    {
        return plan switch
        {
            SubscriptionPlan.Free => new PlanLimits(MaxAthletes: 5, MaxRoutines: 20, MaxCoaches: 1),
            SubscriptionPlan.Pro => new PlanLimits(MaxAthletes: UNLIMITED, MaxRoutines: UNLIMITED, MaxCoaches: UNLIMITED),
            _ => new PlanLimits(MaxAthletes: 5, MaxRoutines: 20, MaxCoaches: 1) // Default to Free for unknown
        };
    }
}
