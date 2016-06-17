<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Contacts.aspx.cs" Inherits="Contacts" Title="Contacts" %>
    <%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <div>
    
            <strong>
            <br />
            <asp:LinkButton ID="lbNewAccount" runat="server" 
                OnClick="lbNewAccount_Click">Create new account</asp:LinkButton>
            </strong>
    
            <br />
            <table style="width:100%;">
                <tr>
                    <td align="left" style="width: 60%">
                        <b>Contacts</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:CheckBox ID="cbValidER4Account" runat="server" AutoPostBack="True" 
                    oncheckedchanged="cbValidER4Account_CheckedChanged" 
                    Text="ER4 accounts only" Checked="True" />
                &nbsp;(Expiry data &gt; 20/03/2010)</td>
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
                       <telerik:GridBoundColumn DataField="CONTACT_ID" 
                           FilterControlAltText="Filter ContactID column" HeaderText="*Contact ID" 
                           UniqueName="ContactID" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="false" AllowFiltering="False" DataType="System.Int32" >
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
                            <HeaderStyle Width="150px" BackColor="#B6C6D6" ForeColor="Black"  />
                            
                        </telerik:GridTemplateColumn> 
                       <telerik:GridBoundColumn DataField="EMAIL" 
                           FilterControlAltText="Filter email column" HeaderText="*Email" 
                           UniqueName="email" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="False" AllowFiltering="False">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="240px"/>
                       </telerik:GridBoundColumn>
                        <telerik:GridBoundColumn DataField="DATE_CREATED" 
                           DataFormatString="&lt;nobr&gt;{0:dd/MM/yyyy}&lt;/nobr&gt;" 
                           AllowFiltering="False" HeaderText="Created" Resizable="False" 
                           UniqueName="created" DataType="System.DateTime" 
                           FilterControlAltText="Filter column column">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="70px"></HeaderStyle>
                        </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="EXPIRY_DATE" 
                           FilterControlAltText="Filter column1 column" HeaderText="Expiry" 
                           UniqueName="expiry" DataFormatString="<nobr>{0:dd/MM/yyyy}</nobr>" 
                           Resizable="False" AllowFiltering="False" DataType="System.DateTime" >
                           <HeaderStyle BackColor="#B6C6D6"  ForeColor="Black" Width="70px"></HeaderStyle>
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
                       
                       <telerik:GridBoundColumn AllowFiltering="False" DataField="CONTACT_NAME" 
                           DataFormatString="&lt;nobr&gt;{0}&lt;/nobr&gt;" 
                           FilterControlAltText="Filter column6 column" HeaderText="Creator" 
                           UniqueName="column6">
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="95px" />
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
            <br />
    
    </div>
</asp:Content>