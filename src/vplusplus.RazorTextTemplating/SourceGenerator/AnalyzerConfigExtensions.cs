
using System;
using Microsoft.CodeAnalysis.Diagnostics;

namespace vplusplus.RazorTextTemplating.SourceGenerator
{
    internal static partial class AnalyzerConfigExtensions
    {
        internal static string TryGetBuildProperty(this AnalyzerConfigOptions options, string buildPropertyName, string defaultValue = null)
        {
            options.TryGetValue($"build_property.{buildPropertyName}", out var something);
            return string.IsNullOrEmpty(something) ? defaultValue : something;
        }
    }
}
