<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="ContactDetails.aspx.cs" Inherits="ContactDetails" Title="Contact details" %>
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
    </script>
        <div>
    
            <b>Your details<br />
            </b>
            <asp:Button ID="cmdPlaceDate" runat="server" BackColor="White" 
                BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px" 
                OnClick="cmdPlaceDate_Click" style="font-weight: bold" Width="1px" />
            <b>
            <br />
            </b>
    <table id="Table1"
            border="1" cellpadding="1" cellspacing="1" width="800">
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Contact name *</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:TextBox ID="tbContactName" runat="server" Columns="60" MaxLength="50" 
                        CssClass="Textbox" Width="60%"></asp:TextBox></td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    User name (Min 4 characters) *</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:TextBox ID="tbUserName" runat="server" Columns="60" MaxLength="50" 
                        CssClass="Textbox" Width="60%"></asp:TextBox>&nbsp; 
                    Username must be unique.</td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Password </td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:TextBox ID="tbPassword" runat="server" Columns="30" 
                        MaxLength="15" CssClass="Textbox" Width="30%"></asp:TextBox>
                    &nbsp;(leave blank if unchanged)&nbsp;
                    &nbsp;&nbsp;
                    <asp:Button ID="cmdGeneratePassword" runat="server" Text="Generate" 
                        onclick="cmdGeneratePassword_Click" />
                    &nbsp;&nbsp; <asp:Button ID="cmdShowHidePassword" runat="server" 
                        onclick="cmdShowHidePassword_Click" Text="Show" Enabled="False" 
                        Visible="False" />
                    </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Email 
                    *</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:TextBox ID="tbEmail" runat="server" Columns="60" CssClass="Textbox" 
                        Width="60%"></asp:TextBox>
                    &nbsp; Email address must be unique.</td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Date created&nbsp;*&nbsp;&nbsp;
                </td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:TextBox ID="tbDateCreated" runat="server" Columns="60" CssClass="Textbox" 
                        Width="60%"></asp:TextBox>
                    
                    &nbsp;<asp:ImageButton ID="IBCalendar1" runat="server" 
                        ImageUrl="~/images/calbtn.gif" />
                    
                    </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Date expiry *&nbsp;&nbsp;&nbsp;&nbsp;
                    <asp:Label ID="lblInSiteLicense" runat="server" Font-Bold="True" 
                        Text="In license #" Visible="False"></asp:Label>
                </td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:TextBox ID="tbDateExpiry" runat="server" Columns="60" CssClass="Textbox" 
                        Width="60%"></asp:TextBox>
                    
                    &nbsp;<asp:ImageButton ID="IBCalendar2" runat="server" 
                        ImageUrl="~/images/calbtn.gif" />
                    
                &nbsp;&nbsp;
                        <asp:Button ID="cmdNullExpiry" runat="server" onclick="cmdNullExpiry_Click" 
                            Text="Null" />
                    
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Months credit</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                        <asp:DropDownList ID="ddlMonthsCredit" runat="server">
                            <asp:ListItem Value="0">0 Months</asp:ListItem>
                            <asp:ListItem Value="1">1 Months</asp:ListItem>
                            <asp:ListItem Value="2">2 Months</asp:ListItem>
                            <asp:ListItem Value="3">3 months</asp:ListItem>
                            <asp:ListItem Value="4">4 months</asp:ListItem>
                            <asp:ListItem Value="5">5 months</asp:ListItem>
                            <asp:ListItem Value="6">6 months</asp:ListItem>
                            <asp:ListItem Value="7">7 months</asp:ListItem>
                            <asp:ListItem Value="8">8 months</asp:ListItem>
                            <asp:ListItem Value="9">9 months</asp:ListItem>
                            <asp:ListItem Value="10">10 months</asp:ListItem>
                            <asp:ListItem Value="11">11 months</asp:ListItem>
                            <asp:ListItem Value="12">12 months</asp:ListItem>
                            <asp:ListItem Value="13">13 months</asp:ListItem>
                            <asp:ListItem Value="14">14 months</asp:ListItem>
                            <asp:ListItem Value="15">15 months</asp:ListItem>
                            <asp:ListItem Value="16">16 months</asp:ListItem>
                            <asp:ListItem Value="17">17 months</asp:ListItem>
                            <asp:ListItem Value="18">18 months</asp:ListItem>
                            <asp:ListItem Value="19">19 months</asp:ListItem>
                            <asp:ListItem Value="20">20 months</asp:ListItem>
                            <asp:ListItem Value="21">21 months</asp:ListItem>
                            <asp:ListItem Value="22">22 months</asp:ListItem>
                            <asp:ListItem Value="23">23 months</asp:ListItem>
                            <asp:ListItem Value="24">24 months</asp:ListItem>
                        </asp:DropDownList>
