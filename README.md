# BlazorEcommerce

This project requires several runtime secrets that are no longer stored in the repository. Provide them using environment variables or your own `appsettings.json`.

Required configuration keys:

- `ConnectionStrings:DefaultConnection` – database connection string
- `AppSettings:Token` – JWT signing key used by the API
- `Stripe:SecretKey` – Stripe API secret key
- `Stripe:WebhookSecret` – Stripe webhook signing secret

Environment variables can be used with double underscore notation, e.g. `Stripe__SecretKey`.
