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
        PropertyDeclaration,
        ObjectCreation,
        Parameter,
        MemberAccess,
        VariableDeclaration
    }

    /// <summary>
    /// Idea copied from http://www.matlus.com/c-to-html-syntax-highlighter-using-roslyn/
    /// </summary>
    public class CSharpCodeNavigationSyntaxWalker : SyntaxWalker, IDotNetSourceCodeNavigationSyntaxWalker
    {
        private Action<TokenKind, string, string> writeDelegate;
        private SemanticModel semanticModel;

        public void DoVisit(ISemanticModel semanticModel, Action<TokenKind, string, string> writeDelegate)
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
                writeDelegate(TokenKind.None, token.GetText(), null);
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
                            writeDelegate(TokenKind.None, token.GetText(), null);
                        }
                        else
                        {
                            try
                            {
                                string fullyQualifiedNamed = this.GetFullyQualifiedName(tokenKind, token);
                                writeDelegate(tokenKind, token.GetText(), fullyQualifiedNamed);
                            }
                            catch (Exception)
                            {
                                writeDelegate(TokenKind.None, token.GetText(),null);
                            }
                        }
                        isProcessed = true;
                        break;
                    default:
                        writeDelegate(TokenKind.None,token.GetText(), null);
                        isProcessed = true;
                        break;
                }
            }

            if (!isProcessed)
            {
                writeDelegate(TokenKind.None,token.GetText(), null);
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
                case TokenKind.PropertyDeclaration:
                case TokenKind.Parameter:
                case TokenKind.VariableDeclaration:
                case TokenKind.MemberAccess:
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
            // customer = new Customer();
            if (IsObjectCreationExpression(token))
            {
                return TokenKind.ObjectCreation;
            }

            // Customer customer;
            if (IsVariableDeclartionExpression(token))
            {
                return TokenKind.VariableDeclaration;
            }

            // string result = customer.GetFulleName();
            if (IsMethodCallExpression(token))
            {
                return TokenKind.MethodCall;
            }

            // public ITest Test { get; set; }
            if (IsPropertyDeclarationExpression(token))
            {
                return TokenKind.PropertyDeclaration;
            }

            // public void Method(ITest test)
            if (IsMethodParameterExpression(token))
            {
                return TokenKind.Parameter;
            }

            //// customer.Name
            //if (IsMemberAccessExpression(token))
            //{
            //    return TokenKind.MemberAccess;
            //}

            return TokenKind.None;
        }

        private static bool IsVariableDeclartionExpression(SyntaxToken token)
        {
            return token.Kind == SyntaxKind.IdentifierToken
                && token.Parent.Kind == SyntaxKind.IdentifierName
                && token.Parent.Parent.Kind == SyntaxKind.VariableDeclaration;

        }

        private static bool IsMemberAccessExpression(SyntaxToken token)
        {
            return token.Kind == SyntaxKind.IdentifierToken
              && token.Parent.Kind == SyntaxKind.IdentifierName
              && token.Parent.Parent.Kind == SyntaxKind.MemberAccessExpression;
        }

        private static bool IsMethodParameterExpression(SyntaxToken token)
        {
            return token.Kind == SyntaxKind.IdentifierToken
                            && token.Parent.Kind != SyntaxKind.PredefinedType
                            && token.Parent.Parent.Kind == SyntaxKind.Parameter;
        }

        private static bool IsPropertyDeclarationExpression(SyntaxToken token)
        {
            return token.Kind == SyntaxKind.IdentifierToken
                            && token.Parent.Parent.Kind == SyntaxKind.PropertyDeclaration;
        }

        private static bool IsMethodCallExpression(SyntaxToken token)
        {
            return (token.Parent.Parent.Kind == SyntaxKind.MemberAccessExpression
                            && token.Parent.Parent.Parent.Kind == SyntaxKind.InvocationExpression
                            && token.GetPreviousToken().Kind == SyntaxKind.DotToken) ||
                        (token.Parent.Parent.Kind == SyntaxKind.InvocationExpression);
        }

        private static bool IsObjectCreationExpression(SyntaxToken token)
        {
            return token.Parent.Parent.Kind == SyntaxKind.ObjectCreationExpression;
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
                    writeDelegate(TokenKind.None,trivia.GetFullText(), null);
                    break;
            }
            base.VisitTrivia(trivia);
        }
    }
}