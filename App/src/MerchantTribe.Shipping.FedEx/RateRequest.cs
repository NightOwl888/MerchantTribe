using System.Xml;
using System.Text;
using System.IO;
using MerchantTribe.Shipping;
using System;

namespace MerchantTribe.Shipping.FedEx
{

    public class RateRequest
    {
        private MerchantTribe.Web.Logging.ILogger _logger = new MerchantTribe.Web.Logging.SupressLogger();

        private string LiveFedExServer = FedExConstants.LiveServiceUrl;
        private FedExGlobalServiceSettings globals = new FedExGlobalServiceSettings();
        private decimal _Weight = 0.1m;
        private decimal _DeclaredValue = 0m;
        public decimal Weight
        {
            get { return _Weight; }
            set { _Weight = Math.Round(value, 1, MidpointRounding.AwayFromZero); }
        }
        public decimal DeclaredValue
        {
            get { return _DeclaredValue; }
            set { _DeclaredValue = Math.Round(value, 2, MidpointRounding.AwayFromZero); }
        }

        public Header RequestHeader { get; set; }
        public ReturnShipmentIndicatorType ReturnType { get; set; }
        public DateTime ShipDate { get; set; }
        public ServiceType Service { get; set; }
        public PackageType Packaging { get; set; }
        public WeightType WeightUnits { get; set; }        
        public Address OriginAddress { get; set; }
        public Address DestinationAddress { get; set; }
        public DimensionsBlock Dimensions { get; set; }        
        public bool ContainsAlcohol { get; set; }
        public RateSpecialServices SpecialServices { get; set; }
        public int PackageCount { get; set; }

        public RateRequest(FedExGlobalServiceSettings globalSettings, MerchantTribe.Web.Logging.ILogger logger)
        {
            RequestHeader = new Header();
            ReturnType = ReturnShipmentIndicatorType.NONRETURN;
            ShipDate = DateTime.Now.AddHours(4);
            globals = globalSettings;
            _logger = logger;
            Service = ServiceType.FEDEXGROUND;
            Packaging = PackageType.YOURPACKAGING;
            WeightUnits = WeightType.LBS;
            OriginAddress = new Address();
            DestinationAddress = new Address();
            Dimensions = new DimensionsBlock();
            this.ContainsAlcohol = false;
            this.SpecialServices = new RateSpecialServices();
            this.PackageCount = 0;
        }

        

        public RateResponse Send()
        {
            return Send(this.LiveFedExServer);
        }

        public RateResponse Send(string serviceUrl)
        {
            RateResponse result = new RateResponse();

            string xmlToSend = BuildXml();
            string responseXml = RateService.SendRequest(serviceUrl, xmlToSend);
            result.Parse(responseXml);

            if (globals.DiagnosticsMode)
            {
                _logger.LogMessage("FedEx Diagnostics - Request=" 
                    + xmlToSend + "<br/>" 
                    + System.Environment.NewLine 
                    + "<br/>"
                    + System.Environment.NewLine 
                    + " Response=" 
                    + responseXml);
            }

            return result;
        }

        public string BuildXml()
        {
            string result = string.Empty;

            StringWriter sw = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            XmlTextWriter xw = new XmlTextWriter(sw);
            xw.Formatting = Formatting.Indented;
            xw.Indentation = 2;

            xw.WriteStartDocument();

            //Preamble
            xw.WriteStartElement("FDXRateRequest");
            xw.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            xw.WriteAttributeString("xsi:noNamespaceSchemaLocation", "FDXRateRequest.xsd");

            RequestHeader.WriteToXml(xw, "RequestHeader");
            xw.WriteElementString("ReturnShipmentIndicator", ReturnType.ToString());
            xw.WriteElementString("ShipDate", ShipDate.ToString("yyyy-MM-dd"));
            xw.WriteElementString("DropoffType", this.globals.DefaultDropOffType.ToString());
            xw.WriteElementString("Service", Service.ToString());
            xw.WriteElementString("Packaging", Packaging.ToString());
            xw.WriteElementString("WeightUnits", WeightUnits.ToString());
            xw.WriteElementString("Weight", _Weight.ToString("0.0"));                        
            xw.WriteElementString("ListRate", globals.UseListRates ? "1" : "0");            
            
            OriginAddress.WriteToXml(xw, "OriginAddress");
            DestinationAddress.WriteToXml(xw, "DestinationAddress");

            xw.WriteStartElement("Payment");
            xw.WriteElementString("PayorType", "SENDER");
            xw.WriteEndElement();

            if (Packaging == PackageType.YOURPACKAGING)
            {
                Dimensions.WriteToXml(xw, "Dimensions");
            }

            xw.WriteStartElement("DeclaredValue");
            xw.WriteElementString("Value", _DeclaredValue.ToString("0.00"));
            xw.WriteElementString("CurrencyCode", "USD");
            xw.WriteEndElement();

            if (ContainsAlcohol)
            {
                xw.WriteElementString("Alcohol", "1");
            }
            SpecialServices.WriteToXml(xw, "SpecialServices");
            xw.WriteElementString("PackageCount", PackageCount.ToString());

            //_RequestContact.WriteToXml(xw, "Contact")
            //_RequestAddress.WriteToXml(xw, "Address")

            xw.WriteEndDocument();

            xw.Flush();
            xw.Close();

            result = sw.GetStringBuilder().ToString();

            return result;
        }


    }

}