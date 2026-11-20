# Mada Finishing

The .NET 11 solution contains an ASP.NET Core MVC web project and an MSTest project.

## Build, test, and run

Install the .NET 11 SDK and run these commands from the repository root.

```powershell
dotnet build MadaFinishing.slnx
dotnet test MadaFinishing.slnx --no-build
dotnet run --project MadaFinishing.Web --launch-profile http --no-build
```

Open [the MVC starter page](http://localhost:5081/) and press Ctrl+C in the terminal to stop the app.

The starter unit test follows Arrange–Act–Assert. It creates a `HomeController`, calls `Index()`, and checks that the action returns a `ViewResult`. A failure means the home action no longer returns an MVC view. The test command above runs it.
