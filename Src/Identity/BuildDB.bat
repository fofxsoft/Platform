dotnet ef migrations add Config -c ConfigurationData -o Data/Migrations/Configuration
dotnet ef migrations add Grants -c GrantData -o Data/Migrations/Grants
dotnet ef migrations add Users -c UserData -o Data/Migrations/Users

dotnet ef migrations script -c ConfigurationData -o Data/Scripts/Configuration.sql
dotnet ef migrations script -c GrantData -o Data/Scripts/Grants.sql
dotnet ef migrations script -c UserData -o Data/Scripts/Users.sql

dotnet ef database update -c ConfigurationData
dotnet ef database update -c GrantData
dotnet ef database update -c UserData

dotnet run /seed