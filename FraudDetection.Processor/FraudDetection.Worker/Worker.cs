using Confluent.Kafka;
using FraudDetection.Worker.Models;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace FraudDetection.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory; // Add Factory
    private readonly string _topic = "transactions";
    private readonly string _bootstrapServers = "localhost:9092";

    // Inject HttpClientFactory
    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "fraud-detector-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_topic);
        
        _logger.LogInformation("Global Fraud Shield Activated. Listening...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. Get Message from Kafka
                var result = consumer.Consume(stoppingToken);
                var jsonString = result.Message.Value;

                // 2. Deserialize to C# Object
                var transaction = JsonSerializer.Deserialize<Transaction>(jsonString);
                
                if (transaction != null)
                {
                    _logger.LogInformation($"Analyzing TX: {transaction.TransactionId} | Amount: ${transaction.Amount}");

                    // 3. Call Python API
                    var client = _httpClientFactory.CreateClient("FraudService");
                    
                    // PostAsJsonAsync handles serialization automatically
                    var response = await client.PostAsJsonAsync("/predict", transaction);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var prediction = await response.Content.ReadFromJsonAsync<FraudPrediction>();
                        
                        if (prediction.IsFraud)
                        {
                            _logger.LogWarning($"[FRAUD DETECTED] TX: {transaction.TransactionId} | Risk: {prediction.RiskScore}");
                        }
                        else
                        {
                            _logger.LogInformation($"[CLEAN] TX: {transaction.TransactionId} | Risk: {prediction.RiskScore}");
                        }
                    }
                    else
                    {
                        _logger.LogError($"API Error: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        }
    }
}