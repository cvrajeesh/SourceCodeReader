namespace SourceCodeReader.Web.LanguageServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SourceCodeReader.Web.Models;

    public interface IEditorService
    {
        string BuildNavigatableSourceCodeFromFile(string username, string project, string path);

        List<TokenResult> FindRefernces(TokenParameter parameter, IFindReferenceProgress findReferenceProgressListener);

        TokenResult GoToDefinition(TokenParameter parameter, IFindReferenceProgress findReferenceProgressListener);
    }
}
