using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Compilers.CSharp;

namespace SourceCodeReader.Web.LanguageServices
{
    internal enum TokenKind
    {
        None,     
        MethodCall,
        ObjectCreation
    }

    /// <summary>
    /// Idea copied from http://www.matlus.com/c-to-html-syntax-highlighter-using-roslyn/
    /// </summary>
    internal class CSharpCodeNavigationSyntaxWalker : SyntaxWalker
    {
        private SemanticModel semanticModel;
        private Action<TokenKind, string, int?> writeDelegate;

        internal void DoVisit(SyntaxNode token, SemanticModel semanticModel, Action<TokenKind, string, int?> writeDelegate)
        {
            this.semanticModel = semanticModel;
            this.writeDelegate = writeDelegate;
            Visit(token);
        }

        public override void VisitToken(SyntaxToken token)
        {
            VisitLeadingTrivia(token);
            bool isProcessed = false;

            if (token.IsKeyword())
            {
                writeDelegate(TokenKind.None, token.GetText(), null);
                isProcessed = true;
            }
            else
            {
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
            if (token.Parent.Parent.Kind == SyntaxKind.MemberAccessExpression
                && token.Parent.Parent.Parent.Kind == SyntaxKind.InvocationExpression
                && token.GetPreviousToken().Kind == SyntaxKind.DotToken)
                return TokenKind.MethodCall;

            return TokenKind.None;
        }

        // Handle SyntaxTrivia
        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.Kind)
            {
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineCommentTrivia:
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