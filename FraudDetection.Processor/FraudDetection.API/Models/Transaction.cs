using System.Text.Json.Serialization;

namespace FraudDetection.API.Models;

public class Transaction
{
    [JsonPropertyName("TransactionId")]
    public string TransactionId { get; set; }

    [JsonPropertyName("Amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("MerchantId")]
    public string MerchantId { get; set; }

    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp { get; set; }
}