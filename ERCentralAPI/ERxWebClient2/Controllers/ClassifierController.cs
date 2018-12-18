using System;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;
using Csla;


namespace ERxWebClient2.Controllers
{
	
	[Authorize]
	[Route("api/[controller]")]
	public class ClassifierController : CSLAController
	{
		private readonly ILogger _logger;
		private int _classifierId = -1;
		private string _returnMessage = "Success";


		public ClassifierController(ILogger<ClassifierController> logger)
		{
			_logger = logger;
		
		}

		[HttpGet("[action]")]
		public IActionResult GetClassifierModelList()
		{
			try
			{
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

				DataPortal<ClassifierModelList> dp = new DataPortal<ClassifierModelList>();

				ClassifierModelList result = dp.Fetch();
				return Ok(result);
							   
			}
			catch (Exception)
			{
				_logger.LogError("models list error");
				throw;
			}

		}
		
		[HttpPost("[action]")]
		public async Task<IActionResult> GetClassifierAsync([FromBody] MVCClassifierCommand MVCcmd)
		{
			

			try
			{
				
				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				
				ClassifierCommand cmd = new ClassifierCommand(
						MVCcmd._title
						, MVCcmd._attributeIdOn
						, MVCcmd._attributeIdNotOn
						, MVCcmd._attributeIdClassifyTo
						, _classifierId
						, MVCcmd._sourceId
					);
				cmd.RevInfo = MVCcmd.revInfo.ToCSLAReviewInfo();

				DataPortal<ClassifierCommand> dp = new DataPortal<ClassifierCommand>();

				cmd = await dp.ExecuteAsync(cmd);

				return Ok(cmd);

			}
			catch (Exception e)
			{
				_logger.LogException(e, "A ClassifierCommand issue");
				throw;
			}

		}


		[HttpPost("[action]")]
		public async Task<IActionResult> ApplyClassifierAsync([FromBody] MVCClassifierCommand MVCcmd)
		{
			try
			{

				SetCSLAUser();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

				ClassifierCommand cmd = new ClassifierCommand(
						MVCcmd._title
						, -1
						, -1
						, MVCcmd._attributeId
						, MVCcmd._modelId
						, MVCcmd._sourceId
					);
				cmd.RevInfo = MVCcmd.revInfo.ToCSLAReviewInfo();

				DataPortal<ClassifierCommand> dp = new DataPortal<ClassifierCommand>();

				cmd = await dp.ExecuteAsync(cmd);

				return Ok(cmd);

			}
			catch (Exception e)
			{
				_logger.LogException(e, "A ClassifierCommand issue");
				throw;
			}

		}
		
		public class MVCClassifierCommand
		{
			public string _title { get; set; }
			public Int64 _attributeIdOn { get; set; }
			public Int64 _attributeIdNotOn { get; set; }
			public Int64 _attributeIdClassifyTo { get; set; }
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
			public bool screeningIndexed { get; set; }
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
				result.ScreeningIndexed = this.screeningIndexed;
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

	}
}
