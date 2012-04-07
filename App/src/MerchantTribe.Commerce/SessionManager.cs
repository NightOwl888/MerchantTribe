using System;
using System.Web;
using MerchantTribe.Commerce.Accounts;

namespace MerchantTribe.Commerce
{
    public class SessionManager
    {

        private SessionManager()
        {

        }

        private const string CurrentUserIdSessionVariable = "Bvc5CurrentUserId";
        private const string ImageBrowserLastFolderVariable = "Bvc5ImageBrowserLastFolder";
        private const string CategoryLastIdVariable = "Bvc5CategoryLastVisited";
        private const string CurrentCartIdSessionVariable = "Bvc5CurrentCartId";
        private const string LastShippingRatesSessionVariable = "Bvc5LastShippingRates";
        private const string LastEditorShippingRatesSessionVariable = "Bvc5LastEditorShippingRates";

        // Admin Settings
        private const string AdminProductSearchCriteriaKeyword = "Bvc5AdminProductSearchCriteriaKeyword";
        private const string AdminProductSearchCriteriaManufacturer = "Bvc5AdminProductSearchCriteriaManufacturer";
        private const string AdminProductSearchCriteriaVendor = "Bvc5AdminProductSearchCriteriaVendor";
        private const string AdminProductSearchCriteriaCategory = "Bvc5AdminProductSearchCriteriaCategory";
        private const string AdminProductSearchCriteriaStatus = "Bvc5AdminProductSearchCriteriaStatus";
        private const string AdminProductSearchCriteriaInventoryStatus = "Bvc5AdminProductSearchCriteriaInventoryStatus";
        private const string AdminProductSearchCriteriaProductType = "Bvc5AdminProductSearchCriteriaProductType";

        private const string AdminKitSearchCriteriaKeyword = "Bvc5AdminKitSearchCriteriaKeyword";
        private const string AdminKitSearchCriteriaManufacturer = "Bvc5AdminKitSearchCriteriaManufacturer";
        private const string AdminKitSearchCriteriaVendor = "Bvc5AdminKitSearchCriteriaVendor";
        private const string AdminKitSearchCriteriaCategory = "Bvc5AdminKitSearchCriteriaCategory";
        private const string AdminKitSearchCriteriaStatus = "Bvc5AdminKitSearchCriteriaStatus";
        private const string AdminKitSearchCriteriaInventoryStatus = "Bvc5AdminKitSearchCriteriaInventoryStatus";
        private const string AdminKitSearchCriteriaProductType = "Bvc5AdminKitSearchCriteriaProductType";

        private const string _AdminOrderSearchKeyword = "Bvc5AdminOrderSearchKeyword";
        private const string _AdminOrderSearchPaymentFilter = "Bvc5AdminOrderSearchPaymentFilter";
        private const string _AdminOrderSearchShippingFilter = "Bvc5AdminOrderSearchShippingFilter";
        private const string _AdminOrderSearchStatusFilter = "Bvc5AdminOrderSearchStatusFilter";
        private const string _AdminOrderSearchDateRange = "Bvc5AdminOrderSearchDateRange";
        private const string _AdminOrderSearchStartDate = "Bvc5AdminOrderSearchStartDate";
        private const string _AdminOrderSearchEndDate = "Bvc5AdminOrderSearchEndDate";
        private const string _AdminOrderSearchLastPage = "Bvc5AdminOrderSearchLastPage";
        private const string _AdminOrderSearchNewestFirst = "BvcAdminOrderSearchNewestFirst";

        private const string _AdminUserSearchCriteriaFirstName = "AdminUserSearchCriteriaFirstName";
        private const string _AdminUserSearchCriteriaLastName = "AdminUserSearchCriteriaLastName";
        private const string _AdminUserSearchCriteriaUserName = "AdminUserSearchCriteriaUserName";
        private const string _AdminUserSearchCriteriaEmail = "AdminUserSearchCriteriaEmail";

