using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using MerchantTribe.Commerce.Shipping;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Linq;
using MerchantTribe.CommerceDTO.v1.Orders;
using MerchantTribe.CommerceDTO.v1.Marketing;
using MerchantTribe.CommerceDTO.v1;

namespace MerchantTribe.Commerce.Orders
{
    public class Order : MerchantTribe.Web.Validation.IValidatable, Content.IReplaceable
    {
        // Sub Items
        public List<OrderCoupon> Coupons { get; set; }
        public List<LineItem> Items { get; set; }
        public List<OrderNote> Notes { get; set; }
        public List<OrderPackage> Packages { get; set; }

        // Basics
        public int Id { get; set; } // not used as primary key, only for insert order in SQL pages
        public string bvin { get; set; } // Primary Key
        public long StoreId { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
        public DateTime TimeOfOrderUtc { get; set; }
        public string OrderNumber { get; set; }
        public string ThirdPartyOrderId { get; set; }
        public string UserEmail { get; set; }
        public string UserID { get; set; }
        public CustomPropertyCollection CustomProperties { get; set; }

        // Status        
        public OrderPaymentStatus PaymentStatus { get; set; }
        public OrderShippingStatus ShippingStatus { get; set; }
        public bool IsPlaced { get; set; }
        public string StatusCode { get; set; }
        public string StatusName { get; set; }

        // Addresses
        public Contacts.Address BillingAddress { get; set; }
        public Contacts.Address ShippingAddress { get; set; }



        // Others
        public string AffiliateID { get; set; }
        public decimal FraudScore { get; set; }
        public string Instructions { get; set; }
        public string ShippingMethodId { get; set; }
        public string ShippingMethodDisplayName { get; set; }
        public string ShippingProviderId { get; set; }
        public string ShippingProviderServiceCode { get; set; }

        // CONSTRUCTOR
        public Order()
        {
            this.Coupons = new List<OrderCoupon>();
            this.Items = new List<LineItem>();
            this.Notes = new List<OrderNote>();
            this.Packages = new List<OrderPackage>();

            this.bvin = string.Empty;
            this.StoreId = 0;
            this.LastUpdatedUtc = DateTime.UtcNow;
            this.TimeOfOrderUtc = DateTime.UtcNow;
            this.OrderNumber = string.Empty;
            this.ThirdPartyOrderId = string.Empty;
            this.UserEmail = string.Empty;
            this.UserID = string.Empty;
            this.CustomProperties = new CustomPropertyCollection();

            this.PaymentStatus = OrderPaymentStatus.Unknown;
            this.ShippingStatus = OrderShippingStatus.Unknown;
            this.IsPlaced = false;
            this.StatusCode = string.Empty;
            this.StatusName = string.Empty;

            this.BillingAddress = new Contacts.Address();
            this.ShippingAddress = new Contacts.Address();

            this.TotalTax = 0m;
            this.TotalTax2 = 0m;
            this.TotalShippingBeforeDiscounts = 0m;
            this.ShippingDiscountDetails = new List<Marketing.DiscountDetail>();
            this.OrderDiscountDetails = new List<Marketing.DiscountDetail>();
            this.TotalHandling = 0m;

            this.AffiliateID = string.Empty;
            this.FraudScore = -1m;
            this.Instructions = string.Empty;
            this.ShippingMethodId = string.Empty;
            this.ShippingMethodDisplayName = string.Empty;
            this.ShippingProviderId = string.Empty;
            this.ShippingProviderServiceCode = string.Empty;
        }


        // Totals                        
        public decimal TotalTax { get; set; }
        public decimal TotalTax2 { get; set; }

        public decimal TotalShippingBeforeDiscountsOverride
        {
            get
            {
                decimal result = -1;
                string setting = this.CustomProperties.GetProperty("bvsoftware", "shippingoverride");
                if (setting.Trim().Length > 0)
                {
                    decimal.TryParse(setting, out result);
                }
                return result;
            }
            set
            {
                this.CustomProperties.SetProperty("bvsoftware", "shippingoverride", value.ToString());
            }
        }
        private decimal totalShippingBeforeDiscounts = 0;
        public decimal TotalShippingBeforeDiscounts { 
            get {
                decimal totalOverride = TotalShippingBeforeDiscountsOverride;
                if (totalOverride >= 0)
                {
                    return totalOverride;
                }
                return totalShippingBeforeDiscounts;
            } 
            set {totalShippingBeforeDiscounts = value;} 
        }

        public decimal TotalHandling { get; set; }

        public List<Marketing.DiscountDetail> OrderDiscountDetails { get; set; }
        public void AddOrderDiscount(decimal amount, string description)
        {
            AddOrderDiscount(new Marketing.DiscountDetail() { Amount = amount, Description = description });
        }
        public void AddOrderDiscount(Marketing.DiscountDetail discount)
        {
            this.OrderDiscountDetails.Add(discount);
        }
        public List<Marketing.DiscountDetail> ShippingDiscountDetails { get; set; }
        public void AddShippingDiscount(decimal amount, string description)
        {
            AddShippingDiscount(new Marketing.DiscountDetail() { Amount = amount, Description = description });
        }
        public void AddShippingDiscount(Marketing.DiscountDetail discount)
        {
            this.ShippingDiscountDetails.Add(discount);
        }

        // Calculated Properties                
        public decimal TotalOrderBeforeDiscounts
        {
            get
            {                
                return this.Items.Sum(y => y.LineTotal);
            }
        }
        public decimal TotalOrderDiscounts
        {
            get
            {
                decimal result = 0m;
                if (this.OrderDiscountDetails.Count > 0)
                {
                    result = this.OrderDiscountDetails.Sum(y => y.Amount);
                }
                return result;
            }
        }
        public decimal TotalOrderAfterDiscounts
        {
            get { return TotalOrderBeforeDiscounts + TotalOrderDiscounts; }
        }
        public decimal TotalShippingDiscounts { 
            get
            {
                decimal result = 0m;
                if (this.ShippingDiscountDetails.Count > 0)
                {
                    result = this.ShippingDiscountDetails.Sum(y => y.Amount);
                }
                return result;
            }         
        }
        public decimal TotalGrand
        {
            get
            {
                return this.TotalOrderAfterDiscounts +
                        this.TotalShippingAfterDiscounts +
                        this.TotalTax +
                        this.TotalTax2;
            }
        }
        public decimal TotalGrandAfterStoreCredits(OrderService orderService)
        {
            List<OrderTransaction> transaction = orderService.Transactions.FindForOrder(this.bvin);
            decimal potentialCredits = orderService.Transactions.TransactionsPotentialStoreCredits(transaction);
            return (TotalGrand - potentialCredits);
        }
        public string ShippingMethodUniqueKey
        {
            get { return this.ShippingMethodId + this.ShippingProviderId + this.ShippingProviderServiceCode; }
        }
        public bool HasShippingItems
        {
            get { return DetermineIfAnyItemsAreShipping(); }
        }
        public decimal TotalQuantity
        {
            get
            {
                decimal result = 0m;
                foreach (LineItem li in Items)
                {
                    result += li.Quantity;
                }
                return result;
            }
        }
        public decimal TotalQuantityShipping
        {
            get
            {
                decimal result = 0m;
                foreach (LineItem li in Items)
                {
                    if (li.ShippingSchedule > -1)
                    {
                        result += li.Quantity;
                    }
                    else
                    {
                        result += li.Quantity;
                    }
                }
                return result;
            }
        }
        public decimal TotalWeight
        {
            get
            {
                decimal result = 0m;
                foreach (LineItem li in Items)
                {
                    result += li.GetTotalWeight();
                }
                return result;
            }
        }
        public decimal TotalShippingAfterDiscounts
        {
            get { return TotalShippingBeforeDiscounts + TotalShippingDiscounts; }
        }
        public decimal SubTotalOfShippingItems()
        {
            decimal result = 0;

            for (int i = 0; i <= Items.Count - 1; i++)
            {
                if (Items[i].ShippingStatus != OrderShippingStatus.NonShipping)
                {
                    result += Items[i].LineTotal;
                }
            }

            return result;
        }
        public decimal TotalWeightOfShippingItems()
        {
            decimal result = 0m;
            foreach (LineItem li in Items)
            {
                if (li.ShippingStatus != OrderShippingStatus.NonShipping)
                {
                    result += li.GetTotalWeight();
                }
            }
            return result;
        }
        public List<Content.IReplaceable> ItemsAsReplaceable()
        {
            List<Content.IReplaceable> result = new List<Content.IReplaceable>();

            foreach (LineItem li in this.Items)
            {
                result.Add(li);
            }

            return result;
        }
        public List<Content.IReplaceable> PackagesAsReplaceable()
        {
            List<Content.IReplaceable> result = new List<Content.IReplaceable>();

            foreach (OrderPackage op in this.Packages)
            {
                result.Add(op);
            }

            return result;
        }

        public string TotalsAsTable()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<table cellspacing=\"0\" cellpadding=\"2\" class=\"totaltable\" border=\"0\">");

