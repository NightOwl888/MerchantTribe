using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MerchantTribe.Payment;

namespace MerchantTribe.Commerce.Accounts
{
    public class StoreSettings
    {
        private Store _Store = null;

        public List<StoreSetting> AllSettings {get;set;}

        public StoreSettings(Store s)
        {
            this._Store = s;
            AllSettings = new List<StoreSetting>();
            Acumatica = new StoreSettingsAcumatica(this);
            Analytics = new StoreSettingsAnalytics(this);            
            MailServer = new StoreSettingsMailServer(this);
            PayPal = new StoreSettingsPayPal(this);
            FaceBook = new StoreSettingsFaceBook(this);
            Twitter = new StoreSettingsTwitter(this);
            GooglePlus = new StoreSettingsGooglePlus(this);
        }

        #region Setter Helpers

        internal void SetProp(string name, string value)
        {
            StoreSetting result = new StoreSetting();
            result.StoreId = this._Store.Id;
            result.SettingName = name;
            result.SettingValue = value;
            AddOrUpdateLocalSetting(result);
        }
        internal void SetPropEncrypted(string name, string value)
        {
            string encoded = string.Empty;
            MerchantTribe.Web.Cryptography.TripleDesEncryption crypto = new MerchantTribe.Web.Cryptography.TripleDesEncryption();
            encoded = crypto.Encode(value);
            SetProp(name, encoded);
        }
        internal void SetProp(string name, bool value)
        {
            StoreSetting result = new StoreSetting();
            result.StoreId = this._Store.Id;
            result.SettingName = name;
            result.ValueAsBool = value;
            AddOrUpdateLocalSetting(result);
        }
        internal void SetProp(string name, int value)
        {
            StoreSetting result = new StoreSetting();
            result.StoreId = this._Store.Id;
            result.SettingName = name;
            result.ValueAsInteger = value;
            AddOrUpdateLocalSetting(result);
        }
        internal void SetProp(string name, long value)
        {
            StoreSetting result = new StoreSetting();
            result.StoreId = this._Store.Id;
            result.SettingName = name;
            result.ValueAsLong = value;
            AddOrUpdateLocalSetting(result);
        }
        internal void SetProp(string name, decimal value)
        {
            StoreSetting result = new StoreSetting();
            result.StoreId = this._Store.Id;
            result.SettingName = name;
            result.ValueAsDecimal = value;
            AddOrUpdateLocalSetting(result);            
        }

        // Keeps local settings synchronized with updates during a single request
        // Does not update database
        public void AddOrUpdateLocalSetting(StoreSetting s)
        {
            // Search local settings storage for setting
            var found = this.AllSettings.Where(y => y.SettingName == s.SettingName).FirstOrDefault();
            if (found == null)
            {
                this.AllSettings.Add(s);
            }
            else
            {
                s.Id = found.Id; // Set Id so we'll get a database update instead of insert
                this.AllSettings[this.AllSettings.IndexOf(found)] = s;
            }
        }

        internal string GetProp(string name)
        {
            StoreSetting s = GetSetting(name);
            return s.SettingValue;
        }
        internal string GetPropEncrypted(string name)
        {
            StoreSetting s = GetSetting(name);
            string result = string.Empty;
            result = s.SettingValue;
            MerchantTribe.Web.Cryptography.TripleDesEncryption crypto = new MerchantTribe.Web.Cryptography.TripleDesEncryption();
            if (result != string.Empty)
            {
                result = crypto.Decode(result);
            }
            crypto = null;
            return result;
        }
        internal bool GetPropBool(string name)
        {
            StoreSetting s = GetSetting(name);
            return s.ValueAsBool;
        }
        internal bool GetPropBoolWithDefault(string name, bool defaultValue)
        {
            StoreSetting s = GetSetting(name);
            if (s.SettingValue == string.Empty)
            {
                SetProp(name, defaultValue);
                return defaultValue;
            }
            return s.ValueAsBool;
        }
        internal int GetPropInt(string name)
        {
            StoreSetting s = GetSetting(name);
            return s.ValueAsInteger;
        }
        internal decimal GetPropDecimal(string name)
        {
            decimal result = GetPropDecimalWithDefault(name);
            if (result == -1) result = 0;
            return result;
        }
        internal decimal GetPropDecimalWithDefault(string name)
        {
            StoreSetting s = GetSetting(name);
            return s.ValueAsDecimal;
        }
        internal long GetPropLong(string name)
        {
            StoreSetting s = GetSetting(name);
            return s.ValueAsLong;
        }

