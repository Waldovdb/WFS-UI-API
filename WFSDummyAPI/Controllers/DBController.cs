using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WFS_UI_API.Service;
using WFS_UI_API.Data;

namespace WFS_UI_API.Controllers
{
    [Route("api/db")]
    [ApiController]
    public class DBController : ControllerBase
    {
        private DataService _dataService { get; set; }
        private string UIDB { get; set; }
        private string CIMDB { get; set; }
        private string TADDB { get; set; }

        public DBController(DataService inDataService)
        {
            #region [ Declaration ]
            _dataService = inDataService;
            UIDB = "Data Source=10.21.160.104;Initial Catalog=AGPUI;User Id=InovoCIM;Password=g8rF1eld;";
            CIMDB = "Data Source=10.21.160.104;Initial Catalog=InovoCIM;User Id=InovoCIM;Password=g8rF1eld;";
            TADDB = "Data Source=10.22.128.60;Initial Catalog=AGPSource;User Id=WCollectReader;Password=dslkjkj,.%2345389SADFdjflh398fdjk;";
            #endregion
        }

        [HttpGet]
        [Route("ProcessAGPEvent")]
        public async Task<IActionResult> GetAGPEvents()
        {
            try
            {
                var res = await _dataService.SelectMany<dynamic, dynamic>("EXEC [dbo].[spGetAGPQueue]", new { }, "UI");
                foreach(var item in res)
                {
                    var tempJson = Newtonsoft.Json.JsonConvert.DeserializeObject(item.EVENTPARAMS);
                    WFS_UI_API.Data.Models.TempModel tempModel = new WFS_UI_API.Data.Models.TempModel() { SourceID = item.SourceID, ServiceID = tempJson.ServiceId, LoadID = tempJson.LoadId, Priority = tempJson.QPriority, IntroType = tempJson.IntroType, Bucket = tempJson.Bucket, SegmentName = tempJson.SegmentName, StrategySegmentID = tempJson.StrategySegmentId };
                    string queryTemp = @"INSERT INTO [dbo].[AGPEventLoad]
                                               ([SourceID]
                                               ,[Phone]
                                               ,[ServiceID]
                                               ,[LoadID]
                                               ,[Priority]
                                               ,[IntroType]
                                               ,[Bucket]
                                               ,[SegmentName]
                                               ,[StrategySegmentID])
                                         VALUES
                                               (@SOURCEID
                                               ,'823241600'
                                               ,@SERVICEID
                                               ,@LOADID
                                               ,@PRIORITY
                                               ,@INTROTYPE
                                               ,@BUCKET
                                               ,@SEGMENTNAME
                                               ,@STRATEGYSEGMENTID)";
                    await _dataService.InsertSingle<dynamic, dynamic>(queryTemp, new { SOURCEID = tempModel.SourceID, SERVICEID = tempModel.ServiceID, LOADID = tempModel.LoadID, PRIORITY = tempModel.Priority, INTROTYPE = tempModel.IntroType, BUCKET = tempModel.Bucket, SEGMENTNAME = tempModel.SegmentName, STRATEGYSEGMENTID = tempModel.StrategySegmentID }, "CIM");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("requestdatacopy")]
        public async Task<IActionResult> RequestDataCopy(DataCopy inData)
        {
            try
            {
                string DBConnStringInput = String.Empty;
                string DBConnStringOutput = String.Empty;
                switch (inData.InputDatabase)
                {
                    case "UI":
                        DBConnStringInput = UIDB;
                        break;
                    case "CIM":
                        DBConnStringInput = CIMDB;
                        break;
                    case "TAD":
                        DBConnStringInput= TADDB;
                        break;

                };
                switch (inData.OutputDatabase)
                {
                    case "UI":
                        DBConnStringOutput = UIDB;
                        break;
                    case "CIM":
                        DBConnStringOutput = CIMDB;
                        break;
                    case "TAD":
                        DBConnStringOutput = TADDB;
                        break;
                };
                if (inData.VerifyInputs(DBConnStringOutput, DBConnStringInput))
                {
                    return (await _dataService.CopyTable(inData.SourceTable, inData.DestinationTable, DBConnStringInput, DBConnStringOutput, inData.MapUnmapped, inData.GetColumnMaps())) ? Ok("Table Copied") : Ok("Table Copy Failed");
                }
                else
                {
                    return BadRequest("Unmapped Database");
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message + " - " + ex.StackTrace);
            }
        }

    }
}
