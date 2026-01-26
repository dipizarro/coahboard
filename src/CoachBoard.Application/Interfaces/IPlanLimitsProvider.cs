using CoachBoard.Application.Models;
using CoachBoard.Domain.Enums;

namespace CoachBoard.Application.Interfaces;

public interface IPlanLimitsProvider
{
    PlanLimits GetLimits(SubscriptionPlan plan);
}
