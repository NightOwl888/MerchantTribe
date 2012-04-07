using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Accounts;
using MerchantTribe.Commerce.Catalog;

namespace MerchantTribeStore.BVAdmin
{
    public partial class ResetPassword : System.Web.UI.Page, IMultiStorePage
    {

        public MerchantTribeApplication MTApp
        {
            get { return MerchantTribeApplication.Current; }
            set { MerchantTribeApplication.Current = value; }
        }
        
        protected override void OnPreInit(System.EventArgs e)
        {
            base.OnPreInit(e);
            MTApp = MerchantTribeApplication.InstantiateForDataBase(new RequestContext());
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
        }
        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                CheckSSL();                
                this.UsernameField.Focus();                               
            }
        }
        private void CheckSSL()
        {
            if (!Request.IsSecureConnection)
            {
                MerchantTribe.Commerce.Utilities.SSL.SSLRedirect(this.MTApp,
                    this.MTApp.CurrentStore,
                    MerchantTribe.Commerce.Utilities.SSL.SSLRedirectTo.SSL);
            }
        }

        protected void lnkReset_Click(object sender, EventArgs e)
        {
            if (MTApp.AccountServices.AdminUserResetRequest(this.UsernameField.Text.Trim(), MTApp.CurrentStore))
            {
                this.MessageBox1.ShowOk("Check your email for your reset password link.");
            }
            else
            {
                this.MessageBox1.ShowWarning("Please check your email address and try again.");
            }                      
        }
    }
}