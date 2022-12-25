
using System;
using Microsoft.AspNetCore.Razor.Language;

namespace vplusplus.RazorTextTemplating.SourceGenerator
{
    internal struct CodeGenOptions
    {
        public string MSBuildProjectDirectory { get; internal set; }
        public string RootNamespace { get; internal set; }
        public string ImportFilesThumbprint { get; internal set; }
    }

    internal struct CSOutput : IEquatable<CSOutput>
    {
        public string SourceHint { get; internal set; }
        public RazorCSharpDocument SourceDoc { get; internal set; }

        bool IEquatable<CSOutput>.Equals(CSOutput that) =>
            0 == this.SourceDoc?.Diagnostics?.Count &&
            0 == that.SourceDoc?.Diagnostics?.Count &&
            string.Equals(this.SourceDoc.GeneratedCode, that.SourceDoc.GeneratedCode);

        public override int GetHashCode() =>
            StringComparer.Ordinal.GetHashCode(this.SourceDoc);
    }
}
