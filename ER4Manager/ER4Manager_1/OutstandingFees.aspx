<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPage.master" CodeFile="OutstandingFees.aspx.cs" Inherits="OutstandingFees" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    
    <script language="javascript" type="text/javascript">

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


        function openReviewerList(ID) {
            var iWidthOfWin = 800;
            var iHeightOfWin = 450;
            var iLocX = (screen.width - iWidthOfWin) / 2;
            var iLocY = (screen.height - iHeightOfWin) / 2;

            var strFeatures = "scrollbars=yes,self.focus(), resizable=yes "
                + ",width=" + iWidthOfWin
                + ",height=" + iHeightOfWin
                + ",screenX=" + iLocX
                + ",screenY=" + iLocY
                + ",left=" + iLocX
                + ",top=" + iLocY;

            var theURL = "SelectReviewer.aspx?contact=" + ID;
            windowName = new String(Math.round(Math.random() * 100000));
            DetailsWindow = window.open(theURL, windowName, strFeatures);
        }
    </script>


    <strong>
        <asp:LinkButton ID="lbNewOutstandingFee" runat="server"
            OnClick="lbNewOutstandingFee_Click" Text="New outstanding fee" Visible="true"></asp:LinkButton>
    </strong>

    <asp:Panel ID="pnlOutstandingFeeDetails" runat="server" Visible="false">
        <asp:Label runat="server" ID="lblPanelText" Text="Please enter details" Font-Bold="True"></asp:Label>&nbsp;&nbsp;
        <asp:LinkButton ID="lbHideDetails" runat="server"
            OnClick="lbHideDetails_Click" Text="(cancel)"></asp:LinkButton>
        
        <table style="width: 100%;" bgcolor="WhiteSmoke">
            <tr>
                <td colspan="1" align="left" style="border-left: medium solid #C0C0C0; border-right: medium none #C0C0C0; border-top: medium solid #C0C0C0; border-bottom: medium none #C0C0C0; padding: 6px; width: 20%; height: 51px;">
                    <b>Outstanding fee ID</b><br />
                    <asp:Label ID="lblOutstandingFeeID" runat="server" Text="N/A"></asp:Label>
                </td>

                <td colspan="2" align="left" style="border-left: medium none #C0C0C0; border-right: medium none #C0C0C0; border-top: medium solid #C0C0C0; border-bottom: medium none #C0C0C0; padding: 6px; width: 40%; height: 51px;" class="w-50">
                    <b>User account</b>
                    <asp:LinkButton ID="lbSelectAccount"
                        runat="server" Visible="True" Text="(select)"></asp:LinkButton>
                    &nbsp;&nbsp;
                    <asp:LinkButton ID="lbLogInAs" runat="server" OnClick="lbLogInAs_Click" Text="Login in as" Visible="False"></asp:LinkButton>
                    <br />
                    <asp:Label ID="lblAccountID" runat="server" Text="N/A"></asp:Label>&nbsp;&nbsp;
                    <asp:Label ID="lblAccountName" runat="server" Text="N/A"></asp:Label>&nbsp;&nbsp;
                    (<asp:Label ID="lblAccountEmail" runat="server" Text="N/A"></asp:Label>)
                </td>

                <td colspan="1" align="left" style="border-left: medium none #C0C0C0; border-right: medium none #C0C0C0; border-top: medium solid #C0C0C0; border-bottom: medium none #C0C0C0; padding: 6px; width: 30%; height: 51px;">
                    <b>Outstanding fee (in £5 increments)</b><br />
                    <asp:TextBox ID="tbOutstandingFee" runat="server" Width="90%"></asp:TextBox>
                </td>
                <td colspan="1" style="border-left: medium none #C0C0C0; border-right: medium solid #C0C0C0; border-top: medium solid #C0C0C0; border-bottom: medium none #C0C0C0; padding: 6px; width: 10%; vertical-align: middle; height: 51px;"></td>
            </tr>
            <tr>
                <td colspan="3" style="border-width: medium; border-color: #C0C0C0; border-style: none none none solid; padding: 6px; vertical-align: middle;">
                    <b>Notes (optional)</b><br />
                    <asp:TextBox runat="server" ID="tbOutstandingFeeNotes" Width="95%" Rows="1"></asp:TextBox>
                    
                </td>
                <td colspan="1" style="padding: 6px; vertical-align: middle;">
                    <b>Date</b>
                    <br />
                    <asp:TextBox ID="tbDateGenerated" runat="server" CssClass="Textbox" width="90%"></asp:TextBox>         
                </td>
                <td colspan="1" style="border-width: medium; border-color: #C0C0C0; border-style: none solid none none; padding: 6px; vertical-align: bottom;">
                    <asp:ImageButton ID="IBCalendar1" runat="server" 
                            ImageUrl="~/images/calbtn.gif" />
                </td>



            </tr>

            <tr>
                <td colspan="1" style="border-width: medium; border-color: #C0C0C0; padding: 6px; border-style: none none solid solid;">
                    <asp:Button ID="cmdSaveNewOutstandingFee" runat="server" Enabled="True" Text="Save" OnClick="cmdSaveNewOutstandingFee_Click" style="height: 21px" />
                    &nbsp;&nbsp;
                    <asp:CheckBox ID="cbWithEmail" runat="server" Text="with email" />                 
                </td>
                <td colspan="1" style="border-width: medium; border-color: #C0C0C0; padding: 6px; width: 15%; border-style: none none solid none;">
                    <asp:Label runat="server" ID="lblOutstandingFeeError" Visible="false" Font-Bold="True" ForeColor="Red" Text="Missing data"></asp:Label>
                </td>
                <td colspan="1" style="border-width: medium; border-color: #C0C0C0; padding: 6px; width: 25%; border-style: none none solid none;">
                    Status:
                    <asp:Label ID="lblStatus" runat="server" Text="Outstanding"></asp:Label>
                </td>
                <td colspan="1" style="border-width: medium; border-color: #C0C0C0; padding: 6px; border-style: none none solid none;">               
                    <asp:Button ID="cmdDeleteOutstandingFee" runat="server" Enabled="True" OnClick="cmdDeleteOutstandingFee_Click" Text="Delete" Visible="False" />
                   &nbsp;                    
                </td>
                <td colspan="1" style="border-width: medium; border-color: #C0C0C0; border-style: none solid solid none; padding: 6px; vertical-align: middle;">                   
                </td>
                
            </tr>
        </table>
    </asp:Panel>



    <br />
    <table style="width: 100%;">
        <tr>
            <td align="left" style="width: 50%">
                <b>Outstanding fees</b>&nbsp;&nbsp;<asp:Label runat="server" ID="lblEmailSentMsg" Visible="false" Font-Bold="True" ForeColor="#00CC00" Text="Email sent"></asp:Label>
            </td>

            <td align="right" width="50%">&nbsp;&nbsp;&nbsp; *Find by name&nbsp;&nbsp;
                        <asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();"></asp:TextBox>
            </td>
        </tr>
    </table>

    <telerik:RadGrid ID="radGVOutstandingFees" runat="server"
        CssClass="Grid" Skin="Windows7" AllowPaging="True" AutoGenerateColumns="False"
        Height="350px" ResolvedRenderMode="Classic" Width="800px"
        OnItemDataBound="radGVOutstandingFees_ItemDataBound"
        OnItemCommand="radGVOutstandingFees_ItemCommand"
        AllowSorting="True"
        OnNeedDataSource="radGVOutstandingFees_NeedDataSource"
        OnPageIndexChanged="radGVOutstandingFees_PageIndexChanged"
        PageSize="12">

        <ClientSettings>
            <Resizing AllowColumnResize="True" AllowRowResize="false" ResizeGridOnColumnResize="false"
                ClipCellContentOnResize="true" EnableRealTimeResize="false"
                AllowResizeToFit="true" />
            <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
            <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
            <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
            <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
            <Resizing AllowColumnResize="True" AllowResizeToFit="True"></Resizing>
        </ClientSettings>
        <PagerStyle Mode="NumericPages"
            PagerTextFormat="{4} Page {0} from {1}, rows {2} to {3} from {5}" />
        <MasterTableView TableLayout="Fixed">
            <Columns>

                <telerik:GridBoundColumn DataField="OUTSTANDING_FEE_ID"
                    FilterControlAltText="Filter ContactID column" HeaderText="Purchase ID"
                    UniqueName="InvoicedCreditPurchaseID" DataFormatString="<nobr>{0}</nobr>"
                    Resizable="false" AllowFiltering="False" DataType="System.Int32">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="75px" />
                </telerik:GridBoundColumn>

                

                <telerik:GridBoundColumn DataField="CONTACT_NAME"
                    FilterControlAltText="Filter name column" HeaderText="*Name"
                    UniqueName="name" DataFormatString="<nobr>{0}</nobr>"
                    Resizable="False" AllowFiltering="True">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="150px" />
                </telerik:GridBoundColumn>


                <telerik:GridBoundColumn DataField="EMAIL"
                    FilterControlAltText="Filter email column" HeaderText="*Email"
                    UniqueName="email" DataFormatString="<nobr>{0}</nobr>"
                    Resizable="False" AllowFiltering="False">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="200px" />
                </telerik:GridBoundColumn>

                <telerik:GridBoundColumn DataField="DATE_CREATED"
                    DataFormatString="&lt;nobr&gt;{0:dd/MM/yyyy}&lt;/nobr&gt;"
                    AllowFiltering="False" HeaderText="Date created" Resizable="False"
                    UniqueName="datePurchased" DataType="System.DateTime"
                    FilterControlAltText="Filter column column">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="90px"></HeaderStyle>
                </telerik:GridBoundColumn>

                <telerik:GridBoundColumn DataField="OUTSTANDING_FEE"
                    FilterControlAltText="Filter column2 column" HeaderText="Fee (£)"
                    UniqueName="column2" DataFormatString="<nobr>{0}</nobr>"
                    Resizable="False" AllowFiltering="False" DataType="System.Int16">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="70px" />
                </telerik:GridBoundColumn>

                <telerik:GridBoundColumn DataField="STATUS"
                    FilterControlAltText="Filter column2 column" HeaderText="Status"
                    UniqueName="column2" DataFormatString="<nobr>{0}</nobr>"
                    Resizable="False" AllowFiltering="False" DataType="System.Int16">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="70px" />
                </telerik:GridBoundColumn>


                <telerik:GridTemplateColumn DataField="DETAILS" HeaderText="Details"
                    UniqueName="details" Resizable="True" AllowFiltering="False">
                    <ItemTemplate>
                        <asp:HyperLink ID="Details" runat="server" Text='<%# Bind("details") %>'
                            NavigateUrl='<%# "~/OutstandingFees.aspx?id=" + Eval("OUTSTANDING_FEE_ID")  %>'
                            DataFormatString="<nobr>{0}</nobr>">HyperLink</asp:HyperLink>
                    </ItemTemplate>
                    <HeaderStyle Width="50px" BackColor="#B6C6D6" ForeColor="Black" />
                </telerik:GridTemplateColumn> 


            </Columns>
        </MasterTableView>
    </telerik:RadGrid>

    <asp:Button ID="cmdAddContact" runat="server" BackColor="White"
        BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px"
        OnClick="cmdAddContact_Click" Style="font-weight: bold" Width="1px" />
    <asp:Button ID="cmdPlaceDate" runat="server" BackColor="White"
        BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px"
        OnClick="cmdPlaceDate_Click" Style="font-weight: bold" Width="1px" />

    <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server" OnAjaxRequest="RadAjaxManager1_AjaxRequest">
        <AjaxSettings>
            <telerik:AjaxSetting AjaxControlID="radGVInvoicedCreditPurchases">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="radGVInvoicedCreditPurchases" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>

            <telerik:AjaxSetting AjaxControlID="RadAjaxManager1">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="radGVInvoicedCreditPurchases" LoadingPanelID="RadAjaxLoadingPanel1" />
                </UpdatedControls>
            </telerik:AjaxSetting>
        </AjaxSettings>
    </telerik:RadAjaxManager>


    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server" Height="75px"
        Width="75px" Transparency="25">
    </telerik:RadAjaxLoadingPanel>


    <telerik:RadCodeBlock ID="RadCodeBlock1" runat="server">
        <script type="text/javascript">
            var timer = null;

            function KeyUp() {
                if (timer != null) {
                    clearTimeout(timer);
                }
                timer = setTimeout(LoadTable, 500);
            }

            function LoadTable() {
                $find("<%= RadAjaxManager1.ClientID %>").ajaxRequest("FilterGrid");
            }
        </script>
    </telerik:RadCodeBlock>


    </asp:Content>
