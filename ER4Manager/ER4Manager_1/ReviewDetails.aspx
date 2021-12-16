<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="ReviewDetails.aspx.cs" Inherits="ReviewDetails" Title="Review details" %>
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

function openFunderList(ID) 
{
    var iWidthOfWin = 800;
    var iHeightOfWin = 450;
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

function openReviewerList(ID) {
    var iWidthOfWin = 800;
    var iHeightOfWin = 450;
    var iLocX = (screen.width - iWidthOfWin) / 2;
    var iLocY = (screen.height - iHeightOfWin) / 2;

    var strFeatures = "scrollbars=yes,self.focus(), resizable=yes "
                 + ",width=" + iWidthOfWin
                 + ",height=" + iHeightOfWin
                 + ",screenX=" + iLocX
                 + ",screenY=" + iLocY
                 + ",left=" + iLocX
                 + ",top=" + iLocY;

    var theURL = "SelectReviewer.aspx?contact=" + ID;
    windowName = new String(Math.round(Math.random() * 100000));
    DetailsWindow = window.open(theURL, windowName, strFeatures);
}


	
    </script>
        <div>
    
            <b>Review details&nbsp;
            
                
                <asp:Button ID="cmdPlaceDate" runat="server" BackColor="White" 
                BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px" 
                OnClick="cmdPlaceDate_Click" style="font-weight: bold" Width="1px" />
                
                <asp:Button ID="cmdPlaceFunder" runat="server" BackColor="White" 
                BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px" 
                OnClick="cmdPlaceFunder_Click" style="font-weight: bold" Width="1px" />
                
                <asp:Button ID="cmdAddContact" runat="server" BackColor="White" 
                BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px" 
                OnClick="cmdAddContact_Click" style="font-weight: bold" Width="1px" />
            </b>
    

                
            <br />
            <table id="Table2" border="1" cellpadding="1" cellspacing="1" width="800">
            <tr>
                <td style="width: 20%; background-color: #B6C6D6">
                    Review title *</td>
                <td style="width: 80%; background-color: #FFFFCC">
                    <asp:TextBox ID="tbReviewTitle" runat="server" Width="600px" CssClass="Textbox" 
                        Rows="2" TextMode="MultiLine"></asp:TextBox></td>
            </tr>
            </table>
        
    <table id="Table3" border="1" cellpadding="1" cellspacing="1" width="800">
                <tr>
                    <td style="width: 20%; background-color: #B6C6D6" rowspan="2">
                        Date created *</td>
                    <td style="width: 30%;background-color: #FFFFCC" rowspan="2">
                        <asp:TextBox ID="tbRegistrationDate" runat="server" CssClass="Textbox"></asp:TextBox>&nbsp;&nbsp;
                        <asp:ImageButton ID="IBCalendar1" runat="server" 
                            ImageUrl="~/images/calbtn.gif" /></td>
                    <td style="width: 20%; background-color: #B6C6D6" colspan="2">
                        Expiry date<br />
                        <asp:Label ID="lblInSiteLicense" runat="server" Font-Bold="True" 
                            Text="In license #" Visible="False"></asp:Label>
                    </td>
                    <td style="width: 30%; background-color: #FFFFCC">
                        <b>null means non-shareable</b>
                        <asp:TextBox ID="tbExpiryDate" runat="server" CssClass="Textbox"></asp:TextBox>&nbsp;&nbsp;<asp:ImageButton ID="IBCalendar2" runat="server" 
                            ImageUrl="~/images/calbtn.gif" />&nbsp;
                        <asp:Button ID="cmdNullExpiry" runat="server" onclick="cmdNullExpiry_Click" 
                            Text="Null" />
                        <br />
                    </td>
                </tr>
                <tr>
                    <td style="width: 20%; background-color: #B6C6D6" colspan="2">
                        Months credit</td>
                    <td style="width: 30%; background-color: #FFFFCC">
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
&nbsp; <b>set expiry to null and</b><br />
                        <b>add months for ghost review</b></td>
                </tr>
                <tr>
                    <td style="width: 20%; background-color: #B6C6D6">
                        Extension types</td>
                    <td colspan="4">
                    <asp:DropDownList ID="ddlExtensionType" runat="server" 
                        DataTextField="EXTENSION_TYPE" DataValueField="EXTENSION_TYPE_ID" 
                        Enabled="False">
                    </asp:DropDownList>
&nbsp;&nbsp;
                    <asp:TextBox ID="tbExtensionDetails" runat="server" Columns="60" 
                        CssClass="Textbox" Enabled="False">Enter further details (optional)</asp:TextBox>
                    
                    </td>
                </tr>
                <tr>
                    <td style="width: 20%; background-color: #B6C6D6">
            Review ID</td>
                    <td style="width: 30%; background-color: #FFFFCC">
                        &nbsp;
                        <asp:Label ID="lblReviewID" runat="server" Text="N / A"></asp:Label>
                    </td>
                    <td style="width: 20%; background-color: #B6C6D6" colspan="2">
                        Funder ID</td>
                    <td style="width: 30%; background-color: #FFFFCC">
                        <asp:Label ID="lblFunderID" runat="server" Text="0"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td style="width: 20%; background-color: #B6C6D6">
                        Review number</td>
                    <td style="width: 30%; background-color: #FFFFCC">
                        <asp:TextBox ID="tbReviewNumber" runat="server" CssClass="Textbox">N/A</asp:TextBox>
                        &nbsp;</td>
                    <td style="width: 20%; background-color: #B6C6D6" colspan="2">
                        Funder name *&nbsp;&nbsp;
            <b> <asp:LinkButton ID="lbSelectFunder" 
                runat="server">Select</asp:LinkButton>
                        </b>
                    </td>
                    <td style="width: 30%; background-color: #FFFFCC">
                        <asp:Label ID="lblFunderName" runat="server" Text="Name"></asp:Label>
                    &nbsp;</td>
                </tr>
                <tr>
                    <td style="width: 20%; background-color: #B6C6D6">
                        ER3 Review ID &nbsp;</td>
                    <td style="width: 30%; background-color: #FFFFCC">
                        &nbsp;
                        <asp:Label ID="lblER3ReviewID" runat="server"></asp:Label>
                    </td>
                    <td style="width: 20%; background-color: #B6C6D6" colspan="2">
                        ER3 Review group ID</td>
                    <td style="width: 30%; background-color: #FFFFCC">
                        &nbsp;
                        <asp:Label ID="lblER3ReviewGroupID" runat="server"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td style="width: 20%; background-color: #E3EAF0; text-align: right;" 
                        valign="top" align="right">
                        <b>Cochrane reviews&nbsp;&nbsp; <br />
                        <br />
                        </b>Archie ID&nbsp;&nbsp;
                        <br />
                        Null = not Cochrane&nbsp;&nbsp;</td>
                    <td style="width: 30%; background-color: #E3EAF0" valign="top">
                        <asp:CheckBox ID="cbPotential" runat="server" AutoPostBack="True" 
                            oncheckedchanged="cbPotential_CheckedChanged" Text="Potential Cochrane review" 
                            ToolTip="(Not auto save)" />
                        <br />
                        <asp:TextBox ID="tbArchieID" runat="server"></asp:TextBox>
                        &nbsp;&nbsp;
                        <asp:Button ID="cmdNullExpiry0" runat="server" onclick="cmdNullExpiry0_Click" 
                            Text="Null" />
                        <br />
                        <asp:LinkButton ID="lbSave" runat="server" onclick="lbSave_Click" 
                            ToolTip="Save the value in the textbox">Save</asp:LinkButton>
                    </td>
                    <td style="width: 20%; background-color: #E3EAF0; vertical-align: top; text-align: right;" 
                        colspan="2">
                        &nbsp;
                        Archie CD&nbsp;&nbsp;<br />
                        <br />
                        <br />
                        <br />
                        Checked out by (ID)&nbsp;&nbsp;</td>
                    <td style="width: 30%; background-color: #E3EAF0">
                        
&nbsp;<asp:TextBox ID="tbArchieCD" runat="server"></asp:TextBox>
&nbsp;&nbsp;&nbsp; <asp:LinkButton ID="lbSave0" runat="server" onclick="lbSave0_Click" 
                            ToolTip="Save the value in the textbox">Save</asp:LinkButton>
                        <br />
                        <asp:CheckBox ID="cbIsCheckedOutHere" runat="server" 
                            oncheckedchanged="cbIsCheckedOutHere_CheckedChanged" Text="Is checked out here" 
                            ToolTip="(auto save)" AutoPostBack="True" />
                        <br />
                        <asp:TextBox ID="tbCheckedOutBy" runat="server"></asp:TextBox>
&nbsp;&nbsp;&nbsp; <asp:LinkButton ID="lbSave1" runat="server" onclick="lbSave1_Click" 
                            ToolTip="Save the value in the textbox">Save</asp:LinkButton>
                    </td>
                </tr>
                <tr>
                    <td style="width: 20%; background-color: #B6C6D6">
                        Priority screening</td>
                    <td style="background-color: #FFFFCC" colspan="3">
                        <asp:CheckBox ID="cbShowScreening" runat="server" AutoPostBack="True" 
                            oncheckedchanged="cbShowScreening_CheckedChanged" Text="SHOW_SCREENING" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:CheckBox ID="cbAllowReviewerTerms" runat="server" AutoPostBack="True" 
                            oncheckedchanged="cbAllowReviewerTerms_CheckedChanged" 
                            Text="ALLOW_REVIEWER_TERMS" /><br />
                        <asp:CheckBox ID="cbAllowClusteredSearch" runat="server" AutoPostBack="True" 
                            oncheckedchanged="cbAllowClusteredSearch_CheckedChanged" 
                            Text="ALLOW_CLUSTERED_SEARCH" />
                    </td>
                    <td style="background-color: #FFFFCC" colspan="4">
                        <asp:CheckBox ID="cbEnableMag" runat="server" AutoPostBack="True" 
                            oncheckedchanged="cbEnableMag_CheckedChanged" Text="ENABLE MAG" />
                        </td>
                </tr>
                <tr>
                    <td style="width: 20%; background-color: #B6C6D6">
                        British library</td>
                    <td style="background-color: #FFFFCC" colspan="2" valign="top">
                        
                        Library privilege copies&nbsp;&nbsp;
                        <asp:LinkButton ID="lbSaveBritLibPrivilege" runat="server" 
                            onclick="lbSaveBritLibPrivilege_Click">Save</asp:LinkButton>
                        <br />
                        <asp:TextBox ID="tbBritLibPrivilegeAccountCode" runat="server" Width="50%"></asp:TextBox>
                        Account code
                        <br />
                        <asp:TextBox ID="tbBritLibPrivilegeAuthCode" runat="server" Width="50%"></asp:TextBox>
                        Authorisation code
                        <br />
                        <asp:TextBox ID="tbBritLibPrivilegeTxLine" runat="server" Width="50%"></asp:TextBox>
                        TX line to export</td>
                    <td style="background-color: #FFFFCC" colspan="2" valign="top">
                        Copyright cleared copies&nbsp;&nbsp;
                        <asp:LinkButton ID="lbSaveBritLibCRCleared" runat="server" 
                            onclick="lbSaveBritLibCopyrightCleard_Click">Save</asp:LinkButton>
                        <br />
                        <asp:TextBox ID="tbBritLibCRClearedAccountCode" runat="server" Width="50%"></asp:TextBox>
                        Account code:<br />
                        <asp:TextBox ID="tbBritLibCRClearedAuthCode" runat="server" Width="50%"></asp:TextBox>
                        Authorisation code<br />
                        <asp:TextBox ID="tbBritLibCRClearedTxLine" runat="server" Width="50%"></asp:TextBox>
                        TX line to export</td>
                </tr>
                <tr>
                    <td style="width: 20%; background-color: #B6C6D6">
                    Extension history&nbsp;&nbsp;
                          <asp:LinkButton ID="lbExtensionHistory" runat="server" Enabled="False" 
                        onclick="lbExtensionHistory_Click">View</asp:LinkButton>
                    </td>
                    <td style="background-color: #FFFFCC" colspan="4">
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
            <asp:Button ID="cmdSave" runat="server" Text="Save" onclick="cmdSave_Click" />
            &nbsp;
            <asp:Label ID="lblMissingFields" runat="server" 
                Text="Please fill in all of the fields" Visible="False" Font-Bold="True"></asp:Label>
            <br />
            <br />
            <b>Review members</b>&nbsp;&nbsp;&nbsp;
            <b> 
            <asp:LinkButton ID="lbAddReviewer" 
                runat="server" Visible="False">Add a user</asp:LinkButton>
                        &nbsp;</b><br />
            <asp:GridView ID="gvContacts" runat="server" AutoGenerateColumns="False" 
                DataKeyNames="CONTACT_ID" onrowcommand="gvContacts_RowCommand" CssClass="grviewFixedWidth"
                onrowdatabound="gvContacts_RowDataBound" EnableModelValidation="True">
                <Columns>
                    <asp:BoundField DataField="CONTACT_ID" HeaderText="ContactID">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:HyperLinkField DataNavigateUrlFields="CONTACT_ID" 
                        DataNavigateUrlFormatString="ContactDetails.aspx?ID={0}" 
                        DataTextField="CONTACT" HeaderText="Name">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:HyperLinkField>
                    <asp:BoundField DataField="EMAIL" HeaderText="Email">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="EXPIRY_DATE" HeaderText="Expiry date">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last access">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:TemplateField HeaderText="Role">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlRole" runat="server" AutoPostBack="True"
                                OnSelectedIndexChanged="ddlRole_SelectedIndexChanged" >
                                <asp:ListItem Value="1">Review admin</asp:ListItem>
                                <asp:ListItem Value="4">Reviewer</asp:ListItem>
                                <asp:ListItem Value="2">Coding only</asp:ListItem>
                                <asp:ListItem Value="3">Read only</asp:ListItem>
                                </asp:DropDownList>
                        </ItemTemplate>
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:TemplateField>
                    <asp:ButtonField CommandName="REMOVE" HeaderText="Remove from&lt;br&gt;review" 
                        Text="Remove">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:ButtonField>
                    <asp:BoundField DataField="HOURS" HeaderText="Hours">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>

                    <asp:TemplateField HeaderText="Review role" Visible="false">
                        <ItemTemplate>
                            <asp:CheckBoxList ID="cblContactReviewRole" runat="server" 
                                DataValueField="ROLE_NAME">
                            </asp:CheckBoxList>
                        </ItemTemplate>
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:TemplateField>
                    <asp:ButtonField CommandName="SAVE_ROLE" HeaderText="Save review&lt;br&gt;role" 
                        Text="Save role" Visible="false">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:ButtonField>

                    
                </Columns>
            </asp:GridView>
            <br /><b>Detailed extension history</b>&nbsp;&nbsp;
            <asp:LinkButton ID="lbShowHide" runat="server" Visible="True" Text="Show" OnClick="lbShowHide_Click"></asp:LinkButton>
            <br />
            <asp:Panel runat="server" ID="pnlDetailedExtensionHistory" Visible="false">

<!--
CONTACT_NAME
CONTACT_ID
EMAIL
EXPIRY_EDIT_ID
DATE_OF_EDIT
TYPE_EXTENDED
ID_EXTENDED
OLD_EXPIRY_DATE
NEW_EXPIRY_DATE
EXTENDED_BY_ID
EXTENSION_TYPE_ID
EXTENSION_NOTES
EXTENSION_TYPE
Months_Ext
£
-->
                <b>Contact details  (not restricted to this review!)</b><br />
                <asp:GridView ID="gvDetailedContactExtension" runat="server" AutoGenerateColumns="False"
                    DataKeyNames="CONTACT_ID" EnableModelValidation="True">
                    <Columns>
                        <asp:BoundField DataField="CONTACT_ID" HeaderText="ContactID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="CONTACT_NAME" HeaderText="Name">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EMAIL" HeaderText="Email">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXPIRY_EDIT_ID" HeaderText="Edit ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="DATE_OF_EDIT" HeaderText="Edit date">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="TYPE_EXTENDED" HeaderText="Ext. Type">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="OLD_EXPIRY_DATE" HeaderText="Old exp. date">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="NEW_EXPIRY_DATE" HeaderText="New exp. date">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXTENDED_BY_ID" HeaderText="Ext. by ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXTENSION_TYPE_ID" HeaderText="ext. type ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXTENSION_NOTES" HeaderText="Ext. notes">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXTENSION_TYPE" HeaderText="Ext. type">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="Months_Ext" HeaderText="Months ext.">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="£" HeaderText="£">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                    </Columns>
                </asp:GridView>
                <br />
                <b>Review details</b><br />
                <asp:GridView ID="gvDetailedReviewExtension" runat="server" AutoGenerateColumns="False"
                    DataKeyNames="REVIEW_ID" EnableModelValidation="True">
                    <Columns>
                        <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXPIRY_EDIT_ID" HeaderText="Edit ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="DATE_OF_EDIT" HeaderText="Edit date">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="TYPE_EXTENDED" HeaderText="Ext. Type">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="OLD_EXPIRY_DATE" HeaderText="Old exp. date">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="NEW_EXPIRY_DATE" HeaderText="New exp. date">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXTENDED_BY_ID" HeaderText="Ext. by ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXTENSION_TYPE_ID" HeaderText="ext. type ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXTENSION_NOTES" HeaderText="Ext. notes">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXTENSION_TYPE" HeaderText="Ext. type">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="Months_Ext" HeaderText="Months ext.">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="£" HeaderText="£">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                    </Columns>
                </asp:GridView>


<!--
REVIEW_NAME
REVIEW_ID
EXPIRY_EDIT_ID
DATE_OF_EDIT
TYPE_EXTENDED
ID_EXTENDED
OLD_EXPIRY_DATE
NEW_EXPIRY_DATE
EXTENDED_BY_ID
EXTENSION_TYPE_ID
EXTENSION_NOTES
EXTENSION_TYPE
Months_Ext
£
-->


            </asp:Panel>
            <br />
            <br />

<!--
CONTACT_NAME
CONTACT_ID
EMAIL
EXPIRY_EDIT_ID
DATE_OF_EDIT
TYPE_EXTENDED
ID_EXTENDED
OLD_EXPIRY_DATE
NEW_EXPIRY_DATE
EXTENDED_BY_ID
EXTENSION_TYPE_ID
EXTENSION_NOTES
EXTENSION_TYPE
Months_Ext
£
-->


    </div>
</asp:Content>