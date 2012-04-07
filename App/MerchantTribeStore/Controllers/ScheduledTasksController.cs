using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MerchantTribe.Commerce;
using System.Configuration;
using MerchantTribe.Commerce.Scheduling;
using MerchantTribe.Web.Logging;

namespace MerchantTribeStore.Controllers
{
    public class ScheduledTasksController : Controller, IMultiStorePage
    {
        #region " Store Specific Setup Code"
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
            IntegrationLoader.AddIntegrations(this.MTApp.CurrentRequestContext.IntegrationEvents, this.MTApp);
        }
        private void CheckFor301(string slug)
        {
            MerchantTribe.Commerce.Content.CustomUrl url = MTApp.ContentServices.CustomUrls.FindByRequestedUrl(slug);
            if (url != null)
            {
                if (url.Bvin != string.Empty)
                {
                    if (url.IsPermanentRedirect)
                    {
                        Response.RedirectPermanent(url.RedirectToUrl);
                    }
                    else
                    {
                        Response.Redirect(url.RedirectToUrl);
                    }
                }
            }
        }
        #endregion

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string storekey)
        {
            string AcutalKey = WebAppSettings.StoreKey;
            if (AcutalKey != storekey)
            {
                EventLog.LogEvent("Scheduled Tasks", "Store Key Mismatch on Scheduled Task Call for Store " + MTApp.CurrentStore.Id, EventLogSeverity.Warning);
                return View();
            }

            
            QueuedTask task = MTApp.ScheduleServices.QueuedTasks.PopATaskForRun(MTApp.CurrentStore.Id);
            if (task == null)
            {
                EventLog.LogEvent("Scheduled Tasks", "No Task found on call to store " + MTApp.CurrentStore.Id, EventLogSeverity.Information);
                return View();
            }

            try
            {

                    // call task processor here
                    task.Status = QueuedTaskStatus.Failed;
                    task.StatusNotes = "Failed to locate the requested processor for this task.";
                    MTApp.ScheduleServices.QueuedTasks.Update(task);

            }
            catch (Exception ex)
            {
                task.Status = QueuedTaskStatus.Failed;
                task.StatusNotes = ex.Message + " " + ex.StackTrace;
                MTApp.ScheduleServices.QueuedTasks.Update(task);
            }

            return View();
        }

    }
}
