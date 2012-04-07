using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Content;
using MerchantTribeStore.Filters;

namespace MerchantTribeStore.Areas.signup.Controllers
{
    public class BaseSignupController : Controller
    {
        // Initialize Store Specific Request Data
        MerchantTribe.Commerce.RequestContext _BVRequestContext = new RequestContext();


        public MerchantTribeApplication MTApp
        {
            get { return MerchantTribeApplication.Current; }
            set { MerchantTribeApplication.Current = value; }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            MTApp = MerchantTribeApplication.InstantiateForDataBase(_BVRequestContext);
            MTApp.CurrentRequestContext.RoutingContext = this.Request.RequestContext;

            // JQuery
            ViewBag.JqueryInclude = Helpers.Html.JQueryIncludes(Url.Content("~/scripts"), this.Request.IsSecureConnection);

            // Phone
            ViewBag.PhoneNumber = "1-804-476-0030";
            
            // Google Analytics Tracker
            ViewBag.SignUpGoogleId = "UA-66804-7";
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
