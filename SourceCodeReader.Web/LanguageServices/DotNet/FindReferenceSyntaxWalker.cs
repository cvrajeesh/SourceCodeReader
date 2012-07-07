using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.CSharp.Retargeting;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    internal class FindReferenceSyntaxWalker : CommonSyntaxWalker
    {
        private Action<int> symbolFoundDelegate;
        private string textToSearch;

        internal void DoVisit(CommonSyntaxNode token, string textToSearch, Action<int> symbolFoundDelegate)
        {
            this.symbolFoundDelegate = symbolFoundDelegate;
            this.textToSearch = textToSearch;
            Visit(token);
        }

        protected override void VisitToken(CommonSyntaxToken token)
        {
            if (token.GetText() == textToSearch)
            {
                symbolFoundDelegate(token.Span.Start);
            }
           
            base.VisitToken(token);
        }
    }
}