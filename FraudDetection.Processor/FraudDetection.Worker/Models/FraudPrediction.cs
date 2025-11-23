using System.Text.Json.Serialization;

namespace FraudDetection.Worker.Models;

public class FraudPrediction
{
    [JsonPropertyName("IsFraud")]
    public bool IsFraud { get; set; }

    [JsonPropertyName("RiskScore")]
    public double RiskScore { get; set; }

    [JsonPropertyName("Reason")]
    public string? Reason { get; set; }
}