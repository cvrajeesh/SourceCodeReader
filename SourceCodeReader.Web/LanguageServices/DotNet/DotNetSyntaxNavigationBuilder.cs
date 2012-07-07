using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    public class DotNetSyntaxNavigationBuilder : ISyntaxNavigationBuilder
    {
        public string GetCodeAsNavigatableHtml(string sourceCode, IDotNetSourceCodeNavigationSyntaxWalker syntaxWalker)
        {
            var htmlBuilder = new StringBuilder();
            syntaxWalker.DoVisit(sourceCode, (tk, text, start) =>
            {
                switch (tk)
                {         
                    case TokenKind.ObjectCreation:
                    case TokenKind.MethodCall:
                        htmlBuilder.Append(string.Format(@"<a href=""javascript:$.findReferences('{0}', '{1}', {2})"">{1}</a>", tk, text, start.GetValueOrDefault()));
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