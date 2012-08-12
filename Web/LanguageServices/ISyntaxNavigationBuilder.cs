using System;
using Roslyn.Compilers.Common;
using SourceCodeReader.Web.LanguageServices.DotNet;

namespace SourceCodeReader.Web.LanguageServices
{
    public interface ISyntaxNavigationBuilder
    {
        string GetCodeAsNavigatableHtml(ISemanticModel semanticModel, IDotNetSourceCodeNavigationSyntaxWalker syntaxWalker);
    }
}
