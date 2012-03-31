
using System.IO;
using System.Xml;
using System.Net;
using System.Collections;
using System;

namespace MerchantTribe.Shipping.Ups
{

    public sealed class XmlTools
    {

        protected internal const string DEVELOPER_KEY = "EB74C27E60949E4C";

        private XmlTools()
        {

        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// builds XML access key for UPS requests
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[mmcconnell]	11/1/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string BuildAccessKey(UpsSettings settings)
        {
            string sXML = "";
            StringWriter strWriter = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(strWriter);

            xw.Formatting = Formatting.Indented;
            xw.Indentation = 3;

            xw.WriteStartDocument();

            //--------------------------------------------            
            // Agreement Request
            xw.WriteStartElement("AccessRequest");

            xw.WriteElementString("AccessLicenseNumber", settings.License);
            xw.WriteElementString("UserId", settings.UserID);
            xw.WriteElementString("Password", settings.Password);

            xw.WriteEndElement();
            // End Agreement Request
            //--------------------------------------------

            xw.WriteEndDocument();
            xw.Flush();
            xw.Close();

            sXML = strWriter.GetStringBuilder().ToString();

            xw = null;

            //HttpContext.Current.Trace.Write("AccessRequest=" & sXML)

            return sXML;
        }

        public static string ReadHtmlPage_POST(string sURL, string sPostData)
        {
            HttpWebResponse objResponse;
            HttpWebRequest objRequest;
            string result = string.Empty;
            //Dim myWriter As StreamWriter
            byte[] bytes;

            // Encode Post Stream
            try
            {
                bytes = System.Text.Encoding.Default.GetBytes(sPostData);
            }
            catch (Exception Ex1)
            {
                throw new ArgumentException("Setup Bytes Exception: " + Ex1.Message);
            }

            // Create Request
            try
            {
                objRequest = (HttpWebRequest)WebRequest.Create(sURL);
                objRequest.Method = "POST";
                objRequest.ContentLength = bytes.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";
            }
            catch (Exception E)
            {
                throw new ArgumentException("Error Creating Web Request: " + E.Message);
            }

            // Dump Post Data to Request Stream
            try
            {
                Stream OutStream = objRequest.GetRequestStream();
                OutStream.Write(bytes, 0, bytes.Length);
                OutStream.Close();
            }
            catch (Exception Ex)
            {
                throw new ArgumentException("Error Posting Data: " + Ex.Message + " " + Ex.Source + " " + Ex.StackTrace);
            }
            finally
            {
            }

            // Read Response
            try
            {
                objResponse = (HttpWebResponse)objRequest.GetResponse();
                StreamReader sr = new StreamReader(objResponse.GetResponseStream(), System.Text.Encoding.Default, true);
                result += sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception Exx)
            {
                throw new ArgumentException("Stream Reader Error: " + Exx.Message + " " + Exx.Source);
            }

            return result;
        }

        public static string CleanPhoneNumber(string sNumber)
        {
            string input = sNumber.Trim();
            input = System.Text.RegularExpressions.Regex.Replace(input, @"[^0-9]","");
            return input;
            
            //string sRet = "";
            //int iCounter = 0;
            //string sChar = "";

            //for (iCounter = 0; iCounter <= sNumber.Length - 1; iCounter++)
            //{
            //    sChar = sNumber.Substring(iCounter, 1);
            //    if (Convert.ToInt32(sChar) >= 48 & Convert.ToInt32(sChar) <= 57)
            //    {
            //        sRet += sChar;
            //    }
            //}

            //return sRet;
        }

        public static string TrimToLength(string input, int maxLength)
        {
            if (input == null)
            {
                return string.Empty;
            }
            else
            {
                if (input.Length < 1)
                {
                    return input;
                }
                else
                {
                    if (maxLength < 0)
                    {
                        maxLength = input.Length;
                    }

                    if (input.Length > maxLength)
                    {
                        return input.Substring(0, maxLength);
                    }
                    else
                    {
                        return input;
                    }
                }
            }

        }
        #region " Conversion Methods "

        public static string ConvertCurrencyCodeToString(CurrencyCode c)
        {
            string result = "USD";

            switch (c)
            {
                case CurrencyCode.AustalianDollar:
                    result = "AUD";
                    break;
                case CurrencyCode.Baht:
                    result = "THB";
                    break;
                case CurrencyCode.BritishPounds:
                    result = "GBP";
                    break;
                case CurrencyCode.CanadianDollar:
                    result = "CAD";
                    break;
                case CurrencyCode.DenmarkKrone:
                    result = "DKK";
                    break;
                case CurrencyCode.Drachma:
                    result = "GRD";
                    break;
                case CurrencyCode.Euro:
                    result = "EUR";
                    break;
                case CurrencyCode.HongKongDollar:
                    result = "HKD";
                    break;
                case CurrencyCode.NewZealandDollar:
                    result = "NZD";
                    break;
                case CurrencyCode.NorwayKrone:
                    result = "NOK";
                    break;
                case CurrencyCode.Peso:
                    result = "MXN";
                    break;
                case CurrencyCode.Ringgit:
                    result = "MYR";
                    break;
                case CurrencyCode.SingaporeDollar:
                    result = "SGD";
                    break;
                case CurrencyCode.SwedishKrona:
                    result = "SEK";
                    break;
                case CurrencyCode.SwissFranc:
                    result = "CHF";
                    break;
                case CurrencyCode.TaiwanDollar:
                    result = "TWD";
                    break;
                case CurrencyCode.UsDollar:
                    result = "USD";
                    break;
                default:
                    result = "USD";
                    break;
            }

            return result;
        }

        public static CurrencyCode ConvertStringToCurrencyCode(string s)
        {
            CurrencyCode result = CurrencyCode.UsDollar;

            switch (s.ToUpper())
            {
                case "AUD":
                    result = CurrencyCode.AustalianDollar;
                    break;
                case "THB":
                    result = CurrencyCode.Baht;
                    break;
                case "GBP":
                    result = CurrencyCode.BritishPounds;
                    break;
                case "CAD":
                    result = CurrencyCode.CanadianDollar;
                    break;
                case "DKK":
                    result = CurrencyCode.DenmarkKrone;
                    break;
                case "GRD":
                    result = CurrencyCode.Drachma;
                    break;
                case "EUR":
                    result = CurrencyCode.Euro;
                    break;
                case "HKD":
                    result = CurrencyCode.HongKongDollar;
                    break;
                case "NZD":
                    result = CurrencyCode.NewZealandDollar;
                    break;
                case "NOK":
                    result = CurrencyCode.NorwayKrone;
                    break;
                case "MXN":
                    result = CurrencyCode.Peso;
                    break;
                case "MYR":
                    result = CurrencyCode.Ringgit;
                    break;
                case "SGD":
                    result = CurrencyCode.SingaporeDollar;
                    break;
                case "SEK":
                    result = CurrencyCode.SwedishKrona;
                    break;
                case "CHF":
                    result = CurrencyCode.SwissFranc;
                    break;
                case "TWD":
                    result = CurrencyCode.TaiwanDollar;
                    break;
                case "USD":
                    result = CurrencyCode.UsDollar;
                    break;
                default:
                    result = CurrencyCode.UsDollar;
                    break;
            }

            return result;
        }

        public static UnitsType ConvertStringToUnits(string s)
        {
            UnitsType result = UnitsType.Imperial;

            switch (s.ToUpper())
            {
                case "LBS":
                    result = UnitsType.Imperial;
                    break;
                case "IN":
                    result = UnitsType.Imperial;
                    break;
                case "KGS":
                    result = UnitsType.Metric;
                    break;
                case "CM":
                    result = UnitsType.Metric;
                    break;
                default:
                    result = UnitsType.Imperial;
                    break;
            }

            return result;
        }

        public static ShipLabelFormat ConvertStringToLabelFormat(string s)
        {
            ShipLabelFormat result = ShipLabelFormat.Gif;

            switch (s.ToUpper())
            {
                case "GIF":
                    result = ShipLabelFormat.Gif;
                    break;
                case "EPL":
                    result = ShipLabelFormat.Epl2;
                    break;
                default:
                    result = ShipLabelFormat.Gif;
                    break;
            }

            return result;
        }

        public static string ConvertReferenceNumberCodeToString(ReferenceNumberCode c)
        {
            string result = "TN";

            switch (c)
            {
                case ReferenceNumberCode.AccountsRecievableCustomerAccount:
                    result = "AJ";
                    break;
                case ReferenceNumberCode.AppropriationNumber:
                    result = "AT";
                    break;
                case ReferenceNumberCode.BillOfLadingNumber:
                    result = "BM";
                    break;
                case ReferenceNumberCode.CodNumber:
                    result = "9V";
                    break;
                case ReferenceNumberCode.DealerOrderNumber:
                    result = "ON";
                    break;
                case ReferenceNumberCode.DepartmentNumber:
                    result = "DP";
                    break;
                case ReferenceNumberCode.EmployersIDNumber:
                    result = "EI";
                    break;
                case ReferenceNumberCode.FdaProductCode:
                    result = "3Q";
                    break;
                case ReferenceNumberCode.FederalTaxPayerIDNumber:
                    result = "TJ";
                    break;
                case ReferenceNumberCode.InvoiceNumber:
                    result = "IK";
                    break;
                case ReferenceNumberCode.ManifestKeyNumber:
                    result = "MK";
                    break;
                case ReferenceNumberCode.ModelNumber:
                    result = "MJ";
                    break;
                case ReferenceNumberCode.PartNumber:
                    result = "PM";
                    break;
                case ReferenceNumberCode.ProductionCode:
                    result = "PC";
                    break;
                case ReferenceNumberCode.PurchaseOrderNumber:
                    result = "PO";
                    break;
                case ReferenceNumberCode.PurchaseRequisitionNumber:
                    result = "RQ";
                    break;
                case ReferenceNumberCode.ReturnAuthorizationNumber:                    
                    result = "RZ";
                    break;
                case ReferenceNumberCode.SalesPersonNumber:
                    result = "SA";
                    break;
                case ReferenceNumberCode.SerialNumber:
                    result = "SE";
                    break;
                case ReferenceNumberCode.SocialSecurityNumber:
                    result = "SY";
                    break;
                case ReferenceNumberCode.StoreNumber:
                    result = "ST";
                    break;
                case ReferenceNumberCode.TransactionReferenceNumber:
                    result = "TN";
                    break;
                default:
                    result = "TN";
                    break;
            }

            return result;
        }
        #endregion
        #region " Write Entity Methods "

        public static bool WriteSingleUpsPackage(ref XmlTextWriter xw, ref Package p)
        {
            bool result = true;

            decimal dGirth = 0;

            decimal dLength = 0;
            decimal dHeight = 0;
            decimal dwidth = 0;

            SortedList sar = new SortedList(2);
            sar.Add(1, p.Length);
            sar.Add(2, p.Width);
            sar.Add(3, p.Height);

            IDictionaryEnumerator myEnumerator = sar.GetEnumerator();
            int place = 0;
            while (myEnumerator.MoveNext())
            {
                place += 1;
                switch (place)
                {
                    case 1:
                        dLength = Convert.ToDecimal(myEnumerator.Value);
                        break;
                    case 2:
                        dwidth = Convert.ToDecimal(myEnumerator.Value);
                        break;
                    case 3:
                        dHeight = Convert.ToDecimal(myEnumerator.Value);
                        break;
                }
            }
            myEnumerator = null;

            dGirth = dwidth + dwidth + dHeight + dHeight;


            //--------------------------------------------
            // Package
            xw.WriteStartElement("Package");

            xw.WriteStartElement("PackagingType");
            string packagingCode = Convert.ToInt32(p.Packaging).ToString();
            if (packagingCode.Length < 2)
            {
                packagingCode = "0" + packagingCode;
            }
            xw.WriteElementString("Code", packagingCode);
            if (p.Description != string.Empty)
            {
                xw.WriteElementString("Description", p.Description);
            }
            xw.WriteEndElement();

            if (p.Packaging == PackagingType.CustomerSupplied)
            {
                if (dLength > 0 | dHeight > 0 | dwidth > 0)
                {
                    xw.WriteStartElement("Dimensions");
                    xw.WriteStartElement("UnitOfMeasure");
                    switch (p.DimensionalUnits)
                    {
                        case UnitsType.Imperial:
                            xw.WriteElementString("Code", "IN");
                            break;
                        case UnitsType.Metric:
                            xw.WriteElementString("Code", "CM");
                            break;
                        default:
                            xw.WriteElementString("Code", "IN");
                            break;
                    }
                    xw.WriteEndElement();
                    xw.WriteElementString("Length", Math.Round(dLength, 0, MidpointRounding.AwayFromZero).ToString());
                    xw.WriteElementString("Width", Math.Round(dwidth, 0, MidpointRounding.AwayFromZero).ToString());
                    xw.WriteElementString("Height", Math.Round(dHeight, 0, MidpointRounding.AwayFromZero).ToString());
                    xw.WriteEndElement();
                }
            }

            // Weight
            if (p.Weight > 0)
            {
                xw.WriteStartElement("PackageWeight");
                xw.WriteStartElement("UnitOfMeasure");
                switch (p.WeightUnits)
                {
                    case UnitsType.Imperial:
                        xw.WriteElementString("Code", "LBS");
                        break;
                    case UnitsType.Metric:
                        xw.WriteElementString("Code", "KGS");
                        break;
                    default:
                        xw.WriteElementString("Code", "LBS");
                        break;
                }
                xw.WriteEndElement();
                xw.WriteElementString("Weight", Math.Round(p.Weight, 1, MidpointRounding.AwayFromZero).ToString());
                xw.WriteEndElement();
            }

            // Oversize Checks
            decimal oversizeCheck = dGirth + dLength;
            if (oversizeCheck > 84)
            {
                if (oversizeCheck < 108 & p.Weight < 30)
                {
                    xw.WriteElementString("OversizePackage", "1");
                }
                else
                {
                    if (p.Weight < 70)
                    {
                        xw.WriteElementString("OversizePackage", "2");
                    }
                    else
                    {
                        xw.WriteElementString("OversizePackage", "0");
                    }
                }
            }

            // ReferenceNumber
            if (p.ReferenceNumber != string.Empty)
            {
                xw.WriteStartElement("ReferenceNumber");
                string codeType = ConvertReferenceNumberCodeToString(p.ReferenceNumberType);
                xw.WriteElementString("Code", codeType);
                xw.WriteElementString("Value", XmlTools.TrimToLength(p.ReferenceNumber, 35));
                xw.WriteEndElement();
            }
            // ReferenceNumber2
            if (p.ReferenceNumber2 != string.Empty)
            {
                xw.WriteStartElement("ReferenceNumber");
                string codeType = ConvertReferenceNumberCodeToString(p.ReferenceNumber2Type);
                xw.WriteElementString("Code", codeType);
                xw.WriteElementString("Value", XmlTools.TrimToLength(p.ReferenceNumber2, 35));
                xw.WriteEndElement();
            }

            // Additional Handling
            if (p.AdditionalHandlingIsRequired == true)
            {
                xw.WriteElementString("AdditionalHandling", "");
            }

            //-------------------------------------------
            // Start Service Options
            xw.WriteStartElement("PackageServiceOptions");


            // Delivery Confirmation
            if (p.DeliveryConfirmation == true)
            {
                xw.WriteStartElement("DeliveryConfirmation");
                xw.WriteElementString("DCISType", Convert.ToInt32(p.DeliveryConfirmationType).ToString());
                if (p.DeliveryConfirmationControlNumber != string.Empty)
                {
                    xw.WriteElementString("DCISNumber", XmlTools.TrimToLength(p.DeliveryConfirmationControlNumber, 11));
                }
                xw.WriteEndElement();
            }

            // InsuredValue
            if (p.InsuredValue > 0)
            {
                xw.WriteStartElement("InsuredValue");
                xw.WriteElementString("MonetaryValue", p.InsuredValue.ToString());
                xw.WriteElementString("CurrencyCode", XmlTools.ConvertCurrencyCodeToString(p.InsuredValueCurrency));
                xw.WriteEndElement();
            }

            // COD
            if (p.COD == true)
            {
                xw.WriteStartElement("COD");
                xw.WriteElementString("CODCode", "3");
                xw.WriteElementString("CODFundsCode", Convert.ToInt32(p.CODPaymentType).ToString());
                xw.WriteStartElement("CODAmount");
                xw.WriteElementString("CurrencyCode", XmlTools.ConvertCurrencyCodeToString(p.CODCurrencyCode));
                xw.WriteElementString("MonetaryValue", p.CODAmount.ToString());
                xw.WriteEndElement();
                xw.WriteEndElement();
            }

            // Verbal Confirmation
            if (p.VerbalConfirmation == true)
            {
                xw.WriteStartElement("VerbalConfirmation");
                xw.WriteStartElement("ContactInfo");
                if (p.VerbalConfirmationName != string.Empty)
                {
                    xw.WriteElementString("Name", p.VerbalConfirmationName);
                }
                if (p.VerbalConfirmationPhoneNumber != string.Empty)
                {
                    xw.WriteElementString("PhoneNumber", p.VerbalConfirmationPhoneNumber);
                }
                xw.WriteEndElement();
                xw.WriteEndElement();
            }

            xw.WriteEndElement();
            // End Service Options
            //-------------------------------------------

            xw.WriteEndElement();
            // End Package
            //--------------------------------------------

            return result;
        }

        public static bool WriteShipperXml(ref XmlTextWriter xw, ref Entity e)
        {
            return WriteEntity(ref xw,ref e, EntityType.Shipper);
        }

        public static bool WriteShipToXml(ref XmlTextWriter xw, ref Entity e)
        {
            return WriteEntity(ref xw,ref e, EntityType.ShipTo);
        }

        public static bool WriteShipFromXml(ref XmlTextWriter xw, ref Entity e)
        {
            return WriteEntity(ref xw,ref e, EntityType.ShipFrom);
        }

        private static bool WriteEntity(ref XmlTextWriter xw, ref Entity e, EntityType et)
        {
            bool result = true;


            //--------------------------------------------
            // Start Opening Tag
            switch (et)
            {
                case EntityType.ShipFrom:
                    xw.WriteStartElement("ShipFrom");
                    xw.WriteElementString("CompanyName", TrimToLength(e.CompanyOrContactName, 35));
                    break;
                case EntityType.Shipper:
                    xw.WriteStartElement("Shipper");
                    xw.WriteElementString("Name", TrimToLength(e.CompanyOrContactName, 35));
                    xw.WriteElementString("ShipperNumber", e.AccountNumber);
                    if (e.TaxIDNumber != string.Empty)
                    {
                        xw.WriteElementString("TaxIdentificationNumber", e.TaxIDNumber);
                    }
                    break;
                case EntityType.ShipTo:
                    xw.WriteStartElement("ShipTo");
                    xw.WriteElementString("CompanyName", TrimToLength(e.CompanyOrContactName, 35));
                    if (e.TaxIDNumber != string.Empty)
                    {
                        xw.WriteElementString("TaxIdentificationNumber", e.TaxIDNumber);
                    }
                    break;

            }

            if (e.AttentionName != string.Empty)
            {
                xw.WriteElementString("AttentionName", TrimToLength(e.AttentionName, 35));
            }

            if (e.PhoneNumber != string.Empty)
            {
                xw.WriteElementString("PhoneNumber", e.PhoneNumber);
            }

            if (e.FaxNumber != string.Empty)
            {
                xw.WriteElementString("FaxNumber", e.FaxNumber);
            }

            //-------------------------------------------
            // Start Address
            xw.WriteStartElement("Address");
            xw.WriteElementString("AddressLine1", TrimToLength(e.AddressLine1, 35));
            if (e.AddressLine2 != string.Empty)
            {
                xw.WriteElementString("AddressLine2", TrimToLength(e.AddressLine2, 35));
            }
            if (e.AddressLine3 != string.Empty)
            {
                xw.WriteElementString("AddressLine3", TrimToLength(e.AddressLine3, 35));
            }
            xw.WriteElementString("City", e.City);
            if (e.StateProvinceCode != string.Empty)
            {
                xw.WriteElementString("StateProvinceCode", TrimToLength(e.StateProvinceCode, 5));
            }
            if (e.PostalCode != string.Empty)
            {
                xw.WriteElementString("PostalCode", TrimToLength(e.PostalCode, 10));
            }
            xw.WriteElementString("CountryCode", TrimToLength(e.CountryCode, 2));

            if (et == EntityType.ShipTo)
            {
                if (e.ResidentialAddress == true)
                {
                    xw.WriteElementString("ResidentialAddress", "");
                }
            }

            xw.WriteEndElement();
            // End Address
            //-------------------------------------------

            xw.WriteEndElement();
            // End Opening Tag
            //--------------------------------------------

            return result;
        }
        #endregion
        #region " XPath Parsing Methods "

        public static string XPathToString(ref XmlDocument xdoc, string xpath)
        {
            string result = string.Empty;

            XmlNode node = xdoc.SelectSingleNode(xpath);
            if (node != null)
            {
                result = node.InnerText;
            }

            return result;
        }

        public static decimal XPathToInteger(ref XmlDocument xdoc, string xpath)
        {
            int result = 0;

            XmlNode node = xdoc.SelectSingleNode(xpath);
            if (node != null)
            {
                result = int.Parse(node.InnerText);
            }

            return result;
        }

        public static decimal XPathToDecimal(ref XmlDocument xdoc, string xpath)
        {
            decimal result = 0;

            XmlNode node = xdoc.SelectSingleNode(xpath);
            if (node != null)
            {
                result = decimal.Parse(node.InnerText);
            }

            return result;
        }

        public static CurrencyCode XPathToCurrencyCode(ref XmlDocument xdoc, string xpath)
        {
            CurrencyCode result = CurrencyCode.UsDollar;

            XmlNode node = xdoc.SelectSingleNode(xpath);
            if (node != null)
            {
                result = ConvertStringToCurrencyCode(node.InnerText);
            }

            return result;
        }

        public static UnitsType XPathToUnits(ref XmlDocument xdoc, string xpath)
        {
            UnitsType result = UnitsType.Imperial;

            XmlNode node = xdoc.SelectSingleNode(xpath);
            if (node != null)
            {
                result = ConvertStringToUnits(node.InnerText);
            }

            return result;
        }

        public static ShipLabelFormat XPathToLabelFormat(ref XmlDocument xdoc, string xpath)
        {
            ShipLabelFormat result = ShipLabelFormat.Gif;

            XmlNode node = xdoc.SelectSingleNode(xpath);
            if (node != null)
            {
                result = ConvertStringToLabelFormat(node.InnerText);
            }

            return result;
        }
        #endregion
        #region " Shipment Confirm "

        public static ShipResponse SendConfirmRequest(ref ShipRequest req)
        {
            ShipResponse result = new ShipResponse();

            // Build Request and save output
            string requestXml = BuildShipConfirmRequest(ref req);
            req.XmlConfirmRequest = requestXml;

            // Build Url for UPS Service
            string actionUrl = req.Settings.ServerUrl;
            actionUrl += "ShipConfirm";

            // Send Request and Store Response
            string responseXml = string.Empty;
            responseXml = XmlTools.ReadHtmlPage_POST(actionUrl, requestXml);
            req.XmlConfirmResponse = responseXml;

            // Parse Response
            result = ParseShipConfirmResponse(responseXml);

            return result;
        }

        private static string BuildShipConfirmRequest(ref ShipRequest req)
        {

            StringWriter strWriter = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(strWriter);

            xw.Formatting = Formatting.Indented;
            xw.Indentation = 3;

            xw.WriteStartDocument();

            //--------------------------------------------            
            // Start Shipment Confirm Request
            xw.WriteStartElement("ShipmentConfirmRequest");

            //--------------------------------------------
            // Request
            xw.WriteStartElement("Request");
            //--------------------------------------------
            // TransactionReference
            xw.WriteStartElement("TransactionReference");
            xw.WriteElementString("CustomerContext", "Shipment Confirm Request");
            xw.WriteElementString("XpciVersion", "1.0001");
            xw.WriteEndElement();
            // End TransactionReference
            //--------------------------------------------
            xw.WriteElementString("RequestAction", "ShipConfirm");

            if (req.AddressVerification == true)
            {
                xw.WriteElementString("RequestOption", "validate");
            }
            else
            {
                xw.WriteElementString("RequestOption", "nonvalidate");
            }

            xw.WriteEndElement();
            // End Request
            //--------------------------------------------

            //--------------------------------------------
            // Start Label Specification
            xw.WriteStartElement("LabelSpecification");

            // Label Print Method
            xw.WriteStartElement("LabelPrintMethod");
            switch (req.LabelFormat)
            {
                case ShipLabelFormat.Epl2:
                    xw.WriteElementString("Code", "EPL");
                    break;
                case ShipLabelFormat.Gif:
                    xw.WriteElementString("Code", "GIF");
                    break;
                default:
                    xw.WriteElementString("Code", "GIF");
                    break;
            }
            xw.WriteEndElement();

            // Image Label Format
            xw.WriteStartElement("LabelImageFormat");
            xw.WriteElementString("Code", "GIF");
            xw.WriteEndElement();

            // Http User Agent
            if (req.BrowserHttpUserAgentString != string.Empty)
            {
                xw.WriteElementString("HTTPUserAgent", req.BrowserHttpUserAgentString);
            }

            // Stock Size
            if ((req.LabelStockSizeHeight != 0) & (req.LabelStockSizeWidth != 0))
            {
                xw.WriteStartElement("LabelStockSize");
                if (req.LabelFormat == ShipLabelFormat.Epl2)
                {
                    // Only 4 is valid for Epl
                    xw.WriteElementString("Height", "4");
                    // Only 6 and 8 are valid
                    if ((req.LabelStockSizeWidth != 6) & (req.LabelStockSizeWidth != 8))
                    {
                        xw.WriteElementString("Width", "6");
                        // Pick a default
                    }
                    else
                    {
                        xw.WriteElementString("Width", req.LabelStockSizeWidth.ToString());
                    }
                }
                else
                {
                    xw.WriteElementString("Height", req.LabelStockSizeHeight.ToString());
                    xw.WriteElementString("Width", req.LabelStockSizeWidth.ToString());
                }
                xw.WriteEndElement();
            }

            xw.WriteEndElement();
            // End Label Specification
            //--------------------------------------------

            //--------------------------------------------
            // Shipment
            xw.WriteStartElement("Shipment");

            if (req.ShipmentDescription != string.Empty)
            {
                xw.WriteElementString("Description", XmlTools.TrimToLength(req.ShipmentDescription, 35));
            }

            if (req.NonValuedDocumentsOnly == true)
            {
                xw.WriteElementString("DocumentsOnly", "");
            }

            // Shipper
            Entity tempShipper = req.Shipper;
            XmlTools.WriteShipperXml(ref xw,ref tempShipper);
            // ShipTo
            Entity tempShipTo = req.ShipTo;
            XmlTools.WriteShipToXml(ref xw,ref tempShipTo);
            // ShipFrom
            Entity tempShipFrom = req.ShipFrom;
            XmlTools.WriteShipFromXml(ref xw,ref tempShipFrom);


            // Start Payment Information
            xw.WriteStartElement("PaymentInformation");
            switch (req.BillTo)
            {
                case PaymentType.ReceiverUpsAccount:
                    xw.WriteStartElement("FreightCollect");
                    xw.WriteStartElement("BillReceiver");
                    xw.WriteElementString("AccountNumber", req.BillToAccountNumber);
                    if (req.BillToPostalCode != string.Empty)
                    {
                        xw.WriteStartElement("Address");
                        xw.WriteElementString("PostalCode", req.BillToPostalCode);
                        xw.WriteEndElement();
                    }

                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    break;
                case PaymentType.ShipperCreditCard:
                    xw.WriteStartElement("Prepaid");
                    xw.WriteStartElement("BillShipper");
                    xw.WriteStartElement("CreditCard");
                    xw.WriteElementString("Type", "0" + Convert.ToInt32(req.BillToCreditCardType).ToString());
                    xw.WriteElementString("Number", req.BillToCreditCardNumber);
                    string expDate = "";
                    if (req.BillToCreditCardExpirationMonth < 10)
                    {
                        expDate = "0" + req.BillToCreditCardExpirationMonth.ToString();
                    }
                    else
                    {
                        expDate = req.BillToCreditCardExpirationMonth.ToString();
                    }

                    expDate = expDate + req.BillToCreditCardExpirationYear.ToString();
                    xw.WriteElementString("ExpirationDate", expDate);
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    break;
                case PaymentType.ShipperUpsAccount:
                    xw.WriteStartElement("Prepaid");
                    xw.WriteStartElement("BillShipper");
                    xw.WriteElementString("AccountNumber", req.BillToAccountNumber);
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    break;
                case PaymentType.ThirdPartyUpsAccount:
                    xw.WriteStartElement("BillThirdParty");
                    xw.WriteStartElement("BillThirdPartyShipper");
                    xw.WriteElementString("AccountNumber", req.BillToAccountNumber);
                    xw.WriteStartElement("ThirdParty");
                    xw.WriteStartElement("Address");
                    if (req.BillToPostalCode != string.Empty)
                    {
                        xw.WriteElementString("PostalCode", req.BillToPostalCode);
                    }

                    xw.WriteElementString("CountryCode", req.BillToCountryCode);
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    break;
            }
            xw.WriteEndElement();
            // End Payment Information

            // Reference Number
            if (req.ReferenceNumber != string.Empty)
            {
                xw.WriteStartElement("ReferenceNumber");
                xw.WriteElementString("Value", XmlTools.TrimToLength(req.ReferenceNumber, 35));
                string codeType = ConvertReferenceNumberCodeToString(req.ReferenceNumberType);
                xw.WriteElementString("Code", codeType);
                xw.WriteEndElement();
            }

            // Start Service
            xw.WriteStartElement("Service");
            string stringServiceCode = Convert.ToInt32(req.Service).ToString();
            if (stringServiceCode.Length < 2)
            {
                stringServiceCode = "0" + stringServiceCode;
            }
            xw.WriteElementString("Code", stringServiceCode);
            xw.WriteEndElement();
            // End Service

            // Invoice Line Total
            if (req.InvoiceLineTotal == true)
            {
                xw.WriteStartElement("InvoiceLineTotal");
                xw.WriteElementString("MonetaryValue", req.InvoiceLineTotalAmount.ToString());
                xw.WriteElementString("CurrencyCode", XmlTools.ConvertCurrencyCodeToString(req.InvoiceLineTotalCurrency));
                xw.WriteEndElement();
            }



            for (int i = 0; i <= req.Packages.Count - 1; i++)
            {
                Package tempPackage = (Package)req.Packages[i];
                XmlTools.WriteSingleUpsPackage(ref xw,ref tempPackage);
            }

            //-------------------------------------------
            // Start Shipment Service Options
            xw.WriteStartElement("ShipmentServiceOptions");

            // SaturdayDelivery
            if (req.SaturdayDelivery == true)
            {
                xw.WriteElementString("SaturdayDelivery", "");
            }

            // COD
            if (req.COD == true)
            {
                xw.WriteStartElement("COD");
                xw.WriteElementString("CODCode", "3");
                xw.WriteElementString("CODFundsCode", Convert.ToInt32(req.CODPaymentType).ToString());
                xw.WriteStartElement("CODAmount");
                xw.WriteElementString("CurrencyCode", XmlTools.ConvertCurrencyCodeToString(req.CODCurrencyCode));
                xw.WriteElementString("MonetaryValue", req.CODAmount.ToString());
                xw.WriteEndElement();
                xw.WriteEndElement();
            }

            // Notification
            if (req.Notification == true)
            {
                xw.WriteStartElement("Notification");
                xw.WriteElementString("NotificationCode", Convert.ToInt32(req.NotificationType).ToString());
                xw.WriteStartElement("EMailMessage");
                xw.WriteElementString("EMailAddress", req.NotificationEmailAddress);
                if (req.NotificationUndeliverableEmailAddress != string.Empty)
                {
                    xw.WriteElementString("UndeliverableEMailAddress", req.NotificationUndeliverableEmailAddress);
                }
                if (req.NotificationFromName != string.Empty)
                {
                    xw.WriteElementString("FromName", req.NotificationFromName);
                }
                if (req.NotificationMemo != string.Empty)
                {
                    xw.WriteElementString("Memo", req.NotificationMemo);
                }
                if (req.NotificationSubjectType != NotificationSubjectCode.DefaultCode)
                {
                    xw.WriteElementString("SubjectCode", "0" + Convert.ToInt32(req.NotificationSubjectType).ToString());
                }
                xw.WriteEndElement();
                xw.WriteEndElement();
            }

            xw.WriteEndElement();
            // End Shipment Service Options
            //-------------------------------------------

            xw.WriteEndElement();
            // End Shipment
            //--------------------------------------------

            xw.WriteEndElement();
            // End Shipment Confirm Request
            //--------------------------------------------

            xw.WriteEndDocument();
            xw.Flush();
            xw.Close();


            // Output Xml As String with Access Key
            string sXML = "";
            sXML = XmlTools.BuildAccessKey(req.Settings);
            sXML += "\n";
            sXML += strWriter.GetStringBuilder().ToString();

            return sXML;
        }

        private static ShipResponse ParseShipConfirmResponse(string xml)
        {
            ShipResponse result = new ShipResponse();

            try
            {
                XmlDocument xDoc;
                xDoc = new XmlDocument();
                xDoc.LoadXml(xml);

                XmlNode nodeResponseCode = xDoc.SelectSingleNode("ShipmentConfirmResponse/Response/ResponseStatusCode");
                if (nodeResponseCode != null)
                {
                    int responseCode = 0;
                    responseCode = int.Parse(nodeResponseCode.InnerText);
                    if (responseCode == 1)
                    {
                        result.Success = true;

                        // Parse appropriate info
                        result.ShipmentDigest = XmlTools.XPathToString(ref xDoc, "ShipmentConfirmResponse/ShipmentDigest");
                        result.TrackingNumber = XmlTools.XPathToString(ref xDoc, "ShipmentConfirmResponse/ShipmentIdentificationNumber");
                        result.BillingWeight = XmlTools.XPathToDecimal(ref xDoc, "ShipmentConfirmResponse/BillingWeight/Weight");
                        result.BillingWeightUnits = XmlTools.XPathToUnits(ref xDoc, "ShipmentConfirmResponse/BillingWeight/UnitOfMeasurement/Code");
                        result.ServiceOptionsCharge = XmlTools.XPathToDecimal(ref xDoc, "ShipmentConfirmResponse/ShipmentCharges/ServiceOptionsCharges/MonetaryValue");
                        result.ServiceOptionsChargeCurrency = XmlTools.XPathToCurrencyCode(ref xDoc, "ShipmentConfirmResponse/ShipmentCharges/ServiceOptionsCharges/CurrencyCode");
                        result.TotalCharge = XmlTools.XPathToDecimal(ref xDoc, "ShipmentConfirmResponse/ShipmentCharges/TotalCharges/MonetaryValue");
                        result.TotalChargeCurrency = XmlTools.XPathToCurrencyCode(ref xDoc, "ShipmentConfirmResponse/ShipmentCharges/TotalCharges/CurrencyCode");
                        result.TransportationCharge = XmlTools.XPathToDecimal(ref xDoc, "ShipmentConfirmResponse/ShipmentCharges/TransportationCharges/MonetaryValue");
                        result.TransportationChargeCurrency = XmlTools.XPathToCurrencyCode(ref xDoc, "ShipmentConfirmResponse/ShipmentCharges/TransportationCharges/CurrencyCode");
                    }
                    else
                    {
                        result.Success = false;

                        // Parse error
                        result.ErrorMessage = XmlTools.XPathToString(ref xDoc, "ShipmentConfirmResponse/Response/Error/ErrorDescription");

                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Unable to Parse Response Code.";
                }
            }

            catch (Exception Exx)
            {
                result.ErrorMessage = "Parsing Error: " + Exx.Message;
                result.Success = false;
            }

            return result;
        }
        #endregion
        #region " Shipment Accept "

        public static ShipAcceptResponse SendShipAcceptRequest(ref ShipAcceptRequest req)
        {
            ShipAcceptResponse result = new ShipAcceptResponse();

            // Build Request and save output
            string requestXml = BuildShipAcceptRequest(ref req);
            req.XmlAcceptRequest = requestXml;

            // Build Url for UPS Service
            string actionUrl = req.Settings.ServerUrl;
            actionUrl += "ShipAccept";

            // Send Request and Store Response
            string responseXml = string.Empty;
            responseXml = XmlTools.ReadHtmlPage_POST(actionUrl, requestXml);
            req.XmlAcceptResponse = responseXml;

            // Parse Response
            result = ParseShipAcceptResponse(responseXml);

            return result;
        }

        private static string BuildShipAcceptRequest(ref ShipAcceptRequest req)
        {

            StringWriter strWriter = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(strWriter);

            xw.Formatting = Formatting.Indented;
            xw.Indentation = 3;

            xw.WriteStartDocument();

            //--------------------------------------------            
            // Start Shipment Accept Request
            xw.WriteStartElement("ShipmentAcceptRequest");

            //--------------------------------------------
            // Request
            xw.WriteStartElement("Request");
            //--------------------------------------------
            // TransactionReference
            xw.WriteStartElement("TransactionReference");
            xw.WriteElementString("CustomerContext", "Shipment Confirm Request");
            xw.WriteElementString("XpciVersion", "1.0001");
            xw.WriteEndElement();
            // End TransactionReference
            //--------------------------------------------
            xw.WriteElementString("RequestAction", "ShipAccept");
            xw.WriteEndElement();
            // End Request
            //--------------------------------------------

            xw.WriteElementString("ShipmentDigest", req.ShipDigest);

            xw.WriteEndElement();
            // End Shipment Accept Request
            //--------------------------------------------

            xw.WriteEndDocument();
            xw.Flush();
            xw.Close();


            // Output Xml As String with Access Key
            string sXML = "";
            sXML = XmlTools.BuildAccessKey(req.Settings);
            sXML += "\n";
            sXML += strWriter.GetStringBuilder().ToString();

            return sXML;
        }

        private static ShipAcceptResponse ParseShipAcceptResponse(string xml)
        {
            ShipAcceptResponse result = new ShipAcceptResponse();

            try
            {
                XmlDocument xDoc;
                xDoc = new XmlDocument();
                xDoc.LoadXml(xml);

                XmlNode nodeResponseCode = xDoc.SelectSingleNode("ShipmentAcceptResponse/Response/ResponseStatusCode");
                if (nodeResponseCode != null)
                {
                    int responseCode = 0;
                    responseCode = int.Parse(nodeResponseCode.InnerText);
                    if (responseCode == 1)
                    {
                        result.Success = true;

                        // Parse appropriate info
                        result.TrackingNumber = XmlTools.XPathToString(ref xDoc, "ShipmentAcceptResponse/ShipmentResults/ShipmentIdentificationNumber");
                        result.BillingWeight = XmlTools.XPathToDecimal(ref xDoc, "ShipmentAcceptResponse/ShipmentResults/BillingWeight/Weight");
                        result.BillingWeightUnits = XmlTools.XPathToUnits(ref xDoc, "ShipmentAcceptResponse/ShipmentResults/BillingWeight/UnitOfMeasurement/Code");
                        result.ServiceOptionsCharge = XmlTools.XPathToDecimal(ref xDoc, "ShipmentAcceptResponse/ShipmentResults/ShipmentCharges/ServiceOptionsCharges/MonetaryValue");
                        result.ServiceOptionsChargeCurrency = XmlTools.XPathToCurrencyCode(ref xDoc, "ShipmentAcceptResponse/ShipmentResults/ShipmentCharges/ServiceOptionsCharges/CurrencyCode");
                        result.TotalCharge = XmlTools.XPathToDecimal(ref xDoc, "ShipmentAcceptResponse/ShipmentCharges/ShipmentResults/TotalCharges/MonetaryValue");
                        result.TotalChargeCurrency = XmlTools.XPathToCurrencyCode(ref xDoc, "ShipmentAcceptResponse/ShipmentResults/ShipmentCharges/TotalCharges/CurrencyCode");
                        result.TransportationCharge = XmlTools.XPathToDecimal(ref xDoc, "ShipmentAcceptResponse/ShipmentResults/ShipmentCharges/TransportationCharges/MonetaryValue");
                        result.TransportationChargeCurrency = XmlTools.XPathToCurrencyCode(ref xDoc, "ShipmentAcceptResponse/ShipmentResults/ShipmentCharges/TransportationCharges/CurrencyCode");

                        XmlNodeList packages = xDoc.SelectNodes("/ShipmentAcceptResponse/ShipmentResults/PackageResults");
                        if (packages != null)
                        {
                            ParseShipAcceptResponsePackages(ref result,ref packages);
                        }
                    }

                    else
                    {
                        result.Success = false;

                        // Parse error
                        result.ErrorMessage = XmlTools.XPathToString(ref xDoc, "ShipmentAcceptResponse/Response/Error/ErrorDescription");

                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Unable to Parse Response Code.";
                }
            }

            catch (Exception Exx)
            {
                result.ErrorMessage = "Parsing Error: " + Exx.Message;
                result.Success = false;
            }

            return result;
        }

        private static void ParseShipAcceptResponsePackages(ref ShipAcceptResponse res, ref XmlNodeList packages)
        {

            foreach (XmlNode node in packages)
            {
                try
                {
                    XmlDocument xp = new XmlDocument();
                    xp.LoadXml(node.OuterXml);

                    ShippedPackageInformation _package = new ShippedPackageInformation();
                    _package.TrackingNumber = XmlTools.XPathToString(ref xp, "PackageResults/TrackingNumber");
                    _package.Base64Image = XmlTools.XPathToString(ref xp, "PackageResults/LabelImage/GraphicImage");
                    _package.Base64Html = XmlTools.XPathToString(ref xp, "PackageResults/LabelImage/HTMLImage");
                    _package.Base64Signature = XmlTools.XPathToString(ref xp, "PackageResults/InternationalSignatureGraphicImage");
                    _package.LabelFormat = XmlTools.XPathToLabelFormat(ref xp, "PackageResults/LabelImage/LabelImageFormat/Code");
                    _package.ServiceOptionsCharge = XmlTools.XPathToDecimal(ref xp, "PackageResults/ServiceOptionsCharge/MonetaryValue");
                    _package.ServiceOptionsChargeCurrency = XmlTools.XPathToCurrencyCode(ref xp, "PackageResults/ServiceOptionsCharge/CurrencyCode");
                    res.AddPackage(ref _package);
                }

                catch
                {

                }
            }
        }
        #endregion
        #region "Void Shipment"

        public static VoidShipmentResponse SendVoidShipmentRequest(ref VoidShipmentRequest req)
        {
            VoidShipmentResponse result = new VoidShipmentResponse();

            // Build Request and save output
            string requestXml = BuildVoidShipRequest(ref req);
            req.XmlRequest = requestXml;

            // Build Url for UPS Service
            string actionUrl = req.Settings.ServerUrl;
            actionUrl += "Void";

            // Send Request and Store Response
            string responseXml = string.Empty;
            responseXml = XmlTools.ReadHtmlPage_POST(actionUrl, requestXml);
            req.XmlResponse = responseXml;

            // Parse Response
            result = ParseVoidShipmentResponse(responseXml);

            return result;
        }

        private static string BuildVoidShipRequest(ref VoidShipmentRequest req)
        {

            StringWriter strWriter = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(strWriter);

            xw.Formatting = Formatting.Indented;
            xw.Indentation = 3;

            xw.WriteStartDocument();

            //--------------------------------------------            
            // Start Void Shipment Request
            xw.WriteStartElement("VoidShipmentRequest");

            //--------------------------------------------
            // Request
            xw.WriteStartElement("Request");
            //--------------------------------------------
            // TransactionReference
            xw.WriteStartElement("TransactionReference");
            xw.WriteElementString("CustomerContext", "Void Shipment Request");
            xw.WriteElementString("XpciVersion", "1.0001");
            xw.WriteEndElement();
            // End TransactionReference
            //--------------------------------------------
            xw.WriteElementString("RequestAction", "1");
            xw.WriteElementString("RequestOption", "");
            xw.WriteEndElement();
            // End Request
            //--------------------------------------------

            xw.WriteElementString("ShipmentIdentificationNumber", req.ShipmentIdentificationNumber);

            xw.WriteEndElement();
            // End Void Shipment Request
            //--------------------------------------------

            xw.WriteEndDocument();
            xw.Flush();
            xw.Close();


            // Output Xml As String with Access Key
            string sXML = "";
            sXML = XmlTools.BuildAccessKey(req.Settings);
            sXML += "\n";
            sXML += strWriter.GetStringBuilder().ToString();

            return sXML;
        }

        private static VoidShipmentResponse ParseVoidShipmentResponse(string xml)
        {
            VoidShipmentResponse result = new VoidShipmentResponse();

            try
            {
                XmlDocument xDoc;
                xDoc = new XmlDocument();
                xDoc.LoadXml(xml);

                XmlNode nodeResponseCode = xDoc.SelectSingleNode("VoidShipmentResponse/Response/ResponseStatusCode");
                if (nodeResponseCode != null)
                {
                    int responseCode = 0;
                    responseCode = int.Parse(nodeResponseCode.InnerText);
                    if (responseCode == 1)
                    {
                        result.Success = true;
                    }
                    else
                    {
                        result.Success = false;

                        // Parse error
                        result.ErrorMessage = XmlTools.XPathToString(ref xDoc, "VoidShipmentResponse/Response/Error/ErrorDescription");

                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Unable to Parse Response Code.";
                }
            }

            catch (Exception Exx)
            {
                result.ErrorMessage = "Parsing Error: " + Exx.Message;
                result.Success = false;
            }

            return result;
        }
        #endregion

    }
 
}