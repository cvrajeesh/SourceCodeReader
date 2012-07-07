
namespace SourceCodeReader.Web.LanguageServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IFindReferenceProgress
    {
        void OnFindReferenceStarted();

        void OnFindReferenceInProgress();

        void OnFindReferenceCompleted(int searchResultCount);
    }
}
