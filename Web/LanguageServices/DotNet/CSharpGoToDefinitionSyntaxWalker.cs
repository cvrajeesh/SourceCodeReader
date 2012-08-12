using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using SourceCodeReader.Web.Models;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    public class CSharpGoToDefinitionSyntaxWalker : SyntaxWalker
    {

        private Action<int> symbolFoundDelegate;
        private TokenParameter parameter;
        private TokenKind searchTokenKind;
        private SemanticModel semanticModel;

        internal void DoVisit(ISemanticModel semanticModel, CommonSyntaxNode token, TokenParameter parameter, TokenKind searchTokenKind, Action<int> symbolFoundDelegate)
        {
            this.semanticModel = semanticModel as SemanticModel;
            this.symbolFoundDelegate = symbolFoundDelegate;
            this.parameter = parameter;
            this.searchTokenKind = searchTokenKind;
            Visit(token as SyntaxNode);
        }

        public override void VisitToken(SyntaxToken token)
        {
            if (token.GetText() == parameter.Text)
            {
                switch (searchTokenKind)
                {             
                    case TokenKind.MethodCall:
                        var methodDeclarationSyntax = token.Parent as MethodDeclarationSyntax;
                        var methodSymbol = this.semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
                        if (methodSymbol.ToString() == parameter.FullyQualifiedName)
                        {
                            symbolFoundDelegate(token.SyntaxTree.GetLineSpan(token.Span, false).StartLinePosition.Line);
                        }
                        break;
                    case TokenKind.ObjectCreation:
                        var classDeclarationSyntax = token.Parent as ClassDeclarationSyntax;
                        var classSymbol = this.semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                        if (classSymbol.ToString() == parameter.FullyQualifiedName)
                        {
                            symbolFoundDelegate(token.SyntaxTree.GetLineSpan(token.Span, false).StartLinePosition.Line);
                        }
                        break;               
                }

            }
            base.VisitToken(token);
        }
    }
}