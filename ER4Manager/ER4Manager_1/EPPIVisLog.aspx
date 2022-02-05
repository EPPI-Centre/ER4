<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="EPPIVisLog.aspx.cs" Inherits="EPPIVisLog" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <asp:Panel ID="pnlEPPIVisusalisations" runat="server">
        Name: <asp:Label ID="lblWebDBName" runat="server" Text="0" Font-Bold="True"></asp:Label>&nbsp;&nbsp;ID: <asp:Label ID="lblWebDBID" runat="server" Text="0"></asp:Label><br />
        Review: <asp:Label ID="lblReviewName" runat="server" Text="0" Font-Bold="True"></asp:Label>&nbsp;&nbsp;ID: <asp:Label ID="lblReviewID" runat="server" Text="0"></asp:Label>      
        <asp:Panel runat="server" ID="pnlLogDetails" Visible="true">
                <table>
                    <tr>
                        <td width="75px">
                          
                            <asp:Button ID="cmdRetrieve" runat="server" Text="Retrieve" OnClick="cmdRetrieve_Click" />
                            
                        </td>
                        <td width="150px">
                            <br />
                            <!--<asp:RadioButtonList ID="rblDatePicker0" runat="server"></asp:RadioButtonList>-->
                            <asp:RadioButtonList ID="rblCalendars" runat="server" AutoPostBack="True" OnSelectedIndexChanged="rblCalendars_SelectedIndexChanged" 
                                RepeatDirection="Horizontal" Visible="True">
                                <asp:ListItem Selected="True" Value="0">From</asp:ListItem>
                                <asp:ListItem Value="1">Between</asp:ListItem>
                            </asp:RadioButtonList>
                        </td>
                        <td width="175px">From<br />
                            <telerik:RadDateTimePicker RenderMode="Lightweight" AutoPostBackControl="Both" Width="100%" ID="RadTimePicker1" runat="server"
                                OnItemCreated="RadTimePicker1_ItemCreated" OnItemDataBound="RadTimePicker1_ItemDataBound"
                                OnSelectedDateChanged="RadTimePicker1_SelectedDateChanged">
                            </telerik:RadDateTimePicker>
                        </td>
                        <td width="175px">
                            <asp:Panel runat="server" ID="pnlRadTimePicker2" Visible="true">
                                To<br />
                                <telerik:RadDateTimePicker RenderMode="Lightweight" AutoPostBackControl="Both" Width="100%" ID="RadTimePicker2" runat="server"
                                    OnItemCreated="RadTimePicker2_ItemCreated" OnItemDataBound="RadTimePicker2_ItemDataBound"
                                    OnSelectedDateChanged="RadTimePicker2_SelectedDateChanged">
                                </telerik:RadDateTimePicker>
                            </asp:Panel>
                        </td>
                    </tr>
                </table>

                
        </asp:Panel>



        <table style="width: 100%;">
            <tr>
                <td align="left" style="width: 60%">
                    <b>Log details</b>&nbsp;&nbsp;(top 5000 entries based on date selected)
                    
                </td>
                <td align="right" width="50%">&nbsp;&nbsp;&nbsp; Type&nbsp;&nbsp;
                    <asp:DropDownList ID="ddlTypes" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlTypes_SelectedIndexChanged">
                         <asp:listitem text="All" value="1"></asp:listitem>
                         <asp:listitem text="Login" value="2"></asp:listitem>
                         <asp:listitem text="Search" value="3"></asp:listitem>
                         <asp:listitem text="GetFrequency" value="4"></asp:listitem>
                         <asp:listitem text="GetSetFrequency" value="5"></asp:listitem>
                         <asp:listitem text="GetFrequencyNewPage" value="6"></asp:listitem>
                         <asp:listitem text="GetItemList" value="7"></asp:listitem>
                         <asp:listitem text="GetMap" value="8"></asp:listitem>
                        <asp:listitem text="ItemDetailsFromList" value="9"></asp:listitem>
                    </asp:DropDownList>                  
                </td>
                <!--<td align="right" width="50%">&nbsp;&nbsp;&nbsp; *Find&nbsp;&nbsp;
                        <asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();" Visible="false"></asp:TextBox>
                </td>-->
            </tr>
        </table>

        <telerik:RadGrid ID="radGVEPPIVisLog" runat="server" 
            CssClass="Grid" Skin="Windows7" AllowPaging="True" AutoGenerateColumns="False"
            Height="280px" ResolvedRenderMode="Classic" Width="800px"
            OnItemDataBound="radGVEPPIVisLog_ItemDataBound"
            OnItemCommand="radGVEPPIVisLog_ItemCommand"
            AllowSorting="True"
            OnNeedDataSource="radGVEPPIVisLog_NeedDataSource"
            OnPageIndexChanged="radGVEPPIVisLog_PageIndexChanged"
            DataKeyName="WEBDB_ID"
            PageSize="10">

            <ClientSettings EnablePostBackOnRowClick="false">
                <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                <Resizing AllowColumnResize="True" AllowRowResize="false" ResizeGridOnColumnResize="false"
                    ClipCellContentOnResize="true" EnableRealTimeResize="false"
                    AllowResizeToFit="true" />
            </ClientSettings>
            <PagerStyle Mode="NumericPages"
                PagerTextFormat="{4} Page {0} of {1}, rows {2} to {3} of {5}" />
            <MasterTableView TableLayout="Fixed">
                <Columns>
                    <telerik:GridBoundColumn DataField="WEBDB_LOG_IDENTITY"
                        FilterControlAltText="Filter WebDB ID column" HeaderText="Identity ID"
                        UniqueName="WEBDB_ID" DataFormatString="<nobr>{0}</nobr>"
                        Resizable="false" AllowFiltering="False" DataType="System.Int32">
                        <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="75px" />
                    </telerik:GridBoundColumn>

                    <telerik:GridBoundColumn DataField="CREATED" 
                           FilterControlAltText="Filter column column" HeaderText="Created" 
                           UniqueName="created" DataFormatString="&lt;nobr&gt;{0:dd/MM/yyyy}&lt;/nobr&gt;" 
                           Resizable="True" AllowFiltering="False" DataType="System.DateTime" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="120px"/>
                       </telerik:GridBoundColumn>

                    <telerik:GridBoundColumn DataField="TYPE"
                        FilterControlAltText="Filter name column" HeaderText="*Type"
                        UniqueName="REVIEW_NAME" DataFormatString="<nobr>{0}</nobr>"
                        Resizable="True" AllowFiltering="False">
                        <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="140px" />
                    </telerik:GridBoundColumn>

                    <telerik:GridBoundColumn DataField="DETAILS"
                        FilterControlAltText="Filter name column" HeaderText="*Details"
                        UniqueName="CONTACT_NAME" DataFormatString="<nobr>{0}</nobr>"
                        Resizable="True" AllowFiltering="False">
                        <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="465px" />
                    </telerik:GridBoundColumn>

                </Columns>
            </MasterTableView>
        </telerik:RadGrid>




        <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server" OnAjaxRequest="RadAjaxManager1_AjaxRequest">
            <AjaxSettings>
                <telerik:AjaxSetting AjaxControlID="radGVContacts">
                    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="radGVEPPIVisLog" LoadingPanelID="RadAjaxLoadingPanel1" />
                    </UpdatedControls>
                </telerik:AjaxSetting>

                <telerik:AjaxSetting AjaxControlID="RadAjaxManager1">
                    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="radGVEPPIVisLog" LoadingPanelID="RadAjaxLoadingPanel1" />
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


    <asp:SqlDataSource ID="SqlDataSource2" ConnectionString="Data Source=localhost;Initial Catalog=ReviewerAdmin;Integrated Security=True"
        ProviderName="System.Data.SqlClient" SelectCommand="SELECT DISTINCT LOG_TYPE FROM TB_WEBDB_LOG"
        runat="server"></asp:SqlDataSource>


</asp:Content>

