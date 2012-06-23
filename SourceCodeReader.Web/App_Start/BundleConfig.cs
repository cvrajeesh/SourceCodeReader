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
            bundles.Add(new ScriptBundle("~/bundles/jquery")
               .Include("~/Scripts/jquery-1.*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval")
                .Include("~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                .Include("~/Scripts/bootstrap*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr")
                .Include("~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/spa")
                .Include("~/Scripts/knockout-2.*",
                        "~/Scripts/sammy-latest.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/applicationjs")
                .Include("~/Scripts/jsuri-1.1.1.min.js",
                        "~/Scripts/App/Application.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr")
                .Include("~/Scripts/jquery.signalR-0.5*"));
        }

        private static void RegisterStyleBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/bootrap")
               .Include("~/Content/bootstrap*",
               "~/Content/bootstrap-responsive*"));

            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/site.css"));
        }
    }
}