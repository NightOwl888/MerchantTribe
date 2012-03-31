using System;
using System.Collections.ObjectModel;

namespace MerchantTribe.Commerce.Shipping
{

	public class ShippingRateDisplay
	{

		private string _DisplayName = string.Empty;
		private string _ProviderId = string.Empty;
		private string _ProviderServiceCode = string.Empty;
		private decimal _Rate = 0m;
		private string _ShippingMethodId = string.Empty;
        private Collection<Orders.OrderPackage> _SuggestedPackages = new Collection<Orders.OrderPackage>();
		private string _ResponseMessage = string.Empty;
		private decimal _potentialDiscount = 0m;

		public string DisplayName {
			get { return _DisplayName; }
			set { _DisplayName = value; }
		}
		public string ProviderId {
			get { return _ProviderId; }
			set { _ProviderId = value; }
		}
		public string ProviderServiceCode {
			get { return _ProviderServiceCode; }
			set { _ProviderServiceCode = value; }
		}
		public decimal Rate {
			get { return _Rate; }
			set { _Rate = value; }
		}
		public string RateAndNameForDisplay {
			get {
				string result = string.Empty;
				if (this.PotentialDiscount > 0) {
					result = "<span class=\"shippingdiscount\">" + string.Format("{0:c}", this.Rate) + "</span> " + string.Format("{0:c}", this.Rate - this.PotentialDiscount) + " - " + this.DisplayName;
				}
				else {
					result = string.Format("{0:c}", this.Rate) + " - " + this.DisplayName;
				}

				return result;
			}
		}
		public string ResponseMessage {
			get { return _ResponseMessage; }
			set { _ResponseMessage = value; }
		}
		public string ShippingMethodId {
			get { return _ShippingMethodId; }
			set { _ShippingMethodId = value; }
		}
		public Collection<Orders.OrderPackage> SuggestedPackages {
			get { return _SuggestedPackages; }
			set { _SuggestedPackages = value; }
		}
		public string UniqueKey {
			get {
				string result = this.ShippingMethodId + this.ProviderId + this.ProviderServiceCode;
				return result;
			}
		}
		public decimal PotentialDiscount {
			get { return _potentialDiscount; }
			set { _potentialDiscount = value; }
		}


        public ShippingRateDisplay AdjustRate(ShippingMethodAdjustmentType adjustmentType, decimal amount)
        {
            if (Rate != 0)
            {
                decimal adjustment = 0;
                switch (adjustmentType)
                {
                    case ShippingMethodAdjustmentType.None:
                        return this;
                    case ShippingMethodAdjustmentType.Amount:
                        adjustment = amount;
                        break;
                    case ShippingMethodAdjustmentType.Percentage:
                        adjustment = Math.Round(this.Rate * (amount / 100m), 2, MidpointRounding.AwayFromZero);
                        break;
                }
                this.Rate += adjustment;
            }
            return this;
        }

		public ShippingRateDisplay()
		{

		}

        public ShippingRateDisplay(MerchantTribe.Shipping.IShippingRate rate)
        {
            this._DisplayName = rate.DisplayName;
            this._Rate = rate.EstimatedCost;
            this._ProviderId = rate.ServiceId;
            this._ProviderServiceCode = rate.ServiceCodes;
        }

		public ShippingRateDisplay(string name, string shipProviderId, string shipProviderServiceCode, decimal totalRate, string shipMethodId)
		{
			_DisplayName = name;
			_ProviderId = shipProviderId;
			_ProviderServiceCode = shipProviderServiceCode;
			_Rate = totalRate;
			_ShippingMethodId = shipMethodId;
		}

		public ShippingRateDisplay(string name, string shipProviderId, string shipProviderServiceCode, decimal totalRate, string shipMethodId, Collection<Orders.OrderPackage> packages, string message)
		{
			_DisplayName = name;
			_ProviderId = shipProviderId;
			_ProviderServiceCode = shipProviderServiceCode;
			_Rate = totalRate;
			_ShippingMethodId = shipMethodId;
			_SuggestedPackages = packages;
			_ResponseMessage = message;
		}

		public Shipping.ShippingRateDisplay GetCopy()
		{
			Shipping.ShippingRateDisplay result = new Shipping.ShippingRateDisplay();

			result.DisplayName = this.DisplayName;
			result.ProviderId = this.ProviderId;
			result.ProviderServiceCode = this.ProviderServiceCode;
			result.Rate = this.Rate;
			result.ShippingMethodId = this.ShippingMethodId;
			result.ResponseMessage = this.ResponseMessage;
			result.SuggestedPackages = new Collection<Orders.OrderPackage>();

			foreach (Orders.OrderPackage item in this.SuggestedPackages) {
				result.SuggestedPackages.Add(item.Clone());
			}

			return result;
		}
	}
}
