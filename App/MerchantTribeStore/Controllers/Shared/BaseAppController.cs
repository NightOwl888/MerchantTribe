using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Content;
using MerchantTribeStore.Filters;
using MerchantTribe.Commerce.Utilities;
using MvcMiniProfiler;

namespace MerchantTribeStore.Controllers.Shared
{
    [StoreClosedFilter]
    public class BaseAppController : Controller, IMultiStorePage
    {

        public MerchantTribeApplication MTApp
        {
            get { return MerchantTribeApplication.Current; }
            set { MerchantTribeApplication.Current = value; }
        }


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var profiler = MvcMiniProfiler.MiniProfiler.Current;
            using (profiler.Step("BaseAppController: Action Executed"))
            {
                using (profiler.Step("Base Controller Action Executed"))
                {
                    base.OnActionExecuting(filterContext);
                }

                using (profiler.Step("Determine Current Store"))
                {
                    using (profiler.Step("Create MT App"))
                    {
                        MTApp = MerchantTribeApplication.InstantiateForDataBase(new RequestContext());
                        // Check for non-www url and redirect if needed
                        //RedirectBVCommerceCom(System.Web.HttpContext.Current);
                        MTApp.CurrentRequestContext.RoutingContext = this.Request.RequestContext;
                    }

                    using (profiler.Step("Parse Store Id from Url"))
                    {
                        // Determine store id        
                        MTApp.CurrentStore = MerchantTribe.Commerce.Utilities.UrlHelper.ParseStoreFromUrl(System.Web.HttpContext.Current.Request.Url, MTApp);
                        if (MTApp.CurrentStore == null)
                        {
                            Response.Redirect("~/storenotfound");
                        }
                    }
                }

                if (MTApp.CurrentStore.Status == MerchantTribe.Commerce.Accounts.StoreStatus.Deactivated)
                {
                    //if ((AvailableWhenInactive == false))
                    //{
                    Response.Redirect("~/storenotavailable");
                    //}
                }

                using (profiler.Step("ViewBag Loads"))
                {
                    using (profiler.Step("IsAdminUser?"))
                    {
                        if (ViewBag.IsAdmin == null)
                        {
                            // Store data for admin panel
                            ViewBag.IsAdmin = IsCurrentUserAdmin(this.MTApp, this.Request.RequestContext.HttpContext);
                        }
                    }

                    using (profiler.Step("RootUrls"))
                    {
                        ViewBag.RootUrlSecure = this.MTApp.StoreUrl(true, false);
                        ViewBag.RootUrl = this.MTApp.StoreUrl(false, true);
                    }

                    using (profiler.Step("Store Closed And Name"))
                    {
                        ViewBag.StoreClosed = MTApp.CurrentStore.Settings.StoreClosed;
                        ViewBag.StoreName = MTApp.CurrentStore.Settings.FriendlyName;
                    }
                    using (profiler.Step("UniqueId"))
                    {
                        ViewBag.StoreUniqueId = MTApp.CurrentStore.StoreUniqueId(MTApp);
                    }
                    using (profiler.Step("ip"))
                    {
                        ViewBag.CustomerIp = Request.UserHostAddress ?? "0.0.0.0";
                    }
                    using (profiler.Step("CustomerId"))
                    {                        
                        ViewBag.CustomerId = MTApp.CurrentCustomerId ?? string.Empty;
                    }
                    using (profiler.Step("Analytics Off?"))
                    {
                        ViewBag.HideAnalytics = MTApp.CurrentStore.Settings.Analytics.DisableMerchantTribeAnalytics;
                    }
                }

                using (profiler.Step("Integration Loader"))
                {
                    // Integrations
                    IntegrationLoader.AddIntegrations(this.MTApp.CurrentRequestContext.IntegrationEvents, this.MTApp);
                }
            }
        }

        public bool IsCurrentUserAdmin(MerchantTribeApplication app, HttpContextBase httpContext)
        {

            Guid? tokenId = MerchantTribe.Web.Cookies.GetCookieGuid(
                                WebAppSettings.CookieNameAuthenticationTokenAdmin(app.CurrentStore.Id),
                                httpContext, new EventLog());

            // no token, return
            if (!tokenId.HasValue) return false;

            if (app.AccountServices.IsTokenValidForStore(app.CurrentStore.Id, tokenId.Value))
            {
                return true;
            }

            return false;
        }


        protected void FlashInfo(string message)
        {
            FlashMessage(message, "flash-message-info");
        }
        protected void FlashSuccess(string message)
        {
            FlashMessage(message, "flash-message-success");
        }
        protected void FlashFailure(string message)
        {
            FlashMessage(message, "flash-message-failure");
        }
        protected void FlashWarning(string message)
        {
            FlashMessage(message, "flash-message-warning");
        }
        private void FlashMessage(string message, string typeClass)
        {
            string format = "<div class=\"{0}\"><p>{1}</p></div>";
            this.TempData["messages"] += string.Format(format, typeClass, message);
        }
    }
}
