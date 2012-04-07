using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using MerchantTribe.Commerce.Payment;

namespace MerchantTribe.Commerce.Membership
{
    public class CustomerPointsManager
    {
        private string _connectionString = string.Empty;
        private int _pointsIssuedPerDollarSpent = 1;
        private int _pointsNeededForOneDollarCredit = 100;
        private long _storeId = 0;

        private IRewardPointsProvider _provider = null;

        public static CustomerPointsManager InstantiateForMemory(int pointsIssuedPerDollar, int pointsNeededForDollarCredit, long storeId)
        {
            return new CustomerPointsManager(pointsIssuedPerDollar, pointsNeededForDollarCredit, storeId, new RewardPointsProviderMemory());
        }
        public static CustomerPointsManager InstantiateForDatabase(int pointsIssuedPerDollar, int pointsNeededForDollarCredit, long storeId)
        {
            return new CustomerPointsManager(pointsIssuedPerDollar, pointsNeededForDollarCredit, storeId, new RewardPointsProviderDatabase());
        }
        public CustomerPointsManager(int pointsIssuedPerDollar, int pointsNeededForDollarCredit, long storeId, IRewardPointsProvider provider)
        {
            _provider = provider;
            _storeId = storeId;
            this._pointsIssuedPerDollarSpent = pointsIssuedPerDollar;
            this._pointsNeededForOneDollarCredit = pointsNeededForDollarCredit;
        }

        public decimal DollarCreditForPoints(int points)
        {
            decimal result = 0;

            result = (decimal)points / (decimal)_pointsNeededForOneDollarCredit;
            result = Math.Round(result, 2, MerchantTribeApplication.Current.CurrentStore.Settings.MidpointRoundingMode);
            
            return result;
        }
        public int PointsNeededForPurchaseAmount(decimal purchaseAmount)
        {
            int result = int.MaxValue;

            decimal r1 = purchaseAmount * (decimal)_pointsNeededForOneDollarCredit;
            result = (int)Math.Ceiling(r1);

            return result;
        }
        public int PointsToIssueForSpend(decimal spend)
        {
            int result = 0;

            decimal spendRounded = Math.Floor(spend);
            decimal points = spendRounded * _pointsIssuedPerDollarSpent;

            result = (int)points;

            return result;
        }

        public int TotalPointsIssuedForStore(long storeId)
        {
            return _provider.RewardPointsTotalIssued(storeId);            
        }
        public int TotalPointsReservedForStore(long storeId)
        {
            return _provider.RewardPointsReservedForStore(storeId);
        }

        public int FindAvailablePoints(string userId)
        {
            return _provider.RewardPointsAvailableForUser(userId, _storeId);
        }
        public int FindReserverdPoints(string userId)
        {
            return _provider.RewardPointsHeldForUser(userId, _storeId);
        }

        public bool CapturePoints(string userId, int points)
        {
            bool result = false;

            if (_provider.CapturePoints(userId, _storeId, points) == 1)
            {
                result = true;
            }
                        
            return result;
        }
        public bool DecreasePoints(string userId, int points)
        {
            bool result = false;

            if (_provider.DecreasePoints(userId, _storeId, points) == 1)
            {
                result = true;
            }
            
            return result;
        }
        public bool HoldPoints(string userId, int points)
        {
            bool result = true;

            if (_provider.HoldPoints(userId,  _storeId, points) == 1)
            {
                result = true;
            }

            return result;
        }
        public bool UnHoldPoints(string userId, int points)
        {
            bool result = true;

            if (_provider.UnHoldPoints(userId, _storeId, points) == 1)
            {
                result = true;
            }

            return result;
        }
        public bool IssuePoints(string userId, int points)
        {
            bool result = false;

            if (_provider.IssuePoints(userId, _storeId, points) == 1)
            {
                result = true;
            }

            return result;
        }
    }
}
