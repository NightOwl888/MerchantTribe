using System;
using System.Web;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MerchantTribe.Commerce.Accounts;
using MerchantTribe.Commerce.Catalog;
using MerchantTribe.Commerce.Contacts;
using MerchantTribe.Commerce.Content;
using MerchantTribe.Commerce.Orders;
using MerchantTribe.Commerce.Membership;
using MerchantTribe.Web.Logging;

namespace MerchantTribe.Commerce
{
    public class MerchantTribeApplication
    {

        private MerchantTribe.Commerce.RequestContext _CurrentRequestContext = new MerchantTribe.Commerce.RequestContext();
        private bool _IsForMemoryOnly = false;

        private string _CurrentRelativeRoot = string.Empty;


#region " Static Access to Current MerchantTribeApplication "

        /// <summary>
        /// Provides global access to the current <see cref="MerchantTribeApplication"/> instance.
        /// </summary>
        /// <remarks></remarks>
        public static MerchantTribeApplication Current
        {
            get
            {
                MerchantTribeApplication result = GetCurrentMerchantTribeApplication();
                if (result == null)
                {
                    // If this is not a web application, it is for memory only
                    bool isForMemoryOnly = (HttpContext.Current == null);
                    result = new MerchantTribeApplication(new RequestContext(), isForMemoryOnly);
                    SetCurrentMerchantTribeApplication(result);

                    //throw new System.NullReferenceException("The global MerchantTribeApplication must be set before accessing any members. " + 
                    //    "Common usage: MerchantTribeApplication.Current = MerchantTribeApplication.InstantiateForXXX(new RequestContext());");
                }
                return result;
            }
            set
            {
                SetCurrentMerchantTribeApplication(value);
            }
        }

        private const string _MerchantTribeApplicationContextName = "MerchantTribe.Commerce.MerchantTribeApplication";

        /// <summary>
        /// Gets a copy of the global instance of <see cref="MerchantTribeApplication"/>.
        /// </summary>
        /// <remarks>
        /// In a web project, the instance will be retrieved from HttpContext.Current.
        /// In a Windows project (such as a unit test) the instance will be retrieved from 
        /// the current Thread.
        /// 
        /// The SetCurrentMerchantTribeApplication() method must be called before 
        /// </remarks>
        /// <returns>The global instance of <see cref="MerchantTribeApplication"/>.</returns>
        private static MerchantTribeApplication GetCurrentMerchantTribeApplication()
        {
            if (HttpContext.Current != null)
            {
                return (MerchantTribeApplication)HttpContext.Current.Items[_MerchantTribeApplicationContextName];
            }
            else
            {
                System.LocalDataStoreSlot slot = Thread.GetNamedDataSlot(_MerchantTribeApplicationContextName);
                return (MerchantTribeApplication)Thread.GetData(slot);
            }
        }

        /// <summary>
        /// Sets the global instance of <see cref="MerchantTribeApplication"/>.
        /// </summary>
        /// <remarks>
        /// In a web project, the instance will be stored in HttpContext.Current.
        /// In a Windows project (such as a unit test) the instance will be stored in 
        /// the current Thread.
        /// </remarks>
        /// <param name="app">An instance of <see cref="MerchantTribeApplication"/>.</param>
        private static void SetCurrentMerchantTribeApplication(MerchantTribeApplication app)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[_MerchantTribeApplicationContextName] = app;
            }
            else
            {
                System.LocalDataStoreSlot slot = Thread.GetNamedDataSlot(_MerchantTribeApplicationContextName);
                Thread.SetData(slot, app);
            }
        }

