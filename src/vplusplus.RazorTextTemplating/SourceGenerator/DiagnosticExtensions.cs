
using System;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace vplusplus.RazorTextTemplating.SourceGenerator
{
    internal static partial class DiagnosticExtensions
    {
        internal static void Log(this SourceProductionContext ctx, DiagnosticSeverity severity, string id, string message)
        {
            const string MyCategory = "RazorTextTemplate";

            var desc = new DiagnosticDescriptor(id, message, message, MyCategory, severity, isEnabledByDefault: true);
            var diag = Diagnostic.Create(desc, Location.None);
            ctx.ReportDiagnostic(diag);
        }

        internal static Diagnostic ToCodeAnalysisDiagnostic(this RazorDiagnostic razorDiagnostic)
        {
            return Diagnostic.Create(
                descriptor: new DiagnosticDescriptor(
                    razorDiagnostic.Id,
                    razorDiagnostic.GetMessage(),
                    razorDiagnostic.GetMessage(),
                    "RazorTextTemplate",
                    ToCodeAnalysisSeverity(razorDiagnostic.Severity),
                    isEnabledByDefault: true
                ),
                location: ToCodeAnalysisLocation(razorDiagnostic.Span)
            );

            static DiagnosticSeverity ToCodeAnalysisSeverity(RazorDiagnosticSeverity razorSeverity) => razorSeverity switch
            {
                RazorDiagnosticSeverity.Error => DiagnosticSeverity.Error,
                RazorDiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
                _ => DiagnosticSeverity.Hidden
            };

            static Location ToCodeAnalysisLocation(SourceSpan razorSpan)
            {
                if (razorSpan == SourceSpan.Undefined) return Location.None;

                var linePosition = new LinePositionSpan(
                    new LinePosition(razorSpan.LineIndex, razorSpan.CharacterIndex),
                    new LinePosition(razorSpan.LineIndex, razorSpan.CharacterIndex + razorSpan.Length)
                );

                return Location.Create
                (
                    razorSpan.FilePath,
                    new TextSpan(razorSpan.AbsoluteIndex, razorSpan.Length),
                    linePosition
                );
            }
        }
    }
}

