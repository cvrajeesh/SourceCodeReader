namespace SourceCodeReader.Web.LanguageServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SourceCodeReader.Web.Models;

    public interface IEditorService
    {
        string BuildNavigatableSourceCodeFromFile(string filename);

        List<FindReferenceResult> FindRefernces(FindReferenceParameter parameter, IFindReferenceProgress findReferenceProgressListener);
    }
}
