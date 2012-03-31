using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;

namespace MerchantTribe.Shipping.USPostal.v4
{
    public class DomesticPackage
    {

        public string Id { get; set; }
        public DomesticServiceType Service { get; set; }
        public string ZipOrigination { get; set; }
        public string ZipDestination { get; set; }
        public int Pounds { get; set; }
        public decimal Ounces { get; set; }
        public DomesticPackageType Container { get; set; }
        
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }

        public decimal DeclaredValue { get; set; }
        public decimal AmountToCollect { get; set; }
        public string Zone { get; set; }
        public List<DomesticPostage> Postages { get; set; }
        public List<DomesticSpecialServiceType> SpecialServices { get; set; }

        private void Init()
        {
            Id = "0";
            Service = DomesticServiceType.All;
            ZipOrigination = string.Empty;
            ZipDestination = string.Empty;
            Pounds = 0;
            Ounces = 0;
            Container = DomesticPackageType.Ignore;
            DeclaredValue = 0m;
            AmountToCollect = 0m;
            Zone = string.Empty;
            Postages = new List<DomesticPostage>();
            SpecialServices = new List<DomesticSpecialServiceType>();
        }

        public DomesticPackage()
        {
            Init();
        }

        public DomesticPackage(XmlNode n)
        {
            Init();
            ParseNode(n);
        }

        private void SortDimensions()
        {
            List<decimal> dimensions = new List<decimal>();
            dimensions.Add(this.Length);
            dimensions.Add(this.Width);
            dimensions.Add(this.Height);

            List<decimal> sorted = (from d in dimensions
                                    orderby d descending
                                    select d).ToList();
            this.Length = sorted[0];
            this.Width = sorted[1];
            this.Height = sorted[2];            
        }
        public decimal Girth()
        {
            if (this.HasDimensions())
            {
                return (this.Length + this.Length + this.Height + this.Height);
            }
            else
            {
                return 1;
            }
        }
        public DomesticPackageSize DeterminePackageSize()
        {
            if (this.Length > 12 || this.Width > 12 || this.Height > 12)
            {
                return DomesticPackageSize.Large;
            }
            if (this.Container == DomesticPackageType.NonRectangular)
            {
                return DomesticPackageSize.Large;
            }
            return DomesticPackageSize.Regular;
        }
        public bool HasDimensions()
        {
            return (this.Length > 0 &&
                    this.Height > 0 &&
                    this.Width > 0);            
        }
        public DomesticMachinable IsMachinable()
        {
            bool required = false;

            if (this.Service == DomesticServiceType.FirstClass || this.Service == DomesticServiceType.FirstClassHoldForPickupCommercial)
            {
                if (this.Container == DomesticPackageType.FirstClassFlat ||
                    this.Container == DomesticPackageType.FirstClassLetter)
                {
                    required = true;
                }
            }
            if (this.Service == DomesticServiceType.ParcelPost ||
                this.Service == DomesticServiceType.All ||
                this.Service == DomesticServiceType.Online)
            {
                required = true;    
            }

            if (required)
            {                
                if (this.Pounds < 1 && this.Ounces < 6) return DomesticMachinable.No;                
                if (this.Pounds > 35) return DomesticMachinable.No;
                if (this.HasDimensions())
                {
                    if (this.Length > 34) return DomesticMachinable.No;
                    if (this.Width > 17 || this.Height > 17) return DomesticMachinable.No;
                    if (this.Container == DomesticPackageType.FirstClassParcel && this.Length < 6) return DomesticMachinable.No;
                    if (this.Container == DomesticPackageType.FirstClassParcel && this.Height < 3) return DomesticMachinable.No;
                }
                return DomesticMachinable.Yes;
            }

            return DomesticMachinable.Ignored;
        }
        public void ParseNode(XmlNode n)
        {
            if (n == null) return;

            if (n.Attributes.GetNamedItem("ID") != null)
            {
                this.Id = n.Attributes.GetNamedItem("ID").InnerText;                
            }
            ZipOrigination = MerchantTribe.Web.Xml.ParseInnerText(n, "ZipOrigination");
            Pounds = MerchantTribe.Web.Xml.ParseInteger(n, "Pounds");
            Ounces = MerchantTribe.Web.Xml.ParseInteger(n, "Ounces");
            Zone = MerchantTribe.Web.Xml.ParseInnerText(n, "Zone");            
            this.Postages.Clear();
            foreach (XmlNode n2 in n.SelectNodes("Postage"))
            {
                DomesticPostage p = new DomesticPostage(n2);
                this.Postages.Add(p);
            }


        }

