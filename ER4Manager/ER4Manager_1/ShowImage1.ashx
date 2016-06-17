<%@ WebHandler Language="C#" Class="ShowImage1" %>

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

public class ShowImage1 : IHttpHandler {
    
    public void ProcessRequest (HttpContext context1) 
    {
        bool isAdmDB = true;
        IDataReader idr;
        idr = Utils.GetReader(isAdmDB, "st_WDGetHeaderImage1", Convert.ToInt32(context1.Request.QueryString["id"]));
        if (idr.Read())
        {
            //string test = idr["HEADER_IMAGE"].ToString();
            if (idr["HEADER_IMAGE_1"].ToString() != "")
            {
                context1.Response.BinaryWrite((Byte[])idr[0]);
                context1.Response.End();
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