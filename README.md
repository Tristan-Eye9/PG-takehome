# PG-takehome
A take-home assessment for PG. It utilizes a react-front end with a node/express backend for API manipulation.

# Project Description (For Reference)

- see: https://www.alphavantage.co/documentation/

## Requirements

- create a self-hosted C# .Net 8+  Java or React solution

    ### Notes about tech choice

    - React is a browser UI library and cannot host an HTTP endpoint or securely store API keys. To meet the assignment’s core 
    requirement—expose a self‑hosted JSON endpoint—the backend is implemented in Node/Express. Node/Express provides the ability 
    to: host the API and listen for requests; keep the Alpha Vantage key server‑side; and handle logging and structured errors. 
    The React app is included only as an optional demo client to visualize and exercise the endpoint.
    - I also already had a node/react environment set up locally on my machine, which made configuring the development environment simpler.

- consume the api and expose a new api that:

    - Note: Node will consume and translate the API, and then react will get the information accordingly.
    - By design the front-end tier, React in this case, will be relatively simple.

- takes the symbol as a string parameter

- queries the intraday data for last month

    - Important note regarding API tiers: AlphaVantage free tier only provides around 2 days of data to look at(outputsize=compact).
    This logic should work for any range between 2 days and the 2 months that the premium key might allow. For this submission, I 
    think the logic demonstrates that it would scale up appropriately with a whole month.

- assume the data is updated every 15 minutes

    - Interestingly, I can showcase how the logic scales by using a different time interval in the intraday function. I'll get 
    approximately 8 - 10 days of data using this method. It's by no means a perfect test, but it is practical and only uses the 
    free tier of the API.

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

    - Formatting and display is fully done in React. React is given the information from the API and displays it as per
    spec requirements.
    

- commit(s) to a github repository and provide link OR zip the contents and share