        public void WriteToXml(ref XmlTextWriter xw)
        {
            this.SortDimensions();

            xw.WriteStartElement("Package");
            xw.WriteAttributeString("ID", this.Id.ToString());
            
            // Package Info
            xw.WriteElementString("Service", TranslateServiceCodeToString(this.Service));

            if (this.Service == DomesticServiceType.FirstClass 
                || this.Service == DomesticServiceType.FirstClassHoldForPickupCommercial)
            {
                xw.WriteElementString("FirstClassMailType", TranslateContainerCode(this.Container));
            }

            xw.WriteElementString("ZipOrigination", this.ZipOrigination);
            xw.WriteElementString("ZipDestination", this.ZipDestination);
            xw.WriteElementString("Pounds", this.Pounds.ToString());
            xw.WriteElementString("Ounces", this.Ounces.ToString());

            // Container and First Class Types
            if (this.Service == DomesticServiceType.FirstClass 
                || this.Service == DomesticServiceType.FirstClassHoldForPickupCommercial)
            {
                xw.WriteElementString("Container", "");                
            }
            else
            {
                xw.WriteElementString("Container", TranslateContainerCode(this.Container));
            }

            // Size
            if (this.DeterminePackageSize() == DomesticPackageSize.Large)
            {
                xw.WriteElementString("Size", "LARGE");
            }
            else
            {
                xw.WriteElementString("Size", "REGULAR");
            }

            // Dimesions here
            if (this.DeterminePackageSize() == DomesticPackageSize.Large)
            {
                if (this.HasDimensions())
                {
                    xw.WriteElementString("Length", Math.Round(this.Length, 1, MidpointRounding.AwayFromZero).ToString());
                    xw.WriteElementString("Width", Math.Round(this.Width, 1, MidpointRounding.AwayFromZero).ToString());
                    xw.WriteElementString("Height", Math.Round(this.Height, 1, MidpointRounding.AwayFromZero).ToString());
                }
                else
                {
                    xw.WriteElementString("Length", Math.Round(6.0, 1, MidpointRounding.AwayFromZero).ToString());
                    xw.WriteElementString("Width", Math.Round(3.0, 1, MidpointRounding.AwayFromZero).ToString());
                    xw.WriteElementString("Height", Math.Round(0.25, 1, MidpointRounding.AwayFromZero).ToString());
                }
            }

            // Girth
            {
                bool girthRequired = false;
                if (this.Container == DomesticPackageType.NonRectangular) girthRequired = true;
                if (this.Container == DomesticPackageType.Variable && this.DeterminePackageSize() == DomesticPackageSize.Large) girthRequired = true;
                if (girthRequired)
                {
                    xw.WriteElementString("Girth", Math.Round(this.Girth(), 1, MidpointRounding.AwayFromZero).ToString());
                }            
            }

            // Machinable
            if (this.IsMachinable() == DomesticMachinable.Yes)
            {
                xw.WriteElementString("Machinable", "true");
            }
            if (this.IsMachinable() == DomesticMachinable.No)
            {
                xw.WriteElementString("Machinable", "false");
            }

            if (this.DeclaredValue > 0)
            {
                xw.WriteElementString("Value", Math.Round(this.DeclaredValue, 2, MidpointRounding.AwayFromZero).ToString());
            }
            if (this.AmountToCollect > 0)
            {
                xw.WriteElementString("AmountToCollect", Math.Round(this.AmountToCollect, 2, MidpointRounding.AwayFromZero).ToString());
            }

            // Special Services
            if (this.SpecialServices.Count > 0)
            {
                xw.WriteStartElement("SpecialServices");
                foreach (DomesticSpecialServiceType s in this.SpecialServices)
                {
                    xw.WriteElementString("SpecialService", ((int)s).ToString());
                }
                xw.WriteEndElement();
            }

            // End Package
            xw.WriteEndElement();
        }

