using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Xml.Linq;

public partial class JumpToWPMUCL : System.Web.UI.Page
{
    double Tot = 0, VatTot = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") == null)
        {
            Server.Transfer("Error.aspx");
        }
        else
        {

            //if (Context.Items["lblEmailAddress"] == null || Context.Items["lblName"] == null)
            if (Utils.GetSessionString("lblEmailAddress") == null || Utils.GetSessionString("ContactName") == null || Utils.GetSessionString("Draft_Bill_ID") == null)
                Server.Transfer("Error.aspx");
            else
            {
                xml.Value = MakeXMLRequest().ToString();
                Utils.ExecuteSP(true, Server, "st_BillMarkAsSubmitted", Utils.GetSessionString("Contact_ID"), Utils.GetSessionString("Draft_Bill_ID"), Math.Round(VatTot, 2));
                Utils.SetSessionString("lblEmailAddress", null);
                form1.Action = Utils.UCLWPMServerUrl;
            }
        }
    }

    protected XElement MakeXMLRequest()
    {

        string billID = Utils.GetSessionString("Draft_Bill_ID");
        if (billID == null || billID == "") return null;

        //        string msg = @"<wpmpaymentrequest msgid='000106e097b6d08e5549fcc9feb21e61'>
        //                        <clientid>20023</clientid> 
        //                        <pathwayid>999</pathwayid> 
        //                        <pspid /> 
        //                        <style><![CDATA[filename.css]]></style>
        //                          <departmentid>1</departmentid> 
        //                        <staffid><![CDATA[]]></staffid>
        //                        <customerid><![CDATA[960]]></customerid>
        //                        <title><![CDATA[]]></title>
        //                        <firstname><![CDATA[WPM]]></firstname>
        //                        <middlename><![CDATA[]]></middlename>
        //                        <lastname><![CDATA[TEST]]></lastname>
        //                        <emailfrom><![CDATA[]]></emailfrom>
        //                        <toemail><![CDATA[aboffee@wpmeducation.com]]></toemail>
        //                        <ccemail><![CDATA[]]></ccemail>
        //                        <emailfooter><![CDATA[]]></emailfooter>
        //                        <retryemailto><![CDATA[]]></retryemailto>
        //                        <transactionreference><![CDATA[R960-096268]]></transactionreference >
        //                        <redirecturl><![CDATA[http://redirectURL.com]]></redirecturl>
        //                        <callbackurl><![CDATA[https://callbackURL.com]]></callbackurl>
        //                        <cancelurl><![CDATA[http://cancelURL.com]]></cancelurl>
        //                          <live>1</live> 
        //                          <customfield1 /> 
        //                          <customfield2 /> 
        //                          <customfield3 /> 
        //                          <customfield4 /> 
        //                          <customfield5 /> 
        //                        <payments id='1' type='P' payoption='AD'>
        //                          <customfield1 /> 
        //                          <customfield2 /> 
        //                          <customfield3 /> 
        //                          <customfield4 /> 
        //                          <customfield5 /> 
        //                          <customfield6 /> 
        //                          <customfield7 /> 
        //                          <customfield8 /> 
        //                          <customfield9 /> 
        //                          <customfield10 /> 
        //                        <description>
        //                        <![CDATA[Accommodation Deposit]]></description>
        //                          <mandatory>1</mandatory> 
        //                          <editable minamount='' maxamount=''>0</editable> 
        //                        <payment payid='1'>
        //                          <customfield1 /> 
        //                          <customfield2 /> 
        //                          <customfield3 /> 
        //                          <customfield4 /> 
        //                          <customfield5 /> 
        //                          <amounttopay>125.00</amounttopay> 
        //                          <amounttopayvat>18.62</amounttopayvat> 
        //                          <amounttopayexvat>106.38</amounttopayexvat> 
        //                          <vatrate>17.50</vatrate> 
        //                          <dateofpayment>2010-05-19 10:05:09</dateofpayment> 
        //                          </payment>
        //                          </payments>
        //                        </wpmpaymentrequest>";



        /*
        < clientid > 1111 </ clientid >
        < requesttype > 1 </ requesttype >
        < pathwayid > 2222 </ pathwayid >
        < departmentid > 1 </ departmentid >
        < staffid ></ staffid >
        < customerid >< ![CDATA[20710579]] ></ customerid >
        < title >< ![CDATA[Mr]] ></ title >
        < firstname >< ![CDATA[Godwin]] ></ firstname >
        < middlename >< ![CDATA[James]] ></ middlename >
        < lastname >< ![CDATA[Smith]] ></ lastname >

        < emailfrom >< ![CDATA[swilkins@wpmeducation.com]] ></ emailfrom >
        < toemail >< ![CDATA[astarmer@wpmeducation.com]] ></ toemail >
        < ccemail >< ![CDATA[smuir@wpmeducation.com]] ></ ccemail >
        < emailfooter >< ![CDATA[HELLO]] ></ emailfooter >

        < transactionreference >< ![CDATA[20710579]] ></ transactionreference >
        < redirecturl >< ![CDATA[https://redirecturl.com]]></redirecturl>
        < callbackurl >< ![CDATA[https://callbackurl.com]]></callbackurl>
        < cancelurl >< ![CDATA[https://cancelurl.com]]></cancelurl>

        < billaddress1 >< ![CDATA[52anaddress]] ></ billaddress1 >
        < billaddress2 >< ![CDATA[address2]] ></ billaddress2 >
        < billaddress3 >< ![CDATA[address3]] ></ billaddress3 >
        < billtown >< ![CDATA[BurgessHill]] ></ billtown >
        < billcounty >< ![CDATA[WestSussex]] ></ billcounty >
        < billpostcode >< ![CDATA[BG238HR]] ></ billpostcode >
        < billcountry >< ![CDATA[UnitedKingdom]] ></ billcountry >

        < customfield1 >< ![CDATA[Sean1TEST]] ></ customfield1 >
        < customfield2 >< ![CDATA[Sean2TEST]] ></ customfield2 >
        < customfield3 >< ![CDATA[Sean3TEST]] ></ customfield3 >
        < customfield4 >< ![CDATA[Sean4TEST]] ></ customfield4 >
        < customfield5 >< ![CDATA[Sean5TEST]] ></ customfield5 >
        < customfield6 >< ![CDATA[Sean6TEST]] ></ customfield6 >
        < customfield7 >< ![CDATA[Sean7TEST]] ></ customfield7 >
        < customfield8 >< ![CDATA[Sean8TEST]] ></ customfield8 >
        < customfield9 >< ![CDATA[Sean9TEST]] ></ customfield9 >
        < customfield10 >< ![CDATA[Sean10TEST]] ></ customfield10 >
        */



        string clientID = Utils.UCLWPMclientID;//this should be provided by WPM

        //string msgid = getMD5Hash(clientID + billID + );
        string pathwayid = Utils.UCLWPMpathwayid;//this should come from WPM
        bool isAdmDB = true;
        float VatRate = 0, currentAmount;
        int numberMonths;
        int credit;
        int outstandingFee;

        IDataReader idr = Utils.GetReader(isAdmDB, "st_VatGet",
            Utils.GetSessionString("CountryID"));
        if (idr.Read())
        {
            VatRate = float.Parse(idr["VAT_RATE"].ToString()) / 100;
        }
        else VatRate = 0;
        idr.Close();

        XElement res = new XElement("wpmpaymentrequest");
        XElement tmpX = new XElement("clientid", clientID);
        res.Add(tmpX);
        res.Add(new XElement("requesttype", 1));
        res.Add(new XElement("pathwayid", pathwayid));
        /*res.Add(new XElement("pspid", "1"));*/
        res.Add(new XElement("departmentid", Utils.UCLWPMdepartmentid));//this should come from WPM
        res.Add(new XElement("staffid", ""));//this should come from WPM
        res.Add(new XElement("customerid", new XCData(Utils.GetSessionString("Contact_ID"))));
        res.Add(new XElement("title", ""));
        res.Add(new XElement("firstname", new XCData(Utils.GetSessionString("ContactName"))));
        res.Add(new XElement("middlename", ""));
        res.Add(new XElement("lastname", ""));
        

        res.Add(new XElement("emailfrom", new XCData("EPPISupport@ucl.ac.uk")));
        res.Add(new XElement("toemail", new XCData(Utils.GetSessionString("lblEmailAddress"))));
        res.Add(new XElement("ccemail", new XCData("EPPISupport@ucl.ac.uk")));
        res.Add(new XElement("emailfooter", new XCData("Test Footer")));

        res.Add(new XElement("transactionreference", new XCData(billID)));
        res.Add(new XElement("redirecturl", new XCData(HttpContext.Current.Request.Url.ToString().Replace("JumpToWPMUCL.aspx", "ReturnFromPayment.aspx"))));
        if (Utils.USEproxyIN)
            res.Add(new XElement("callbackurl", new XCData(Utils.ProxyURL)));
        else
            res.Add(new XElement("callbackurl", new XCData(Utils.WPMCallBackURL)));
        res.Add(new XElement("cancelurl", new XCData(HttpContext.Current.Request.Url.ToString().Replace("JumpToWPMUCL.aspx", "ReturnFromPayment.aspx" + "?ID=" + billID))));


        /*
        res.Add(new XElement("billaddress1", "52anaddress"));
        res.Add(new XElement("billaddress2", "address2"));
        res.Add(new XElement("billaddress3", "address3"));
        res.Add(new XElement("billtown", "BurgessHill"));
        res.Add(new XElement("billcounty", "WestSussex"));
        res.Add(new XElement("billpostcode", "BG238HR"));
        res.Add(new XElement("billcountry", "UnitedKingdom"));

        res.Add(new XElement("customfield1", "test c01"));
        res.Add(new XElement("customfield2", "test c02"));
        res.Add(new XElement("customfield3", "test c03"));
        res.Add(new XElement("customfield4", "test c04"));
        res.Add(new XElement("customfield5", "test c05"));
        res.Add(new XElement("customfield6", "test c06"));
        res.Add(new XElement("customfield7", "test c07"));
        res.Add(new XElement("customfield8", "test c08"));
        res.Add(new XElement("customfield9", "test c09"));
        res.Add(new XElement("customfield10", "test c10"));
        */

        //res.Add(new XElement("live", Utils.UCLWPMisLive));

        idr = Utils.GetReader(isAdmDB, "st_BillGetDraft", Utils.GetSessionString("Contact_ID"));
        if (idr.Read())
        {
            //we could get more details on the bill here, think about this

            idr.NextResult(); //second reader returns the bill lines...
            while (idr.Read())
            {
                numberMonths = int.Parse(idr["MONTHS_CREDIT"].ToString());
                currentAmount = float.Parse(idr["COST"].ToString());
                credit = int.Parse(idr["MONTHS_CREDIT"].ToString()) * 5;
                outstandingFee = int.Parse(idr["MONTHS_CREDIT"].ToString()) * 1;
                if (numberMonths == 0 || currentAmount == 0) continue;
                //option 1: if it's an account
                if (idr["TYPE_NAME"].ToString() == "Professional")
                {
                    if (idr["AFFECTED_ID"].ToString() == null || idr["AFFECTED_ID"].ToString() == "")
                    {//it's a ghost account
                        res.Add(buildXMLRequestLine
                            (//string LineID, string description, string AmountEx, string VAT, string VATRate, string AmountInc
                                idr["LINE_ID"].ToString(),
                                "New account: " + numberMonths + " months (bill ID = " + idr["LINE_ID"] + ").",
                                currentAmount.ToString("F2"),
                                (VatRate * currentAmount).ToString("F2"),
                                VatRate.ToString("F2"), (VatRate * currentAmount + currentAmount).ToString("F2")
                           )
                       );
                    }
                    else
                    {//it's an existing account
                        res.Add(buildXMLRequestLine
                                (
                                    idr["LINE_ID"].ToString(),
                                    "Account Extension: Name = "
                                    + (idr["AFFECTED_NAME"] == null || idr["AFFECTED_NAME"].ToString() == "" ? "Not Activated" : idr["AFFECTED_NAME"].ToString())
                                        + ", ID = " + idr["AFFECTED_ID"].ToString() + ", " + numberMonths + " months.",
                                    currentAmount.ToString("F2"),
                                    (VatRate * currentAmount).ToString("F2"),
                                    VatRate.ToString("F2"), (VatRate * currentAmount + currentAmount).ToString("F2")
                               )
                           );
                    }
                }
                else if (idr["TYPE_NAME"].ToString() == "Shareable Review")
                { //option 2: if it's a review
                    if (idr["AFFECTED_ID"].ToString() == null || idr["AFFECTED_ID"].ToString() == "")
                    {//it's a ghost review
                        res.Add(buildXMLRequestLine
                            (
                               idr["LINE_ID"].ToString(),
                                "New Review: " + numberMonths + " months (bill ID = " + idr["LINE_ID"] + ").",
                                currentAmount.ToString("F2"),
                                (VatRate * currentAmount).ToString("F2"),
                                VatRate.ToString("F2"), (VatRate * currentAmount + currentAmount).ToString("F2")
                           )
                       );
                    }
                    else
                    {//it's an existing review

                        res.Add(buildXMLRequestLine
                            (
                                idr["LINE_ID"].ToString(),
                                "Review Extension: Name = "
                                + (idr["AFFECTED_NAME"] == null || idr["AFFECTED_NAME"].ToString() == "" ? "Not Activated" : idr["AFFECTED_NAME"].ToString())
                                        + ", ID = " + idr["AFFECTED_ID"].ToString() + ", " + numberMonths + " months.",
                                currentAmount.ToString("F2"),
                                (VatRate * currentAmount).ToString("F2"),
                                VatRate.ToString("F2"), (VatRate * currentAmount + currentAmount).ToString("F2")
                           )
                       );
                    }
                }
                else if (idr["TYPE_NAME"].ToString() == "Credit purchase")
                { //option 3: it's a credit purchase
                    res.Add(buildXMLRequestLine
                        (
                            idr["LINE_ID"].ToString(),
                            "Credit Purchase: £" + credit + ".00",                          
                            currentAmount.ToString("F2"),
                            (VatRate * credit).ToString("F2"),
                            VatRate.ToString("F2"), (VatRate * credit + credit).ToString("F2")
                        )
                    );
                }
                else if (idr["TYPE_NAME"].ToString() == "Outstanding fee")
                { //option 3: it's a credit purchase
                    res.Add(buildXMLRequestLine
                        (
                            idr["LINE_ID"].ToString(),
                            "Outstanding fee: £" + outstandingFee + ".00",
                            currentAmount.ToString("F2"),
                            (VatRate * outstandingFee).ToString("F2"),
                            VatRate.ToString("F2"), (VatRate * outstandingFee + outstandingFee).ToString("F2")
                        )
                    );
                }
            }
        }
        //        res.RemoveAll();
        //        res.Add(XElement.Parse(@"<wpmpaymentrequest msgid="+ "\"" + ""+ "\"" + @">
        //    <clientid>8185</clientid>
        //    <requesttype>1</requesttype>
        //    <pathwayid>442</pathwayid>
        //    <pspid>1</pspid>
        //    <departmentid>1</departmentid>
        //    <staffid><![CDATA[]]></staffid>
        //    <customerid><![CDATA[960]]></customerid>
        //    <title><![CDATA[]]></title>
        //    <firstname><![CDATA[WPM]]></firstname>
        //    <middlename><![CDATA[]]></middlename>
        //    <lastname><![CDATA[TEST]]></lastname>
        //    <toemail><![CDATA[mchouteau@wpmeducation.com]]></toemail>
        //    <emailfrom><![CDATA[EPPISupport@ioe.ac.uk]]></emailfrom>
        //    <ccemail><![CDATA[]]></ccemail>
        //    <emailfooter><![CDATA[]]></emailfooter>
        //    <transactionreference><![CDATA[26]]></transactionreference>
        //    <customfield1 />
        //    <customfield2 />
        //    <customfield3 />
        //    <customfield4 />
        //    <customfield5 />
        //    <customfield6 />
        //    <customfield7 />
        //    <customfield8 />
        //    <customfield9 />
        //    <customfield10 />
        //    <sha1hashvalue>852b0fff7821cce22ed1b68a071b3281</sha1hashvalue>
        //	<redirecturl><![CDATA[http://www.ecommercegateway.co.uk/WPM_CB/DEV/CPGCB/interfaces/XMLPostReceiver.asp]]></redirecturl>
        //	<callbackurl><![CDATA[http://www.ecommercegateway.co.uk/WPM_CB/DEV/CPGCB/interfaces/XMLPostReceiver.asp?interface=generic]]></callbackurl>
        //	<cancelurl><![CDATA[http://www.ecommercegateway.co.uk/WPM_CB/DEV/CPGCB/interfaces/XMLPostReceiver.asp]]></cancelurl>
        //    <live>0</live>
        //    <payments id="+ "\"" + "1"+ "\"" + " type="+ "\"" + "PN"+ "\"" + " payoption="+ "\"" + "G"+ "\"" + @">
        //        <description><![CDATA[New Student Application Fee Network Deposit]]></description>
        //        <payment payid="+ "\"" + "34"+ "\"" + @">
        //            <amounttopay>125.00</amounttopay>
        //            <amounttopayvat>0.00</amounttopayvat>
        //            <amounttopayexvat>125.00</amounttopayexvat>
        //            <dateofpayment>2010-05-19 10:05:09</dateofpayment>
        //            <mandatory>1</mandatory>
        //            <openamount minamount="+ "\"" + "0"+ "\"" + " maxamount="+ "\"" + "0"+ "\"" + @">0</openamount>
        //        </payment>
        //    </payments>
        //	<payments id="+ "\"" + "2"+ "\"" + " type="+ "\"" + "PN"+ "\"" + " payoption="+ "\"" + "G"+ "\"" + @">
        //		<description><![CDATA[Accommodation Deposit]]></description>
        //		<payment payid="+ "\"" + "34"+ "\"" + @">
        //			<amounttopay>250.00</amounttopay>
        //			<amounttopayvat>0.00</amounttopayvat>
        //			<amounttopayexvat>250.00</amounttopayexvat>
        //			<dateofpayment>2010-05-19 10:05:09</dateofpayment>
        //			<mandatory>1</mandatory>
        //			<openamount minamount="+ "\"" + "0"+ "\"" + " maxamount="+ "\"" + "0"+ "\"" + @">0</openamount>
        //		</payment>
        //	</payments>
        //</wpmpaymentrequest>"));
        string msgid = Utils.getMD5HashUCL(clientID + billID + Math.Round(Tot, 2).ToString("F2"));
        res.Add(new XAttribute("msgid", msgid));
        //string test = getMD5Hash("8185" + "26" + "375.00");
        // test = getMD5Hash("8185" + "26" + "375");
        // test = getMD5Hash("8185" + "26" + "375");
        // test = getMD5Hash("8185" + "26" + "375");
        // test = getMD5Hash("8185" + "26" + "375");
        // test = getMD5Hash("8185" + "26" + "375");
        // test = getMD5Hash("8185" + "26" + "375");

        //Console.WriteLine(test);
        return res;
    }
    protected XElement buildXMLRequestLine(string LineID, string description, string AmountEx, string VAT, string VATRate, string AmountInc)
    {// what is payoption ????
        Tot += Math.Round(double.Parse(AmountInc), 2);
        VatTot += Math.Round(double.Parse(VAT), 2);
        XElement res = new XElement("payments", new XAttribute("id", LineID), new XAttribute("type", "PN"), new XAttribute("payoption", "G"));
        //res.Add(new XElement("customfield1", ""));
        //res.Add(new XElement("customfield2", ""));
        //res.Add(new XElement("customfield3", ""));
        //res.Add(new XElement("customfield4", ""));
        //res.Add(new XElement("customfield5", ""));
        //res.Add(new XElement("customfield6", ""));
        //res.Add(new XElement("customfield7", ""));
        //res.Add(new XElement("customfield8", ""));
        //res.Add(new XElement("customfield9", ""));
        //res.Add(new XElement("customfield10", ""));
        res.Add(new XElement("description", new XCData(description)));
        XElement subEl = new XElement("payment", new XAttribute("payid", "1"));
        //subEl.Add(new XElement("customfield1", ""));
        //subEl.Add(new XElement("customfield2", ""));
        //subEl.Add(new XElement("customfield3", ""));
        //subEl.Add(new XElement("customfield4", ""));
        //subEl.Add(new XElement("customfield5", ""));
        subEl.Add(new XElement("amounttopay", AmountInc));
        subEl.Add(new XElement("amounttopayvat", VAT == "0" ? "" : VAT));
        subEl.Add(new XElement("amounttopayexvat", AmountEx));
        //subEl.Add(new XElement("vatrate", VATRate == "0" ? "" : VATRate));

        subEl.Add(new XElement("dateofpayment", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));       
        subEl.Add(new XElement("editable", 0, new XAttribute("minamount", "0"), new XAttribute("maxamount", "0")));
        subEl.Add(new XElement("mandatory", "1"));
        res.Add(subEl);
        return res;
    }


}