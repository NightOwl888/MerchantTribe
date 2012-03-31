using System;
using System.Text;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Catalog;

namespace MerchantTribeStore
{

    partial class BVAdmin_Controls_OptionItemEditor : MerchantTribe.Commerce.Content.BVUserControl
    {

        private string _OptionId = string.Empty;
        private bool _AllowLabels = false;
        public string OptionId
        {
            get { return _OptionId; }
            set { _OptionId = value; }
        }
        public bool AllowLabels
        {
            get { return _AllowLabels; }
            set { _AllowLabels = value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Page.ClientScript.RegisterClientScriptBlock(typeof(System.Web.UI.Page), "optionitemssort", RenderJQuery(), true);
            Page.ClientScript.RegisterClientScriptBlock(typeof(System.Web.UI.Page), "optionitemsdialog", RenderDialogJquery(), true);

            RenderItems();
        }


        public void RenderItems()
        {

            Option opt = MyPage.MTApp.CatalogServices.ProductOptions.Find(OptionId);
            if ((opt != null))
            {
                //opt.ReloadItems();
                StringBuilder sb = new StringBuilder();
                sb.Append("<div class=\"option-items-list\">");

                foreach (OptionItem oi in opt.Items)
                {
                    sb.Append(RenderSingleItem(oi));
                }

                sb.Append("</div>");
                this.litItem.Text = sb.ToString();
            }
        }

        private string RenderSingleItem(OptionItem oi)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class=\"dragitem\" id=\"" + oi.Bvin + "\"><table class=\"formtable\" width=\"100%\"><tr>");
            sb.Append("<td width=\"50%\" class=\"namefield\">" + oi.Name + "</td>");
            sb.Append("<td width=\"20%\" class=\"pricefield\">Price: " + oi.PriceAdjustment.ToString("c") + "</td>");
            sb.Append("<td width=\"20%\" class=\"weightfield\">Weight: " + Math.Round(oi.WeightAdjustment, 3, MidpointRounding.AwayFromZero).ToString() + "</td>");
            sb.Append("<td width=\"75\"><a href=\"#\" class=\"dragitemedit\" id=\"edit" + oi.Bvin + "\"><img src=\"../images/buttons/edit.png\" alt=\"edit\" /></a></td>");
            sb.Append("<td><a href=\"#\" class=\"trash\" id=\"rem" + oi.Bvin + "\"><img src=\"../../images/system/trashcan.png\" alt=\"Delete\" /></a></td>");
            sb.Append("<td><a href=\"#\" class=\"handle\"><img src=\"../../images/system/draghandle.png\" alt=\"Move\" /></a></td>");
            sb.Append("</tr></table></div>");

            return sb.ToString();
        }

        protected void btnNew_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            if ((OptionId != string.Empty))
            {
                Option opt = MyPage.MTApp.CatalogServices.ProductOptions.Find(OptionId);
                if ((opt != null))
                {
                    opt.AddItem(this.NewNameField.Text.Trim());
                    MyPage.MTApp.CatalogServices.ProductOptions.Update(opt);
                    this.NewNameField.Text = string.Empty;
                    this.NewNameField.Focus();
                }
            }
            RenderItems();
        }

        private string RenderDialogJquery()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("function CloseDialog() {");
            sb.Append("$('.overlay').remove();");
            sb.Append("$('.modal').hide();");
            sb.Append("}");

            sb.Append("function OpenDialog(lnk) {");
            sb.Append("$('<div />').addClass('overlay').appendTo('body').show();");
            sb.Append("$('.modal').show();");
            sb.Append(" var loadid = $(lnk).attr('id');");
            sb.Append(" LoadDialog(loadid.replace('edit', ''));");
            sb.Append("}");

            sb.Append("function LoadDialog(itemid) {");
            sb.Append(" var dest = 'OptionItems_Get.aspx?id=' + itemid;");
            sb.Append("$.getJSON(dest,'', function(data) {");
            sb.Append("$('#dialog-bvin').val(data.Bvin);");
            sb.Append("$('#dialog-name').val(data.Name);");
            sb.Append("var isLabel = data.IsLabel;");
            sb.Append(" if (isLabel == true) { $('#dialog-label').attr('checked', true); }");
            sb.Append(" else { $('#dialog-label').removeAttr('checked');} ");
            sb.Append("$('#dialog-price').val(data.PriceAdjustment);");
            sb.Append("$('#dialog-weight').val(data.WeightAdjustment);");
            sb.Append("});");
            sb.Append("}");

            sb.Append("function SaveDialog() {");
            sb.Append(" var isLabel = $('#dialog-label').is(':checked'); ");
            sb.Append(" var isLabelData = '';");
            sb.Append(" if (isLabel == true) isLabelData = 'checked';");
            sb.Append("$.post('OptionItems_Update.aspx',");
            sb.Append("  { \"id\": $('#dialog-bvin').val(),");
            sb.Append("  \"name\": $('#dialog-name').val(), ");
            sb.Append("  \"islabel\": isLabelData, ");
            sb.Append("  \"price\": $('#dialog-price').val(), ");
            sb.Append("  \"weight\": $('#dialog-weight').val(), ");
            sb.Append("  },");
            sb.Append("  function() {");
            sb.Append("    CloseDialog();");
            sb.Append("    $('#btnSaveOption').click();");
            sb.Append("  }");
            sb.Append(" );");
            sb.Append("}");


            return sb.ToString();
        }

        private string RenderJQuery()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("$(document).ready(function() {");

            sb.Append("$(\".option-items-list\").sortable({");
            sb.Append("placeholder:  'ui-state-highlight',");
            sb.Append("axis:   'y',");
            sb.Append("opacity:  '0.75',");
            sb.Append("cursor:  'move',");
            sb.Append("update: function(event, ui) {");
            sb.Append(" var sorted = $(this).sortable('toArray');");
            sb.Append(" sorted += '';"); // append string to force json to render as string
            sb.Append("$.post('OptionItems_Sort.aspx',");
            sb.Append("  { \"ids\": sorted,");
            sb.Append("    \"optionid\": \"" + OptionId + "\"");
            sb.Append("  });");
            sb.Append(" }");
            sb.Append("});");


            // Dialog Functions
            sb.Append("$(\".dragitemedit\").click(function() {");
            sb.Append("OpenDialog($(this));");
            sb.Append("});");

            sb.Append("$(\"#dialog-close\").click(function() {");
            sb.Append("CloseDialog();");
            sb.Append("return false;");
            sb.Append("});");

            sb.Append("$(\"#dialog-save\").click(function() {");
            sb.Append("SaveDialog();");
            sb.Append("return false;");
            sb.Append("});");


            sb.Append("$('.option-items-list').disableSelection();");

            sb.Append("$('.trash').click(function() {");
            sb.Append("RemoveItem($(this)); return false;");
            sb.Append("});");

            sb.Append("});");
            // End of Document Ready

            sb.Append("function RemoveItem(lnk) {");
            sb.Append("  var id = $(lnk).attr('id');");
            sb.Append("$.post('OptionItems_Delete.aspx',");
            sb.Append("  { \"id\": id.replace('rem', ''),");
            sb.Append("  \"optionid\": \"" + OptionId + "\"");
            sb.Append("  },");
            sb.Append("  function() {");
            sb.Append("lnk.parent().parent().parent().parent().parent().slideUp('slow', function() {");
            sb.Append("lnk.parent().parent().parent().parent().parent().remove();});");
            sb.Append("  }");
            sb.Append(" );");
            sb.Append("}");

            return sb.ToString();
        }
    }
}