
using System.Globalization;
using System.Text.Json; //For simple Json Parsing
using DotNetEnv; // For importing .env variables (my api key)

var builder = WebApplication.CreateBuilder(args);

// Load .env file into environment variables
Env.Load();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();



app.MapGet("/api/intraday/{symbol}", async (string symbol) => {
    var timeInterval = "15min"; // Hardcoded due to specifications, but can be changed for testing
    var apiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey)) 
        return Results.BadRequest(new
        {
            error = "Missing ALPHAVANTAGE_API_KEY"
        });
    
    var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval={timeInterval}&outputsize=compact&apikey={apiKey}";
    using var http = new HttpClient();
    var response = await http.GetAsync(url);

    if (!response.IsSuccessStatusCode)
        return Results.StatusCode((int)response.StatusCode);
    
    var json = await response.Content.ReadAsStringAsync();

    //TODO: Parse the response

    //Set up map
    using var preParse = JsonDocument.Parse(json);
    var root = preParse.RootElement;

    // If supported timeInterval, populate timeSeries
    if (!root.TryGetProperty($"Time Series ({timeInterval})", out var timeSeries))
        return Results.BadRequest(new { error = "No time series data found" });

    //build tuples
    var data = new List<(decimal low, decimal high, long volume)>();

    foreach (var i in timeSeries.EnumerateObject())
    {
        var entry = i.Value;
        
        var low = decimal.Parse(entry.GetProperty("3. low").GetString()!, CultureInfo.InvariantCulture);
        var high = decimal.Parse(entry.GetProperty("2. high").GetString()!, CultureInfo.InvariantCulture);
        var volume = long.Parse(entry.GetProperty("5. volume").GetString()!, CultureInfo.InvariantCulture);

        data.Add((low, high, volume));
    }
    
    return Results.Text(json, "application/json");

});

app.Run();