            // Sub Total
            if (this.OrderDiscountDetails.Count > 0)
            {
                sb.Append("<tr>");
                sb.Append("<td class=\"totallabel\">");
                sb.Append("Before Discounts:");
                sb.Append("</td>");
                sb.Append("<td class=\"totalsub\">" + this.TotalOrderBeforeDiscounts.ToString("c") + "</td>");
                sb.Append("</tr>");

                foreach (MerchantTribe.Commerce.Marketing.DiscountDetail d in this.OrderDiscountDetails)
                {
                    sb.Append("<tr>");
                    sb.Append("<td class=\"totaldiscountdetail\">");
                    sb.Append(d.Description + ":");
                    sb.Append("</td>");
                    sb.Append("<td class=\"totaldiscount\">");
                    sb.Append(string.Format("{0:c}", d.Amount));
                    sb.Append("</td>");
                    sb.Append("</tr>");
                }

                sb.Append("<tr>");
                sb.Append("<td class=\"totallabel\">");
                sb.Append("Sub Total:</td>");
                sb.Append("<td class=\"totalsub\">");
                sb.Append(string.Format("{0:c}", this.TotalOrderAfterDiscounts));
                sb.Append("</td>");
                sb.Append("</tr>");
            }
            else
            {
                sb.Append("<tr>");
                sb.Append("<td class=\"totallabel\">");
                sb.Append("Sub Total:");
                sb.Append("</td>");
                sb.Append("<td class=\"totalsub\">" + this.TotalOrderBeforeDiscounts.ToString("c") + "</td>");
                sb.Append("</tr>");
            }

            // Shipping
            if (this.ShippingDiscountDetails.Count > 0)
            {
                sb.Append("<tr>");
                sb.Append("<td class=\"totallabel\">");
                sb.Append("S&amp;H Before Discounts:");
                sb.Append("</td>");
                sb.Append("<td class=\"totalshipping\">" + this.TotalShippingBeforeDiscounts.ToString("c") + "</td>");
                sb.Append("</tr>");

                foreach (MerchantTribe.Commerce.Marketing.DiscountDetail d in this.ShippingDiscountDetails)
                {
                    sb.Append("<tr>");
                    sb.Append("<td class=\"totaldiscountdetail\">");
                    sb.Append(d.Description + ":");
                    sb.Append("</td>");
                    sb.Append("<td class=\"totaldiscount\">");
                    sb.Append(string.Format("{0:c}", d.Amount));
                    sb.Append("</td>");
                    sb.Append("</tr>");
                }

                sb.Append("<tr>");
                sb.Append("<td class=\"totallabel\">");
                sb.Append("Shipping:<br /><span class=\"tiny\">" + ShippingMethodDisplayName + "</span></td>");
                sb.Append("<td class=\"totalshipping\">");
                sb.Append(string.Format("{0:c}", this.TotalShippingAfterDiscounts));
                sb.Append("</td>");
                sb.Append("</tr>");
            }
            else
            {
                sb.Append("<tr>");
                sb.Append("<td class=\"totallabel\">");
                sb.Append("Shipping:<br /><span class=\"tiny\">" + ShippingMethodDisplayName + "</span></td>");
                sb.Append("<td class=\"totalshipping\">");
                sb.Append(this.TotalShippingAfterDiscounts.ToString("c"));
                sb.Append("</td>");
                sb.Append("</tr>");
            }

