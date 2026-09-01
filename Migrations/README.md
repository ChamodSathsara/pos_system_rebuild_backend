# Migrations

Intentionally empty in this generated project. Run the following once you have NuGet/EF tooling
available locally:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
```

This will populate this folder with the generated migration + model snapshot based on
`Data/ApplicationDbContext.cs` and the configurations in `Data/Configurations/`.
