using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerchantTribe.Billing
{
    public class Invoice
    {
        private DateTime Date = DateTime.UtcNow;

        public long Id { get; set; }
        public long AccountId { get; set; }
        public InvoiceTerms Terms { get; set; }
        public DateTime DateLocal
        {
            get { return Date.ToLocalTime(); }
            set { Date = value.ToUniversalTime(); }
        }
        public DateTime DateUtc
        {
            get { return Date; }
            set { Date = value; }
        }
        public decimal TaxRate { get; set; }

        public List<InvoiceItem> Items { get; set; }
        
        public Invoice()
        {
            Id = 0;
            AccountId = 0;
            Terms = InvoiceTerms.DueOnReceipt;
            TaxRate = 0;
            Items = new List<InvoiceItem>();            
        }

        public void AddItem(string sku, string description, decimal quantity, decimal unitPrice, bool taxable)
        {
            InvoiceItem i = new InvoiceItem()
            {
                Sku = sku,
                Description = description,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Taxable = taxable
            };
            Items.Add(i);
        }

        public decimal TotalItems()
        {
            decimal result = 0;
            foreach (InvoiceItem i in Items)
            {
                result += i.LineTotal;
            }
            return result;
        }
        public decimal TotalItemsNonTaxable()
        {
            decimal result = 0;
            foreach (InvoiceItem i in Items)
            {
                if (!i.Taxable)
                {
                    result += i.LineTotal;
                }
            }
            return result;
        }
        public decimal TotalItemsTaxable()
        {
            decimal result = 0;
            foreach (InvoiceItem i in Items)
            {
                if (i.Taxable)
                {
                    result += i.LineTotal;
                }
            }
            return result;
        }
        public decimal TotalTax()
        {
            return Math.Round((TaxRate / 100m) * TotalItemsTaxable(), 2, MidpointRounding.AwayFromZero);
        }
        public decimal TotalGrand()
        {
            return TotalItems() + TotalTax();
        }
    }
}
