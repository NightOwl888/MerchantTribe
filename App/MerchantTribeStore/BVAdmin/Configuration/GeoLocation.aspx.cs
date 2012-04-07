using System;
using System.Collections.Generic;
using System.Web.UI;
using MerchantTribe.Commerce.Membership;
using MerchantTribe.Web.Geography;
using MerchantTribe.Commerce.Utilities;

namespace MerchantTribeStore.BVAdmin.Configuration
{
    public partial class GeoLocation : BaseAdminPage
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            this.PageTitle = "Geo-Location";
            this.CurrentTab = AdminTabType.Configuration;
            ValidateCurrentUserHasPermission(SystemPermissions.SettingsView);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                PopulateTimeZones();
                PopulateCultures();
                PopulateMidpointRoundingModes();
                SetTimeZone();
                SetCulture();
                SetMidpointRoundingMode();
            }
        }

        private void PopulateTimeZones()
        {
            this.lstTimeZone.DataSource = TimeZoneInfo.GetSystemTimeZones();
            this.lstTimeZone.DataTextField = "DisplayName";
            this.lstTimeZone.DataValueField = "Id";
            this.lstTimeZone.DataBind();
        }

        private void PopulateCultures()
        {
            List<Country> allCountries = MerchantTribe.Web.Geography.Country.FindAll();
            
            // Trim down duplicate culture codes that might conflict
            // with United States as default for en-US (like Niger)
            List<Country> trimmed = new List<Country>();
            foreach (Country c in allCountries)
            {
                if (c.CultureCode == "en-US")
                {
                    if (c.Bvin == Country.UnitedStatesCountryBvin)
                    {
                        trimmed.Add(c);
                    }
                }
                else
                {
                    trimmed.Add(c);
                }
            }
            

            this.lstCulture.DataSource = trimmed;
            this.lstCulture.DataTextField = "SampleNameAndCurrency";
            this.lstCulture.DataValueField = "CultureCode";
            this.lstCulture.DataBind();                       
        }

        private void PopulateMidpointRoundingModes()
        {
            IDictionary<string, string> modes = new Dictionary<string, string>();
            foreach (MidpointRounding m in Enum.GetValues(typeof(MidpointRounding)))
            {
                modes.Add(new KeyValuePair<string,string>(m.ToString(), EnumToString.MidpointRoundingModes(m)));
            }

            this.lstMidpointRoundingMode.DataSource = modes;
            this.lstMidpointRoundingMode.DataTextField = "Value";
            this.lstMidpointRoundingMode.DataValueField = "Key";
            this.lstMidpointRoundingMode.DataBind();
        }

        private void SetTimeZone()
        {
            TimeZoneInfo t = MTApp.CurrentStore.Settings.TimeZone;
            if (this.lstTimeZone.Items.FindByValue(t.Id) != null)
            {
                this.lstTimeZone.ClearSelection();
                this.lstTimeZone.Items.FindByValue(t.Id).Selected = true;
            }
        }

        private void SetCulture()
        {
            string cc = MTApp.CurrentStore.Settings.CultureCode;
            if (this.lstCulture.Items.FindByValue(cc) != null)
            {
                this.lstCulture.ClearSelection();
                this.lstCulture.Items.FindByValue(cc).Selected = true;
            }
        }

        private void SetMidpointRoundingMode()
        {
            MidpointRounding m = MTApp.CurrentStore.Settings.MidpointRoundingMode;
            if (this.lstMidpointRoundingMode.Items.FindByValue(m.ToString()) != null)
            {
                this.lstMidpointRoundingMode.ClearSelection();
                this.lstMidpointRoundingMode.Items.FindByValue(m.ToString()).Selected = true;
            }
        }

        protected void btnSubmit_Click(object sender, ImageClickEventArgs e)
        {
            TimeZoneInfo t = TimeZoneInfo.FindSystemTimeZoneById(this.lstTimeZone.SelectedItem.Value);
            if (t != null)
            {
                MTApp.CurrentStore.Settings.TimeZone = t;
            }
                        
            string cc = this.lstCulture.SelectedItem.Value;
            MTApp.CurrentStore.Settings.CultureCode = cc;

            string mr = this.lstMidpointRoundingMode.SelectedValue;
            MidpointRounding m;
            if (!Enum.TryParse<MidpointRounding>(mr, out m))
            {
                m = MidpointRounding.AwayFromZero;
            }
            MTApp.CurrentStore.Settings.MidpointRoundingMode = m;

            MTApp.AccountServices.Stores.Update(MTApp.CurrentStore);

            this.MessageBox1.ShowOk("Changed Saved!");
        }
    }
}