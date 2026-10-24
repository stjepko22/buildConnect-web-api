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

## Profilna i naslovna fotografija

- `POST /api/users/me/images/profile` i `/cover`: autorizirani multipart zahtjev s poljem `file`.
- `DELETE /api/users/me/images/profile` i `/cover`: uklanjanje vlastite fotografije.
- Oba zahtjeva vracaju cijeli vlasnikov profil, ukljucujuci privatni kontakt broj.
- Javna polja `profileImageUrl` i `coverImageUrl` su opcionalna; `null` znaci da fotografija nije postavljena.
- `GET /api/users/{id}/images/profile` i `/cover` javno vraca fotografiju. URL sadrzi verziju `?v=...` koja se mijenja pri zamjeni.

Prihvacaju se JPG/JPEG, PNG i WebP do 5 MiB po fotografiji. Provjerava se MIME tip,
potpis datoteke i stvarna velicina procitanog sadrzaja. SVG se ne prihvaca.
Fotografije su javni dio profila, a mijenjati ih moze samo vlasnik prijavljen JWT tokenom.

Sadrzaj fotografija sprema se u SQL tablicu `UserProfileImages`, odvojeno od `Users`.
Pretraga i direktorij ucitavaju samo verzije URL-ova, bez binarnog sadrzaja.
Svaka slika i njezina verzija na profilu spremaju se u jednoj transakciji.
Nije potrebna mapa za upload ni dodatni servis za datoteke; ukljucite tablicu u backup baze.
Prije koristenja treba primijeniti migraciju `AddUserProfileImages` (u developmentu pri ponovnom pokretanju API-ja).

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
