<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="RecentActivity.aspx.cs" Inherits="RecentActivity" Title="RecentActivity" %>

    <%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
<%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <div>
            <asp:Panel ID="pnlNewDisplay" runat="server" Visible="False">
                <table style="width:100%;">
                    <tr>
                        <td align="left" style="width: 60%"><b>Contacts</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;</td>
                        <td align="right" width="50%">&nbsp;&nbsp;&nbsp; *Find&nbsp;&nbsp;
                            <asp:TextBox ID="tbFilter0" runat="server" onkeyup="KeyUp0();"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                <telerik:RadGrid ID="radGVContacts0" runat="server" AllowPaging="True" AllowSorting="True" AutoGenerateColumns="False" CssClass="Grid" Height="350px" onitemdatabound="radGVContacts0_ItemDataBound" onneeddatasource="radGVContacts0_NeedDataSource" onpageindexchanged="radGVContacts0_PageIndexChanged" PageSize="12" ResolvedRenderMode="Classic" Skin="Windows7" Width="800px">
                    <ClientSettings>
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="true" AllowRowResize="false" ClipCellContentOnResize="true" EnableRealTimeResize="false" ResizeGridOnColumnResize="false" />
                    </ClientSettings>
                    <PagerStyle Mode="NumericPages" PagerTextFormat="{4} Page {0} from {1}, rows {2} to {3} from {5}" />
                    <MasterTableView TableLayout="Fixed">
                        <Columns>
                            <telerik:GridTemplateColumn AllowFiltering="False" DataField="REVIEW_ID" DataType="System.Int16" FilterControlAltText="Filter Review ID column" HeaderText="*Rev ID" Resizable="False" SortExpression="REVIEW_ID" UniqueName="ReviewID">
                                <ItemTemplate>
                                    <asp:HyperLink ID="ReviewID0" runat="server" DataFormatString="&lt;nobr&gt;{0}&lt;/nobr&gt;" NavigateUrl='<%# "~/ReviewDetails.aspx?id=" + Eval("REVIEW_ID")  %>' Text='<%# Bind("REVIEW_ID") %>'>HyperLink</asp:HyperLink>
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="50px" />
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn AllowFiltering="False" DataField="CONTACT_ID" DataType="System.Int16" FilterControlAltText="Filter Contact ID column" HeaderText="*Cont ID" Resizable="False" SortExpression="CONTACT_ID" UniqueName="ContactID">
                                <ItemTemplate>
                                    <asp:HyperLink ID="ContactID0" runat="server" DataFormatString="&lt;nobr&gt;{0}&lt;/nobr&gt;" NavigateUrl='<%# "~/ContactDetails.aspx?id=" + Eval("CONTACT_ID")  %>' Text='<%# Bind("CONTACT_ID") %>'>HyperLink</asp:HyperLink>
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="60px" />
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn AllowFiltering="False" DataField="CREATED" DataFormatString="&lt;nobr&gt;{0:dd/MM/yyyy}&lt;/nobr&gt;" DataType="System.DateTime" FilterControlAltText="Filter column column" HeaderText="Created" Resizable="True" UniqueName="created">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="110px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn AllowFiltering="False" DataField="LAST_RENEWED" DataFormatString="&lt;nobr&gt;{0:dd/MM/yyyy}&lt;/nobr&gt;" DataType="System.DateTime" FilterControlAltText="Filter column1 column" HeaderText="Last activity" Resizable="True" UniqueName="expiry">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="110px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn AllowFiltering="False" DataField="CONTACT_NAME" DataFormatString="&lt;nobr&gt;{0}&lt;/nobr&gt;" FilterControlAltText="Filter email column" HeaderText="*Name" Resizable="True" UniqueName="contactName">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="130px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn AllowFiltering="False" DataField="EMAIL" DataFormatString="&lt;nobr&gt;{0}&lt;/nobr&gt;" FilterControlAltText="Filter email column" HeaderText="*Email" Resizable="True" UniqueName="email">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="150px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn AllowFiltering="False" DataField="REVIEW_TYPE" DataFormatString="&lt;nobr&gt;{0}&lt;/nobr&gt;" FilterControlAltText="Filter email column" HeaderText="Rev Type" Resizable="True" UniqueName="revType">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="60px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn AllowFiltering="False" DataField="ACTIVE_HOURS" DataType="System.Int16" FilterControlAltText="Filter email column" HeaderText="Hours" Resizable="False" UniqueName="activeHours">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="50px" />
                            </telerik:GridBoundColumn>
                            
                            
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
                
                <br />
                <br />
            </asp:Panel>
            <br />
            <br />
            <br />

              <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server" OnAjaxRequest="RadAjaxManager1_AjaxRequest">
                <AjaxSettings>
                    

                    <telerik:AjaxSetting AjaxControlID="radGVContacts0">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVContacts0" LoadingPanelID="RadAjaxLoadingPanel2" />
                        </UpdatedControls>
                    </telerik:AjaxSetting>

                    <telerik:AjaxSetting AjaxControlID="RadAjaxManager1">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVContacts0" LoadingPanelID="RadAjaxLoadingPanel2" />
                        </UpdatedControls>
                    </telerik:AjaxSetting>




                </AjaxSettings>
            </telerik:RadAjaxManager>



            <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel2" runat="server" Height="75px" Transparency="25" Width="75px">
                </telerik:RadAjaxLoadingPanel>





           <telerik:RadCodeBlock ID="RadCodeBlock2" runat="server">   
                <script type="text/javascript">
                    var timer = null;

                    function KeyUp0() {
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

            </div>
</asp:Content>

