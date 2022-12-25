
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Razor.Language;

namespace RazorTextTemplating.PreProcessor
{
    /// <summary>
    /// Imports content of .imports.cshtml file(s) to all templates.
    /// </summary>
    internal sealed class SharedImportsProjectFeature : RazorProjectEngineFeatureBase, IImportProjectFeature
    {
        // Our shared import file name
        const string SharedImportsFileName = ".imports.cshtml";

        public IReadOnlyList<RazorProjectItem> GetImports(RazorProjectItem projectItem)
        {
            if (null == projectItem) throw new ArgumentNullException(nameof(projectItem));

            var importFiles = ProjectEngine
                .FileSystem
                .FindHierarchicalItems(projectItem.FilePath, SharedImportsFileName)
                .Reverse()
                .ToList();

            return importFiles; 
        }
    }
}
