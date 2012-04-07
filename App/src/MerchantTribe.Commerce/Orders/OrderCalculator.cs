using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MerchantTribe.Commerce.Taxes;
using MerchantTribe.Web;
using MerchantTribe.Web.Geography;
using MerchantTribe.Commerce.Marketing;

namespace MerchantTribe.Commerce.Orders
{
    public class OrderCalculator: IOrderCalculator
    {
        private MerchantTribeApplication _app = null;
        public bool SkipRepricing { get; set; }

        public OrderCalculator(MerchantTribeApplication app)                
        {
            this.SkipRepricing = false;
            this._app = app;
        }

        public bool Calculate(Order o)
        {
            // reset values
            o.TotalShippingBeforeDiscounts = 0;            
            o.TotalTax = 0;
            o.TotalTax2 = 0;
            o.TotalHandling = 0;

            o.ClearDiscounts();

            if (!SkipRepricing)
            {
                // Price items for current user
                RepriceItemsForUser(o);
            }
    
            // Discount prices for volume ordered
            ApplyVolumeDiscounts(o);            
                    
            //Apply Offers to Line Items and Sub Total
            ApplyOffersToOrder(o, PromotionActionMode.ForLineItems);
            ApplyOffersToOrder(o, PromotionActionMode.ForSubTotal);                

            // Calculate Handling, Merge with Shipping For Display
            o.TotalHandling = CalculateHandlingAmount(o);

            OrdersCalculateShipping(o);            
            
            // Calculate Per Item shipping and handling portion
            //CalculateShippingPortion(o);

            // Apply shipping offers
            ApplyOffersToOrder(o, PromotionActionMode.ForShipping);
            
            // Calcaulte Taxes
            o.ClearTaxes();
            List<Taxes.ITaxSchedule> schedules = _app.OrderServices.TaxSchedules.FindAllAndCreateDefaultAsInterface(o.StoreId);
            Contacts.Address destination = o.ShippingAddress;
            ApplyTaxes(schedules, o.ItemsAsITaxable(), destination);

            // Calculate Sub Total of Items
            foreach (LineItem li in o.Items)
            {
                o.TotalTax += li.TaxPortion;
            }

                                                            
            return true;
        }
        private void RepriceItemsForUser(Order o)
        {
            foreach (LineItem li in o.Items)
            {                
                Catalog.UserSpecificPrice price = _app.PriceProduct(li.ProductId, o.UserID, li.SelectionData);
                // Null check because if the item isn't in the catalog
                // we will get back a null user specific price. 
                //
                // In the future it may be a good idea to add an option
                // allowing merchant to select if they would like to allow
                // items not in the catalog to exist in carts or if we should
                // just remove items from the cart with a warning here.
                if (price != null)
                {
                    li.BasePricePerItem = price.BasePrice;
                    foreach (Marketing.DiscountDetail discount in price.DiscountDetails)
                    {
                        li.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = discount.Amount * li.Quantity, Description = discount.Description });
                    }
                }
            }
        }      
        private void ApplyOffersToOrder(Order o, PromotionActionMode mode)
        {
            if (o == null) return;
            Membership.CustomerAccount currentUser = null;
            if (o.UserID != string.Empty) currentUser = _app.MembershipServices.Customers.Find(o.UserID);

            List<Marketing.Promotion> offers = _app.MarketingServices.Promotions.FindAllPotentiallyActiveOffers(DateTime.UtcNow);
            foreach (Marketing.Promotion offer in offers)
            {                
                offer.ApplyToOrder(_app, o, currentUser, DateTime.UtcNow, mode);
            }
        }
        private void ApplyVolumeDiscounts(Order o)
        {

            // Count up how many of each item in order
            List<string> products = new List<string>();
            Dictionary<string, int> quantities = new Dictionary<string, int>();
            foreach (LineItem item in o.Items)
            {
                if (!products.Contains(item.ProductId))
                {
                    products.Add(item.ProductId);
                }
                if (quantities.ContainsKey(item.ProductId))
                {
                    quantities[item.ProductId] += (int)item.Quantity;
                }
                else
                {
                    quantities.Add(item.ProductId, (int)item.Quantity);
                }


            }

            // Check for discounts on each item
            foreach (string id in products)
            {
                List<MerchantTribe.Commerce.Catalog.ProductVolumeDiscount> volumeDiscounts = _app.CatalogServices.VolumeDiscounts.FindByProductId(id);                    
                int quantity = quantities[id];
                MerchantTribe.Commerce.Catalog.ProductVolumeDiscount volumeDiscountToApply = null;
                if (volumeDiscounts.Count > 0)
                {
                    // Locate the correct discount in the chart of discounts
                    foreach (MerchantTribe.Commerce.Catalog.ProductVolumeDiscount volumeDiscount in volumeDiscounts)
                    {
                        if (quantity >= volumeDiscount.Qty)
                        {
                            volumeDiscountToApply = volumeDiscount;
                        }
                    }
                    //now we have to go through the entire order and discount all items
                    //that are this id
                    if (volumeDiscountToApply != null)
                    {
                        foreach (LineItem item in o.Items)
                        {
                            if (item.ProductId == id)
                            {
                                Catalog.Product p = _app.CatalogServices.Products.Find(item.ProductId);
                                if (p != null)
                                {
                                    bool alreadyDiscounted = (p.SitePrice > item.AdjustedPricePerItem);
                                    if (!alreadyDiscounted)
                                    {
                                        // item isn't discounted yet so apply the exact price the merchant set
                                        decimal toDiscount = -1 * (item.AdjustedPricePerItem - volumeDiscountToApply.Amount);
                                        toDiscount = toDiscount * item.Quantity;
                                        item.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = toDiscount, Description = "Volume Discount" });
                                    }
                                    else
                                    {
                                        // item is already discounted (probably by user group) so figure out
                                        // the percentage of volume discount instead
                                        decimal originalPriceChange = p.SitePrice - volumeDiscountToApply.Amount;
                                        decimal percentChange = originalPriceChange / p.SitePrice;
                                        decimal newDiscount = -1 * (percentChange * item.AdjustedPricePerItem);
                                        newDiscount = newDiscount * item.Quantity;
                                        item.DiscountDetails.Add(new Marketing.DiscountDetail() { Amount = newDiscount, Description = percentChange.ToString("p0") + " Volume Discount" });
                                    }
                                    
                                }
                                                                                                
                            }
                        }
                    }
                }
            }
        }
        private decimal CalculateHandlingAmount(Order o)
        {
            Accounts.Store currentStore = _app.CurrentStore;
            if (currentStore == null) return 0;

            if (currentStore.Settings.HandlingType == (int)HandlingMode.PerItem)
            {
                decimal amount = 0;
                foreach (Orders.LineItem item in o.Items)
                {
                    if (item.ShippingSchedule == -1)
                    {
                        if (currentStore.Settings.HandlingNonShipping)
                        {
                            amount += item.Quantity;
                        }
                    }
                    else
                    {
                        amount += item.Quantity;
                    }
                }
                return (currentStore.Settings.HandlingAmount * amount);
            }
            else if (currentStore.Settings.HandlingType == (int)HandlingMode.PerOrder)
            {
                //charge handling if there aren't non shipping items
                if (currentStore.Settings.HandlingNonShipping)
                {
                    foreach (Orders.LineItem item in o.Items)
                    {
                        return currentStore.Settings.HandlingAmount;
                    }
                }
                else
                {
                    foreach (Orders.LineItem item in o.Items)
                    {
                        if (item.ShippingSchedule != -1)
                        {
                            return currentStore.Settings.HandlingAmount;
                        }
                    }
                }
            }

            return 0;
        }
        private void OrdersCalculateShipping(Order o)
        {
            o.TotalShippingBeforeDiscounts = 0;
            if (o.ShippingMethodId != string.Empty)
            {
                Shipping.ShippingRateDisplay r = _app.OrderServices.OrdersFindShippingRateByUniqueKey(o.ShippingMethodUniqueKey, o, _app.CurrentStore);
                if (r != null)
                {
                    o.ShippingMethodDisplayName = r.DisplayName;
                    o.ShippingProviderId = r.ProviderId;
                    o.ShippingProviderServiceCode = r.ProviderServiceCode;
                    o.TotalShippingBeforeDiscounts = Math.Round(r.Rate, 2, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode);
                    //this.AddPackages(r.SuggestedPackages);
                }
            }

            if (o.TotalShippingBeforeDiscountsOverride >= 0)
            {
                o.TotalShippingBeforeDiscounts = o.TotalShippingBeforeDiscountsOverride;
            }
        }

        private void ApplyTaxes(ITaxSchedule schedule, ITaxable item, IAddress address)
        {
            List<ITaxSchedule> schedules = new List<ITaxSchedule>();
            schedules.Add(schedule);
            List<ITaxable> items = new List<ITaxable>();
            items.Add(item);
            ApplyTaxes(schedules, items, address);
        }
        private void ApplyTaxes(List<ITaxSchedule> schedules, List<ITaxable> items, IAddress address)
        {
            if (schedules != null)
            {
                foreach (ITaxSchedule ts in schedules)
                {
                    TaxItems(ts, items, address);
                }
            }
        }
        private void TaxItems(ITaxSchedule schedule, List<ITaxable> items, IAddress address)
        {
            if (schedule == null) return;
            if (items == null) return;
            if (address == null) return;

            // Holds all taxes for the schedule summarized
            TaxableItem summaryForSchedule = new TaxableItem();
            summaryForSchedule.ScheduleId = schedule.TaxScheduleId();

            // Find all items that match the schedule
            List<ITaxable> itemsMatchingSchedule = new List<ITaxable>();
            foreach (ITaxable item in items)
            {
                if (item.TaxScheduleId() == schedule.TaxScheduleId())
                {
                    summaryForSchedule.Value += item.TaxableValue();
                    summaryForSchedule.ShippingValue += item.TaxableShippingValue();
                    itemsMatchingSchedule.Add(item);
                }
            }

            // Now Apply all taxes in schedule to total price of all items
            foreach (ITaxRate rate in _app.OrderServices.Taxes.GetRates(_app.CurrentStore.Id, schedule.TaxScheduleId()))
            {
                rate.TaxItem(summaryForSchedule, address);
            }

            // Total Tax for all items on this schedule is calculated
            // Now, we assign the tax parts to each line item based on their
            // linetotal value. The last item should get the remainder of the tax
            decimal RoundedTotal = Math.Round(summaryForSchedule.TaxedValue, 2, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode);

            decimal TotalApplied = 0M;

            for (int i = 0; i < itemsMatchingSchedule.Count(); i++)
            {
                ITaxable item = itemsMatchingSchedule[i];

                item.ClearTaxValue();
                if (i == itemsMatchingSchedule.Count() - 1)
                {
                    // last item
                    item.IncrementTaxValue(RoundedTotal - TotalApplied);
                }
                else
                {
                    decimal percentOfTotal = 0;
                    if (summaryForSchedule.TaxableValue() != 0)
                    {
                        percentOfTotal = item.TaxableValue() / summaryForSchedule.TaxableValue();
                    }
                    decimal part = Math.Round(percentOfTotal * summaryForSchedule.TaxedValue, 2, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode);
                    item.IncrementTaxValue(part);
                    TotalApplied += part;
                }
            }

        }        
    }
}
