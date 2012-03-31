using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Catalog;
using MerchantTribe.Commerce.Membership;
using MerchantTribe.Commerce.Contacts;
using MerchantTribe.Commerce.Content;

namespace MerchantTribeStore
{

    partial class BVAdmin_Catalog_Products_Edit : BaseProductAdminPage
    {

        protected Collection<string> ProductTypeProperties = new Collection<string>();

        string LastProductType
        {
            get
            {
                object obj = ViewState["LastProductType"];
                if (obj != null)
                {
                    return (string)ViewState["LastProductType"];
                }
                else
                {
                    return "";
                }
            }
            set { ViewState["LastProductType"] = value; }
        }

        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);

            SetDefaultValues();
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            this.PageMessageBox = MessageBox1;
            
            if (!Page.IsPostBack)
            {
                PopulateManufacturers();
                PopulateVendors();
                PopulateProductTypes();
                PopulateColumns();
                PopulateTaxes();
                this.SkuField.Focus();

                if (Request.QueryString["id"] != null)
                {
                    this.BvinField.Value = Request.QueryString["id"];
                    LoadProduct();
                    if (Request.QueryString["u"] == "1")
                    {
                        this.MessageBox1.ShowOk("Product Updated");
                    }
                }
                else
                {
                    this.BvinField.Value = string.Empty;
                }
                List<ProductProperty> props = MTApp.CatalogServices.ProductPropertiesFindForType(lstProductType.SelectedValue);
                foreach (ProductProperty prop in props)
                {
                    ProductTypeProperties.Add(MTApp.CatalogServices.ProductPropertyValues.GetPropertyValue(this.BvinField.Value, prop.Id));
                }
                GenerateProductTypePropertyFields();
                this.UrlsAssociated1.ObjectId = this.BvinField.Value;
                this.UrlsAssociated1.LoadUrls();
            }
            else
            {
                //this is a postback
                int count = 1;
                while (Request["ProductTypeProperty" + count.ToString()] != null)
                {
                    ProductTypeProperties.Add(Request["ProductTypeProperty" + count.ToString()]);
                    count += 1;
                }
                CheckIfProductTypePropertyChanged();
                GenerateProductTypePropertyFields();
            }
            
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            this.PageTitle = "Edit Product";
            this.CurrentTab = AdminTabType.Catalog;
            ValidateCurrentUserHasPermission(SystemPermissions.CatalogView);
        }

        protected void SetDefaultValues()
        {
            decimal val = 0;
            ListPriceField.Text = val.ToString("c");
            CostField.Text = val.ToString("c");
            SitePriceField.Text = val.ToString("c");
            WeightField.Text = val.ToString("N");
            LengthField.Text = val.ToString("N");
            WidthField.Text = val.ToString("N");
            HeightField.Text = val.ToString("N");
            ExtraShipFeeField.Text = val.ToString("c");
            this.txtGiftWrapCharge.Text = val.ToString("c");
            LongDescriptionField.EditorHeight = WebAppSettings.ProductLongDescriptionEditorHeight;
        }

        private void PopulateManufacturers()
        {
            this.lstManufacturers.DataSource = MTApp.ContactServices.Manufacturers.FindAll();
            this.lstManufacturers.DataTextField = "DisplayName";
            this.lstManufacturers.DataValueField = "Bvin";
            this.lstManufacturers.DataBind();
            this.lstManufacturers.Items.Insert(0, "- No Manufacturer -");
        }

        private void PopulateVendors()
        {
            this.lstVendors.DataSource = MTApp.ContactServices.Vendors.FindAll();
            this.lstVendors.DataTextField = "DisplayName";
            this.lstVendors.DataValueField = "Bvin";
            this.lstVendors.DataBind();
            this.lstVendors.Items.Insert(0, "- No Vendor -");
        }
        private void PopulateTaxes()
        {
            this.TaxClassField.DataSource = MTApp.OrderServices.TaxSchedules.FindAllAndCreateDefault(MTApp.CurrentStore.Id);
            this.TaxClassField.DataTextField = "Name";
            this.TaxClassField.DataValueField = "Id";
            this.TaxClassField.DataBind();
        }
        private void PopulateProductTypes()
        {
            this.lstProductType.Items.Clear();
            this.lstProductType.Items.Add(new System.Web.UI.WebControls.ListItem("Generic", ""));
            this.lstProductType.AppendDataBoundItems = true;
            this.lstProductType.DataSource = MTApp.CatalogServices.ProductTypes.FindAll();
            this.lstProductType.DataTextField = "ProductTypeName";
            this.lstProductType.DataValueField = "bvin";
            this.lstProductType.DataBind();
        }

        private void PopulateColumns()
        {
            List<ContentColumn> columns = MTApp.ContentServices.Columns.FindAll();
            foreach (ContentColumn col in columns)
            {
                this.PreContentColumnIdField.Items.Add(new System.Web.UI.WebControls.ListItem(col.DisplayName, col.Bvin));
                this.PostContentColumnIdField.Items.Add(new System.Web.UI.WebControls.ListItem(col.DisplayName, col.Bvin));
            }
        }

        private void LoadProduct()
        {
            Product p;
            p = MTApp.CatalogServices.Products.Find(this.BvinField.Value);
            if (p != null)
            {
                if (p.Bvin != string.Empty)
                {
                    if (p.Status == ProductStatus.Active)
                    {
                        this.chkActive.Checked = true;
                    }
                    else
                    {
                        this.chkActive.Checked = false;
                    }

                    this.chkFeatured.Checked = p.Featured;

                    this.SkuField.Text = p.Sku;
                    this.productnamefield.Text = p.ProductName;

                    this.ListPriceField.Text = p.ListPrice.ToString("C");
                    this.CostField.Text = p.SiteCost.ToString("C");
                    this.SitePriceField.Text = p.SitePrice.ToString("C");

                    this.LongDescriptionField.Text = p.LongDescription;
                    if (LongDescriptionField.SupportsTransform == true)
                    {
                        if (p.PreTransformLongDescription.Trim().Length > 0)
                        {
                            this.LongDescriptionField.Text = p.PreTransformLongDescription;
                        }
                    }
                    //Me.ShortDescriptionField.Text = p.ShortDescription

                    //If p.ShortDescription.Length > 255 Then
                    //Me.CountField.Text = "0"
                    //Else
                    //Me.CountField.Text = CType(255 - p.ShortDescription.Length, String)
                    //End If
                    if (p.TaxExempt == true)
                    {
                        this.TaxExemptField.Checked = true;
                    }
                    else
                    {
                        this.TaxExemptField.Checked = false;
                    }
                    if (this.TaxClassField.Items.FindByValue(p.TaxSchedule.ToString()) != null)
                    {
                        this.TaxClassField.ClearSelection();
                        this.TaxClassField.Items.FindByValue(p.TaxSchedule.ToString()).Selected = true;
                    }
                    if (this.lstManufacturers.Items.FindByValue(p.ManufacturerId) != null)
                    {
                        this.lstManufacturers.ClearSelection();
                        this.lstManufacturers.Items.FindByValue(p.ManufacturerId).Selected = true;
                    }
                    if (this.lstVendors.Items.FindByValue(p.VendorId) != null)
                    {
                        this.lstVendors.ClearSelection();
                        this.lstVendors.Items.FindByValue(p.VendorId).Selected = true;
                    }

                    LoadImagePreview(p);
                    this.SmallImageAlternateTextField.Text = p.ImageFileSmallAlternateText;

                    if (this.lstProductType.Items.FindByValue(p.ProductTypeId) != null)
                    {
                        this.lstProductType.ClearSelection();
                        this.lstProductType.Items.FindByValue(p.ProductTypeId).Selected = true;
                    }
                    // Added this line to stop errors on immediate load and save - Marcus
                    this.LastProductType = p.ProductTypeId;

                    if (p.PreContentColumnId.Trim() != string.Empty)
                    {
                        if (this.PreContentColumnIdField.Items.FindByValue(p.PreContentColumnId) != null)
                        {
                            this.PreContentColumnIdField.Items.FindByValue(p.PreContentColumnId).Selected = true;
                        }
                    }
                    if (p.PostContentColumnId.Trim() != string.Empty)
                    {
                        if (this.PostContentColumnIdField.Items.FindByValue(p.PostContentColumnId) != null)
                        {
                            this.PostContentColumnIdField.Items.FindByValue(p.PostContentColumnId).Selected = true;
                        }
                    }

                    this.Keywords.Text = p.Keywords;
                    this.MetaTitleField.Text = p.MetaTitle;
                    this.MetaDescriptionField.Text = p.MetaDescription;
                    this.MetaKeywordsField.Text = p.MetaKeywords;

                    this.WeightField.Text = Math.Round(p.ShippingDetails.Weight, 3, MidpointRounding.AwayFromZero).ToString();
                    this.LengthField.Text = Math.Round(p.ShippingDetails.Length, 3, MidpointRounding.AwayFromZero).ToString();
                    this.WidthField.Text = Math.Round(p.ShippingDetails.Width, 3, MidpointRounding.AwayFromZero).ToString();
                    this.HeightField.Text = Math.Round(p.ShippingDetails.Height, 3, MidpointRounding.AwayFromZero).ToString();

                    this.ExtraShipFeeField.Text = p.ShippingDetails.ExtraShipFee.ToString("C");
                    if (this.ShipTypeField.Items.FindByValue(((int)p.ShippingMode).ToString()) != null)
                    {
                        this.ShipTypeField.ClearSelection();
                        this.ShipTypeField.Items.FindByValue(((int)p.ShippingMode).ToString()).Selected = true;
                    }
                    this.chkNonShipping.Checked = p.ShippingDetails.IsNonShipping;
                    this.chkShipSeparately.Checked = p.ShippingDetails.ShipSeparately;

                    this.MinimumQtyField.Text = Math.Round((decimal)p.MinimumQty, 0, MidpointRounding.AwayFromZero).ToString();

                    this.RewriteUrlField.Text = p.UrlSlug;
                    this.PriceOverrideTextBox.Text = p.SitePriceOverrideText;

                    this.chkGiftWrapAllowed.Checked = p.GiftWrapAllowed;
                    this.txtGiftWrapCharge.Text = p.GiftWrapPrice.ToString("C");

                    this.lnkViewInStore.NavigateUrl = MerchantTribe.Commerce.Utilities.UrlRewriter.BuildUrlForProduct(p, this.Page);
                    
                    this.lnkClone.NavigateUrl = "ProductClone.aspx?id=" + p.Bvin;

                    this.chkAllowReviews.Checked = p.AllowReviews;
                }
            }

        }

        private void LoadImagePreview(Product p)
        {
            this.imgPreviewSmall.ImageUrl = MerchantTribe.Commerce.Storage.DiskStorage.ProductImageUrlSmall(MTApp, p.Bvin, p.ImageFileSmall, true);
        }
      
        private void CancelClick()
        {
            Response.Redirect("~/BVAdmin/Catalog/Default.aspx");
        }

        private void SaveClick()
        {
            this.lblError.Text = string.Empty;

            if (this.Save() == true)
            {
                Response.Redirect("~/BVAdmin/Catalog/Default.aspx");
            }
        }

        private void SaveClickOnPage()
        {
            this.lblError.Text = string.Empty;

            if (this.Save() == true)
            {
                this.Response.Redirect("~/BVAdmin/Catalog/Products_Edit.aspx?id=" + BvinField.Value + "&u=1");
            }
        }

        protected void btnCancel_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            CancelClick();
        }

        protected void btnSaveChanges_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            SaveClick();
        }

        protected void btnCancel2_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            CancelClick();
        }

        protected void btnSave2_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            SaveClick();
        }

        protected override bool Save()
        {
            bool result = false;
            if (Page.IsValid)
            {
                Product p;
                p = MTApp.CatalogServices.Products.Find(this.BvinField.Value);
                if (p == null)
                {
                    //if it is nothing then create a new product
                    p = new Product();
                }

                if (p != null)
                {

                    if (this.chkActive.Checked)
                    {
                        p.Status = ProductStatus.Active;
                    }
                    else
                    {
                        p.Status = ProductStatus.Disabled;
                    }

                    p.Featured = this.chkFeatured.Checked;

                    if (string.Compare(p.Sku.Trim(), this.SkuField.Text.Trim(), true) != 0)
                    {
                        //sku changed, so do a sku check
                        Product skuCheckProduct = MTApp.CatalogServices.Products.FindBySku(this.SkuField.Text.Trim());
                        if (skuCheckProduct != null)
                        {
                            MessageBox1.ShowError("Sku already exists on another product. Please pick another sku.");
                            return false;
                        }
                    }

                    p.Sku = this.SkuField.Text.Trim();


                    p.ProductName = this.productnamefield.Text.Trim();
                    if (this.lstProductType.SelectedValue != null)
                    {
                        p.ProductTypeId = this.lstProductType.SelectedValue;
                    }
                    else
                    {
                        p.ProductTypeId = string.Empty;
                    }

                    decimal listPrice;
                    if (decimal.TryParse(this.ListPriceField.Text, System.Globalization.NumberStyles.Currency,
                                        System.Threading.Thread.CurrentThread.CurrentUICulture, out listPrice))
                    {
                        p.ListPrice = listPrice;
                    }
                    decimal cost;
                    if (decimal.TryParse(this.CostField.Text, System.Globalization.NumberStyles.Currency,
                        System.Threading.Thread.CurrentThread.CurrentUICulture, out cost))
                    {
                        p.SiteCost = cost;
                    }
                    decimal price;
                    if (decimal.TryParse(this.SitePriceField.Text, System.Globalization.NumberStyles.Currency,
                        System.Threading.Thread.CurrentThread.CurrentUICulture, out price))
                    {
                        p.SitePrice = price;
                    }
                    p.LongDescription = this.LongDescriptionField.Text.Trim();
                    p.PreTransformLongDescription = this.LongDescriptionField.PreTransformText;
                    //p.ShortDescription = Me.ShortDescriptionField.Text.Trim
                    if (this.lstManufacturers.SelectedValue != null)
                    {
                        p.ManufacturerId = this.lstManufacturers.SelectedValue;
                    }
                    else
                    {
                        p.ManufacturerId = string.Empty;
                    }
                    if (this.lstVendors.SelectedValue != null)
                    {
                        p.VendorId = this.lstVendors.SelectedValue;
                    }
                    else
                    {
                        p.VendorId = string.Empty;
                    }
                    //If Me.lstTemplateName.SelectedValue IsNot Nothing Then
                    //p.TemplateName = Me.lstTemplateName.SelectedValue
                    //End If


                    if (string.IsNullOrEmpty(this.SmallImageAlternateTextField.Text))
                    {
                        p.ImageFileSmallAlternateText = p.ProductName + " " + p.Sku;
                    }
                    else
                    {
                        p.ImageFileSmallAlternateText = this.SmallImageAlternateTextField.Text;
                    }

                    p.PreContentColumnId = this.PreContentColumnIdField.SelectedValue;
                    p.PostContentColumnId = this.PostContentColumnIdField.SelectedValue;

                    string oldUrl = p.UrlSlug;

                    // no entry, generate one
                    if (p.UrlSlug.Trim().Length < 1)
                    {
                        p.UrlSlug = MerchantTribe.Web.Text.Slugify(p.ProductName, true, true);
                    }
                    else
                    {
                        p.UrlSlug = MerchantTribe.Web.Text.Slugify(this.RewriteUrlField.Text, true, true);
                    }

                    if (MerchantTribe.Commerce.Utilities.UrlRewriter.IsProductSlugInUse(p.UrlSlug, p.Bvin, MTApp))
                    {
                        this.MessageBox1.ShowWarning("The requested URL is already in use by another item.");
                        return false;
                    }

                    p.SitePriceOverrideText = this.PriceOverrideTextBox.Text.Trim();
                    p.Keywords = this.Keywords.Text.Trim();
                    p.ProductTypeId = this.lstProductType.SelectedValue;
                    p.TaxSchedule = long.Parse(this.TaxClassField.SelectedValue);

                    p.MetaTitle = this.MetaTitleField.Text.Trim();
                    p.MetaDescription = this.MetaDescriptionField.Text.Trim();
                    p.MetaKeywords = this.MetaKeywordsField.Text.Trim();

                    p.ShippingDetails.Weight = decimal.Parse(this.WeightField.Text, System.Globalization.NumberStyles.Float, System.Threading.Thread.CurrentThread.CurrentUICulture);
                    p.ShippingDetails.Length = decimal.Parse(this.LengthField.Text, System.Globalization.NumberStyles.Float, System.Threading.Thread.CurrentThread.CurrentUICulture);
                    p.ShippingDetails.Width = decimal.Parse(this.WidthField.Text, System.Globalization.NumberStyles.Float, System.Threading.Thread.CurrentThread.CurrentUICulture);
                    p.ShippingDetails.Height = decimal.Parse(this.HeightField.Text, System.Globalization.NumberStyles.Float, System.Threading.Thread.CurrentThread.CurrentUICulture);

                    p.ShippingDetails.ExtraShipFee = decimal.Parse(this.ExtraShipFeeField.Text, System.Globalization.NumberStyles.Currency, System.Threading.Thread.CurrentThread.CurrentUICulture);
                    p.ShippingMode = (MerchantTribe.Commerce.Shipping.ShippingMode)int.Parse(this.ShipTypeField.SelectedValue);
                    p.ShippingDetails.IsNonShipping = this.chkNonShipping.Checked;
                    p.ShippingDetails.ShipSeparately = this.chkShipSeparately.Checked;

                    p.MinimumQty = int.Parse(this.MinimumQtyField.Text, System.Globalization.NumberStyles.Integer, System.Threading.Thread.CurrentThread.CurrentUICulture);

                    if (this.TaxExemptField.Checked == true)
                    {
                        p.TaxExempt = true;
                    }
                    else
                    {
                        p.TaxExempt = false;
                    }

                    p.GiftWrapAllowed = false;
                    //p.GiftWrapAllowed = this.chkGiftWrapAllowed.Checked;
                    //p.SetGiftWrapPrice(decimal.Parse(this.txtGiftWrapCharge.Text));

                    p.AllowReviews = this.chkAllowReviews.Checked;

                    if ((p.Bvin == string.Empty))
                    {
                        result = MTApp.CatalogServices.ProductsCreateWithInventory(p, true);
                    }
                    else
                    {
                        result = MTApp.CatalogServices.ProductsUpdateWithSearchRebuild(p);
                    }

                    if (result)
                    {
                        UploadImage(p);
                    }

                    if (result)
                    {
                        List<ProductProperty> props = MTApp.CatalogServices.ProductPropertiesFindForType(lstProductType.SelectedValue);
                        MTApp.CatalogServices.ProductPropertyValues.DeleteByProductId(p.Bvin);
                        for (int i = 0; i <= (props.Count - 1); i++)
                        {
                            MTApp.CatalogServices.ProductPropertyValues.SetPropertyValue(p.Bvin, props[i].Id, ProductTypeProperties[i]);
                        }
                    }

                    if (result == false)
                    {
                        this.MessageBox1.ShowError("Unable to save product. Unknown error. Please check event log.");
                    }
                    else
                    {
                        // Update bvin field so that next save will call updated instead of create
                        this.BvinField.Value = p.Bvin;

                        if (oldUrl != string.Empty)
                        {
                            if (oldUrl != p.UrlSlug)
                            {
                                MTApp.ContentServices.CustomUrls.Register301(GetRouteUrl("bvroute", new { slug = oldUrl }),
                                                      GetRouteUrl("bvroute", new { slug = p.UrlSlug }),
                                                      p.Bvin, CustomUrlType.Product, MTApp.CurrentRequestContext, MTApp);
                                this.UrlsAssociated1.LoadUrls();
                            }
                        }
                    }

                    HttpContext.Current.Items["productid"] = p.Bvin;
                }
            }
            return result;
        }


        private bool UploadImage(Product p)
        {
            // Image Upload
            if ((this.imgupload.HasFile))
            {
                // Apparently, .PostedFile.FileName returns the full path (in IE)
                // where .FileName returns just the file name which is what we want.

                //string fileName = System.IO.Path.GetFileNameWithoutExtension(imgupload.PostedFile.FileName);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(imgupload.FileName);
                //string ext = System.IO.Path.GetExtension(imgupload.PostedFile.FileName);
                string ext = System.IO.Path.GetExtension(imgupload.FileName);

                if (MerchantTribe.Commerce.Storage.DiskStorage.ValidateImageType(ext))
                {
                    fileName = MerchantTribe.Web.Text.CleanFileName(fileName);
                    if ((MerchantTribe.Commerce.Storage.DiskStorage.UploadProductImage(MTApp.CurrentStore.Id, p.Bvin, this.imgupload.PostedFile)))
                    {
                        p.ImageFileSmall = fileName + ext;
                    }
                }
                else
                {
                    this.MessageBox1.ShowError("Only .PNG, .JPG, .GIF file types are allowed for icon images");
                    return false;
                }
            }

            return MTApp.CatalogServices.ProductsUpdateWithSearchRebuild(p);
        }

        protected void CheckIfProductTypePropertyChanged()
        {
            if (lstProductType.SelectedValue != LastProductType)
            {
                ProductTypeProperties.Clear();
                LastProductType = lstProductType.SelectedValue;
            }
        }

        protected void GenerateProductTypePropertyFields()
        {
            ProductTypePropertiesLiteral.Text = "";
            if (lstProductType.SelectedValue.Trim() != string.Empty)
            {
                string productTypeBvin = lstProductType.SelectedValue;
                List<ProductProperty> props = MTApp.CatalogServices.ProductPropertiesFindForType(productTypeBvin);
                StringBuilder sb = new StringBuilder();
                int count = 0;
                foreach (ProductProperty item in props)
                {
                    count += 1;
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter writer = new HtmlTextWriter(sw);
                    sb.Append("<tr><td class=\"formlabel\">");
                    sb.Append(item.DisplayName);
                    sb.Append("</td><td class=\"formfield\">");
                    if (item.TypeCode == ProductPropertyType.CurrencyField)
                    {
                        HtmlInputText input = new HtmlInputText();
                        input.ID = "ProductTypeProperty" + count.ToString();
                        if (ProductTypeProperties.Count > (count - 1))
                        {
                            if (ProductTypeProperties[count - 1] != null)
                            {
                                input.Value = ProductTypeProperties[count - 1];
                            }
                            else
                            {
                                input.Value = item.DefaultValue;
                            }
                        }
                        else
                        {
                            input.Value = item.DefaultValue;
                        }
                        input.RenderControl(writer);
                        writer.Flush();
                        sb.Append(sw.ToString());
                    }
                    else if (item.TypeCode == ProductPropertyType.DateField)
                    {
                        HtmlInputText input = new HtmlInputText();
                        input.ID = "ProductTypeProperty" + count.ToString();
                        if (ProductTypeProperties.Count > (count - 1))
                        {
                            if (ProductTypeProperties[count - 1] != null)
                            {
                                input.Value = ProductTypeProperties[count - 1];
                            }
                            else
                            {
                                input.Value = item.DefaultValue;
                            }
                        }
                        else
                        {
                            input.Value = item.DefaultValue;
                        }
                        input.RenderControl(writer);
                        writer.Flush();
                        sb.Append(sw.ToString());
                    }
                    else if (item.TypeCode == ProductPropertyType.HyperLink)
                    {
                        HtmlInputText input = new HtmlInputText();
                        input.ID = "ProductTypeProperty" + count.ToString();
                        if (ProductTypeProperties.Count > (count - 1))
                        {
                            if (ProductTypeProperties[count - 1] != null)
                            {
                                input.Value = ProductTypeProperties[count - 1];
                            }
                            else
                            {
                                input.Value = item.DefaultValue;
                            }
                        }
                        else
                        {
                            input.Value = item.DefaultValue;
                        }
                        input.RenderControl(writer);
                        writer.Flush();
                        sb.Append(sw.ToString());
                    }
                    else if (item.TypeCode == ProductPropertyType.MultipleChoiceField)
                    {
                        HtmlSelect input = new HtmlSelect();
                        input.ID = "ProductTypeProperty" + count.ToString();                        
                        bool setWidth = false;                        
                        foreach (ProductPropertyChoice choice in item.Choices)
                        {
                            if (choice.ChoiceName.Length > 25)
                            {
                                setWidth = true;
                            }
                            System.Web.UI.WebControls.ListItem li = new System.Web.UI.WebControls.ListItem(choice.ChoiceName, choice.Id.ToString());
                            input.Items.Add(li);
                        }
                        if (setWidth)
                        {
                            input.Style.Add("width", "305px");
                        }

                        if (ProductTypeProperties.Count > (count - 1))
                        {
                            if (ProductTypeProperties[count - 1] != null)
                            {
                                input.Value = ProductTypeProperties[count - 1];
                            }
                            else
                            {
                                input.Value = item.DefaultValue;
                            }
                        }
                        else
                        {
                            input.Value = item.DefaultValue;
                        }
                        input.RenderControl(writer);
                        writer.Flush();
                        sb.Append(sw.ToString());
                    }
                    else if (item.TypeCode == ProductPropertyType.TextField)
                    {
                        HtmlTextArea input = new HtmlTextArea();
                        input.ID = "ProductTypeProperty" + count.ToString();
                        if (ProductTypeProperties.Count > (count - 1))
                        {
                            if (ProductTypeProperties[count - 1] != null)
                            {
                                input.Value = ProductTypeProperties[count - 1];
                            }
                            else
                            {
                                input.Value = item.DefaultValue;
                            }
                        }
                        else
                        {
                            input.Value = item.DefaultValue;
                        }
                        input.Rows = 5;
                        input.Cols = 40;
                        input.RenderControl(writer);
                        writer.Flush();
                        sb.Append(sw.ToString());
                    }
                    sb.Append("</td></tr>");
                    ProductTypePropertiesLiteral.Text = sb.ToString();
                }
            }
        }

        protected void ProductTypeCustomValidator_ServerValidate(object source, System.Web.UI.WebControls.ServerValidateEventArgs args)
        {
            List<ProductProperty> props = MTApp.CatalogServices.ProductPropertiesFindForType(lstProductType.SelectedValue);
            for (int i = 0; i <= (ProductTypeProperties.Count - 1); i++)
            {
                switch (props[i].TypeCode)
                {
                    case ProductPropertyType.CurrencyField:
                        decimal temp;
                        if (!decimal.TryParse(ProductTypeProperties[i], out temp))
                        {
                            args.IsValid = false;
                            ProductTypeCustomValidator.ErrorMessage = props[i].DisplayName + " must be a valid monetary type.";
                            return;
                        }
                        break;
                    case ProductPropertyType.DateField:
                        DateTime temp2;
                        if (!System.DateTime.TryParse(ProductTypeProperties[i], out temp2))
                        {
                            args.IsValid = false;
                            ProductTypeCustomValidator.ErrorMessage = props[i].DisplayName + " must be a valid date.";
                            return;
                        }
                        break;
                    case ProductPropertyType.MultipleChoiceField:
                        break;

                }
            }
        }

        protected void update1_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            SaveClickOnPage();
        }

        protected void update2_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            SaveClickOnPage();
        }

        protected void IsNumeric_ServerValidate(object source, System.Web.UI.WebControls.ServerValidateEventArgs args)
        {
            decimal val = 0;
            if (decimal.TryParse(args.Value, System.Globalization.NumberStyles.Float, System.Threading.Thread.CurrentThread.CurrentUICulture, out val))
            {
                args.IsValid = true;
            }
            else
            {
                args.IsValid = false;
            }
        }

        protected void IsMonetary_ServerValidate(object source, System.Web.UI.WebControls.ServerValidateEventArgs args)
        {
            decimal val = 0m;
            if (decimal.TryParse(args.Value, System.Globalization.NumberStyles.Currency, System.Threading.Thread.CurrentThread.CurrentUICulture, out val))
            {
                args.IsValid = true;
            }
            else
            {
                args.IsValid = false;
            }
        }

        protected void ProductPropertyCopier_Clicked(object sender, MerchantTribe.Commerce.Content.NotifyClickControl.ClickedEventArgs e)
        {
            if (!this.Save())
            {
                e.ErrorOccurred = true;
            }
        }

        protected void valSkuLength_ServerValidate(object source, System.Web.UI.WebControls.ServerValidateEventArgs args)
        {
            if ((this.SkuField.Text.Trim().Length > 50))
            {
                args.IsValid = false;
            }
            else
            {
                args.IsValid = true;
            }
        }

        protected void valNameLength_ServerValidate(object source, System.Web.UI.WebControls.ServerValidateEventArgs args)
        {
            if ((this.productnamefield.Text.Trim().Length > 255))
            {
                args.IsValid = false;
            }
            else
            {
                args.IsValid = true;
            }
        }

        protected void btnDelete_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            if (!MTApp.DestroyProduct(this.BvinField.Value))
            {
                this.MessageBox1.ShowWarning("Unable to delete product. Unknown Error.");
            }
            else
            {
                Response.Redirect("default.aspx");
            }
            
        }
      
    }
}