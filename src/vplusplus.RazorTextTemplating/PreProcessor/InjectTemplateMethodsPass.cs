
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
        System.Text.StringBuilder __buffer__ = null;

        // Appends generated text fragments to the buffer.
        void WriteLiteral(string something) => __buffer__.Append(something);

        // WARNING: Write() doesn't encode the content.        
        // WARNING: NOT intended for runtime HTML rendering using user-input.
        void Write(object something) => __buffer__.Append(something);

        /// <summary />
        public async System.Threading.Tasks.Task<string> RenderAsync()
        {
            try
            {
                __buffer__ = new System.Text.StringBuilder(1024);
                await ExecuteAsync().ConfigureAwait(false);
                __buffer__ = __WS.Trim(__buffer__);
                return __buffer__.ToString();
            }
            finally
            {
                __buffer__ = null;
            }
        }

        //...............................................................................
        // Post processing whitespace directive and whitespace removal helper 
        //...............................................................................
        static class __WS
        {
            const char CR = '\r', LF = '\n', BackSlash = '\\', SUB = (char)0x1A, IGNORE = '!';

            static readonly System.Text.RegularExpressions.Regex RxWhitespaceDirective = new System.Text.RegularExpressions.Regex(@""\[[\-\+\*\!]{1,2}\]"", System.Text.RegularExpressions.RegexOptions.Compiled);

            public static System.Text.StringBuilder Trim(System.Text.StringBuilder buffer)
            {
                if (null == buffer) throw new ArgumentNullException(nameof(buffer));

                // Find whitespace directives.
                var matches = buffer.Length > 0 ? RxWhitespaceDirective.Matches(buffer.ToString()) : null;
                if (null == matches || 0 == matches.Count) return buffer;

                // Process each directive
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    MarkWhiteSpacesForRemoval(buffer, match.Index, match.Value);
                    MarkDirectiveForRemoval(buffer, match.Index, match.Length);
                }

                // Remove the unwanted whitespace markers.
                return RemoveCharacter(buffer, SUB);
            }

            static void MarkDirectiveForRemoval(System.Text.StringBuilder buf, int index, int length)
            {
                for (int i = index; i < index + length; i++) buf[i] = SUB;
            }

            static void MarkWhiteSpacesForRemoval(System.Text.StringBuilder buffer, int index, string directive)
            {
                var befDirective = directive.Length >= 3 ? directive[1] : IGNORE;
                var aftDirective = directive.Length == 4 ? directive[2] : IGNORE;

                if (IGNORE != befDirective) MarkWhiteSpaces(buffer, index - 1, befDirective, before: true);
                if (IGNORE != aftDirective) MarkWhiteSpaces(buffer, index + directive.Length, aftDirective, before: false);
            }

            static void MarkWhiteSpaces(System.Text.StringBuilder buffer, int index, char directive, bool before)
            {
                switch (directive)
                {
                    case '-': MarkWhiteSpacesInThisLine(buffer, index, before); break;
                    case '+': MarkWhiteSpacesAndBlankLines(buffer, index, before); break;
                    case '*': MarkWhiteSpacesAllLines(buffer, index, before); break;
                }
            }

            static void MarkWhiteSpacesInThisLine(System.Text.StringBuilder buffer, int index, bool before)
            {
                static bool IsWhiteSpaceButNotCRLF(char c) => CR != c && LF != c && char.IsWhiteSpace(c);

                while (index >= 0 && index < buffer.Length && IsWhiteSpaceButNotCRLF(buffer[index]))
                {
                    buffer[index] = SUB;
                    index = before ? index - 1 : index + 1;
                }
            }

            static void MarkWhiteSpacesAndBlankLines(System.Text.StringBuilder buffer, int index, bool before)
            {
                int cr = 0, lf = 0;

                while (index >= 0 && index < buffer.Length && char.IsWhiteSpace(buffer[index]))
                {
                    var c = buffer[index];

                    if (CR == c) cr++;
                    if (LF == c) lf++;

                    buffer[index] = SUB;
                    index = before ? index - 1 : index + 1;
                }

                if (before)
                {
                    if (cr > 0) buffer[++index] = CR;
                    if (lf > 0) buffer[++index] = LF;
                }
                else
                {
                    if (lf > 0) buffer[--index] = LF;
                    if (cr > 0) buffer[--index] = CR;
                }
            }

            static void MarkWhiteSpacesAllLines(System.Text.StringBuilder buffer, int index, bool before)
            {
                while (index >= 0 && index < buffer.Length && char.IsWhiteSpace(buffer[index]))
                {
                    buffer[index] = SUB;
                    index = before ? index - 1 : index + 1;
                }
            }

            static System.Text.StringBuilder RemoveCharacter(System.Text.StringBuilder dirty, char unwanted)
            {
                var clean = new System.Text.StringBuilder(dirty.Length);
                for (int i = 0; i < dirty.Length; i++) if (dirty[i] != unwanted) clean.Append(dirty[i]);
                return clean;
            }

        }

        //...............................................................................
        #endregion

        ";
    }
}
