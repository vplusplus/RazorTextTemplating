
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System;

namespace RazorTextTemplating.PreProcessor
{
    internal sealed class InjectTemplateMethodsPass : IntermediateNodePassBase, IRazorDirectiveClassifierPass
    {
        public override int Order => 5;

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            if (null == codeDocument) throw new ArgumentNullException(nameof(codeDocument));
            if (null == documentNode) throw new ArgumentNullException(nameof(documentNode));

            // Prepare a node that represents our code-fragment.
            var csCodeIntermediateNode = new CSharpCodeIntermediateNode();
            csCodeIntermediateNode.Children.Add(new IntermediateToken()
            {
                Kind = TokenKind.CSharp,
                Content = MyTemplatePropertiesAndMethods
            });

            // Add our code-fragment to the generated class.
            documentNode
                ?.FindPrimaryClass()
                ?.Children
                ?.Add(csCodeIntermediateNode);
        }

        const string MyTemplatePropertiesAndMethods = @"

        //...............................................................................
        #region RenderAsync() and private methods supporting template generation.
        //...............................................................................

        // A buffer to collect generated text fragments
        private readonly System.Text.StringBuilder __buffer__ = new System.Text.StringBuilder(1024);

        // Appends generated text fragments to the buffer.
        private void WriteLiteral(string something) => __buffer__.Append(something);

        // NOT intended for runtime HTML rendering. Write() doesn't encode.
        private void Write(object something) => __buffer__.Append(something);

        /// <summary />
        public async System.Threading.Tasks.Task<string> RenderAsync()
        {
            try
            {
                __buffer__.Clear();
                await ExecuteAsync().ConfigureAwait(false);
                return __buffer__.ToString();
            }
            finally
            {
                __buffer__.Clear();
            }
        }

        //...............................................................................
        #endregion

        ";
    }
}
