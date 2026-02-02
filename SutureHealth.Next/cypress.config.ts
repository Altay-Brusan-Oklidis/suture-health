import { defineConfig } from 'cypress';

export default defineConfig({
  projectId: 'ydxa3x',
  video: false,
  e2e: {
    baseUrl:
      process.env.NODE_ENV === 'test'
        ? 'http://localhost:3000'
        : 'https://next.dev.suturehealth.com:3000',
    viewportWidth: 1280,
    viewportHeight: 1024,
    setupNodeEvents(on, config) {
      /* eslint-disable global-require */
      require('@cypress/code-coverage/task')(on, config);

      return config;
    },
  },
  env: {
    codeCoverage: {
      exclude: [
        'styles.js',
        'src/utils/*',
        'src/formAdapters/**',
        'src/theme/**',
        'src/redux/*',
        'node_modules/**',
        'src/icons/*',
      ],
    },
  },
});
