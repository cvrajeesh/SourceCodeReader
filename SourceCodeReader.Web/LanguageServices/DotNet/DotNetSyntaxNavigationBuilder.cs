using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Roslyn.Compilers.Common;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    public class DotNetSyntaxNavigationBuilder : ISyntaxNavigationBuilder
    {
        public string GetCodeAsNavigatableHtml(ISemanticModel semanticModel, IDotNetSourceCodeNavigationSyntaxWalker syntaxWalker)
        {
            var htmlBuilder = new StringBuilder();
            syntaxWalker.DoVisit(semanticModel, (tk, text, fullyQualifiedName) =>
            {
                if (tk == TokenKind.None)
                {
                    htmlBuilder.Append(System.Web.HttpUtility.HtmlEncode(text));
                }
                else
                {
                    htmlBuilder.Append(string.Format(@"<a href=""javascript:$.goToDefinition('{0}')"">{1}</a>", fullyQualifiedName, text));
                }
            });

            return htmlBuilder.ToString();
        }
    }
}