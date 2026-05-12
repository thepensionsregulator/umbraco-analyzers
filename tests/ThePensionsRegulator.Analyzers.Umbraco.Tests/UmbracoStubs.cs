namespace ThePensionsRegulator.Analyzers.Umbraco.Tests;

// Stub types that stand in for Umbraco's real types — no Umbraco NuGet dependency needed in tests
internal static class UmbracoStubs
{
    // Both overloads in the same static class
    public const string Source = """
        namespace Umbraco.Cms.Core.Models.PublishedContent
        {
            public interface IPublishedValueFallback { }
            public interface IPublishedElement { }
            public interface IPublishedContent : IPublishedElement { }

            public static class PublishedElementExtensions
            {
                public static T? Value<T>(this IPublishedElement content, string alias) => default;
                public static T? Value<T>(this IPublishedElement content, IPublishedValueFallback fallback, string alias) => default;
            }
        }
        """;

    // Overloads split across two static classes, matching real Umbraco's structure
    public const string SplitSource = """
        namespace Umbraco.Cms.Core.Models.PublishedContent
        {
            public interface IPublishedValueFallback { }
            public interface IPublishedElement { }
            public interface IPublishedContent : IPublishedElement { }

            public static class PublishedElementExtensions
            {
                public static T? Value<T>(this IPublishedElement content, string alias) => default;
            }

            public static class PublishedElementFallbackExtensions
            {
                public static T? Value<T>(this IPublishedElement content, IPublishedValueFallback fallback, string alias) => default;
            }
        }
        """;

    public static string WithUsing(string testCode) =>
        "using Umbraco.Cms.Core.Models.PublishedContent;\n" + Source + testCode;

    public static string WithUsingSplit(string testCode) =>
        "using Umbraco.Cms.Core.Models.PublishedContent;\n" + SplitSource + testCode;

    // Stub for a type that wraps IPublishedElement and exposes its own Value<T> instance method
    // (mirrors IOverridablePublishedElement / OverridablePublishedElement)
    public const string InstanceMethodSource = """
        namespace Umbraco.Cms.Core.Models.PublishedContent
        {
            public interface IPublishedValueFallback { }
            public interface IPublishedElement { }
            public interface IPublishedContent : IPublishedElement { }
        }
        namespace Custom
        {
            using Umbraco.Cms.Core.Models.PublishedContent;

            public interface IInheritFromIPublishedElement : IPublishedElement
            {
                T? Value<T>(string alias);
                T? Value<T>(IPublishedValueFallback fallback, string alias);
            }
        }
        """;

    public static string WithUsingInstanceMethod(string testCode) =>
        "using Umbraco.Cms.Core.Models.PublishedContent;\nusing Custom;\n" + InstanceMethodSource + testCode;
}
