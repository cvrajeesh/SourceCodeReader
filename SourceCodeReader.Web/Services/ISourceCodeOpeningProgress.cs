
namespace SourceCodeReader.Web.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface ISourceCodeOpeningProgress
    { 

        void OnFindProjectStarted();

        void OnProjectFound();

        void OnProjectNotFound();

        void OnProjectDownloadStarted();        

        void OnProjectDownloadCompleted();

        void OnProjectDownloadFailed();

        void OnProjectLoaded();

        void OnProjectPreparing();

        void OnProjectLoadingError();
    }
}
