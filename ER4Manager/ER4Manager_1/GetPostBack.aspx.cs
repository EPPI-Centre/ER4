using System;

using System.Data;
using System.Xml.Linq;

public partial class GetPostBack : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.ContentLength != 0)
        {
            System.IO.Stream str; String strmContents;
            Int32 strLen, strRead;
            // Create a Stream object.
            str = Request.InputStream;
            // Find number of bytes in stream.
            strLen = Convert.ToInt32(str.Length);
            // Create a byte array.
            byte[] strArr = new byte[strLen];
            // Read stream into byte array.
            strRead = str.Read(strArr, 0, strLen);
            // Convert byte array to a text string.
            strmContents = "";
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            strmContents = encoder.GetString(strArr);
            

            if (VerifyReqSource() && strmContents.IndexOf("<wpmpaymentrequest") == 0)// && Request.UserHostAddress != "sbirla")
            {//look at the XMl content to see if it matches an existing bill, if it does, action the bill and return.
                //get the billID, without it, there is little you can do
                string billID, msgid;
                XElement Xe;
                try
                {
                    Xe = XElement.Parse(strmContents);
                    billID = Xe.Element("transactionreference").Value;
                    msgid = Xe.Attribute("msgid").Value;
                }
                catch
                {
                    Server.Transfer("Error.aspx");
                    return;
                }//don't know yet where to save the error data!
                if (VerifyWPMPostback(strmContents))
                {//mark bill as paid and do the changes
                    WPMValidateAnswer(msgid, true, true);
                    Utils.ExecuteSP(true, Server, "st_BillMarkAsPaid", billID);
                    //WPMSuccess(sender, e);
                }
                else
                {//mark bill as failed
                    if (msgid != null && msgid != "")
                    {
                        WPMValidateAnswer(msgid, true, false);
                        if (billID != null && billID != "")
                            Utils.ExecuteSP(true, Server, "st_BillMarkAsFailed", billID, "FAILED: Data from WPM doesn't match bill");
                    }
                }
            }
            else { Server.Transfer("Error.aspx"); }
        }
        //no matter what, if we are getting a request for this page from an unauthenticated client, always transfer to the generic error. 
        else Server.Transfer("Error.aspx");
    }
    protected bool VerifyReqSource()
    {
        if (Request == null || Request.UserHostAddress == null || Request.UserHostAddress == "") return false;
        if (Request.UserHostAddress == "144.82.33.83" && Utils.USEproxyIN) return true;
        string[] dec = Request.UserHostAddress.Split('.');
        if (dec.Length != 4) return false;
        if (dec[0] == "213" && dec[1] == "206" && (dec[2] == "140" || dec[2] == "141" || dec[2] == "142" || dec[2] == "143")) return true;
        return false;
    }
    protected bool VerifyWPMPostback(string content)
    {
        try
        {
            XElement Xe = XElement.Parse(content);
            string contactID = Xe.Element("customerid").Value, billID = Xe.Element("transactionreference").Value, msgid = Xe.Attribute("msgid").Value;
            //first check: see if msgid computes as the hash of contactid+billID
            IDataReader idr = Utils.GetReader(true, "st_BillGetSubmitted", contactID, billID);
            if (!idr.Read()) return false;//if no bill is found, return insuccess
            double tot = Math.Round(double.Parse(idr["DUE_PRICE"].ToString()), 2) + Math.Round(double.Parse(idr["VAT"].ToString()), 2);
            idr.Close();
            if (msgid != Utils.getMD5HashUCL(Utils.UCLWPMclientID + billID + tot.ToString("F2"))) return false;
            //third, match bill with postback
            XElement X2 = Xe.Element("transaction");
            double wpmPaid = double.Parse(X2.Element("totalpaid").Value);
            if (wpmPaid >= tot) return true;
            else return false;
        }
        catch
        { return false; }
    }
    protected void WPMValidateAnswer(string msgid, bool isValid, bool isSucces)
    {

        XElement res = new XElement("wpmmessagevalidation", new XAttribute("msgid", msgid));
        res.Add(new XElement("validation", isValid ? "1" : "0"));
        res.Add(new XElement("validationmessage", new XCData(isSucces ? "Success" : "Failure: data from WMP does not match our records")));
        string msg = res.ToString();
        this.Response.Write(msg);
        //string url = Utils.WPMServerUrl;
        //HttpResponse resp = HttpResponse.Create(url) as HttpWebRequest;
        //request.Method = "POST";
        //request.ContentType = "application/x-www-form-urlencoded";
        ////The encoding might have to be chaged based on requirement
        //msg = HttpUtility.UrlEncode(msg);
        //System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
        ////msg = "xml=" + msg;
        //byte[] data = encoder.GetBytes(msg); //postbody is plain string of xml
        ////byte[] data = HttpUtility.UrlEncodeToBytes(msg, encoder);
        //request.ContentLength = data.Length;
        ////add session cookie
        ////CookieContainer ckc = new CookieContainer(1);
        ////HttpCookie co = Request.Cookies[0];
        ////Cookie coo = new Cookie(co.Name, co.Value);
        ////coo.Domain = "epay.ioe.ac.uk";
        ////ckc.Add(coo);
        ////request.CookieContainer = ckc;
        ////end of add session cookie
        //Stream reqStream = request.GetRequestStream();//add into a try to handle "server not found!" and timeouts
        //reqStream.Write(data, 0, data.Length);
        //reqStream.Close();
        //System.Net.WebResponse response = request.GetResponse();//should go into the same try to handle timeouts!!!!
        //System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());
        //string str = reader.ReadToEnd();
        //Console.Write(str);
    }
}