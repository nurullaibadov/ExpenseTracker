Run these commands from the solution root to create and apply migrations:

dotnet ef migrations add InitialCreate \
  --project src/ExpenseTracker.Infrastructure \
  --startup-project src/ExpenseTracker.API

dotnet ef database update \
  --project src/ExpenseTracker.Infrastructure \
  --startup-project src/ExpenseTracker.API