&nbsp;&nbsp; <b>set expiry to null and add months for ghost account</b></td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Extension types</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:DropDownList ID="ddlExtensionType" runat="server" 
                        DataTextField="EXTENSION_TYPE" DataValueField="EXTENSION_TYPE_ID" 
                        Enabled="False">
                    </asp:DropDownList>
&nbsp;&nbsp;&nbsp;&nbsp;
                    <asp:TextBox ID="tbExtensionDetails" runat="server" Columns="50" 
                        CssClass="Textbox" Enabled="False" Width="50%">Enter further details (optional)</asp:TextBox>
                    
                    &nbsp;</td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    About</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:TextBox ID="tbAbout" runat="server" Width="60%"></asp:TextBox>
                    
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Account creator</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:DropDownList ID="ddlCreatorID" runat="server">
                        <asp:ListItem Value="NewID">Self Created</asp:ListItem>
                        <asp:ListItem Value="AdminsID">Admin Created</asp:ListItem>
                    </asp:DropDownList>
                &nbsp; <b>Only ghost and tmp accounts should be &#39;Admin created&#39;</b></td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    ER Role</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:DropDownList ID="ddlERRole" runat="server">
                        <asp:ListItem Selected="True" Value="0">Reviewer</asp:ListItem>
                        <asp:ListItem Value="1">Admin</asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Last login (into ER4)</td>
                <td style="width: 25%; background-color: #FFFFCC";>
                    &nbsp;
                    <asp:Label ID="lblLastLogin" runat="server" Text="N/A"></asp:Label>
                </td>
                <td style="width: 15%; background-color: #B6C6D6";>
                    Archie ID</td>
                <td style="width: 35%; background-color: #FFFFCC";>
                    <asp:TextBox ID="tbArchieID" runat="server" Width="95%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Total hours logged in</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    &nbsp;&nbsp;<asp:Label ID="lblHoursActive" runat="server" Text="0"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Contact ID</td>
                <td style="width: 25%; background-color: #FFFFCC"; colspan="2">
                    &nbsp;
                    <asp:Label ID="lblContactID" runat="server" Text="N/A"></asp:Label>
                </td>
                <td style="width: 50%; background-color: #FFFFCC"; colspan="2">
                    <asp:CheckBox ID="cbSendNewsletter" runat="server" AutoPostBack="True" 
                        oncheckedchanged="cbSendNewsletter_CheckedChanged" Text="Send newsletter" />
                &nbsp;<b>(Don&#39;t click until account created!)</b></td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    ER3 Contact ID</td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    &nbsp;
                    <asp:Label ID="lblER3ContactID" runat="server" Text="N/A"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">
                    Extension history&nbsp;&nbsp;
                    <asp:LinkButton ID="lbExtensionHistory" runat="server" Enabled="False" 
                        onclick="lbExtensionHistory_Click">View</asp:LinkButton>
                </td>
                <td style="width: 75%; background-color: #FFFFCC"; colspan="4">
                    <asp:GridView ID="gvExtensionHistory" runat="server" 
                        AutoGenerateColumns="False" EnableModelValidation="True" Visible="False">
                        <Columns>
<asp:BoundField HeaderText="Edit ID" DataField="EXPIRY_EDIT_ID">
                            <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
                                HorizontalAlign="Left" />
                            <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField HeaderText="Date extended" DataField="DATE_OF_EDIT" >
                            <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
                                HorizontalAlign="Left" />
                            <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField HeaderText="Old date" DataField="OLD_EXPIRY_DATE" >
                            <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
                                HorizontalAlign="Left" />
                            <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
<asp:BoundField HeaderText="New date" DataField="NEW_EXPIRY_DATE">
                            <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
                                HorizontalAlign="Left" />
                            <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField HeaderText="Extended by" DataField="CONTACT_NAME" >
                            <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
                                HorizontalAlign="Left" />
                            <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField HeaderText="Reason" DataField="EXTENSION_TYPE" >
                            <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
                                HorizontalAlign="Left" />
                            <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField HeaderText="Notes" DataField="EXTENSION_NOTES" >
                            <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
                                HorizontalAlign="Left" />
                            <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                        </Columns>
                    </asp:GridView>
                </td>
            </tr>
            </table>

            <asp:Button ID="cmdSave" runat="server" Text="Save" 
                onclick="cmdSave_Click" />
            &nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Label ID="lblMissingFields" runat="server" Font-Bold="True" 
                ForeColor="Red" Text="Select an extension type." Visible="False"></asp:Label>
            <br />
            <asp:Label ID="lblPassword" runat="server" 
                Text="Passwords must be at least 8 characters and contain at least one one lower case letter, one upper case letter and one digit."></asp:Label>
            <br />
           

            <asp:Panel ID="pnlShareableReviews" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td width="50%" align="left">
                            <b>Your reviews (both owned and a member of)</b>&nbsp;&nbsp;
                        </td>
                        <td align="right" width="50%">
                            &nbsp;&nbsp;&nbsp; *Find&nbsp;&nbsp;
                            <asp:TextBox ID="tbFilter" runat="server" onkeyup="KeyUp();"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                
           <telerik:RadGrid ID="radGVReviews" runat="server"
                 CssClass="Grid"  Skin="Windows7" AllowPaging="True" AutoGenerateColumns="False" 
                    Height="350px" ResolvedRenderMode="Classic" Width="800px" 
                    onitemdatabound="radGVReviewss_ItemDataBound" 
                    AllowSorting="True"
                    onneeddatasource="radGVReviews_NeedDataSource" 
                    onpageindexchanged="radGVReviews_PageIndexChanged" PageSize="12">
                  
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
                           Resizable="false" AllowFiltering="False" DataType="System.Int16" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="100px" />
                       </telerik:GridBoundColumn>
                       <telerik:GridTemplateColumn DataField="REVIEW_NAME" HeaderText="*Review" 
                           UniqueName="Review" Resizable="True"  AllowFiltering="False" 
                           SortExpression="REVIEW_NAME" >  
                            <ItemTemplate>  
                                <asp:HyperLink ID="Review1" runat="server"  Text='<%# Bind("REVIEW_NAME") %>' 
                                NavigateUrl='<%# "~/ReviewDetails.aspx?id=" + Eval("REVIEW_ID")  %>'  
                                DataFormatString="<nobr>{0}</nobr>">HyperLink</asp:HyperLink>  
                            </ItemTemplate>  
                            <HeaderStyle Width="480px" BackColor="#B6C6D6" ForeColor="Black"  />
                            
                        </telerik:GridTemplateColumn> 
                       <telerik:GridBoundColumn DataField="DATE_CREATED" 
                           FilterControlAltText="Filter column column" HeaderText="Date created" 
                           UniqueName="column" DataFormatString="<nobr>{0:dd/MM/yyyy}</nobr>" Resizable="False" 
                           AllowFiltering="False" DataType="System.DateTime" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="110px"/>
                       </telerik:GridBoundColumn>
                       <telerik:GridBoundColumn DataField="HOURS" 
                           FilterControlAltText="Filter column3 column" HeaderText="Hours logged in" 
                           UniqueName="column3" DataFormatString="<nobr>{0}</nobr>" 
                           Resizable="False" AllowFiltering="False" DataType="System.Int16" >
                           <HeaderStyle BackColor="#B6C6D6" ForeColor="Black" Width="110px"/>
                       </telerik:GridBoundColumn>
                       
                   </Columns>
               </MasterTableView>
            </telerik:RadGrid>
            </asp:Panel>
    
            <br />

            <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server" OnAjaxRequest="RadAjaxManager1_AjaxRequest">
                <AjaxSettings>
                    <telerik:AjaxSetting AjaxControlID="radGVReviews">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVReviews" LoadingPanelID="RadAjaxLoadingPanel1" />
                        </UpdatedControls>
                    </telerik:AjaxSetting>

                    <telerik:AjaxSetting AjaxControlID="RadAjaxManager1">
                        <UpdatedControls>
                            <telerik:AjaxUpdatedControl ControlID="radGVReviews" LoadingPanelID="RadAjaxLoadingPanel1" />
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
