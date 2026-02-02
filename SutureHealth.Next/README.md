Suture Next Gen App
Runbook:
development:

#### Initial Run Only

- add the following to your .env.local

```
NEXT_PUBLIC_WEBHOST=ci.suturehealth.com
NEXT_PUBLIC_WEBPORT=443
```

- add "app.dev.suturehealth.com" as a host in your hosts file (mac) - _only if you are running the dotnet app locally_
- add "ci.suturehealth.com" as a host in your hosts file (mac)
- run `yarn ssl:setup` to create a local development certificate for use by the nextjs application

#### Daily Cookie Authentication

- Sign into AWS VPN
- navigate to "https://app.ci.suturesign.com" and sign in with dev credentials. (puts auth cookie on your machine)
