# PG-takehome
A take-home assessment for PG. It utilizes a react-front end with a node/express backend for API manipulation.

# Project Description (For Reference)

- see: https://www.alphavantage.co/documentation/

## Requirements

- create a self-hosted C# .Net 8+  Java or React solution

    ### Notes about tech choice

    - I chose to use a React/Node tech stack given my previous experience with react and interest in building a simple front-end component. 
    - I also already had a node environment set up locally on my machine, which made configuring the project simpler.

- consume the api and expose a new api that:

- takes the symbol as a string parameter

- queries the intraday data for last month

- assume the data is updated every 15 minutes

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