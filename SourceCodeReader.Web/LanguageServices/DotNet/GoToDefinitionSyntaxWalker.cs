using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Compilers.Common;
using SourceCodeReader.Web.Models;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    internal class GoToDefinitionSyntaxWalker : CommonSyntaxWalker
    {
        private Action<int> symbolFoundDelegate;
        private TokenParameter parameter;
        private TokenKind searchTokenKind;
        private ISemanticModel semanticModel;

        internal void DoVisit(ISemanticModel semanticModel, CommonSyntaxNode token, TokenParameter parameter, TokenKind searchTokenKind, Action<int> symbolFoundDelegate)
        {
            this.semanticModel = semanticModel;
            this.symbolFoundDelegate = symbolFoundDelegate;
            this.parameter = parameter;
            this.searchTokenKind = searchTokenKind;
            Visit(token);
        }

        protected override void VisitToken(CommonSyntaxToken token)
        {
            if (token.GetText() == parameter.Text)
            {
                var symbol = this.semanticModel.GetSymbolInfo(token.Parent);
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