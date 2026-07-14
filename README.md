# DataTechnologies Team 5 CLI

Console-applicatie (.NET 8) voor het beheren van gebruikers, producten, bestellingen en adressen, met PostgreSQL, MongoDB, Neo4j en Redis als datastores.

## Vereisten

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Toegang tot de gedeelde PostgreSQL-, MongoDB- en Neo4j-instanties van het team (connectiegegevens via je teamlead of `.env`)
- Optioneel: Visual Studio 2022 / VS Code met de C# extensie

## Installatie

1. Clone de repository en open de map:
   ```bash
   git clone <repo-url>
   cd DataTechnologies-Team5-4
   ```
2. Plaats het `.env`-bestand in `src/DataTechnologies/.env`.

   Het bestand bevat de volgende variabelen:

   | Variabele | Omschrijving |
   |---|---|
   | `PII_ENCRYPTION_KEY` | 32-byte base64 sleutel waarmee persoonsgegevens (e-mail, naam, adres) versleuteld worden opgeslagen. |
   | `PII_INDEX_KEY` | 32-byte base64 sleutel voor de "blind index" waarmee versleutelde velden (zoals e-mail) toch doorzoekbaar zijn zonder ze te ontsleutelen. |
   | `MONGODB_URI` | Volledige connectiestring (inclusief gebruiker/wachtwoord) naar de MongoDB-instantie. |
   | `NEO4J_URI` | Adres van de Neo4j-graafdatabase. |
   | `NEO4J_USER` | Gebruikersnaam voor Neo4j. |
   | `NEO4J_PASSWORD` | Wachtwoord voor Neo4j. |
   | `NEO4J_BROWSER` | URL naar de Neo4j Browser (webinterface om de graafdatabase te inspecteren). |
   | `POSTGRESQL_HOST` | Hostnaam/IP van de PostgreSQL-server. |
   | `POSTGRESQL_PORT` | Poort waarop PostgreSQL luistert. |
   | `POSTGRESQL_USERNAME` | Gebruikersnaam voor PostgreSQL. |
   | `POSTGRESQL_PASSWORD` | Wachtwoord voor PostgreSQL. |
   | `PAPERTRAIL` | Bearer token voor Papertrail logging dashboard. |
   
4. Herstel de dependencies:
   ```bash
   dotnet restore
   ```

## Applicatie starten

```bash
dotnet run --project src/DataTechnologies/DataTechnologies-Team5.csproj
```

Bij de eerste keer draaien:
1. Kies **Initialize Database Tables** om het schema aan te maken.
2. Kies optioneel **Insert Test Data** om voorbeeldgebruikers, producten en bestellingen te seeden.
3. Log in of registreer een nieuwe gebruiker via het hoofdmenu.

## Logs bekijken (Papertrail)

De applicatie stuurt real-time logs door naar Papertrail via de token die is geconfigureerd in de `.env`-bestand.
Volg de onderstaande stappen om dit op te zetten en de logs live te bekijken:

1. Log in op je **Papertrail-account** en klik in het menu op **Destinations**.
2. Klik op **Create Destination** en daarna op **Create**.
3. Selecteer in het nieuwe menu de optie **HTTPS**.
4. Kopieer de gegenereerde token en plak deze in je `.env`-bestand achter `PAPERTRAIL`.
5. Start de applicatie. Wanneer het programma draait, komen de logs binnen onder het menu **Logs** (het kan een paar seconden duren voordat deze updaten).

## Tests uitvoeren

```bash
dotnet test
```

Dit draait alle testprojecten (`DataTechnologies.User.Tests`, `DataTechnologies.Product.Tests`, `DataTechnologies.Address.Tests`, `DataTechnologies.Validation.Tests`).

## Input-validatie (whitelisting)

Gebruikersinvoer (e-mail, naam, wachtwoord) wordt gevalideerd via [Utils/InputWhitelistValidator.cs](src/DataTechnologies/Utils/InputWhitelistValidator.cs) om SQL-injectie, script/markup-injectie en lege/ongeldige invoer te weren:

- **E-mail** — max. 254 tekens, moet een geldig `naam@domein.tld`-formaat hebben.
- **Naam** — max. 35 tekens, alleen letters (inclusief accenten), spaties, koppeltekens en apostroffen.
- **Wachtwoord** — 8-64 tekens, minstens één hoofdletter, kleine letter, cijfer én speciaal teken, geen spaties of controletekens.

Lege, alleen-witruimte of `null`-invoer wordt altijd afgewezen. Anti-SQL-injectie opbouw.

## Belangrijk: het veld `EmailVerified`

Op het `User`-model ([Models/User.cs](src/DataTechnologies/Models/User.cs)) staat een boolean-veld `EmailVerified`. Ondanks de naam wordt dit veld in dit project gebruikt als benoeming voor **beheerdersrang**: `EmailVerified = true` betekent dat een gebruiker als administrator wordt beschouwd, `EmailVerified = false` betekent een gewone (niet-admin) gebruiker.

## Projectstructuur

- `Models/` — datamodellen (User, Product, Order, Address, ...)
- `Repositories/` — databasetoegang (PostgreSQL, met SQL-queries)
- `Services/` — businesslogica (UserService, ProductBrowseService, CryptoService, ...)
- `UI/` — Spectre.Console menu's en schermen
- `Utils/` — validatie- en hulpfuncties
- `Data/` — connectiebeheer voor PostgreSQL/MongoDB/Neo4j/Redis
