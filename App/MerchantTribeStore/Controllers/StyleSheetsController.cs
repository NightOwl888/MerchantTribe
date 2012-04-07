using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Content;
using MerchantTribe.Commerce.Accounts;
using System.IO;

namespace MerchantTribeStore.Controllers
{
    public class StyleSheetsController : Controller
    {


        // Initialize Store Specific Request Data
        //MerchantTribe.Commerce.RequestContext _BVRequestContext = new RequestContext();
        public MerchantTribe.Commerce.RequestContext CurrentRequestContext
        {
            get { return MTApp.CurrentRequestContext; }
            set { MTApp.CurrentRequestContext = value; }
        }

        public MerchantTribeApplication MTApp
        {
            get { return MerchantTribeApplication.Current; }
            set  { MerchantTribeApplication.Current = value; }
        }
        

        public MerchantTribe.Commerce.Accounts.Store CurrentStore
        {
            get { return MTApp.CurrentStore; }
            set { MTApp.CurrentStore = value; }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            MTApp = MerchantTribeApplication.InstantiateForDataBase(new RequestContext());
            MTApp.CurrentRequestContext.RoutingContext = this.Request.RequestContext;

            // Determine store id        
            MTApp.CurrentStore = MerchantTribe.Commerce.Utilities.UrlHelper.ParseStoreFromUrl(System.Web.HttpContext.Current.Request.Url, MTApp);
            if (MTApp.CurrentStore == null)
            {
                Response.Redirect("~/storenotfound");
            }
            

        }        

        //
        // GET: /StyleSheets/
        //[OutputCache(VaryByHeader = "Host", VaryByParam = "themename", VaryByCustom = "disablecsscaching", Duration = 150)]
        public ActionResult Index(string themename)
        {
            string themeId = themename.Replace("theme-", "");
            ThemeManager themes = MTApp.ThemeManager();

            string css = themes.CurrentStyleSheetContentMinifiedAndReplaced(themeId);
                                                                              
            Response.AddHeader("content-disposition", "attachment; filename=styles.css");            
            return Content(css, "text/css");                        
        }

    }
}
