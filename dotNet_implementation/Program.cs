using System.Globalization;
using System.Text.Json; //For simple Json Parsing
using DotNetEnv; // For importing .env variables (my api key)
using System.IO; // For testing IO
var builder = WebApplication.CreateBuilder(args);

// Load .env file into environment variables
Env.Load();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()){
    app.MapOpenApi();
}
app.UseHttpsRedirection();

app.MapGet("/api/intraday/{symbol}", async (string symbol) => {
    var timeInterval = "15min"; // Hardcoded due to specifications, but can be changed for testing
    var outputSize = "compact"; // Full would meet the specifications, but since it requires premium, I'm using compact.
    var useFixture = Environment.GetEnvironmentVariable("USE_FIXTURE") == "true";
    string json;

    if (useFixture) {
        var fixturePath = Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "intraday_15min_month.json");
        if (!File.Exists(fixturePath))
        {
            // Try repo-root relative path as a fallback
            fixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures", "intraday_15min_month.json");
        }

        if (!File.Exists(fixturePath))
            return Results.BadRequest(new { error = $"Fixture not found at expected paths. Create fixtures/intraday_15min_month.json or unset USE_FIXTURE." });

        json = await File.ReadAllTextAsync(fixturePath);
    }
    else {
        var apiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            return Results.BadRequest(new { error = "Missing ALPHAVANTAGE_API_KEY" });

        var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval={timeInterval}&outputsize={outputSize}&apikey={apiKey}";
        using var http = new HttpClient();
        var response = await http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return Results.StatusCode((int)response.StatusCode);

        json = await response.Content.ReadAsStringAsync();
    }

    // Parse initial Json
    using var preParse = JsonDocument.Parse(json);
    var root = preParse.RootElement;

    // Begin error handling
    // Check for "Note" output the alphavantage api sometimes gives
    if (root.TryGetProperty("Note", out var note))
        return Results.BadRequest(new { error = note.GetString() });

    if (root.TryGetProperty("Error Message", out var errorMessage))
        return Results.BadRequest(new { error = errorMessage.GetString() });

    if (root.TryGetProperty("Information", out var info))
        return Results.BadRequest(new { error = info.GetString() });
    // End error handling

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
    var results = groupByDay.OrderByDescending(kvp => kvp.Key).Select(kvp => new
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