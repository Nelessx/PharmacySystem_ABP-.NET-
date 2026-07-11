# PharmacySystem

A **pharmacy management system** for a retail/community pharmacy, built as a
layered [Domain-Driven Design](https://abp.io/docs/latest/framework/architecture/domain-driven-design)
solution on the [ABP Framework](https://abp.io) (.NET 10) with an Angular 20 front end.

## Features

| Module | Capabilities |
| --- | --- |
| **Categories** | Group medicines (e.g. Antibiotics, Analgesics). |
| **Medicines** | Catalogue with generic name, unit, barcode, purchase/sale price, reorder level. |
| **Suppliers** | Supplier master data. |
| **Customers** | Customer/patient records for non-walk-in sales. |
| **Purchases** | Record supplier purchases; each purchase item increases batch stock. |
| **Sales** | Record sales; each sale item deducts the exact medicine + batch + expiry lot. |
| **POS** | Fast point-of-sale screen with FEFO batch selection, cart, discount, and a thermal (80 mm) receipt / PDF. |
| **Stock** | Per-lot on-hand quantities, low-stock and expiring-stock views. |
| **Dashboard** | KPIs plus sales/purchase trend, top-selling, stock-by-category and expiry-timeline charts. |

Access is controlled by ABP permissions. Besides `admin`, the DbMigrator seeds
three ready-to-use roles: **Manager** (full access), **Pharmacist** (dispensing +
day-to-day catalogue/stock) and **Cashier** (POS + customer registration).

## Architecture

Standard ABP layered monolith:

* `PharmacySystem.Domain` / `.Domain.Shared` — entities (Sale, Purchase, Stock, Medicine, …), the `StockManager` domain service, permissions and role seeding.
* `PharmacySystem.Application` / `.Application.Contracts` — application services, DTOs and permission definitions.
* `PharmacySystem.EntityFrameworkCore` — EF Core (PostgreSQL) mappings and migrations.
* `PharmacySystem.HttpApi` / `.HttpApi.Host` — auto-generated REST API and its host (also the OpenIddict auth server + Swagger).
* `PharmacySystem.DbMigrator` — console app that applies migrations and seeds data.
* `angular` — Angular 20 standalone-component SPA using the ABP `@abp/ng.*` packages and the LeptonX Lite theme.

### Pre-requirements

* [.NET 10.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node v18 or 20](https://nodejs.org/en) and [Yarn 1.x](https://classic.yarnpkg.com/)
* [PostgreSQL](https://www.postgresql.org/) (the default connection string targets a local instance)

## Running locally

1. **Provide secrets** — create the git-ignored `appsettings.secrets.json` files described under *Configurations* below (at minimum the `ConnectionStrings:Default` for both the host and the DbMigrator).
2. **Generate the signing certificate** — see *Generating a Signing Certificate* below (produces the git-ignored `openiddict.pfx`).
3. **Restore client libraries** — from the solution root run `abp install-libs`.
4. **Create & seed the database** — run the `PharmacySystem.DbMigrator` project (`dotnet run --project src/PharmacySystem.DbMigrator`). This applies migrations and seeds the admin user and pharmacy roles.
5. **Start the API host** — `dotnet run --project src/PharmacySystem.HttpApi.Host` (defaults to `https://localhost:44378`, Swagger at `/swagger`).
6. **Start the Angular app** — `cd angular && yarn && yarn start` (serves at `http://localhost:4200`).

Default admin credentials are `admin` / `1q2w3E*` — **change these before any real use.**

### Configurations

Secrets are **not** committed to source control. The tracked `appsettings.json` files ship with empty placeholders for every sensitive value; provide real values through a local, git-ignored `appsettings.secrets.json` (already wired up via `AddAppSettingsSecretsJson()`) or environment variables / user-secrets.

Create `src/PharmacySystem.HttpApi.Host/appsettings.secrets.json` (and a matching one under `PharmacySystem.DbMigrator`) with your local values:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=PharmacySystem;User ID=postgres;Password=<your-db-password>;"
  },
  "AuthServer": {
    "CertificatePassPhrase": "<your-certificate-password>"
  },
  "StringEncryption": {
    "DefaultPassPhrase": "<your-32-char-encryption-passphrase>"
  }
}
```

> **Security note:** The DB password, OpenIddict certificate passphrase and `StringEncryption` passphrase were previously committed in plaintext. They have been removed from tracked files, but they still exist in git history — **rotate all three (and the seeded admin password) before deploying.**

### Before running the application

* Run `abp install-libs` command on your solution folder to install client-side package dependencies. This step is automatically done when you create a new solution, if you didn't especially disabled it. However, you should run it yourself if you have first cloned this solution from your source control, or added a new client-side package dependency to your solution.
* Run `PharmacySystem.DbMigrator` to create the initial database. This step is also automatically done when you create a new solution, if you didn't especially disabled it. This should be done in the first run. It is also needed if a new database migration is added to the solution later.

#### Generating a Signing Certificate

In the production environment, you need to use a production signing certificate. ABP Framework sets up signing and encryption certificates in your application and expects an `openiddict.pfx` file in your application.

To generate a signing certificate, you can use the following command:

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p <your-certificate-password>
```

> Replace `<your-certificate-password>` with a strong password of your choice and store it in `appsettings.secrets.json` under `AuthServer:CertificatePassPhrase` (never commit it). The generated `openiddict.pfx` is git-ignored.

It is recommended to use **two** RSA certificates, distinct from the certificate(s) used for HTTPS: one for encryption, one for signing.

For more information, please refer to: [OpenIddict Certificate Configuration](https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html#registering-a-certificate-recommended-for-production-ready-scenarios)

> Also, see the [Configuring OpenIddict](https://abp.io/docs/latest/Deployment/Configuring-OpenIddict#production-environment) documentation for more information.

## Deploying the application

Deploying an ABP application follows the same process as deploying any .NET or ASP.NET Core application. However, there are important considerations to keep in mind. For detailed guidance, refer to ABP's [deployment documentation](https://abp.io/docs/latest/Deployment/Index).

### Additional resources


#### Internal Resources

You can find detailed setup and configuration guide(s) for your solution below:

* [Angular](./angular/README.md)

#### External Resources
You can see the following resources to learn more about your solution and the ABP Framework:

* [Web Application Development Tutorial](https://abp.io/docs/latest/tutorials/book-store/part-1)
* [Application Startup Template](https://abp.io/docs/latest/startup-templates/application/index)
