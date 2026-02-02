module.exports = {
  ci: {
    collect: {
      puppeteerScript: './login-script.js',
      puppeteerLaunchOptions: {
        args: ['--no-sandbox', '--disable-setuid-sandbox', '--disable-gpu'],
        defaultViewport: null,
        DISPLAY: ':10.0',
        headless: true,
      },
      url: ['https://app.ci.suturesign.com/request/sign'],
      numberOfRuns: 3,
      headful: false,
      settings: {
        disableStorageReset: true,
        preset: 'desktop',
      },
    },
    upload: {
      target: 'lhci',
      serverBaseUrl: 'http://10.50.10.155:8080',
      token: '552b4c45-4f40-4d94-8225-53083d7fda7b',
    },
  },
};
