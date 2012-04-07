using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Routing;
using MerchantTribe.Commerce;
using System.Web.Mvc;
using System.Web.Caching;
using log4net;
using MvcMiniProfiler;

namespace MerchantTribeStore
{
    public class Global : System.Web.HttpApplication
    {

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        #region " Scheduled Tasks "

        static void ScheduleTaskTrigger()
        {
            HttpRuntime.Cache.Add("ScheduledTaskTrigger",
                                  "42", // Magic string that really could be anything but 42 is nice.
                                  null, 
                                  Cache.NoAbsoluteExpiration, 
                                  TimeSpan.FromMinutes(2), 
                                  CacheItemPriority.NotRemovable, 
                                  new CacheItemRemovedCallback(PerformScheduledTasks));
        }        
        static void PerformScheduledTasks(string key, Object value, CacheItemRemovedReason reason)
        {
            try
            {
                MerchantTribe.Commerce.RequestContext context = new MerchantTribe.Commerce.RequestContext();
                MerchantTribeApplication.Current = MerchantTribe.Commerce.MerchantTribeApplication.InstantiateForDataBase(context);

                List<long> storeIds = MerchantTribeApplication.Current.ScheduleServices.QueuedTasks.ListStoresWithTasksToRun();
                if (storeIds != null)
                {
                    List<MerchantTribe.Commerce.Accounts.StoreDomainSnapshot> stores = MerchantTribeApplication.Current.AccountServices.Stores.FindDomainSnapshotsByIds(storeIds);
                    if (stores != null)
                    {
                        System.Threading.Tasks.Parallel.ForEach(stores, CallTasksOnStore);
                        //foreach (MerchantTribe.Commerce.Accounts.StoreDomainSnapshot snap in stores)
                        //{
                        //    string storekey = System.Configuration.ConfigurationManager.AppSettings["storekey"];
                        //    string rootUrl = snap.RootUrl();
                        //    string destination = rootUrl + "scheduledtasks/" + storekey;
                        //    MerchantTribe.Commerce.Utilities.WebForms.SendRequestByPost(destination, string.Empty);
                        //}
                    }
                }
                
            }
            catch
            {
                // suppress error and schedule next run
            }

            ScheduleTaskTrigger();
        }

        private static void CallTasksOnStore(MerchantTribe.Commerce.Accounts.StoreDomainSnapshot snap)
        {
            string storekey = System.Configuration.ConfigurationManager.AppSettings["storekey"];
            string rootUrl = snap.RootUrl();
            string destination = rootUrl + "scheduledtasks/" + storekey;
            MerchantTribe.Commerce.Utilities.WebForms.SendRequestByPost(destination, string.Empty);
        }

#endregion

        void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("bvc.js");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("images/{*imagedata}");
            //routes.IgnoreRoute("bvadmin/*");
            //routes.IgnoreRoute("{resource}.aspx/{*pathInfo}");
            
            // Admin Routes
            routes.MapPageRoute("adminlogin", "adminaccount/login", "~/BVAdmin/Login.aspx");
            routes.MapPageRoute("adminresetpassword", "adminaccount/resetpassword", "~/BVAdmin/ResetPassword.aspx");
            routes.MapPageRoute("adminresetpassword2", "adminaccount/resetpassword2", "~/BVAdmin/ResetPassword2.aspx");
            routes.MapPageRoute("adminlogout", "adminaccount/logout", "~/BVAdmin/logout.aspx");
                                                                                      
            // Products
            routes.MapRoute("products-rendersingle", "products/rendersingleproduct/{*params}",
                                new {controller = "Products",action = "RenderSingleProduct"});
            routes.MapRoute("products-validate", "products/validate/{*params}",
                                new { controller = "Products", action = "Validate" });

            // Product Reviews                      
            routes.MapRoute("productreview-route", "products/reviews/{slug}",
                                new { controller = "ProductReviews", action = "Index" });
            routes.MapRoute("productreviews", "productreviews/{action}/{id}",
                                new { controller = "ProductReviews", action = "index", id = "" });
                                      
            // policies
            routes.MapRoute("policy-route", "policies/{policykind}",
                                new { controller = "Policies", action="Index", area="" },
                                new[] {"MerchantTribeStore.Controllers"});
            routes.MapRoute("faq-route", "faq", new { controller = "Policies", action = "Faq" });

            // estimate shipping
            routes.MapRoute("estimate-shipping", "estimateshipping/{action}/{id}",
                                new { controller = "EstimateShipping", action = "index", id = "0" });

            // Cart
            routes.MapRoute("cart-route", "cart", new { controller = "Cart", action = "index" });
            routes.MapRoute("cart-addcoupon", "cart/addcoupon", new { controller = "Cart", action = "AddCoupon" });
            routes.MapRoute("cart-removecoupont", "cart/removecoupon", new { controller = "Cart", action = "RemoveCoupon" });
            routes.MapRoute("cart-removeitem", "cart/removelineitem", new { controller = "Cart", action = "RemoveLineItem" });
            routes.MapRoute("cart-bulkadd", "cart/bulkadd", new { controller = "Cart", action = "BulkAdd" });

            // checkouts
            routes.MapRoute("checkout-route", "checkout/{action}/{*slug}",
                                new { controller = "Checkout", action = "Index" });
            routes.MapRoute("checkout-paypal-route", "paypalexpresscheckout",
                                new { controller = "PayPalExpressCheckout", action = "Index" });
            routes.MapRoute("checkout-paypal-edit", "paypalexpresscheckout/edit",
                                new { controller = "PayPalExpressCheckout", action = "Edit" });
            routes.MapRoute("paypal-ipn-route", "paypalipn", 
                                new {controller = "PayPalIpn", action="Index" });

