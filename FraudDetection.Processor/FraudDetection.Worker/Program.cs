using FraudDetection.Worker;
using Polly;
using Polly.Extensions.Http;

var builder = Host.CreateApplicationBuilder(args);

// DEFINING THE POLICY:
// 1. Handle HTTP 5xx errors or network failures
// 2. Break the circuit after 3 consecutive failures
// 3. Keep the circuit broken (open) for 30 seconds
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30)
    );

// Register the Client WITH the policy
// Read API URL from Config, default to localhost if missing
var apiUrl = builder.Configuration["ApiUrl"] ?? "http://localhost:5000";

builder.Services.AddHttpClient("FraudService", client =>
{
    client.BaseAddress = new Uri(apiUrl); 
})
.AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();