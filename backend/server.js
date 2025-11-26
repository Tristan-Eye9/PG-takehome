const express = require('express');
const app = express();
const PORT = 3001;
//Use environment variables for API key security
require('dotenv').config();
const apiKey = process.env.ALPHAVANTAGE_API_KEY;

// DELETE ME LATER
// Simple test route 
app.get('/', (req, res) => {
    res.json({ message: 'Express is working!' });
});

// Intraday API usage

// Intraday API Route for testing purposes
// Example usage: http://localhost:3001/api/intraday/IBM
app.get('/api/intraday/:symbol', async(req, res) => {
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

// Listen here
app.listen(PORT, () => {
  console.log(`Server running on http://localhost${PORT}`);
});
