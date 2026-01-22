using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using CoachBoard.Infrastructure.Persistence;

namespace CoachBoard.Api.Tests;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    protected readonly CustomWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly CoachBoardDbContext Db;

    protected BaseIntegrationTest(CustomWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        Db = Scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
    }

    protected async Task AuthenticateAsUserAAsync()
    {
        var token = await Factory.GetUserATokenAsync(Client);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task AuthenticateAsUserBAsync()
    {
        var token = await Factory.GetUserBTokenAsync(Client);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void Dispose()
    {
        Scope.Dispose();
        Client.Dispose();
    }
}
