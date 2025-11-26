# PG-takehome
A take-home assessment for PG. It utilizes a react-front end with a node/express backend for API manipulation.

# Project Description (For Reference)

- see: https://www.alphavantage.co/documentation/

## Requirements

- create a self-hosted C# .Net 8+  Java or React solution

    ### Notes about tech choice

    - I chose to use a React/Node tech stack given my previous experience with react and node, as well as demonstrating tiered 
    web development (front-end, middle-end, back-end).
    - Since the specifications allowed for a React solution and that React alone can’t expose an API, I paired it with Node/Express to securely 
    consume Alpha Vantage and return the required format. The program architecture still remains very simple despite a second tier.
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

- commit(s) to a github repository and provide link OR zip the contents and share