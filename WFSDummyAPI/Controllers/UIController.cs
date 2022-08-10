using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WFS_UI_API.Service;
using System.Threading.Tasks;

namespace WFS_UI_API.Controllers
{
    [Route("api/ui")]
    [ApiController]
    public class UIController : ControllerBase
    {
        private DataService _dataService { get; set; }

        public UIController(DataService inDataService)
        {
            _dataService = inDataService;
        }

        #region [ Deprecated Endpoints ]
        [HttpGet]
        [Route("lookupsources")]
        public async Task<IActionResult> GetLookupSource()
        {
            try
            {
                return BadRequest("Deprecated Endpoint");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpGet]
        [Route("lookupsource")]
        public async Task<IActionResult> GetLookupSource(string inName, string ChannelType)
        {
            try
            {
                #region [ Old Dummy Data ]
                /*
                List<KVCollDDL> outList = new();
                KVCollDDL ActionValue = new();
                ActionValue.RepeatingControl = "ddlActionValue";
                ActionValue.Values.Add(new KVPair("1", "Payment Arrangement"));
                ActionValue.Values.Add(new KVPair("2", "Account Enquiry"));
                ActionValue.Values.Add(new KVPair("3", "Complaint / Customer Experience"));
                ActionValue.Values.Add(new KVPair("4", "Letter Request"));
                ActionValue.Values.Add(new KVPair("5", "Customer Retrenched"));
                ActionValue.Values.Add(new KVPair("6", "Customer Deceased"));
                ActionValue.Values.Add(new KVPair("7", "Other Insurance Query"));
                ActionValue.Values.Add(new KVPair("8", "Recoveries Query"));
                ActionValue.Values.Add(new KVPair("9", "Payment Query / Missing Payment"));
                ActionValue.Values.Add(new KVPair("10", "Missing Payment POP Requested"));
                ActionValue.Values.Add(new KVPair("11", "In Duplum Remediation"));
                ActionValue.Values.Add(new KVPair("12", "Potential Fraud"));
                ActionValue.Values.Add(new KVPair("13", "Debt Review"));
                ActionValue.Values.Add(new KVPair("14", "Call Back"));
                ActionValue.Values.Add(new KVPair("15", "Customer to Confirm / No Commitment / No PTP"));
                ActionValue.Values.Add(new KVPair("16", "Wrong Selection / Transfer to other department"));
                ActionValue.Values.Add(new KVPair("17", "Session Disconnected / Dropped Call"));
                KVCollDDL BankAccountType = new();
                BankAccountType.RepeatingControl = "ddlPaymentSalaryFrequency";
                BankAccountType.Values.Add(new KVPair("1", "Current Account"));
                BankAccountType.Values.Add(new KVPair("2", "Savings Account"));
                BankAccountType.Values.Add(new KVPair("3", "Weekly"));
                BankAccountType.Values.Add(new KVPair("4", "Bi-Weekly"));
                BankAccountType.Values.Add(new KVPair("5", "Monthly"));
                KVCollDDL AbilityToPay = new();
                AbilityToPay.RepeatingControl = "ddlAbilityToPay";
                AbilityToPay.Values.Add(new KVPair("1", "Full Outstanding Amount"));
                AbilityToPay.Values.Add(new KVPair("2", "Minimum Amount Required"));
                KVCollDDL ddlPaymentBankAccountType = new();
                ddlPaymentBankAccountType.RepeatingControl = "ddlPaymentBankAccountType";
                ddlPaymentBankAccountType.Values.Add(new KVPair("1", "Current Account"));
                ddlPaymentBankAccountType.Values.Add(new KVPair("2", "Savings Account"));
                KVCollDDL ddlNonPaymentReason = new();
                ddlNonPaymentReason.RepeatingControl = "ddlNonPaymentReason";
                ddlNonPaymentReason.Values.Add(new KVPair("1", "Retrenched"));
                ddlNonPaymentReason.Values.Add(new KVPair("2", "Forgot"));
                KVCollDDL ddlPaymentNumberOfArrangements = new();
                ddlPaymentNumberOfArrangements.RepeatingControl = "ddlPaymentNumberOfArrangements";
                ddlPaymentNumberOfArrangements.Values.Add(new KVPair("1", "1"));
                ddlPaymentNumberOfArrangements.Values.Add(new KVPair("2", "2"));
                ddlPaymentNumberOfArrangements.Values.Add(new KVPair("3", "3"));
                KVCollDDL ddlPreferredLanguage = new();
                ddlPreferredLanguage.RepeatingControl = "ddlPreferredLanguage";
                ddlPreferredLanguage.Values.Add(new KVPair("1", "Ndebele"));
                ddlPreferredLanguage.Values.Add(new KVPair("2", "Northern Sotho"));
                ddlPreferredLanguage.Values.Add(new KVPair("3", "Sotho"));
                ddlPreferredLanguage.Values.Add(new KVPair("4", "SiSwati"));
                ddlPreferredLanguage.Values.Add(new KVPair("5", "Tsonga"));
                ddlPreferredLanguage.Values.Add(new KVPair("6", "Tswana"));
                ddlPreferredLanguage.Values.Add(new KVPair("7", "Venda"));
                ddlPreferredLanguage.Values.Add(new KVPair("8", "Xhosa"));
                ddlPreferredLanguage.Values.Add(new KVPair("9", "Zulu"));
                ddlPreferredLanguage.Values.Add(new KVPair("10", "Afrikaans"));
                ddlPreferredLanguage.Values.Add(new KVPair("11", "English"));
                KVCollDDL ddlPaymentPTPMethode = new();
                ddlPaymentPTPMethode.RepeatingControl = "ddlPaymentPTPMethode";
                ddlPaymentPTPMethode.Values.Add(new KVPair("1", "Debit Order Payment"));
                ddlPaymentPTPMethode.Values.Add(new KVPair("2", "EFT"));
                ddlPaymentPTPMethode.Values.Add(new KVPair("3", "Woolies Store"));
                ddlPaymentPTPMethode.Values.Add(new KVPair("4", "ABSA Branch"));
                ddlPaymentPTPMethode.Values.Add(new KVPair("5", "ATM"));
                ddlPaymentPTPMethode.Values.Add(new KVPair("6", "PayNow"));
                KVCollDDL ddlLetterType = new();
                ddlLetterType.RepeatingControl = "ddlLetterType";
                ddlLetterType.Values.Add(new KVPair("1", "Statements"));
                ddlLetterType.Values.Add(new KVPair("2", "Statement Letter"));
                ddlLetterType.Values.Add(new KVPair("3", "Paid Up Letter"));
                ddlLetterType.Values.Add(new KVPair("4", "Up to Date Letter"));
                KVCollDDL ddlPaymentBankName = new();
                ddlPaymentBankName.RepeatingControl = "ddlPaymentBankName";
                ddlPaymentBankName.Values.Add(new KVPair("1", "ABSA"));
                ddlPaymentBankName.Values.Add(new KVPair("2", "FNB"));
                KVCollDDL ddlDeceaseTitle = new();
                ddlDeceaseTitle.RepeatingControl = "ddlDeceaseTitle";
                ddlDeceaseTitle.Values.Add(new KVPair("1", "Mr"));
                ddlDeceaseTitle.Values.Add(new KVPair("2", "Ms"));
                KVCollDDL ddlDeceaseRelation = new();
                ddlDeceaseRelation.RepeatingControl = "ddlDeceaseRelation";
                ddlDeceaseRelation.Values.Add(new KVPair("1", "Brother"));
                ddlDeceaseRelation.Values.Add(new KVPair("2", "Sister"));
                ddlDeceaseRelation.Values.Add(new KVPair("3", "Mother"));
                ddlDeceaseRelation.Values.Add(new KVPair("4", "Father"));
                outList.Add(ddlPaymentBankName);
                outList.Add(ddlDeceaseTitle);
                outList.Add(ddlDeceaseRelation);
                outList.Add(AbilityToPay);
                outList.Add(BankAccountType);
                outList.Add(ActionValue);
                outList.Add(ddlPaymentBankAccountType);
                outList.Add(ddlNonPaymentReason);
                outList.Add(ddlPaymentNumberOfArrangements);
                outList.Add(ddlPreferredLanguage);
                outList.Add(ddlPaymentPTPMethode);
                outList.Add(ddlLetterType);
                */
                #endregion
                #region [ Construct Query ]
                string query = String.Empty;
                query = (ChannelType == "OUT") ?
                    @"SELECT [ValueID] AS [Value]
                                        ,[ValueString] AS [Key]
                                    FROM [AGPUI].[dbo].[LOOKUPSOURCE]
                                    WHERE FieldName = @INPUTNAME
                                    AND [ChannelType] = 'OUT'" :
                    @"SELECT [ValueID] AS [Value]
                                        ,[ValueString] AS [Key]
                                    FROM [AGPUI].[dbo].[LOOKUPSOURCE]
                                    WHERE FieldName = @INPUTNAME";
                #endregion
                #region [ Database Dip and Construct Response ]
                var outListKVPair = await _dataService.SelectMany<KVPair, dynamic>(query, new { INPUTNAME = inName }, "UI");
                var outColl = new KVCollDDL() { RepeatingControl = inName, Values = outListKVPair };
                #endregion
                return Ok(outColl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("teamleader")]
        public async Task<IActionResult> GetTeamLeader(int Login)
        {
            try
            {
                #region [ Define Query ]
                string query = @"EXEC [API].[spFetchTeamLeader] @INPUTLOGIN";
                #endregion
                var outGroupName = await _dataService.SelectSingle<dynamic, dynamic>(query, new { INPUTLOGIN = Login }, "UI");
                return (outGroupName == null) ? Ok("No Team Leader") : Ok(outGroupName.NAME.ToString());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
