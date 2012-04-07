using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerchantTribe.Commerce.Catalog
{
    public class UserSpecificPrice
    {
        private decimal _ModifierAdjustments = 0;
        private decimal _ListPrice = 0m;

        public bool IsValid { get; private set; }
        public string OverrideText {get;set;}
        public decimal BasePrice {get;set;}
        public string Sku { get; private set;}
        public string VariantId { get; private set; }
        public List<Marketing.DiscountDetail> DiscountDetails { get; set; }
        // Calculated Properties
        public decimal ListPrice
        {
            get
            {
                return _ListPrice + _ModifierAdjustments;
            }
            private set
            {
                _ListPrice = value;
            }
        }
        public decimal Savings
        {
            get {
                if (this.ListPrice <= this.PriceWithAdjustments()) return 0;
                return Math.Round((this.ListPrice - this.PriceWithAdjustments()), 2, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode);
            }
        }
        public decimal SavingsPercent
        {
            get { return Math.Round((Savings / this.ListPrice) * 100, 0, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode); }
        }
        public bool ListPriceGreaterThanCurrentPrice
        {
            get { return this.ListPrice > this.BasePrice; }
        }
        
        public UserSpecificPrice(Product initialProduct, OptionSelectionList selections)
        {
            if (initialProduct == null) throw new ArgumentNullException("Initial Product can not be null");

            // init
            this.IsValid = true;
            this.DiscountDetails = new List<Marketing.DiscountDetail>();
            this.VariantId = string.Empty;

            // Load from Product
            this.ListPrice = initialProduct.ListPrice;
            this.Sku = initialProduct.Sku;
            this.BasePrice = initialProduct.SitePrice;
            this.OverrideText = initialProduct.SitePriceOverrideText;            
                                                
            PriceForSelections(initialProduct, selections);

        }

        private void PriceForSelections(Catalog.Product p, OptionSelectionList selections)
        {
            this.IsValid = true;
            this.VariantId = string.Empty;
            this._ModifierAdjustments = 0;

            if (selections == null) return;
            if (p == null) return;

            // Check for Option Price Modifiers
            if (!p.HasOptions()) return;
            this._ModifierAdjustments = selections.GetPriceAdjustmentForSelections(p.Options);
            this.BasePrice += this._ModifierAdjustments;

            // Check for Variant Changes
            if (!p.HasVariants()) return;
            Variant v = p.Variants.FindBySelectionData(selections, p.Options);
            if (v == null)
            {
                this.IsValid = false;
                return;
            }

            // Assign Variant Attributes to this price data
            this.VariantId = v.Bvin;
            if (v.Sku.Trim().Length > 0) this.Sku = v.Sku;
            if (v.Price >= 0) this.BasePrice = v.Price + this._ModifierAdjustments;

        }

        public string DisplayPrice()
        {
            return DisplayPrice(true);
        }
        public string DisplayPrice(bool includeAdjustments)
        {
            if (OverrideText.Length > 0)
            {
                return OverrideText;
            }
            else
            {
                if (includeAdjustments)
                {
                    decimal pricewith = PriceWithAdjustments();
                    if (pricewith < this.BasePrice)
                    {
                        return "<strike>" + this.BasePrice.ToString("C") + "</strike> " + pricewith.ToString("C");
                    }
                    else
                    {
                        return pricewith.ToString("C");
                    }
                }
                else
                {                                        
                    return BasePrice.ToString("C");
                }
            }
        }

        public void AddAdjustment(decimal amount, string description)
        {
            AddAdjustment(new Marketing.DiscountDetail(){ Amount = amount, Description = description});
        }
        public void AddAdjustment(Marketing.DiscountDetail discount)
        {
            this.DiscountDetails.Add(discount);
        }
        public void ClearAllDiscounts()
        {
            this.DiscountDetails.Clear();
        }
        public decimal PriceWithAdjustments()
        {
            decimal result = BasePrice;
            result += this.DiscountDetails.Sum(y => y.Amount);
            return result;
        }        

    }
}
