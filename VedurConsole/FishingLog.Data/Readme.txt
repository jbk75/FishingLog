

Við viljum scaffolda allt fyrir entitiframework

If you have an ASP.NET Core project, you can use the Name=<connection-string> syntax to read the connection string from configuration.

This works well with the Secret Manager tool to keep your database password separate from your codebase.

Dæmi
dotnet user-secrets set ConnectionStrings:Chinook "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Chinook"
dotnet ef dbcontext scaffold Name=ConnectionStrings:FishingLog Microsoft.EntityFrameworkCore.SqlServer
