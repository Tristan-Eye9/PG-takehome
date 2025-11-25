const express = require('express');
const app = express();
const PORT = 3000;

// Simple test route
app.get('/', (req, res) => {
    res.json({ message: 'Express is working!' });
});

app.listen(PORT, () => {
  console.log(`Server running on httplocalhost${PORT}`);
});
