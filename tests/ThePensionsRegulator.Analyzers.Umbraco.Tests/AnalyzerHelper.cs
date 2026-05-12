using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace ThePensionsRegulator.Analyzers.Umbraco.Tests;

internal static class AnalyzerHelper
{
    public static async Task VerifyAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<PublishedValueFallbackAnalyzer, DefaultVerifier>
        {
            TestCode = source,
        };
        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }
}
