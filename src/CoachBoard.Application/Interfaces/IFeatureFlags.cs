namespace CoachBoard.Application.Interfaces;

public interface IFeatureFlags
{
    Task<bool> IsEnabledAsync(string featureName);
}
