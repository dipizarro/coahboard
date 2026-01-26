namespace CoachBoard.Application.Exceptions;

public class PlanLimitReachedException : Exception
{
    public string ResourceName { get; }
    public int Limit { get; }

    public PlanLimitReachedException(string resourceName, int limit)
        : base($"Has alcanzado el límite de {limit} para {resourceName} en tu plan actual.")
    {
        ResourceName = resourceName;
        Limit = limit;
    }
}
