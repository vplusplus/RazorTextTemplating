
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RazorTextTemplating.PreProcessor;

namespace vplusplus.RazorTextTemplating.SourceGenerator
{
    [Generator]
    internal sealed class RazorTextTemplateSourceGenerator : IIncrementalGenerator
    {
        const string TemplateFileExtension = ".cshtml";
        const string ImportsFileName = ".imports.cshtml";

        static bool IsMyCshtmlFile(AdditionalText someFile) => true == someFile?.Path?.EndsWith(TemplateFileExtension, StringComparison.OrdinalIgnoreCase);
        static bool IsMyImportFile(AdditionalText someFile) => true == someFile?.Path?.EndsWith(ImportsFileName, StringComparison.OrdinalIgnoreCase);
        static bool IsMyTemplateFile(AdditionalText someFile) => IsMyCshtmlFile(someFile) && !IsMyImportFile(someFile);

        void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            // Input
            var analyzerConfigOptions = initContext.AnalyzerConfigOptionsProvider;
            var additionalFiles = initContext.AdditionalTextsProvider;

            // My inputs...
            var myImportFiles = additionalFiles.Where(IsMyImportFile);
            var myTemplateFiles = additionalFiles.Where(IsMyTemplateFile);

            // Prepare pipeline
            var importFilesThumbprint = myImportFiles.Collect().Select(ComputeImportFilesSignature);
            var myCodeGenOptions = analyzerConfigOptions.Combine(importFilesThumbprint).Select(PrepareCodeGenOptions);
            var csDocuments = myTemplateFiles.Combine(myCodeGenOptions).Select(ParseTemplates);

            // Contribue
            initContext.RegisterSourceOutput(csDocuments, ContributeSource);
        }

        // Generates a thumbprint of ./**/.import.cshtml files to invalidate caching if any of the the import files had changed.
        static string ComputeImportFilesSignature(ImmutableArray<AdditionalText> importFiles, CancellationToken ct)
        {
            var buffer = new StringBuilder();

            buffer.Append(importFiles.Length).AppendLine();

            for(int i=0; i<importFiles.Length; i++)
            {
                var file = importFiles[i];
                var fileInfo = File.Exists(file.Path) ? new FileInfo(file.Path) : null;

                if (null != fileInfo)
                {
                    buffer
                        .Append(fileInfo.LastAccessTimeUtc.Ticks)
                        .Append("|")
                        .Append(fileInfo.Length)
                        .Append("|")
                        .Append(fileInfo.FullName)
                        .AppendLine();
                }
            }

            return buffer.ToString();
        }

        // Prepare CodeGenOptions from build properties.
        static CodeGenOptions PrepareCodeGenOptions((AnalyzerConfigOptionsProvider, string) pair, CancellationToken cancellationToken)
        {
            var (provider, importFilesThumbprint) = pair;

            var projectDirectory = provider.GlobalOptions.TryGetBuildProperty("MSBuildProjectDirectory");
            var rootNamespace = provider.GlobalOptions.TryGetBuildProperty($"RootNamespace", "Templates");

            return new CodeGenOptions()
            {
                MSBuildProjectDirectory = projectDirectory,
                RootNamespace = rootNamespace,
                ImportFilesThumbprint = importFilesThumbprint
            };
        }

        // Translate razor-text-template to pre-processed source code.
        static CSOutput ParseTemplates((AdditionalText, CodeGenOptions) pair, CancellationToken cancellationToken)
        {
            var (additionalText, codeGenOptions) = pair;

            var projectFolder = codeGenOptions.MSBuildProjectDirectory;
            var templateFileName = additionalText.Path;
            var rootNamespace = codeGenOptions.RootNamespace;

            var sourceDoc = RazorTextTemplatePreProcessor.ParseTemplate(projectFolder, templateFileName, rootNamespace);
            var sourceHint = PrepareSourceHint(projectFolder, templateFileName);

            return new CSOutput()
            {
                SourceDoc = sourceDoc,
                SourceHint = sourceHint
            };

            static string PrepareSourceHint(string basePath, string fileName)
            {
                var relativePath = fileName.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) ? fileName.Substring(basePath.Length) : Path.GetFileName(fileName);

                var hint = Path.ChangeExtension(relativePath, ".g.cs")
                    .Replace('\\', '/')
                    .Trim('/', '.', ' ')
                    .Replace('/', '-');

                return hint;
            }
        }

        // Contribute Razor parsing diagnostics and generated source code.
        void ContributeSource(SourceProductionContext srcPrdContext, CSOutput csOutput)
        {
            // Inform parsing errors if any.
            var csDiag = csOutput.SourceDoc.Diagnostics;
            for (var i = 0; i < csDiag.Count; i++)
            {
                var razorDiagnostic = csDiag[i];
                var csharpDiagnostic = razorDiagnostic.ToCodeAnalysisDiagnostic();
                srcPrdContext.ReportDiagnostic(csharpDiagnostic);
            }

            // Contribute
            var sourceCode = csOutput.SourceDoc.GeneratedCode;
            var hint = csOutput.SourceHint;
            srcPrdContext.AddSource(hint, sourceCode);

            // Inform
            srcPrdContext.Log(DiagnosticSeverity.Warning, "RTXT-101", $"[{DateTime.Now:HHmmss}] Processed {hint}");
        }
    }
}
