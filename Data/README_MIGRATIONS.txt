To set up the database:
1. Open Package Manager Console or terminal in project directory
2. Run: dotnet ef migrations add InitialCreate
3. Run: dotnet ef database update
This will create the VehicleRentalFullDb database in SQL Server LocalDB.
