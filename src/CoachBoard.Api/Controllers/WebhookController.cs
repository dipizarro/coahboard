using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoachBoard.Api.Controllers;

[ApiController]
[Route("webhooks/mercadopago")]
[AllowAnonymous]
public class WebhookController : ControllerBase
{
    private readonly IMercadoPagoClient _mpClient;
    private readonly CoachBoardDbContext _dbContext;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IMercadoPagoClient mpClient, 
        CoachBoardDbContext dbContext, 
        ILogger<WebhookController> logger)
    {
        _mpClient = mpClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Receive([FromHeader(Name = "x-signature")] string? xSignature, [FromHeader(Name = "x-request-id")] string? xRequestId)
    {
        if (string.IsNullOrEmpty(xSignature) || string.IsNullOrEmpty(xRequestId))
        {
            _logger.LogWarning("Missing MercadoPago webhook headers.");
            return Unauthorized();
        }

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var parts = xSignature!.Split(',');
        var ts = parts.FirstOrDefault(p => p.StartsWith("ts="))?.Substring(3);

        if (string.IsNullOrEmpty(ts))
        {
             return Unauthorized();
        }

        string? dataId = null;
        try 
        {
            using var json = JsonDocument.Parse(body);
             // Robust parsing using GetProperty
            try 
            {
                dataId = json.RootElement.GetProperty("data").GetProperty("id").ToString();
            }
            catch 
            {
                try 
                {
                    dataId = json.RootElement.GetProperty("id").ToString();
                }
                catch {}
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to parse webhook body");
            return BadRequest();
        }
        
        if (string.IsNullOrEmpty(dataId))
        {
             _logger.LogWarning("Could not extract data.id from webhook");
             return Ok(); 
        }

        var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts};";

        if (!_mpClient.VerifyWebhookSignature(xSignature!, manifest))
        {
            _logger.LogWarning("Invalid webhook signature.");
            return Unauthorized();
        }

        _logger.LogInformation("Processing webhook for Data ID: {DataId}", dataId);
        
        await _mpClient.GetPaymentOrSubscriptionAsync(dataId);
        
        Guid? matchedGuid = null;
        if (dataId.StartsWith("STUB_FOR_"))
        {
            var guidStr = dataId.Replace("STUB_FOR_", "");
             if (Guid.TryParse(guidStr, out var g)) matchedGuid = g;
        }

        Subscription? sub = null;
        
        if (matchedGuid.HasValue)
        {
             var guidString = matchedGuid.Value.ToString();
             // Direct DB access to bypass tenant filter (System Context)
             sub = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.ProviderSubscriptionId == guidString);
        }
        
        if (sub != null)
        {
            if (sub.Status == SubscriptionStatus.Pending)
            {
                sub.Status = SubscriptionStatus.Active;
                sub.ProviderSubscriptionId = dataId; 
                sub.UpdatedAt = DateTime.UtcNow;
                
                _dbContext.Subscriptions.Update(sub);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Subscription {Id} activated.", sub.Id);
            }
            else
            {
                 _logger.LogInformation("Subscription {Id} already processed (Status: {Status}).", sub.Id, sub.Status);
            }
        }
        else
        {
            _logger.LogWarning("No subscription found for Data ID: {DataId}", dataId);
        }

        return Ok();
    }
}
