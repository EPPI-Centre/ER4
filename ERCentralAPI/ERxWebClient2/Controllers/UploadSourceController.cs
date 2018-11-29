using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using EPPIDataServices.Helpers;
using Newtonsoft.Json;
using BusinessLibrary.BusinessClasses.ImportItems;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UploadSourceController : CSLAController
    {

        private readonly ILogger _logger;
		private SearchCodesCommand cmd;

		public UploadSourceController(ILogger<SearchListController> logger)
        {
            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult GetSources()
        {
			try
            {
                SetCSLAUser();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                throw new NotImplementedException();

                //return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "GetSearches data portal error");
                throw;
            }

		}

		[HttpPost("[action]")]
		public IActionResult VerifyFile([FromBody] UploadOrCheckSource incoming)
		{

			try
			{
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    FilterRules rules = new FilterRules();
                    DataPortal<ReadOnlyImportFilterRuleList> dp = new DataPortal<ReadOnlyImportFilterRuleList>();
                    ReadOnlyImportFilterRuleList roifrList = dp.Fetch();

                    ReadOnlyImportFilterRule inRules = new ReadOnlyImportFilterRule();
                    foreach (ReadOnlyImportFilterRule rule in roifrList)
                    {
                        if (rule.RuleName == incoming.FilterName)
                        {
                            inRules = rule;
                            break;
                        }
                    }
                    if (inRules.RuleName == "") throw new Exception("Filter name was not found, operation is ABORTED");
                    foreach (BusinessLibrary.BusinessClasses.TypeRules tprs in inRules.typeRules)
                    {
                        rules.typeRules.Add(new BusinessLibrary.BusinessClasses.ImportItems.TypeRules(tprs.Type_ID, tprs.RuleName, tprs.RuleRegexSt));
                    }
                    foreach (KeyValuePair<int, string> kvp in inRules.typesMap)
                    {
                        rules.AddTypeDef(kvp.Key, kvp.Value);
                    }
                    rules.Abstract_Set(inRules.Abstract);
                    rules.author_Set(inRules.Author);
                    rules.Availability_Set(inRules.Availability);
                    rules.City_Set(inRules.City);
                    rules.date_Set(inRules.Date);
                    rules.DefaultTypeCode = inRules.DefaultTypeCode;
                    rules.Edition_Set(inRules.Edition);
                    rules.EndPage_Set(inRules.EndPage);
                    rules.Institution_Set(inRules.Institution);
                    rules.Issue_Set(inRules.Issue);
                    rules.month_Set(inRules.Month);
                    rules.Notes_Set(inRules.Notes);
                    rules.OldItemID_Set(inRules.OldItemId);
                    rules.Pages_Set(inRules.Pages);
                    rules.pAuthor_Set(inRules.ParentAuthor);
                    rules.pTitle_Set(inRules.pTitle);
                    rules.Publisher_Set(inRules.Publisher);
                    rules.shortTitle_Set(inRules.shortTitle);
                    rules.StandardN_Set(inRules.StandardN);
                    rules.startOfNewField_Set(inRules.StartOfNewField);
                    rules.startOfNewRec_Set(inRules.StartOfNewRec);
                    rules.StartPage_Set(inRules.StartPage);
                    rules.title_Set(inRules.Title);
                    rules.typeField_Set(inRules.typeField);
                    rules.Url_Set(inRules.Url);
                    rules.Volume_Set(inRules.Volume);
                    rules.DOI_Set(inRules.DOI);
                    rules.Keywords_Set(inRules.Keywords);
                    List<ItemIncomingData> FullRes = ImportRefs.Imp(incoming.FileContent, rules);
                    IncomingItemsList res = new IncomingItemsList();
                    res.totalReferences = FullRes.Count;
                    if (FullRes.Count > 100)
                    {//send back only 100 results...
                        res.incomingItems = FullRes.GetRange(0, 100);
                    }
                    else
                    {
                        res.incomingItems = FullRes;
                    }
                    return Ok(res);
                }
                else return Forbid();
            }
			catch (Exception e)
			{
				_logger.LogException(e, "GetSearches data portal error");
				throw;
			}

		}


		[HttpPost("[action]")]
		public IActionResult SearchImportedIDs([FromBody] CodeCommand cmdIn)
		{

			try
			{
                SetCSLAUser4Writing();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

				SearchImportedIDsCommand cmd = new SearchImportedIDsCommand(
					cmdIn._title, cmdIn._included
					);
				DataPortal<SearchImportedIDsCommand> dp = new DataPortal<SearchImportedIDsCommand>();
				cmd = dp.Execute(cmd);

				return Ok(cmd.SearchId);
			}
			catch (Exception e)
			{
				_logger.LogException(e, "GetSearches data portal error");
				throw;
			}

		}
	}

    public class UploadOrCheckSource
    {
        public string FilterName = "";
        public string FileContent = "";
    }
    public class IncomingItemsList
    {
        public int totalReferences = 0;
        public List<ItemIncomingData> incomingItems = new List<ItemIncomingData>();
    }
//    export interface IncomingItem
//    {
//        title: string;
//    parentTitle: string;
//    authors: string;
//    year: string;
//}
}

