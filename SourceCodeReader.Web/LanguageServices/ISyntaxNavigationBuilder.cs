using System;
using SourceCodeReader.Web.LanguageServices.DotNet;

namespace SourceCodeReader.Web.LanguageServices
{
    public interface ISyntaxNavigationBuilder
    {
        string GetCodeAsNavigatableHtml(string sourceCode, IDotNetSourceCodeNavigationSyntaxWalker syntaxWalker);
    }
}
