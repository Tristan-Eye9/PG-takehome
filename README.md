# PG-takehome
A take‑home assessment for PG. This implementation uses a C# .NET 10 Minimal API to consume Alpha Vantage intraday data and expose a new JSON endpoint.

## Running the Program

- Note that this process should be relatively simple. It should be very similar to running the .Net template when creating a new web API.

    ### Prerequisites
    - .Net 10 sdk installed
    - Alphavantage API key (free or premium)

    ### Setup
    1. Clone the project.
    2. Create an `.env` file in the `/dotNet_implementation` directory with: ALPHAVANTAGE_API_KEY=your_api_key_here
    3. This project uses [DotNetEnv](https://www.nuget.org/packages/DotNetEnv) to load environment variables from a `.env` file.
    Be sure it is loaded into the project, as it loads environment variables.
    4.  IMPORTANT NOTE: Due to limitations of the free alphavantage API, the current implementation calls the API using the `compact`
    path parameter. For accurate usage according to specs (requires premium key), change line 39 of `dotNet_implementation/Program.cs` to: 
    ```
    var outputSize = "full";
    ```
    5. Run the API while in the `/dotNet_implementation` directory:
    ```
    dotnet run
    ```
    6. The API will start locally (default: http://localhost:5251).

    ### Example Request
    ```
    curl http://localhost:5251/api/intraday/MSFT
    ```

    ### Example Response
    ```
    [
        {
            "day": "2025-11-25",
            "lowAverage": 472.1556,
            "highAverage": 475.7339,
            "volume": 38883569
        },
        {
            "day": "2025-11-24",
            "lowAverage": 471.4530,
            "highAverage": 477.0950,
            "volume": 33357081
        }
    ]
    ```


# Project Description & Commentary (For Reference)

- see: https://www.alphavantage.co/documentation/

## Requirements

- create a self-hosted C# .Net 8+  Java or React solution
    - This project is implemented as a Minimal API in .NET 10.
    - API keys are stored securely in environment variables via .env and never exposed client‑side.

    ### Notes about tech choice

    - My initial plan was to use a Node/Express backend, since I already had that environment configured locally and it made rapid prototyping simple.
    - However, the assignment’s core requirement is to expose a self‑hosted JSON endpoint, and the team indicated a preference for .NET. 
    I saw this as a good opportunity to practice C# and .NET Minimal APIs, which are likely more robust to set up for everyone.
    - This implementation therefore focuses on the .NET backend. I emphasized a specification-first API that demonstrates grouping, averaging, and error 
    handling logic in C#.

- consume the api and expose a new api that:

- takes the symbol as a string parameter
    - /api/intraday/{symbol}

- queries the intraday data for last month

    - Important note regarding API tiers: AlphaVantage free tier only provides around 2 days of data to look at(outputsize=compact).
    This logic should work for any range between 2 days and the 2 months that the premium key might allow. For this submission, I 
    think the logic demonstrates that it would scale up appropriately with a whole month.
    - While the specification requires a 15‑minute interval (hardcoded in this submission), I also tested with a 60‑minute interval. 
    This produced a dataset spanning ~8 trading days with the same 100‑point cap, demonstrating that the logic scales to longer ranges 
    without modification.

- assume the data is updated every 15 minutes

    - Hardcoded to "15min" per specification.
    - Alpha Vantage’s free tier only returns ~100 intraday points (compact mode), which is usually 1–2 trading days 
    at 15‑minute intervals. The same grouping/averaging logic would scale to a full month if premium access (outputsize=full) were available.
    - A simple CS script was built to test cases in the test branch. It helped test if the app could process a full month of data. For 
    further details refer to the testing section.

- groups by the day

- returns a json response in this format:

    ```
    [
        {
            "day": "2009-01-30",
            "lowAverage": 40.2958,
            "highAverage": 49.7534,
            "volume": 49073348
        },
        ...
    ]
    ```

- commit(s) to a github repository and provide link OR zip the contents and share

    ### Notes on Alpha Vantage Free tier
    - By default, outputsize=compact returns ~100 data points (≈ 1–2 trading days at 15‑minute intervals).
    - outputsize=full would return trailing 30 days, but requires a premium key.
    - This submission demonstrates correct grouping/averaging logic, which scales appropriately to a full month if premium access is available.

    ### Error Handling
    - The API gracefully handles Alpha Vantage’s "Note", "Error Message", and "Information" responses.
    - Clear JSON error messages are returned instead of crashes.

## Test Branch

- After careful consideration of project scope and limitations, a Json generator and injector were developed to 'simulate'
how a full month would function if I access to the AlphaVantage Premium features.
- Here are simple instructions to use the tools within test branch. Otherwise a generated Json is already in the 'fixtures' folder of the test branch API.

1. Essentially, there are 2 differences from the primary deployment. The Tool folder essentially exists as a host for a second dotNet project.
It's a simple script designed to take a seed, and other basic user input, and output a Json shaped like the intraday outputs. There isn't guaranteed
accuracy to the actual market, but that isn't important for testing.
2. To begin using it, navigate to the tools directory, and in the console perform `dotnet run`. Run the generator in `tools/`, which outputs 
a JSON to `tools/fixtures/`. Copy that file into `dotNet_implementation/fixtures/`.
3. Ensure that an additional line in the `.env` file exists. This will inject the Json into the program instead of using the actual API.
    ```
    USE_FIXTURE=true
    ```
4. Inspect the output via the exposed endpoint via browser or curl- nothing changes in this part of the process because 
of the way the switch works. Output verification can be performed by-hand, since automating a check wouldn't be useful 
due to replication of API logic.

## Misc. Design Notes
- Chose a Minimal API in .NET 10 to keep the implementation lightweight and aligned with modern best practices. 
This approach minimized boilerplate and made the service easy to configure and run.
- Due to Alpha Vantage’s free tier limitations, only ~100 intraday points are available at 15‑minute intervals. 
The grouping and averaging logic was designed to scale seamlessly to a full month if premium access were available. It was further
tested through my Test Branch.
- The HTTPS redirection warning appears in development when no certificate is configured. Since the API functions correctly over HTTP, 
this does not affect submission or functionality.
- Used the default .NET `.gitignore` template to ensure build artifacts (`bin/`, `obj/`) and environment files (`.env`) are excluded, 
while keeping all necessary source and configuration files in the repository.
- When faced with trade‑offs, I prioritized adherence to the design requirements.

