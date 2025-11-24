using Confluent.Kafka;
using FraudDetection.API.Hubs;
using FraudDetection.API.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace FraudDetection.API.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IHubContext<FraudHub> _hubContext;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _topic = "transactions";

    public KafkaConsumerService(IHubContext<FraudHub> hubContext, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _hubContext = hubContext;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _config["Kafka:BootstrapServers"] ?? "localhost:9092";
        
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "react-dashboard-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var json = result.Message.Value;
                var tx = JsonSerializer.Deserialize<Transaction>(json);

                if (tx != null)
                {
                    // 1. Call Python API
                    var client = _httpClientFactory.CreateClient("FraudService");
                    var response = await client.PostAsJsonAsync("/predict", tx);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var prediction = await response.Content.ReadFromJsonAsync<FraudPrediction>();

                        // 2. PUSH TO REACT via SignalR
                        await _hubContext.Clients.All.SendAsync("ReceiveFraudAlert", new 
                        {
                            tx.TransactionId,
                            tx.Amount,
                            tx.MerchantId,
                            RiskScore = prediction.RiskScore,
                            IsFraud = prediction.IsFraud,
                            Timestamp = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex) 
            {
                // Log error
            }
        }
    }
}