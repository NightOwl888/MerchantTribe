using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerchantTribe.Commerce.Metrics
{
    public class GoogleAnalytics
    {

        private static string GoogleSafeString(string input)
        {
            string result = input.Replace("'", " ");
            return result;
        }
        public static string RenderLatestTracker(string googleId)
        {
            return RenderLatestTrackerAndTransaction(googleId, null, string.Empty, string.Empty);
        }
        public static string RenderLatestTrackerAndTransaction(string googleId, Orders.Order o, string storeName, string categoryName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">\n");
            sb.Append("var _gaq = _gaq || [];\n");
            sb.Append("_gaq.push(['_setAccount', '" + googleId + "']);\n");
            sb.Append("_gaq.push(['_trackPageview']);\n");

            if (o != null)
            {
                sb.Append("_gaq.push(['_addTrans',\n");
                sb.Append("'" + GoogleSafeString(o.OrderNumber) + "',\n");           // order ID - required
                sb.Append("'" + GoogleSafeString(storeName) + "',\n");  // affiliation or store name
                sb.Append("'" + o.TotalGrand + "',\n");          // total - required
                sb.Append("'" + o.TotalTax + "',\n");           // tax
                sb.Append("'" + o.TotalShippingAfterDiscounts + "',\n");              // shipping
                sb.Append("'" + GoogleSafeString(o.ShippingAddress.City) + "',\n");       // city
                sb.Append("'" + GoogleSafeString(o.ShippingAddress.RegionName) + "',\n");     // state or province
                sb.Append("'" + GoogleSafeString(o.ShippingAddress.CountyName) + "'\n");             // country
                sb.Append("]);\n");

                foreach(Orders.LineItem li in o.Items)
                {
                    // add item might be called for every item in the shopping cart
                    // where your ecommerce engine loops through each item in the cart and
                    // prints out _addItem for each
                    sb.Append("_gaq.push(['_addItem',\n");
                    sb.Append("'" + GoogleSafeString(o.OrderNumber) + "',\n");           // order ID - required
                    sb.Append("'" + GoogleSafeString(li.ProductSku) + "',\n");           // SKU/code - required
                    sb.Append("'" + GoogleSafeString(li.ProductName) + "',\n");        // product name
                    sb.Append("'" + GoogleSafeString(categoryName) + "',\n");   // category or variation
                    sb.Append("'" + li.AdjustedPricePerItem + "',\n");          // unit price - required
                    sb.Append("'" + li.Quantity + "'\n");               // quantity - required
                    sb.Append("]);\n");
                }
   
                sb.Append("_gaq.push(['_trackTrans']);\n"); //submits transaction to the Analytics servers
            }

            sb.Append("(function() {\n");
            sb.Append("var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;\n");
            sb.Append("ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';\n");
            sb.Append("var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);\n");
            sb.Append("})();\n");
            sb.Append("</script>\n");

            return sb.ToString();
        }

        public static string RenderGoogleAdwordTracker(decimal orderValue, string conversionId, string conversionLabel, string backgroundColor, bool https)
        {
            StringBuilder sb = new StringBuilder();

            string total = Math.Round(orderValue, 2, MidpointRounding.AwayFromZero).ToString();

            sb.Append("<!-- Google Code for purchase Conversion Page -->" + System.Environment.NewLine);
            sb.Append("<script type=\"text/javascript\">" + System.Environment.NewLine);
            sb.Append("/* <!CDATA[ */" + System.Environment.NewLine);
            sb.Append("var google_conversion_id = " + conversionId + ";" + System.Environment.NewLine);
            sb.Append("var google_conversion_language = \"en_US\";" + System.Environment.NewLine);
            sb.Append("var google_conversion_format = \"1\";" + System.Environment.NewLine);
            sb.Append("var google_conversion_color = \"" + backgroundColor + "\";" + System.Environment.NewLine);
            sb.Append("var google_conversion_label = \"" + conversionLabel + "\";" + System.Environment.NewLine);
            sb.Append("if (" + total + ") {" + System.Environment.NewLine);
            sb.Append("  var google_conversion_value = " + total + ";" + System.Environment.NewLine);
            sb.Append("}" + System.Environment.NewLine);
            sb.Append("/* ]]> */" + System.Environment.NewLine);
            sb.Append("</script>" + System.Environment.NewLine);

            sb.Append("<script type=\"text/javascript\" src=\"https://www.googleadservices.com/pagead/conversion.js\">" + System.Environment.NewLine);
            sb.Append("</script>" + System.Environment.NewLine);
            sb.Append("<noscript>" + System.Environment.NewLine);
            sb.Append("<img height=\"1\" width=\"1\" border=\"0\" src=\"");
            if (https)
            {
                sb.Append("https://");
            }
            else
            {
                sb.Append("http://");
            }
            sb.Append("www.googleadservices.com/pagead/conversion/" + conversionId + "/imp.gif?value=" + total + "&label=" + conversionLabel + "&script=0\">" + System.Environment.NewLine);
            sb.Append("</noscript>" + System.Environment.NewLine);

            return sb.ToString();
        }        
    }
}