        internal StoreSetting GetSetting(string name)
        {
            // Search local settings storage for setting
            var s = this.AllSettings.Where(y => y.SettingName == name).FirstOrDefault();
            if (s == null) s = new StoreSetting();
            return (StoreSetting)s;
        }

        #endregion

        public StoreSettingsAcumatica Acumatica { get; private set; }
        public StoreSettingsAnalytics Analytics { get; private set; }        
        public StoreSettingsMailServer MailServer { get; private set; }
        public StoreSettingsPayPal PayPal { get; private set; }
        public StoreSettingsFaceBook FaceBook { get; private set; }
        public StoreSettingsTwitter Twitter { get; private set; }
        public StoreSettingsGooglePlus GooglePlus { get; private set; }

        public string UniqueId
        {
            get { string result = GetProp("UniqueId");
                return result;
                }
            set { SetProp("UniqueId", value); }
        }

        public bool IsPrivateStore
        {
            get { return GetPropBool("IsPrivateStore"); }
            set { SetProp("IsPrivateStore", value); }
        }
        public DateTime AllowApiToClearUntil
        {
            get
            {
                DateTime result = DateTime.Now.AddYears(-1);
                string prop = GetProp("AllowApiToClearUntil");
                DateTime temp = DateTime.UtcNow.AddYears(-1);
                if (DateTime.TryParse(prop, out temp))
                {
                    return temp.ToUniversalTime();
                }
                return result;
            }
            set
            {                
                string temp = value.ToString("u");
                SetProp("AllowApiToClearUntil", temp);
            }
        }
        public bool RememberUserPasswords
        {
            get { return GetPropBool("RememberUserPasswords"); }
            set { SetProp("RememberUserPasswords", value); }
        }
        public bool IsPayPalLead
        {
            get
            {
                if (LeadSource == "PayPalOffer") return true;

                return false;
            }
        }
        public string LeadSource
        {
            get { return GetProp("LeadSource"); }
            set { SetProp("LeadSource", value); }
        }

        // SSL and Domain Settings
        public bool ForceDomains
        {
            get {
                // Force Domain Redirects for Hosted Version
                if (WebAppSettings.IsIndividualMode ||
                    WebAppSettings.IsCommercialVersion)
                {
                    return GetPropBool("ForceDomains");
                }
                return true;
            }
            set { SetProp("ForceDomains", value); }
        }
        public bool UseSSL
        {
            get {
                // Force SSL for hosted version
                if (WebAppSettings.IsIndividualMode ||
                    WebAppSettings.IsCommercialVersion)
                {
                    return GetPropBool("UseSSL");
                }
                return true;
            }
            set { SetProp("UseSSL", value); }
        }

        public bool WizardComplete
        {
            get
            {
                return GetPropBool("WizardComplete");
            }
            set { SetProp("WizardComplete", value); }
        }

        // Time and Culture
        public TimeZoneInfo TimeZone
        {
            get
            {
                string id = GetProp("TimeZone");
                if (id == string.Empty)
                {
                    id = "Eastern Standard Time";
                }
                TimeZoneInfo t = TimeZoneInfo.FindSystemTimeZoneById(id);
                return t;
            }
            set { SetProp("TimeZone", value.Id); }
        }
        public string CultureCode
        {
            get
            {
                string cc = GetProp("CultureCode");
                if (cc == string.Empty)
                {
                    cc = "en-US";
                }
                return cc;
            }
            set
            {
                SetProp("CultureCode", value);
            }
        }

