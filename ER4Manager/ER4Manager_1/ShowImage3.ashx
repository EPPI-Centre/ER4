<%@ WebHandler Language="C#" Class="ShowImage3" %>

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

public class ShowImage3 : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) 
    {
        bool isAdmDB = true;
        IDataReader idr;
        idr = Utils.GetReader(isAdmDB, "st_WDGetHeaderImage3", Convert.ToInt32(context.Request.QueryString["id"]));
        if (idr.Read())
        {
            //string test = idr["HEADER_IMAGE"].ToString();
            if (idr["HEADER_IMAGE_3"].ToString() != "")
            {
                context.Response.BinaryWrite((Byte[])idr[0]);
                context.Response.End();
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