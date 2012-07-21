using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SourceCodeReader.Web.Infrastructure;
using System.Web.Http;
using System.Reflection;
using Ninject;

namespace SourceCodeReader.Web
{
    public static class BootStrapper
    {
        public static void Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            var kernal = new Ninject.StandardKernel();
            kernal.Load(new[] { Assembly.GetExecutingAssembly()});
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernal);


            var applicationConfigurationProvider = kernal.Get<IApplicationConfigurationProvider>();
            applicationConfigurationProvider.SourceCodeIndexPath.EnsureDirectoryExists();
        }
    }
}