        public long LastOrderNumber
        {
            get { return GetPropLong("LastOrderNumber"); }
            set { SetProp("LastOrderNumber", value); }
        }
        public string LogoImage
        {
            get { return GetProp("LogoImage"); }
            set { SetProp("LogoImage", value); }
        }

        public string LogoImageFullUrl(MerchantTribeApplication app, bool isSecure)
        {
            return Storage.DiskStorage.StoreLogoUrl(app, this.LogoRevision, this.LogoImage, isSecure);
        }

        public bool UseLogoImage
        {
            get { return GetPropBool("UseLogoImage"); }
            set { SetProp("UseLogoImage", value); }
        }
        public string LogoText
        {
            get
            {
                string s = GetProp("LogoText");
                if (s == string.Empty)
                {
                    s = this._Store.StoreName;
                }
                return s;
            }
            set { SetProp("LogoText", value); }
        }
        public decimal MinumumOrderAmount
        {
            get { return GetPropDecimal("MinimumOrderAmount"); }
            set { SetProp("MinimumOrderAmount", value); }
        }
        public string WelcomeText
        {
            get { return GetProp("WelcomeText"); }
            set { SetProp("WelcomeText", value); }
        }
        public string FriendlyName
        {
            get
            {
                string result = GetProp("FriendlyName");
                if (result == string.Empty)
                {
                    result = _Store.StoreName;
                }
                return result;
            }
            set { SetProp("FriendlyName", value); }
        }
        public int LogoRevision
        {
            get
            {
                int result = GetPropInt("LogoRevision");
                if (result < 1)
                {
                    result = 1;
                }
                return result;
            }
            set { SetProp("LogoRevision", value); }
        }
        public int ThemeRevision
        {
            get
            {
                int result = GetPropInt("ThemeRevision");
                if (result < 1)
                {
                    result = 1;
                }
                return result;
            }
            set { SetProp("ThemeRevision", value); }
        }
        public long DefaultTaxSchedule
        {
            get
            {
                long result = 0;
                result = GetPropLong("DefaultTaxScheduleId");
                if (result < 0)
                {
                    result = 0;
                }
                return result;
            }
            set { SetProp("DefaultTaxScheduleId", value); }
        }
        public string ThemeId
        {
            get { return GetProp("ThemeId"); }
        }
        public string MetaDescription
        {
            get { return GetProp("MetaDescription"); }
            set { SetProp("MetaDescription", value); }
        }
        public string MetaKeywords
        {
            get { return GetProp("MetaKeywords"); }
            set { SetProp("MetaKeywords", value); }
        }

        /// <summary>
        /// Gets or sets the global rounding mode for the application for use in the <see cref="Math.Round(System.Decimal, System.Int32, System.MidpointRounding)"/> method.
        /// </summary>
        /// <remarks>
        /// Can be set to one of the values of the <see cref="System.MidpointRounding"/> enumeration to control how the mid point between two whole numbers will be rounded.
        /// </remarks>
        public MidpointRounding MidpointRoundingMode
        {
            get
            {
                MidpointRounding result;
                string value = GetProp("MidpointRoundingMode");
                if (Enum.TryParse<MidpointRounding>(value, out result))
                {
                    return result;
                }
                // Default value
                return MidpointRounding.AwayFromZero;
            }
            set { SetProp("MidpointRoundingMode", value.ToString()); }
        }