        public static bool IsUserAuthenticated(MerchantTribeApplication app)
        {
                string uid = app.CurrentCustomerId;
                if (uid.Trim() == string.Empty) return false;
                MerchantTribe.Commerce.Membership.CustomerAccount customer = app.MembershipServices.Customers.Find(uid);
                if (customer == null) return false;
                if (customer.Bvin.Trim() == string.Empty) return false;
                return true;                
        }

        public static string StoreClosedGuestPasswordForCurrentUser
        {
            get { return GetSessionString("GuestPassword"); }
            set { SetSessionString("GuestPassword", value); }
        }

        //public static bool IsAdminUser
        //{
        //    get
        //    {
        //        bool result = false;

        //        if (IsUserAuthenticated == true)
        //        {
        //            if (Membership.CustomerAccount.DoesUserHavePermission(GetCurrentUserId(), Membership.SystemPermissions.LoginToAdmin) == true)
        //            {
        //                result = true;
        //            }
        //        }

        //        return result;
        //    }
        //}
        public static string CategoryLastId
        {
            get
            {
                string id = GetSessionString(CategoryLastIdVariable);
                if (id == string.Empty)
                {
                    id = "0";
                }
                return id;
            }
            set { SetSessionString(CategoryLastIdVariable, value); }
        }
        public static Utilities.SortableCollection<Shipping.ShippingRateDisplay> LastShippingRates
        {
            get
            {
                Utilities.SortableCollection<Shipping.ShippingRateDisplay> result = null;
                result = (Utilities.SortableCollection<Shipping.ShippingRateDisplay>)GetSessionObject(LastShippingRatesSessionVariable);
                if (result == null)
                {
                    result = new Utilities.SortableCollection<Shipping.ShippingRateDisplay>();
                }
                return result;
            }
            set { SetSessionObject(LastShippingRatesSessionVariable, value); }
        }
        public static Utilities.SortableCollection<Shipping.ShippingRateDisplay> LastEditorShippingRates
        {
            get
            {
                Utilities.SortableCollection<Shipping.ShippingRateDisplay> result = null;
                result = (Utilities.SortableCollection<Shipping.ShippingRateDisplay>)GetSessionObject(LastEditorShippingRatesSessionVariable);
                if (result == null)
                {
                    result = new Utilities.SortableCollection<Shipping.ShippingRateDisplay>();
                }
                return result;
            }
            set { SetSessionObject(LastEditorShippingRatesSessionVariable, value); }
        }


        public static Catalog.ProductStoreSearchCriteria StoreSearchCriteria
        {
            get
            {
                object obj = GetSessionObject("StoreSearchCriteria");
                if (obj != null && obj is Catalog.ProductStoreSearchCriteria)
                {
                    return (Catalog.ProductStoreSearchCriteria)obj;
                }
                else
                {
                    return null;
                }
            }
            set { SetSessionObject("StoreSearchCriteria", value); }
        }

        // Admin Settings
        public static int AdminProductGridPage
        {
            get
            {
                int val = 0;
                if (int.TryParse(GetSessionString("AdminProductGridPage"), out  val))
                {
                    return val;
                }
                else
                {
                    return 0;
                }
            }
            set { SetSessionString("AdminProductGridPage", value.ToString()); }
        }
        public static int AdminKitGridPage
        {
            get
            {
                int val = 0;
                if (int.TryParse(GetSessionString("AdminKitGridPage"), out val))
                {
                    return val;
                }
                else
                {
                    return 0;
                }
            }
            set { SetSessionString("AdminKitGridPage", value.ToString()); }
        }
        public static int AdminCategoriesGridPage
        {
            get
            {
                int val = 0;
                if (int.TryParse(GetSessionString("AdminCategoriesGridPage"), out val))
                {
                    return val;
                }
                else
                {
                    return 0;
                }
            }
            set { SetSessionString("AdminCategoriesGridPage", value.ToString()); }
        }

