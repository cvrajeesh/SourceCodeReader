using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using SourceCodeReader.Web.Models;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    public class CSharpCodeIndexingSyntaxWalker : SyntaxWalker
    {
        private SemanticModel model;
        private IList<DeclaredItemDocument> declaredItems = null;

        public IList<DeclaredItemDocument> DoVisit(ISemanticModel model, CommonSyntaxNode node)
        {
            this.model = model as SemanticModel;
            this.declaredItems = new List<DeclaredItemDocument>();
            Visit(node as SyntaxNode);
            return this.declaredItems;
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            VisitDeclaration(symbol, node.Span);

            base.VisitStructDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            VisitDeclaration(symbol, node.Span);

            base.VisitClassDeclaration(node);
        }

        private void VisitDeclaration(NamedTypeSymbol symbol, TextSpan span)
        {
            AddDeclaredItem(symbol, span);

            var constructors = symbol.GetConstructors();
            if (constructors.Count() == 1)
            {
                // Check whether constructor is a default constructor is defined explicitily
                if (constructors.First().IsImplicitlyDeclared)
                {
                    // Add default constructor path
                    this.declaredItems.Add(new DeclaredItemDocument
                    {
                        Name = symbol.Name,
                        Location = this.model.SyntaxTree.GetLineSpan(span, false).StartLinePosition.Line,
                        Identifier = string.Format("{0}.{1}()", symbol.ToDisplayString(), symbol.Name),
                        Type = "Constructor"
                    });
                }
            }
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            this.declaredItems.Add(new DeclaredItemDocument
            {
                Name = symbol.Name,
                Location = this.model.SyntaxTree.GetLineSpan(node.Span, false).StartLinePosition.Line,
                Identifier = symbol.ToDisplayString(),
                Type = "Constructor"
            });
            base.VisitConstructorDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            if (symbol.ContainingType != null)
            {
                this.declaredItems.Add(new DeclaredItemDocument
                {
                    Name = symbol.Name,
                    Location = this.model.SyntaxTree.GetLineSpan(node.Span, false).StartLinePosition.Line,
                    Identifier = symbol.ToDisplayString(),
                    Type = "Method"

                });
            }
            base.VisitMethodDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            AddDeclaredItem(symbol, node);
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            AddDeclaredItem(symbol, node);
            base.VisitEnumDeclaration(node);
        }

        public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            this.declaredItems.Add(new DeclaredItemDocument
            {
                Name = symbol.Name,
                Location = this.model.SyntaxTree.GetLineSpan(node.Span, false).StartLinePosition.Line,
                Identifier = symbol.ToDisplayString(),
                Type = "EnumMember"

            });
            base.VisitEnumMemberDeclaration(node);
        }

        private void AddDeclaredItem(NamedTypeSymbol symbol, SyntaxNode node)
        {
            AddDeclaredItem(symbol, node.Span);
        }

        private void AddDeclaredItem(NamedTypeSymbol symbol, TextSpan span)
        {
            this.declaredItems.Add(new DeclaredItemDocument
            {
                Name = symbol.Name,
                Location = this.model.SyntaxTree.GetLineSpan(span, false).StartLinePosition.Line,
                Identifier = symbol.ToDisplayString(),
                Type = symbol.TypeKind.ToString()
            });
        }
    }
}