using System;
using System.Data;
using System.Web.UI.WebControls;
using MerchantTribe.Commerce.Membership;
using MerchantTribe.Commerce.Metrics;
using System.Collections.Generic;
using System.Linq;

namespace MerchantTribeStore
{

    partial class BVAdmin_Reports_SearchKeywords : BaseAdminPage
    {

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                RunReport();
            }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            this.PageTitle = "Keyword Searches";
            this.CurrentTab = AdminTabType.Reports;
            ValidateCurrentUserHasPermission(SystemPermissions.ReportsView);
        }

        private void RunReport()
        {

            List<MerchantTribe.Commerce.Metrics.SearchQueryRepository.SearchQueryData> reportData = MTApp.MetricsSerices.SearchQueries.FindQueryCountReport();

            AddPercentages(ref reportData);

            this.GridView1.DataSource = reportData;
            this.GridView1.DataBind();
        }

        private void AddPercentages(ref List<MerchantTribe.Commerce.Metrics.SearchQueryRepository.SearchQueryData> data)
        {
            if (data != null)
            {
                int totalSearches = data.Sum(y => y.Count);
                foreach (MerchantTribe.Commerce.Metrics.SearchQueryRepository.SearchQueryData q in data)
                {
                    if (totalSearches > 0)
                    {
                        decimal percent = ((decimal)q.Count / (decimal)totalSearches) * 100m;
                        q.Percentage = Math.Round(percent, 2, MTApp.CurrentStore.Settings.MidpointRoundingMode);
                    }                    
                }
            }
        }

        protected void GridView1_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                decimal percent = decimal.Parse(e.Row.Cells[2].Text.TrimEnd("%".ToCharArray()));
                //= e.Row.DataItem Eval("QueryCount")
                System.Web.UI.WebControls.Image imgBar;
                imgBar = (Image)e.Row.FindControl("imgBar");
                if (imgBar != null)
                {
                    imgBar.AlternateText = percent.ToString() + "%";
                    int w = (int)Math.Floor(percent) * 3;
                    if (w < 1)
                    {
                        w = 1;
                    }
                    imgBar.Width = w;
                }
            }
        }

        protected void btnReset_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            MTApp.MetricsSerices.SearchQueries.DeleteAll();
            RunReport();
        }

    }
}