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
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;

public partial class PresenterEditHeaderImage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            //if (Utils.GetSessionString("IsAdm") == "True")
            //{
            if (!IsPostBack)
            {
                lblWebDB_ID.Text = Request.QueryString.Get("WebDb_ID").ToString();

                bool isAdmDB = true;
                IDataReader idr;
                    idr = Utils.GetReader(isAdmDB, "st_WDDescriptionGet_1",
                        lblWebDB_ID.Text);
                    if (idr.Read())
                    {
                        if (Request.QueryString.Get("Area").ToString() == "intro")
                        {
                            tbImageURL1.Text = idr["HEADER_IMAGE_URL_1"].ToString();
                            tbImageURL2.Text = idr["HEADER_IMAGE_URL_2"].ToString();
                            tbImageURL3.Text = idr["HEADER_IMAGE_URL_3"].ToString();
                            lblWebDbName.Text = idr["WEBDB_NAME"].ToString();
                        }
                    }
                    idr.Close();
            }
            imgHeaderImage1.ImageUrl = "~/ShowImage1.ashx?id=" + lblWebDB_ID.Text;
            imgHeaderImage2.ImageUrl = "~/ShowImage2.ashx?id=" + lblWebDB_ID.Text;
            imgHeaderImage3.ImageUrl = "~/ShowImage3.ashx?id=" + lblWebDB_ID.Text;
        }
    }

    protected void lbClose_Click(object sender, EventArgs e)
    {
        string scriptString = "";
        Type cstype = this.GetType();
        ClientScriptManager cs = Page.ClientScript;
        scriptString = "<script language=JavaScript>";
        scriptString += "window.close();</script>";

        if (!cs.IsStartupScriptRegistered("Startup"))
            cs.RegisterStartupScript(cstype, "Startup", scriptString);
    }

    protected void cmdUploadImage1_Click(object sender, EventArgs e)
    {
        long lMaxFileSize = 256000;
        lblMessage1.Visible = false;
        if (fDocument1.PostedFile.ContentLength <= lMaxFileSize)
        {
            // check it is actually an image
            try
            {
                System.Drawing.Image newImage = System.Drawing.Image.FromStream(fDocument1.PostedFile.InputStream);

                // resize it to a maximum size
                int image_height = newImage.Height;
                int image_width = newImage.Width;
                int max_height = 90;
                int max_width = 500;
                if (image_height > max_height)
                {
                    image_width = (image_width * max_height) / image_height;
                    image_height = max_height;
                }
                if (image_width > max_width)
                {
                    image_width = max_width;
                    image_height = (image_height * max_width) / image_width;
                }
                else { }
                Bitmap bitmap_file = new Bitmap(newImage, image_width, image_height);
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                stream.Position = 0;
                bitmap_file.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] d2 = stream.ToArray();
                stream.Close();
                bitmap_file.Dispose();

                using (SqlConnection conn = new SqlConnection(Utils.AdmConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("st_WDUploadHeaderImage", conn))
                    {
                        SqlParameter[] spParams = new SqlParameter[3];
                        spParams[0] = new SqlParameter("@IMAGE_NUMBER", SqlDbType.NVarChar, 10, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, "1");
                        spParams[1] = new SqlParameter("@WEBDB_ID", SqlDbType.NVarChar, 200, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, lblWebDB_ID.Text);
                        spParams[2] = new SqlParameter("@IMAGE", SqlDbType.Binary, d2.Length, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, d2);
                        foreach (SqlParameter p in spParams) cmd.Parameters.Add(p);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                lblMessage1.Text = "Not an image";
                lblMessage1.Visible = true;
            }
        }
        else
        {
            lblMessage1.Text = "Image too large";
            lblMessage1.Visible = true;
        }
    }
    protected void cmdUploadImage2_Click(object sender, EventArgs e)
    {
        long lMaxFileSize = 256000;
        lblMessage2.Visible = false;
        if (fDocument2.PostedFile.ContentLength <= lMaxFileSize)
        {
            // check it is actually an image
            try
            {
                System.Drawing.Image newImage = System.Drawing.Image.FromStream(fDocument2.PostedFile.InputStream);

                // resize it to a maximum size
                int image_height = newImage.Height;
                int image_width = newImage.Width;
                int max_height = 90;
                int max_width = 500;
                if (image_height > max_height)
                {
                    image_width = (image_width * max_height) / image_height;
                    image_height = max_height;
                }
                if (image_width > max_width)
                {
                    image_width = max_width;
                    image_height = (image_height * max_width) / image_width;
                }
                else { }
                Bitmap bitmap_file = new Bitmap(newImage, image_width, image_height);
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                stream.Position = 0;
                bitmap_file.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] d2 = stream.ToArray();
                stream.Close();
                bitmap_file.Dispose();

                using (SqlConnection conn = new SqlConnection(Utils.AdmConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("st_WDUploadHeaderImage", conn))
                    {
                        SqlParameter[] spParams = new SqlParameter[3];
                        spParams[0] = new SqlParameter("@IMAGE_NUMBER", SqlDbType.NVarChar, 10, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, "2");
                        spParams[1] = new SqlParameter("@WEBDB_ID", SqlDbType.NVarChar, 200, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, lblWebDB_ID.Text);
                        spParams[2] = new SqlParameter("@IMAGE", SqlDbType.Binary, d2.Length, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, d2);
                        foreach (SqlParameter p in spParams) cmd.Parameters.Add(p);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                lblMessage2.Text = "Not an image";
                lblMessage2.Visible = true;
            }
        }
        else
        {
            lblMessage2.Text = "Image too large";
            lblMessage2.Visible = true;
        }
    }
    protected void lbSaveLink1_Click(object sender, EventArgs e)
    {
        // save what's left
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDUploadHeaderURL", "1", lblWebDB_ID.Text, tbImageURL1.Text);
    }
    protected void lbSaveLink2_Click(object sender, EventArgs e)
    {
        // save what's left
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDUploadHeaderURL", "2", lblWebDB_ID.Text, tbImageURL2.Text);
    }

    protected void lbDeleteImage1_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDDeleteHeaderImage", "1", lblWebDB_ID.Text);
    }
    protected void lbDeleteImage2_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDDeleteHeaderImage", "2", lblWebDB_ID.Text);
    }

    protected void cmdUploadImage3_Click1(object sender, EventArgs e)
    {
        long lMaxFileSize = 256000;
        lblMessage3.Visible = false;
        if (fDocument3.PostedFile.ContentLength <= lMaxFileSize)
        {
            // check it is actually an image
            try
            {
                System.Drawing.Image newImage = System.Drawing.Image.FromStream(fDocument3.PostedFile.InputStream);

                // resize it to a maximum size
                int image_height = newImage.Height;
                int image_width = newImage.Width;
                int max_height = 90;
                int max_width = 500;
                if (image_height > max_height)
                {
                    image_width = (image_width * max_height) / image_height;
                    image_height = max_height;
                }
                if (image_width > max_width)
                {
                    image_width = max_width;
                    image_height = (image_height * max_width) / image_width;
                }
                else { }
                Bitmap bitmap_file = new Bitmap(newImage, image_width, image_height);
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                stream.Position = 0;
                bitmap_file.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] d2 = stream.ToArray();
                stream.Close();
                bitmap_file.Dispose();

                using (SqlConnection conn = new SqlConnection(Utils.AdmConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("st_WDUploadHeaderImage", conn))
                    {
                        SqlParameter[] spParams = new SqlParameter[3];
                        spParams[0] = new SqlParameter("@IMAGE_NUMBER", SqlDbType.NVarChar, 10, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, "3");
                        spParams[1] = new SqlParameter("@WEBDB_ID", SqlDbType.NVarChar, 200, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, lblWebDB_ID.Text);
                        spParams[2] = new SqlParameter("@IMAGE", SqlDbType.Binary, d2.Length, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, d2);
                        foreach (SqlParameter p in spParams) cmd.Parameters.Add(p);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                lblMessage3.Text = "Not an image";
                lblMessage3.Visible = true;
            }
        }
        else
        {
            lblMessage3.Text = "Image too large";
            lblMessage3.Visible = true;
        }
    }
    protected void lbSaveLink3_Click1(object sender, EventArgs e)
    {
        // save what's left
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDUploadHeaderURL", "3", lblWebDB_ID.Text, tbImageURL3.Text);
    }
    protected void lbDeleteImage3_Click1(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDDeleteHeaderImage", "3", lblWebDB_ID.Text);
    }
}