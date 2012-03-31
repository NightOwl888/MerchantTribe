using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using MerchantTribe.Commerce.Membership;
using MerchantTribe.Commerce.Orders;
using System.Linq;

namespace MerchantTribeStore
{

    partial class BVAdmin_Reports_Sales_Day : BaseAdminPage
    {
        private decimal TotalSub = 0;
        private decimal TotalDiscounts = 0;
        private decimal TotalShip = 0;
        private decimal TotalShipDiscounts = 0;
        private decimal TotalTax = 0;       
        private decimal TotalGrand = 0;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            this.PageTitle = "Daily Transactions";
            this.CurrentTab = AdminTabType.Reports;
            ValidateCurrentUserHasPermission(SystemPermissions.ReportsView);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.DatePicker.DateChanged += this.DatePicker_DateChanged;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                this.CurrentTab = AdminTabType.Reports;
                SetUserControlValues();
                FillList(this.DatePicker.SelectedDate);
            }
        }

        protected void btnLast_Click(System.Object sender, System.Web.UI.ImageClickEventArgs e)
        {
            this.DatePicker.SelectedDate = this.DatePicker.SelectedDate.AddDays(-1);
            FillList(this.DatePicker.SelectedDate);
        }

        protected void btnNext_Click(System.Object sender, System.Web.UI.ImageClickEventArgs e)
        {
            this.DatePicker.SelectedDate = this.DatePicker.SelectedDate.AddDays(1);
            FillList(this.DatePicker.SelectedDate);
        }

        protected void btnShow_Click(System.Object sender, System.Web.UI.ImageClickEventArgs e)
        {
            dgList.CurrentPageIndex = 0;
            FillList(this.DatePicker.SelectedDate);
        }

        void FillList(System.DateTime d)
        {
            msg.ClearMessage();

            try
            {                
                TotalGrand = 0;
                TotalSub = 0;
                TotalShip = 0;
                TotalTax = 0;                
                TotalDiscounts = 0;
                TotalShipDiscounts = 0;
                
                DateTime startDateUtc = TimeZoneInfo.ConvertTimeToUtc(MerchantTribe.Web.Dates.ZeroOutTime(this.DatePicker.SelectedDate), MTApp.CurrentStore.Settings.TimeZone);
                DateTime endDateUtc = TimeZoneInfo.ConvertTimeToUtc(MerchantTribe.Web.Dates.MaxOutTime(this.DatePicker.SelectedDate), MTApp.CurrentStore.Settings.TimeZone);
                int totalItems = 0;
                List<OrderTransaction> transactions = MTApp.OrderServices.Transactions.FindForReportByDateRange(startDateUtc, endDateUtc, MTApp.CurrentStore.Id, int.MaxValue, 1, ref totalItems);
                ProcessOrderPortions(transactions);                                           

                dgList.DataSource = transactions;
                dgList.DataBind();
                lblResponse.Text = totalItems + " Transactions Found";
            }

            catch (Exception Ex)
            {
                msg.ShowException(Ex);
            }

        }

        // Estimates the portion of other totals before report goes live
        private void ProcessOrderPortions(List<OrderTransaction> transactions)
        {
            if (transactions == null) return;

            List<string> orderIds = (from t in transactions select t.OrderId).Distinct().ToList();
            List<OrderSnapshot> orderSnaps = MTApp.OrderServices.Orders.FindManySnapshots(orderIds);

            foreach (OrderTransaction t in transactions)
            {
                OrderSnapshot snap = orderSnaps.Where(y => y.bvin == t.OrderId).FirstOrDefault();
                if (snap != null)
                {
                    decimal percentOfTotal = 0;
                    if (snap.TotalGrand > 0) percentOfTotal = t.AmountAppliedToOrder / snap.TotalGrand;

                    t.TempEstimatedHandlingPortion = Math.Round(snap.TotalHandling * percentOfTotal, 2, MidpointRounding.AwayFromZero);
                    t.TempEstimatedItemPortion = Math.Round(snap.TotalOrderBeforeDiscounts * percentOfTotal, 2, MidpointRounding.AwayFromZero);
                    t.TempEstimatedItemDiscount = Math.Round(snap.TotalOrderDiscounts * percentOfTotal, 2, MidpointRounding.AwayFromZero);
                    t.TempEstimatedShippingPortion = Math.Round(snap.TotalShippingBeforeDiscounts * percentOfTotal, 2, MidpointRounding.AwayFromZero);
                    t.TempEstimatedShippingDiscount = Math.Round(snap.TotalShippingDiscounts * percentOfTotal, 2, MidpointRounding.AwayFromZero);
                    t.TempEstimatedTaxPortion = Math.Round((snap.TotalTax + snap.TotalTax2) * percentOfTotal, 2, MidpointRounding.AwayFromZero);
                    t.TempCustomerEmail = snap.UserEmail;
                    t.TempCustomerName = snap.BillingAddress.LastName + ", " + snap.BillingAddress.FirstName;
                }
            }
        }

        void SetUserControlValues()
        {
            string sDate = "";
            if (Request.Params["date"] != null)
            {
                sDate = Request.Params["date"];
                this.DatePicker.SelectedDate = System.DateTime.Parse(sDate);
            }
            else
            {
                this.DatePicker.SelectedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MTApp.CurrentStore.Settings.TimeZone);
            }

        }

        protected void dgList_PageIndexChanged(object sender, DataGridPageChangedEventArgs e)
        {
            dgList.CurrentPageIndex = e.NewPageIndex;
            FillList(this.DatePicker.SelectedDate);
        }

        protected void dgList_Edit(object sender, DataGridCommandEventArgs e)
        {
            string orderID = (string)dgList.DataKeys[e.Item.ItemIndex];
            Response.Redirect("~/BVAdmin/Orders/ViewOrder.aspx?id=" + orderID, true);
        }

        protected void dgList_ItemDataBound(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
        {
            

            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                OrderTransaction t = (OrderTransaction)e.Item.DataItem;
                if (t == null) return;

                Literal litTimeStamp = (Literal)e.Item.FindControl("litTimeStamp");
                Literal litOrderNumber = (Literal)e.Item.FindControl("litOrderNumber");
                Literal litDescription = (Literal)e.Item.FindControl("litDescription");
                Literal litAmount = (Literal)e.Item.FindControl("litAmount");
                Literal litCustomerName = (Literal)e.Item.FindControl("litCustomerName");

                if (litTimeStamp != null)
                {
                    litTimeStamp.Text = TimeZoneInfo.ConvertTimeFromUtc(t.TimeStampUtc, MTApp.CurrentStore.Settings.TimeZone).ToShortTimeString();
                }

                if (litOrderNumber != null)
                {
                    litOrderNumber.Text = t.OrderNumber;
                }

                if (litDescription != null)
                {
                    litDescription.Text = MerchantTribe.Payment.EnumHelper.ActionTypeToString(t.Action);
                }

                if (litAmount != null)
                {
                    litAmount.Text = t.AmountAppliedToOrder.ToString("C");
                }

                if (litCustomerName != null)
                {
                    litCustomerName.Text = "<strong>" + t.TempCustomerName + "</strong><br /><span class=\"tiny\">" + t.TempCustomerEmail + "</span>";
                }
                this.TotalSub += t.TempEstimatedItemPortion;
                this.TotalDiscounts += t.TempEstimatedItemDiscount;
                this.TotalShip += t.TempEstimatedShippingPortion;
                this.TotalShipDiscounts += t.TempEstimatedShippingDiscount;
                this.TotalTax += t.TempEstimatedTaxPortion;
                this.TotalGrand += t.AmountAppliedToOrder;
            }
            else
            {
                if (e.Item.ItemType == ListItemType.Footer)
                {
                    e.Item.Cells[0].Text = "Totals:";

                    e.Item.Cells[2].Text = string.Format("{0:C}", TotalSub);
                    e.Item.Cells[3].Text = string.Format("{0:C}", TotalDiscounts);
                    e.Item.Cells[4].Text = string.Format("{0:C}", TotalShip);
                    e.Item.Cells[5].Text = string.Format("{0:C}", TotalShipDiscounts);
                    e.Item.Cells[6].Text = string.Format("{0:C}", TotalTax);
                    e.Item.Cells[7].Text = string.Format("{0:C}", TotalGrand);
                }
            }
        }

        protected void DatePicker_DateChanged(object sender, MerchantTribe.Commerce.Content.BVModuleEventArgs e)
        {
            FillList(this.DatePicker.SelectedDate);
        }
    }
}