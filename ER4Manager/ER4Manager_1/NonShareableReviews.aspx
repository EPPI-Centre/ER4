<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="NonShareableReviews.aspx.cs" Inherits="NonShareableReviews" Title="Non-Shareable reviews" %>
    <%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <div>
    
    
    <br />
            <strong>
            <asp:LinkButton ID="lbNewReview" runat="server" 
                OnClick="lbNewReview_Click">Create new review</asp:LinkButton>
            </strong>

            <asp:Panel ID="pnlNonShareableReviews" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td align="left" width="50%">
                            <b>Non-shareable reviews&nbsp;&nbsp; </b>
                        </td>
                        <td align="right" width="50%">
                            &nbsp;&nbsp;&nbsp;*Find&nbsp;&nbsp;
                            <asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();"></asp:TextBox>
                        </td>
                    </tr>
                </table>

                <telerik:RadGrid ID="radGVNonShareableReviews" runat="server" 
                    AllowPaging="True" AllowSorting="True" AutoGenerateColumns="False" 
                    CssClass="Grid" Height="350px" 
                    onitemdatabound="radGVNonShareableReviews_ItemDataBound" 
                    onneeddatasource="radGVNonShareableReviews_NeedDataSource" 
                    onpageindexchanged="radGVNonShareableReviews_PageIndexChanged" PageSize="12" 
                    ResolvedRenderMode="Classic" Skin="Windows7" Width="800px">
                    
                    <ClientSettings>
                        <Resizing AllowColumnResize="True" AllowResizeToFit="true" 
                            AllowRowResize="false" ClipCellContentOnResize="true" 
                            EnableRealTimeResize="false" ResizeGridOnColumnResize="false" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                    </ClientSettings>
                    <PagerStyle Mode="NumericPages" 
                        PagerTextFormat="{4} Page {0} from {1}, rows {2} to {3} from {5}" />
                    <MasterTableView TableLayout="Fixed">
                        <Columns>
                            <telerik:GridBoundColumn AllowFiltering="False" DataField="REVIEW_ID" 
                                DataFormatString="&lt;nobr&gt;{0}&lt;/nobr&gt;" DataType="System.Int32" 
                                FilterControlAltText="Filter column8 column" HeaderText="*Review ID" 
                                Resizable="false" UniqueName="ReviewID">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="75px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn AllowFiltering="False" DataField="REVIEW_NAME" 
                                HeaderText="*Review" Resizable="True" SortExpression="REVIEW_NAME" 
                                UniqueName="Review" >
                                <ItemTemplate>
                                    <asp:HyperLink ID="Review1" runat="server" 
                                        DataFormatString="<nobr>{0}</nobr>" 
                                        NavigateUrl='<%# "~/ReviewDetails.aspx?id=" + Eval("REVIEW_ID")  %>' 
                                        Text='<%# Bind("REVIEW_NAME") %>'>HyperLink</asp:HyperLink>
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="465px" />
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn AllowFiltering="False" DataField="DATE_CREATED" 
                                DataFormatString="&lt;nobr&gt;{0:dd/MM/yyyy}&lt;/nobr&gt;" 
                                DataType="System.DateTime" FilterControlAltText="Filter column column" 
                                HeaderText="Created" Resizable="False" UniqueName="column">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="70px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn AllowFiltering="False" DataField="CONTACT_NAME" 
                                DataFormatString="&lt;nobr&gt;{0}&lt;/nobr&gt;" 
                                FilterControlAltText="Filter column6 column" HeaderText="*Owner" 
                                Resizable="True" UniqueName="column6">
                                <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="140px" />
                            </telerik:GridBoundColumn>
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
            </asp:Panel>
    
            <br />


            <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server" OnAjaxRequest="RadAjaxManager1_AjaxRequest">
                <AjaxSettings>
                    <telerik:AjaxSetting AjaxControlID="radGVNonShareableReviews">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVNonShareableReviews" LoadingPanelID="RadAjaxLoadingPanel1" />
                        </UpdatedControls>
                    </telerik:AjaxSetting>

                    <telerik:AjaxSetting AjaxControlID="RadAjaxManager1">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVNonShareableReviews" LoadingPanelID="RadAjaxLoadingPanel1" />
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

    </div>
</asp:Content>
