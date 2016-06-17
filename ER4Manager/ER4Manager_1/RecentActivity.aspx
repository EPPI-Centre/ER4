<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="RecentActivity.aspx.cs" Inherits="RecentActivity" Title="RecentActivity" %>

    <%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
<%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <div>
            <br />
            <table style="width:100%;">
                <tr>
                    <td align="left" style="width: 60%">
                        <b>Contacts</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        &nbsp;</td>
                    <td align="right" width="50%">
                        &nbsp;&nbsp;&nbsp; *Find&nbsp;&nbsp;
                        <asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();"></asp:TextBox>
                    </td>
                </tr>
            </table>
                
           <telerik:RadGrid ID="radGVContacts" runat="server"
                 CssClass="Grid"  Skin="Windows7" AllowPaging="True" AutoGenerateColumns="False" 
                    Height="350px" ResolvedRenderMode="Classic" Width="800px" 
                    onitemdatabound="radGVContacts_ItemDataBound" 
                    AllowSorting="True"
                    onneeddatasource="radGVContacts_NeedDataSource" 
                    onpageindexchanged="radGVContacts_PageIndexChanged" 
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

                        <telerik:GridBoundColumn DataField="CONTACT_NAME" 
                           FilterControlAltText="Filter email column" HeaderText="*Name" 
                           UniqueName="email" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="True" AllowFiltering="False">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="200px"/>
                       </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="EMAIL" 
                           FilterControlAltText="Filter email column" HeaderText="*Email" 
                           UniqueName="email" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="True" AllowFiltering="False">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="200px"/>
                       </telerik:GridBoundColumn>
                       
                       <telerik:GridTemplateColumn DataField="CONTACT_ID" HeaderText="*Contact ID" 
                           UniqueName="ContactID" Resizable="False"  AllowFiltering="False" 
                           SortExpression="CONTACT_ID" 
                           FilterControlAltText="Filter Contact ID column" DataType="System.Int16" >  
                            <ItemTemplate>  
                                <asp:HyperLink ID="ContactID" runat="server"  Text='<%# Bind("CONTACT_ID") %>' 
                                NavigateUrl='<%# "~/ContactDetails.aspx?id=" + Eval("CONTACT_ID")  %>'  
                                DataFormatString="<nobr>{0}</nobr>"
                                >HyperLink</asp:HyperLink>  
                            </ItemTemplate>  
                            <HeaderStyle Width="80px" BackColor="#B6C6D6" ForeColor="Black"  />                       
                        </telerik:GridTemplateColumn> 

                        <telerik:GridTemplateColumn DataField="REVIEW_ID" HeaderText="*Review ID" 
                           UniqueName="ReviewID" Resizable="False"  AllowFiltering="False" 
                           SortExpression="REVIEW_ID" 
                           FilterControlAltText="Filter Review ID column" DataType="System.Int16" >  
                            <ItemTemplate>  
                                <asp:HyperLink ID="ReviewID" runat="server"  Text='<%# Bind("REVIEW_ID") %>' 
                                NavigateUrl='<%# "~/ReviewDetails.aspx?id=" + Eval("REVIEW_ID")  %>'  
                                DataFormatString="<nobr>{0}</nobr>">HyperLink</asp:HyperLink>  
                            </ItemTemplate>  
                            <HeaderStyle Width="80px" BackColor="#B6C6D6" ForeColor="Black"  />                       
                        </telerik:GridTemplateColumn>

                        <telerik:GridBoundColumn DataField="CREATED" 
                           DataFormatString="&lt;nobr&gt;{0:dd/MM/yyyy}&lt;/nobr&gt;" 
                           AllowFiltering="False" HeaderText="Created" Resizable="False" 
                           UniqueName="created" DataType="System.DateTime" 
                           FilterControlAltText="Filter column column">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="120px"></HeaderStyle>
                        </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="LAST_RENEWED" 
                           FilterControlAltText="Filter column1 column" HeaderText="Last activity" 
                           UniqueName="expiry" DataFormatString="<nobr>{0:dd/MM/yyyy}</nobr>" 
                           Resizable="False" AllowFiltering="False" DataType="System.DateTime" >
                           <HeaderStyle BackColor="#B6C6D6"  ForeColor="Black" Width="120px"></HeaderStyle>
                       </telerik:GridBoundColumn>
                       
                   </Columns>
               </MasterTableView>
            </telerik:RadGrid>
            <br />

              <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server" OnAjaxRequest="RadAjaxManager1_AjaxRequest">
                <AjaxSettings>
                    <telerik:AjaxSetting AjaxControlID="radGVContacts">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVContacts" LoadingPanelID="RadAjaxLoadingPanel1" />
                        </UpdatedControls>
                    </telerik:AjaxSetting>

                    <telerik:AjaxSetting AjaxControlID="RadAjaxManager1">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVContacts" LoadingPanelID="RadAjaxLoadingPanel1" />
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

            </div>
</asp:Content>

