using System;
using System.Linq;
using System.Web;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Accounts;
using MerchantTribe.Commerce.Controls;
using MerchantTribe.Commerce.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MerchantTribe.Web;
using MerchantTribe.Commerce.Catalog;

namespace MerchantTribeStore
{
    /// <summary>
    /// Summary description for BaseSuperPage
    /// </summary>
    public class BaseSuperPage : System.Web.UI.Page, IMultiStorePage
    {
        private Guid? _AuthTokenGuid = null;
        private RequestContext _CurrentRequestContext = new RequestContext();

        public Guid? AuthTokenGuid
        {
            get { return _AuthTokenGuid; }
            set { _AuthTokenGuid = value; }
        }
        public UserAccount CurrentUser
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
        public MerchantTribeApplication MTApp
        {
            get { return MerchantTribeApplication.Current; }
            set { MerchantTribeApplication.Current = value; }
        }
        public bool RequiresSSL
        {
            get { return true; }
        }
        public string PageTitle
        {
            get { return this.Title; }
            set { this.Title = value; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            MTApp = MerchantTribeApplication.InstantiateForDataBase(new RequestContext());
            MTApp.CurrentRequestContext.RoutingContext = this.Request.RequestContext;

            // Determine store id        
            MTApp.CurrentStore = UrlHelper.ParseStoreFromUrl(System.Web.HttpContext.Current.Request.Url, MTApp);
            if (MTApp.CurrentStore == null)
            {
                Response.Redirect("~/storenotfound");
            }

            ValidateSuperLogin();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!Request.IsSecureConnection)
            {
                SSL.SSLRedirect(this.MTApp, this.MTApp.CurrentStore, SSL.SSLRedirectTo.SSL);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            WebForms.MakePageNonCacheable(this);
        }

        public void ValidateSuperLogin()
        {
            bool validLogin = false;

            Guid? tokenId = Cookies.GetCookieGuid(WebAppSettings.CookieNameAuthenticationTokenAdmin(MTApp.CurrentStore.Id),
                    this.Page.Request.RequestContext.HttpContext,
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