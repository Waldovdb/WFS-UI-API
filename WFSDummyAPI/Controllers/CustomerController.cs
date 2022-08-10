using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WFS_UI_API.Service;
using System.Threading.Tasks;
using System.Dynamic;
using System.Linq;

namespace WFS_UI_API.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {

        private DataService _dataService { get; set; }

        public CustomerController(DataService inDataService)
        {
            _dataService = inDataService;
        }

        [HttpGet]
        [Route("getaccounts")]
        public async Task<IActionResult> GetAccountsByID(string CustomerID)
        {
            try
            {
                #region [ Deprecated Dummy Data ]
                //var toReturn = new List<KVPair>();
                //toReturn.Add(new KVPair("Credit Card", "12345"));
                //toReturn.Add(new KVPair("Store Card", "54321"));
                #endregion
                #region [ Build and execute Query ]
                string query = @"EXEC [API].[spFetchCustomerAccounts] @INCUSTOMERID";
                var toReturn = await _dataService.SelectMany<KVPair, dynamic>(query, new { INCUSTOMERID = CustomerID }, "UI");
                #endregion
                return Ok(toReturn);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getbanking")]
        public async Task<IActionResult> GetBankAccounts(string CustomerID)
        {
            try
            {
                #region [ Build Queries ]
                string Query = $"EXEC [API].[spFetchCustomerBankAccounts] @InCustomerID";
                #endregion
                var toReturn = new List<BankCollection>();
                #region [ Get Account Values ]
                var dynamicObj = await _dataService.SelectMany<dynamic, dynamic>(Query, new { InCustomerID = CustomerID }, "UI");
                if(dynamicObj != null)
                {
                    if(dynamicObj.Count > 0)
                    {
                        foreach(var item in dynamicObj)
                        {
                            var toAdd = new BankCollection
                            {
                                ddlPaymentBankAccountType = new KVCollDDLResult() { RepeatingControl = "ddlPaymentBankAccountType", Values = new KVPair(item.BankAccountType.ToString(), item.BankAccountType.ToString()) },
                                ddlPaymentBankName = new KVCollDDLResult() { RepeatingControl = "ddlPaymentBankName", Values = new KVPair(item.BankName.ToString(), item.BankName.ToString()) },
                                txtPaymentBankAccountNumber = item.BankAccountNumber.ToString(),
                                txtPaymentBankBranch = "South Africa",
                                txtPaymentBankCode = item.BankBranchCode.ToString()
                            };
                            toReturn.Add(toAdd);
                        }
                    }
                }
                #endregion
                return Ok(toReturn);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("customerattributes")]
        public async Task<IActionResult> GetCustomerAttributes(string IDNumber)
        {
            try
            {
                #region [ Create Query ]
                string query = @"EXEC [API].[spFetchCustomerAttributes] @INIDNUMBER";
                #endregion
                #region [ Execute Query ]
                List<KVPair> outList = await _dataService.SelectMany<KVPair, dynamic>(query, new { INIDNUMBER = IDNumber }, "UI");
                #endregion
                return (outList == null) ? Ok(new List<KVPair>()) : Ok(outList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("accountattributes")]
        public async Task<IActionResult> GetAccountAttributes(string AccountNo)
        {
            try
            {
                #region [ Create Query ]
                string query = @"EXEC [API].[spFetchAccountAttributes] @INACCOUNTNUMBER";
                #endregion
                #region [ Execute Query ]
                List<KVPair> outList = await _dataService.SelectMany<KVPair, dynamic>(query, new { INACCOUNTNUMBER = AccountNo }, "UI");
                #endregion
                return (outList == null) ? Ok(new List<KVPair>()) : Ok(outList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("customstoredproc")]
        public async Task<IActionResult> GetCustomStoredProc(string procName, string input)
        {
            try
            {
                KVCollDDL returnColl = new();
                #region [ Declare Query ]
                string query = $"EXEC [API].[{procName}] @DYNAMICINPUT";
                #endregion
                #region [ Execute Query ]
                var dynamicResult = await _dataService.SelectMany<dynamic, dynamic>(query, new { DYNAMICINPUT = input }, "UI");
                foreach (IDictionary<string, object> row in dynamicResult)
                {
                    foreach (var pair in row)
                    {
                        if(pair.Key.ToString() == "RepeatingControl")
                        {
                            returnColl.RepeatingControl = pair.Value.ToString();
                        }
                        else
                        {
                            returnColl.Values.Add(new KVPair(pair.Key, pair.Value.ToString()));
                        }
                    }
                }
                #endregion
                return (returnColl.RepeatingControl == String.Empty) ? BadRequest("No Repeating Control") : Ok(returnColl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getaccount")]
        public async Task<IActionResult> GetAccount(string accountno)
        {
            try
            {
                #region [ dynamic object stuff ]
                var dataObject = new ExpandoObject() as IDictionary<string, object>;
                #endregion

                List<KVPair> returnColl = new();
                #region [ Declare Query ]
                string query = $"EXEC [API].[spFetchCustomerAccount] @INACCOUNTNO";
                #endregion
                #region [ Execute Query ]
                var dynamicResult = await _dataService.SelectMany<dynamic, dynamic>(query, new { INACCOUNTNO = accountno }, "UI");
                #endregion
                foreach(var item in dynamicResult[0])
                {
                    dataObject.Add(item.Key.ToString(), item.Value);
                }
                #region [ Get Mappings ]
                string queryMap = $"EXEC [API].[spGetAttributeMap]";
                var dynamicMapping = await _dataService.SelectMany<dynamic, dynamic>(queryMap, new {  }, "UI");
                Dictionary<string,string> map = new();
                foreach(var pair in dynamicMapping)
                {
                    map.Add(pair.AttributeName, pair.AgilePointName);
                }
                #endregion
                #region [ Get Account Attributes ]
                string queryAttributes = $"EXEC [API].[spGetAccountAttributes] @INACCOUNTNO";
                var dynamicAttributes = await _dataService.SelectMany<dynamic, dynamic>(queryAttributes, new { INACCOUNTNO = accountno }, "UI");
                if(dynamicAttributes.Count > 0)
                {
                    foreach (var item in dynamicAttributes)
                    {
                        var tempObj = Newtonsoft.Json.JsonConvert.DeserializeObject(item.ValueString);
                        foreach(var pair in tempObj)
                        {
                            if (map.ContainsKey(pair.Name))
                            {
                                string ConvertedName = map[pair.Name];
                                dataObject.Add(ConvertedName, pair.Value.ToString().Replace(",","."));
                                //var temp = pair;
                                
                                //dynamicResult.Add(temp);
                            }
                            //else+
                            //{
                            //    dynamicResult[0].Add(pair);
                            //}
                        }
                    }
                }

                string queryAGPEvent = $"EXEC [API].[spGetAdditionalAttributes] @INACCOUNTNO";
                var dynamicAGPEvent = await _dataService.SelectMany<dynamic, dynamic>(queryAGPEvent, new { INACCOUNTNO = accountno }, "UI");
                if (dynamicAGPEvent.Count > 0)
                {
                    foreach (var itemNew in dynamicAGPEvent)
                    {
                        var checknew = itemNew.ValueString;
                        var tempObjNew = Newtonsoft.Json.JsonConvert.DeserializeObject(itemNew.ValueString);
                        foreach (var pairNew in tempObjNew)
                        {
                            if (map.ContainsKey(pairNew.Name))
                            {
                                string ConvertedName = map[pairNew.Name];
                                dataObject.Add(ConvertedName, pairNew.Value.ToString());
                                //var temp = pair;

                                //dynamicResult.Add(temp);
                            }
                            else
                            {
                                dataObject.Add(pairNew.Name.ToString(), pairNew.Value.ToString());
                            }
                        }
                    }
                }
                #endregion

                List<IDictionary<string, object>> outList = new List<IDictionary<string, object>>();
                outList.Add(dataObject);
                return Ok(outList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getbyid")]
        public async Task<IActionResult> GetCustomerByID(string IDNumber)
        {
            try
            {
                var PacketReturn = new Packet();

                #region [ Database dip - fetch customer details ]
                var tempObj1 = await _dataService.SelectMany<dynamic, dynamic>("EXEC [API].[spFetchCustomerDetails] @INPUTIDNUMBER", new { INPUTIDNUMBER = IDNumber }, "UI");
                foreach(IDictionary<string, object> row in tempObj1)
                {
                    foreach(var pair in row)
                    {
                        if(pair.Value != null)
                        {
                            PacketReturn.kVPairs.Add(new KVPair(pair.Key, pair.Value.ToString()));
                        }
                        else
                        {
                            PacketReturn.kVPairs.Add(new KVPair(pair.Key, String.Empty));
                        }
                    }
                }
                #endregion

                #region [ Get Mappings ]
                string queryMap = $"EXEC [API].[spGetAttributeMap]";
                var dynamicMapping = await _dataService.SelectMany<dynamic, dynamic>(queryMap, new { }, "UI");
                Dictionary<string, string> map = new();
                foreach (var pair in dynamicMapping.Where(o => o.Input == "sfAccount"))
                {
                    map.Add(pair.AttributeName, pair.AgilePointName);
                }
                #endregion
                #region [ Database dip - fetch account details ]
                var tempObj2 = await _dataService.SelectMany<dynamic, dynamic>("EXEC [API].[spFetchCustomerAccountDetails] @INPUTIDNUMBER", new { INPUTIDNUMBER = IDNumber }, "UI");
                foreach (IDictionary<string, object> row in tempObj2)
                {
                    NestedCollection tempAccNested = new()
                    {
                        RepeatingControl = "sfAccount"
                    };
                    KVPairListNested tempAccountDetails = new();
                    string AccNo = row["txtAccountNumber"].ToString();
                    var tempObjPTP = await _dataService.SelectMany<dynamic, dynamic>("EXEC [API].[spFetchCustomerAccountPTPs] @INPUTACCOUNTNO", new { INPUTACCOUNTNO = AccNo }, "UI");
                    var tempAccList = new List<KVPair>();
                    foreach (var pair in row)
                    {
                        if (pair.Value != null)
                        {
                            tempAccList.Add(new KVPair(pair.Key, pair.Value.ToString()));
                        }
                        else
                        {
                            tempAccList.Add(new KVPair(pair.Key, String.Empty));
                        }
                    }
                    // HERE 

                    string queryAttributes = $"EXEC [API].[spGetAccountAttributes] @INACCOUNTNO";
                    var dynamicAttributes = await _dataService.SelectMany<dynamic, dynamic>(queryAttributes, new { INACCOUNTNO = AccNo }, "UI");
                    if (dynamicAttributes.Count > 0)
                    {
                        foreach (var item in dynamicAttributes)
                        {
                            var checknew = item.ValueString;
                            var tempObjNew = Newtonsoft.Json.JsonConvert.DeserializeObject(item.ValueString);
                            foreach (var pair in tempObjNew)
                            {
                                if (map.ContainsKey(pair.Name))
                                {
                                    string ConvertedName = map[pair.Name];
                                    tempAccList.Add(new KVPair(ConvertedName, pair.Value.ToString()));
                                    //var temp = pair;

                                    //dynamicResult.Add(temp);
                                }
                                else
                                {
                                    //tempAccList.Add(new KVPair(pair.Name, pair.Value.ToString()));
                                }
                            }
                        }
                    }
                    // HERE
                    tempAccountDetails.Values = tempAccList;
                    KVColl tempAccountPTPs = new();
                    tempAccountPTPs.RepeatingControl = "sfAccountPTP";
                    foreach (IDictionary<string, object> ptp in tempObjPTP)
                    {
                        List<KVPair> tempPTPList = new();
                        foreach (var pair in ptp)
                        {
                            if (pair.Value != null)
                            {
                                tempPTPList.Add(new KVPair(pair.Key, pair.Value.ToString()));
                            }
                            else
                            {
                                tempPTPList.Add(new KVPair(pair.Key, String.Empty));
                            }
                        }
                        tempAccountPTPs.Values.Add(tempPTPList);
                    }
                    tempAccountDetails.NestedColl = tempAccountPTPs;
                    tempAccNested.KVNested = new List<KVPairListNested>
                    {
                        tempAccountDetails
                    };
                    PacketReturn.kVColls.Add(tempAccNested);
                }
                #endregion
                #region [ Deprecated = Populate First Account Details ]
                //var tempList = new List<KVPair>();
                //tempList.Add(new KVPair("txtAccountNumber", "12345"));
                //tempList.Add(new KVPair("txtAccountProductType", "Credit Card"));
                //tempList.Add(new KVPair("nbxAccountBalance", "1024.23"));
                //tempList.Add(new KVPair("nbxAccountBalance ", "1024.23"));
                //tempList.Add(new KVPair("nbxAccountTotalDue", "1000.2"));
                //tempList.Add(new KVPair("nbxAccountAmountInArrears", "24.03"));
                //tempList.Add(new KVPair("txtAccountPaymentArrangement", "No"));
                //tempList.Add(new KVPair("txtAccountForbearance", ""));
                //tempList.Add(new KVPair("txtAccountPTPPlanStatus", "Broken"));
                //tempList.Add(new KVPair("txtAccountActiveInsurance", "Yes"));
                //tempList.Add(new KVPair("txtAccountCreationDate", "13/2/2020"));
                //tempList.Add(new KVPair("txtAccountUpdatedDate", "7/2/2022"));
                //tempList.Add(new KVPair("txtAccountStatus", "Open"));
                //tempList.Add(new KVPair("nbxAccountCurrentDue", "24.03"));
                //tempList.Add(new KVPair("txtAccountDueDate", "25/2/2022"));
                //tempList.Add(new KVPair("txtAccountDelinquencyCycle", "2"));
                //tempList.Add(new KVPair("txtAccountExistingPTP", "No"));
                //tempList.Add(new KVPair("txtAccountGraceDate", "No"));
                //tempList.Add(new KVPair("txtAccountInsuranceOption", ""));
                //tempList.Add(new KVPair("nbxAccountXDays", "29182.12"));
                //tempList.Add(new KVPair("nbxAccount30Days", "28157.89"));
                //tempList.Add(new KVPair("nbxAccount60Days", "27133.66"));
                //tempList.Add(new KVPair("nbxAccount90Days", "26109.43"));
                //tempList.Add(new KVPair("nbxAccount120Days", "25085.2"));
                //tempList.Add(new KVPair("nbxAccount150Days", "24060.97"));
                //tempList.Add(new KVPair("txtLastPaymentDate", "25/12/2021"));
                //tempList.Add(new KVPair("nbxLastPaymentAmount", "1000.2"));
                //var tempList2 = new List<KVPair>();
                //tempList2.Add(new KVPair("txtAccountNumber", "67890"));
                //tempList2.Add(new KVPair("txtAccountProductType", "Store Card"));
                //tempList2.Add(new KVPair("nbxAccountBalance", "51.23"));
                //tempList2.Add(new KVPair("nbxAccountBalance ", "1241.23"));
                //tempList2.Add(new KVPair("nbxAccountTotalDue", "1231.2"));
                //tempList2.Add(new KVPair("nbxAccountAmountInArrears", "532.03"));
                //tempList2.Add(new KVPair("txtAccountPaymentArrangement", "Yes"));
                //tempList2.Add(new KVPair("txtAccountForbearance", ""));
                //tempList2.Add(new KVPair("txtAccountPTPPlanStatus", "In Effect"));
                //tempList2.Add(new KVPair("txtAccountActiveInsurance", "No"));
                //tempList2.Add(new KVPair("txtAccountCreationDate", "13/2/2012"));
                //tempList2.Add(new KVPair("txtAccountUpdatedDate", "7/2/2022"));
                //tempList2.Add(new KVPair("txtAccountStatus", "Open"));
                //tempList2.Add(new KVPair("nbxAccountCurrentDue", "1231.03"));
                //tempList2.Add(new KVPair("txtAccountDueDate", "25/2/2022"));
                //tempList2.Add(new KVPair("txtAccountDelinquencyCycle", "0"));
                //tempList2.Add(new KVPair("txtAccountExistingPTP", "Yes"));
                //tempList2.Add(new KVPair("txtAccountGraceDate", "No"));
                //tempList2.Add(new KVPair("txtAccountInsuranceOption", ""));
                //tempList2.Add(new KVPair("nbxAccountXDays", "21411.12"));
                //tempList2.Add(new KVPair("nbxAccount30Days", "16351.89"));
                //tempList2.Add(new KVPair("nbxAccount60Days", "12231.66"));
                //tempList2.Add(new KVPair("nbxAccount90Days", "10895.43"));
                //tempList2.Add(new KVPair("nbxAccount120Days", "9656.2"));
                //tempList2.Add(new KVPair("nbxAccount150Days", "5453.97"));
                //tempList2.Add(new KVPair("txtLastPaymentDate", "25/12/2021"));
                //tempList2.Add(new KVPair("nbxLastPaymentAmount", "1231.2"));
                //#endregion

                //KVColl NestedColl = new KVColl();
                //KVColl NestedColl1 = new KVColl();
                //NestedColl.RepeatingControl = "sfAccountPTP";
                //NestedColl1.RepeatingControl = "sfAccountPTP";
                //NestedColl.Values = new List<List<KVPair>>();
                //NestedColl1.Values = new List<List<KVPair>>();
                //List<KVPair> nestedValues1 = new List<KVPair>();
                //nestedValues1.Add(new KVPair("txtAccountPTPDate", "25/1/2022"));
                //nestedValues1.Add(new KVPair("nbxAccountPTPAmount", "1000.21"));
                //nestedValues1.Add(new KVPair("txtAccountPTPMethode", "Debit Order"));
                //nestedValues1.Add(new KVPair("txtAccountPTPOutcome", "Honored"));
                //List<KVPair> nestedValues2 = new List<KVPair>();
                //nestedValues2.Add(new KVPair("txtAccountPTPDate", "25/12/2021"));
                //nestedValues2.Add(new KVPair("nbxAccountPTPAmount", "1000.21"));
                //nestedValues2.Add(new KVPair("txtAccountPTPMethode", "Debit Order"));
                //nestedValues2.Add(new KVPair("txtAccountPTPOutcome", "Honored"));
                //List<KVPair> nestedValues3 = new List<KVPair>();
                //nestedValues3.Add(new KVPair("txtAccountPTPDate", "25/11/2021"));
                //nestedValues3.Add(new KVPair("nbxAccountPTPAmount", "1000.21"));
                //nestedValues3.Add(new KVPair("txtAccountPTPMethode", "Debit Order"));
                //nestedValues3.Add(new KVPair("txtAccountPTPOutcome", "Honored"));
                //NestedColl.Values.Add(nestedValues1);
                //NestedColl.Values.Add(nestedValues2);
                //NestedColl.Values.Add(nestedValues3);

                //NestedColl1.Values.Add(nestedValues1);
                //NestedColl1.Values.Add(nestedValues2);
                //NestedColl1.Values.Add(nestedValues3);

                //List<KVPairListNested> kVPairListNesteds = new();

                //KVPairListNested kvPairs1 = new() { Values = tempList, NestedColl = NestedColl };
                //KVPairListNested kvPairs2 = new() { Values = tempList2, NestedColl = NestedColl1 };

                //kVPairListNesteds.Add(kvPairs1);
                //kVPairListNesteds.Add(kvPairs2);

                //var addToPacket = new NestedCollection() { RepeatingControl = "sfAccount", KVNested = kVPairListNesteds};


                //PacketReturn.kVColls.Add(addToPacket);
                #endregion
                return Ok(PacketReturn);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
