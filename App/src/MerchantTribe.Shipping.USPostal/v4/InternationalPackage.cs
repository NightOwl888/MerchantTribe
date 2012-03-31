using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MerchantTribe.Shipping.USPostal.v4
{
    public class InternationalPackage
    {

        // Properties for Request and Response
        public string Id { get; set; }
        public int Pounds { get; set; }
        public decimal Ounces { get; set; }
        // Machinable is function
        public InternationalPackageType MailType { get; set; }
        public bool GxgToPoBox { get; set; }
        public bool GxgGift { get; set; }        
        public decimal ValueOfContents { get; set; }
        public string DestinationCountry { get; set; }
        public InternationalContainerType Container { get; set; }
        // Package Size is function
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        // Girth is Function
        public string ZipOrigination { get; set; } // Used for GXG mailability 
        public bool CommercialRates { get; set; }                                             
        public List<InternationalExtraServiceType> ExtraServices { get; set; }


        // Values on Return
        public string Prohibitions { get; set; }
        public string Restrictions { get; set; }
        public string Observations { get; set; }
        public string CustomsForms { get; set; }
        public string ExpressMail { get; set; }
        public string AreasServed { get; set; }
        public List<InternationalPostage> Postages { get; set; }        

        private void Init()
        {            
            Id = "0";
            this.Pounds = 0;
            this.Ounces = 0m;
            this.Length = 0m;
            this.Width = 0m;
            this.Height = 0m;
            this.MailType = InternationalPackageType.All;
            this.GxgGift = false;
            this.GxgToPoBox = false;
            this.ValueOfContents = 0m;
            this.DestinationCountry = string.Empty;
            this.Container = InternationalContainerType.Rectangular;
            this.ZipOrigination = string.Empty;
            this.CommercialRates = false;
            this.ExtraServices = new List<InternationalExtraServiceType>();

            Prohibitions = string.Empty;
            Restrictions = string.Empty;
            Observations = string.Empty;
            CustomsForms = string.Empty;
            ExpressMail = string.Empty;
            AreasServed = string.Empty;
            this.Postages = new List<InternationalPostage>();         
        }

        public InternationalPackage()
        {
            Init();
        }
        public InternationalPackage(XmlNode n)
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
        public bool HasDimensions()
        {
            return (this.Length > 0 &&
                    this.Height > 0 &&
                    this.Width > 0);
        }
        public InternationalPackageSize DeterminePackageSize()
        {
            if (this.Length > 12 || this.Width > 12 || this.Height > 12)
            {
                return InternationalPackageSize.Large;
            }
            if (this.Container == InternationalContainerType.NonRectangular)
            {
                return InternationalPackageSize.Large;
            }
            return InternationalPackageSize.Regular;
        }
        public InternationalMachinable IsMachinable()
        {
            this.SortDimensions();
            decimal aspectRatio = this.Length / this.Height;
            if (aspectRatio < 1.3m || aspectRatio > 2.5m)
            {
                return InternationalMachinable.No;
            }
            return InternationalMachinable.Yes;            
        }

        public void ParseNode(XmlNode n)
        {
            if (n == null) return;
          
            if (n.Attributes.GetNamedItem("ID") != null)
            {
                Id = n.Attributes.GetNamedItem("ID").InnerText;                
            }
            this.Prohibitions = MerchantTribe.Web.Xml.ParseInnerText(n, "Prohibitions");
            this.Restrictions = MerchantTribe.Web.Xml.ParseInnerText(n, "Restrictions");
            this.Observations = MerchantTribe.Web.Xml.ParseInnerText(n, "Observations");
            this.CustomsForms = MerchantTribe.Web.Xml.ParseInnerText(n, "CustomsForms");
            this.ExpressMail = MerchantTribe.Web.Xml.ParseInnerText(n, "ExpressMail");
            this.AreasServed = MerchantTribe.Web.Xml.ParseInnerText(n, "AreasServed");
            this.Postages.Clear();
            foreach (XmlNode n2 in n.SelectNodes("Service"))
            {
                InternationalPostage p = new InternationalPostage(n2);
                this.Postages.Add(p);
            }

        }

        public void WriteToXml(ref XmlTextWriter xw)
        {
            this.SortDimensions();

            xw.WriteStartElement("Package");
            xw.WriteAttributeString("ID", this.Id.ToString());

            // Weight Info
            xw.WriteElementString("Pounds", this.Pounds.ToString());
            xw.WriteElementString("Ounces", this.Ounces.ToString());

            // Machinable
            if (this.IsMachinable() == InternationalMachinable.Yes)
            {
                xw.WriteElementString("Machinable", "true");
            }
            if (this.IsMachinable() == InternationalMachinable.No)
            {
                xw.WriteElementString("Machinable", "false");
            }

            // Mail Type
            xw.WriteElementString("MailType", TranslateMailType(this.MailType));

            // Gxg
            xw.WriteStartElement("GXG");
            xw.WriteElementString("POBoxFlag", this.GxgToPoBox ? "Y" : "N");
            xw.WriteElementString("GiftFlag", this.GxgGift ? "Y" : "N");
            xw.WriteEndElement();

            // Value
            xw.WriteElementString("ValueOfContents", Math.Round(this.ValueOfContents, 2, MidpointRounding.AwayFromZero).ToString());

            // Country
            xw.WriteElementString("Country", this.DestinationCountry);


            // Determine Size
            InternationalPackageSize _size = this.DeterminePackageSize();

            // Container
            if (_size == InternationalPackageSize.Large)
            {
                if (this.Container == InternationalContainerType.NonRectangular)
                {
                    xw.WriteElementString("Container", "NONRECTANGULAR");
                }
                else
                {
                    xw.WriteElementString("Container", "RECTANGULAR");
                }
                xw.WriteElementString("Size", "LARGE");
            }
            else
            {
                xw.WriteElementString("Container", "RECTANGULAR");
                xw.WriteElementString("Size", "REGULAR");
            }

            // Dimesions here
            if (this.HasDimensions())
            {
                xw.WriteElementString("Width", Math.Round(this.Width, 1, MidpointRounding.AwayFromZero).ToString());
                xw.WriteElementString("Length", Math.Round(this.Length, 1, MidpointRounding.AwayFromZero).ToString());
                xw.WriteElementString("Height", Math.Round(this.Height, 1, MidpointRounding.AwayFromZero).ToString());
            }
            else
            {
                xw.WriteElementString("Width", Math.Round(3.0, 1, MidpointRounding.AwayFromZero).ToString());
                xw.WriteElementString("Length", Math.Round(6.0, 1, MidpointRounding.AwayFromZero).ToString());
                xw.WriteElementString("Height", Math.Round(1.0, 1, MidpointRounding.AwayFromZero).ToString());
            }
            xw.WriteElementString("Girth", Math.Round(this.Girth(), 1, MidpointRounding.AwayFromZero).ToString());
            
            // Origin Zip
            xw.WriteElementString("OriginZip", this.ZipOrigination);

            // Commercial Flag
            xw.WriteElementString("CommercialFlag", this.CommercialRates ? "Y" : "N");
            
            // Extra Services
            if (this.ExtraServices.Count > 0)
            {
                xw.WriteStartElement("ExtraServices");
                foreach (InternationalExtraServiceType s in this.ExtraServices)
                {
                    xw.WriteElementString("ExtraService", ((int)s).ToString());
                }
                xw.WriteEndElement();
            }

            // End Package
            xw.WriteEndElement();
        }

        private string TranslateMailType(InternationalPackageType internationalPackageType)
        {
            switch (internationalPackageType)
            {
                case InternationalPackageType.All:
                    return "All";
                case InternationalPackageType.Envelope:
                    return "Envelope";
                case InternationalPackageType.MatterForTheBlind:
                    return "Matter for the blind";
                case InternationalPackageType.Package:
                    return "Package";
                case InternationalPackageType.PostCards:
                    return "Postcards or aerogrammes";
            }
            return "All";
        }

    }
}
