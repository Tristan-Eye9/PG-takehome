
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

    // Parse initial Json
    using var preParse = JsonDocument.Parse(json);
    var root = preParse.RootElement;

    // Check for "Note" output the alphavantage api sometimes gives
    if (root.TryGetProperty("Note", out var note))
        return Results.BadRequest(new { error = note.GetString() });

    // If supported timeInterval, populate timeSeries
    if (!root.TryGetProperty($"Time Series ({timeInterval})", out var timeSeries))
        return Results.BadRequest(new { error = "No time series data found" });

    // Dictionary keyed by day
    var groupByDay = new Dictionary<string, (decimal lowSum, decimal highSum, long volumeSum, int count)>();

    foreach (var kvp in timeSeries.EnumerateObject())
    {

        // set up timestamp sorting
        var timestamp = kvp.Name; // gives full timestamp with time included
        var day = timestamp.Substring(0, 10); //Strips the time from the timestamp YYYY-MM-DD

        // set up each entry in the dictionary
        var entry = kvp.Value;
        var low = decimal.Parse(entry.GetProperty("3. low").GetString()!, CultureInfo.InvariantCulture);
        var high = decimal.Parse(entry.GetProperty("2. high").GetString()!, CultureInfo.InvariantCulture);
        var volume = long.Parse(entry.GetProperty("5. volume").GetString()!, CultureInfo.InvariantCulture);

        // initialize a day that doesn't exist
        if (!groupByDay.ContainsKey(day)){
        groupByDay[day] = (0m, 0m, 0L, 0);
        }

        // perform summation arithmetic
        var current = groupByDay[day];
        groupByDay[day] = (
            current.lowSum + low,
            current.highSum + high,
            current.volumeSum + volume,
            current.count + 1);
    }

    // perform final arithmetic
    var results = groupByDay.Select(kvp => new
    {
        day = kvp.Key,
        lowAverage = Math.Round(kvp.Value.lowSum / kvp.Value.count, 4),
        highAverage = Math.Round(kvp.Value.highSum / kvp.Value.count, 4),
        volume = kvp.Value.volumeSum
    });
    
    // The specifications want pretty indentions, so I've turned them on
    var options = new JsonSerializerOptions {WriteIndented = true};
    return Results.Json(results, options); //Return transformed data
});

app.Run();

