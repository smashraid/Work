using System;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.WebHost;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Umbraco.Web;

public partial class Global : Umbraco.Web.UmbracoApplication
{
    protected override void OnApplicationStarted(object sender, EventArgs e)
    {

        //AreaRegistration.RegisterAllAreas();
        //ModelBinders.Binders.Add(typeof(JsonViewModel), new JsonModelBinder());
        WebApiConfig.Configure(GlobalConfiguration.Configuration);
        //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);

        //GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        base.OnApplicationStarted(sender, e);
    }



}