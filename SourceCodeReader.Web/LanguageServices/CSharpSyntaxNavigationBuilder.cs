using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System.Text;

namespace SourceCodeReader.Web.LanguageServices
{
    /// <summary>
    /// Idea copied from http://www.matlus.com/c-to-html-syntax-highlighter-using-roslyn/
    /// </summary>
    public class CSharpSyntaxNavigationBuilder
    {
        private readonly AssemblyFileReference mscorlib;

        public CSharpSyntaxNavigationBuilder()
        {
            this.mscorlib = new AssemblyFileReference(typeof(object).Assembly.Location);
        }

        public string GetCodeAsNavigatableHtml(string sourceCode)
        {
            var syntaxTree = SyntaxTree.ParseCompilationUnit(sourceCode);

            var compilation = Compilation.Create(
                outputName: "CSharpToHtmlSyntaxHighlighterCompilation",
                syntaxTrees: new[] { syntaxTree },
                references: new[] { mscorlib });

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var htmlColorizerSyntaxWalker = new CSharpSyntaxWalker();

            var htmlBuilder = new StringBuilder();
            htmlColorizerSyntaxWalker.DoVisit(syntaxTree.GetRoot(), semanticModel, (tk, text, start) =>
            {
                switch (tk)
                {         
                    case TokenKind.ObjectCreation:
                    case TokenKind.MethodCall:
                        htmlBuilder.Append(string.Format(@"<a href=""javascript:$.findReferences('{0}', '{1}', {2})"">{1}</a>", tk, text, start.GetValueOrDefault()));
                        break;
                    default:
                        htmlBuilder.Append(text);
                        break;
                }
            });

            return htmlBuilder.ToString();

        }
    }
}