#endregion


        public string CurrentRelativeRoot { 
            get {
                if (_CurrentRelativeRoot.Trim() == string.Empty)
                {
                    if (this.CurrentRequestContext != null)
                    {
                        if (this.CurrentRequestContext.RoutingContext != null)
                        {
                            try
                            {
                                System.Web.Mvc.UrlHelper helper = new System.Web.Mvc.UrlHelper(this.CurrentRequestContext.RoutingContext);
                                if (helper != null)
                                {
                                    this._CurrentRelativeRoot = helper.Content("~");
                                }
                            }
                            catch
                            {
                                this._CurrentRelativeRoot = string.Empty;
                            }
                        }
                    }
                }                
                return _CurrentRelativeRoot;                
            } 
            private set{ _CurrentRelativeRoot = value;}         
        }
        public string StoreUrl(bool forceSecure, bool forceNonSecure)
        {
            string result = this.CurrentRelativeRoot;

            // Force non-SSL if we've turned off SSL
            bool useSSL = this.CurrentStore.Settings.UseSSL;
            if (!useSSL) forceNonSecure = true;

            if (this.CurrentStore != null)
            {
                if (this.CurrentStore.Settings.ForceDomains)
                {

                    if (forceSecure && useSSL)
                    {
                        result = this.CurrentStore.RootUrlSecure();
                    }
                    else
                    {
                        result = this.CurrentStore.RootUrl();
                    }
                }
                else
                {
                    // not forcing domains so build off of relative
                    if (forceSecure && useSSL)
                    {
                        if (result.StartsWith("http://"))
                        {
                            result = result.Replace("http://", "https://");
                        }
                    }
                    if (forceNonSecure)
                    {
                        if (result.StartsWith("https://"))
                        {
                            result = result.Replace("https://", "http://");
                        }
                    }
                }
            }
            if (result.EndsWith("/") == false)
            {
                result += "/";
            }
            return result;
        }

        private AccountService _AccountService = null;
        public AccountService AccountServices
        {
            get
            {
                if (_AccountService == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _AccountService = AccountService.InstantiateForMemory(_CurrentRequestContext);
                    }
                    else
                    {
                        _AccountService = AccountService.InstantiateForDatabase(_CurrentRequestContext);
                    }
                }
                return _AccountService;
            }
        }
        private CatalogService _CatalogService = null;
        public CatalogService CatalogServices
        {
            get
            {
                if (_CatalogService == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _CatalogService = CatalogService.InstantiateForMemory(_CurrentRequestContext);
                    }
                    else
                    {
                        _CatalogService = CatalogService.InstantiateForDatabase(_CurrentRequestContext);
                    }
                }
                return _CatalogService;
            }
        }
        private ContactService _ContactServices = null;
        public ContactService ContactServices
        {
            get
            {
                if (_ContactServices == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _ContactServices = ContactService.InstantiateForMemory(_CurrentRequestContext);
                    }
                    else
                    {
                        _ContactServices = ContactService.InstantiateForDatabase(_CurrentRequestContext);
                    }
                }
                return _ContactServices;
            }
        }
        private ContentService _ContentServices = null;
        public ContentService ContentServices
        {
            get
            {
                if (_ContentServices == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _ContentServices = ContentService.InstantiateForMemory(_CurrentRequestContext);
                    }
                    else
                    {
                        _ContentServices = ContentService.InstantiateForDatabase(_CurrentRequestContext);
                    }
                }
                return _ContentServices;
            }
        }
        private OrderService _OrderServices = null;
        public OrderService OrderServices
        {
            get
            {
                if (_OrderServices == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _OrderServices = OrderService.InstantiateForMemory(_CurrentRequestContext);
                    }
                    else
                    {
                        _OrderServices = OrderService.InstantiateForDatabase(_CurrentRequestContext);
                    }
                }
                return _OrderServices;
            }
        }
        private Marketing.MarketingService _MarketingServices = null;
        public Marketing.MarketingService MarketingServices
        {
            get
            {
                if (_MarketingServices == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _MarketingServices = Marketing.MarketingService.InstantiateForMemory(_CurrentRequestContext);
                    }
                    else
                    {
                        _MarketingServices = Marketing.MarketingService.InstantiateForDatabase(_CurrentRequestContext);
                    }
                }
                return _MarketingServices;
            }
        }
        private Scheduling.ScheduleService _ScheduleServices = null;
        public Scheduling.ScheduleService ScheduleServices
        {
            get
            {
                if (_ScheduleServices == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _ScheduleServices = Scheduling.ScheduleService.InstantiateForMemory(_CurrentRequestContext);
                    }
                    else
                    {
                        _ScheduleServices = Scheduling.ScheduleService.InstantiateForDatabase(_CurrentRequestContext);
                    }
                }
                return _ScheduleServices;
            }
        }
        private Membership.MembershipServices _MembershipServices = null;
        public Membership.MembershipServices MembershipServices
        {
            get
            {
                if (_MembershipServices == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _MembershipServices = Membership.MembershipServices.InstantiateForMemory(_CurrentRequestContext);
                    }
                    else
                    {
                        _MembershipServices = Membership.MembershipServices.InstantiateForDatabase(_CurrentRequestContext);
                    }
                }
                return _MembershipServices;
            }
        }
        private Metrics.MetricsServices _MetricsServices = null;
        public Metrics.MetricsServices MetricsSerices
        {
            get
            {
                if (_MetricsServices == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _MetricsServices = Metrics.MetricsServices.InstantiateForMemory(_CurrentRequestContext);
                    }
                    else
                    {
                        _MetricsServices = Metrics.MetricsServices.InstantiateForDatabase(_CurrentRequestContext);
                    }
                }
                return _MetricsServices;
            }
        }
        private CustomerPointsManager _CustomerPointsManager = null;
        public CustomerPointsManager CustomerPointsManager
        {
            get
            {
                if (_CustomerPointsManager == null)
                {
                    if (_IsForMemoryOnly)
                    {
                        _CustomerPointsManager = CustomerPointsManager.InstantiateForMemory(
                                CurrentStore.Settings.RewardsPointsIssuedPerDollarSpent,
                                CurrentStore.Settings.RewardsPointsNeededPerDollarCredit,
                                CurrentStore.Id);
                    }
                    else
                    {
                        _CustomerPointsManager = CustomerPointsManager.InstantiateForDatabase(
                                CurrentStore.Settings.RewardsPointsIssuedPerDollarSpent,
                                CurrentStore.Settings.RewardsPointsNeededPerDollarCredit,
                                CurrentStore.Id);
                    }
                }
                return _CustomerPointsManager;
            }
        }

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

        public static MerchantTribeApplication InstantiateForMemory(RequestContext c)
        {
            return new MerchantTribeApplication(c, true);
        }
        public static MerchantTribeApplication InstantiateForDataBase(RequestContext c)
        {
            return new MerchantTribeApplication(c, false);
        }
        public MerchantTribeApplication(RequestContext c)
        {
            this._IsForMemoryOnly = false;
            this.CurrentRequestContext = c;
        }
        public MerchantTribeApplication(RequestContext c, bool isForMemoryOnly)
        {
            this.CurrentRequestContext = c;
            this._IsForMemoryOnly = isForMemoryOnly;
        }

        public string CurrentCustomerId
        {
            get
            {
                return SessionManager.GetCurrentUserId(this.CurrentStore, CurrentRequestContext.RoutingContext.HttpContext.Request.Cookies);
            }
        }
        public Membership.CustomerAccount CurrentCustomer
        {
            get
            {
                Membership.CustomerAccount c = MembershipServices.Customers.Find(this.CurrentCustomerId);
                if (c == null) c = new Membership.CustomerAccount();
                return c;
            }
        }

        public bool UpdateCurrentStore()
        {
            return AccountServices.Stores.Update(CurrentStore);
        }
        public bool CalculateOrder(Order o)
        {
            Orders.OrderCalculator calc = new OrderCalculator(this);
            return calc.Calculate(o);
        }
        public bool CalculateOrderWithoutRepricing(Order o)
        {
            Orders.OrderCalculator calc = new OrderCalculator(this);
            calc.SkipRepricing = true;
            return calc.Calculate(o);
        }
        public bool CalculateOrderAndSave(Order o)
        {
            if (!CalculateOrder(o)) return false;
            return OrderServices.Orders.Upsert(o);
        }
        public bool CalculateOrderAndSaveWithoutRepricing(Order o)
        {
            if (!CalculateOrderWithoutRepricing(o)) return false;
            return OrderServices.Orders.Upsert(o);
        }
        public bool AddToOrderWithCalculateAndSave(Order o, LineItem li)
        {
            o.Items.Add(li);
            CalculateOrder(o);
            return OrderServices.Orders.Upsert(o);
        }

        private class ProductIdCombo
        {
            public string Bvin = string.Empty;
            public string VariantId = string.Empty;
            public int Quantity = 0;
            public string Key()
            {
                return Bvin + VariantId;
            }
        }
        public SystemOperationResult CheckForStockOnItems(Order o)
        {

            // Build a list of product quantities to check
            Dictionary<string, ProductIdCombo> products = new Dictionary<string, ProductIdCombo>();
            foreach (Orders.LineItem item in o.Items)
            {
                ProductIdCombo combo = new ProductIdCombo() { Bvin = item.ProductId, VariantId = item.VariantId, Quantity = item.Quantity };

                if (products.ContainsKey(combo.Key()))
                {
                    products[combo.Key()].Quantity += combo.Quantity;
                }
                else
                {
                    products.Add(combo.Key(), combo);
                }
            }

            // Now check each quantity for the order
            foreach (string key in products.Keys)
            {
                Catalog.Product prod = null;
                Orders.LineItem lineItem = null;
                foreach (LineItem item in o.Items)
                {
                    if (item.ProductId + item.VariantId == key)
                    {
                        //we just need this to get the product 
                        //name if the product is not found
                        lineItem = item;
                        prod = item.GetAssociatedProduct(this);
                        break;
                    }
                }

                if (prod != null)
                {
                    if (prod.Status == Catalog.ProductStatus.Disabled)
                    {
                        return new SystemOperationResult(false, Content.SiteTerms.ReplaceTermVariable(Content.SiteTerms.GetTerm(Content.SiteTermIds.ProductNotAvailable), "productName", prod.ProductName));
                    }

                    ProductIdCombo checkcombo = products[key];

                    Catalog.InventoryCheckData data = CatalogServices.InventoryCheck(prod, checkcombo.VariantId);

                    if (data.IsAvailableForSale)
                    {
                        if (data.IsInStock)
                        {
                            if (data.Qty < checkcombo.Quantity)
                            {
                                string message = Content.SiteTerms.GetTerm(Content.SiteTermIds.CartNotEnoughQuantity);
                                message = Content.SiteTerms.ReplaceTermVariable(message, "ProductName", prod.ProductName);
                                message = Content.SiteTerms.ReplaceTermVariable(message, "Quantity", data.Qty.ToString());
                                return new SystemOperationResult(false, message);
                            }
                        }
                    }
                    else
                    {
                        return new SystemOperationResult(false, Content.SiteTerms.ReplaceTermVariable(Content.SiteTerms.GetTerm(Content.SiteTermIds.CartOutOfStock), "productName", prod.ProductName));
                    }
                }
                else
                {
                    return new SystemOperationResult(false, Content.SiteTerms.ReplaceTermVariable("{productname} is not availble at the moment.", "productName", lineItem.ProductName));
                }
            }

            return new SystemOperationResult(true, string.Empty);
        }


        // Product Pricing
        public UserSpecificPrice PriceProduct(string productBvin, string userId, OptionSelectionList selections)
        {
            Product p = CatalogServices.Products.Find(productBvin);
            Membership.CustomerAccount customer = MembershipServices.Customers.Find(userId);
            return PriceProduct(p, customer, selections);
        }
        public UserSpecificPrice PriceProduct(Catalog.Product p, Membership.CustomerAccount currentUser, OptionSelectionList selections)
        {
            if (p == null) return null;
            UserSpecificPrice result = new UserSpecificPrice(p, selections);
            AdjustProductPriceForUser(result, p, currentUser);
            ApplySales(result, p, currentUser);
            CheckForPricesBelowZero(result);

            return result;
        }
        private void ApplySales(UserSpecificPrice price, Catalog.Product p, Membership.CustomerAccount currentUser)
        {
            List<Marketing.Promotion> sales = MarketingServices.Promotions.FindAllPotentiallyActiveSales(DateTime.UtcNow);
            foreach (Marketing.Promotion promo in sales)
            {
                promo.ApplyToProduct(this, p, price, currentUser, DateTime.UtcNow);
            }
        }
        private void AdjustProductPriceForUser(UserSpecificPrice price, Catalog.Product p, Membership.CustomerAccount currentUser)
        {
            if (currentUser == null) return;
            if (currentUser.Bvin == string.Empty) return;
            if (currentUser.PricingGroupId == string.Empty) return;
            if (p == null) return;
            if (price == null) return;

            decimal startingPrice = price.PriceWithAdjustments();

            Contacts.PriceGroup pricingGroup = ContactServices.PriceGroups.Find(currentUser.PricingGroupId);
            if (pricingGroup == null) return;

            decimal nonGroupPrice = price.BasePrice;

            decimal groupPrice = nonGroupPrice;
            groupPrice = pricingGroup.GetAdjustedPriceForThisGroup(p.SitePrice, p.ListPrice, p.SiteCost);

            decimal amountOfDiscount = groupPrice - nonGroupPrice;
            price.AddAdjustment(amountOfDiscount, "Price Group");
        }
        private void CheckForPricesBelowZero(UserSpecificPrice price)
        {
            decimal final = price.PriceWithAdjustments();

            if (final < 0)
            {
                decimal tweak = -1 * final;
                price.AddAdjustment(tweak, "Price can not be below zero");
            }
        }


        // Orders
        public bool OrdersReserveInventoryForAllItems(Orders.Order o, List<string> errors)
        {
            bool result = true;
            foreach (LineItem li in o.Items)
            {

                decimal amountReserved = CatalogServices.InventoryLineItemReserveInventory(li);
                if (amountReserved != li.Quantity)
                {
                    if (errors != null)
                    {
                        errors.Add("Item " + li.ProductName + " did not have enough quantity to complete order.");
                    }
                    EventLog.LogEvent("Reserve Inventory For All Items", "Unable to reserve quantity of " + li.Quantity + " for product " + li.ProductId, EventLogSeverity.Debug);
                    result = false;
                }
            }
            return result;
        }
        public bool OrdersUnreserveInventoryForAllItems(Orders.Order o)
        {
            bool result = true;
            foreach (LineItem li in o.Items)
            {
                if (CatalogServices.InventoryLineItemUnreserveInventory(li) == false)
                {
                    EventLog.LogEvent("Unreserve Inventory For All Items", "Unable to unreserve quantity of " + li.Quantity + " for product " + li.ProductId, EventLogSeverity.Debug);
                }
            }
            return result;
        }

        // Order Packages
        public bool OrdersShipAllPackages(Order o)
        {
            foreach (OrderPackage p in o.Packages)
            {
                if (!p.HasShipped)
                {
                    OrdersShipPackage(p, o);
                }
            }
            OrderServices.Orders.Update(o);
            return true;
        }
        public bool OrdersShipPackage(OrderPackage p, Order o)
        {
            p.HasShipped = true;
            OrdersShipItems(p, o);
            BusinessRules.OrderTaskContext c = new BusinessRules.OrderTaskContext(this);
            c.Order = o;
            c.UserId = this.CurrentCustomerId;
            if (!BusinessRules.Workflow.RunByName(c, BusinessRules.WorkflowNames.PackageShipped))
            {
                EventLog.LogEvent("PackageShippedWorkflow", "Package Shipped Workflow Failed", EventLogSeverity.Debug);
                foreach (BusinessRules.WorkflowMessage item in c.Errors)
                {
                    EventLog.LogEvent("PackageShippedWorkflow", item.Description, EventLogSeverity.Debug);
                }
            }
            return true;
        }
        private bool OrdersShipItems(OrderPackage p, Order o)
        {
            bool result = true;

            if (p.Items.Count > 0)
            {
                if (o != null)
                {
                    if (o.bvin != string.Empty)
                    {
                        foreach (OrderPackageItem pi in p.Items)
                        {
                            if (pi.LineItemId > 0)
                            {
                                Orders.LineItem li = o.GetLineItem(pi.LineItemId);
                                if (li != null)
                                {
                                    if (pi.Quantity > (li.Quantity - li.QuantityShipped))
                                    {
                                        pi.Quantity = li.Quantity - li.QuantityShipped;
                                    }

                                    if (pi.Quantity <= 0)
                                    {
                                        pi.Quantity = 0;
                                    }
                                    else
                                    {
                                        CatalogServices.InventoryLineItemShipQuantity(li, pi.Quantity);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
        private bool OrdersUnshipItems(OrderPackage p, Order o)
        {
            bool result = true;

            if (p.Items.Count > 0)
            {
                if (o != null)
                {
                    if (o.bvin != string.Empty)
                    {
                        foreach (OrderPackageItem pi in p.Items)
                        {
                            if (pi.LineItemId > 0)
                            {
                                Orders.LineItem li = o.GetLineItem(pi.LineItemId);
                                if (li != null)
                                {
                                    int quantityToUnship = 0;
                                    if (li.QuantityShipped < pi.Quantity)
                                    {
                                        quantityToUnship = li.QuantityShipped;
                                    }
                                    else
                                    {
                                        quantityToUnship = pi.Quantity;
                                    }
                                    CatalogServices.InventoryLineItemUnShipQuantity(li, quantityToUnship);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
        public decimal OrderPackagesFindDeclaredValue(OrderPackage p, Order o)
        {
            decimal result = 0m;
            if (o != null)
            {
                foreach (OrderPackageItem pi in p.Items)
                {
                    foreach (Orders.LineItem li in o.Items)
                    {
                        if (li.Id == pi.LineItemId)
                        {
                            decimal lineValue = 0m;
                            lineValue = li.AdjustedPricePerItem * pi.Quantity;
                            result += lineValue;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        // Sample Data
        public void AddSampleProductsToStore()
        {
            List<Catalog.Category> cats = CreateSampleCategories();
            CreateSampleProducts(cats);
        }
        public void RemoveSampleProductsFromStore()
        {
            DeleteSampleCategoryBySlug("sample-products");
            DeleteSampleCategoryBySlug("more-sample");
            DeleteSampleCategoryBySlug("demo-category");
            DeleteSampleCategoryBySlug("about-us-sample");
            DeleteSampleProduct("SAMPLE001");
            DeleteSampleProduct("SAMPLE002");
            DeleteSampleProduct("SAMPLE003");
            DeleteSampleProduct("SAMPLE004");
            DeleteSampleProduct("SAMPLE005");
            DeleteSampleProduct("SAMPLE006");
        }
        private void DeleteSampleCategoryBySlug(string slug)
        {
            Catalog.Category c = CatalogServices.Categories.FindBySlug(slug);
            if (c != null) DestroyCategory(c.Bvin);
        }
        private void DeleteSampleProduct(string sku)
        {
            Catalog.Product p = CatalogServices.Products.FindBySku(sku);
            if (p != null) DestroyProduct(p.Bvin);
        }
        private List<Catalog.Category> CreateSampleCategories()
        {
            List<Catalog.Category> samples = new List<Category>();
            samples.Add(CreateSampleCategory("Sample Products", "sample-products", "This is a sample category", "Grid"));
            samples.Add(CreateSampleCategory("More Sample Products", "more-sample", "", "DetailedList"));
            samples.Add(CreateSampleCategory("Demo Category", "demo-category", "", "BulkQuantityList"));
            samples.Add(CreateSampleCategoryPage("About Us", "about-us-sample", "<h1>About Us</h1> <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.</p> <hr /> <p> <b>Mailing Address:</b><br /> {{storeaddress}} <p>"));
            return samples;
        }
        private Catalog.Category CreateSampleCategory(string name, string url, string description, string template)
        {
            Catalog.Category result = new Category();
            result.Name = name;
            result.RewriteUrl = url;
            result.Description = description;
            result.MetaTitle = MerchantTribe.Web.Text.TrimToLength(name, 250);
            result.MetaDescription = MerchantTribe.Web.Text.TrimToLength(description, 250);
            result.Keywords = MerchantTribe.Web.Text.TrimToLength(name, 250);
            result.ShowTitle = true;
            result.ShowInTopMenu = true;
            result.TemplateName = template;
            CatalogServices.Categories.Create(result);
            return result;
        }
        private Catalog.Category CreateSampleCategoryPage(string name, string url, string description)
        {
            Catalog.Category result = new Category();
            result.CustomPageLayout = CustomPageLayoutType.WithSideBar;
            result.SourceType = CategorySourceType.CustomPage;
            result.Name = name;
            result.RewriteUrl = url;
            result.Description = description;
            result.MetaTitle = MerchantTribe.Web.Text.TrimToLength(name, 250);
            result.MetaDescription = MerchantTribe.Web.Text.TrimToLength(description, 250);
            result.Keywords = MerchantTribe.Web.Text.TrimToLength(name, 250);
            result.ShowTitle = true;
            result.ShowInTopMenu = true;
            CatalogServices.Categories.Create(result);
            return result;
        }
        private void CreateSampleProducts(List<Catalog.Category> categories)
        {
            Catalog.Product sample1 = CreateBlueBracelet();
            Catalog.Product sample2 = CreateBrownFedora();
            Catalog.Product sample3 = CreateButterflyEarings();
            Catalog.Product sample4 = CreateCupCake();
            Catalog.Product sample5 = CreateLaptop();
            Catalog.Product sample6 = CreateShirt();

            AssignToCategories(categories, sample1);
            AssignToCategories(categories, sample2);
            AssignToCategories(categories, sample3);
            AssignToCategories(categories, sample4);
            AssignToCategories(categories, sample5);
            AssignToCategories(categories, sample6);
        }
        private void AssignToCategories(List<Catalog.Category> cats, Product p)
        {
            foreach (Catalog.Category c in cats)
            {
                CatalogServices.CategoriesXProducts.AddProductToCategory(p.Bvin, c.Bvin);
            }
        }
        private Catalog.Product CreateBlueBracelet()
        {
            Catalog.Product p = new Product();
            p.Sku = "SAMPLE001";
            p.ProductName = "Blue Bracelet";
            p.AllowReviews = true;
            p.Featured = true;
            p.ImageFileSmall = "BraceletBlue.png";
            p.ImageFileSmallAlternateText = "Blue Bracelet SAMPLE001";
            p.InventoryMode = ProductInventoryMode.AlwayInStock;
            p.Keywords = "bracelett";
            p.ListPrice = 0;
            p.LongDescription = "An incredible blue bracelet sample product. This item is not actually for sale. It is a sample product to demonstrate how your store may look with products loaded";
            p.MetaDescription = "Sample Blue Bracelet for Demonstration";
            p.MetaKeywords = "Blue,Bracelet,Sample,Demo";
            p.MetaTitle = "Sample Blue Bracelet";
            p.SitePrice = 42.95m;
            p.Status = ProductStatus.Active;
            p.UrlSlug = "blue-bracelet";
            p.Tabs.Add(new ProductDescriptionTab() { TabTitle = "Sustainability", HtmlData = "<p>All of our jewelry products are recycled and made in sustainable eco-friendly environments</p>" });
            CatalogServices.ProductsCreateWithInventory(p, true);

            Storage.DiskStorage.CopyDemoProductImage(p.StoreId, p.Bvin, p.ImageFileSmall);

            // Create Review
            ProductReview review = new ProductReview();
            review.Approved = true;
            review.Description = "This is a great bracelet. I would recommend it to anyone who appreciates sample products. Easy to wear!";
            review.ProductBvin = p.Bvin;
            review.Rating = ProductReviewRating.FourStars;
            CatalogServices.ProductReviews.Create(review);

            return p;
        }
        private Catalog.Product CreateBrownFedora()
        {
            Catalog.Product p = new Product();
            p.Sku = "SAMPLE004";
            p.ProductName = "Brown Fedora";
            p.AllowReviews = true;
            p.Featured = true;
            p.ImageFileSmall = "indiana-jones-hat.jpg";
            p.ImageFileSmallAlternateText = "Brown Fedora SAMPLE004";
            p.InventoryMode = ProductInventoryMode.AlwayInStock;
            p.LongDescription = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
            p.SitePrice = 59.87m;
            p.Status = ProductStatus.Active;
            p.UrlSlug = "brown-fedora";
            CatalogServices.ProductsCreateWithInventory(p, true);

            Storage.DiskStorage.CopyDemoProductImage(p.StoreId, p.Bvin, p.ImageFileSmall);

            return p;
        }
        private Catalog.Product CreateButterflyEarings()
        {
            Catalog.Product p = new Product();
            p.Sku = "SAMPLE006";
            p.ProductName = "Butterfly Earings";
            p.AllowReviews = true;
            p.Featured = true;
            p.ImageFileSmall = "Earrings.jpg";
            p.ImageFileSmallAlternateText = "Butterfly Earning SAMPLE006";
            p.InventoryMode = ProductInventoryMode.AlwayInStock;
            p.LongDescription = "Sample Butterfly Earings Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
            p.SitePrice = 29.95m;
            p.Status = ProductStatus.Active;
            p.UrlSlug = "butterfly-earings";
            CatalogServices.ProductsCreateWithInventory(p, true);

            Storage.DiskStorage.CopyDemoProductImage(p.StoreId, p.Bvin, p.ImageFileSmall);

            return p;
        }
        private Catalog.Product CreateCupCake()
        {
            Catalog.Product p = new Product();
            p.Sku = "SAMPLE002";
            p.ProductName = "Cup Cake Sample";
            p.AllowReviews = true;
            p.Featured = true;
            p.ImageFileSmall = "CupCake.jpg";
            p.ImageFileSmallAlternateText = "Cup Cake Sample SAMPLE002";
            p.InventoryMode = ProductInventoryMode.AlwayInStock;
            p.LongDescription = "Savor this sweet treat from our famous collection of sample items. This product is not for sale and is a demonstration of how items could appear in your store";
            p.MetaDescription = "Vanilla Cup Cake with Rich Frosting";
            p.MetaKeywords = "cup,cake,cupcake,valentine,small,treats,baked goods";
            p.MetaTitle = "Vanilla Cup Cake with Rich Frosting";
            p.SitePrice = 1.99m;
            p.Status = ProductStatus.Active;
            p.UrlSlug = "cup-cake-sample";
            CatalogServices.ProductsCreateWithInventory(p, true);

            Storage.DiskStorage.CopyDemoProductImage(p.StoreId, p.Bvin, p.ImageFileSmall);

            return p;
        }
        private Catalog.Product CreateLaptop()
        {
            Catalog.Product p = new Product();
            p.Sku = "SAMPLE005";
            p.ProductName = "Laptop Computer Sample";
            p.AllowReviews = true;
            p.Featured = true;
            p.ImageFileSmall = "Laptop.png";
            p.ImageFileSmallAlternateText = "Laptop Computer Sample SAMPLE005";
            p.InventoryMode = ProductInventoryMode.AlwayInStock;
            p.LongDescription = "This is a sample laptop computer. It is not for sale and is a demonstration of what products could look like in your store";
            p.MetaTitle = "Laptop Sample";
            p.ListPrice = 1999.00m;
            p.SitePrice = 1299.00m;
            p.Status = ProductStatus.Active;
            p.UrlSlug = "laptop-computer-sample";
            CatalogServices.ProductsCreateWithInventory(p, true);

            Option opt1 = Catalog.Option.Factory(OptionTypes.DropDownList);
            opt1.IsShared = false;
            opt1.IsVariant = true;
            opt1.Name = "Screen Size";
            opt1.NameIsHidden = false;
            CatalogServices.ProductOptions.Create(opt1);
            opt1.AddItem("15 inch LCD");
            opt1.AddItem("17 inch LCD");
            CatalogServices.ProductOptions.Update(opt1);
            CatalogServices.ProductsAddOption(p, opt1.Bvin);

            Option opt2 = Catalog.Option.Factory(OptionTypes.RadioButtonList);
            opt2.IsShared = false;
            opt2.IsVariant = true;
            opt2.Name = "Memory (RAM)";
            opt2.NameIsHidden = false;
            CatalogServices.ProductOptions.Create(opt2);
            opt2.AddItem("2GB");
            opt2.AddItem("4GB");
            opt2.AddItem("8GB");
            CatalogServices.ProductOptions.Update(opt2);
            CatalogServices.ProductsAddOption(p, opt2.Bvin);

            Option opt3 = Catalog.Option.Factory(OptionTypes.CheckBoxes);
            opt3.IsShared = false;
            opt3.IsVariant = false;
            opt3.Name = "Warranty";
            opt3.NameIsHidden = false;
            CatalogServices.ProductOptions.Create(opt3);
            opt3.AddItem(new OptionItem() { Name = "3 Year Extended Warranty [+$199]", PriceAdjustment = 199 });
            CatalogServices.ProductOptions.Update(opt3);
            CatalogServices.ProductsAddOption(p, opt3.Bvin);

            CatalogServices.VariantsGenerateAllPossible(p);

            Storage.DiskStorage.CopyDemoProductImage(p.StoreId, p.Bvin, p.ImageFileSmall);

            return p;
        }
        private Catalog.Product CreateShirt()
        {
            Catalog.Product p = new Product();
            p.Sku = "SAMPLE003";
            p.ProductName = "Purple Top";
            p.AllowReviews = true;
            p.Featured = true;
            p.ImageFileSmall = "PurpleTop.jpg";
            p.ImageFileSmallAlternateText = "Purple Top SAMPLE003";
            p.InventoryMode = ProductInventoryMode.AlwayInStock;
            p.LongDescription = "This sample purple top is made of 100% cotton and is made in the USA. Durable and easy to clean, this versatile top will be one of the best sample items not available for purchase you've ever seen!";
            p.MetaTitle = "Purple Top";
            p.SitePrice = 39.95m;
            p.Status = ProductStatus.Active;
            p.UrlSlug = "purple-top";
            CatalogServices.ProductsCreateWithInventory(p, true);

            Option opt1 = Catalog.Option.Factory(OptionTypes.DropDownList);
            opt1.IsShared = false;
            opt1.IsVariant = true;
            opt1.Name = "Size";
            opt1.NameIsHidden = false;
            CatalogServices.ProductOptions.Create(opt1);
            opt1.AddItem("Small");
            opt1.AddItem("Medium");
            opt1.AddItem("Large");
            CatalogServices.ProductOptions.Update(opt1);
            CatalogServices.ProductsAddOption(p, opt1.Bvin);

            Option opt2 = Catalog.Option.Factory(OptionTypes.DropDownList);
            opt2.IsShared = false;
            opt2.IsVariant = true;
            opt2.Name = "Sleeve Length";
            opt2.NameIsHidden = false;
            CatalogServices.ProductOptions.Create(opt2);
            opt2.AddItem("Short");
            opt2.AddItem("Long");
            CatalogServices.ProductOptions.Update(opt2);
            CatalogServices.ProductsAddOption(p, opt2.Bvin);

            CatalogServices.VariantsGenerateAllPossible(p);

            Storage.DiskStorage.CopyDemoProductImage(p.StoreId, p.Bvin, p.ImageFileSmall);

            return p;
        }

        //Reporting
        public List<Product> ReportingTop10Sellers()
        {
            List<Product> results = new List<Product>();
            
            DateTime startDateUtc = DateTime.UtcNow.AddMonths(-2);
            DateTime endDateUtc = DateTime.UtcNow;

            var idList = OrderServices.Orders.FindPopularItems(startDateUtc, endDateUtc, 10);                       
            var allKeys = idList.Select(y => y.Key).ToList();            
            results = CatalogServices.Products.FindMany(allKeys);

            return results;
        }
        public List<Product> ReportingTopSellersByDate(DateTime startDateUtc, DateTime endDateUtc, int maxResults)
        {
            List<Product> results = new List<Product>();
            
            var idList = OrderServices.Orders.FindPopularItems(startDateUtc, endDateUtc, maxResults);
            var allKeys = idList.Select(y => y.Key).ToList();
            results = CatalogServices.Products.FindMany(allKeys);

            return results;

            //// Find orders in the time period
            //List<Orders.OrderSnapshot> orders = OrderServices.Orders.FindByCriteria(new OrderSearchCriteria()
            //{
            //    EndDateUtc = endDateUtc,
            //    StartDateUtc = startDateUtc,
            //    IsPlaced = true
            //});

            //// Find items for those orders
            //List<Orders.LineItem> items = OrderServices.Orders.FindLineItemsForOrders(orders);

            //// Group quantities on items
            //Dictionary<string, int> totals = new Dictionary<string, int>();
            //foreach (LineItem li in items)
            //{
            //    if (totals.ContainsKey(li.ProductId))
            //    {
            //        totals[li.ProductId] += li.Quantity;
            //    }
            //    else
            //    {
            //        totals[li.ProductId] = li.Quantity;
            //    }
            //}
            //// Select just the top bvins
            //var productIds = totals.OrderByDescending(y => y.Value).Take(maxResults);
            //var allKeys = productIds.Select(y => y.Key).ToList();
            //// Get the products matching
            //results = CatalogServices.Products.FindMany(allKeys);

            //return results;
        }
        public List<ProductRelationship> GetAutoSuggestedRelatedItems(string bvin, int maxItemsToReturn)
        {
            List<ProductRelationship> relationships = new List<ProductRelationship>();

            long storeId = this.CurrentRequestContext.CurrentStore.Id;

            var db = new Data.EF.EntityFrameworkDevConnectionString(WebAppSettings.ApplicationConnectionStringForEntityFramework);

            var results = (from a in
                               (
                                   (from l in db.bvc_LineItem
                                    join o in db.bvc_Order on new { OrderBvin = l.OrderBvin } equals new { OrderBvin = o.bvin }
                                    join p in db.bvc_Product on new { bvin = l.ProductId } equals new { bvin = p.bvin }
                                    where
                                        o.IsPlaced == 1 &&
                                        p.IsAvailableForSale == true &&
                                        p.StoreId == storeId &&
                                        p.bvin != bvin &&

                                    (from bvc_lineitem in db.bvc_LineItem
                                     where
                                         bvc_lineitem.ProductId == bvin &&
                                         bvc_lineitem.StoreId == storeId
                                     select new
                                     {
                                         bvc_lineitem.OrderBvin
                                     }).Contains(new { l.OrderBvin })
                                    select new
                                    {
                                        ProductID = p.bvin,
                                        l.Quantity
                                    }))
                           group a by new
                           {
                               a.ProductID
                           } into g
                           orderby
                               g.Sum(p => p.Quantity) descending
                           select new
                           {
                               g.Key.ProductID,
                               Total_Ordered = g.Sum(p => p.Quantity)
                           }).Take(maxItemsToReturn);

            foreach (var r in results)
            {
                relationships.Add(new ProductRelationship()
                {
                    ProductId = bvin,
                    RelatedProductId = r.ProductID,
                    IsSubstitute = false,
                    StoreId = storeId
                });
            }

            return relationships;
        }

        // Cross Area Destroy Functions
        public bool DestroyCategory(string bvin)
        {
            if (CatalogServices.CategoriesXProducts.DeleteAllForCategory(bvin) == false) return false;

            // sales/offers

            // CustomUrl
            List<Content.CustomUrl> urls = ContentServices.CustomUrls.FindBySystemData(bvin);
            foreach (Content.CustomUrl u in urls)
            {
                ContentServices.CustomUrls.Delete(u.Bvin);
            }

            return CatalogServices.Categories.Delete(bvin);
        }
        public bool DestroyAllCategories()
        {
            DateTime current = DateTime.UtcNow;
            DateTime availableUntil = CurrentRequestContext.CurrentStore.Settings.AllowApiToClearUntil;
            int compareResult = DateTime.Compare(current, availableUntil);
            if (compareResult >= 0)
            {
                return false;
            }

            List<CategorySnapshot> all = CatalogServices.Categories.FindAll();
            foreach (CategorySnapshot snap in all)
            {
                DestroyCategory(snap.Bvin);
            }

            return true;
        }
        public bool DestroyCustomerAccount(string bvin)
        {
            CatalogServices.WishListItems.DeleteForCustomerId(bvin);
            ContactServices.Affiliates.DeleteAffiliateContactsForCustomer(bvin);
            return MembershipServices.Customers.Delete(bvin);
        }           
        public MerchantTribe.CommerceDTO.v1.Catalog.ClearProductsData ClearProducts(int howMany)
        {
            MerchantTribe.CommerceDTO.v1.Catalog.ClearProductsData result = new CommerceDTO.v1.Catalog.ClearProductsData();

            DateTime current = DateTime.UtcNow;
            DateTime availableUntil = CurrentRequestContext.CurrentStore.Settings.AllowApiToClearUntil;
            int compareResult = DateTime.Compare(current, availableUntil);
            if (compareResult >= 0)
            {
                result.ProductsCleared = 0;
                result.ProductsRemaining = -1;
                return result;
            }

            return DeleteSome(CurrentRequestContext.CurrentStore.Id, howMany);
        }

        private MerchantTribe.CommerceDTO.v1.Catalog.ClearProductsData DeleteSome(long storeId, int howMany)
        {

            List<Product> items = CatalogServices.Products.FindAllPaged(1, howMany);
            int totalCount = CatalogServices.Products.FindAllCount(storeId);
                       
            if (items != null)
            {
                foreach (Product p in items)
                {
                    this.DestroyProduct(p.Bvin);
                }
            }

            int left = totalCount - howMany;
            if (left < 0) left = 0;
            int cleared = howMany;
            if (totalCount < howMany) cleared = totalCount;
            MerchantTribe.CommerceDTO.v1.Catalog.ClearProductsData result = new CommerceDTO.v1.Catalog.ClearProductsData();
            result.ProductsCleared = cleared;
            result.ProductsRemaining = left;

            return result;
        }

        public bool DestroyProduct(string bvin)
        {
            // Category Assignments
            if (CatalogServices.CategoriesXProducts.DeleteAllForProduct(bvin) == false) return false;

            // WishList
            CatalogServices.WishListItems.DeleteForProductId(bvin);

            // Files   
            CatalogServices.ProductFiles.DeleteForProductId(bvin);

            // Volume Discounts        
            CatalogServices.VolumeDiscounts.DeleteByProductId(bvin);

            // Product Property Values
            CatalogServices.ProductPropertyValues.DeleteByProductId(bvin);

            // Inventory
            CatalogServices.ProductInventories.DeleteByProductId(bvin);

            // Images (included as sub repository)
            // Variants (included as sub repository)
            // Reviews (included as sub repository)            
            // Product Options (included as sub repository)            

            // Sales / Offers

            // Custom Url
            List<Content.CustomUrl> urls = ContentServices.CustomUrls.FindBySystemData(bvin);
            foreach (Content.CustomUrl u in urls)
            {
                ContentServices.CustomUrls.Delete(u.Bvin);
            }

            return CatalogServices.Products.Delete(bvin);
        }

        // Themes
        public MerchantTribe.Commerce.Content.ThemeManager ThemeManager()
        {
            return new Content.ThemeManager(this);
        }
        public bool SwitchTheme(string newThemeId)
        {
            bool result = true;
            string old = CurrentStore.Settings.ThemeId;
            if (old != newThemeId)
            {
                if (newThemeId != string.Empty)
                {
                    Content.ThemeManager tm = new Content.ThemeManager(this);
                    if (old != string.Empty)
                    {
                        tm.CopyCurrentContentColumnsToTheme(old);
                    }
                    tm.CopyColumnsFromThemeToStore(newThemeId);
                }
                CurrentStore.Settings.SetProp("ThemeId", newThemeId);
                _AccountService.Stores.Update(CurrentStore);
            }
            return result;
        }

        // Customer Affiliate Relationships
        public List<Membership.CustomerAccount> FindCustomersAssignedToAffiliate(long affiliateId)
        {
            var aff = ContactServices.Affiliates.Find(affiliateId);
            List<string> ids = new List<string>();
            foreach (var rel in aff.Contacts)
            {
                ids.Add(rel.UserId);
            }
            return MembershipServices.Customers.FindMany(ids);
        }


        // Flex Page Status
        public bool IsEditMode
        {
            get
            {
                bool result = false;
                if (SessionManager.GetCookieString("IsEditMode", this.CurrentStore) == "1")
                {
                    if (this.CurrentRequestContext.IsAdmin(this))
                    {
                        result = true;
                    }
                }
                return result;
            }
            set
            {
                if (value == true)
                {
                    SessionManager.SetCookieString("IsEditMode", "1", this.CurrentStore);
                }
                else
                {
                    SessionManager.SetCookieString("IsEditMode", "0", this.CurrentStore);
                }
            }
        }
        
    }
}
