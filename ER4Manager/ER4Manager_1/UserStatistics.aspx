<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="UserStatistics.aspx.cs" Inherits="UserStatistics" Title="UserStatistics" %>

    <%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>

<script runat="server">


</script>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <div>
            <br />
            Total number of accounts created after 20/03/2010:
            <asp:Label ID="lblAccountsCreated" runat="server" Font-Bold="True" Text="n "></asp:Label>
            (both active and expired)<br />
            Total number of accounts that have logged on after 01/07/2010:
            <asp:Label ID="lblNumActiveUsers" runat="server" Text="n" Font-Bold="True"></asp:Label>
            &nbsp;(both active and expired)<br />
            <br />
            <table style="width:100%;">
                <tr>
                    <td align="left" width="12%">
                        <b>Contacts</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        &nbsp;</td>
                    <td align="left" width="15%">
                        <asp:CheckBox ID="cbWithLogin" runat="server" AutoPostBack="True" 
                            Checked="True" oncheckedchanged="cbValidER4Account_CheckedChanged" 
                            Text="# ER4 logins &gt; 0" ToolTip="(Expiry date &gt; 20/03/2010)" />
                        </td>
                    <td align="left" width="15%">
            <asp:CheckBox ID="cbRemaining" runat="server" AutoPostBack="True" 
                Checked="True" oncheckedchanged="cbRemaining_CheckedChanged" 
                Text="Days remaining &gt; 0" />
                    </td>
                    <td align="right" width="10%">
                        &nbsp;<asp:DropDownList ID="ddlTimePeriod" runat="server" AutoPostBack="True" 
                onselectedindexchanged="ddlTimePeriod_SelectedIndexChanged" ToolTip="Time period">
                <asp:ListItem Value="10000">All time</asp:ListItem>
                <asp:ListItem Value="1">Today</asp:ListItem>
                <asp:ListItem Value="7">1 week</asp:ListItem>
                <asp:ListItem Value="30">1 Month</asp:ListItem>
                <asp:ListItem Value="90">3 Months</asp:ListItem>
                <asp:ListItem Value="180">6 Months</asp:ListItem>
                <asp:ListItem Value="365">1 Year</asp:ListItem>
            </asp:DropDownList>
                    </td>
                    <td align="left" width="10%">
                        starting<br />
                        01/07/2010</td>
                    <td align="right" width="20%">
                        *Find&nbsp;<asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();"></asp:TextBox>
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
                       <telerik:GridBoundColumn DataField="CONTACT_ID" 
                           FilterControlAltText="Filter ContactID column" HeaderText="*Contact ID" 
                           UniqueName="ContactID" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="false" AllowFiltering="False" DataType="System.Int16" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="75px" />
                       </telerik:GridBoundColumn>
                       <telerik:GridTemplateColumn DataField="CONTACT_NAME" HeaderText="*Name" 
                           UniqueName="Review" Resizable="True"  AllowFiltering="False" 
                           SortExpression="CONTACT_NAME" 
                           FilterControlAltText="Filter Contact column" >  
                            <ItemTemplate>  
                                <asp:HyperLink ID="Contact1" runat="server"  Text='<%# Bind("CONTACT_NAME") %>' 
                                NavigateUrl='<%# "~/ContactDetails.aspx?id=" + Eval("CONTACT_ID")  %>'  
                                DataFormatString="<nobr>{0}</nobr>">HyperLink</asp:HyperLink>  
                            </ItemTemplate>  
                            <HeaderStyle Width="225px" BackColor="#B6C6D6" ForeColor="Black"  />
                            
                        </telerik:GridTemplateColumn> 
                       <telerik:GridBoundColumn DataField="NUMBER_LOGINS" 
                           FilterControlAltText="Filter email column" HeaderText="Number logins" 
                           UniqueName="email" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="False" AllowFiltering="False" DataType="System.Int16">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="95px"/>
                       </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn DataField="HOURS" 
                           DataFormatString="<nobr>{0}</nobr>" 
                           AllowFiltering="False" HeaderText="Hours logged in" Resizable="False" 
                           UniqueName="expiry"  
                           FilterControlAltText="Filter column1 column" DataType="System.Int16">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="100px"></HeaderStyle>
                        </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="DAYS_SINCE_CREATION" 
                           FilterControlAltText="Filter column2 column" HeaderText="Account age" 
                           UniqueName="column2" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="False" AllowFiltering="False" DataType="System.Int16">
                           <HeaderStyle BackColor="#B6C6D6"  ForeColor="Black" Width="85px"></HeaderStyle>
                       </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="DAYS_REMAINING" 
                           FilterControlAltText="Filter column3 column" HeaderText="Days remaining" 
                           UniqueName="column3" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="False" AllowFiltering="False" DataType="System.Int16">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="100px"/>
                       </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="LAST_ACCESS" 
                           FilterControlAltText="Filter column column" HeaderText="Last access" 
                           UniqueName="created" DataFormatString="&lt;nobr&gt;{0:dd/MM/yyyy}&lt;/nobr&gt;" 
                           Resizable="False" AllowFiltering="False" DataType="System.DateTime" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="120px"/>
                       </telerik:GridBoundColumn>
                       
                   </Columns>
               </MasterTableView>
            </telerik:RadGrid>
            <asp:Label ID="lblSiteLicenceMsg" runat="server" 
                Text="Note: negative (-) 'Days remaining' are contacts in a site license."></asp:Label>
           <br />
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
            
            
            
            
            
            
            

            
                        </div>
</asp:Content>