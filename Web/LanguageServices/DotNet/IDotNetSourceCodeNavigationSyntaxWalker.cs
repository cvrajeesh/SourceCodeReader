namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
using Roslyn.Compilers.Common;

    public interface IDotNetSourceCodeNavigationSyntaxWalker
    {
        void DoVisit(ISemanticModel semanticModel, Action<TokenKind, string, string> writeDelegate);
    }
}
