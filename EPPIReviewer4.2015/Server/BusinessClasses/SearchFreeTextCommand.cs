using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;

#endif

namespace BusinessLibrary.BusinessClasses
{
	[Serializable]
	public class SearchFreeTextCommand : CommandBase<SearchFreeTextCommand>
	{
#if SILVERLIGHT
    public SearchFreeTextCommand(){}
#else
		public SearchFreeTextCommand() { }
#endif

		private string _title;
		private bool _included;
		private string _search_what;
		private int _searchId;

		public int SearchId
		{
			get
			{
				return _searchId;
			}
		}

		public SearchFreeTextCommand(string Title, bool Included, string SearchWhat)
		{
			_title = Title;

			_included = Included;
			_search_what = SearchWhat;
		}

		protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
		{
			base.OnGetState(info, mode);
			info.AddValue("_title", _title);
			info.AddValue("_included", _included);
			info.AddValue("_search_what", _search_what);
			info.AddValue("_searchId", _searchId);
		}
		protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
		{
			_title = info.GetValue<string>("_title");
			_included = info.GetValue<bool>("_included");
			_search_what = info.GetValue<string>("_search_what");
			_searchId = info.GetValue<int>("_searchId");
		}


#if !SILVERLIGHT

		protected override void DataPortal_Execute()
		{
			using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
			{
				connection.Open();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

				FullTextSearch fts = new FullTextSearch(_title);
				string Title = _title;
				string query = fts.NormalForm;
				if (Title.Length > 3970) //it's too long, will need to shorten it.
				{
					string temp = Title.Substring(0, 3970);

					while (temp.Length > 3969)
					{
						if (temp.LastIndexOf(' ') == -1)
						{//can't find a space, will need to truncate
							temp = temp.Substring(0, 3960);
						}
						else
						{
							temp = Title.Substring(0, temp.LastIndexOf(' '));
						}
					}
					Title = temp + " [...]";
				}
				Title = "\"" + Title + "\" (in ";
				switch (_search_what)
				{
					case "TitleAbstract":
						Title += "Title and Abstract)";
						break;
					case "Title":
						Title += "Title)";
						break;
					case "Abstract":
						Title += "Abstract)";
						break;
					case "PubYear":
						Title += "Year)";
						query = _title;
						break;
					case "AdditionalText":
						Title += "Additional Text)";
						break;
					case "Authors":
						Title += "Authors)";
						query = _title;
						break;
					case "UploadedDocs":
						Title += "Uploaded Documents)";
						break;
					default:
						Title += "Unknown)";
						query = _title;
						break;
				}
				Int64 SearchItemId = 0;
				if ((_search_what == "ItemId") && (!Int64.TryParse(_title, out SearchItemId)))
				{
					SearchItemId = 0;
				}
				using (SqlCommand command = new SqlCommand("st_SearchFreeText", connection))
				{
					command.CommandType = System.Data.CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
					command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
					command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", Title));
					command.Parameters.Add(new SqlParameter("@SEARCH_TEXT", query));
					command.Parameters.Add(new SqlParameter("@INCLUDED", _included));
					command.Parameters.Add(new SqlParameter("@SEARCH_WHAT", _search_what));
					command.Parameters.Add(new SqlParameter("@SEARCH_ID", 0));
					command.Parameters.Add(new SqlParameter("@SEARCH_ITEM_ID", SearchItemId));
					command.Parameters["@SEARCH_ID"].Direction = System.Data.ParameterDirection.Output;
					command.ExecuteNonQuery();
					_searchId = Convert.ToInt32(command.Parameters["@SEARCH_ID"].Value);
				}
				connection.Close();
			}
		}

#endif
	}
}