        // Rewards Points
        public bool RewardsPointsOnPurchasesActive
        {
            get { return GetPropBool("RewardsPointsOnPurchasesActive"); }
            set { SetProp("RewardsPointsOnPurchasesActive", value); }
        }
        public bool RewardsPointsOnProductsActive
        {
            get { return GetPropBool("RewardsPointsOnProductsActive"); }
            set { SetProp("RewardsPointsOnProductsActive", value); }
        }
        public int RewardsPointsIssuedPerDollarSpent
        {
            get
            {
                int temp = GetPropInt("RewardsPointsIssuedPerDollarSpent");
                if (temp < 1) temp = 1;
                return temp;
            }
            set { SetProp("RewardsPointsIssuedPerDollarSpent", value); }
        }
        public int RewardsPointsNeededPerDollarCredit
        {
            get
            {
                int temp = GetPropInt("RewardsPointsNeededPerDollarCredit");
                if (temp < 1) temp = 100;
                return temp;
            }
            set { SetProp("RewardsPointsNeededPerDollarCredit", value); }
        }
        public string RewardsPointsName
        {
            get
            {
                string temp = GetProp("RewardsPointsName");
                if (temp == string.Empty) temp = "Rewards Points";
                return temp;
            }
            set { SetProp("RewardsPointsName", value); }
        }
       
        //Orders and Checkout
        public bool ForceTermsAgreement
        {
            get { return GetPropBool("ForceTermsAgreement"); }
            set { SetProp("ForceTermsAgreement", value); }
        }
        public int MaxItemsPerOrder
        {
            get { return GetPropInt("MaxItemsPerOrder"); }
            set { SetProp("MaxItemsPerOrder", value); }
        }
        public decimal MaxWeightPerOrder
        {
            get { decimal result = GetPropDecimalWithDefault("MaxWeightPerOrder");
                    if (result < 0) result = 9999m;
                    return result;
                }
            set { SetProp("MaxWeightPerOrder", value); }
        }
        public string MaxOrderMessage
        {
            get { string result = GetProp("MaxOrderMessage");
            if (result.Trim() == string.Empty) result = "That's a really big order! Please call us instead of ordering online.";
            return result;
            }
            set { SetProp("MaxOrderMessage", value); }
        }

        public bool RejectFailedCreditCardOrdersAutomatically
        {
            get { return GetPropBool("RejectFailedCC"); }
            set { SetProp("RejectFailedCC", value); }
        }
        public bool MetricsRecordSearches
        {
            get { return GetPropBool("MetricsRecordSearches"); }
            set { SetProp("MetricsRecordSearches", value); }
        }
        
        // Handling
        public decimal HandlingAmount
        {
            get { return GetPropDecimal("HandlingAmount"); }
            set { SetProp("HandlingAmount", value); }
        }
        public bool HandlingNonShipping
        {
            get { return GetPropBool("HandlingNonShipping"); }
            set { SetProp("HandlingNonShipping", value); }
        }
        public int HandlingType
        {
            get { return GetPropInt("HandlingType"); }
            set { SetProp("HandlingType", value); }
        }

