using FraudDetection.API.Hubs;
using FraudDetection.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR
builder.Services.AddSignalR();

// Add CORS (Crucial for React to talk to .NET)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // The Docker React Port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Needed for WebSockets
    });
});

// Register Python API Client
var apiUrl = builder.Configuration["ApiUrl"] ?? "http://localhost:5000";
builder.Services.AddHttpClient("FraudService", client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// Register Background Service
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.UseCors("ReactPolicy");
app.MapHub<FraudHub>("/fraudHub"); // The WebSocket URL

app.Run();