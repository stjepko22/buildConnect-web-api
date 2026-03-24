# buildConnect-web-api

.NET 8 Web API backend za BuildConnect aplikaciju.

## Tehnologije

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server LocalDB
- JWT autentikacija
- xUnit testovi

## Pokretanje

```powershell
cd src
dotnet run --project BuildConnect.WebApi\BuildConnect.WebAPI\BuildConnect.WebAPI.csproj
```

API adrese:

- `http://localhost:56602`
- `https://localhost:56601`

Health:

- `http://localhost:56602/api/health`

## Baza

Development connection string:

```text
Server=(localdb)\MSSQLLocalDB;Database=BuildConnectDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

U developmentu startup automatski:

- primijeni migracije
- odradi seed

To je definirano u:

- `src/BuildConnect.WebApi/BuildConnect.WebAPI/appsettings.Development.json`

Izvan developmenta je to po defaultu iskljuceno:

- `src/BuildConnect.WebApi/BuildConnect.WebAPI/appsettings.json`

## Demo login

- `investitor@buildconnect.hr` / `invest123`
- `izvodjac@buildconnect.hr` / `izvodjac123`

## Testovi

```powershell
cd src
dotnet test BuildConnect.WebAPI.sln -v minimal
```

Trenutno su pokriveni:

- service unit testovi
- integration testovi za `auth -> JWT -> protected endpoint`

## Napomena za migracije

Ako je API upaljen iz Visual Studija ili terminala, a povukao si nove DLL ili migration promjene:

1. ugasi backend
2. ponovno ga pokreni

U developmentu ce se migracije automatski primijeniti.
