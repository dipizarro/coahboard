using CoachBoard.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoachBoard.Infrastructure.Payment;

public class MercadoPagoClient : IMercadoPagoClient
{
    private readonly HttpClient _httpClient;
    private readonly MercadoPagoOptions _options;
    private readonly ILogger<MercadoPagoClient> _logger;

    public MercadoPagoClient(
        HttpClient httpClient, 
        IOptions<MercadoPagoOptions> options, 
        ILogger<MercadoPagoClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task<string> CreateProCheckoutAsync(int userId, string email, int tenantId)
    {
        _logger.LogInformation("Creating Pro Checkout for User {UserId} ({Email}) Tenant {TenantId}. Currency: {Currency}", userId, email, tenantId, _options.Currency);
        
        // Stub implementation
        var simulatedCheckoutUrl = $"{_options.PendingUrl}?simulated=true&user={userId}&tenant={tenantId}&plan=pro";
        return Task.FromResult(simulatedCheckoutUrl);
    }

    public Task<object?> GetPaymentOrSubscriptionAsync(string id)
    {
        _logger.LogInformation("Getting Payment/Subscription info for ID: {Id}", id);
        
        // Stub implementation
        return Task.FromResult<object?>(new { Id = id, Status = "active", Mock = true });
    }

    public bool VerifyWebhookSignature(string signature, string payload)
    {
        _logger.LogDebug("Verifying webhook signature. Secret configured: {IsConfigured}", !string.IsNullOrEmpty(_options.WebhookSecret));
        
        // Stub: always return true if secret is present
        return !string.IsNullOrEmpty(_options.WebhookSecret);
    }
}
