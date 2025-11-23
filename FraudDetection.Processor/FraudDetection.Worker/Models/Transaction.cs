using System.Text.Json.Serialization;

namespace FraudDetection.Worker.Models;

public class Transaction
{
    [JsonPropertyName("TransactionId")]
    public required string TransactionId { get; set; }

    [JsonPropertyName("Amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("MerchantId")]
    public required string MerchantId { get; set; }

    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp { get; set; }
}