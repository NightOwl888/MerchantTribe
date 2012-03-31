using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PayPal.Payments.Common;
using PayPal.Payments.Common.Utility;
using PayPal.Payments.DataObjects;
using PayPal.Payments.Transactions;


namespace MerchantTribe.Payment.Methods
{
    public class PayFlowPro: Method
    {
        private const string versionNumber= "4.3.1";
        private const string LiveUrl = "payflowpro.verisign.com";
        private const string TestUrl = "pilot-payflowpro.verisign.com";

        public override string Name
        {
            get { return "Payflow Pro"; }
        }

        public override string Id
        {
            get { return "6EF0F678-9C67-4ade-A95D-9E8DE2C7780B"; }
        }

        public PayFlowProSettings Settings { get; set; }

        public PayFlowPro()
        {
            Settings = new PayFlowProSettings();
        }

        public override void ProcessTransaction(Transaction t)
        {
            bool result = false;

            //'Get Configuration Settings
            string MerchantPartner = Settings.MerchantPartner;
            string MerchantLogin = Settings.MerchantLogin;
            string MerchantUser = Settings.MerchantUser;
            string MerchantPassword = Settings.MerchantPassword;
            string CurrencyCode = Settings.CurrencyCode;
            bool TestMode = Settings.TestMode;
            bool DebugMode = Settings.DeveloperMode;

            UserInfo User = new UserInfo(MerchantUser, MerchantLogin, MerchantPartner, MerchantPassword);
        

            //Set HostAddress URL
            string HostAddress = LiveUrl;
            if (TestMode) HostAddress = TestUrl;

            //Connection Info
            PayflowConnectionData Connection = new PayflowConnectionData(HostAddress, 443, 45); //', CertLocation)

            // *** Create a new Invoice data object ***
            // Set Invoice object with the Amount, Billing & Shipping Address, etc. ***
            Invoice Inv = new Invoice();

            // Set the amount object. A valid amount is a two decimal value.  An invalid amount will generate a result code                
            Currency Amt = new Currency(Decimal.Round(t.Amount, 2, MidpointRounding.AwayFromZero), CurrencyCode); //' 840 is US ISO currency code.  If no code passed, 840 is default.
            Inv.Amt = Amt;

            // Generate a unique transaction ID
            string strRequestID = PayflowUtility.RequestId + t.MerchantInvoiceNumber;

            //InvNum and CustRef are sent to the processors and could show up on a customers
            // or your bank statement. These fields are reportable but not searchable in PayPal Manager.

            Inv.InvNum = t.MerchantInvoiceNumber;
            Inv.CustRef = t.Customer.Email;

            //' Comment1 and Comment2 fields are searchable within PayPal Manager .
            Inv.Comment1 = "Order Number: " + Inv.InvNum;
            Inv.Comment2 = "Customer Email: " + t.Customer.Email;

            // Create the BillTo object.
            BillTo Bill = new BillTo();

            Bill.FirstName = t.Customer.FirstName;
            Bill.LastName = t.Customer.LastName;
            Bill.Street = t.Customer.Street;
            Bill.City = t.Customer.City;
            Bill.Zip = t.Customer.PostalCode;
            Bill.PhoneNum = t.Customer.Phone;
            Bill.Email = t.Customer.Email;
            Bill.State = t.Customer.Region;                        

            //' BillToCountry code is based on numeric ISO country codes. (e.g. 840 = USA)
            //Get Country Code
            string CountryCode = MerchantTribe.Web.Geography.Country.FindByName(t.Customer.Country).IsoNumeric;
            Bill.BillToCountry = CountryCode;
        
            // Set the BillTo object into invoice.
            Inv.BillTo = Bill;

            CustomerInfo CustInfo = new CustomerInfo();

            CustInfo.CustId = t.Customer.Email;
            CustInfo.CustIP = t.Customer.IpAddress;

            // *** Create a new Payment Device - Credit Card data object. ***
            // Note: Expiration date is in the format MMYY
            string CCExpDate = t.Card.ExpirationMonthPadded + t.Card.ExpirationYearTwoDigits;

            CreditCard CC = new CreditCard(t.Card.CardNumber, CCExpDate);

            //' *** Card Security Code ***
            //' This is the 3 or 4 digit code on either side of the Credit Card.
            if (t.Card.SecurityCode.Length > 0)
            {
                CC.Cvv2 = t.Card.SecurityCode;
            }

            // Name on Credit Card is optional.
            CC.Name = t.Card.CardHolderName;

            // *** Create a new Tender - Card Tender data object. ***
            CardTender Card = new CardTender(CC);

            // *** Create a new Transaction. ***
            // The Request Id is the unique id necessary for each transaction. 
            // If you pass a non-unique request id, you will receive the transaction details from the original request.

            //'DO TRANSACTION
            try
            {
                BaseTransaction Trans = null;

                switch(t.Action)
                {
                    case ActionType.CreditCardHold:
                        Trans = new AuthorizationTransaction(User,Connection, Inv, Card, strRequestID);
                        break;
                    case ActionType.CreditCardCharge:
                        Trans = new SaleTransaction(User,Connection, Inv, Card, strRequestID);
                        break;
                    case ActionType.CreditCardCapture:
                        Trans = new CaptureTransaction(t.PreviousTransactionNumber, User, Connection, Inv, strRequestID);
                        break;
                    case ActionType.CreditCardRefund:
                        Trans = new CreditTransaction(t.PreviousTransactionNumber, User, Connection, Inv, strRequestID);
                        break;
                    case ActionType.CreditCardVoid:
                       Trans = new VoidTransaction(t.PreviousTransactionNumber, User, Connection, strRequestID);
                        break;
                }

                int Result = 0;
                System.Globalization.CultureInfo currCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                Response Resp = null;

                try
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                    //' Submit the Transaction
                    Resp = Trans.SubmitTransaction();
                }
                finally
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = currCulture;
                }

