# ThePensionsRegulator.Analyzers.Umbraco

Roslyn analyzers for Umbraco projects, published as a NuGet package.

## Installation

```shell
dotnet add package ThePensionsRegulator.Analyzers.Umbraco
```

No extra `.csproj` configuration is needed — NuGet automatically wires up the analyzer because the DLL is placed in `analyzers/dotnet/cs/` inside the package.

## Analyzers

| ID | Severity | Description |
|----|----------|-------------|
| [TPRUMB0001](docs/TPRUMB0001.md) | Warning | `.Value<T>()` called without `IPublishedValueFallback` |

### TPRUMB0001

Umbraco's `.Value<T>()` extension method has an overload that accepts `IPublishedValueFallback`. Always use this overload to ensure correct fallback behaviour and testability.

```csharp
// ❌ Triggers TPRU0001
var x = page.Value<string>("alias");

// ✅ Correct
var x = page.Value<string>(_publishedValueFallback, "alias");
```

See [docs/TPRUMB0001.md](docs/TPRUMB0001.md)

## Configuring Severity

Adjust the severity per-project via `.editorconfig`:

```ini
# Treat as an error
dotnet_diagnostic.TPRUMB0001.severity = error

# Or demote to a suggestion
dotnet_diagnostic.TPRUMB0001.severity = suggestion
```

## Building

```shell
dotnet build
dotnet test
dotnet pack --configuration Release
```

## License

MIT
