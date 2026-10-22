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

## Potvrda e-maila

Registracija stvara račun koji se može prijaviti nakon potvrde e-mail adrese.
Potvrdna poveznica vrijedi 24 sata, a korisnik može zatražiti novo slanje.
Ako slanje ne uspije, račun ostaje spremljen, a odgovor vraća
`isVerificationEmailSent: false`.

Resend API ključ ne spremajte u datoteke koje ulaze u Git. Za lokalni
development koristite .NET User Secrets (komanda iz korijena backend repozitorija):

```powershell
dotnet user-secrets set "Resend:ApiKey" "<RESEND_API_KEY>" --project src/BuildConnect.WebApi/BuildConnect.WebAPI/BuildConnect.WebAPI.csproj
```

User Secrets automatski se učitavaju u `Development` okruženju. Na produkciji
postavite varijablu okruženja `Resend__ApiKey`. Podesite i `Resend__From` na
dopuštenu adresu pošiljatelja te `Email__ClientBaseUrl` na javnu adresu web aplikacije.
Razvojna vrijednost te adrese je `http://localhost:3000`.

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
