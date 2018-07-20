using System;
using System.Net;
using Csla.DataPortalClient;
using System.Security.Principal;
using System.Configuration;
using System.Windows;

namespace BusinessLibrary.Compression
{
  public class CompressedProxy<T> : WcfProxy<T>
    where T : Csla.Serialization.Mobile.IMobileObject
  {
      public CompressedProxy()
      {
          BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
          DateTime? until = Application.Current.Resources["UseHTTPSuntil"] as DateTime?;
          string host = System.Windows.Application.Current.Host.Source.DnsSafeHost.ToLower();
          if (host == "eppi.ioe.ac.uk" | host == "epi2" | host =="epi2.ioe.ac.uk")
          {
                  if( ri == null || ri.Ticket == null || ri.Ticket == "") //(ri != null && ri.Ticket != "")
                  {//not authenticated, use https
                      this.EndPoint = "HttpsBinding_IWcfPortal";
                  }
                  else if (until != null && until > DateTime.Now)
                  {//current config tells us to use https (for archie, or from another option)
                      this.EndPoint = "HttpsBinding_IWcfPortal";
                  }
                  else
                  {//all normal use http
                      this.EndPoint = "BasicHttpBinding_IWcfPortal";
                  }
          }
          else if (host == "epi3" | host == "epi3.westeurope.cloudapp.azure.com")
          {//for Azure, as a first attempt, we'll only use http, so always AzureHttpBinding_IWcfPortal
              //WARNING: this should disappear and merge with the previous IF when in production!!
              //unless we get a cert for epi3.westeurope.cloudapp.azure.com, the HTTPS endpoint can only work after changing DNS!!
              if (ri == null || ri.Ticket == null || ri.Ticket == "") //(ri != null && ri.Ticket != "")
              {//not authenticated, use https
                  this.EndPoint = "AzureHttpBinding_IWcfPortal";
              }
              else if (until != null && until > DateTime.Now)
              {//current config tells us to use https (for archie, or from another option)
                  this.EndPoint = "AzureHttpBinding_IWcfPortal";
              }
              else
              {//all normal use http
                  this.EndPoint = "AzureHttpBinding_IWcfPortal";
              }
          }

          else if (host == "bk-epi" | host == "bk-epi.ioead" | host == "bk-epi.ioe.ac.uk")
          {
              if (ri == null || ri.Ticket == null || ri.Ticket == "") //(ri != null && ri.Ticket != "")
              {//not authenticated, use https
                  this.EndPoint = "TestingHttpsBinding_IWcfPortal";
              }
              else if (until != null && until > DateTime.Now)
              {//current config tells us to use https (for archie, or from another option)
                  this.EndPoint = "TestingHttpsBinding_IWcfPortal";
              }
              else
              {//all normal use http
                  this.EndPoint = "TestingBasicHttpBinding_IWcfPortal";
              }
          }
          else if (host == "cs00032214" || host == "cs00032214.ucl.ac.uk" || host == "cs00032214.ad.ucl.ac.uk")
          {
                //if (ri == null || ri.Ticket == null || ri.Ticket == "") //(ri != null && ri.Ticket != "")
                //{//not authenticated, use https
                //    this.EndPoint = "LocalHttpsBinding_IWcfPortal";
                //}
                //else if (until != null && until > DateTime.Now)
                //{//current config tells us to use https (for archie, or from another option)
                //    this.EndPoint = "LocalHttpsBinding_IWcfPortal";
                //}
                //else
                //{//all normal use http
                //    this.EndPoint = "LocalBasicHttpBinding_IWcfPortal";
                //}
                this.EndPoint = "cs00032214HttpBinding_IWcfPortal";
          }
            else if (host == "eppi-management" )
            {
                if (ri == null || ri.Ticket == null || ri.Ticket == "") //(ri != null && ri.Ticket != "")
                {//not authenticated, use https
                    this.EndPoint = "HttpsManagement_Binding_IWcfPortal";
                }
                else if (until != null && until > DateTime.Now)
                {//current config tells us to use https (for archie, or from another option)
                    this.EndPoint = "HttpsManagement_Binding_IWcfPortal";
                }
                else
                {//all normal use http
                    this.EndPoint = "Management_HttpBinding_IWcfPortal";
                }
            }
            else
          {//don't care, this is local, use http
              this.EndPoint = "LocalBasicHttpBinding_IWcfPortal";
          }
      }
      protected override Csla.WcfPortal.CriteriaRequest ConvertRequest(Csla.WcfPortal.CriteriaRequest request)
    {

      Csla.WcfPortal.CriteriaRequest returnValue = new Csla.WcfPortal.CriteriaRequest();
      returnValue.ClientContext = CompressionUtility.Compress(request.ClientContext);
      returnValue.GlobalContext = CompressionUtility.Compress(request.GlobalContext);
      if (request.CriteriaData != null)
        returnValue.CriteriaData = CompressionUtility.Compress(request.CriteriaData);
      returnValue.Principal = CompressionUtility.Compress(request.Principal);
      returnValue.TypeName = request.TypeName;
      return returnValue;
    }

    protected override Csla.WcfPortal.UpdateRequest ConvertRequest(Csla.WcfPortal.UpdateRequest request)
    {
      Csla.WcfPortal.UpdateRequest returnValue = new Csla.WcfPortal.UpdateRequest();
      returnValue.ClientContext = CompressionUtility.Compress(request.ClientContext);
      returnValue.GlobalContext = CompressionUtility.Compress(request.GlobalContext);
      returnValue.ObjectData = CompressionUtility.Compress(request.ObjectData);
      returnValue.Principal = CompressionUtility.Compress(request.Principal);
      return returnValue;
    }

    protected override Csla.WcfPortal.WcfResponse ConvertResponse(Csla.WcfPortal.WcfResponse response)
    {
      Csla.WcfPortal.WcfResponse returnValue = new Csla.WcfPortal.WcfResponse();
      returnValue.GlobalContext = CompressionUtility.Decompress(response.GlobalContext);
      returnValue.ObjectData = CompressionUtility.Decompress(response.ObjectData);
      returnValue.ErrorData = response.ErrorData;
      return returnValue;
    }
  }
}
