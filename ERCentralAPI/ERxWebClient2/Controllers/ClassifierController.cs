using System;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;
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
		private IHubContext<NotifyHub, ITypedHubClient> _hubContext;
		private static string _connectionId = "";

		public ClassifierController(ILogger<ClassifierController> logger, IHubContext<NotifyHub, ITypedHubClient> hubContext)
		{
			_logger = logger;
			_hubContext = hubContext;
		
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

				_connectionId = MVCcmd.ConnectionId;

				Table_Watcher tw = new Table_Watcher(_hubContext, _connectionId);
				tw.WatchTable();
				tw.StartTableWatcher();

				// just insert into the table and see if it is recognised!!
				//string insertString = "INSERT INTO [Reviewer].[dbo].[TB_SEARCH] (	REVIEW_ID,	CONTACT_ID,	SEARCH_TITLE,"
				//+ "SEARCH_NO,	ANSWERS,	HITS_NO,	SEARCH_DATE,	IS_CLASSIFIER_RESULT)"
				//+ "VALUES(7, 1800, 'Items classified according to model: Cochrane RCT Classifier: may be RCTs', 250, NULL,"
				//+ "23, '2018-12-12 14:01:18.677', 1)";

				//string _con = Program.SqlHelper.ER4DB;
				//SqlConnection connection = new SqlConnection(_con);
				//Program.SqlHelper.ExecuteNonQueryNonSP(connection, insertString);


				//string insertString = "INSERT INTO [Reviewer].[dbo].[tb_CLASSIFIER_MODEL] (	MODEL_TITLE,	CONTACT_ID,	REVIEW_ID,	ATTRIBUTE_ID_ON,"
				//	+ " ATTRIBUTE_ID_NOT_ON,	ACCURACY,	AUC,	PRECISION,	RECALL,	TIME_STARTED,	TIME_ENDED)"
				//+ " VALUES('test47(in progress...) ',1799,    7 ,  5307 ,   203640 , NULL, NULL,    NULL, NULL   ,'2018-12-14 10:18:35.520','2018-12-14 10:18:35.520')";


				//string _con = Program.SqlHelper.ER4DB;
				//SqlConnection connection = new SqlConnection(_con);
				//Program.SqlHelper.ExecuteNonQueryNonSP(connection, insertString);


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

				//return Ok();

			}
			catch (Exception e)
			{
				_logger.LogException(e, "A ClassifierCommand issue");
				throw;
			}

		}

		public class MVCClassifierCommand
		{
			public string ConnectionId { get; set; }
			public string _title { get; set; }
			public Int64 _attributeIdOn { get; set; }
			public Int64 _attributeIdNotOn { get; set; }
			public Int64 _attributeIdClassifyTo { get; set; }
			public int _sourceId { get; set; }
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

		public class Table_Watcher
		{
			public string _connectionString = Program.SqlHelper.ER4DB;
			private IHubContext<NotifyHub, ITypedHubClient> _hubContext;
			private string _connectionId = "";

			public Table_Watcher(IHubContext<NotifyHub, ITypedHubClient> hubcontext, string connectionId)
			{
				_hubContext = hubcontext;
				_connectionId = connectionId;

			}

			// System.Configuration.ConfigurationManager.ConnectionStrings["constring"].ConnectionString;
			private SqlTableDependency<ClassifierModel> _dependency;

			public void WatchTable()
			{
				//var mapper = new ModelToTableMapper<Searches>();

				//mapper.AddMapping(model => model.Search_Title, "SEARCH_TITLE");
				//_dependency = new SqlTableDependency<Searches>(_connectionString, "TB_SEARCH", null, mapper);

				var mapper = new ModelToTableMapper<ClassifierModel>();

				mapper.AddMapping(model => model.ModelTitle, "MODEL_TITLE");

				_dependency = new SqlTableDependency<ClassifierModel>(_connectionString, "tb_CLASSIFIER_MODEL", null, mapper);

				_dependency.OnChanged += _dependency_OnChanged;
				_dependency.OnError += _dependency_OnError;

			}

			public void StartTableWatcher()
			{
				_dependency.Start();
			}
			public void StopTableWatcher()
			{
				_dependency.Stop();
			}

			void _dependency_OnError(object sender, ErrorEventArgs e)
			{

				throw e.Error;
			}

			void _dependency_OnChanged(object sender, RecordChangedEventArgs<ClassifierModel> e)
			{
				
				if (e.ChangeType != ChangeType.None)
				{
					switch (e.ChangeType)
					{
						case ChangeType.Delete:

							break;
						case ChangeType.Insert:

							// use signal R to boradcast the message
							// this needs to be changed for one client 
							// not all of them...
							//_hubContext.Clients.All.BroadcastMessage(" ", "");

							_hubContext.Clients.Client(_connectionId).BroadcastMessage("A","B");
							//(_hubContext.Clients);
							//this.StopTableWatcher();
							//Console.WriteLine("present");

							break;
						case ChangeType.Update:

							// use signal R to boradcast the message
							// this needs to be changed for one client 
							// not all of them...
							//_hubContext.Clients.All.BroadcastMessage(" ", "");

							_hubContext.Clients.Client(_connectionId).BroadcastMessage("C", "D");
							this.StopTableWatcher();
					
							//Console.WriteLine("present");

							break;
					}

				}
			}

		}

	}
}
