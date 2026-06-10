# Postman

Import:

- `PhanMemQuanLyCongNo.postman_collection.json`
- `PhanMemQuanLyCongNo.local.postman_environment.json`

Start the API:

```powershell
dotnet run --project PhanMemQuanLyCongNo.Api/PhanMemQuanLyCongNo.Api.csproj
```

Run the collection folders in their existing order. The login requests save JWTs
for each seeded role, while list/create requests save IDs used by later requests.

Seeded accounts use password `123456`:

- `admin@demo.vn`
- `operator@demo.vn`
- `field@demo.vn`
- `customer@demo.vn`

`GET /api/tenants` requires a `SuperAdmin` token, but the current seed data does
not create a SuperAdmin account. Set `superAdminToken` manually after adding one.

The Application project contains request models for creating/updating contracts,
tenants, and notifications, but the current API controllers do not expose those
endpoints, so they are intentionally not included in the collection.