        public static string AdminProductCriteriaKeyword
        {
            get { return GetSessionString(AdminProductSearchCriteriaKeyword); }
            set { SetSessionString(AdminProductSearchCriteriaKeyword, value); }
        }
        public static string AdminProductCriteriaManufacturer
        {
            get { return GetSessionString(AdminProductSearchCriteriaManufacturer); }
            set { SetSessionString(AdminProductSearchCriteriaManufacturer, value); }
        }
        public static string AdminProductCriteriaVendor
        {
            get { return GetSessionString(AdminProductSearchCriteriaVendor); }
            set { SetSessionString(AdminProductSearchCriteriaVendor, value); }
        }
        public static string AdminProductCriteriaCategory
        {
            get { return GetSessionString(AdminProductSearchCriteriaCategory); }
            set { SetSessionString(AdminProductSearchCriteriaCategory, value); }
        }
        public static string AdminProductCriteriaStatus
        {
            get { return GetSessionString(AdminProductSearchCriteriaStatus); }
            set { SetSessionString(AdminProductSearchCriteriaStatus, value); }
        }
        public static string AdminProductCriteriaInventoryStatus
        {
            get { return GetSessionString(AdminProductSearchCriteriaInventoryStatus); }
            set { SetSessionString(AdminProductSearchCriteriaInventoryStatus, value); }
        }
        public static string AdminProductCriteriaProductType
        {
            get { return GetSessionString(AdminProductSearchCriteriaProductType); }
            set { SetSessionString(AdminProductSearchCriteriaProductType, value); }
        }

        public static string AdminKitCriteriaKeyword
        {
            get { return GetSessionString(AdminKitSearchCriteriaKeyword); }
            set { SetSessionString(AdminKitSearchCriteriaKeyword, value); }
        }
        public static string AdminKitCriteriaManufacturer
        {
            get { return GetSessionString(AdminKitSearchCriteriaManufacturer); }
            set { SetSessionString(AdminKitSearchCriteriaManufacturer, value); }
        }
        public static string AdminKitCriteriaVendor
        {
            get { return GetSessionString(AdminKitSearchCriteriaVendor); }
            set { SetSessionString(AdminKitSearchCriteriaVendor, value); }
        }
        public static string AdminKitCriteriaCategory
        {
            get { return GetSessionString(AdminKitSearchCriteriaCategory); }
            set { SetSessionString(AdminKitSearchCriteriaCategory, value); }
        }
        public static string AdminKitCriteriaStatus
        {
            get { return GetSessionString(AdminKitSearchCriteriaStatus); }
            set { SetSessionString(AdminKitSearchCriteriaStatus, value); }
        }
        public static string AdminKitCriteriaInventoryStatus
        {
            get { return GetSessionString(AdminKitSearchCriteriaInventoryStatus); }
            set { SetSessionString(AdminKitSearchCriteriaInventoryStatus, value); }
        }
        public static string AdminKitCriteriaProductType
        {
            get { return GetSessionString(AdminKitSearchCriteriaProductType); }
            set { SetSessionString(AdminKitSearchCriteriaProductType, value); }
        }

