using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Services;

public class FeatureFlagsService(IRepository<FeatureFlag> repository) : IFeatureFlags
{
    private readonly IRepository<FeatureFlag> _repository = repository;

    public async Task<bool> IsEnabledAsync(string featureName)
    {
        var flags = await _repository.FindAsync(f => f.Name == featureName);
        var flag = flags.FirstOrDefault();
        return flag?.IsEnabled ?? false;
    }
}
