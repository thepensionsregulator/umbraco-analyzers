# TPRUMB0001

## Summary

Umbraco's `.Value<T>()` and `.GetCropUrl()` methods have overloads with `IPublishedValueFallback`.
Always use the overload that accepts `IPublishedValueFallback`:

```csharp
// Without fallback — avoid this
page.Value<string>("alias");
page.GetCropUrl("image");

// With fallback — always use this
page.Value<string>(_publishedValueFallback, "alias");
page.GetCropUrl(_publishedValueFallback, "image");
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
        var image = CurrentPage!.GetCropUrl(_publishedValueFallback, "image");
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
