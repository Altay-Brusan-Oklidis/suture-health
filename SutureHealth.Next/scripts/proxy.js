const express = require('express');
const { createProxyMiddleware } = require('http-proxy-middleware');

const app = express();

app.use(
  '/',
  createProxyMiddleware({
    target: process.env.NEXT_PUBLIC_PROXY_WEBHOST,
    changeOrigin: true,
  })
);
app.listen(3001);
