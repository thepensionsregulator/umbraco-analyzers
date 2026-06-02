using Xunit;

namespace ThePensionsRegulator.Analyzers.Umbraco.Tests;

public class PublishedValueFallbackAnalyzerTests
{
    [Fact]
    public async Task Reports_Diagnostic_When_IPublishedValueFallback_Is_Missing()
    {
        var source = UmbracoStubs.WithUsing("""
            class C
            {
                void M(IPublishedContent page)
                {
                    var x = {|TPRUMB0001:page.Value<string>("alias")|};
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task No_Diagnostic_When_IPublishedValueFallback_Is_Provided()
    {
        var source = UmbracoStubs.WithUsing("""
            class C
            {
                void M(IPublishedContent page, IPublishedValueFallback fallback)
                {
                    var x = page.Value<string>(fallback, "alias");
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task Reports_Diagnostic_When_GetCropUrl_IPublishedValueFallback_Is_Missing()
    {
        var source = UmbracoStubs.WithUsing("""
            class C
            {
                void M(IPublishedContent page)
                {
                    var x = {|TPRUMB0001:page.GetCropUrl("image")|};
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task No_Diagnostic_When_GetCropUrl_IPublishedValueFallback_Is_Provided()
    {
        var source = UmbracoStubs.WithUsing("""
            class C
            {
                void M(IPublishedContent page, IPublishedValueFallback fallback)
                {
                    var x = page.GetCropUrl(fallback, "image");
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task No_Diagnostic_When_No_Fallback_Overload_Exists()
    {
        // If a .Value<T>() method has no sibling overload with IPublishedValueFallback,
        // the analyzer should stay silent — it is not an Umbraco Value call.
        var source = """
            class OtherType
            {
                public T? Value<T>(string alias) => default;
            }
            class C
            {
                void M(OtherType obj)
                {
                    var x = obj.Value<string>("alias");
                }
            }
            """;

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task Reports_Diagnostic_On_IPublishedElement_Not_Just_IPublishedContent()
    {
        var source = UmbracoStubs.WithUsing("""
            class C
            {
                void M(IPublishedElement element)
                {
                    var x = {|TPRUMB0001:element.Value<int>("alias")|};
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task Reports_Diagnostic_When_Using_Null_Conditional_Operator()
    {
        var source = UmbracoStubs.WithUsing("""
            class C
            {
                void M(IPublishedContent? page)
                {
                    var x = page?{|TPRUMB0001:.Value<string>("alias")|};
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task Reports_Diagnostic_When_Fallback_Overload_Is_In_A_Different_Static_Class()
    {
        // Mirrors real Umbraco where the overloads are split across extension classes
        var source = UmbracoStubs.WithUsingSplit("""
            class C
            {
                void M(IPublishedContent page)
                {
                    var x = {|TPRUMB0001:page.Value<string>("alias")|};
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task Reports_Diagnostic_When_GetCropUrl_Fallback_Overload_Is_In_A_Different_Static_Class()
    {
        // Mirrors real Umbraco where GetCropUrl overloads are split across extension classes
        var source = UmbracoStubs.WithUsingSplit("""
            class C
            {
                void M(IPublishedContent page)
                {
                    var x = {|TPRUMB0001:page.GetCropUrl("image")|};
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task Reports_Diagnostic_On_Type_That_Inherits_From_IPublishedElement()
    {
        // A custom interface/class that inherits from IPublishedElement
        // e.g. IInheritFromIPublishedElement : IPublishedElement
        var source = UmbracoStubs.WithUsing("""
            namespace Custom
            {
                using Umbraco.Cms.Core.Models.PublishedContent;

                public interface IInheritFromIPublishedElement : IPublishedElement { }

                class C
                {
                    void M(IInheritFromIPublishedElement element)
                    {
                        var x = {|TPRUMB0001:element.Value<string>("alias")|};
                    }
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task Reports_Diagnostic_When_Value_Is_An_Instance_Method_On_Derived_Type()
    {
        // Mirrors IInheritFromIPublishedElement which defines its own Value<T> instance method
        var source = UmbracoStubs.WithUsingInstanceMethod("""
            class C
            {
                void M(IInheritFromIPublishedElement element)
                {
                    var x = {|TPRUMB0001:element.Value<string>("alias")|};
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }

    [Fact]
    public async Task No_Diagnostic_When_Instance_Method_Uses_IPublishedValueFallback()
    {
        var source = UmbracoStubs.WithUsingInstanceMethod("""
            class C
            {
                void M(IInheritFromIPublishedElement element, IPublishedValueFallback fallback)
                {
                    var x = element.Value<string>(fallback, "alias");
                }
            }
            """);

        await AnalyzerHelper.VerifyAsync(source);
    }
}
