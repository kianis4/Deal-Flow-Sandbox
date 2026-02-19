using DealFlow.Contracts.Messages;
using MassTransit;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Json;

namespace DealFlow.NotifyWorker.Consumers;

public class DealScoredConsumer(
    IConfiguration config,
    ILogger<DealScoredConsumer> logger) : IConsumer<DealScored>
{
    public async Task Consume(ConsumeContext<DealScored> context)
    {
        var msg = context.Message;

        // Always log Teams-style payload
        var payload = new
        {
            type = "AdaptiveCard",
            title = $"Deal Scored — {msg.RiskFlag} Risk",
            dealId = msg.DealId,
            score = msg.Score,
            riskFlag = msg.RiskFlag,
            scoredAt = msg.ScoredAt
        };

        logger.LogInformation("[NOTIFY] Teams payload:\n{Payload}",
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));

        // Optional: send via SendGrid if key is configured
        var sendgridKey = config["SendGrid:ApiKey"];
        if (!string.IsNullOrWhiteSpace(sendgridKey))
        {
            await SendEmailAsync(sendgridKey, msg, logger);
        }

        // Update deal status to NOTIFIED
        logger.LogInformation("Notification sent for deal {DealId} [correlation: {CorrelationId}]",
            msg.DealId, msg.CorrelationId);
    }

    private static async Task SendEmailAsync(string apiKey, DealScored msg, ILogger logger)
    {
        try
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("noreply@dealflow.demo", "DealFlow Sandbox");
            var to = new EmailAddress("demo@example.com");
            var subject = $"[DealFlow] Deal {msg.DealId} scored — {msg.RiskFlag}";
            var body = $"Score: {msg.Score}/100 | Risk: {msg.RiskFlag} | Scored at: {msg.ScoredAt:u}";
            var mail = MailHelper.CreateSingleEmail(from, to, subject, body, body);
            var response = await client.SendEmailAsync(mail);
            logger.LogInformation("SendGrid response: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SendGrid send failed — notification still logged to console");
        }
    }
}