        public string TranslateServiceCodeToString(DomesticServiceType service)
        {
            switch (service)
            {
                case DomesticServiceType.All:
                    return "ALL";
                case DomesticServiceType.ExpressMail:
                    return "EXPRESS";
                case DomesticServiceType.ExpressMailCommerceial:
                    return "EXPRESS COMMERCIAL";
                case DomesticServiceType.ExpressMailHoldForPickup:
                    return "EXPRESS HFP";
                case DomesticServiceType.ExpressMailHoldForPickupCommercial:
                    return "EXPRESS HFP COMMERCIAL";                
                case DomesticServiceType.ExpressMailSundayHoliday:
                    return "EXPRESS SH";
                case DomesticServiceType.ExpressMailSundayHolidayCommercial:
                    return "EXPRESS SH COMMERCIAL";
                case DomesticServiceType.FirstClass:
                    return "FIRST CLASS";
                case DomesticServiceType.LibraryMaterial:
                    return "LIBRARY";
                case DomesticServiceType.MediaMail:
                    return "MEDIA";
                case DomesticServiceType.Online:
                    return "ONLINE";
                case DomesticServiceType.ParcelPost:
                    return "PARCEL";
                case DomesticServiceType.PriorityMail:
                    return "PRIORITY";
            }

            return "ALL";
        }
        public DomesticServiceType TranslateServiceCode(string service)
        {
            switch (service.Trim().ToUpperInvariant())
            {
                case "ALL":
                    return DomesticServiceType.All;
                case "EXPRESS":
                    return DomesticServiceType.ExpressMail;
                case "EXPRESS COMMERCIAL":
                    return DomesticServiceType.ExpressMailCommerceial;
                case "EXPRESS HFP":
                    return DomesticServiceType.ExpressMailHoldForPickup;
                case "EXPRESS HFP COMMERCIAL":
                    return DomesticServiceType.ExpressMailHoldForPickupCommercial;
                case "EXPRESS SH":
                    return DomesticServiceType.ExpressMailSundayHoliday;
                case "EXPRESS SH COMMERCIAL":
                    return DomesticServiceType.ExpressMailSundayHolidayCommercial;
                case "FIRST CLASS":
                    return DomesticServiceType.FirstClass;
                case "LIBRARY":
                    return DomesticServiceType.LibraryMaterial;
                case "MEDIA":
                    return DomesticServiceType.MediaMail;
                case "ONLINE":
                    return DomesticServiceType.Online;
                case "PARCEL":
                    return DomesticServiceType.ParcelPost;
                case "PRIORITY":
                    return DomesticServiceType.PriorityMail;
            }

            return DomesticServiceType.All;
        }
        public string TranslateContainerCode(DomesticPackageType package)
        {
            switch (package)
            {
                case DomesticPackageType.FirstClassFlat:
                    return "FLAT";
                case DomesticPackageType.FirstClassLetter:
                    return "LETTER";
                case DomesticPackageType.FirstClassParcel:
                    return "PARCEL";
                case DomesticPackageType.FirstClassPostCard:
                    return "POSTCARD";
                case DomesticPackageType.FlatRateBox:
                    return "FLAT RATE BOX";
                case DomesticPackageType.FlatRateBoxLarge:
                    return "LG FLAT RATE BOX";
                case DomesticPackageType.FlatRateBoxMedium:
                    return "MD FLAT RATE BOX";
                case DomesticPackageType.FlatRateBoxSmall:
                    return "SM FLAT RATE BOX";
                case DomesticPackageType.FlatRateEnvelope:
                    return "FLAT RATE ENVELOPE";
                case DomesticPackageType.FlatRateEnvelopePadded:
                    return "PADDED FLAT RATE ENVELOPE";
                case DomesticPackageType.FlatRateEnvelopeLegal:
                    return "LEGAL FLAT RATE ENVELOPE";
                case DomesticPackageType.FlatRateEnvelopeWindow:
                    return "WINDOW FLAT RATE ENVELOPE";
                case DomesticPackageType.FlatRateEnvelopeGiftCard:
                    return "GIFT CARD FLAT RATE ENVELOPE";
                case DomesticPackageType.RegionalBoxRateA:
                    return "REGIONAL BOX RATE A";
                case DomesticPackageType.RegionalBoxRateB:
                    return "REGIONAL BOX RATE B";
                case DomesticPackageType.Ignore:
                    return "VARIABLE";
                case DomesticPackageType.NonRectangular:
                    return "NONRECTANGULAR";
                case DomesticPackageType.Rectangular:
                    return "RECTANGULAR";
                case DomesticPackageType.Variable:
                    return "VARIABLE";
            }
            return "VARIABLE";
        }


    }
}
