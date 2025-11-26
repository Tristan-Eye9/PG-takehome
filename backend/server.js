const express = require('express');
const app = express();
const PORT = 3001;
//Use environment variables for API key security
require('dotenv').config();
const apiKey = process.env.ALPHAVANTAGE_API_KEY;

// Intraday API Route for testing purposes
// Example usage: http://localhost:3001/api/intraday/IBM
app.get('/test/intraday/:symbol', async(req, res) => {
  const {symbol} = req.params;
  //URL using params similar to documentation
  const url = `https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=${symbol}&interval=15min&apikey=${apiKey}`;

//Try to fetch the request
try {
  const response = await fetch(url);

  //If the fetch fails
  if (!response.ok){
    return res.status(response.status).json({error: 'API request failed'});
  }

  //otherwise Await a response from the url
  const data = await response.json();
  res.json(data);
} //Otherwise catch any errors
catch (issue){
  console.error(issue);
  res.status(500).json({error: 'Server error fetching data'});
}
});

// Actual implementation for data translation and endpoint configuration.
// Example usage: http://localhost:3001/api/intraday/IBM
app.get('/api/intraday/:symbol', async(req, res) => {
  const {symbol} = req.params;
  //URL using params similar to documentation
  const url = `https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=${symbol}&interval=15min&outputsize=compact&apikey=${apiKey}`;

//Try to fetch the request
try {
  const response = await fetch(url);

  //If the fetch fails
  if (!response.ok){
    return res.status(response.status).json({error: 'API request failed'});
  }

  //otherwise Await a response from the url
  const data = await response.json();

  //Begin data translation here
  const series = data['Time Series (15min)'];
  const GroupByDay = {};

  //Expected formatting:
    // "day": "2009-01-30",
    // "lowAverage": 40.2958,
    // "highAverage": 49.7534,
    // "volume": 49073348

  for (const [timestamp, values] of Object.entries(series)){
    const day = timestamp.slice(0, 10) // returns date in YYYY-MM-DD format for day
    const low = parseFloat(values['3. low']); //For calculating lowAverage
    const high = parseFloat(values['2. high']); // For calculating highAverage
    const volume = parseInt(values['5. volume'], 10); //For volume

    //Check if day exists. If not, initialize dictionary
    if (!GroupByDay[day]){
      GroupByDay[day] = {lowSum: 0, highSum: 0, volumeSum: 0, count: 0};
    }

    //Otherwise, accumulate running totals for a day
    GroupByDay[day].lowSum += low;
    GroupByDay[day].highSum += high;
    GroupByDay[day].volumeSum += volume;
    GroupByDay[day].count++; //needed for calculating running average
  }

  const result = Object.entries(GroupByDay).map(([day, total]) => ({
    day,
    lowAverage: parseFloat((total.lowSum / total.count).toFixed(4)),
    highAverage: parseFloat((total.highSum / total.count).toFixed(4)),
    volume: total.volumeSum
  }));

  //Return Result of translation
  res.json(result);


} //Otherwise catch any errors
catch (issue){
  console.error(issue);
  res.status(500).json({error: 'Server error fetching data'});
}
});


// Listen here
app.listen(PORT, () => {
  console.log(`Server running on http://localhost${PORT}`);
});
