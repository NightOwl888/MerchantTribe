using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MerchantTribe.Shipping;
using System.Xml;
using System.Collections.ObjectModel;
using MerchantTribe.Web.Geography;

namespace MerchantTribe.Shipping.Ups
{
    public class UPSService: IShippingService
    {
        private List<ShippingServiceMessage> _Messages = new List<ShippingServiceMessage>();
        private List<IServiceCode> _Codes = new List<IServiceCode>();

        private MerchantTribe.Web.Logging.ILogger _Logger = new MerchantTribe.Web.Logging.SupressLogger();

        public string Name
        {
            get { return "UPS"; }
        }
        public string Id
        {
            get { return "55E5A698-1111-4F78-958B-70B1BC5941B8"; }
        }

        public UPSServiceGlobalSettings GlobalSettings { get; set; }
        public UPSServiceSettings Settings { get; set; }
        public ServiceSettings BaseSettings
        {
            get { return Settings; }
        }
        public List<ShippingServiceMessage> LatestMessages
        {
            get
            {
                return _Messages;
            }
            set
            {
                _Messages = value;
            }
        }

        public UPSService(UPSServiceGlobalSettings globalSettings, MerchantTribe.Web.Logging.ILogger logger)
        {
            _Logger = logger;
            GlobalSettings = globalSettings;
            Settings = new UPSServiceSettings();
            InitializeCodes();
        }
        private void InitializeCodes()
        {
            _Codes.Add(new ServiceCode() { Code = "1", DisplayName = "UPS Next Day Air" });
            _Codes.Add(new ServiceCode() { Code = "2", DisplayName = "UPS Second Day Air" });
            _Codes.Add(new ServiceCode() { Code = "3", DisplayName = "UPS Ground" });
            _Codes.Add(new ServiceCode() { Code = "7", DisplayName = "UPS Worldwide Express" });
            _Codes.Add(new ServiceCode() { Code = "8", DisplayName = "UPS Worldwide Expedited" });
            _Codes.Add(new ServiceCode() { Code = "11", DisplayName = "UPS Standard" });
            _Codes.Add(new ServiceCode() { Code = "12", DisplayName = "UPS Three Day Select" });
            _Codes.Add(new ServiceCode() { Code = "13", DisplayName = "UPS Next Day Air Saver" });
            _Codes.Add(new ServiceCode() { Code = "14", DisplayName = "UPS Next Day Air Early AM" });
            _Codes.Add(new ServiceCode() { Code = "54", DisplayName = "UPS Worldwide Express Plus" });
            _Codes.Add(new ServiceCode() { Code = "59", DisplayName = "UPS Second Day Air AM" });
            _Codes.Add(new ServiceCode() { Code = "65", DisplayName = "UPS Saver" });
        }

        public List<IServiceCode> ListAllServiceCodes()
        {
            return _Codes;
        }

        public List<IShippingRate> RateShipment(IShipment shipment)
        {
            _Messages.Clear();
            return GetUPSRatesForShipment(shipment);            
        }

        // Gets all rates filtered by service settings
        private List<IShippingRate> GetUPSRatesForShipment(IShipment shipment)
        {
            List<IShippingRate> rates = new List<IShippingRate>();

            List<IShippingRate> allrates = GetAllShippingRatesForShipment(shipment);

            // Filter all rates by just the ones we want
            List<IServiceCode> codefilters = this.Settings.ServiceCodeFilter;
            foreach (IShippingRate rate in allrates)
            {
                if (this.Settings.GetAllRates || codefilters.Count < 1)
                {
                    rates.Add(rate);
                    continue;
                }
                else
                {
                    if (codefilters.Where(y => y.Code == (rate.ServiceCodes.TrimStart('0'))).Count() > 0)
                    {
                        rates.Add(rate);
                        continue;
                    }
                }
            }

            return rates;
        }

