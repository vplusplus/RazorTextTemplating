
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;

namespace RazorTextTemplating.PreProcessor
{
    /// <summary>
    /// Can preprocess cshtml text template to standalone class with no external dependencies.
    /// </summary>
    public sealed class RazorTextTemplatePreProcessor
    {
        /// <summary />
        public static RazorCSharpDocument ParseTemplate(string projectFolder, string templateFileName, string rootNamespace)
        {
            if (null == projectFolder) throw new ArgumentNullException(nameof(projectFolder));
            if (null == templateFileName) throw new ArgumentNullException(nameof(templateFileName));

            // Using Razor Language 6.0, a predictable language version as against the 'latest' version.
            var myRazorConfiguration = RazorConfiguration.Create(RazorLanguageVersion.Version_6_0, "RazorTextTemplates", Array.Empty<RazorExtension>(), false);

            // Prepare a RazorProjectFileSystem on suggested base folder.
            var myProjectFileSystem = RazorProjectFileSystem.Create(projectFolder);
            var myProjectItem = myProjectFileSystem.GetItem(templateFileName);

            // Prepare RazorProjectEngine
            var myProjectEngine = RazorProjectEngine.Create(myRazorConfiguration, myProjectFileSystem, builder =>
            {
                builder.SetRootNamespace(rootNamespace ?? "Templates");

                builder.AddDirective(ModelPropertyDirectivePass.ModelDirective);

                builder.Features.Add(new SharedImportsProjectFeature());
                builder.Features.Add(new RazorTextTemplateDocumentClassifierPass());
                builder.Features.Add(new ModelPropertyDirectivePass());
                builder.Features.Add(new InjectTemplateMethodsPass());
                builder.Features.Add(new SuppressMetadataAttributesFeature());
            });

            return myProjectEngine
                .Process(myProjectItem)
                .GetCSharpDocument();
        }
    }
}
