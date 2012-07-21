using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    public enum TokenKind
    {
        None,     
        MethodCall,
        ObjectCreation
    }

    /// <summary>
    /// Idea copied from http://www.matlus.com/c-to-html-syntax-highlighter-using-roslyn/
    /// </summary>
    public class CSharpCodeNavigationSyntaxWalker : SyntaxWalker, IDotNetSourceCodeNavigationSyntaxWalker
    {
        private Action<TokenKind, string, string, int?> writeDelegate;
        private SemanticModel semanticModel;

        public void DoVisit(ISemanticModel semanticModel, Action<TokenKind, string, string, int?> writeDelegate)
        {
            this.semanticModel = semanticModel as SemanticModel;
            var syntaxRootNode = this.semanticModel.SyntaxTree.GetRoot();

            this.writeDelegate = writeDelegate;
            Visit(syntaxRootNode);
        }

        public override void VisitToken(SyntaxToken token)
        {
            VisitLeadingTrivia(token);
            bool isProcessed = false;

            if (token.IsKeyword())
            {
                writeDelegate(TokenKind.None, token.GetText(), token.GetText(), null);
                isProcessed = true;
            }
            else
            {
                switch (token.Kind)
                {
                    case SyntaxKind.IdentifierToken:
                        TokenKind tokenKind = this.GetTokenKind(token);
                        if (tokenKind == TokenKind.None)
                        {
                            writeDelegate(TokenKind.None,token.GetText(), token.GetText(), token.Span.Start);
                        }
                        else
                        {
                            string fullyQualifiedNamed = this.GetFullyQualifiedName(tokenKind, token);
                            writeDelegate(tokenKind, fullyQualifiedNamed, token.GetText(), token.Span.Start);
                        }
                        isProcessed = true;
                        break;
                    default:
                        writeDelegate(TokenKind.None,token.GetText(), token.GetText(), null);
                        isProcessed = true;
                        break;
                }
            }

            if (!isProcessed)
            {
                writeDelegate(TokenKind.None,token.GetText(), token.GetText(), null);
            }
            base.VisitTrailingTrivia(token);
        }

        private string GetFullyQualifiedName(TokenKind tokenKind, SyntaxToken token)
        {            
            string result = token.GetText();
            SymbolInfo symbolInfo;

            switch (tokenKind)
            {                
                case TokenKind.MethodCall:

                    var identifierSyntax = token.Parent as IdentifierNameSyntax;
                    symbolInfo = this.semanticModel.GetSymbolInfo(identifierSyntax);
                    if (symbolInfo.Symbol != null)
                    {
                        result = symbolInfo.Symbol.ToString();
                    }                    
                    break;
                case TokenKind.ObjectCreation:

                    var objectCreationSyntax = token.Parent.Parent as ObjectCreationExpressionSyntax;
                    symbolInfo = this.semanticModel.GetSymbolInfo(objectCreationSyntax);
                    if (symbolInfo.Symbol != null)
                    {
                        result = symbolInfo.Symbol.ToString();
                    }
                    break;
                case TokenKind.None:                    
                default:
                    break;
            }

            return result;
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
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.DisabledTextTrivia:
                case SyntaxKind.DocumentationComment:
                case SyntaxKind.RegionDirective:
                case SyntaxKind.EndRegionDirective:
                default:
                    writeDelegate(TokenKind.None,trivia.GetFullText(),  trivia.GetFullText(), null);
                    break;
            }
            base.VisitTrivia(trivia);
        }
    }
}