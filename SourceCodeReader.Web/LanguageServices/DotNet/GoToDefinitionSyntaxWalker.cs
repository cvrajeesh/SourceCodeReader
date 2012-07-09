using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Compilers.Common;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    internal class GoToDefinitionSyntaxWalker : CommonSyntaxWalker
    {
        private Action<int> symbolFoundDelegate;
        private string textToSearch;
        private TokenKind searchTokenKind;

        internal void DoVisit(CommonSyntaxNode token, string textToSearch, TokenKind searchTokenKind, Action<int> symbolFoundDelegate)
        {
            this.symbolFoundDelegate = symbolFoundDelegate;
            this.textToSearch = textToSearch;
            this.searchTokenKind = searchTokenKind;
            Visit(token);
        }

        protected override void VisitToken(CommonSyntaxToken token)
        {
            if (token.GetText() == textToSearch)
            {
                if ((searchTokenKind == TokenKind.MethodCall && token.Parent.Kind == (int)Roslyn.Compilers.CSharp.SyntaxKind.MethodDeclaration) ||
                    (searchTokenKind == TokenKind.ObjectCreation && token.Parent.Kind == (int)Roslyn.Compilers.CSharp.SyntaxKind.ClassDeclaration))
                {
                    symbolFoundDelegate(token.SyntaxTree.GetLineSpan(token.Span, false).StartLinePosition.Line);
                }
            }

            base.VisitToken(token);
        }
    }
}