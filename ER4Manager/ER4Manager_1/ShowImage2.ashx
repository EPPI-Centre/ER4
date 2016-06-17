<%@ WebHandler Language="C#" Class="ShowImage2" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Configuration;
using System.Collections;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Data.SqlClient;

public class ShowImage2 : IHttpHandler {
    
    public void ProcessRequest (HttpContext context2) 
    {
        bool isAdmDB = true;
        IDataReader idr;
        idr = Utils.GetReader(isAdmDB, "st_WDGetHeaderImage2", Convert.ToInt32(context2.Request.QueryString["id"]));
        if (idr.Read())
        {
            //string test = idr["HEADER_IMAGE"].ToString();
            if (idr["HEADER_IMAGE_2"].ToString() != "")
            {
                context2.Response.BinaryWrite((Byte[])idr[0]);
                context2.Response.End();
            }
        }
        idr.Close();            
    }
 
    public bool IsReusable 
    {
        get {
            return false;
        }
    }

}