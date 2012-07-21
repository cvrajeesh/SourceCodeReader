using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject.Modules;
using SourceCodeReader.Web.Services;
using SourceCodeReader.Web.Services.GitHub;
using SourceCodeReader.Web.LanguageServices;
using SourceCodeReader.Web.LanguageServices.DotNet;
using Ninject;
using System.IO;


namespace SourceCodeReader.Web.Infrastructure
{
    public class ApplicationNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEditorService>().To<DotNetCodeEditorService>();

            Bind<DotNetSourceCodeSearchService>().ToSelf().InSingletonScope();
            Bind<ISourceCodeIndexingService>().ToMethod(context => (ISourceCodeIndexingService)context.Kernel.Get<DotNetSourceCodeSearchService>());
            Bind<ISourceCodeQueryService>().ToMethod(context => (ISourceCodeQueryService)context.Kernel.Get<DotNetSourceCodeSearchService>());
            
            Bind<IApplicationConfigurationProvider>().To<ApplicationConfigurationProvider>();
            Bind<IFindReferenceProgress>().To<DefaultFindReferenceProgressListener>();
            Bind<IProjectDiscoveryService>().To<GitHubProjectDiscoveryService>();
            Bind<ISourceCodeOpeningProgress>().To<DefaultSourceCodeOpeningProgressListener>();
            Bind<ISourceCodeProviderService>().To<GitHubSourceCodeProviderService>();
            Bind<Lucene.Net.Store.Directory>().ToMethod(context =>
                Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(context.Kernel.Get<IApplicationConfigurationProvider>().SourceCodeIndexPath)));
            Bind<Lucene.Net.Analysis.Analyzer>().To<Lucene.Net.Analysis.Standard.StandardAnalyzer>().WithConstructorArgument("matchVersion", Lucene.Net.Util.Version.LUCENE_29);
        }
    }
}