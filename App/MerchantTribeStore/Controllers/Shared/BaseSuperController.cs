using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Accounts;

namespace MerchantTribeStore.Controllers.Shared
{
    public class BaseSuperController : Controller
    {
        // Initialize Store Specific Request Data
        private Guid? _AuthTokenGuid = null;
        public Guid? AuthTokenGuid
        {
            get { return _AuthTokenGuid; }
            set { _AuthTokenGuid = value; }
        }

        MerchantTribe.Commerce.RequestContext _CurrentRequestContext = new RequestContext();
        public MerchantTribe.Commerce.RequestContext CurrentRequestContext
        {
            get { return _CurrentRequestContext; }
            set { _CurrentRequestContext = value; }
        }
        public MerchantTribe.Commerce.Accounts.Store CurrentStore
        {
            get { return _CurrentRequestContext.CurrentStore; }
            set { _CurrentRequestContext.CurrentStore = value; }
        }

        public MerchantTribeApplication MTApp
        {
            get { return MerchantTribeApplication.Current; }
            set { MerchantTribeApplication.Current = value; }
        }

        public UserAccount CurrentSuperUser
        {
            get
            {
                if (AuthTokenGuid.HasValue)
                {
                    return MTApp.AccountServices.FindAdminUserByAuthTokenId(AuthTokenGuid.Value);
                }
                return null;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
                        
            CurrentRequestContext.RoutingContext = this.Request.RequestContext;
            MTApp = new MerchantTribeApplication(CurrentRequestContext);

            // Determine store id        
            CurrentStore = MerchantTribe.Commerce.Utilities.UrlHelper.ParseStoreFromUrl(System.Web.HttpContext.Current.Request.Url, MTApp);
            if (CurrentStore == null)
            {
                Response.Redirect("~/storenotfound");
            }

            if (CurrentStore.Status == MerchantTribe.Commerce.Accounts.StoreStatus.Deactivated)
            {
                Response.Redirect("~/storenotavailable");
            }

            // Culture Settings
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(CurrentStore.Settings.CultureCode);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(CurrentStore.Settings.CultureCode);

            ValidateSuperLogin();

            // Jquery
            ViewData["JQueryInclude"] = Helpers.Html.JQueryIncludes(Url.Content("~/scripts"), this.Request.IsSecureConnection);
            
            ViewBag.AppVersion = WebAppSettings.SystemVersionNumber;
            ViewBag.StoreName = MTApp.CurrentStore.Settings.FriendlyName;
            ViewBag.RenderedMenu = Helpers.Html.RenderSuperMenu(MTApp);                

        }

        public void ValidateSuperLogin()
        {
            bool validLogin = false;

            Guid? tokenId = MerchantTribe.Web.Cookies.GetCookieGuid(WebAppSettings.CookieNameAuthenticationTokenAdmin(MTApp.CurrentStore.Id),
                    this.HttpContext,
                    new EventLog());

            if (tokenId.HasValue)
            {
                if (MTApp.AccountServices.IsTokenValidForSuperUser(tokenId.Value))
                {
                    validLogin = true;
                }
            }

            if (validLogin == false)
            {
                Response.Redirect("~/adminaccount/login");
            }

            _AuthTokenGuid = tokenId;
        }

    }
}
