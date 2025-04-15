# Market Place Windows Desktop App

## Database Initialization

Use file [MarketPlaceQuery.sql](MarketPlace924/DBConnection/MarketPlaceQuery.sql) to initialize/reset the DB Schema.

## External Configation

The external configuration is read from a `appsettings.json` file located in the root folder of the .sln file, a.k.a. the root folder in the Solution Explorer on VS.

Create this file and add inside the Database connection string.

```json
{
    "ConnectionStrings": {
      "MyLocalDb": "Data Source=server-name;Initial Catalog=MarketPlaceDB;Integrated Security=True;TrustServerCertificate=True"
    }
}
```

OBS: Change the 'server-name' field in the json above to your server name from SSMS(SQL Server Management Studio)
Ex: DESKTOP-...