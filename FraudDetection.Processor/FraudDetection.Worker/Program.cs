using FraudDetection.Worker;

var builder = Host.CreateApplicationBuilder(args);

// 1. Register the HTTP Client for the Python API
builder.Services.AddHttpClient("FraudService", client =>
{
    // The address where your Python Uvicorn server is running
    client.BaseAddress = new Uri("http://localhost:5000"); 
});

// 2. Register the Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();