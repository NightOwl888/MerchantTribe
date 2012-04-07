using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using MerchantTribe.Commerce.Contacts;
using MerchantTribe.Commerce.Membership;
using MerchantTribe.Commerce.Orders;

namespace MerchantTribeStore
{

    partial class BVAdmin_Reports_Affiliates : BaseAdminPage
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                MonthDropDownList.SelectedValue = System.DateTime.Now.Month.ToString();
                YearDropDownList.SelectedValue = System.DateTime.Now.Year.ToString();
                BindAffiliates();
            }

        }

        protected void BindAffiliates()
        {
            AffiliatesDropDownList.DataSource = MTApp.ContactServices.Affiliates.FindAllPaged(1, int.MaxValue);
            AffiliatesDropDownList.DataTextField = "DisplayName";
            AffiliatesDropDownList.DataValueField = "Id";
            AffiliatesDropDownList.DataBind();
            AffiliatesDropDownList.Items.Insert(0, "-All Affiliates-");
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            this.PageTitle = "Sales By Affiliates";
            this.CurrentTab = AdminTabType.Reports;
            ValidateCurrentUserHasPermission(SystemPermissions.ReportsView);
        }

        protected void ViewImageButton_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            List<Affiliate> affiliates;
            if (AffiliatesDropDownList.SelectedValue == "-All Affiliates-")
            {
                affiliates = MTApp.ContactServices.Affiliates.FindAllPaged(1, int.MaxValue);
            }
            else
            {
                affiliates = new List<Affiliate>();
                long temp = 0;
                long.TryParse(AffiliatesDropDownList.SelectedValue, out temp);
                Affiliate a = MTApp.ContactServices.Affiliates.Find(temp);
                if (a != null)
                {
                    affiliates.Add(a);
                }
            }

            AffiliatesDataList.DataSource = affiliates;
            AffiliatesDataList.DataKeyField = "Id";
            AffiliatesDataList.DataBind();
        }

        protected void AffiliatesDataList_ItemDataBound(object sender, System.Web.UI.WebControls.DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item | e.Item.ItemType == ListItemType.AlternatingItem)
            {
                if (e.Item.DataItem != null)
                {
                    OrderSearchCriteria orderCriteria = new OrderSearchCriteria();
                    AffiliateReferralSearchCriteria referralCriteria = new AffiliateReferralSearchCriteria();

                    Affiliate affiliate = (Affiliate)e.Item.DataItem;

                    orderCriteria.AffiliateId = affiliate.ReferralId;

                    DateTime zonedStart = new DateTime(int.Parse(YearDropDownList.SelectedValue), int.Parse(MonthDropDownList.SelectedValue), 1, 0, 0, 0, 0);
                    orderCriteria.StartDateUtc = TimeZoneInfo.ConvertTimeToUtc(zonedStart, MTApp.CurrentStore.Settings.TimeZone);
                    orderCriteria.EndDateUtc = orderCriteria.StartDateUtc.AddMonths(1);
                    orderCriteria.EndDateUtc = orderCriteria.EndDateUtc.AddSeconds(-1);

                    referralCriteria.AffiliateId = affiliate.Id;
                    referralCriteria.StartDateUtc = orderCriteria.StartDateUtc;
                    referralCriteria.EndDateUtc = orderCriteria.EndDateUtc;

                    List<OrderSnapshot> affiliateOrders = MTApp.OrderServices.Orders.FindByCriteria(orderCriteria);

                    Label currLabel = (Label)e.Item.FindControl("ReferralsLabel");
                    int totalCount = 0;
                    List<AffiliateReferral> referrals = MTApp.ContactServices.AffiliateReferrals.FindByCriteria(referralCriteria, 1, int.MaxValue, ref totalCount);
                    currLabel.Text = referrals.Count.ToString();

                    currLabel = (Label)e.Item.FindControl("SalesLabel");
                    currLabel.Text = affiliateOrders.Count.ToString();

                    currLabel = (Label)e.Item.FindControl("ConversionLabel");
                    double conversion = (double)affiliateOrders.Count / (double)referrals.Count;
                    currLabel.Text = string.Format("{0:p}", conversion);

                    decimal total = 0;

                    foreach (OrderSnapshot order in affiliateOrders)
                    {
                        total += order.TotalOrderBeforeDiscounts;
                    }

                    currLabel = (Label)e.Item.FindControl("CommissionLabel");
                    decimal commission = 0;
                    string commissionText = string.Empty;
                    if (affiliate != null)
                    {
                        if (affiliate.CommissionType == AffiliateCommissionType.FlatRateCommission)
                        {
                            commission = Math.Round(affiliate.CommissionAmount * (decimal)affiliateOrders.Count, 2, MTApp.CurrentStore.Settings.MidpointRoundingMode);
                            commissionText = string.Format("{0:c}", affiliate.CommissionAmount) + " per";
                        }
                        else
                        {
                            commission = Math.Round((affiliate.CommissionAmount / (decimal)100) * total, 2, MTApp.CurrentStore.Settings.MidpointRoundingMode);
                            commissionText = string.Format("{0:p}", (affiliate.CommissionAmount / (decimal)100));
                        }

                        currLabel.Text = commissionText + " = " + string.Format("{0:c}", commission);
                    }

                    GridView gv = (GridView)e.Item.FindControl("OrdersGridView");
                    if (gv != null)
                    {
                        gv.DataSource = affiliateOrders;
                        gv.DataKeyNames = new string[] { "bvin" };
                        gv.DataBind();
                    }

                    Literal openDiv = (Literal)e.Item.FindControl("openDiv");
                    Literal closeDiv = (Literal)e.Item.FindControl("closeDiv");

                    if ((openDiv != null) & (closeDiv != null))
                    {
                        openDiv.Text = "<a href=\"";
                        openDiv.Text += "javascript:toggle('aff" + affiliate.Id + "');";
                        openDiv.Text += "\"><img id=\"aff" + affiliate.Id + "Carrot\" name=\"aff" + affiliate.Id + "Carrot\" src=\"../../../BVAdmin/Images/Buttons/Details.png\" border=\"0\" alt=\"Details\"></a></img><div style=\"display:none\" id=\"aff" + affiliate.Id + "\" class=\"hidden\">";

                        closeDiv.Text = "</div>";
                    }
                }
            }
        }

        protected void NextImageButton_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            int month = int.Parse(MonthDropDownList.SelectedValue);
            int year = int.Parse(YearDropDownList.SelectedValue);
            if (month < 12)
            {
                month += 1;
            }
            else
            {
                month = 1;
                year += 1;
            }
            MonthDropDownList.SelectedValue = month.ToString();
            YearDropDownList.SelectedValue = year.ToString();
        }

        protected void PreviousImageButton_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            int month = int.Parse(MonthDropDownList.SelectedValue);
            int year = int.Parse(YearDropDownList.SelectedValue);
            if (month > 1)
            {
                month -= 1;
            }
            else
            {
                month = 12;
                year -= 1;
            }
            MonthDropDownList.SelectedValue = month.ToString();
            YearDropDownList.SelectedValue = year.ToString();
        }
    }
}