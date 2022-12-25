
using System;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace RazorTextTemplating.PreProcessor
{
    internal sealed class RazorTextTemplateDocumentClassifierPass : DocumentClassifierPassBase
    {
        protected override string DocumentKind => "RazorTextTemplate";

        // We do not expect caller to process anything other than a file intended as TextTemplate.
        protected override bool IsMatch(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode) => true;

        protected override void OnDocumentStructureCreated(RazorCodeDocument codeDocument, NamespaceDeclarationIntermediateNode namespaceNode, ClassDeclarationIntermediateNode classNode,MethodDeclarationIntermediateNode methodNode)
        {
            if (null == codeDocument) throw new ArgumentNullException(nameof(codeDocument));
            if (null == namespaceNode) throw new ArgumentNullException(nameof(namespaceNode));
            if (null == classNode) throw new ArgumentNullException(nameof(classNode));
            if (null == methodNode) throw new ArgumentNullException(nameof(methodNode));

            base.OnDocumentStructureCreated(codeDocument, namespaceNode, classNode, methodNode);

            // Do not touch (namespaceNode|classNode|methodNode).Annotations 
            // The annotations are required to find the primary-namespace, primary-class and primary-method

            // Supports @namespace directive
            // TryComputeNamespace() will discover the @namespace directive value.
            namespaceNode.Content = codeDocument.TryComputeNamespace(fallbackToRootNamespace: false, out var namespaceName) ? namespaceName : "Templates";

            // Generated class is internal and partial.
            // TODO: ClassName is NOT sanitized.
            // Example: internal partial class <ClassName>
            classNode.ClassName = Path.GetFileNameWithoutExtension(codeDocument.Source.FilePath);
            classNode.Modifiers.Clear();
            classNode.Modifiers.Add("internal");
            classNode.Modifiers.Add("partial");

            // Generated method is private.
            // Our template will wrap the generated method with additional infra.
            // Example: private async Task ExecuteAsync()
            methodNode.MethodName = "ExecuteAsync";
            methodNode.Modifiers.Clear();
            methodNode.Modifiers.Add("private");
            methodNode.Modifiers.Add("async");
            methodNode.ReturnType = $"{typeof(System.Threading.Tasks.Task).FullName}";
        }
    }
}

