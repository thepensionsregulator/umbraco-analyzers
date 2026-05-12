# TPRUMB0001

## Summary

Umbraco's `.Value<T>()` extension method has two overloads:

```csharp
// Without fallback — avoid this
page.Value<string>("alias");

// With fallback — always use this
page.Value<string>(_publishedValueFallback, "alias");
```

## Why

- Provides explicit control over how missing or null values are resolved
- Required for correct multi-language/culture content resolution
- Makes the call testable — Avoids the use of Umbraco's static service locator which is shared between tests leading to flaky test behaviour

## How to Fix

Inject `IPublishedValueFallback` via the constructor and pass it as the first argument:

```csharp
public class MyController(..., IPublishedValueFallback _publishedValueFallback) : RenderController(...)
{
    public IActionResult Index()
    {
        var value = CurrentPage!.Value<string>(_publishedValueFallback, "alias");
    }
}
```

## Suppression

If there is a genuine reason to suppress:

```csharp
#pragma warning disable TPRUMB0001
var x = page.Value<string>("alias");
#pragma warning restore TPRUMB0001
```

Or in `.editorconfig` to downgrade to a suggestion project-wide:

```ini
dotnet_diagnostic.TPRUMB0001.severity = suggestion
```
