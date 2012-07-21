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
            syntaxWalker.DoVisit(semanticModel, (tk, fullyQualifiedName, text, start) =>
            {
                switch (tk)
                {         
                    case TokenKind.ObjectCreation:
                    case TokenKind.MethodCall:
                        htmlBuilder.Append(string.Format(@"<a href=""javascript:$.goToDefinition('{0}','{1}', '{2}', {3})"">{1}</a>", tk,  text, fullyQualifiedName, start.GetValueOrDefault()));
                        break;
                    default:
                        htmlBuilder.Append(System.Web.HttpUtility.HtmlEncode(text));
                        break;
                }
            });

            return htmlBuilder.ToString();
        }
    }
}