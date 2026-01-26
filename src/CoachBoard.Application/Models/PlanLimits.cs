namespace CoachBoard.Application.Models;

/// <summary>
/// Representa los límites asignados a un plan de suscripción.
/// El valor -1 indica ilimitado.
/// </summary>
public record PlanLimits(int MaxAthletes, int MaxRoutines, int MaxCoaches);
