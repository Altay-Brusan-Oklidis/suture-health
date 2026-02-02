/**
 * @type {import('next').NextConfig}
 * */
const path = require('path');
const { version, dependencies } = require('./package.json');

module.exports = () => {
  const apiUrl = process.env.DOCKER
    ? 'REPLACE_ME_API_BASE'
    : process.env.NEXT_PUBLIC_WEBHOST || 'ci.suturehealth.com';

  return {
    reactStrictMode: true,
    output: 'standalone',
    serverRuntimeConfig: {
      apiUrl,
    },
    experimental: {
      swcPlugins:
        process.env.NODE_ENV === 'production'
          ? []
          : [['swc-plugin-coverage-instrument', {}]],
    },
    async redirects() {
      return [
        {
          source: '/identity/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/identity/:slug*',
          permanent: false,
        },
        {
          source: '/identity/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination: 'https://app.ci.suture.health/identity/:slug*',
          permanent: false,
        },
        {
          source: '/identity/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/identity/:slug*',
          permanent: false,
        },
        {
          source: '/identity/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/identity/:slug*',
          permanent: false,
        },
        {
          source: '/identity/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/identity/:slug*',
          permanent: false,
        },
        {
          source: '/identity/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/identity/:slug*',
          permanent: false,
        },
        {
          source: '/identity/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/identity/:slug*',
          permanent: false,
        },
        {
          source: '/organization/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/organization/:slug*',
          permanent: false,
        },
        {
          source: '/organization/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination: 'https://app.ci.suture.health/organization/:slug*',
          permanent: false,
        },
        {
          source: '/organization/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/organization/:slug*',
          permanent: false,
        },
        {
          source: '/organization/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/organization/:slug*',
          permanent: false,
        },
        {
          source: '/organization/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/organization/:slug*',
          permanent: false,
        },
        {
          source: '/organization/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/organization/:slug*',
          permanent: false,
        },
        {
          source: '/organization/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/organization/:slug*',
          permanent: false,
        },
        {
          source: '/member/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/member/:slug*',
          permanent: false,
        },
        {
          source: '/member/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination: 'https://app.ci.suture.health/member/:slug*',
          permanent: false,
        },
        {
          source: '/member/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/member/:slug*',
          permanent: false,
        },
        {
          source: '/member/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/member/:slug*',
          permanent: false,
        },
        {
          source: '/member/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/member/:slug*',
          permanent: false,
        },
        {
          source: '/member/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/member/:slug*',
          permanent: false,
        },
        {
          source: '/member/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/member/:slug*',
          permanent: false,
        },
        {
          source: '/account/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/account/:slug*',
          permanent: false,
        },
        {
          source: '/account/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health.com',
            },
          ],
          destination: 'https://app.ci.suture.health/account/:slug*',
          permanent: false,
        },
        {
          source: '/account/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/account/:slug*',
          permanent: false,
        },
        {
          source: '/account/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/account/:slug*',
          permanent: false,
        },
        {
          source: '/account/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/account/:slug*',
          permanent: false,
        },
        {
          source: '/account/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/account/:slug*',
          permanent: false,
        },
        {
          source: '/account/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/account/:slug*',
          permanent: false,
        },
        {
          source: '/request/history-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/request/history/:slug*',
          permanent: false,
        },
        {
          source: '/request/history-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination: 'https://app.ci.suture.health/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/request/history-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/request/history-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/request/history-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/request/history-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/request/history-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/network-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/network/:slug*',
          permanent: false,
        },
        {
          source: '/network-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination: 'https://app.ci.suture.health/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/network-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/network-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/network-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/network-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/network-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/visit-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/visit/:slug*',
          permanent: false,
        },
        {
          source: '/visit-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination: 'https://app.ci.suture.health/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/visit-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/visit-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/visit-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/visit-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/visit-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/revenue-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/revenue-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination: 'https://app.ci.suture.health/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/revenue-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/revenue-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/revenue-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/revenue-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/revenue-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/revenue/:slug*',
          permanent: false,
        },
        {
          source: '/request/send-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/request/send/:slug*',
          permanent: false,
        },
        {
          source: '/request/send-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination: 'https://app.ci.suture.health/request/send/:slug*',
          permanent: false,
        },
        {
          source: '/request/send-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/request/send/:slug*',
          permanent: false,
        },
        {
          source: '/request/send-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/request/send/:slug*',
          permanent: false,
        },
        {
          source: '/request/send-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/request/send/:slug*',
          permanent: false,
        },
        {
          source: '/request/send-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/request/send/:slug*',
          permanent: false,
        },
        {
          source: '/request/send-iframe/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/request/send/:slug*',
          permanent: false,
        },
      ];
    },
    async rewrites() {
      return [
        {
          source: '/api/:slug*',
          has: [
            {
              type: 'host',
              value: 'localhost',
            },
          ],
          destination: 'http://localhost:3001/api/v1.0/:slug*',
        },
        {
          source: '/api/:slug*',
          has: [
            {
              type: 'host',
              value: 'next.dev.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/api/v1.0/:slug*',
        },
        {
          source: '/api/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination: 'https://app.ci.suturehealth.com/api/v1.0/:slug*',
        },
        {
          source: '/api/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination: 'https://app.ci.suture.health/api/v1.0/:slug*',
        },
        {
          source: '/api/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination: 'https://app.qa.suturehealth.com/api/v1.0/:slug*',
        },
        {
          source: '/api/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination: 'https://app.qa.suture.health/api/v1.0/:slug*',
        },
        {
          source: '/api/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination: 'https://app.stage.suturehealth.com/api/v1.0/:slug*',
        },
        {
          source: '/api/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination: 'https://app.demo.suturehealth.com/api/v1.0/:slug*',
        },
        {
          source: '/api/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination: 'https://app.suturesign.com/api/v1.0/:slug*',
        },
        {
          source: '/patients/:slug*',
          has: [
            {
              type: 'host',
              value: 'next.dev.suturehealth.com',
            },
          ],
          destination:
            'https://app.ci.suturehealth.com/patients/:slug*?api-version=1.0',
        },
        {
          source: '/patients/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suturehealth.com',
            },
          ],
          destination:
            'https://app.ci.suturehealth.com/patients/:slug*?api-version=1.0',
        },
        {
          source: '/patients/:slug*',
          has: [
            {
              type: 'host',
              value: 'ci.suture.health',
            },
          ],
          destination:
            'https://app.ci.suture.health/patients/:slug*?api-version=1.0',
        },
        {
          source: '/patients/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suturehealth.com',
            },
          ],
          destination:
            'https://app.qa.suturehealth.com/patients/:slug*?api-version=1.0',
        },
        {
          source: '/patients/:slug*',
          has: [
            {
              type: 'host',
              value: 'qa.suture.health',
            },
          ],
          destination:
            'https://app.qa.suture.health/patients/:slug*?api-version=1.0',
        },
        {
          source: '/patients/:slug*',
          has: [
            {
              type: 'host',
              value: 'stage.suturehealth.com',
            },
          ],
          destination:
            'https://app.stage.suturehealth.com/patients/:slug*?api-version=1.0',
        },
        {
          source: '/patients/:slug*',
          has: [
            {
              type: 'host',
              value: 'demo.suturehealth.com',
            },
          ],
          destination:
            'https://app.demo.suturehealth.com/patients/:slug*?api-version=1.0',
        },
        {
          source: '/patients/:slug*',
          has: [
            {
              type: 'host',
              value: 'new.suturesign.com',
            },
          ],
          destination:
            'https://app.suturesign.com/patients/:slug*?api-version=1.0',
        },
      ];
    },
    webpack: (config) => {
      config.resolve.alias = {
        ...config.resolve.alias,
        react: path.resolve('./node_modules/react'),
        'react-dom': path.resolve('./node_modules/react-dom'),
        '@chakra-ui/*': path.resolve('./node_modules/@chakra-ui/*'),
        '@emotion/react': path.resolve('./node_modules/@emotion/react'),
        '@emotion/styled': path.resolve('./node_modules/@emotion/styled'),
        '@framer-motion': path.resolve('./node_modules/@framer-motion'),
      };

      return config;
    },
    publicRuntimeConfig: {
      version,
      themeVersion: dependencies['suture-theme'],
    },
  };
};
