using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Compilers.VisualBasic;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    public class VisualBasicCodeNavigationSyntaxWalker : SyntaxWalker, IDotNetSourceCodeNavigationSyntaxWalker
    {
        private Action<TokenKind, string, int?> writeDelegate;

        public void DoVisit(string sourceCode, Action<TokenKind, string, int?> writeDelegate)
        {
            var syntaxTree = SyntaxTree.ParseCompilationUnit(sourceCode);
            this.writeDelegate = writeDelegate;
            Visit(syntaxTree.GetRoot());
        }

        public override void VisitToken(SyntaxToken token)
        {
            VisitLeadingTrivia(token);
            bool isProcessed = false;

            switch (token.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    writeDelegate(this.GetTokenKind(token), token.GetText(), token.Span.Start);
                    isProcessed = true;
                    break;
                default:
                    writeDelegate(TokenKind.None, token.GetText(), null);
                    isProcessed = true;
                    break;
            }

            if (!isProcessed)
            {
                writeDelegate(TokenKind.None, token.GetText(), null);
            }
            base.VisitTrailingTrivia(token);
        }

        private TokenKind GetTokenKind(SyntaxToken token)
        {
            // Customer customer = new Customer();
            if (token.Parent.Parent.Kind == SyntaxKind.ObjectCreationExpression)
                return TokenKind.ObjectCreation;

            // string result = customer.GetFulleName();
            if ((token.Parent.Parent.Kind == SyntaxKind.MemberAccessExpression
                && token.Parent.Parent.Parent.Kind == SyntaxKind.InvocationExpression
                && token.GetPreviousToken().Kind == SyntaxKind.DotToken) ||
            (token.Parent.Parent.Kind == SyntaxKind.InvocationExpression))
                return TokenKind.MethodCall;

            return TokenKind.None;
        }

        // Handle SyntaxTrivia
        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.Kind)
            {
                case SyntaxKind.CommentTrivia:
                case SyntaxKind.DisabledTextTrivia:
                case SyntaxKind.DocumentationComment:
                case SyntaxKind.RegionDirective:
                case SyntaxKind.EndRegionDirective:
                default:
                    writeDelegate(TokenKind.None, trivia.GetFullText(), null);
                    break;
            }
            base.VisitTrivia(trivia);
        }
    }
}