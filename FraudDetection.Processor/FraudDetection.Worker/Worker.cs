using Confluent.Kafka;
using FraudDetection.Worker.Models;
using System.Text.Json;
using System.Net.Http.Json; 
using Polly.CircuitBreaker;

namespace FraudDetection.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _inputTopic = "transactions";
    private readonly string _dlqTopic = "transactions-dlq"; // The safety net
    private readonly string _bootstrapServers = "localhost:9092";

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. CONSUMER CONFIG
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "fraud-detector-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        // 2. PRODUCER CONFIG (For the DLQ)
        var producerConfig = new ProducerConfig { BootstrapServers = _bootstrapServers };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();

        consumer.Subscribe(_inputTopic);
        
        _logger.LogInformation("Global Fraud Shield Activated. Resilience Mode: ON.");

        while (!stoppingToken.IsCancellationRequested)
        {
            string rawMessage = "";
            try
            {
                var result = consumer.Consume(stoppingToken);
                rawMessage = result.Message.Value;

                var transaction = JsonSerializer.Deserialize<Transaction>(rawMessage);
                
                if (transaction != null)
                {
                    // CALL PYTHON API (Protected by Circuit Breaker)
                    var client = _httpClientFactory.CreateClient("FraudService");
                    var response = await client.PostAsJsonAsync("/predict", transaction);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var prediction = await response.Content.ReadFromJsonAsync<FraudPrediction>();
                        _logger.LogInformation($"[PROCESSED] {transaction.TransactionId} -> Risk: {prediction?.RiskScore}");
                    }
                    else
                    {
                        throw new HttpRequestException($"API Error: {response.StatusCode}");
                    }
                }
            }
            catch (BrokenCircuitException)
            {
                _logger.LogWarning($"[CIRCUIT OPEN] Python API is down. Moving to DLQ: {rawMessage}");
                await SendToDlq(producer, rawMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[FAILURE] {ex.Message}. Moving to DLQ.");
                await SendToDlq(producer, rawMessage);
            }
        }
    }

    private async Task SendToDlq(IProducer<Null, string> producer, string message)
    {
        try
        {
            await producer.ProduceAsync(_dlqTopic, new Message<Null, string> { Value = message });
            _logger.LogInformation($"[DLQ] Message saved to {_dlqTopic}");
        }
        catch (Exception ex)
        {
             _logger.LogCritical($"[FATAL] Could not write to DLQ: {ex.Message}");
        }
    }
}