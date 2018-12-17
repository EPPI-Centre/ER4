<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="MessageOfTheDay.aspx.cs" Inherits="MessageOfTheDay" Title="MessageOfTheDay" %>

    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

        <script language="javascript" type="text/javascript">
            var DetailsWindow = null;

            function openCalendar1(date) {
                var iWidthOfWin = 270;
                var iHeightOfWin = 290;
                var iLocX = (screen.width - iWidthOfWin) / 2;
                var iLocY = (screen.height - iHeightOfWin) / 2;

                var strFeatures = "scrollbars=yes,self.focus()"
                 + ",width=" + iWidthOfWin
                 + ",height=" + iHeightOfWin
                 + ",screenX=" + iLocX
                 + ",screenY=" + iLocY
                 + ",left=" + iLocX
                 + ",top=" + iLocY;

                var theURL = "Calendar_window_1.aspx?date=" + date;
                windowName = new String(Math.round(Math.random() * 100000));
                DetailsWindow = window.open(theURL, windowName, strFeatures);
            }
    </script>

        <div style="max-width:800px">
        <asp:Button ID="cmdPlaceDate" runat="server" BackColor="White" 
                BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px" 
                OnClick="cmdPlaceDate_Click" style="font-weight: bold" Width="1px" />

                                <br />
                                Enter your message. Place a ! at the start of the message to highlight it in 
                                yellow.<br />
                                <asp:TextBox ID="tbMessage" runat="server" Width="90%"></asp:TextBox>
                                <br />
                                <asp:Button ID="cmdSave" runat="server" onclick="cmdSave_Click" Text="Save" />
&nbsp; To save the message and publish it click <b>Save</b><br />
                                <br />
                                <asp:GridView ID="gvMessage" runat="server" AutoGenerateColumns="False" 
                                    EnableModelValidation="True" Width="99%">
                                    <Columns>
                                        <asp:BoundField DataField="MESSAGE" HeaderText="Message">
                                        <HeaderStyle BackColor="#B6C6D6" />
                                        </asp:BoundField>
                                        <asp:BoundField DataField="INSERT_TIME" HeaderText="Insert time">
                                        <HeaderStyle BackColor="#B6C6D6" />
                                        </asp:BoundField>
                                    </Columns>
                                </asp:GridView>
                                <br />
                                <br />
            <b>Adjust Expiry dates&nbsp;&nbsp; </b>
            <asp:Label ID="lblMessage" runat="server" Visible="False"></asp:Label>
                                <asp:Panel ID="Panel1" runat="server" BorderStyle="Solid" 
                BorderWidth="1px">
                                    <br />
                                    <table style="width:100%;">
                                        <tr>
                                            <td style="width: 132px">
                                                <asp:RadioButtonList ID="rblAddRemove" runat="server" 
                                                    RepeatDirection="Horizontal">
                                                    <asp:ListItem Selected="True">Add</asp:ListItem>
                                                    <asp:ListItem>Remove</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                            <td style="width: 151px">
                                                &nbsp;&nbsp;
                                                <asp:TextBox ID="tbDays" runat="server" style="margin-left: 0px"></asp:TextBox>
                                                &nbsp; </td>
                                            <td style="width: 188px">
                                                days for accounts and reviews
                                                <br />
                                                not expired as of</td>
                                            <td style="width: 225px">
                                                &nbsp;&nbsp;
                                                <asp:TextBox ID="tbDate" runat="server"></asp:TextBox>
                                                &nbsp;&nbsp;&nbsp;<asp:ImageButton ID="IBCalendar1" runat="server" 
                                                    ImageUrl="~/images/calbtn.gif" />
                                                &nbsp;&nbsp;
                                            </td>
                                            <td>
                                                &nbsp;
                                                <asp:Button ID="cmdGo" runat="server" onclick="cmdGo_Click" Text="Go" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="5">
                                                &nbsp;&nbsp;
                                                <asp:TextBox ID="tbNotes" runat="server" Width="431px"></asp:TextBox>
                                                &nbsp; Notes (optional)</td>
                                        </tr>
                                    </table>
                                    &nbsp;&nbsp; All extensions will be shown as &#39;Maintenance&#39;<br />
                                    <br />
                                </asp:Panel>
                                <br />
                                <br />
                                <asp:Panel ID="pnlTest" runat="server" Visible="False">
                                    <asp:TextBox ID="tbBillID" runat="server">Enter bill ID</asp:TextBox>
                                    &nbsp;&nbsp;&nbsp;
                                    <asp:Button ID="cmdMarkBillAsPaid" runat="server" 
                                        onclick="cmdMarkBillAsPaid_Click" Text="Mark Bill as paid" />
                                </asp:Panel>
                                <br />
                                <br />

                                <br />

                                </div>
</asp:Content>
