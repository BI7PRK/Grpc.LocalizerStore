## AspNetCore.Grpc.LocalizerStore
This is a simple library for storing and retrieving localized strings in an ASP.NET Core application using gRPC. It provides a way to manage localization resources in a centralized manner, making it easier to maintain and update localized strings across different parts of the application.

## Features
- Store localized strings in a gRPC service.
- Retrieve localized strings using gRPC calls.
- Supports multiple languages and cultures.
- Easy to integrate into existing ASP.NET Core applications.
- Supports dependency injection for easy configuration.
- Supports caching for improved performance.
- Automatic fallback to default culture if a localized string is not found.

## Installation
You can install the package via NuGet Package Manager Console:
```bash
Install-Package AspNetCore.Grpc.LocalizerStore
```
Or by adding the following line to your `.csproj` file:
```xml
<PackageReference Include="AspNetCore.Grpc.LocalizerStore" Version="1.0.0" />
```
Or by using the .NET CLI:
```bash
dotnet add package AspNetCore.Grpc.LocalizerStore
```
## Usage
```csharp
public void ConfigureServices(IServiceCollection services)
{
	// Add the gRPC Localizer Store service
	services.AddLocalizerStore(opt =>
	{
		opt.Url = "http://localhost:50001";
	});
}

// add the following code to configure the middleware:
app.UseRequestLocalizatioStore();
```

## gRPC Service
 Go to see: https://github.com/BI7PRK/go-i18n-grpc