            // Search
            routes.MapRoute("search-route", "search", new { controller = "Search", action = "Index" });

            // Store Systems
            routes.MapRoute("notfound", "storenotfound", new { controller = "Store", action = "NotFound" });
            routes.MapRoute("notavailable", "storenotavailable", new { controller = "Store", action = "NotAvailable" });
            routes.MapRoute("storeclosed", "storeclosed", new { controller = "Store", action = "Closed" });
            routes.MapRoute("nopermisssion", "nopermission", new { controller = "Store", action = "NoPermission" });
            routes.MapRoute("error-route", "error", new { controller = "Store", action = "Error" });

            // Admin
            routes.MapPageRoute("admin", "admin", "~/bvadmin/default.aspx");
                                    
            // Home page
            routes.MapRoute("homepage", "", new { controller = "Home", action = "Index" });

            // Other Routes
            routes.MapRoute("fileuploadhandler", "fileuploadhandler/{typecode}/{*details}", new { controller = "FileUpload", action = "Index" });
            routes.MapRoute("flexpartjson", "flexpartjson/{pageid}/{partid}", new { controller = "FlexPartJson", action = "Index" });
            routes.MapRoute("todo", "todo/{action}/{id}/{*params}", new { controller = "ToDo", Action = "Index", id = 0 });
            routes.MapRoute("css", "css/{themename}/styles.css", new { controller = "StyleSheets", action = "Index" });
            routes.MapRoute("js", "js/{filename}", new { controller = "Javascript", action = "Index" });
            routes.MapRoute("scheduledtasks", "scheduledtasks/{storekey}", new { controller = "ScheduledTasks", action = "Index" });

            // Site Map
            routes.MapRoute("sitemap-route", "sitemap/{*params}", new { controller = "SiteMap", action = "Index" });
            routes.MapRoute("sitemapxml", "sitemap.xml", new { controller = "SiteMap", action = "Xml"});
            
            // Api
            routes.MapRoute("apirest", "api/rest/v{version}/{modelname}/{*parameters}", new { controller = "ApiRest", action = "Index", version=1 });
            routes.MapRoute("stoneedge", "api/stoneedge", new { controller = "StoneEdge", action = "Index" });

            // Multi-Store Super Routes          
            routes.MapRoute("superstores", "super/stores/{action}/{*params}", new { controller = "SuperStores", Action = "Index" });
            routes.MapRoute("superhome", "super/{action}/{*params}", new { controller = "Super", Action = "Index" });


            routes.MapRoute("templatetest", "templatetest/{*params}", new { controller = "TemplateTest", Action = "Index" });

            // Custom Router
            // This should catch anything
            routes.Add("bvroute", new Route("{*slug}", new CustomRouter()));            
                        
        }
        
        void Application_Start(object sender, EventArgs e)
        {            
            // Log4Net
            log4net.Config.XmlConfigurator.Configure();
            EventLog.LogEvent("System", "Application Startup", MerchantTribe.Web.Logging.EventLogSeverity.Information);

            // routing
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);                        

            // Scheduled Tasks Launcher
            string disableCacheTimers = System.Configuration.ConfigurationManager.AppSettings["disablecachetimers"];
            if (disableCacheTimers != "1")
            {
                ScheduleTaskTrigger();
            }

            
        }

        void Application_End(object sender, EventArgs e)
        {
            EventLog.LogEvent("System", "Application Shutdown", MerchantTribe.Web.Logging.EventLogSeverity.Information);
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            Exception ex = Server.GetLastError().GetBaseException();            
            MerchantTribe.Commerce.EventLog.LogEvent(ex, MerchantTribe.Web.Logging.EventLogSeverity.Error);
            while (ex.InnerException != null)
            {
                MerchantTribe.Commerce.EventLog.LogEvent(ex);
            }
        }

        void Session_Start(object sender, EventArgs e)
        {
        }

        void Session_End(object sender, EventArgs e)
        {           
        }

        void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.IsLocal)
            {
                MvcMiniProfiler.MiniProfiler profiler = MvcMiniProfiler.MiniProfiler.Start();
            }
            var pro = MvcMiniProfiler.MiniProfiler.Current;
            using (pro.Step("Clean Up Domains"))
            {
                CleanUpDomains();
            }
            using (pro.Step("Culture"))
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(MerchantTribe.Commerce.WebAppSettings.SiteCultureCode);
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(MerchantTribe.Commerce.WebAppSettings.SiteCultureCode);
            }
        }

        void Application_EndRequest()
        {
            MvcMiniProfiler.MiniProfiler.Stop();
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (custom.Equals("disablecsscaching", StringComparison.CurrentCultureIgnoreCase))
            {
                if (System.Configuration.ConfigurationManager.AppSettings["disablecsscaching"] == "1")
                {
                    return System.Guid.NewGuid().ToString();
                }
                return "0";
            }
            else
                return base.GetVaryByCustomString(context, custom);
        }

        // Lower cases url and replaces *.bvcommerce.com with *.merchanttribestores.com
        private void CleanUpDomains()
        {
            // skip fixup for form posts
            if (Request.HttpMethod.ToUpperInvariant() == "POST") return;

            string original = Request.Url.OriginalString;

            UriBuilder builder = new UriBuilder(Request.Url);

            if (builder.Host.EndsWith("bvcommerce.com"))
            {
                builder.Host = builder.Host.Replace("bvcommerce.com", "merchanttribestores.com");
                Response.RedirectPermanent(builder.ToString());
            }
            
            // Skip case adjustment for now until we can test more
            //builder.Host = builder.Host.ToLowerInvariant();
            //builder.Path = builder.Path.ToLowerInvariant();

            //string destination = builder.ToString();
            //if (destination != original)
            //{
            //    Response.RedirectPermanent(destination);
            //}            
        }
    }
}
