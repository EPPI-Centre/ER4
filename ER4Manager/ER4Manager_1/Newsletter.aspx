<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Newsletter.aspx.cs" Inherits="Newsletter" Title="Newsletter" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

           <script type="text/javascript">

               function openViewNewsletter(newsletterID) {
                   var iWidthOfWin = 780;
                   var iHeightOfWin = 700;
                   var iLocX = (screen.width - iWidthOfWin) / 2;
                   var iLocY = (screen.height - iHeightOfWin) / 3;

                   var strFeatures = "scrollbars=yes"
			           + ",width=" + iWidthOfWin
				       + ",height=" + iHeightOfWin
					   + ",screenX=" + iLocX
			            + ",screenY=" + iLocY
				       + ",left=" + iLocX
					   + ",top=" + iLocY;

                   var theURL = "NewsletterView.aspx?NewsletterID=" + newsletterID;

                   windowName = new String(Math.round(Math.random() * 100000));
                   DetailsWindow = window.open(theURL, windowName, strFeatures);

               }
			
		</script>


        <br />
        <b>View newsletters</b><asp:GridView ID="gvNewsletters" runat="server" AutoGenerateColumns="False" 
                                    EnableModelValidation="True" Width="99%" 
            DataKeyNames="NEWSLETTER_ID" 
               onrowdatabound="gvNewsletters_RowDataBound">
                                    <Columns>
                                        <asp:BoundField DataField="NEWSLETTER_ID" HeaderText="Newsletter ID">
                                        <HeaderStyle BackColor="#B6C6D6" />
                                        <ItemStyle Width="15%" />
                                        </asp:BoundField>
                                        <asp:BoundField DataField="MONTH" HeaderText="Month">
                                        <HeaderStyle BackColor="#B6C6D6" />
                                        <ItemStyle Width="15%" />
                                        </asp:BoundField>
                                        <asp:BoundField DataField="YEAR" HeaderText="Year">
                                        <HeaderStyle BackColor="#B6C6D6" />
                                        <ItemStyle Width="15%" />
                                        </asp:BoundField>
                                        <asp:BoundField DataField="NUMBER_SENT" HeaderText="Number sent">
                                        <HeaderStyle BackColor="#B6C6D6" />
                                        <ItemStyle Width="15%" />
                                        </asp:BoundField>
                                        <asp:TemplateField HeaderText="View in Editor">
                                            <ItemTemplate>
                                                <asp:HyperLink ID="hlViewNewsletter" runat="server">View</asp:HyperLink>
                                            </ItemTemplate>
                                            <HeaderStyle BackColor="#B6C6D6" />
                                            <ItemStyle Width="20%" />
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
        <br />
        <hr />

                                <b>Upload newsletter</b><br />
        <table style="width: 400px">
            <tr>
                <td>
                    <b>Year</b></td>
                <td>
                    <b>Month</b></td>
            </tr>
            <tr>
                <td>
                    <asp:DropDownList ID="ddlYear" runat="server" AutoPostBack="True" 
                        onselectedindexchanged="ddlYear_SelectedIndexChanged">
                        <asp:ListItem Value="0">Select a year...</asp:ListItem>
                        <asp:ListItem Value="2012">2012</asp:ListItem>
                        <asp:ListItem Value="2013">2013</asp:ListItem>
                        <asp:ListItem Value="2015">2014</asp:ListItem>
                        <asp:ListItem Value="2015">2015</asp:ListItem>
                        <asp:ListItem Value="2016">2016</asp:ListItem>
                    </asp:DropDownList>
                </td>
                <td>
                    <asp:DropDownList ID="ddlMonth" runat="server" AutoPostBack="True" 
                        onselectedindexchanged="ddlMonth_SelectedIndexChanged">
                        <asp:ListItem Value="0">Select a month...</asp:ListItem>
                        <asp:ListItem Value="1">January</asp:ListItem>
                        <asp:ListItem Value="2">February</asp:ListItem>
                        <asp:ListItem Value="3">March</asp:ListItem>
                        <asp:ListItem Value="4">April</asp:ListItem>
                        <asp:ListItem Value="5">May</asp:ListItem>
                        <asp:ListItem Value="6">June</asp:ListItem>
                        <asp:ListItem Value="7">July</asp:ListItem>
                        <asp:ListItem Value="8">August</asp:ListItem>
                        <asp:ListItem Value="9">September</asp:ListItem>
                        <asp:ListItem Value="10">October</asp:ListItem>
                        <asp:ListItem Value="11">November</asp:ListItem>
                        <asp:ListItem Value="12">December</asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
        </table>
        <br />
                <input ID="fDocument" runat="server" class="button" name="fDocument" 
                    type="file" />&nbsp;&nbsp;
        <asp:Button 
                    ID="cmdUpload" runat="server" 
                    OnClick="cmdUpload_Click" Text="Upload" Enabled="False" />
                &nbsp;&nbsp;
        <asp:Label ID="lblUploadMessage" runat="server" Font-Bold="True" 
            ForeColor="Red" Text="Please select a year and month" Visible="False"></asp:Label>
        <br />
        <asp:Button 
                    ID="cmdView" runat="server" 
                    OnClick="cmdView_Click" Text="View" Visible="False" />
                <asp:Label ID="lblViewMessage" runat="server" Font-Bold="True" 
            ForeColor="Red" Text="Please select a year and month" Visible="False"></asp:Label>
        <br />
        <br />
        <hr />

        <br />
        <b>Send newsletter<br />
        </b>This will email the most recent newsletter to all eligible recipients who 
        have not yet received that newsletter.
        <br />
        It will be handled in batches with a delay between each individual email.<br />
        <br />
        <table style="width:50%;">
            <tr>
                <td style="width: 50%">
                    <b>Number of emails in batch</b></td>
                <td>
                    <b>Delay in seconds</b></td>
            </tr>
            <tr>
                <td style="width: 50%">
                    <asp:RadioButtonList ID="rblBatch" runat="server">
                        <asp:ListItem Selected="True">100</asp:ListItem>
                        <asp:ListItem>500</asp:ListItem>
                        <asp:ListItem>1000</asp:ListItem>
                        <asp:ListItem Value="2">2 (test)</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
                <td>
                    <asp:RadioButtonList ID="rblDelay" runat="server">   
                        <asp:ListItem Value="00:00:0.500">0.5</asp:ListItem>
                        <asp:ListItem Selected="True" Value="00:00:1.000">1.0</asp:ListItem>
                        <asp:ListItem Value="00:00:2.000">2.0</asp:ListItem>
                        <asp:ListItem Value="00:00:3.000">3.0</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
            </tr>
        </table>
        <br />
        <asp:Button ID="cmdEmailNewsletter" runat="server" 
            onclick="cmdEmailNewsletter_Click" Text="Email newsletter" />
&nbsp;&nbsp;
        <asp:Label ID="lblEmailMessage" runat="server" Font-Bold="True" 
            ForeColor="Red" Text="Please select a year and month" Visible="False"></asp:Label>
        <br />
        To be sent:
        <asp:Label ID="lblToBeSent" runat="server" Font-Bold="True" Text="n/a"></asp:Label>
&nbsp; (number eligible who have not been sent the latest newsletter yet)<br />
        Eligible recipients:
        <asp:Label ID="lblNumEligible" runat="server" Text="n/a" Font-Bold="True"></asp:Label>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp; Number recipients:
        <asp:Label ID="lblNumRecipients" runat="server" Text="n/a" Font-Bold="True"></asp:Label>
        &nbsp; (after removing the email failures)<br />
        <br />
        <hr />

        <br />
        <asp:Button ID="cmdTest" runat="server" onclick="cmdTest_Click" 
            Text="Test email" />
&nbsp;This will send the most recent newsletter to EPPISupport@ucl.ac.uk<br />
                                <br />

    </asp:Content>
