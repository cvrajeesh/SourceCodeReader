using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            AddDeclaredItem(symbol, node.Span.Start);

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
                        Location = node.Span.Start,
                        Identifier = string.Format("{0}.{1}()", symbol.ToDisplayString(), symbol.Name),
                        Type = "Constructor"
                    });
                }
            }

            base.VisitClassDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            this.declaredItems.Add(new DeclaredItemDocument
            {
                Name = symbol.Name,
                Location = node.Span.Start,
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
                    Location = node.Span.Start,
                    Identifier = symbol.ToDisplayString(),
                    Type = "Method"

                });
            }
            base.VisitMethodDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            AddDeclaredItem(symbol, node.Span.Start);
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            AddDeclaredItem(symbol, node.Span.Start);
            base.VisitEnumDeclaration(node);
        }

        public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            this.declaredItems.Add(new DeclaredItemDocument
            {
                Name = symbol.Name,
                Location = node.Span.Start,
                Identifier = symbol.ToDisplayString(),
                Type = "EnumMember"

            });
            base.VisitEnumMemberDeclaration(node);
        }

        private void AddDeclaredItem(NamedTypeSymbol symbol, int location)
        {
            this.declaredItems.Add(new DeclaredItemDocument
            {
                Name = symbol.Name,
                Location = location,
                Identifier = symbol.ToDisplayString(),
                Type = symbol.TypeKind.ToString()
            });
        }
    }
}