        public static string AdminOrderSearchKeyword
        {
            get { return GetSessionString(_AdminOrderSearchKeyword); }
            set { SetSessionString(_AdminOrderSearchKeyword, value); }
        }
        public static string AdminOrderSearchPaymentFilter
        {
            get { return GetSessionString(_AdminOrderSearchPaymentFilter); }
            set { SetSessionString(_AdminOrderSearchPaymentFilter, value); }
        }
        public static string AdminOrderSearchShippingFilter
        {
            get { return GetSessionString(_AdminOrderSearchShippingFilter); }
            set { SetSessionString(_AdminOrderSearchShippingFilter, value); }
        }
        public static string AdminOrderSearchStatusFilter
        {
            get { return GetSessionString(_AdminOrderSearchStatusFilter); }
            set { SetSessionString(_AdminOrderSearchStatusFilter, value); }
        }
        public static Utilities.DateRangeType AdminOrderSearchDateRange
        {
            get
            {
                Utilities.DateRangeType result = Utilities.DateRangeType.AllDates;
                if (GetSessionObject(_AdminOrderSearchDateRange) != null)
                {
                    result = (Utilities.DateRangeType)GetSessionObject(_AdminOrderSearchDateRange);
                }
                return result;
            }
            set { SetSessionObject(_AdminOrderSearchDateRange, value); }
        }
        public static DateTime AdminOrderSearchStartDate
        {
            get
            {
                DateTime result = DateTime.Now.AddDays(1);
                if (GetSessionObject(_AdminOrderSearchStartDate) != null)
                {
                    result = (DateTime)GetSessionObject(_AdminOrderSearchStartDate);
                }
                return result;
            }
            set { SetSessionObject(_AdminOrderSearchStartDate, value); }
        }
        public static DateTime AdminOrderSearchEndDate
        {
            get
            {
                DateTime result = DateTime.Now.AddDays(1);
                if (GetSessionObject(_AdminOrderSearchEndDate) != null)
                {
                    result = (DateTime)GetSessionObject(_AdminOrderSearchEndDate);
                }
                return result;
            }
            set { SetSessionObject(_AdminOrderSearchEndDate, value); }
        }
        public static int AdminOrderSearchLastPage
        {
            get
            {
                int result = 1;
                object o = GetSessionObject(_AdminOrderSearchLastPage);
                if (o != null)
                {
                    result = (int)o;
                }
                return result;
            }
            set { SetSessionObject(_AdminOrderSearchLastPage, value); }
        }
        public static bool AdminOrderSearchNewestFirst
        {
            get
            {
                object temp = GetSessionObject(_AdminOrderSearchNewestFirst);
                if (temp != null) return (bool)temp;
                return false;
            }
            set { SetSessionObject(_AdminOrderSearchNewestFirst, value); }
        }
        public static string AdminUserSearchCriteriaFirstName
        {
            get { return GetSessionString(_AdminUserSearchCriteriaFirstName); }
            set { SetSessionString(_AdminUserSearchCriteriaFirstName, value); }
        }
        public static string AdminUserSearchCriteriaLastName
        {
            get { return GetSessionString(_AdminUserSearchCriteriaLastName); }
            set { SetSessionString(_AdminUserSearchCriteriaLastName, value); }
        }
        public static string AdminUserSearchCriteriaUserName
        {
            get { return GetSessionString(_AdminUserSearchCriteriaUserName); }
            set { SetSessionString(_AdminUserSearchCriteriaUserName, value); }
        }
        public static string AdminUserSearchCriteriaEmail
        {
            get { return GetSessionString(_AdminUserSearchCriteriaEmail); }
            set { SetSessionString(_AdminUserSearchCriteriaEmail, value); }
        }

