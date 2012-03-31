using System;

namespace MerchantTribe.Commerce.Utilities
{
	
	public class Money
	{

		public static decimal ApplyDiscountPercent(decimal monetaryAmount, decimal percentage)
		{
            return Math.Round((monetaryAmount * ((100 - percentage) / 100)), 2, MidpointRounding.AwayFromZero);
		}

		public static decimal ApplyIncreasedPercent(decimal monetaryAmount, decimal percentage)
		{
            return Math.Round((monetaryAmount * ((100 + percentage) / 100)), 2, MidpointRounding.AwayFromZero);
		}

		public static decimal GetDiscountAmountByPercent(decimal monetaryAmount, decimal percentage)
		{
            return Math.Round((monetaryAmount * (percentage / 100)), 2, MidpointRounding.AwayFromZero);
		}

		//this may look silly, but it is here in case in the future we ever need to do any monetary conversions or rounding logic
		public static decimal GetDiscountAmount(decimal monetaryAmount, decimal amount)
		{
			return amount;
		}

		public static decimal ApplyDiscountAmount(decimal monetaryAmount, decimal discountAmount)
		{
			return (monetaryAmount - discountAmount);
		}

		public static decimal ApplyIncreasedAmount(decimal monetaryAmount, decimal increaseAmount)
		{
			return (monetaryAmount + increaseAmount);
		}

		public static string FormatMonetaryAmountForDisplay(decimal monetaryAmount)
		{
			return monetaryAmount.ToString("c");
		}
	}
}