            // Tax
            sb.Append("<tr>");
            sb.Append("<td class=\"totallabel\">");
            sb.Append("Tax:</td>");
            sb.Append("<td class=\"totaltax\">");
            sb.Append(this.TotalTax.ToString("c"));
            sb.Append("</td>");
            sb.Append("</tr>");
            
            // Grand Total
            sb.Append("<tr>");
            sb.Append("<td class=\"totallabel\">");
            sb.Append("&nbsp;");
            sb.Append("</td>");
            sb.Append("<td class=\"totalgrand\" style=\"border-top: solid 1px #666;\">");
            sb.Append("<strong>");
            sb.Append(this.TotalGrand.ToString("c"));
            sb.Append("</strong></td>");
            sb.Append("</tr>");

            sb.Append("</table>");

            return sb.ToString();
        }

        // Alternate View for Taxes
        public List<Taxes.ITaxable> ItemsAsITaxable()
        {
            List<Taxes.ITaxable> result = new List<Taxes.ITaxable>();

            foreach (Taxes.ITaxable t in this.Items)
            {
                result.Add(t);
            }

            return result;
        }

        // Recalculate Helpers		                        
        public void ClearTaxes()
        {
            foreach (Taxes.ITaxable item in this.ItemsAsITaxable())
            {
                item.ClearTaxValue();
            }
        }
        public void ClearShippingPricesAndMethod()
        {
            this.ShippingMethodId = string.Empty;
            this.ShippingMethodDisplayName = string.Empty;
            this.ShippingProviderId = string.Empty;
            this.ShippingProviderServiceCode = string.Empty;
            this.TotalShippingBeforeDiscounts = 0;
        }
        public void ClearDiscounts()
        {
            for (int i = 0; i <= Items.Count - 1; i++)
            {
                Items[i].DiscountDetails.Clear();
            }            
            this.OrderDiscountDetails.Clear();
            this.ShippingDiscountDetails.Clear();
        }

        // Payments
        public MerchantTribe.Payment.Transaction GetEmptyTransaction()
        {
            MerchantTribe.Payment.Transaction t = new MerchantTribe.Payment.Transaction();

            t.Customer.City = this.BillingAddress.City;
            t.Customer.Company = this.BillingAddress.Company;
            t.Customer.Country = this.BillingAddress.CountryName;
            t.Customer.Email = this.UserEmail;
            t.Customer.FirstName = this.BillingAddress.FirstName;
            t.Customer.LastName = this.BillingAddress.LastName;
            t.Customer.Phone = this.BillingAddress.Phone;
            t.Customer.PostalCode = this.BillingAddress.PostalCode;
            t.Customer.Region = this.BillingAddress.RegionName;
            t.Customer.Street = this.BillingAddress.Line1;

            t.Customer.ShipCity = this.ShippingAddress.City;
            t.Customer.ShipCompany = this.ShippingAddress.Company;
            t.Customer.ShipCountry = this.ShippingAddress.CountryName;
            t.Customer.ShipFirstName = this.ShippingAddress.FirstName;
            t.Customer.ShipLastName = this.ShippingAddress.LastName;
            t.Customer.ShipPhone = this.ShippingAddress.Phone;
            t.Customer.ShipPostalCode = this.ShippingAddress.PostalCode;
            t.Customer.ShipRegion = this.ShippingAddress.RegionName;
            t.Customer.ShipStreet = this.ShippingAddress.Line1;

            t.MerchantDescription = "Order " + this.OrderNumber;
            t.MerchantInvoiceNumber = this.OrderNumber;

            return t;
        }

        //public Order CloneAsNewOrder()
        //{
        //    Order clone = new Order();
            
        //    // Copy items
        //    foreach (LineItem li in this.Items)
        //    {
        //        clone.Items.Add(li.Clone());
        //    }            

        //    // Copy Coupons
        //    foreach (OrderCoupon coupon in this.Coupons)
        //    {
        //        clone.Coupons.Add(new OrderCoupon() { CouponCode = coupon.CouponCode });
        //    }

        //    clone.AffiliateID = this.AffiliateID;
        //    this.BillingAddress.CopyTo(clone.BillingAddress);
        //    this.ShippingAddress.CopyTo(clone.ShippingAddress);
        //    foreach (CustomProperty prop in this.CustomProperties)
        //    {
        //        clone.CustomProperties.Add(new CustomProperty(prop.DeveloperId, prop.Key, prop.Value));
        //    }
        //    clone.Instructions = this.Instructions;
        //    clone.IsPlaced = false;
        //    clone.ShippingMethodDisplayName = this.ShippingMethodDisplayName;
        //    clone.ShippingMethodId = this.ShippingMethodId;            
        //    clone.ShippingProviderId = this.ShippingProviderId;
        //    clone.ShippingProviderServiceCode = this.ShippingProviderServiceCode;
        //    clone.StoreId = this.StoreId;
        //    clone.UserEmail = this.UserEmail;
        //    clone.UserID = this.UserID;

        //    return clone;
        //}

        // Coupons