        private static Orders.Order CachedShoppingCart
        {
            get
            {
                //we must check the HttpContext, otherwise this will fail during unit tests
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Items["CurrentShoppingCart"] != null)
                    {
                        return (Orders.Order)HttpContext.Current.Items["CurrentShoppingCart"];
                    }
                }
                return null;
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items["CurrentShoppingCart"] = value;
                }
            }
        }
        #region " Private Set and Get "

        public static string GetSessionString(string variableName)
        {
            string result = string.Empty;

            object temp = GetSessionObject(variableName);
            if (temp != null)
            {
                result = (string)GetSessionObject(variableName);
            }

            return result;
        }
        public static void SetSessionString(string variableName, string value)
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session[variableName] = value ?? string.Empty;
                }
            }
        }
        public static object GetSessionObject(string variableName)
        {
            object result = null;


            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Session != null)
                {
                    if (HttpContext.Current.Session[variableName] != null)
                    {
                        result = HttpContext.Current.Session[variableName];
                    }
                }
            }

            return result;
        }
        public static void SetSessionObject(string variableName, object value)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Session[variableName] = value;
            }
        }
        public static string GetCookieString(string cookieName, Store currentStore)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Request != null)
                    {
                        if (HttpContext.Current.Request.Browser.Cookies == true)
                        {
                            var cookies = HttpContext.Current.Request.Cookies;
                            return GetCookieString(cookieName, currentStore, cookies);
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }
        public static string GetCookieString(string cookieName, Store currentStore, HttpCookieCollection cookies)
        {
            string result = string.Empty;

            if (cookies == null) return string.Empty;

            try
            {                
                if (currentStore.Settings.CookieDomain.Trim() != string.Empty)
                {
                    string domain = System.Text.RegularExpressions.Regex.Replace(currentStore.Settings.CookieDomain, "[^A-Za-z0-9]", "");
                    cookieName = cookieName + domain;
                }

                if (currentStore.Settings.CookiePath != string.Empty)
                {
                    string path = System.Text.RegularExpressions.Regex.Replace(currentStore.Settings.CookiePath, "[^A-Za-z0-9]", "");
                    cookieName = cookieName + path;
                }

                HttpCookie checkCookie;
                checkCookie = cookies[cookieName];
                if (checkCookie != null)
                {
                    result = checkCookie.Value;
                }
                checkCookie = null;                
            }
            catch
            {
                result = string.Empty;
            }

            return result;
        }
        public static void SetCookieString(string cookieName, string value, Store currentStore)
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Request.Browser.Cookies == true)
                {
                    try
                    {                        
                        if (currentStore.Settings.CookieDomain.Trim() != string.Empty)
                        {
                            string domain = System.Text.RegularExpressions.Regex.Replace(currentStore.Settings.CookieDomain, "[^A-Za-z0-9]", "");
                            cookieName = cookieName + domain;
                        }

                        if (currentStore.Settings.CookiePath != string.Empty)
                        {
                            string path = System.Text.RegularExpressions.Regex.Replace(currentStore.Settings.CookiePath, "[^A-Za-z0-9]", "");
                            cookieName = cookieName + path;
                        }

                        System.Web.HttpCookie saveCookie = new System.Web.HttpCookie(cookieName, value);
                        if (currentStore.Settings.CookieDomain.Trim() != string.Empty)
                        {
                            saveCookie.Domain = currentStore.Settings.CookieDomain;
                        }
                        if (currentStore.Settings.CookiePath.Trim() != string.Empty)
                        {
                            saveCookie.Path = currentStore.Settings.CookiePath;
                        }
                        saveCookie.Expires = DateTime.Now.AddYears(50);
                        HttpContext.Current.Response.Cookies.Add(saveCookie);
                    }
                    catch (Exception Ex)
                    {
                        EventLog.LogEvent(Ex);
                    }
                }
            }
        }
        public static void SetCookieString(string cookieName, string value, Store currentStore, DateTime expirationDate, bool secure)
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Request.Browser.Cookies == true)
                {
                    try
                    {
                        if (currentStore.Settings.CookieDomain.Trim() != string.Empty)
                        {
                            string domain = System.Text.RegularExpressions.Regex.Replace(currentStore.Settings.CookieDomain, "[^A-Za-z0-9]", "");
                            cookieName = cookieName + domain;
                        }

                        if (currentStore.Settings.CookiePath != string.Empty)
                        {
                            string path = System.Text.RegularExpressions.Regex.Replace(currentStore.Settings.CookiePath, "[^A-Za-z0-9]", "");
                            cookieName = cookieName + path;
                        }

                        System.Web.HttpCookie saveCookie = new System.Web.HttpCookie(cookieName, value);
                        if (currentStore.Settings.CookieDomain.Trim() != string.Empty)
                        {
                            saveCookie.Domain = currentStore.Settings.CookieDomain;
                        }
                        if (currentStore.Settings.CookiePath.Trim() != string.Empty)
                        {
                            saveCookie.Path = currentStore.Settings.CookiePath;
                        }
                        saveCookie.Expires = expirationDate;
                        saveCookie.Secure = secure;
                        HttpContext.Current.Response.Cookies.Add(saveCookie);
                    }
                    catch (Exception Ex)
                    {
                        EventLog.LogEvent(Ex);
                    }
                }
            }
        }
        #endregion

        public static void SaveOrderCookies(Orders.Order o, Store currentStore)
        {            
            if (o.IsPlaced)
            {
                // Clear Cookies
                SetCookieString(WebAppSettings.CookieNameCartId(currentStore.Id), string.Empty, currentStore);
                SetCookieString(WebAppSettings.CookieNameCartItemCount(currentStore.Id), "0", currentStore);
                SetCookieString(WebAppSettings.CookieNameCartSubTotal(currentStore.Id), "0", currentStore);
            }
            else
            {
                // Save Cart Cookie
                SetCookieString(WebAppSettings.CookieNameCartId(currentStore.Id), o.bvin, currentStore);
                SetCookieString(WebAppSettings.CookieNameCartItemCount(currentStore.Id), Math.Round(o.TotalQuantity, 0, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode).ToString(), currentStore);
                SetCookieString(WebAppSettings.CookieNameCartSubTotal(currentStore.Id), o.TotalOrderAfterDiscounts.ToString("c"), currentStore);
            }
        }

        public static string GetCurrentCartID(Store currentStore)
        {
                string result = string.Empty;
                result = GetCookieString(WebAppSettings.CookieNameCartId(currentStore.Id), currentStore);            

                return result;
        }
        public static void SetCurrentCartId(Store currentStore, string value)
        {            
                SetCookieString(WebAppSettings.CookieNameCartId(currentStore.Id), value, currentStore);            
        }
        public static string GetCurrentPaymentPendingCartId(Store currentStore)
        {
                return GetCookieString(WebAppSettings.CookieNameCartIdPaymentPending(currentStore.Id), currentStore);                                    
        }
        public static void SetCurrentPaymentPendingCartId(Store currentStore, string value)
        {
                SetCookieString(WebAppSettings.CookieNameCartIdPaymentPending(currentStore.Id), value, currentStore, DateTime.Now.AddDays(14), true);                
        }

        public static void SetCurrentAffiliateId(long id, DateTime expirationDate, Store currentStore)
        {
            SetCookieString(WebAppSettings.CustomerIdCookieName + "Referrer", id.ToString(), currentStore, expirationDate, false);
        }
        public static long CurrentAffiliateID(Store currentStore)
        {
                string temp = GetCookieString(WebAppSettings.CustomerIdCookieName + "Referrer", currentStore);
                long result = 0;
                long.TryParse(temp, out result);
                return result;
        }

        public static bool CurrentUserHasCart(Store currentStore)
        {
            bool result = false;
            if (GetCurrentCartID(currentStore) == string.Empty)
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static Orders.Order CurrentShoppingCart(Orders.OrderService svc, Store currentStore)
        {
            Orders.Order result = null;

            if (CurrentUserHasCart(currentStore))
            {
                Orders.Order cachedCart = SessionManager.CachedShoppingCart;
                if (cachedCart != null)
                {
                    return cachedCart;
                }
                else
                {
                    result = svc.Orders.FindForCurrentStore(GetCurrentCartID(currentStore));
                    if (result != null)
                    {
                        if (!result.IsPlaced)
                        {
                            if (result.bvin != string.Empty)
                            {
                                SessionManager.CachedShoppingCart = result;
                                return result;
                            }
                        }
                    }
                }
            }

            result = new Orders.Order();
            svc.Orders.Upsert(result);
            SetCurrentCartId(currentStore, result.bvin);
            SessionManager.CachedShoppingCart = result;
            return result;
        }

        public static void InvalidateCachedCart()
        {
            CachedShoppingCart = null;
        }


        public static string GetCurrentUserId(Store currentStore, HttpCookieCollection cookies)
        {
            string result = string.Empty;
            result = GetCookieString(WebAppSettings.CookieNameAuthenticationTokenCustomer(currentStore.Id), currentStore, cookies);
            return result;
        }

    }
}