        // Gets all available rates regardless of settings
        private List<IShippingRate> GetAllShippingRatesForShipment(IShipment shipment)
        {
            List<IShippingRate> rates = new List<IShippingRate>();

            bool hasErrors = false;
            const string UPSLIVESERVER = @"https://www.ups.com/ups.app/xml/";

            try
            {
                

                string sErrorMessage = "";
                string sErrorCode = "";

                string sURL = "";
                sURL = UPSLIVESERVER;
                sURL += "Rate";

                // Build XML
                string sXML = "";
                UpsSettings settings = new UpsSettings();

                settings.UserID = GlobalSettings.Username;
                settings.Password = GlobalSettings.Password;
                settings.ServerUrl = UPSLIVESERVER;
                settings.License = GlobalSettings.LicenseNumber;

                sXML = XmlTools.BuildAccessKey(settings);
                sXML += "\n";


                sXML += BuildUPSRateRequestForShipment(shipment);

                if (GlobalSettings.DiagnosticsMode)
                {
                    _Logger.LogMessage("UPS Request: " + sXML);
                    ShippingServiceMessage m = new ShippingServiceMessage();
                    m.SetDiagnostics("", "Request:" + sXML);
                    _Messages.Add(m);                    
                }

              
                string sResponse = "";
                sResponse = XmlTools.ReadHtmlPage_POST(sURL, sXML);

                if (GlobalSettings.DiagnosticsMode)
                {
                    _Logger.LogMessage("UPS Response: " + sResponse);
                    ShippingServiceMessage m = new ShippingServiceMessage();
                    m.SetDiagnostics("","Response:" + sResponse);                    
                    _Messages.Add(m);
                }

                                          
                XmlDocument xDoc;
                XmlNodeList NodeList;
                string sStatusCode = "-1";

                // Rated Packages will be used to return a list of suggested packages
                Collection<Package> ratedPackages = new Collection<Package>();

                try
                {
                    xDoc = new XmlDocument();
                    xDoc.LoadXml(sResponse);

                    if (xDoc.DocumentElement.Name == "RatingServiceSelectionResponse")
                    {
                        XmlNode n;
                        int i = 0;
                        XmlNode nTag;

                        NodeList = xDoc.GetElementsByTagName("RatingServiceSelectionResponse");
                        n = NodeList.Item(0);
                        for (i = 0; i <= n.ChildNodes.Count - 1; i++)
                        {
                            nTag = n.ChildNodes.Item(i);
                            switch (nTag.Name)
                            {
                                case "Response":
                                    int iRes = 0;
                                    XmlNode nRes;
                                    for (iRes = 0; iRes <= nTag.ChildNodes.Count - 1; iRes++)
                                    {
                                        nRes = nTag.ChildNodes[iRes];
                                        switch (nRes.Name)
                                        {
                                            case "ResponseStatusCode":
                                                sStatusCode = nRes.FirstChild.Value;
                                                break;
                                            case "ResponseStatusDescription":
                                                // Not Used
                                                break;
                                            case "Error":
                                                int iErr = 0;
                                                XmlNode nErr;
                                                for (iErr = 0; iErr <= nRes.ChildNodes.Count - 1; iErr++)
                                                {
                                                    nErr = nRes.ChildNodes[iErr];
                                                    switch (nErr.Name)
                                                    {
                                                        case "ErrorCode":
                                                            sErrorCode = nErr.FirstChild.Value;
                                                            break;
                                                        case "ErrorDescription":
                                                            sErrorMessage = nErr.FirstChild.Value;
                                                            break;
                                                        case "ErrorSeverity":
                                                            // Not Used
                                                            break;
                                                    }
                                                }
                                                break;

                                        }
                                    }
                                    break;
                                case "RatedShipment":

                                    int iRated = 0;
                                    XmlNode nRated;

                                    string sPostage = string.Empty;
                                    string sCurrencyCode =string.Empty;
                                    string sCode = string.Empty;
                                    string sDescription = string.Empty;

                                    for (iRated = 0; iRated <= nTag.ChildNodes.Count - 1; iRated++)
                                    {
                                        nRated = nTag.ChildNodes[iRated];
                                        switch (nRated.Name)
                                        {
                                            case "Service":
                                                int iServices = 0;
                                                XmlNode nServices;
                                                for (iServices = 0; iServices <= nRated.ChildNodes.Count - 1; iServices++)
                                                {
                                                    nServices = nRated.ChildNodes[iServices];
                                                    switch (nServices.Name)
                                                    {
                                                        case "Code":
                                                            sCode = nServices.FirstChild.Value;
                                                            sDescription = DecodeUpsServiceCode(sCode);
                                                            break;
                                                        case "Description":
                                                            sDescription = nServices.FirstChild.Value;
                                                            break;
                                                    }
                                                }
                                                break;
                                            case "TotalCharges":
                                                int iCharges = 0;
                                                XmlNode nCharges;
                                                for (iCharges = 0; iCharges <= nRated.ChildNodes.Count - 1; iCharges++)
                                                {
                                                    nCharges = nRated.ChildNodes[iCharges];
                                                    switch (nCharges.Name)
                                                    {
                                                        case "MonetaryValue":
                                                            sPostage = nCharges.FirstChild.Value;
                                                            break;
                                                        case "CurrencyCode":
                                                            sCurrencyCode = nCharges.FirstChild.Value;
                                                            break;
                                                    }
                                                }
                                                break;

                                        }
                                    }

                                    decimal dRate = -1;

                                    if (sPostage.Length > 0)
                                    {
                                        dRate = decimal.Parse(sPostage, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        ShippingServiceMessage nop = new ShippingServiceMessage();
                                        nop.SetInfo("","No UPS Postage Found");
                                        _Messages.Add(nop);
                                        hasErrors = true;
                                    }

                                    if (dRate >= 0)
                                    {
                                        ShippingRate r =new ShippingRate();
                                        r.DisplayName = sDescription;
                                        r.EstimatedCost = dRate;
                                        r.ServiceCodes = sCode;
                                        r.ServiceId = this.Id;
                                        rates.Add(r);
                                    }

                                    if (GlobalSettings.DiagnosticsMode)
                                    {
                                        ShippingServiceMessage msg = new ShippingServiceMessage();
                                        msg.SetDiagnostics("UPS Rates Found", "StatusCode=" + sStatusCode + ",Postage=" + sPostage + ",Errors=" + sErrorMessage + ",Rate=" + dRate.ToString());
                                        _Messages.Add(msg);
                                        _Logger.LogMessage("UPS Rates Found: StatusCode=" + sStatusCode + ",Postage=" + sPostage + ",Errors=" + sErrorMessage + ",Rate=" + dRate.ToString());
                                    }

                                    break;

                            }
                        }
                    }
                    else
                    {
                        hasErrors = true;
                        sErrorMessage = "Couldn't find valid XML response from server.";
                    }
                }

                catch (Exception Exx)
                {
                    _Logger.LogException(Exx);
                    ShippingServiceMessage mex = new ShippingServiceMessage();
                    mex.SetError("Exception", Exx.Message + " | " + Exx.Source);
                    _Messages.Add(mex);
                    return rates;
                }

                if (sStatusCode != "1")
                {
                    hasErrors = true;
                }
                              
            }

            catch (Exception ex)
            {
                _Logger.LogException(ex);
                ShippingServiceMessage m = new ShippingServiceMessage();
                m.SetError("Exception", ex.Message + " | " + ex.StackTrace);
                _Messages.Add(m);                  
            }

            if (hasErrors)
            {
                rates = new List<IShippingRate>();
            }
            return rates;
        }

        private string DecodeUpsServiceCode(string sCode)
        {
            string temp = sCode;
            if (temp.StartsWith("0"))
            {
                temp = temp.Substring(1, temp.Length - 1);
            }

            foreach (IServiceCode code in _Codes)
            {
                if (code.Code == temp)
                {
                    return code.DisplayName;
                }
            }

            return "UPS";
        }

        private string BuildUPSRateRequestForShipment(IShipment shipment)
        {
            string sXML = "";
            
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            XmlTextWriter xw = new XmlTextWriter(strWriter);

            try
            {                
                xw.Formatting = Formatting.Indented;
                xw.Indentation = 3;

                //xw.WriteStartDocument();

                //--------------------------------------------            
                // Agreement Request
                xw.WriteStartElement("RatingServiceSelectionRequest");

                //--------------------------------------------
                // Request
                xw.WriteStartElement("Request");
                //--------------------------------------------
                // TransactionReference
                xw.WriteStartElement("TransactionReference");
                xw.WriteElementString("CustomerContext", "Rate Request");
                xw.WriteElementString("XpciVersion", "1.0001");
                xw.WriteEndElement();
                // End TransactionReference
                //--------------------------------------------
                //xw.WriteElementString("RequestAction", "Rate");
                xw.WriteElementString("RequestOption", "Shop");
                xw.WriteEndElement();
                // End Request
                //--------------------------------------------

                //--------------------------------------------
                // Pickup Type
                if (GlobalSettings.PickUpType != PickupType.Unknown)
                {
                    string pickupCode = ((int)GlobalSettings.PickUpType).ToString();
                    if (pickupCode.Trim().Length < 2)
                    {
                        pickupCode = "0" + pickupCode;
                    }
                    xw.WriteStartElement("PickupType");
                    xw.WriteElementString("Code", pickupCode);
                    xw.WriteEndElement();
                }
                // End Pickup TYpe
                //--------------------------------------------

                //--------------------------------------------
                // Shipment
                xw.WriteStartElement("Shipment");

                // Shipper
                xw.WriteStartElement("Shipper");
                xw.WriteStartElement("Address");

                //Use City name for countries that don't have postal codes
                if (shipment.SourceAddress.PostalCode.Trim().Length > 0)
                {
                    xw.WriteElementString("PostalCode", XmlTools.TrimToLength(shipment.SourceAddress.PostalCode.Trim(), 9));
                }
                else
                {
                    xw.WriteElementString("City", XmlTools.TrimToLength(shipment.SourceAddress.City.Trim(), 30));
                }

                xw.WriteElementString("CountryCode", Country.FindByBvin(shipment.SourceAddress.CountryData.Bvin).IsoCode);
                xw.WriteEndElement();
                xw.WriteEndElement();

                // Ship To
                xw.WriteStartElement("ShipTo");
                xw.WriteStartElement("Address");

                if (shipment.DestinationAddress.PostalCode.Length > 0)
                {
                    xw.WriteElementString("PostalCode", shipment.DestinationAddress.PostalCode);
                }
                else
                {
                    if (shipment.DestinationAddress.City.Length > 0)
                    {
                        xw.WriteElementString("City", shipment.DestinationAddress.City);
                    }
                }
                if (shipment.DestinationAddress.CountryData.Bvin.Length > 0)
                {
                    xw.WriteElementString("CountryCode", Country.FindByBvin(shipment.DestinationAddress.CountryData.Bvin).IsoCode);
                }
                if (GlobalSettings.ForceResidential)
                {
                    xw.WriteElementString("ResidentialAddress", "");
                }
                //else
                //{
                //    if (package.DestinationAddress.Company.Trim().Length > 0)
                //    {
                //    }
                //    // Do nothing
                //    else
                //    {
                //        xw.WriteElementString("ResidentialAddress", "");
                //    }
                //}
                xw.WriteEndElement();
                xw.WriteEndElement();

                // Service
                // Ignore service code to get back all rates
                //xw.WriteStartElement("Service");

                //string stringServiceCode = serviceCode.Code;
                //if (stringServiceCode.Length < 2)
                //{
                //    stringServiceCode = "0" + stringServiceCode;
                //}
                //xw.WriteElementString("Code", stringServiceCode);
                //xw.WriteEndElement();

                bool ignoreDimensions = GlobalSettings.IgnoreDimensions;
                //shipment.GenerateDimensions();

                // Optimize Packages for Weight
                List<IShippable> optimizedPackages = this.OptimizeSingleGroup(shipment);

                foreach (IShippable p in optimizedPackages)
                {
                    WriteSingleUPSPackage(ref xw, p, ignoreDimensions);
                }


                if (Settings.NegotiatedRates)
                {
                    xw.WriteStartElement("RateInformation");
                    xw.WriteElementString("NegotiatedRatesIndicator", string.Empty);
                    xw.WriteEndElement();
                }

                xw.WriteEndElement();
                // End Shipment
                //--------------------------------------------

                xw.WriteEndElement();
                // End Agreement Request
                //--------------------------------------------

                xw.WriteEndDocument();
                xw.Flush();
                xw.Close();

            }
            catch (Exception ex)
            {
                _Logger.LogException(ex);
            }

            sXML = strWriter.GetStringBuilder().ToString();

            return sXML;
        }

        protected bool IsOversized(IShippable prod)
        {
            bool IsOversize = false;
            double girth = (double)(prod.BoxLength + (2 * prod.BoxHeight) + (2 * prod.BoxWidth));
            if ((girth - 84 > 0))
            {
                //this is an oversize product
                IsOversize = true;
            }

            return IsOversize;
        }

        private class DimensionAmount
        {
            public decimal Amount { get; set; }
            public DimensionAmount(decimal amount)
            {
                Amount = amount;
            }
        }

        private void WriteSingleUPSPackage(ref XmlTextWriter xw, IShippable pak, bool ignoreDimensions)
        {
            decimal dGirth = 0;
            decimal dLength = 0;
            decimal dHeight = 0;
            decimal dwidth = 0;            
            
            List<DimensionAmount> dimensions = new List<DimensionAmount>();

            if (pak.BoxLengthType == LengthType.Centimeters)
            {
                dimensions.Add(new DimensionAmount(MerchantTribe.Web.Conversions.CentimetersToInches(pak.BoxLength)));
                dimensions.Add(new DimensionAmount(MerchantTribe.Web.Conversions.CentimetersToInches(pak.BoxWidth)));
                dimensions.Add(new DimensionAmount(MerchantTribe.Web.Conversions.CentimetersToInches(pak.BoxHeight)));
            }
            else
            {
                dimensions.Add(new DimensionAmount(pak.BoxLength));
                dimensions.Add(new DimensionAmount(pak.BoxWidth));
                dimensions.Add(new DimensionAmount(pak.BoxWidth));
            }

            List<decimal> sorted = (from d in dimensions
                                    orderby d.Amount descending
                                    select d.Amount).ToList();                                    
            dLength = sorted[0];
            dwidth = sorted[1];
            dHeight = sorted[2];
            
            dGirth = dwidth + dwidth + dHeight + dHeight;


            //--------------------------------------------
            // Package
            xw.WriteStartElement("Package");

            xw.WriteStartElement("PackagingType");

            string packageType = "02";
            if ((GlobalSettings.DefaultPackaging != (int)PackagingType.Unknown))
            {
                packageType = ((int)GlobalSettings.DefaultPackaging).ToString();
                if (packageType.Trim().Length < 2)
                {
                    packageType = "0" + packageType;
                }
            }
            xw.WriteElementString("Code", packageType);
            xw.WriteElementString("Description", "Package");
            xw.WriteEndElement();

            //Dimensions can be skipped in latest UPS specs
            if (ignoreDimensions == false)
            {
                if (dLength > 0 | dHeight > 0 | dwidth > 0)
                {
                    xw.WriteStartElement("Dimensions");
                    xw.WriteStartElement("UnitOfMeasure");
                    xw.WriteElementString("Code", "IN");
                    xw.WriteEndElement();
                    xw.WriteElementString("Length", Math.Round(dLength, 2, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    xw.WriteElementString("Width", Math.Round(dwidth, 2, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    xw.WriteElementString("Height", Math.Round(dHeight, 2, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    xw.WriteEndElement();
                }
            }

            if (pak.BoxWeight > 0)
            {
                xw.WriteStartElement("PackageWeight");
                xw.WriteStartElement("UnitOfMeasure");
                if (pak.BoxWeightType == WeightType.Pounds)
                {
                    xw.WriteElementString("Code", "LBS");
                }
                else
                {
                    xw.WriteElementString("Code", "KGS");
                }
                xw.WriteEndElement();
                xw.WriteElementString("Weight", Math.Round(pak.BoxWeight, 1, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture));
                xw.WriteEndElement();
            }
            else
            {
                xw.WriteStartElement("PackageWeight");
                xw.WriteStartElement("UnitOfMeasure");
                if (pak.BoxWeightType == WeightType.Pounds)
                {
                    xw.WriteElementString("Code", "LBS");
                }
                else
                {
                    xw.WriteElementString("Code", "KGS");
                }

                xw.WriteEndElement();
                xw.WriteElementString("Weight", Math.Round(0.1, 1, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture));
                xw.WriteEndElement();
            }

            if (ignoreDimensions == false)
            {
                // Oversize Checks
                decimal oversizeCheck = dGirth + dLength;
                if (oversizeCheck > 84)
                {
                    if (oversizeCheck < 108 & pak.BoxWeight < 30)
                    {
                        xw.WriteElementString("OversizePackage", "1");
                    }
                    else
                    {
                        if (pak.BoxWeight < 70)
                        {
                            xw.WriteElementString("OversizePackage", "2");
                        }
                        else
                        {
                            xw.WriteElementString("OversizePackage", "0");
                        }
                    }
                }
            }

            //Package Service Options
            //xw.WriteStartElement("PackageServiceOptions")
            //xw.WriteStartElement("InsuredValue")
            //xw.WriteElementString("CurrencyCode", "USD")
            //xw.WriteElementString("MonetaryValue", Math.Round(pak.DeclaredValue, 2, MidpointRounding.AwayFromZero).ToString())
            //xw.WriteEndElement()
            //xw.WriteEndElement()

            xw.WriteEndElement();
            // End Package
            //--------------------------------------------

        }

        private List<IShippable> OptimizeSingleGroup(IShipment shipment)
        {

            const decimal MAXWEIGHT = 70;

            List<IShippable> result = new List<IShippable>();


            List<IShippable> itemsToSplit = new List<IShippable>();
            
            foreach (IShippable item in shipment.Items)
            {
                if (IsOversized(item))
                {
                    result.Add(item.CloneShippable());
                }
                else
                {
                    itemsToSplit.Add(item);
                }
            }


            IShippable tempPackage = new Shippable();

            foreach (IShippable pak in itemsToSplit)
            {
                if (MAXWEIGHT - tempPackage.BoxWeight >= pak.BoxWeight)
                {
                    // add to current box
                    tempPackage.BoxWeight += pak.BoxWeight;
                    tempPackage.QuantityOfItemsInBox += pak.QuantityOfItemsInBox;
                    tempPackage.BoxValue += pak.BoxValue;
                }
                else
                {
                    // Save the temp package if it has items
                    if (tempPackage.BoxWeight > 0 || tempPackage.QuantityOfItemsInBox > 0)
                    {
                        result.Add(tempPackage.CloneShippable());
                        tempPackage = new Shippable();
                    }

                    // create new box
                    if (pak.BoxWeight > MAXWEIGHT)
                    {
                        //Loop to break up > maxWeight Packages
                        int currentItemsInBox = pak.QuantityOfItemsInBox;
                        decimal currentWeight = pak.BoxWeight;

                        while (currentWeight > 0)
                        {
                            if (currentWeight > MAXWEIGHT)
                            {
                                IShippable newP = pak.CloneShippable();
                                newP.BoxWeight = MAXWEIGHT;
                                if (currentItemsInBox > 0)
                                {
                                    currentItemsInBox -= 1;
                                    newP.QuantityOfItemsInBox = 1;
                                }
                                result.Add(newP);
                                currentWeight = currentWeight - MAXWEIGHT;
                                if (currentWeight < 0)
                                {
                                    currentWeight = 0;
                                }
                            }
                            else
                            {
                                // Create a new shippable box 
                                IShippable newP = pak.CloneShippable();
                                newP.BoxWeight = currentWeight;
                                if (currentItemsInBox > 0)
                                {
                                    newP.QuantityOfItemsInBox = currentItemsInBox;                                    
                                }                                
                                result.Add(newP);
                                currentWeight = 0;
                            }
                        }
                    }
                    else
                    {
                        tempPackage = pak.CloneShippable();
                    }
                }             
            }

            // Save the temp package if it has items
            if (tempPackage.BoxWeight > 0 || tempPackage.QuantityOfItemsInBox > 0)
            {
                result.Add(tempPackage.CloneShippable());
                tempPackage = new Shippable();
            }

            return result;
        }    

    }
}
