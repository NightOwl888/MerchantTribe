using System;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using MerchantTribe.Commerce.Catalog;
using MvcMiniProfiler;

namespace MerchantTribeStore
{
    public class CustomRouter : IRouteHandler
    {
        private class CategoryUrlMatchData
        {
            public bool IsFound { get; set; }
            public CategorySourceType SourceType { get; set; }

            public CategoryUrlMatchData()
            {
                IsFound = false;
                SourceType = CategorySourceType.Manual;
            }
        }


        public System.Web.IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var pro = MvcMiniProfiler.MiniProfiler.Current;
            using (pro.Step("Custom Router"))
            {
                MerchantTribe.Commerce.MerchantTribeApplication MTApp;
                string fullSlug;

                using (pro.Step("Determine Store Id"))
                {
                    fullSlug = (string)(requestContext.RouteData.Values["slug"] ?? string.Empty);
                    fullSlug = fullSlug.ToLowerInvariant() ?? string.Empty;

                    // Application Context
                    MTApp = MerchantTribe.Commerce.MerchantTribeApplication.InstantiateForDataBase(new MerchantTribe.Commerce.RequestContext());
                    MTApp.CurrentRequestContext.RoutingContext = requestContext;

                    // Determine store id        
                    MTApp.CurrentStore = MerchantTribe.Commerce.Utilities.UrlHelper.ParseStoreFromUrl(System.Web.HttpContext.Current.Request.Url, MTApp);
                }

                using (pro.Step("Home Page Check"))
                {
                    // Home page check
                    if (fullSlug == string.Empty)
                    {
                        // Redirect to Sign up if we're multi-store
                        if (MerchantTribe.Commerce.WebAppSettings.IsHostedVersion)
                        {
                            if (MTApp.CurrentStore.StoreName == "www")
                            {
                                requestContext.RouteData.Values["controller"] = "Home";
                                requestContext.RouteData.Values["area"] = "signup";
                                requestContext.RouteData.Values["action"] = "Index";
                                System.Web.Mvc.MvcHandler signupHandler = new MvcHandler(requestContext);
                                return signupHandler;
                            }
                        }

                        using (pro.Step("Sending to Home Page Controller"))
                        {
                            // Send to home controller
                            requestContext.RouteData.Values["controller"] = "Home";
                            requestContext.RouteData.Values["action"] = "Index";
                            System.Web.Mvc.MvcHandler homeHandler = new MvcHandler(requestContext);
                            return homeHandler;
                        }
                    }
                }

                // Check for Category/Page Match
                CategoryUrlMatchData categoryMatchData = IsCategoryMatch(fullSlug, MTApp);
                if (categoryMatchData.IsFound)
                {
                    switch (categoryMatchData.SourceType)
                    {
                        case CategorySourceType.ByRules:
                        case CategorySourceType.CustomLink:
                        case CategorySourceType.Manual:
                            requestContext.RouteData.Values["controller"] = "Category";
                            requestContext.RouteData.Values["action"] = "Index";
                            System.Web.Mvc.MvcHandler mvcHandlerCat = new MvcHandler(requestContext);
                            return mvcHandlerCat;
                        case CategorySourceType.DrillDown:
                            requestContext.RouteData.Values["controller"] = "Category";
                            requestContext.RouteData.Values["action"] = "DrillDownIndex";
                            System.Web.Mvc.MvcHandler mvcHandlerCatDrill = new MvcHandler(requestContext);
                            return mvcHandlerCatDrill;
                        case CategorySourceType.FlexPage:
                            requestContext.RouteData.Values["controller"] = "FlexPage";
                            requestContext.RouteData.Values["action"] = "Index";
                            System.Web.Mvc.MvcHandler mvcHandler2 = new System.Web.Mvc.MvcHandler(requestContext);
                            return mvcHandler2;
                        case CategorySourceType.CustomPage:
                            requestContext.RouteData.Values["controller"] = "CustomPage";
                            requestContext.RouteData.Values["action"] = "Index";
                            System.Web.Mvc.MvcHandler mvcHandlerCustom = new MvcHandler(requestContext);
                            return mvcHandlerCustom;
                    }
                }

                // Check for Product URL
                if (IsProductUrl(fullSlug, MTApp))
                {
                    requestContext.RouteData.Values["controller"] = "Products";
                    requestContext.RouteData.Values["action"] = "Index";
                    System.Web.Mvc.MvcHandler mvcHandlerProducts = new MvcHandler(requestContext);
                    return mvcHandlerProducts;
                }

                // no match on product or category so do a 301 check
                CheckFor301(fullSlug, MTApp);

                // If not product, send to FlexPage Controller
                requestContext.RouteData.Values["controller"] = "FlexPage";
                requestContext.RouteData.Values["action"] = "Index";
                System.Web.Mvc.MvcHandler mvcHandler = new System.Web.Mvc.MvcHandler(requestContext);
                return mvcHandler;
            }

        }

        private void CheckFor301(string slug, MerchantTribe.Commerce.MerchantTribeApplication app)
        {
            MerchantTribe.Commerce.Content.CustomUrl url = app.ContentServices.CustomUrls.FindByRequestedUrl(slug);
            if (url != null)
            {
                if (url.Bvin != string.Empty)
                {
                    string destination = app.StoreUrl(false, false) + url.RedirectToUrl.TrimStart('/');

                    if (url.IsPermanentRedirect)
                    {
                        app.CurrentRequestContext.RoutingContext.HttpContext.Response.RedirectPermanent(destination);
                    }
                    else
                    {
                        app.CurrentRequestContext.RoutingContext.HttpContext.Response.Redirect(destination);
                    }
                }
            }
        }

        private CategoryUrlMatchData IsCategoryMatch(string fullSlug, MerchantTribe.Commerce.MerchantTribeApplication app)
        {
            CategoryUrlMatchData result = new CategoryUrlMatchData();

            Category cat = app.CatalogServices.Categories.FindBySlugForStore(fullSlug, app.CurrentRequestContext.CurrentStore.Id);
            if (cat != null)
            {
                result.IsFound = true;
                result.SourceType = cat.SourceType;
            }

            return result;
        }

        private bool IsProductUrl(string fullSlug, MerchantTribe.Commerce.MerchantTribeApplication app)
        {
            // See if we have a matching Product URL
            MerchantTribe.Commerce.Catalog.Product p = app.CatalogServices.Products.FindBySlug(fullSlug);
            if (p != null)
            {
                if (p.Bvin != string.Empty) return true;
            }

            return false;
        }

        //private string GetCustomSlug(MerchantTribe.Commerce.MerchantTribeApplication mtapp)
        //{
        //    string result = string.Empty;
        //    if (mtapp == null) return result;
        //    if (mtapp.CurrentStore.Settings.HomePageIsFlex == false) return result;
        //    string bvin = mtapp.CurrentStore.Settings.HomePageIsFlexBvin;
        //    var cat = mtapp.CatalogServices.Categories.Find(bvin);
        //    if (cat == null) return result;
        //    result = cat.RewriteUrl;
        //    return result;
        //}
    }
}