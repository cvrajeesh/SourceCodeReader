using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SourceCodeReader.Web.Infrastructure;
using System.Web.Http;
using System.Reflection;

namespace SourceCodeReader.Web
{
    public static class BootStrapper
    {
        public static void Start()
        {
            var kernal = new Ninject.StandardKernel();
            kernal.Load(new[] { Assembly.GetExecutingAssembly()});
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernal);
        }
    }
}