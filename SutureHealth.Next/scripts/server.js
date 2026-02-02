const { createServer: createHttpsServer } = require('https');
const { createServer } = require('http');
const next = require('next');
const fs = require('fs');

const dev = process.env.NODE_ENV !== 'production';
const hostname =
  process.env.NODE_ENV === 'test'
    ? 'localhost'
    : process.env.HOST_NAME || 'ci.suturehealth.com';
const port = process.env.PORT_HTTPS || 3000;

const app = next({ dev, hostname, port });
const handle = app.getRequestHandler();

app
  .prepare()
  .then(() => {
    const server =
      process.env.NODE_ENV === 'test' || process.env.HOST_NAME === 'localhost'
        ? createServer((req, res) => handle(req, res))
        : createHttpsServer(
            {
              key: fs.readFileSync('./certs/devcert.key'),
              cert: fs.readFileSync('./certs/devcert.cert'),
            },
            (req, res) => handle(req, res)
          );

    return server.listen(port, (err) => {
      if (err) throw err;

      console.log(`> Ready on ${hostname}:${port}`);
    });
  })
  .catch((err) => {
    console.error(err);
  });
