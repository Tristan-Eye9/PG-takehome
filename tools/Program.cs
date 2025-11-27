// Note: This is a tool I threw together to test my API for a full month worth of data.
// Without the premium API I needed a way to verify/test my program, and this generator allows
// me to generate test cases. It's not perfect, but without direct access to alphavantage it 
// serves its' purpose. It especially helped me when I ran out of API calls.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

// Define the test case generation
var symbol = "MSFT";
var intervalLabel = "15min";
var intervalMinutes = 15;
var seed = 12345;
var rnd = new Random(seed);

// Generate last 30 calendar days ending today
var end = DateTime.UtcNow.Date;
var start = end.AddDays(-30);

// used a sorted dictionary
var timeSeries = new SortedDictionary<string, Dictionary<string, string>>(
    Comparer<string>.Create((a, b) => b.CompareTo(a))
);

// Arbitrary starting price.
decimal price = 300m;

// Iterate through each day in a 30 day window (the requested length of time). This includes weekends.
for (var d = start; d <= end; d = d.AddDays(1)){
    // using UTC times for simplicity, but I don't believe it matters. 
    // They broadly match market hours.
    var open = d.Date.AddHours(13).AddMinutes(30); 
    var close = d.Date.AddHours(20);               

    // Steps through intervals of specified timeframe.
    // Note: It was either more complex calculations or something more static. I opted for more general pattern
    // matching with the data from the last month. It's not really consistent with the actual market,
    // but it's sufficient for testing different values.
    for (var t = open; t <= close; t = t.AddMinutes(intervalMinutes)){
        var delta = (decimal)(rnd.NextDouble() - 0.5) * 0.8m; //simulates random change
        var o = price; // opening price
        var h = Math.Max(o, o + Math.Abs(delta) + 0.2m); // the extra addition adds to randomness, guarantees change over time
        var l = Math.Min(o, o - Math.Abs(delta) - 0.2m); // same here, but subtraction
        var c = o + delta; // closing price
        var v = rnd.Next(20_000, 200_000); // Just a random volume. This varies a lot in actual data anyways.

        // Formats the timestamp and stores the values. Everything needs to match exactly for the injection to work.
        var stamp = t.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        timeSeries[stamp] = new Dictionary<string, string>{
            ["1. open"] = o.ToString("F2", CultureInfo.InvariantCulture),
            ["2. high"] = h.ToString("F2", CultureInfo.InvariantCulture),
            ["3. low"] = l.ToString("F2", CultureInfo.InvariantCulture),
            ["4. close"] = c.ToString("F2", CultureInfo.InvariantCulture),
            ["5. volume"] = v.ToString(CultureInfo.InvariantCulture)
        };
        price = c;
    }
}

// Builds the Json payload to inject
var payload = new Dictionary<string, object>{
    ["Meta Data"] = new Dictionary<string, string>{
        ["1. Information"] = "Intraday (mock)",
        ["2. Symbol"] = symbol,
        ["3. Last Refreshed"] = end.ToString("yyyy-MM-dd"),
        ["4. Interval"] = intervalLabel,
        ["5. Output Size"] = "full"
    },
    [$"Time Series ({intervalLabel})"] = timeSeries
};

// Json formatting like in the primary program
var options = new JsonSerializerOptions { WriteIndented = true };
var json = JsonSerializer.Serialize(payload, options);

// Put the file in its own directory, "fixtures," if it doesn't already exist.
// From the root of this project, it is root::fixtures/intraday_15min_month.json
Directory.CreateDirectory("fixtures");
File.WriteAllText("fixtures/intraday_15min_month.json", json);
Console.WriteLine("Wrote fixtures/intraday_15min_month.json");
