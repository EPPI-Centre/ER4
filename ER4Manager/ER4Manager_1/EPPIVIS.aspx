<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="EPPIVIS.aspx.cs" Inherits="EPPIVIS" %>
    <%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

    <!--http://localhost:50813/Login/Open?WebDBid=-->
    <!--"~/EPPI-Vis/Login/Open?WebDBid="-->

    <asp:Panel ID="pnlSiteLicences" runat="server">

        <table style="width: 100%;">
            <tr>
                <td align="left" style="width: 60%">
                    <b>All EPPI-Vis visualisations</b>
                </td>
                <td align="right" width="50%">&nbsp;&nbsp;&nbsp; *Find&nbsp;&nbsp;
                        <asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();"></asp:TextBox>
                </td>
            </tr>
        </table>

        <telerik:RadGrid ID="radGVEPPIVis" runat="server"
            CssClass="Grid" Skin="Windows7" AllowPaging="True" AutoGenerateColumns="False"
            Height="350px" ResolvedRenderMode="Classic" Width="800px"
            OnItemDataBound="radGVEPPIVis_ItemDataBound"
            OnItemCommand="radGVEPPIVis_ItemCommand"
            AllowSorting="True"
            OnNeedDataSource="radGVEPPIVis_NeedDataSource"
            OnPageIndexChanged="radGVEPPIVis_PageIndexChanged"
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
                    <telerik:GridBoundColumn DataField="WEBDB_ID"
                        FilterControlAltText="Filter WebDB ID column" HeaderText="*WebDB ID"
                        UniqueName="WEBDB_ID" DataFormatString="<nobr>{0}</nobr>"
                        Resizable="false" AllowFiltering="False" DataType="System.Int32">
                        <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="75px" />
                    </telerik:GridBoundColumn>

                    <telerik:GridTemplateColumn DataField="WEBDB_NAME" HeaderText="*Visualisation"
                        UniqueName="WEBDB_NAME" Resizable="True" AllowFiltering="False"
                        SortExpression="WEBDB_NAME"
                        FilterControlAltText="Filter EPPI-Vis column">
                        <ItemTemplate>
                            <asp:HyperLink ID="EPPIVis" runat="server" Text='<%# Bind("WEBDB_NAME") %>'
                                NavigateUrl='<%# EPPIVisUrl() + "Login/Open?WebDBid=" + Eval("WEBDB_ID")  %>'
                                DataFormatString="<nobr>{0}</nobr>" Target="_blank">HyperLink</asp:HyperLink>
                        </ItemTemplate>
                        <HeaderStyle Width="200px" BackColor="#B6C6D6" ForeColor="Black" />
                    </telerik:GridTemplateColumn>




                    <telerik:GridBoundColumn DataField="REVIEW_ID"
                        FilterControlAltText="Filter name column" HeaderText="*ReviewID"
                        UniqueName="REVIEW_ID"
                        Resizable="False" AllowFiltering="False" Visible="true">
                        <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="70px" />
                    </telerik:GridBoundColumn>

                    <telerik:GridBoundColumn DataField="REVIEW_NAME"
                        FilterControlAltText="Filter name column" HeaderText="*Review"
                        UniqueName="REVIEW_NAME" DataFormatString="<nobr>{0}</nobr>"
                        Resizable="False" AllowFiltering="False">
                        <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="190px" />
                    </telerik:GridBoundColumn>

                    <telerik:GridBoundColumn DataField="CONTACT_NAME"
                        FilterControlAltText="Filter name column" HeaderText="*Created by"
                        UniqueName="CONTACT_NAME" DataFormatString="<nobr>{0}</nobr>"
                        Resizable="False" AllowFiltering="False">
                        <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="190px" />
                    </telerik:GridBoundColumn>

                    <telerik:GridBoundColumn DataField="IS_OPEN"
                        FilterControlAltText="Filter name column" HeaderText="Is open"
                        UniqueName="IS_OPEN" DataFormatString="<nobr>{0}</nobr>"
                        Resizable="False" AllowFiltering="False">
                        <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="75px" />
                    </telerik:GridBoundColumn>



                </Columns>
            </MasterTableView>
        </telerik:RadGrid>


        <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server" OnAjaxRequest="RadAjaxManager1_AjaxRequest">
            <AjaxSettings>
                <telerik:AjaxSetting AjaxControlID="radGVContacts">
                    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="radGVEPPIVis" LoadingPanelID="RadAjaxLoadingPanel1" />
                    </UpdatedControls>
                </telerik:AjaxSetting>

                <telerik:AjaxSetting AjaxControlID="RadAjaxManager1">
                    <UpdatedControls>
                        <telerik:AjaxUpdatedControl ControlID="radGVEPPIVis" LoadingPanelID="RadAjaxLoadingPanel1" />
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


</asp:Content>

