<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Reviews.aspx.cs" Inherits="Reviews" Title="Reviews" %>
    <%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <div>
    
    
    <br />
            <strong>
            <asp:LinkButton ID="lbNewReview" runat="server" 
                OnClick="lbNewReview_Click">Create new review</asp:LinkButton>
            </strong>

            <asp:Panel ID="pnlShareableReviews" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td width="50%" align="left">
                            <b>Shareable reviews<strong>&nbsp;&nbsp;</b>
                        </td>
                        <td align="right" width="50%">
                            &nbsp;&nbsp;&nbsp; *Find&nbsp;&nbsp;
                            <asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                
           <telerik:RadGrid ID="radGVShareableReviews" runat="server"
                 CssClass="Grid"  Skin="Windows7" AllowPaging="True" AutoGenerateColumns="False" 
                    Height="350px" ResolvedRenderMode="Classic" Width="800px" 
                    onitemdatabound="radGVShareableReviews_ItemDataBound" 
                    AllowSorting="True"
                    onneeddatasource="radGVShareableReviews_NeedDataSource" 
                    onpageindexchanged="radGVShareableReviews_PageIndexChanged" PageSize="12">
                  
                  <ClientSettings>
                        <Resizing AllowColumnResize="True" AllowRowResize="false" ResizeGridOnColumnResize="false"
                            ClipCellContentOnResize="true" EnableRealTimeResize="false" AllowResizeToFit="true" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                        <Resizing AllowColumnResize="True" AllowResizeToFit="True" />
                 </ClientSettings>
                 <PagerStyle Mode="NumericPages" PagerTextFormat="{4} Page {0} from {1}, rows {2} to {3} from {5}" />
               <MasterTableView TableLayout="Fixed">
                   <Columns>
                       <telerik:GridBoundColumn DataField="REVIEW_ID" 
                           FilterControlAltText="Filter column8 column" HeaderText="*Review ID" 
                           UniqueName="ReviewID" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="false" AllowFiltering="False" DataType="System.Int32" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="65px" />
                       </telerik:GridBoundColumn>

                       <telerik:GridTemplateColumn DataField="REVIEW_NAME" HeaderText="*Review" 
                           UniqueName="Review" Resizable="True"  AllowFiltering="False" 
                           SortExpression="REVIEW_NAME" >  
                            <ItemTemplate>  
                                <asp:HyperLink ID="Review1" runat="server"  Text='<%# Bind("REVIEW_NAME") %>' 
                                NavigateUrl='<%# "~/ReviewDetails.aspx?id=" + Eval("REVIEW_ID")  %>'  
                                DataFormatString="<nobr>{0}</nobr>">HyperLink</asp:HyperLink>  
                            </ItemTemplate>  
                            <HeaderStyle Width="350px" BackColor="#B6C6D6" ForeColor="Black"  />
                            
                        </telerik:GridTemplateColumn> 
                       <telerik:GridBoundColumn DataField="DATE_CREATED" 
                           FilterControlAltText="Filter column column" HeaderText="Created" 
                           UniqueName="column" DataFormatString="<nobr>{0:dd/MM/yyyy}</nobr>" Resizable="False" 
                           AllowFiltering="False" DataType="System.DateTime" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="70px"/>
                       </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="EXPIRY_DATE" 
                           FilterControlAltText="Filter column1 column" HeaderText="Expiry date" 
                           UniqueName="column1" DataFormatString="<nobr>{0:dd/MM/yyyy}</nobr>" 
                           Resizable="False" AllowFiltering="False" DataType="System.DateTime" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="70px"/>
                       </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="CONTACT_NAME" 
                           FilterControlAltText="Filter column6 column" HeaderText="*Owner" 
                           UniqueName="column6" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="True" AllowFiltering="False">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="95px"/>
                       </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="SITE_LIC_ID" 
                           FilterControlAltText="Filter column2 column" HeaderText="Lic. ID" 
                           UniqueName="column2" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="False" AllowFiltering="False" DataType="System.Int16" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="50px"/>
                       </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="MONTHS_CREDIT" 
                           FilterControlAltText="Filter column3 column" HeaderText="Months credit" 
                           UniqueName="column3" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="False" AllowFiltering="False" DataType="System.Int16" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="50px"/>
                       </telerik:GridBoundColumn>
                       
                   </Columns>
               </MasterTableView>
            </telerik:RadGrid>
            </asp:Panel>
    
            <br />


           <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server" OnAjaxRequest="RadAjaxManager1_AjaxRequest">
                <AjaxSettings>
                    <telerik:AjaxSetting AjaxControlID="radGVShareableReviews">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVShareableReviews" LoadingPanelID="RadAjaxLoadingPanel1" />
                        </UpdatedControls>
                    </telerik:AjaxSetting>

                    <telerik:AjaxSetting AjaxControlID="RadAjaxManager1">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVShareableReviews" LoadingPanelID="RadAjaxLoadingPanel1" />
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