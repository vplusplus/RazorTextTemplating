
using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace RazorTextTemplating.PreProcessor
{
    internal sealed class ModelPropertyDirectivePass : IntermediateNodePassBase, IRazorDirectiveClassifierPass
    {
        // Razor directive: @model <typeName>
        internal static readonly DirectiveDescriptor ModelDirective = DirectiveDescriptor.CreateDirective(
            "model",
            DirectiveKind.SingleLine,
            builder => {
                builder.AddTypeToken();
                builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
                builder.Description = "Model Directive";
            });

        public override int Order => 5;  // After the @inherits directive.    

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            if (null == codeDocument) throw new ArgumentNullException(nameof(codeDocument));
            if (null == documentNode) throw new ArgumentNullException(nameof(documentNode));

            // Find @model directive.
            var modelDirectiveNode = documentNode
                .FindDirectiveReferences(ModelDirective)
                .Select(x => x.Node)
                .OfType<DirectiveIntermediateNode>()
                .SingleOrDefault();

            // The modelType if @model is defined...
            var modelType = modelDirectiveNode
                ?.Tokens
                ?.FirstOrDefault()
                ?.Content;

            if (null != modelType)
            {
                // Define model property: public <modelType> Model { get; set; }
                var modelPropertyNode = new PropertyDeclarationIntermediateNode()
                {
                    PropertyName = "Model",
                    PropertyType = modelType,
                };
                modelPropertyNode.Modifiers.Add("public");

                // Add Model property to the generated class.
                documentNode
                    ?.FindPrimaryClass()
                    ?.Children
                    ?.Insert(0, modelPropertyNode);
            }
        }
    }
}