        public bool AddCouponCode(string code)
        {
            if (!CouponCodeExists(code))
            {
                this.Coupons.Add(new OrderCoupon() { CouponCode = code.Trim().ToUpper(), UserId = this.UserID, StoreId = this.StoreId, OrderBvin = this.bvin, IsUsed = false });
            }
            return true;
        }
        public bool CouponCodeExists(string code)
        {
            var c = this.Coupons.Where(y => y.CouponCode.Trim().ToUpper() == code.Trim().ToUpper()).Count();
            return c > 0;
        }
        public bool RemoveCouponCode(long id)
        {
            OrderCoupon c = this.Coupons.Where(y => y.Id == id).SingleOrDefault();
            if (c != null)
            {
                this.Coupons.Remove(c);
            }
            return true;
        }
        public bool RemoveCouponCodeByCode(string code)
        {
            bool result = false;
            string testCode = code.Trim().ToUpper();
            var codes = this.Coupons.Where(y => y.CouponCode == testCode).ToList();
            List<long> toRemove = new List<long>();
            foreach (OrderCoupon oc in codes)
            {
                toRemove.Add(oc.Id);
            }
            foreach (long id in toRemove)
            {
                this.RemoveCouponCode(id);
                result = true;
            }
            return result;
        }
        public bool RemoveAllCouponCodes()
        {
            this.Coupons.Clear();
            return true;
        }

