using DotNetEnv;

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
    var apiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey)) 
        return Results.BadRequest(new
        {
            error = "Missing ALPHAVANTAGE_API_KEY"
        });
    
    var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval=15min&outputsize=compact&apikey={apiKey}";
    using var http = new HttpClient();
    var response = await http.GetAsync(url);

    if (!response.IsSuccessStatusCode)
        return Results.StatusCode((int)response.StatusCode);
    
    var json = await response.Content.ReadAsStringAsync();

    //TODO: Parse the response

    return Results.Text(json, "application/json");

});

app.Run();

