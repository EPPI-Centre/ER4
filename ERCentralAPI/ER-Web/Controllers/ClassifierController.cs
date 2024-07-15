using System;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;
using Csla;
using System.Collections.Generic;

namespace ERxWebClient2.Controllers
{
	
	[Authorize]
	[Route("api/[controller]")]
	public class ClassifierController : CSLAController
	{
		private int _classifierId = -1;
		private string _returnMessage = "";
		
		public ClassifierController(ILogger<ClassifierController> logger) :base(logger)
		{ }

		[HttpGet("[action]")]
		public IActionResult GetClassifierModelList()
		{
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

				DataPortal<ClassifierModelList> dp = new DataPortal<ClassifierModelList>();

				ClassifierModelList result = dp.Fetch();
				return Ok(result);
							   
			}
			catch (Exception e)
			{
				_logger.LogError("models list error");
				return StatusCode(500, e.Message);
			}

		}
		
		[HttpPost("[action]")]
		public IActionResult Classifier([FromBody] MVCClassifierCommand MVCcmd)
		{
			
			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					ClassifierCommand cmd = new ClassifierCommand(
							MVCcmd._title
							, MVCcmd._attributeIdOn
							, MVCcmd._attributeIdNotOn
							, MVCcmd._attributeIdClassifyTo
							, MVCcmd._classifierId
							, MVCcmd._sourceId
						);
					cmd.RevInfo = MVCcmd.revInfo.ToCSLAReviewInfo();

					DataPortal<ClassifierCommand> dp = new DataPortal<ClassifierCommand>();

					cmd = dp.Execute(cmd);
					
					return Ok(cmd);

				}
				else
				{
					return Forbid();
				}

			}
			catch (Exception e)
			{
				_logger.LogException(e, "A ClassifierCommand issue");
				return StatusCode(500, e.Message);
			}

		}


		[HttpPost("[action]")]
		public IActionResult ApplyClassifier([FromBody] MVCClassifierCommand MVCcmd)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

					ClassifierCommand cmd = new ClassifierCommand(
							MVCcmd._title
							, -1
							, -1
							, MVCcmd._attributeIdClassifyTo
							, MVCcmd._classifierId
							, MVCcmd._sourceId
						);

					cmd.RevInfo = MVCcmd.revInfo.ToCSLAReviewInfo();

					DataPortal<ClassifierCommand> dp = new DataPortal<ClassifierCommand>();

					cmd =  dp.Execute(cmd);

					return Ok(cmd);

				}
				else
				{

					return Forbid();
				}

			}
			catch (Exception e)
			{
				_logger.LogException(e, "A ClassifierCommand issue");
				return StatusCode(500, e.Message);
			}

		}

		[HttpPost("[action]")]
		public IActionResult DeleteModel([FromBody] MVCClassifierCommand _model)
		{
			ClassifierCommand command = new ClassifierCommand();
			try
			{
			    command = new ClassifierCommand(
					       "DeleteThisModel~~",
					       -1,
					       -1,
					       -1,
					      _model._modelId,
					       -1);
				if (SetCSLAUser4Writing()) 
				{
                    DataPortal<ClassifierCommand> dp = new DataPortal<ClassifierCommand>();
                    command.RevInfo = _model.revInfo.ToCSLAReviewInfo();
                    command = dp.Execute(command);
                    return Ok(command);
                }
				else return Forbid();

			}
			catch (Exception e)
			{
				_logger.LogException(e, "DeleteModel has failed. Modelid: " + _model._modelId + " command res: " + command.ReturnMessage);
				return StatusCode(500, e.Message);
			}
		}


		[HttpPost("[action]")]
		public IActionResult UpdateModelName([FromBody] JSONModelName data)
		{
			try
			{
				if (SetCSLAUser4Writing())
				{
					DataPortal<ClassifierContactModel> dp = new DataPortal<ClassifierContactModel>();
					ClassifierContactModel res = dp.Fetch(new SingleCriteria<ClassifierContactModel, int>(data.ModelID));
					if (res.ModelId == 0 || res.ModelId != data.ModelID) return NotFound();
					res.ModelTitle = data.ModelName;
					res = res.Save(); // asking object to save itself
					return Ok(true);
				}
				else return Forbid();
			}
			catch (Exception e)
			{
				_logger.LogException(e, "Contact data portal error");
				return StatusCode(500, e.Message);
			}
		}








		public class MVCClassifierCommand
		{
			public string _title { get; set; }
			public int _attributeIdOn { get; set; }
			public int _attributeIdNotOn { get; set; }
			public int _attributeIdClassifyTo { get; set; }
			public int _sourceId { get; set; }
			public int _modelId { get; set; }
			public int _attributeId { get; set; }
			public int _classifierId { get; set; }
			public MVCReviewInfo revInfo { get; set; }

		}

		public class MVCReviewInfo
		{
			public int reviewId { get; set; }
			public string reviewName { get; set; }
			public bool showScreening { get; set; }
			public int screeningCodeSetId { get; set; }
			public string screeningMode { get; set; }
			public string screeningReconcilliation { get; set; }
			public Int64 screeningWhatAttributeId { get; set; }
			public int screeningNPeople { get; set; }
			public bool screeningAutoExclude { get; set; }
			public bool screeningModelRunning { get; set; }
			//public bool screeningIndexed { get; set; }
			public bool screeningListIsGood { get; set; }
			public string bL_ACCOUNT_CODE { get; set; }
			public string bL_AUTH_CODE { get; set; }
			public string bL_TX { get; set; }
			public string bL_CC_ACCOUNT_CODE { get; set; }
			public string bL_CC_AUTH_CODE { get; set; }
			public string bL_CC_TX { get; set; }
			public ReviewInfo ToCSLAReviewInfo()
			{
				ReviewInfo result = new ReviewInfo();
				result.BL_ACCOUNT_CODE = this.bL_ACCOUNT_CODE;
				result.BL_AUTH_CODE = this.bL_AUTH_CODE;
				result.BL_CC_ACCOUNT_CODE = this.bL_CC_ACCOUNT_CODE;
				result.BL_CC_AUTH_CODE = this.bL_CC_AUTH_CODE;
				result.BL_CC_TX = this.bL_CC_TX;
				result.BL_TX = this.bL_TX;
				result.ReviewId = this.reviewId;
				result.ReviewName = this.reviewName;
				result.ScreeningAutoExclude = this.screeningAutoExclude;
				result.ScreeningCodeSetId = this.screeningCodeSetId;
				//result.ScreeningIndexed = this.screeningIndexed;
				result.ScreeningListIsGood = this.screeningListIsGood;
				result.ScreeningMode = this.screeningMode;
				result.ScreeningModelRunning = this.screeningModelRunning;
				result.ScreeningNPeople = this.screeningNPeople;
				result.ScreeningReconcilliation = this.screeningReconcilliation;
				result.ScreeningWhatAttributeId = this.screeningWhatAttributeId;
				result.ShowScreening = this.showScreening;
				return result;
			}
		}

		public class Searches
		{
			public int SearchId { get; set; }
			public int SearchNo { get; set; }
			public string Search_Title { get; set; }
		}



		public class ClassifierModel
		{
			public int ModelId { get; set; }
			public string ModelTitle { get; set; }
			public double Accuracy { get; set; }
		}



		public class JSONModelName
		{
			public int ModelID = 0;
			public string ModelName = "";
		}

	}
}
