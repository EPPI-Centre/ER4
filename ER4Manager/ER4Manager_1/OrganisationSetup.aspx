<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="OrganisationSetup.aspx.cs" Inherits="OrganisationSetup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

            <script language="javascript" type="text/javascript">
    var DetailsWindow = null;
    
    function openCalendar1(date)
	{
		var iWidthOfWin = 270;
		var iHeightOfWin = 300;
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

function openAdminList(ID) 
{
    var iWidthOfWin = 800;
    var iHeightOfWin = 400;
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
 
   </script>       
        
        
        <div>

            <asp:Panel ID="pnlSiteLicences" runat="server">
                <br />
                <b>All organisations&nbsp; </b>
                <asp:LinkButton ID="lbNewOrganisation" runat="server"
                    OnClick="lbNewOrganisation_Click">New organisation</asp:LinkButton>
                <asp:GridView ID="gvOrganisations" runat="server" AutoGenerateColumns="False"
                    DataKeyNames="CONTACT_ID" EnableModelValidation="True"
                    OnRowCommand="gvOrganisations_RowCommand"
                    Width="100%" OnRowEditing="gvOrganisations_RowEditing" AllowPaging="True"
                    OnPageIndexChanging="gvOrganisations_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="ORGANISATION_ID" HeaderText="License ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="ORGANISATION_NAME" HeaderText="Name">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="CONTACT_NAME" HeaderText="License Adm">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:ButtonField CommandName="EDT" HeaderText="Details" Text="Details">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:ButtonField>
                    </Columns>
                    <PagerSettings Mode="NumericFirstLast" />
                    <PagerStyle BackColor="#B6C6D6" HorizontalAlign="Center" />
                </asp:GridView>
                <br />
            </asp:Panel>


            <asp:Panel ID="pnlOrganisation" runat="server" Visible="False">
                <b>Organisation</b><br />
                <table id="Table1" border="1" cellpadding="1" cellspacing="1" width="100%">
                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%; height: 27px;">Organisation name *</td>
                        <td style="width: 25%; background-color: #FFFFCC; height: 27px;">
                            <asp:TextBox ID="tbOrganisationName" runat="server" Width="95%"></asp:TextBox>
                        </td>
                        <td style="background-color: #B6C6D6; width: 20%; height: 27px;">Organisation ID</td>
                        <td style="background-color: #FFFFCC; height: 27px;">&nbsp;
                                                        <asp:Label ID="lblOrganisationID" runat="server" Text="N/A"></asp:Label>
                        </td>
                    </tr>

                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%;">Address *</td>
                        <td colspan="3" style="width: 75%; background-color: #FFFFCC">
                            <asp:TextBox ID="tbAddress" runat="server" TextMode="MultiLine" Width="90%"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%;">Telephone *</td>
                        <td style="width: 25%; background-color: #FFFFCC">
                            <asp:TextBox ID="tbTelephone" runat="server"></asp:TextBox>
                        </td>
                        <td style="background-color: #B6C6D6; width: 20%;">Notes</td>
                        <td style="width: 25%; background-color: #FFFFCC">
                            <asp:TextBox ID="tbNotes" runat="server" TextMode="MultiLine" Width="90%"></asp:TextBox>
                        </td>
                    </tr>

                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%;" valign="middle">Date created *&nbsp;&nbsp;
                                                        <asp:Button ID="cmdPlaceDate" runat="server" BackColor="#B6C6D6" BorderColor="#B6C6D6" BorderStyle="None" ForeColor="#B6C6D6" Height="1px" OnClick="cmdPlaceDate_Click" Style="font-weight: bold" Width="1px" />
                        </td>
                        <td style="width: 25%; background-color: #FFFFCC;" valign="bottom">
                            <asp:TextBox ID="tbDateOrganisationCreated" runat="server" Width="100px"></asp:TextBox>
                            &nbsp;&nbsp;<asp:ImageButton ID="IBCalendar1" runat="server" ImageUrl="~/images/calbtn.gif" />
                        </td>
                        <td style="background-color: #B6C6D6; width: 20%;" valign="middle">Created by</td>
                        <td style="width: 25%; background-color: #FFFFCC;" valign="middle">&nbsp;&nbsp;
                                                        <asp:Label ID="lblCreatedBy" runat="server" Text="N/A"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%; height: 27px;">First administrator&nbsp;*&nbsp; <b>
                            <asp:LinkButton ID="lbSelectAdmin" runat="server">Select</asp:LinkButton>
                            &nbsp;&nbsp;
                        </b>

                            <asp:Button ID="cmdPlaceFunder" runat="server" BackColor="#B6C6D6"
                                BorderColor="#B6C6D6" BorderStyle="None" ForeColor="#B6C6D6" Height="1px"
                                OnClick="cmdPlaceFunder_Click" Style="font-weight: bold" Width="1px" />
                        </td>
                        <td style="width: 25%; background-color: #FFFFCC; height: 27px;">&nbsp;
                                                        <asp:Label ID="lblInitialAdministrator" runat="server" Text="N/A"></asp:Label>
                            &nbsp;&nbsp;
                                                        <asp:Label ID="lblAdminID" runat="server" Text="N/A" Visible="False"></asp:Label>
                            &nbsp;</td>
                        <td style="background-color: #B6C6D6; width: 20%;">Is Public?</td>
                        <td style="width: 25%; background-color: #FFFFCC; height: 27px;">
                            <asp:RadioButtonList ID="rblIsOrganisationPublic" runat="server" AutoPostBack="True" OnSelectedIndexChanged="rblIsOrganisationPublic_SelectedIndexChanged" RepeatDirection="Horizontal">
                                <asp:ListItem Selected="True" Value="0">Yes</asp:ListItem>
                                <asp:ListItem Value="1">No</asp:ListItem>
                            </asp:RadioButtonList>
                        </td>
                    </tr>

                </table>
                <asp:Button ID="cmdSaveOrganisation" runat="server" OnClick="cmdSaveOrganisation_Click"
                    Text="Save Organisation" Width="120px" />
                &nbsp;&nbsp;
                                            <asp:Label ID="lblOrganisationMessage" runat="server" Font-Bold="False"
                                                Text="Required fields *" Visible="False"></asp:Label>
            </asp:Panel>
                                        
                                         
                                        <asp:Panel ID="pnlOrganisationDetails" runat="server" Visible="False">
                                       
                                            
                                            <table id="Table2" border="1" cellpadding="1" cellspacing="1" width="100%">
                                                <tr>
                                                    <td style="background-color: #B6C6D6; width: 25%;">Organisation history&nbsp;&nbsp;
                                                        <asp:LinkButton ID="lbOrganisationHistory" runat="server" onclick="lbOrganisationHistory_Click">View</asp:LinkButton>
                                                    </td>
                                                    <td ;="" colspan="3" style="background-color: #FFFFCC">
                                                        <asp:GridView ID="gvOrganisationHistory" runat="server" AutoGenerateColumns="False" EnableModelValidation="True" Visible="False">
                                                            <Columns>
                                                                <asp:BoundField DataField="SITE_LIC_DETAILS_ID" HeaderText="Package ID">
                                                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" HorizontalAlign="Left" />
                                                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="CONTACT_ID" HeaderText="Changed by">
                                                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" HorizontalAlign="Left" />
                                                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="AFFECTED_ID" HeaderText="Affected ID">
                                                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" HorizontalAlign="Left" />
                                                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="CHANGE_TYPE" HeaderText="Change">
                                                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" HorizontalAlign="Left" />
                                                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="REASON" HeaderText="Reason">
                                                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" HorizontalAlign="Left" />
                                                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                                                </asp:BoundField>
                                                                <asp:BoundField DataField="DATE" HeaderText="Date">
                                                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" HorizontalAlign="Left" />
                                                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                                                </asp:BoundField>
                                                            </Columns>
                                                        </asp:GridView>
                                                    </td>
                                                </tr>
                                            </table>
                                        <br />
                                        </asp:Panel>
                                   





            <asp:Panel ID="pnlAccountsAndReviews" runat="server" Visible="False">
                <table style="width:100%;">
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;">
                            <b>Accounts in organisation</b></td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;">
                            <b>Reviews in organisation</b></td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%; height: 114px;" valign="top">
                            <asp:GridView ID="gvAccounts" runat="server" 
                                AutoGenerateColumns="False" 
                                DataKeyNames="EMAIL" EnableModelValidation="True" 
                                onrowcommand="gvAccounts_RowCommand" Width="100%" 
                                onrowdatabound="gvAccounts_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="CONTACT_ID" HeaderText="ContactID">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="CONTACT_NAME" HeaderText="Name">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="EMAIL" HeaderText="Email address">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:ButtonField CommandName="REMOVE" HeaderText="Remove" Text="Remove">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:ButtonField>
                                </Columns>
                            </asp:GridView>
                        </td>
                        <td style="background-color: #ffffff; width: 10%; height: 114px;" valign="top">
                            </td>
                        <td style="background-color: #ffffff; width: 45%; height: 114px;" valign="top">
                            <asp:GridView ID="gvReviews" runat="server" 
                                AutoGenerateColumns="False" 
                                DataKeyNames="REVIEW_ID" EnableModelValidation="True" Width="100%" 
                                onrowcommand="gvReviews_RowCommand" onrowdatabound="gvReviews_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:ButtonField CommandName="REMOVE" HeaderText="Remove" Text="Remove">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:ButtonField>
                                </Columns>
                            </asp:GridView>
                            (review removal restricted to ER4 adminstrators)</td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%; height: 16px;">
                            Add an account</td>
                        <td style="background-color: #ffffff; width: 10%; height: 16px;">
                            </td>
                        <td style="background-color: #ffffff; width: 45%; height: 16px;">
                            Add a review&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%; border-bottom:solid 1px black;">
                            <asp:TextBox ID="tbEmail" runat="server" Width="60%">Enter email address</asp:TextBox>
                            &nbsp;&nbsp;
                            <asp:Button ID="cmdAddAccount" runat="server" Text="Add account" 
                                onclick="cmdAddAccount_Click" style="width: 109px" />
                            <br />
                            <asp:Label ID="lblAccountMsg" runat="server" Text="Label" Visible="False"></asp:Label>
                        </td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%; border-bottom:solid 1px black;">
                            <asp:TextBox ID="tbReviewID" runat="server">Enter Review ID</asp:TextBox>
                            &nbsp;&nbsp;
                            <asp:Button ID="cmdAddReview" runat="server" Text="Add review" 
                                onclick="cmdAddReview_Click" />
                            <br />
                            <asp:Label ID="lblReviewMsg" runat="server" Text="Label" Visible="False"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%; height: 17px;">
                            <b>Organisation administrators</b></td>
                        <td style="background-color: #ffffff; width: 10%; height: 17px;">
                            </td>
                        <td style="background-color: #ffffff; width: 45%; height: 17px;">
                            </td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;" valign="top">
                            <asp:GridView ID="gvOrganisationAdms" runat="server" AutoGenerateColumns="False" 
                                DataKeyNames="EMAIL" EnableModelValidation="True" 
                                onrowcommand="gvLicenseAdms_RowCommand" onrowdatabound="gvLicenseAdms_RowDataBound" 
                                Width="100%">
                                <Columns>
                                    <asp:BoundField DataField="CONTACT_ID" HeaderText="ContactID">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="CONTACT_NAME" HeaderText="Name">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="EMAIL" HeaderText="Email address">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:ButtonField CommandName="REMOVE" HeaderText="Remove" Text="Remove">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:ButtonField>
                                </Columns>
                            </asp:GridView>
                        </td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;" valign="top">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%; height: 17px;">
                            Add an admin</td>
                        <td style="background-color: #ffffff; width: 10%; height: 17px;">
                            </td>
                        <td style="background-color: #ffffff; width: 45%; height: 17px;">
                            </td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;">
                            <asp:TextBox ID="tbEmailAdm" runat="server" Width="60%">Enter email address</asp:TextBox>
                            &nbsp;&nbsp;
                            <asp:Button ID="cmdAddAdm" runat="server" onclick="cmdAddAdm_Click" 
                                style="width: 109px" Text="Add admin" />
                        </td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;">
                            <asp:Label ID="lblAccountAdmMessage" runat="server" Text="Label" 
                                Visible="False"></asp:Label>
                        </td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;">
                            &nbsp;</td>
                    </tr>
                </table>
                <br />
            </asp:Panel>
            <br />
            <br />

            <br />
            <br />
            <br />




                    </div>
</asp:Content>

