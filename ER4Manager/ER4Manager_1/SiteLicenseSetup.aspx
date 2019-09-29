<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="SiteLicenseSetup.aspx.cs" Inherits="SiteLicenseSetup" Title="Setup" %>
    <%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        
        <script language="javascript" type="text/javascript">
    var DetailsWindow = null;
    
    function openCalendar1(date)
	{
		var iWidthOfWin = 270;
		var iHeightOfWin = 290;
		var iLocX = ( screen.width - iWidthOfWin ) / 2;
		var iLocY = ( screen.height - iHeightOfWin ) / 2;
		
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

function openAdminList(ID) 
{
    var iWidthOfWin = 800;
    var iHeightOfWin = 400;
    var iLocX = (screen.width - iWidthOfWin) / 2;
    var iLocY = (screen.height - iHeightOfWin) / 2;

    var strFeatures = "scrollbars=yes,self.focus(), resizable=yes "
                 + ",width=" + iWidthOfWin
                 + ",height=" + iHeightOfWin
                 + ",screenX=" + iLocX
                 + ",screenY=" + iLocY
                 + ",left=" + iLocX
                 + ",top=" + iLocY;

    var theURL = "SelectFunder.aspx?funder=" + ID;
    windowName = new String(Math.round(Math.random() * 100000));
    DetailsWindow = window.open(theURL, windowName, strFeatures);
}
 
   </script>       
        
        
        <div>

            <asp:Panel ID="pnlSiteLicences" runat="server">

                <table style="width: 100%;">
                    <tr>
                        <td align="left" style="width: 60%">
                            <b>All site licenses</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                
                            &nbsp;&nbsp;&nbsp;
                            <asp:HyperLink ID="newLicenseLink" runat="server" NavigateUrl="~/SiteLicenseDetails.aspx?New">Create new license</asp:HyperLink>
                        </td>
                        <td align="right" width="50%">&nbsp;&nbsp;&nbsp; *Find&nbsp;&nbsp;
                        <asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();"></asp:TextBox>
                        </td>
                    </tr>
                </table>

                <telerik:RadGrid ID="radGVSiteLicenses" runat="server"
                    CssClass="Grid" Skin="Windows7" AllowPaging="True" AutoGenerateColumns="False"
                    Height="350px" ResolvedRenderMode="Classic" Width="800px"
                    OnItemDataBound="radGVSiteLicenses_ItemDataBound"
                    OnItemCommand="radGVSiteLicenses_ItemCommand"
                    AllowSorting="True"
                    OnNeedDataSource="radGVSiteLicenses_NeedDataSource"
                    OnPageIndexChanged="radGVSiteLicenses_PageIndexChanged"
                    DataKeyName="SITE_LIC_ID"
                    PageSize="10">

                    <ClientSettings EnablePostBackOnRowClick="false" >
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowRowResize="false" ResizeGridOnColumnResize="false"
                            ClipCellContentOnResize="true" EnableRealTimeResize="false"
                            AllowResizeToFit="true"  />
                    </ClientSettings>
                    <PagerStyle Mode="NumericPages"
                        PagerTextFormat="{4} Page {0} of {1}, rows {2} to {3} of {5}" />
                    <MasterTableView TableLayout="Fixed">
                        <Columns>
                            <telerik:GridBoundColumn DataField="SITE_LIC_ID"
                                FilterControlAltText="Filter License ID column" HeaderText="*License ID"
                                UniqueName="SITE_LIC_ID" DataFormatString="<nobr>{0}</nobr>"
                                Resizable="false" AllowFiltering="False" DataType="System.Int32">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="75px" />
                            </telerik:GridBoundColumn>

                            <telerik:GridTemplateColumn DataField="SITE_LIC_NAME" HeaderText="*Name"
                                UniqueName="SITE_LIC_NAME" Resizable="True" AllowFiltering="False"
                                SortExpression="SITE_LIC_NAME"
                                FilterControlAltText="Filter License column">
                                <ItemTemplate>
                                    <asp:HyperLink ID="License" runat="server" Text='<%# Bind("SITE_LIC_NAME") %>'
                                        NavigateUrl='<%# "~/SiteLicenseDetails.aspx?licID=" + Eval("SITE_LIC_ID") + "&admID=" + Eval("CONTACT_ID")  %>'
                                        DataFormatString="<nobr>{0}</nobr>">HyperLink</asp:HyperLink>
                                </ItemTemplate>
                                <HeaderStyle Width="235px" BackColor="#B6C6D6" ForeColor="Black" />
                            </telerik:GridTemplateColumn>

                            <telerik:GridBoundColumn DataField="CONTACT_ID"
                                FilterControlAltText="Filter name column" HeaderText="ContactID"
                                UniqueName="CONTACT_ID" 
                                Resizable="False" AllowFiltering="False" Visible="true">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="0px" />
                            </telerik:GridBoundColumn>

                            <telerik:GridBoundColumn DataField="COMPANY_NAME"
                                FilterControlAltText="Filter name column" HeaderText="Org"
                                UniqueName="COMPANY_NAME" DataFormatString="<nobr>{0}</nobr>"
                                Resizable="False" AllowFiltering="False">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="235px" />
                            </telerik:GridBoundColumn>

                            <telerik:GridBoundColumn DataField="CONTACT_NAME"
                                FilterControlAltText="Filter name column" HeaderText="*License Adm"
                                UniqueName="CONTACT_NAME" DataFormatString="<nobr>{0}</nobr>"
                                Resizable="False" AllowFiltering="False">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="235px" />
                            </telerik:GridBoundColumn>

                            <telerik:GridBoundColumn DataField="EXPIRY_DATE"
                                FilterControlAltText="Filter column1 column" HeaderText="Expiry date"
                                UniqueName="EXPIRY_DATE" DataFormatString="<nobr>{0:dd/MM/yyyy}</nobr>"
                                Resizable="False" AllowFiltering="False" DataType="System.DateTime">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="80px"></HeaderStyle>
                            </telerik:GridBoundColumn>





                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
                <br />


                <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server" OnAjaxRequest="RadAjaxManager1_AjaxRequest">
                    <AjaxSettings>
                        <telerik:AjaxSetting AjaxControlID="radGVContacts">
                            <UpdatedControls>
                                <telerik:AjaxUpdatedControl ControlID="radGVSiteLicenses" LoadingPanelID="RadAjaxLoadingPanel1" />
                            </UpdatedControls>
                        </telerik:AjaxSetting>

                        <telerik:AjaxSetting AjaxControlID="RadAjaxManager1">
                            <UpdatedControls>
                                <telerik:AjaxUpdatedControl ControlID="radGVSiteLicenses" LoadingPanelID="RadAjaxLoadingPanel1" />
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



                <br />
            </asp:Panel>






                    </div>



</asp:Content>


