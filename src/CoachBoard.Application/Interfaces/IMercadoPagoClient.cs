namespace CoachBoard.Application.Interfaces;

public interface IMercadoPagoClient
{
    Task<string> CreateProCheckoutAsync(int userId, string email, int tenantId, string externalReference);
    Task<object?> GetPaymentOrSubscriptionAsync(string id);
    bool VerifyWebhookSignature(string signature, string payload);
}
