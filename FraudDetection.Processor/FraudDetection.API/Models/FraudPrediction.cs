namespace FraudDetection.API.Models;
public class FraudPrediction
{
    public bool IsFraud { get; set; }
    public double RiskScore { get; set; }
    public string Reason { get; set; }
}