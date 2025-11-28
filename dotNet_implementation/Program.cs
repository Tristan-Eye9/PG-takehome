using System.Globalization;
using System.Text.Json; //For simple Json Parsing
using DotNetEnv; // For importing .env variables (my api key)
var builder = WebApplication.CreateBuilder(args);

// Load .env file into environment variables
// Note: I prefer using .env files for a variety of reasons, but generally
// they're the easiest way to implement security in early development. And
// Much easier than constantly changing congifuration files.
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

// GET endpoint: Returns transformed intraday data as a formatted json
// Expected Response: 
//    [
//        {
//            "day": "YYYY-MM-DD",
//            "lowAverage": XX.XXXX,
//            "highAverage": XX.XXXX,
//            "volume": XXXXXXXX
//        },
//        ...
//    ]
// Route: /api/intraday/{symbol}
// Example call: http://localhost:5251/api/intraday/IBM
app.MapGet("/api/intraday/{symbol}", async (string symbol) => {
    var timeInterval = "15min"; // Hardcoded due to specifications, but can be changed for testing
    var outputSize = "compact"; // Full would meet the specifications, but since it requires premium, default to compact.
    var apiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey)) 
        return Results.BadRequest(new{
            error = "Missing ALPHAVANTAGE_API_KEY"
        });
    
    // NOTE: that I cannot actually test outputsize=full without a premium API key. The logic should carry to higher date ranges, however.
    // According to documentation, 'full' would return trailing 30 days, but requires a premium key.
    var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval={timeInterval}&outputsize={outputSize}&apikey={apiKey}";
    using var http = new HttpClient();
    var response = await http.GetAsync(url);

    if (!response.IsSuccessStatusCode)
        return Results.StatusCode((int)response.StatusCode);
    
    var json = await response.Content.ReadAsStringAsync();

    // Begin Json transformation here 
    // Parse initial Json
    using var preParse = JsonDocument.Parse(json);
    var root = preParse.RootElement;

    // Begin error handling
    // Check for "Note" output the alphavantage api sometimes gives
    if (root.TryGetProperty("Note", out var note))
        return Results.BadRequest(new { error = note.GetString() });

    // Sometimes the API just fails and returns various error messages
    if (root.TryGetProperty("Error Message", out var errorMessage))
        return Results.BadRequest(new { error = errorMessage.GetString() });

    // Mostly a defensive check. In reality API calls from this code *shouldn't*
    // result in this. If it does it forwards the error to the new, locally hosted client.
    if (root.TryGetProperty("Information", out var info))
        return Results.BadRequest(new { error = info.GetString() });
    // End error handling

    // If supported timeInterval, populate timeSeries
    if (!root.TryGetProperty($"Time Series ({timeInterval})", out var timeSeries))
        return Results.BadRequest(new { error = "No time series data found" });

    // Dictionary keyed by day - entries are time specific
    var groupByDay = new Dictionary<string, (decimal lowSum, decimal highSum, long volumeSum, int count)>();

    // Essentially, run through the fetched aplhavantage json and iterate through it, populating groupByDay
    foreach (var kvp in timeSeries.EnumerateObject()){

        // set up timestamp sorting
        var timestamp = kvp.Name; // gives full timestamp with time included
        var day = timestamp.Substring(0, 10); //Strips the time from the timestamp YYYY-MM-DD

        // parse the entry (a time) for the values we need (low, high, volume)
        var entry = kvp.Value;
        var low = decimal.Parse(entry.GetProperty("3. low").GetString()!, CultureInfo.InvariantCulture);
        var high = decimal.Parse(entry.GetProperty("2. high").GetString()!, CultureInfo.InvariantCulture);
        var volume = long.Parse(entry.GetProperty("5. volume").GetString()!, CultureInfo.InvariantCulture);

        // initialize a day that doesn't exist
        if (!groupByDay.ContainsKey(day)){
        groupByDay[day] = (0m, 0m, 0L, 0);
        }

        // perform summation arithmetic. Populate the day with starting values.
        // Note: I could have averaged here, but the logic would have been more complex. I chose
        // to use a count variable and calculate the avg outside of the loop for simplicity.
        var current = groupByDay[day];
        groupByDay[day] = (
            current.lowSum + low,
            current.highSum + high,
            current.volumeSum + volume,
            current.count + 1);
    }

    // Perform final arithmetic. Average each day using the stored count of entries that day.
    // NOTE: While count could theoretically be hardcoded due to X time always being in a given 
    // day, defensive coding here was more practical. I also noted that some symbols gave 
    // strange sets of data despite using the compact API path parameter - LSE for example.

    var results = groupByDay.OrderByDescending(kvp => kvp.Key).Select(kvp => new {
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