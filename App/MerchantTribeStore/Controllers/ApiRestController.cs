using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Accounts;
using MerchantTribe.CommerceDTO.v1;
using System.Dynamic;
using System.Xml.Serialization;
using System.IO;
using MerchantTribe.Commerce.Catalog;

using MerchantTribeStore.api.rest;

namespace MerchantTribeStore.Controllers
{    
    public class ApiRestController : Controller, IMultiStorePage
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

            HttpContext.Items.Add("mtapp", MTApp);

            // Jquery
            //ViewData["JQueryInclude"] = Helpers.Html.JQueryIncludes(Url.Content("~/scripts"), this.Request.IsSecureConnection);
        }

        // Authenticate API Key Methods
        public bool AuthenticateKey(string apikey, IApiResponse response)
        {

            if (apikey.Trim().Length < 1)
            {
                response.Errors.Add(new ApiError("KEY", "Api Key is invalid or missing."));
                return false;
            }

            ApiKey key = MTApp.AccountServices.ApiKeys.FindByKey(apikey);
            if (key == null)
            {
                response.Errors.Add(new ApiError("KEY", "Api Key is invalid or missing."));
                return false;
            }
            if (key.Key != apikey)
            {
                response.Errors.Add(new ApiError("KEY", "Api Key is invalid or missing."));
                return false;
            }
            if (key.StoreId < 0)
            {
                response.Errors.Add(new ApiError("KEY", "Api Key is invalid or missing."));
                return false;
            }

            long storeID = key.StoreId;
            MTApp.CurrentStore = MTApp.AccountServices.Stores.FindById(storeID);
            if (MTApp.CurrentStore == null)
            {
                response.Errors.Add(new ApiError("STORENOTFOUND", "No store was found at this URL."));
                return false;
            }
            if (MTApp.CurrentStore.Id < 0)
            {
                response.Errors.Add(new ApiError("STORENOTFOUND2", "No store was found at this URL."));
                return false;
            }
            if (MTApp.CurrentStore.Status == MerchantTribe.Commerce.Accounts.StoreStatus.Deactivated)
            {
                response.Errors.Add(new ApiError("STOREDISABLED", "Store is not active. Contact an Administrator for Assistance"));
                return false;
            }

            return true;
        }

        // Actual Responder             
        public ActionResult Index(string version, string modelname, string parameters)
        {
            string data = string.Empty;

            // Key Api Key First
            string key = Request.QueryString["key"];
            ApiResponse<object> FailedKeyResponse = new ApiResponse<object>();
            if (!AuthenticateKey(key, FailedKeyResponse))
            {
                data = MerchantTribe.Web.Json.ObjectToJson(FailedKeyResponse);                
            }
            else
            {
                // Create Handler
                IRestHandler handler = RestHandlerFactory.Instantiate(version, modelname, MTApp);

                // Read Posted JSON
                string postedString = string.Empty;
                Stream inputStream = Request.InputStream;
                if (inputStream != null)
                {
                    StreamReader rdr = new StreamReader(inputStream);
                    postedString = rdr.ReadToEnd();
                }

                switch (Request.HttpMethod.ToUpperInvariant())
                {
                    case "GET":
                        data = handler.GetAction(parameters, Request.QueryString);
                        break;
                    case "POST":
                        data = handler.PostAction(parameters, Request.QueryString, postedString);
                        break;
                    case "PUT":
                        data = handler.PutAction(parameters, Request.QueryString, postedString);
                        break;
                    case "DELETE":
                        data = handler.DeleteAction(parameters, Request.QueryString, postedString);
                        break;
                }                
            }

            // Return Result (or text formatted result)
            if (Request.QueryString["apiformat"] == "text")
            {
                return new RawResult(data, "text/html");
            }
            return new PreJsonResult(data);                        
        }



    }
}
