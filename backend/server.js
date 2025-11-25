const express = require('express');
const fetch = require('node-fetch');
const app = express();
const PORT = 3001;

// DELETE ME LATER
// Simple test route 
app.get('/', (req, res) => {
    res.json({ message: 'Express is working!' });
});

// Intraday API usage
// TODO: Implement Intraday API route




// Listen here
app.listen(PORT, () => {
  console.log(`Server running on httplocalhost${PORT}`);
});