                if (Resp != null)
                {
            
                    // Get the Transaction Response parameters.
                    TransactionResponse TrxnResponse = Resp.TransactionResponse;
                                
                    // RESULT codes returns.
                    if (TrxnResponse != null)
                    {

                        //Check for Approval (0 = approved, all else = Decline or Error)
                        Result = TrxnResponse.Result;
                        string RespMsg = TrxnResponse.RespMsg;

                        //if success then save our reference number
                        if (Result == 0)
                        {
                            t.Result.ReferenceNumber = TrxnResponse.Pnref;
                        }

                        t.Result.ResponseCode = TrxnResponse.AuthCode;

                        //Custom Properties
                        t.Result.AvsCode = AvsResponseType.Unavailable;
                        t.Result.AvsCodeDescription = "AVSADDR: " + TrxnResponse.AVSAddr + " AVSZIP: " + TrxnResponse.AVSZip + " IAVS: " + TrxnResponse.IAVS;
                        t.Result.ResponseCode = TrxnResponse.Result.ToString();
                        t.Result.CvvCode = CvnResponseType.Unavailable;
                        t.Result.CvvCodeDescription = TrxnResponse.CVV2Match;
                        t.Result.ResponseCodeDescription = TrxnResponse.RespMsg;
                        
                    }
                    else
                    {
                        t.Result.Messages.Add(new Message("Payment Error: Transaction Response is Null", "BVP_PFP_1003", MessageType.Error));
                    }

                    //Show Complete Response if Debug Mode
                    if (DebugMode)
                    {
                        t.Result.Messages.Add(new Message("PayflowPro Request:" + Trans.Request, "REQ", MessageType.Information));
                        t.Result.Messages.Add(new Message("PayflowPro Request:" + Trans.Response.ResponseString, "RES", MessageType.Information));

                        // Get the Transaction Context
                        Context TransCtx = Resp.TransactionContext;

                        if ((TransCtx != null) && (TransCtx.getErrorCount() > 0))
                        {
                            t.Result.Messages.Add(new Message("PayflowPro Context:" + TransCtx.ToString(), "CTX", MessageType.Information));
                        }
                    }

                    if (Result == 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    t.Result.Messages.Add(new Message("Payment Error: Response is Null", "BVP_PFP_1002", MessageType.Error));
                }
            }
            catch(Exception ex)
            {
                result = false;
                t.Result.Messages.Add(new Message("Payment Error: " + ex.Message, "BVP_PFP_1001", MessageType.Error));
                t.Result.Messages.Add(new Message("Stack Trace " + ex.StackTrace, "STACKTRACE", MessageType.Error));
            }
            
            t.Result.Succeeded = result;                     
        }
                
        public override MethodSettings BaseSettings
        {
            get { return Settings; }
        }

      
    }
}