        // Misc.
        public List<Shipping.ShippingGroup> GetShippingGroups()
        {
            List<Shipping.ShippingGroup> result = new List<Shipping.ShippingGroup>();
            foreach (LineItem item in this.Items)
            {
                // skip non-shipping items;
                if (item.ShippingSchedule == -1) continue;

                Shipping.ShippingGroup packageToAddTo = null;

                if (!item.ShipSeparately)
                {
                    if (item.ShipFromMode == Shipping.ShippingMode.ShipFromManufacturer)
                    {
                        foreach (Shipping.ShippingGroup package in result)
                        {
                            if ((package.ShippingMode == Shipping.ShippingMode.ShipFromManufacturer) & (!package.ShipSeperately))
                            {
                                if (package.ShipId == item.ShipFromNotificationId)
                                {
                                    packageToAddTo = package;
                                    break;
                                }
                            }
                        }
                    }
                    else if (item.ShipFromMode == Shipping.ShippingMode.ShipFromSite)
                    {
                        foreach (Shipping.ShippingGroup package in result)
                        {
                            if ((package.ShippingMode == Shipping.ShippingMode.ShipFromSite) & (!package.ShipSeperately))
                            {
                                packageToAddTo = package;
                                break;
                            }
                        }
                    }
                    else if (item.ShipFromMode == Shipping.ShippingMode.ShipFromVendor)
                    {
                        foreach (Shipping.ShippingGroup package in result)
                        {
                            if ((package.ShippingMode == Shipping.ShippingMode.ShipFromVendor) & (!package.ShipSeperately))
                            {
                                if (package.ShipId == item.ShipFromNotificationId)
                                {
                                    packageToAddTo = package;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Throw New ApplicationException("Unrecognized shipping mode.")
                        // Assume ship from store if no other mode located
                        foreach (Shipping.ShippingGroup package in result)
                        {
                            if ((package.ShippingMode == Shipping.ShippingMode.ShipFromSite) & (!package.ShipSeperately))
                            {
                                packageToAddTo = package;
                                break;
                            }
                        }
                    }
                }


                if ((packageToAddTo == null))
                {
                    packageToAddTo = new Shipping.ShippingGroup();
                    packageToAddTo.DestinationAddress = this.ShippingAddress;
                    packageToAddTo.ShippingMode = item.ShipFromMode;
                    packageToAddTo.SourceAddress = item.ShipFromAddress;
                    packageToAddTo.ShipId = item.ShipFromNotificationId;
                    packageToAddTo.ShipSeperately = item.ShipSeparately;

                    if ((!item.ShipSeparately))
                    {
                        result.Add(packageToAddTo);
                    }
                }

                if ((item.ShipSeparately))
                {
                    if (((item.Quantity - item.QuantityShipped) > 1))
                    {
                        for (int i = 0; i <= (int)(item.Quantity - item.QuantityShipped) - 1; i++)
                        {
                            Orders.LineItem newLineItem = item.Clone(true);
                            newLineItem.Quantity = 1;
                            Shipping.ShippingGroup newPackage = packageToAddTo.Clone(false);
                            newPackage.Items.Add(newLineItem);
                            result.Add(newPackage);
                        }
                    }
                    else
                    {
                        packageToAddTo.Items.Add(item);
                        result.Add(packageToAddTo);
                    }
                }
                else
                {
                    packageToAddTo.Items.Add(item);
                }
            }
            return result;
        }
        private OrderShippingStatus EvaluateShippingStatus()
        {
            OrderShippingStatus result = OrderShippingStatus.Unknown;

            if (Items.Count > 0)
            {
                bool shippedFound = false;
                bool unshippedFound = false;
                bool nonShippingFound = false;

                for (int i = 0; i <= Items.Count - 1; i++)
                {
                    switch (Items[i].ShippingStatus)
                    {
                        case OrderShippingStatus.NonShipping:
                            nonShippingFound = true;
                            break;
                        case OrderShippingStatus.FullyShipped:
                            shippedFound = true;
                            break;
                        case OrderShippingStatus.PartiallyShipped:
                            shippedFound = true;
                            unshippedFound = true;
                            break;
                        case OrderShippingStatus.Unknown:
                        case OrderShippingStatus.Unshipped:
                            unshippedFound = true;
                            break;
                    }
                }

                if (nonShippingFound && (unshippedFound == false) && (shippedFound == false))
                {
                    // Only non shipping items
                    result = OrderShippingStatus.FullyShipped;
                }
                else
                {
                    if (shippedFound == true && unshippedFound == true)
                    {
                        // Some items shipping and others not
                        result = OrderShippingStatus.PartiallyShipped;
                    }
                    else
                    {
                        if (shippedFound == true)
                        {
                            // only shipped found
                            result = OrderShippingStatus.FullyShipped;
                        }
                        else
                        {
                            // only unshipped found
                            result = OrderShippingStatus.Unshipped;
                        }
                    }
                }
            }

            ShippingStatus = result;
            return result;
        }
        public string FullOrderStatusDescription()
        {
            string result = string.Empty;
            result = this.StatusName + " / ";
            result += Utilities.EnumToString.OrderPaymentStatus(this.PaymentStatus);
            result += " / ";
            result += Utilities.EnumToString.OrderShippingStatus(this.ShippingStatus);

            return result;
        }
        public LineItem GetLineItem(long Id)
        {
            var li = this.Items.Where(y => y.Id == Id).SingleOrDefault();
            return li;
        }



        //protected void ApplyExtraShipFees(Utilities.SortableCollection<Shipping.ShippingRateDisplay> rates)
        //{
        //    //Get Extra ship fees
        //    decimal extraShipFees = 0m;
        //    foreach (LineItem item in this.Items)
        //    {
        //        Catalog.Product associatedProduct = item.GetAssociatedProduct();
        //        if (associatedProduct != null)
        //        {
        //            if (associatedProduct.ShippingDetails.ExtraShipFee != 0)
        //            {
        //                extraShipFees += Math.Round((associatedProduct.ShippingDetails.ExtraShipFee * item.Quantity), 2, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode);
        //            }
        //        }
        //    }

        //    //Apply Extra ship fees
        //    foreach (Shipping.ShippingRateDisplay rate in rates)
        //    {
        //        rate.Rate += extraShipFees;
        //    }
        //}

        public bool IsOrderFreeShipping()
        {

            foreach (Orders.LineItem item in this.Items)
            {
                if (item.CustomProperties["freeshipping"] == null)
                {
                    return false;
                }
            }

            return true;
        }

        // File Downloads
        private string FileDownloadPropertyKey(string fileBvin)
        {
            return "file" + fileBvin;
        }
        public int GetFileDownloadCount(string fileBvin)
        {
            string key = FileDownloadPropertyKey(fileBvin);
            return this.CustomProperties.GetPropertyAsInt("bvsoftware", key);            
        }
        public void IncreaseFileDownloadCount(string fileBvin)
        {
            string key = FileDownloadPropertyKey(fileBvin);
            int current = this.CustomProperties.GetPropertyAsInt("bvsoftware", key);
            if (current < 0) current = 0;
            current++;
            this.CustomProperties.SetProperty("bvsoftware", key, current);
        }
        public void DecreaseFileDownloadCount(string fileBvin)
        {
            string key = FileDownloadPropertyKey(fileBvin);
            int current = this.CustomProperties.GetPropertyAsInt("bvsoftware", key);
            if (current > 0)
            {
                current--;
            }
            this.CustomProperties.SetProperty("bvsoftware", key, current);
        }
        public void ResetFileDownloadCount(string fileBvin)
        {
            string key = FileDownloadPropertyKey(fileBvin);
            this.CustomProperties.SetProperty("bvsoftware", key, 0);
        }


        private bool DetermineIfAnyItemsAreShipping()
        {
            bool result = false;
            for (int i = 0; i <= this.Items.Count - 1; i++)
            {
                if (this.Items[i].ShippingSchedule != -1)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private void ClearShippingMethod()
        {
            TotalShippingBeforeDiscounts = 0m;
            this.ShippingDiscountDetails.Clear();
            ShippingMethodId = string.Empty;
            ShippingMethodDisplayName = string.Empty;
            ShippingProviderId = string.Empty;
            ShippingProviderServiceCode = string.Empty;
        }

        public List<Content.HtmlTemplateTag> GetReplaceableTags(MerchantTribeApplication app)
        {
            Accounts.Store currentStore = app.CurrentRequestContext.CurrentStore;

            List<Content.HtmlTemplateTag> result = new List<Content.HtmlTemplateTag>();
            result.Add(new Content.HtmlTemplateTag("[[Order.AdminLink]]", System.IO.Path.Combine(currentStore.RootUrl(), "BVAdmin/Orders/ViewOrder.aspx?id=" + this.bvin)));
            result.Add(new Content.HtmlTemplateTag("[[Order.AffiliateId]]", this.AffiliateID));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress]]", this.BillingAddress.ToHtmlString()));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.City]]", this.BillingAddress.City));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.Company]]", this.BillingAddress.Company));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.CountryName]]", this.BillingAddress.CountryName));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.CountyName]]", this.BillingAddress.CountyName));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.Fax]]", this.BillingAddress.Fax));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.FirstName]]", this.BillingAddress.FirstName));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.LastName]]", this.BillingAddress.LastName));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.Line1]]", this.BillingAddress.Line1));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.Line2]]", this.BillingAddress.Line2));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.Line3]]", this.BillingAddress.Line3));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.MiddleInitial]]", this.BillingAddress.MiddleInitial));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.NickName]]", this.BillingAddress.NickName));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.Phone]]", this.BillingAddress.Phone));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.PostalCode]]", this.BillingAddress.PostalCode));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.RegionName]]", this.BillingAddress.RegionName));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress.WebSiteUrl]]", this.BillingAddress.WebSiteUrl));
            result.Add(new Content.HtmlTemplateTag("[[Order.Bvin]]", this.bvin));
            string coupons = string.Empty;
            for (int i = 0; i <= this.Coupons.Count - 1; i++)
            {
                coupons += this.Coupons[i].CouponCode + ", ";
            }
            result.Add(new Content.HtmlTemplateTag("[[Order.Coupons]]", coupons));
            result.Add(new Content.HtmlTemplateTag("[[Order.FraudScore]]", this.FraudScore.ToString("#.#")));
            result.Add(new Content.HtmlTemplateTag("[[Order.GrandTotal]]", this.TotalGrand.ToString("c")));            
            result.Add(new Content.HtmlTemplateTag("[[Order.Instructions]]", this.Instructions));
            result.Add(new Content.HtmlTemplateTag("[[Order.LastUpdated]]", this.LastUpdatedUtc.ToString()));
            result.Add(new Content.HtmlTemplateTag("[[Order.OrderDiscounts]]", this.TotalOrderDiscounts.ToString("c")));
            result.Add(new Content.HtmlTemplateTag("[[Order.OrderNumber]]", this.OrderNumber));

            //result.Add(new Content.HtmlTemplateTag("[[Order.PaymentMethod]]", orderService.OrdersListPaymentMethods(this)));			

            result.Add(new Content.HtmlTemplateTag("[[Order.PaymentStatus]]", Utilities.EnumToString.OrderPaymentStatus(this.PaymentStatus)));
                        
            //result.Add(new Content.HtmlTemplateTag("[[Order.NonPciFullCreditCard]]", fullCC)));

            string notes = string.Empty;
            foreach (Orders.OrderNote item in this.Notes)
            {
                if (item.IsPublic)
                {
                    notes += item.Note;
                }
            }
            result.Add(new Content.HtmlTemplateTag("[[Order.PublicNotes]]", notes));

            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress]]", this.ShippingAddress.ToHtmlString()));
            result.Add(new Content.HtmlTemplateTag("[[Order.BillingAddress]]", this.ShippingAddress.ToHtmlString()));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.City]]", this.ShippingAddress.City));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.Company]]", this.ShippingAddress.Company));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.CountryName]]", this.ShippingAddress.CountryName));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.CountyName]]", this.ShippingAddress.CountyName));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.Fax]]", this.ShippingAddress.Fax));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.FirstName]]", this.ShippingAddress.FirstName));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.LastName]]", this.ShippingAddress.LastName));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.Line1]]", this.ShippingAddress.Line1));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.Line2]]", this.ShippingAddress.Line2));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.Line3]]", this.ShippingAddress.Line3));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.MiddleInitial]]", this.ShippingAddress.MiddleInitial));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.NickName]]", this.ShippingAddress.NickName));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.Phone]]", this.ShippingAddress.Phone));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.PostalCode]]", this.ShippingAddress.PostalCode));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.RegionName]]", this.ShippingAddress.RegionName));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingAddress.WebSiteUrl]]", this.ShippingAddress.WebSiteUrl));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingDiscounts]]", this.TotalShippingDiscounts.ToString("c")));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingMethod]]", this.ShippingMethodDisplayName));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingStatus]]", Utilities.EnumToString.OrderShippingStatus(this.ShippingStatus)));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingTotal]]", this.TotalShippingBeforeDiscounts.ToString("c")));
            result.Add(new Content.HtmlTemplateTag("[[Order.ShippingTotalMinusDiscounts]]", (this.TotalShippingBeforeDiscounts - this.TotalShippingDiscounts).ToString("c")));
            result.Add(new Content.HtmlTemplateTag("[[Order.Status]]", this.StatusName));
            result.Add(new Content.HtmlTemplateTag("[[Order.SubTotal]]", this.TotalOrderBeforeDiscounts.ToString("c")));
            result.Add(new Content.HtmlTemplateTag("[[Order.SubTotalMinusDiscounts]]", (this.TotalOrderBeforeDiscounts - this.TotalOrderDiscounts).ToString("c")));
            result.Add(new Content.HtmlTemplateTag("[[Order.SubTotalMinusDiscouts]]", (this.TotalOrderBeforeDiscounts - this.TotalOrderDiscounts).ToString("c")));
            result.Add(new Content.HtmlTemplateTag("[[Order.TaxTotal]]", this.TotalTax.ToString("c")));
            result.Add(new Content.HtmlTemplateTag("[[Order.TaxTotal2]]", this.TotalTax2.ToString("c")));
            result.Add(new Content.HtmlTemplateTag("[[Order.TimeOfOrder]]", this.TimeOfOrderUtc.ToString()));
            result.Add(new Content.HtmlTemplateTag("[[Order.TotalQuantity]]", this.TotalQuantity.ToString()));
            result.Add(new Content.HtmlTemplateTag("[[Order.TotalWeight]]", this.TotalWeight.ToString()));

            result.Add(new Content.HtmlTemplateTag("[[Order.TotalsAsTable]]", this.TotalsAsTable()));

            List<OrderPackage> packages = this.FindShippedPackages();
            if (packages.Count > 0)
            {
                System.Text.StringBuilder trackingNumbersOutput = new System.Text.StringBuilder("<ul class=\"trackingnumberlist\">");
                System.Text.StringBuilder trackingNumberLinksOutput = new System.Text.StringBuilder("<ul class=\"trackingnumberlinklist\">");
                foreach (OrderPackage item in packages)
                {
                    trackingNumbersOutput.Append("<li>");
                    trackingNumbersOutput.Append(item.TrackingNumber);
                    //foreach (Shipping.ShippingProvider provider in Shipping.AvailableProviders.Providers) {
                    //    if (provider.ProviderId == item.ShippingProviderId) {
                    //        if (provider.SupportsTracking()) {
                    //            trackingNumberLinksOutput.Append("<li>");
                    //            trackingNumberLinksOutput.Append("<a href=\"" + provider.GetTrackingUrl(item.TrackingNumber) + "\">" + item.TrackingNumber + "</a>");
                    //            trackingNumberLinksOutput.Append("</li>");
                    //        }
                    //    }
                    //}
                    trackingNumbersOutput.Append("</li>");
                }
                trackingNumbersOutput.Append("</ul>");
                trackingNumberLinksOutput.Append("</ul>");
                result.Add(new Content.HtmlTemplateTag("[[Order.TrackingNumbers]]", trackingNumbersOutput.ToString()));
                result.Add(new Content.HtmlTemplateTag("[[Order.TrackingNumberLinks]]", trackingNumberLinksOutput.ToString()));
            }
            else
            {
                result.Add(new Content.HtmlTemplateTag("[[Order.TrackingNumbers]]", "<ul class=\"trackingnumberlist\"><li>None Available yet</li></ul>"));
                result.Add(new Content.HtmlTemplateTag("[[Order.TrackingNumberLinks]]", "<ul class=\"trackingnumberlinklist\"><li>None Available yet</li></ul>"));
            }

            result.Add(new Content.HtmlTemplateTag("[[Order.UserID]]", this.UserID));
            result.Add(new Content.HtmlTemplateTag("[[Order.UserName]]", this.UserEmail));
            result.Add(new Content.HtmlTemplateTag("[[Order.UserEmail]]", this.UserEmail));

            return result;
        }

        public void MoveToNextStatus()
        {
            List<Orders.OrderStatusCode> codes = Orders.OrderStatusCode.FindAll();

            for (int i = 0; i <= codes.Count - 1; i++)
            {
                if (codes[i].Bvin == this.StatusCode)
                {
                    // Found Current                    
                    if (i < codes.Count - 1)
                    {
                        this.StatusCode = codes[i + 1].Bvin;
                        this.StatusName = codes[i + 1].StatusName;
                    }
                    break;
                }
            }
        }
        public void MoveToPreviousStatus()
        {
            List<Orders.OrderStatusCode> codes = Orders.OrderStatusCode.FindAll();

            for (int i = 0; i <= codes.Count - 1; i++)
            {
                if (codes[i].Bvin == this.StatusCode)
                {
                    // Found Current                    
                    if (i > 0)
                    {
                        this.StatusCode = codes[i - 1].Bvin;
                        this.StatusName = codes[i - 1].StatusName;
                    }
                    break;
                }
            }
        }

        //public bool CopyItemsTo(Orders.Order destinationOrder, OrderService orderService, Contacts.ContactService contactService)
        //{
        //    bool result = true;

        //    foreach (Orders.LineItem li in this.Items) {
        //        LineItem newLi = li.Clone(false);
        //        destinationOrder.AddItem(newLi, orderService, contactService);
        //    }

        //    return result;
        //}

        public List<OrderPackage> FindShippedPackages()
        {
            List<OrderPackage> result = new List<OrderPackage>();
            foreach (OrderPackage p in this.Packages)
            {
                if (p.HasShipped == true)
                {
                    result.Add(p);
                }
            }
            return result;
        }
        public void EvaluateCurrentShippingStatus()
        {
            this.ShippingStatus = this.EvaluateShippingStatus();
        }


        
                
        public IDictionary<long, decimal> GetLineItemValuesAccountingForOrderDiscounts()
        {
            Dictionary<long, decimal> result = new Dictionary<long, decimal>();

            if (this.TotalOrderDiscounts > 0)
            {
                decimal lineItemTotals = 0;
                foreach (LineItem item in this.Items)
                {
                    lineItemTotals += item.LineTotal;
                }

                List<string> items = new List<string>();
                foreach (LineItem item in this.Items)
                {
                    result.Add(item.Id, item.LineTotal / lineItemTotals);
                }

                foreach (LineItem item in this.Items)
                {
                    result[item.Id] = Math.Round(item.LineTotal - (this.TotalOrderDiscounts * result[item.Id]), 2, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode);
                }

                decimal discountedTotals = 0;
                foreach (long key in result.Keys)
                {
                    discountedTotals += result[key];
                }

                decimal difference = (lineItemTotals - this.TotalOrderDiscounts) - discountedTotals;
                if (difference != 0)
                {
                    foreach (LineItem item in this.Items)
                    {
                        if (result[item.Id] >= difference)
                        {
                            result[item.Id] += difference;
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (LineItem item in this.Items)
                {
                    result.Add(item.Id, item.LineTotal);
                }
            }

            return result;
        }

        // Ivalidatable Members
        public bool IsValid()
        {
            if (GetRuleViolations().Count > 0)
            {
                return false;
            }
            return true;
        }
        public List<MerchantTribe.Web.Validation.RuleViolation> GetRuleViolations()
        {
            List<MerchantTribe.Web.Validation.RuleViolation> violations = new List<MerchantTribe.Web.Validation.RuleViolation>();

            if (this.HasShippingItems)
            {
                MerchantTribe.Web.Validation.ValidationHelper.Required("Shipping Method", this.ShippingMethodUniqueKey, violations, "shippingrate");
            }

            return violations;
        }

        // DTO
        public OrderDTO ToDto()
        {
            OrderDTO dto = new OrderDTO();
            
            dto.AffiliateID = this.AffiliateID ?? string.Empty;
            dto.BillingAddress = this.BillingAddress.ToDto();
            dto.Bvin = this.bvin ?? string.Empty;
            dto.Coupons = new List<OrderCouponDTO>();
            foreach (OrderCoupon c in this.Coupons)
            {
                dto.Coupons.Add(c.ToDto());
            }
            dto.CustomProperties = new List<CustomPropertyDTO>();
            foreach (CustomProperty prop in this.CustomProperties)
            {
                dto.CustomProperties.Add(prop.ToDto());
            }
            dto.FraudScore = this.FraudScore;
            dto.Id = this.Id;
            dto.Instructions = this.Instructions ?? string.Empty;
            dto.IsPlaced = this.IsPlaced;
            dto.Items = new List<LineItemDTO>();
            foreach (LineItem li in this.Items)
            {
                dto.Items.Add(li.ToDto());
            }
            dto.LastUpdatedUtc = this.LastUpdatedUtc;
            dto.Notes = new List<OrderNoteDTO>();
            foreach (OrderNote n in this.Notes)
            {
                dto.Notes.Add(n.ToDto());
            }
            dto.OrderDiscountDetails = new List<DiscountDetailDTO>();
            foreach (Marketing.DiscountDetail d in this.OrderDiscountDetails)
            {
                dto.OrderDiscountDetails.Add(d.ToDto());
            }
            dto.OrderNumber = this.OrderNumber ?? string.Empty;
            dto.Packages = new List<OrderPackageDTO>();
            foreach (OrderPackage pak in this.Packages)
            {
                dto.Packages.Add(pak.ToDto());
            }
            dto.PaymentStatus = (OrderPaymentStatusDTO)((int)this.PaymentStatus);
            dto.ShippingAddress = this.ShippingAddress.ToDto();
            dto.ShippingDiscountDetails = new List<DiscountDetailDTO>();
            foreach (Marketing.DiscountDetail sd in this.ShippingDiscountDetails)
            {
                dto.ShippingDiscountDetails.Add(sd.ToDto());
            }
            dto.ShippingMethodDisplayName = this.ShippingMethodDisplayName ?? string.Empty;
            dto.ShippingMethodId = this.ShippingMethodId ?? string.Empty;
            dto.ShippingProviderId = this.ShippingProviderId ?? string.Empty;
            dto.ShippingProviderServiceCode = this.ShippingProviderServiceCode ?? string.Empty;
            dto.ShippingStatus = (OrderShippingStatusDTO)((int)this.ShippingStatus);
            dto.StatusCode = this.StatusCode ?? string.Empty;
            dto.StatusName = this.StatusName ?? string.Empty;
            dto.StoreId = this.StoreId;
            dto.ThirdPartyOrderId = this.ThirdPartyOrderId ?? string.Empty;
            dto.TimeOfOrderUtc = this.TimeOfOrderUtc;
            dto.TotalHandling = this.TotalHandling;
            dto.TotalShippingBeforeDiscounts = this.TotalShippingBeforeDiscounts;
            dto.TotalTax = this.TotalTax;
            dto.TotalTax2 = this.TotalTax2;
            dto.UserEmail = this.UserEmail ?? string.Empty;
            dto.UserID = this.UserID ?? string.Empty;

            return dto;
        }
        public void FromDTO(OrderDTO dto)
        {
            if (dto == null) return;

            this.AffiliateID = dto.AffiliateID ?? string.Empty;
            this.BillingAddress.FromDto(dto.BillingAddress);
            this.bvin = dto.Bvin ?? string.Empty;
            this.Coupons.Clear();
            if (dto.Coupons != null)
            {
                foreach (OrderCouponDTO c in dto.Coupons)
                {
                    OrderCoupon cp = new OrderCoupon();
                    cp.FromDto(c);
                    this.Coupons.Add(cp);
                }
            }
            this.CustomProperties.Clear();
            if (dto.CustomProperties != null)
            {
                foreach (CustomPropertyDTO prop in dto.CustomProperties)
                {
                    CustomProperty p = new CustomProperty();
                    p.FromDto(prop);
                    this.CustomProperties.Add(p);
                }
            }
            this.FraudScore = dto.FraudScore;
            this.Id = dto.Id;
            this.Instructions = dto.Instructions ?? string.Empty;
            this.IsPlaced = dto.IsPlaced;
            this.Items.Clear();
            if (dto.Items != null)
            {
                foreach (LineItemDTO li in dto.Items)
                {
                    LineItem l = new LineItem();
                    l.FromDto(li);
                    this.Items.Add(l);
                }
            }
            this.LastUpdatedUtc = dto.LastUpdatedUtc;
            this.Notes.Clear();
            if (dto.Notes != null)
            {
                foreach (OrderNoteDTO n in dto.Notes)
                {
                    OrderNote nn = new OrderNote();
                    nn.FromDto(n);
                    this.Notes.Add(nn);
                }
            }
            this.OrderDiscountDetails.Clear();
            if (dto.OrderDiscountDetails != null)
            {
                foreach (DiscountDetailDTO d in dto.OrderDiscountDetails)
                {
                    Marketing.DiscountDetail m = new Marketing.DiscountDetail();
                    m.FromDto(d);
                    this.OrderDiscountDetails.Add(m);
                }
            }
            this.OrderNumber = dto.OrderNumber ?? string.Empty;
            this.Packages.Clear();
            if (dto.Packages != null)
            {
                foreach (OrderPackageDTO pak in dto.Packages)
                {
                    OrderPackage pak2 = new OrderPackage();
                    pak2.FromDto(pak);
                    this.Packages.Add(pak2);
                }
            }
            this.PaymentStatus = (OrderPaymentStatus)((int)dto.PaymentStatus);
            this.ShippingAddress.FromDto(dto.ShippingAddress);
            this.ShippingDiscountDetails.Clear();
            if (dto.ShippingDiscountDetails != null)
            {
                foreach (DiscountDetailDTO sd in dto.ShippingDiscountDetails)
                {
                    Marketing.DiscountDetail sdd = new Marketing.DiscountDetail();
                    sdd.FromDto(sd);
                    this.ShippingDiscountDetails.Add(sdd);
                }
            }
            this.ShippingMethodDisplayName = dto.ShippingMethodDisplayName ?? string.Empty;
            this.ShippingMethodId = dto.ShippingMethodId ?? string.Empty;
            this.ShippingProviderId = dto.ShippingProviderId ?? string.Empty;
            this.ShippingProviderServiceCode = dto.ShippingProviderServiceCode ?? string.Empty;
            this.ShippingStatus = (OrderShippingStatus)((int)dto.ShippingStatus);
            this.StatusCode = dto.StatusCode ?? string.Empty;
            this.StatusName = dto.StatusName ?? string.Empty;
            this.StoreId = dto.StoreId;
            this.ThirdPartyOrderId = dto.ThirdPartyOrderId ?? string.Empty;
            this.TimeOfOrderUtc = dto.TimeOfOrderUtc;
            this.TotalHandling = dto.TotalHandling;
            this.TotalShippingBeforeDiscounts = dto.TotalShippingBeforeDiscounts;
            this.TotalTax = dto.TotalTax;
            this.TotalTax2 = dto.TotalTax2;
            this.UserEmail = dto.UserEmail ?? string.Empty;
            this.UserID = dto.UserID ?? string.Empty;

        }

    }

}
