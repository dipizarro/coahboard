using System;
using CoachBoard.Domain.Enums;

namespace CoachBoard.Application.DTOs;

public record BillingStatusDto(
    SubscriptionPlan Plan,
    SubscriptionStatus SubscriptionStatus,
    bool CanAccessPro,
    DateTime? CurrentPeriodEnd,
    string? ProviderSubscriptionId
);
