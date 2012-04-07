using System;

namespace MerchantTribe.Commerce.Utilities
{
	public class EnumToString
	{

		public static string OrderShippingStatus(Orders.OrderShippingStatus e)
		{
			string result = string.Empty;

			switch (e) {
				case Orders.OrderShippingStatus.FullyShipped:
					result = "Shipped";
                    break;
				case Orders.OrderShippingStatus.NonShipping:
					result = "Non Shipping";
                    break;
				case Orders.OrderShippingStatus.PartiallyShipped:
					result = "Partially Shipped";
                    break;
				case Orders.OrderShippingStatus.Unknown:
					result = "Unknown";
                    break;
				case Orders.OrderShippingStatus.Unshipped:
                    result = "Unshipped"; 
                    break;
			}

			return result;
		}

		public static string OrderPaymentStatus(Orders.OrderPaymentStatus e)
		{
			string result = string.Empty;

			switch (e) {
				case Orders.OrderPaymentStatus.Overpaid:
					result = "Overpaid";
                    break;
				case Orders.OrderPaymentStatus.Paid:
					result = "Paid";
                    break;
				case Orders.OrderPaymentStatus.PartiallyPaid:
					result = "Partially Paid";
                    break;
				case Orders.OrderPaymentStatus.Unknown:
					result = "Unknown";
                    break;
				case Orders.OrderPaymentStatus.Unpaid:
					result = "Unpaid";
                    break;
			}

			return result;
		}

		public static string ProductInventoryStatus(Catalog.ProductInventoryStatus pi)
		{
			string result = "unknown";

			switch (pi) {
				case Catalog.ProductInventoryStatus.Available:
					result = "Available";
                    break;
				case Catalog.ProductInventoryStatus.NotAvailable:
					result = "Not Available";
                    break;
				case Catalog.ProductInventoryStatus.NotSet:
					result = "Not Set";
                    break;
			}

			return result;
		}

		public static string Versions(Versions v)
		{
			string result = "unknown";

			switch (v) {
				case Utilities.Versions.FreshInstall:
					result = "Fresh Install";
                    break;
				case Utilities.Versions.NotSet:
					result = "Not Set";
                    break;
				case Utilities.Versions.SP1:
					result = "SP1";
                    break;
				case Utilities.Versions.SP2:
					result = "SP2";
                    break;
				case Utilities.Versions.SP3:
                    result = "SP3"; 
                    break;
				case Utilities.Versions.SP31:
					result = "SP3.1";
                    break;
				case Utilities.Versions.SP32:
					result = "SP3.2";
                    break;
				case Utilities.Versions.SP4:
					result = "SP4";
                    break;
				case Utilities.Versions.SP5:
					result = "SP5";
                    break;
                case Utilities.Versions.SP6:
                    result = "SP6";
                    break;
			}

			return result;
		}

        public static string MidpointRoundingModes(MidpointRounding m)
        {
            string result = "unknown";

            switch (m)
            {
                case MidpointRounding.ToEven:
                    result = "Toward Nearest Even Number (Example: 2.5 rounds to 2)";
                    break;
                case MidpointRounding.AwayFromZero:
                    result = "Away From Zero (Example: 2.5 rounds to 3)";
                    break;
            }

            return result;
        }
	}
}
