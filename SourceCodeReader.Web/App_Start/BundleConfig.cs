using System.Web;
using System.Web.Optimization;

namespace SourceCodeReader.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            RegisterScriptBundles(bundles);
            RegisterStyleBundles(bundles);           
        }

        private static void RegisterScriptBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/all")
                .Include("~/Scripts/jquery-1.7.2.js",
                "~/Scripts/jquery.cookie.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/knockout-2.1.0.js",
                "~/Scripts/sammy-latest.min.js",
                "~/Scripts/jsuri-1.1.1.min.js",
                "~/Scripts/jquery.signalR-0.5.1.js"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr")
                .Include("~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/applicationjs")
              .Include("~/Scripts/App/Application.js"));

        
        }

        private static void RegisterStyleBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/content/all")
            .Include("~/Content/bootstrap.css",
                "~/Content/bootstrap-responsive.css",
                "~/Content/site.css"));         
        }
    }
}