        // Payment
        public bool PaymentCreditCardAuthorizeOnly
        {
            get { return GetPropBool("PaymentCreditCardAuthorizeOnly"); }
            set { SetProp("PaymentCreditCardAuthorizeOnly", value); }
        }
        public bool PaymentCreditCardRequireCVV
        {
            get { return GetPropBool("PaymentCreditCardRequireCVV"); }
            set { SetProp("PaymentCreditCardRequireCVV", value); }
        }
        public Dictionary<string, string> PaymentMethodsEnabled
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                string data = GetProp("PaymentMethodsEnabled");
                string[] methodIds = data.Split(',');
                for (int i = 0; i < methodIds.Length; i++)
                {
                    result.Add(methodIds[i], string.Empty);
                }
                return result;
            }
            set
            {
                string data = string.Empty;
                foreach (KeyValuePair<string, string> k in value)
                {
                    data += k.Key + ",";
                }
                data = data.TrimEnd(',');
                SetProp("PaymentMethodsEnabled", data);
            }
        }
        public string PaymentCreditCardGateway
        {
            get { return GetProp("PaymentCreditCardGateway"); }
            set { SetProp("PaymentCreditCardGateway", value); }
        }
        public List<MerchantTribe.Payment.CardType> PaymentAcceptedCards
        {
            get
            {
                List<CardType> result = new List<CardType>();

                string data = GetProp("PaymentAcceptedCards");

                if (data == string.Empty) return AllCards();

                string[] cardIds = data.Split(',');
                for (int i = 0; i < cardIds.Length; i++)
                {
                    int temp = 0;
                    if (int.TryParse(cardIds[i], out temp))
                    {
                        try
                        {
                            result.Add((CardType)temp);
                        }
                        catch
                        {
                            return AllCards();
                        }
                    }
                }
                return result;
            }
            set
            {
                string data = string.Empty;
                foreach (CardType c in value)
                {
                    data += (int)c + ",";
                }
                data = data.TrimEnd(',');
                SetProp("PaymentAcceptedCards", data);
            }
        }
        private List<CardType> AllCards()
        {
            List<CardType> result = new List<CardType>();
            result.Add(CardType.Amex);
            //result.Add(CardType.DinersClub);
            result.Add(CardType.Discover);
            //result.Add(CardType.JCB);
            //result.Add(CardType.Maestro);
            result.Add(CardType.MasterCard);
            //result.Add(CardType.Solo);
            //result.Add(CardType.Switch);
            result.Add(CardType.Visa);
            return result;
        }

        

        // Reviews
        public int ProductReviewCount
        {
            get
            {
                int temp = GetPropInt("ProductReviewCount");
                if (temp < 1)
                {
                    temp = 3;
                }
                return temp;
            }
            set { SetProp("ProductReviewCount", value); }
        }
        public bool ProductReviewModerate
        {
            get { return GetPropBool("ProductReviewModerate"); }
            set { SetProp("ProductReviewModerate", value); }
        }
        public bool ProductReviewShowRating
        {
            get { return GetPropBool("ProductReviewShowRating"); }
            set { SetProp("ProductReviewShowRating", value); }
        }

        // Store Protection
        public bool StoreClosed
        {
            get { return GetPropBool("StoreClosed"); }
            set { SetProp("StoreClosed", value); }
        }
        public string StoreClosedDescription
        {
            get { return GetProp("StoreClosedDescription"); }
            set { SetProp("StoreClosedDescription", value); }
        }
        public string StoreClosedDescriptionPreTransform
        {
            get { return GetProp("StoreClosedDescriptionPreTransform"); }
            set { SetProp("StoreClosedDescriptionPreTransform", value); }
        }
        public string StoreClosedGuestPassword
        {
            get { return GetProp("StoreClosedGuestPassword"); }
            set { SetProp("StoreClosedGuestPassword", value); }
        }
        
        // Shipping
        public string ShippingFedExAccountNumber
        {
            get { return GetProp("ShippingFedExAccountNumber"); }
            set { SetProp("ShippingFedExAccountNumber", value); }
        }
        public bool ShippingFedExDiagnostics
        {
            get { return GetPropBool("ShippingFedExDiagnostics"); }
            set { SetProp("ShippingFedExDiagnostics", value); }
        }
        public int ShippingFedExDefaultPackaging
        {
            get { return GetPropInt("ShippingFedExDefaultPackaging"); }
            set { SetProp("ShippingFedExDefaultPackaging", value); }
        }
        public int ShippingFedExDropOffType
        {
            get { return GetPropInt("ShippingFedExDropOffType"); }
            set { SetProp("ShippingFedExDropOffType", value); }
        }
        public bool ShippingFedExForceResidentialRates
        {
            get { return GetPropBool("ShippingFedExForceResidentialRates"); }
            set { SetProp("ShippingFedExForceResidentialRates", value); }
        }
        public string ShippingFedExMeterNumber
        {
            get { return GetProp("ShippingFedExMeterNumber"); }
            set { SetProp("ShippingFedExMeterNumber", value); }
        }
        public bool ShippingFedExUseListRates
        {
            get { return GetPropBool("ShippingFedExUseListRates"); }
            set { SetProp("ShippingFedExUseListRates", value); }
        }
        public string ShippingUpsAccountNumber
        {
            get { return GetProp("ShippingUpsAccountNumber"); }
            set { SetProp("ShippingUpsAccountNumber", value); }
        }
        public int ShippingUpsDefaultPackaging
        {
            get { return GetPropInt("ShippingUpsDefaultPackaging"); }
            set { SetProp("ShippingUpsDefaultPackaging", value); }
        }
        public int ShippingUpsDefaultPayment
        {
            get { return GetPropInt("ShippingUpsDefaultPayment"); }
            set { SetProp("ShippingUpsDefaultPayment", value); }
        }
        public int ShippingUpsDefaultService
        {
            get { return GetPropInt("ShippingUpsDefaultService"); }
            set { SetProp("ShippingUpsDefaultService", value); }
        }
        public bool ShippingUPSDiagnostics
        {
            get { return GetPropBool("ShippingUPSDiagnostics"); }
            set { SetProp("ShippingUPSDiagnostics", value); }
        }
        public bool ShippingUpsForceResidential
        {
            get { return GetPropBool("ShippingUpsForceResidential"); }
            set { SetProp("ShippingUpsForceResidential", value); }
        }
        public string ShippingUpsLicense
        {
            get { return GetProp("Shipping_UPS_License"); }
            set { SetProp("Shipping_UPS_License", value); }
        }
        public string ShippingUpsPassword
        {
            get { return GetProp("Shipping_UPS_Password"); }
            set { SetProp("Shipping_UPS_Password", value); }
        }
        public int ShippingUpsPickupType
        {
            get { return GetPropInt("Shipping_UPS_Pickup_Type"); }
            set { SetProp("Shipping_UPS_Pickup_Type", value); }
        }
        public bool ShippingUpsSkipDimensions
        {
            get { return GetPropBool("ShippingUpsSkipDimensions"); }
            set { SetProp("ShippingUpsSkipDimensions", value); }
        }
        public string ShippingUpsUsername
        {
            get { return GetProp("Shipping_UPS_Username"); }
            set { SetProp("Shipping_UPS_Username", value); }
        }
        public bool ShippingUPSWriteXML
        {
            get { return GetPropBool("ShippingUPSWriteXML"); }
            set { SetProp("ShippingUPSWriteXML", value); }
        }
        public bool ShippingUSPostalDiagnostics
        {
            get { return GetPropBool("ShippingUSPostalDiagnostics"); }
            set { SetProp("ShippingUSPostalDiagnostics", value); }
        }

        // Affiliates
        public decimal AffiliateCommissionAmount
        {
            get { return GetPropDecimal("AffiliateCommissionAmount"); }
            set { SetProp("AffiliateCommissionAmount", value); }
        }
        public Contacts.AffiliateCommissionType AffiliateCommissionType
        {
            get
            {
                Contacts.AffiliateCommissionType result = Contacts.AffiliateCommissionType.PercentageCommission;
                result = (Contacts.AffiliateCommissionType)GetPropInt("AffiliateCommissionType");
                return result;
            }
            set { SetProp("AffiliateCommissionType", (int)value); }
        }
        public Contacts.AffiliateConflictMode AffiliateConflictMode
        {
            get
            {
                Contacts.AffiliateConflictMode result = Contacts.AffiliateConflictMode.FavorOldAffiliate;
                result = (Contacts.AffiliateConflictMode)GetPropInt("AffiliateConflictMode");
                return result;
            }
            set { SetProp("AffiliateConflictMode", (int)value); }
        }
        public int AffiliateReferralDays
        {
            get { return GetPropInt("AffiliateReferralDays"); }
            set { SetProp("AffiliateReferralDays", value); }
        }
        public bool AllowZeroDollarOrders
        {
            get { return GetPropBool("AllowZeroDollarOrders"); }
            set { SetProp("AllowZeroDollarOrders", value); }
        }

        public bool HideGettingStarted
        {
            get { return GetPropBool("HideGettingStarted"); }
            set { SetProp("HideGettingStarted", value); }
        }
        public bool AutomaticallyIssueRMANumbers
        {
            get { return GetPropBool("AutomaticallyIssueRMANumbers"); }
            set { SetProp("AutomaticallyIssueRMANumbers", value); }
        }
        public string CookieDomain
        {
            get { return GetProp("CookieDomain"); }
            set { SetProp("CookieDomain", value); }
        }
        public string CookiePath
        {
            get { return GetProp("CookiePath"); }
            set { SetProp("CookiePath", value); }
        }

        // Pay Settings
        public MerchantTribe.Payment.MethodSettings PaymentSettingsGet(string methodId)
        {
            string encrypted = GetProp("paysettings" + methodId);
            if (encrypted.Length > 2)
            {
                string key = MerchantTribe.Web.Cryptography.KeyManager.GetKey(0);
                string json = MerchantTribe.Web.Cryptography.AesEncryption.Decode(encrypted, key);
                return MerchantTribe.Web.Json.ObjectFromJson<MerchantTribe.Payment.MethodSettings>(json);
            }
            return new MerchantTribe.Payment.MethodSettings();
        }
        public void PaymentSettingsSet(string methodId, MerchantTribe.Payment.MethodSettings settings)
        {
            string json = MerchantTribe.Web.Json.ObjectToJson(settings);
            string key = MerchantTribe.Web.Cryptography.KeyManager.GetKey(0);
            string encrypted = MerchantTribe.Web.Cryptography.AesEncryption.Encode(json, key);
            SetProp("paysettings" + methodId, encrypted);
        }
        public MerchantTribe.Payment.Method PaymentCurrentCreditCardProcessor()
        {
            MerchantTribe.Payment.Method m = new MerchantTribe.Payment.Methods.TestGateway();
            string gatewayId = this.PaymentCreditCardGateway;
            m = MerchantTribe.Payment.Methods.AvailableMethod.Create(gatewayId);
            if (m != null)
            {
                MerchantTribe.Payment.MethodSettings settings = this.PaymentSettingsGet(gatewayId);
                m.BaseSettings.Merge(settings);
            }
            else
            {
                m = new MerchantTribe.Payment.Methods.TestGateway();
            }
            return m;
        }

        public List<MerchantTribe.Web.Geography.Country> FindActiveCountries()
        {
            return MerchantTribe.Web.Geography.Country.FindAllExceptList(this.DisabledCountryIso3Codes);
        }
        public List<string> DisabledCountryIso3Codes
        {
            get
            {
                List<string> result = new List<string>();
                string data = GetProp("DisabledCountryIso3Codes");
                string[] codes = data.Split(',');
                for (int i = 0; i < codes.Length; i++)
                {
                    result.Add(codes[i]);
                }
                return result;
            }
            set
            {
                string data = string.Empty;
                foreach (string code in value)
                {
                    data += code + ",";
                }
                data = data.TrimEnd(',');
                SetProp("DisabledCountryIso3Codes", data);
            }
        }
        public void DisableCountry(string iso3code)
        {
            List<string> newCodes = new List<string>();
            foreach (string code in DisabledCountryIso3Codes)
            {
                newCodes.Add(code);
                if (code == iso3code.Trim().ToLowerInvariant())
                {
                    // Already disabled
                    return;
                }
            }

            newCodes.Add(iso3code.Trim().ToLowerInvariant());
            DisabledCountryIso3Codes = newCodes;
        }
        public void EnableCountry(string iso3code)
        {
            List<string> newCodes = new List<string>();

            foreach (string code in DisabledCountryIso3Codes)
            {
                if (code != iso3code.Trim().ToLowerInvariant())
                {
                    newCodes.Add(code);
                }
            }

            DisabledCountryIso3Codes = newCodes;
        }

        

    }
}
