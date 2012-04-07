using System;
using MerchantTribe.Commerce;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MerchantTribe.Commerce.Accounts;
using MerchantTribe.Commerce.Catalog;
using MerchantTribe.Commerce.Contacts;

namespace MerchantTribeStore
{
    public class BaseAdminPage : System.Web.UI.Page, IMultiStorePage
    {
        protected const string DefaultCatalogPage = "~/BVAdmin/Catalog/Default.aspx";

        private Guid? _AuthTokenGuid = null;

        //private MerchantTribe.Commerce.RequestContext _CurrentRequestContext = new MerchantTribe.Commerce.RequestContext();
        private IMessageBox _messageBox = null;

        public MerchantTribeApplication MTApp
        {
            get { return MerchantTribeApplication.Current; }
            set { MerchantTribeApplication.Current = value; }
        }
     
        public IMessageBox PageMessageBox
        {
            get { return _messageBox; }
            set { _messageBox = value; }
        }
        public Guid? AuthTokenGuid
        {
            get { return _AuthTokenGuid; }
            set { _AuthTokenGuid = value; }
        }
        public MerchantTribe.Commerce.Accounts.UserAccount CurrentUser
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
        public virtual bool RequiresSSL
        {
            get { return false; }
        }
        public string PageTitle
        {
            get { return this.Title; }
            set { this.Title = value; }
        }
        public AdminTabType CurrentTab
        {
            get
            {
                if (Session["ActiveAdminTab"] == null)
                {
                    return AdminTabType.Dashboard;
                }
                else
                {
                    return (AdminTabType)Session["ActiveAdminTab"];
                }
            }
            set { Session["ActiveAdminTab"] = value; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            MTApp = MerchantTribeApplication.InstantiateForDataBase(new RequestContext());

            // Store routing context for URL Rewriting
            MTApp.CurrentRequestContext.RoutingContext = this.Request.RequestContext;

            // Determine store id        
            MTApp.CurrentStore = MerchantTribe.Commerce.Utilities.UrlHelper.ParseStoreFromUrl(System.Web.HttpContext.Current.Request.Url, MTApp);
            if (MTApp.CurrentStore == null)
            {
                Response.Redirect("~/storenotfound");
            }

            if (MTApp.CurrentStore.Status == MerchantTribe.Commerce.Accounts.StoreStatus.Deactivated)
            {
                Response.Redirect("~/storenotavailable");
            }

            // Culture Settings
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(MTApp.CurrentStore.Settings.CultureCode);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(MTApp.CurrentStore.Settings.CultureCode);

            IntegrationLoader.AddIntegrations(this.MTApp.CurrentRequestContext.IntegrationEvents, this.MTApp);

			ValidateAdminLogin();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // don't redirect during design time
            if (DesignMode) { return; }
            
            if (RequiresSSL)
            {
                if (WebAppSettings.UseSsl)
                {
                    if (!Request.IsSecureConnection)
                    {
                        MerchantTribe.Commerce.Utilities.SSL.SSLRedirect(this.MTApp, this.MTApp.CurrentStore, MerchantTribe.Commerce.Utilities.SSL.SSLRedirectTo.SSL);
                    }
                }
            }
            else
            {
                if (WebAppSettings.UseSsl)
                {
                    //if (Request.IsSecureConnection)
                    //{
                    //    MerchantTribe.Commerce.Utilities.SSL.SSLRedirect(this, this.MTApp.CurrentStore, MerchantTribe.Commerce.Utilities.SSL.SSLRedirectTo.NonSSL);
                    //}
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                MerchantTribe.Commerce.Utilities.WebForms.MakePageNonCacheable(this);
            }
        }

        public void ValidateAdminLogin()
        {
            bool validLogin = false;

            Guid? tokenId = MerchantTribe.Web.Cookies.GetCookieGuid(WebAppSettings.CookieNameAuthenticationTokenAdmin(MTApp.CurrentStore.Id),
                    this.Page.Request.RequestContext.HttpContext,
                    new EventLog());

            if (tokenId.HasValue)
            {
                if (this.MTApp.AccountServices.IsTokenValidForStore(MTApp.CurrentStore.Id, tokenId.Value))
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

        public void ValidateCurrentUserHasPermission(string p)
        {
            Collection<string> l = new Collection<string>();
            l.Add(p);
            //ValidateCurrentUserHasPermissions(l)
        }

        public void ShowMessage(string message, ErrorTypes type)
        {
            switch (type)
            {
                case ErrorTypes.Ok:
                    this.PageMessageBox.ShowOk(message);
                    break;
                case ErrorTypes.Info:
                    this.PageMessageBox.ShowInformation(message);
                    break;
                case ErrorTypes.Error:
                    this.PageMessageBox.ShowError(message);
                    break;
                case ErrorTypes.Warning:
                    this.PageMessageBox.ShowWarning(message);
                    break;
            }
        }

    }

}