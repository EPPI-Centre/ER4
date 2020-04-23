<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="PurchaseCredit.aspx.cs" Inherits="PurchaseCredit" %>

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
        <asp:LinkButton ID="lbNewInvoicedCreditPurchase" runat="server"
            OnClick="lbNewInvoicedCreditPurchase_Click" Text="New invoiced credit purchase" Visible="true"></asp:LinkButton>
    </strong>

    <asp:Panel ID="pnlPurchaseDetails" runat="server" Visible="false">
        <asp:Label runat="server" ID="lblPanelText" Text="Please enter details" Font-Bold="True"></asp:Label>&nbsp;&nbsp;
        <asp:LinkButton ID="lbHideDetails" runat="server"
            OnClick="lbHideDetails_Click" Text="(cancel)"></asp:LinkButton>
        <table style="width: 100%;" bgcolor="WhiteSmoke">
            <tr>
                <td colspan="1" align="left" style="border-left: medium solid #C0C0C0; border-right: medium none #C0C0C0; border-top: medium solid #C0C0C0; border-bottom: medium none #C0C0C0; padding: 6px; width: 20%; height: 51px;">
                    <b>Purchase ID</b><br />
                    <asp:Label ID="lblPurchaseID" runat="server" Text="N/A"></asp:Label>
                </td>

                <td colspan="1" align="left" style="border-left: medium none #C0C0C0; border-right: medium none #C0C0C0; border-top: medium solid #C0C0C0; border-bottom: medium none #C0C0C0; padding: 6px; width: 50%; height: 51px;" class="w-50">
                    <b>Purchaser</b>
                    <asp:LinkButton ID="lbAddPurchaser"
                        runat="server" Visible="True" Text="(select)"></asp:LinkButton>
                    &nbsp;<br />
                    <asp:Label ID="lblPurchaserID" runat="server" Text="N/A"></asp:Label>&nbsp;&nbsp;
                    <asp:Label ID="lblPurchaserName" runat="server" Text="N/A"></asp:Label>&nbsp;&nbsp;
                    (<asp:Label ID="lblPurchaserEmail" runat="server" Text="N/A"></asp:Label>)
                </td>

                <td colspan="1" align="left" style="border-left: medium none #C0C0C0; border-right: medium none #C0C0C0; border-top: medium solid #C0C0C0; border-bottom: medium none #C0C0C0; padding: 6px; width: 30%; height: 51px;">
                    <b>Credit purchased (in £5 increments)</b><br />
                    <asp:TextBox ID="tbCreditPurchased" runat="server" Width="90%"></asp:TextBox>
                </td>
                <td colspan="1" style="border-left: medium none #C0C0C0; border-right: medium solid #C0C0C0; border-top: medium solid #C0C0C0; border-bottom: medium none #C0C0C0; padding: 6px; vertical-align: middle; height: 51px;"></td>
            </tr>
            <tr>
                <td colspan="2" style="border-width: medium; border-color: #C0C0C0; border-style: none none none solid; padding: 6px; vertical-align: middle;">
                    <b>Notes (optional)</b><br />
                    <asp:TextBox runat="server" ID="tbPurchaseNotes" Width="95%" Rows="1"></asp:TextBox>
                    
                </td>
                <td colspan="1" style="padding: 6px; vertical-align: middle;">
                    <b>Date</b>
                    <br />
                    <asp:TextBox ID="tbDatePurchased" runat="server" CssClass="Textbox" width="90%"></asp:TextBox>         
                </td>
                <td colspan="1" style="border-width: medium; border-color: #C0C0C0; border-style: none solid none none; padding: 6px; vertical-align: bottom;">
                    <asp:ImageButton ID="IBCalendar1" runat="server" 
                            ImageUrl="~/images/calbtn.gif" />
                </td>



            </tr>

            <tr>
                <td colspan="2" style="border-width: medium; border-color: #C0C0C0; padding: 6px; border-style: none none solid solid;">
                    <asp:Button ID="cmdSaveNewPurchaser" runat="server" Enabled="True" Text="Save" OnClick="cmdSaveNewPurchaser_Click" />
                    &nbsp;&nbsp;
                    <asp:Label runat="server" ID="lblPurchaserError" Font-Bold="True" Visible="false" ForeColor="Red" Text="Missing data"></asp:Label>
                   
                </td>
                <td colspan="1" style="border-width: medium; border-color: #C0C0C0; padding: 6px; border-style: none none solid none;">                    
                    <b>Remaining</b>: £
                    <asp:Label ID="lblRemaining" runat="server" Text="N/A"></asp:Label>                    
                </td>
                <td colspan="1" style="border-width: medium; border-color: #C0C0C0; border-style: none solid solid none; padding: 6px; vertical-align: middle;"></td>
                
            </tr>
        </table>
    </asp:Panel>



    <br />
    <table style="width: 100%;">
        <tr>
            <td align="left" style="width: 20%">
                <b>Credit purchases</b>
            </td>
            <td>
                <asp:RadioButtonList runat="server" ID="rblCreditType" RepeatDirection="Horizontal" AutoPostBack="True" OnSelectedIndexChanged="rblCreditType_SelectedIndexChanged">
                    <asp:ListItem Value="Invoice">Invoice</asp:ListItem>
                    <asp:ListItem>Shop</asp:ListItem>
                    <asp:ListItem Selected="True">Both</asp:ListItem>
                </asp:RadioButtonList>
            </td>
            <td align="right" width="50%">&nbsp;&nbsp;&nbsp; *Find by name&nbsp;&nbsp;
                        <asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();"></asp:TextBox>
            </td>
        </tr>
    </table>

    <telerik:RadGrid ID="radGVInvoicedCreditPurchases" runat="server"
        CssClass="Grid" Skin="Windows7" AllowPaging="True" AutoGenerateColumns="False"
        Height="350px" ResolvedRenderMode="Classic" Width="800px"
        OnItemDataBound="radGVInvoicedCreditPurchases_ItemDataBound"
        OnItemCommand="radGVInvoicedCreditPurchases_ItemCommand"
        AllowSorting="True"
        OnNeedDataSource="radGVInvoicedCreditPurchases_NeedDataSource"
        OnPageIndexChanged="radGVInvoicedCreditPurchases_PageIndexChanged"
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

                <telerik:GridBoundColumn DataField="CREDIT_PURCHASE_ID"
                    FilterControlAltText="Filter ContactID column" HeaderText="Purchase ID"
                    UniqueName="InvoicedCreditPurchaseID" DataFormatString="<nobr>{0}</nobr>"
                    Resizable="false" AllowFiltering="False" DataType="System.Int32">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="75px" />
                </telerik:GridBoundColumn>

                

                <telerik:GridBoundColumn DataField="PURCHASER_CONTACT_NAME"
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

                <telerik:GridBoundColumn DataField="DATE_PURCHASED"
                    DataFormatString="&lt;nobr&gt;{0:dd/MM/yyyy}&lt;/nobr&gt;"
                    AllowFiltering="False" HeaderText="Date purchased" Resizable="False"
                    UniqueName="datePurchased" DataType="System.DateTime"
                    FilterControlAltText="Filter column column">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="90px"></HeaderStyle>
                </telerik:GridBoundColumn>

                <telerik:GridBoundColumn DataField="CREDIT_PURCHASED"
                    FilterControlAltText="Filter column2 column" HeaderText="Purchased (£)"
                    UniqueName="column2" DataFormatString="<nobr>{0}</nobr>"
                    Resizable="False" AllowFiltering="False" DataType="System.Int16">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="70px" />
                </telerik:GridBoundColumn>


                <telerik:GridTemplateColumn DataField="DETAILS" HeaderText="Details"
                    UniqueName="details" Resizable="True" AllowFiltering="False">
                    <ItemTemplate>
                        <asp:HyperLink ID="Details" runat="server" Text='<%# Bind("details") %>'
                            NavigateUrl='<%# "~/PurchaseCredit.aspx?id=" + Eval("CREDIT_PURCHASE_ID")  %>'
                            DataFormatString="<nobr>{0}</nobr>">HyperLink</asp:HyperLink>
                    </ItemTemplate>
                    <HeaderStyle Width="50px" BackColor="#B6C6D6" ForeColor="Black" />
                </telerik:GridTemplateColumn> 

                <telerik:GridBoundColumn DataField="CREDIT_REMAINING"
                    FilterControlAltText="Filter column3 column" HeaderText="Remaining (£)"
                    UniqueName="column3" DataFormatString="<nobr>{0}</nobr>"
                    Resizable="False" AllowFiltering="False" DataType="System.Int16" Visible="false">
                    <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="50px" />
                </telerik:GridBoundColumn>


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

