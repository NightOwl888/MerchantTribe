using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MerchantTribe.Commerce.Accounts;
using MerchantTribe.Commerce;

namespace MerchantTribeStore
{
    /// <summary>
    /// Summary description for BaseSignupPage
    /// </summary>
    /// 
    public class BaseSignupPage : System.Web.UI.Page
    {
        protected ViewDataDictionary ViewData { get; set; }

        public MerchantTribeApplication MTApp
        {
            get { return MerchantTribeApplication.Current; }
            set { MerchantTribeApplication.Current = value; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            MTApp = MerchantTribeApplication.InstantiateForDataBase(new RequestContext());
            MTApp.CurrentRequestContext.RoutingContext = this.Request.RequestContext;
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ViewData = new ViewDataDictionary();
        }

    }
}