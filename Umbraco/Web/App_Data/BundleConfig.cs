using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using BundleTransformer.Core.Transformers;


public class BundleConfig
{
    public static void RegisterBundles(BundleCollection bundles)
    {
        //Cdn
        bundles.UseCdn = true;
        var jqueryCdnPath = "http://code.jquery.com/jquery-1.8.2.js";
        var jqueryUICdnPath = "http://code.jquery.com/ui/1.10.0/jquery-ui.js";

        //Scripts
        bundles.Add(new ScriptBundle("~/scripts/jquery", jqueryCdnPath).Include("~/scripts/jquery.js"));

        bundles.Add(new ScriptBundle("~/scripts/jqueryui", jqueryUICdnPath).Include("~/scripts/jquery.ui.js"));

        bundles.Add(
            new ScriptBundle("~/scripts/all").Include(
                "~/scripts/bootstrap.js",
                "~/scripts/jquery.cycle.js",
                "~/scripts/knockout.js",
                "~/scripts/knockout.mapping.js",
                "~/scripts/site.js",
                "~/scripts/section"
                ));

        //bundles.Add(
        //    new ScriptBundle("~/scripts/jqueryvalidation").Include(
        //        "~/scripts/jquery.validate.unobtrusive.js",
        //        "~/scripts/jquery.validate.js",
        //        "~/scripts/jquery.validate.additional-methods.js",
        //        "~/scripts/jquery.validate.messages-es.js",
        //        "~/scripts/form.js"
        //        ));


        bundles.Add(new ScriptBundle("~/scripts/modernizr").Include("~/scripts/modernizr*"));


        bundles.Add(new ScriptBundle("~/scripts/jquery.jqgrid").Include(
                "~/scripts/jquery.jqgrid.js",
                "~/scripts/jquery.jqgrid.locale-es.js"
                ));

        //bundles.Add(new ScriptBundle("~/scripts/section").Include("~/scripts/section.js"));

        //Styles 
        
        bundles.Add(new StyleBundle("~/css/all").Include(
            "~/css/ui/jquery.ui.css",
            "~/css/bootstrap/bootstrap.css",
            "~/css/bootstrap/bootstrap-responsive.css",
            "~/css/site/site.css",
            "~/css/site/section.css"
            ));

        bundles.Add(new StyleBundle("~/css/jqgrid/jqgrid").Include("~/css/jqgrid/jqgrid.css"));

        //bundles.Add(new StyleBundle("~/css/section").Include("~/css/section.css"));

        ////Less
        //bundles.Add(
        //    new Bundle("~/css/less", new CssTransformer()).Include(
        //        "~/css/style.less",
        //        "~/css/custom.less",
        //        "~/css/fancybox.css"
        //        ));

        //BundleTable.EnableOptimizations = true;
    }
}