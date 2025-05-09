﻿<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="SiteLicenseDetails.aspx.cs" Inherits="SiteLicenseDetails" Title="Details" %>

<%@ Register Assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">




    <script language="javascript" type="text/javascript">
        var DetailsWindow = null;

        function openCalendar1(date) {
            var iWidthOfWin = 270;
            var iHeightOfWin = 290;
            var iLocX = (screen.width - iWidthOfWin) / 2;
            var iLocY = (screen.height - iHeightOfWin) / 2;

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

        function openAdminList(ID) {
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
    <script src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js" integrity="sha384-ApNbgh9B+Y1QKtv3Rn7W3mgPxhU9K/ScQsAP7hUibX39j7fakFPskvXusvfa0b4Q" crossorigin="anonymous"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" integrity="sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl" crossorigin="anonymous"></script>




    <asp:Panel ID="pnlSiteLicense" runat="server" Visible="False">

        <b>Site license</b>
        &nbsp;&nbsp;
            <button type="button" class="btn btn-outline-secondary btn-sm" data-toggle="collapse" data-target="#demo">Help</button>
        <br />

        <div class="row">
            <div class="col-md-12">
                <div class="collapse" id="demo">
                    <div class="card card-body bg-light">
                        <asp:Label ID="lblLicenseDetailsHelp" runat="server" Text=""></asp:Label>
                    </div>
                </div>
            </div>
        </div>



        <table id="Table1" border="1" cellpadding="1" cellspacing="1" width="100%">
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Licence name *</td>
                <td style="width: 20%; background-color: #FFFFCC">
                    <asp:TextBox ID="tbLicenseName" runat="server" Width="95%"></asp:TextBox>
                </td>
                <td style="background-color: #B6C6D6; width: 20%;">Licence ID</td>
                <td style="background-color: #FFFFCC">&nbsp;
                                                        <asp:Label ID="lblSiteLicID" runat="server" Text="N/A"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Organisation *</td>
                <td colspan="3" style="width: 75%; background-color: #FFFFCC">
                    <asp:TextBox ID="tbOrganisation" runat="server" Width="90%"></asp:TextBox>
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
                <td style="background-color: #B6C6D6; width: 20%;">Notes:<br />
                    These are visible to the license owners.</td>
                <td style="width: 25%; background-color: #FFFFCC">
                    <asp:TextBox ID="tbNotes" runat="server" TextMode="MultiLine" Width="90%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%; height: 24px;"
                    valign="middle">License model *
                    &nbsp;&nbsp;
                    (<asp:Label ID="lblModelType" runat="server" Text="0" Visible="true"></asp:Label>)
                </td>
                <td style="width: 25%; background-color: #FFFFCC; height: 24px;" valign="bottom">
                    <asp:DropDownList ID="ddlLicenseModel" runat="server" DataTextField="DATE_CREATED" DataValueField="SITE_LIC_DETAILS_ID"  Width="90%">
                        <asp:ListItem Value="1" Selected="True">Fixed reviews</asp:ListItem>
                        <asp:ListItem Value="2">Removable reviews</asp:ListItem>
                    </asp:DropDownList>
                </td>
                <td style="background-color: #B6C6D6; width: 20%; height: 24px;" valign="middle">Change review owner</td>
                <td style="width: 25%; background-color: #FFFFCC; height: 24px;"
                    valign="middle">
                    <asp:CheckBox ID="cbAllowReviewOwnershipChange" runat="server" AutoPostBack="True" OnCheckedChanged="cbAllowReviewOwnershipChange_CheckedChanged" Text="Allow" ToolTip="If you check this box the admins in this license can change the ownership of the reviews in the license" />
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;" valign="middle">Date created *&nbsp;&nbsp;
                                                        <asp:Button ID="cmdPlaceDate" runat="server" BackColor="#B6C6D6" BorderColor="#B6C6D6" BorderStyle="None" ForeColor="#B6C6D6" Height="1px" OnClick="cmdPlaceDate_Click" Style="font-weight: bold" Width="1px" />
                </td>
                <td style="width: 25%; background-color: #FFFFCC;" valign="bottom">
                    <asp:TextBox ID="tbDateLicenseCreated" runat="server" Width="100px"></asp:TextBox>
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
                <td style="background-color: #B6C6D6; width: 20%;">Packages: past, present &amp; offers
                    &nbsp;&nbsp;<asp:LinkButton ID="lbRemoveOffer" runat="server" Visible="false" OnClick="lbRemoveOffer_Click">Delete offer</asp:LinkButton>
                    <br />
                    (based on date created)</td>
                <td style="width: 25%; background-color: #99FF99; height: 27px;">
                    <asp:DropDownList ID="ddlPackages" runat="server" AutoPostBack="True"
                        DataTextField="DATE_CREATED" DataValueField="SITE_LIC_DETAILS_ID"
                        OnSelectedIndexChanged="ddlPackages_SelectedIndexChanged" Width="90%">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%; height: 27px;">EPPI Notes<br /> (Only visible to EPPI admns)<br />
                    <asp:TextBox ID="tbEPPINotes" runat="server" TextMode="MultiLine" Width="95%" Rows="2"></asp:TextBox>
                </td>
                <td style="width: 25%; background-color: #FFFFCC; height: 27px;vertical-align:top">
                    OpenAI:<br />
                    Enter a Purchase ID to give this Site License OpenAI access<br />
                                    <asp:TextBox ID="tbCreditPurchaseID" runat="server" Visible="true" Width="100px"></asp:TextBox>
&nbsp;                 <asp:LinkButton ID="lbSavePurchaseCreditID" runat="server" onclick="lbSavePurchaseCreditID_Click" 
                            ToolTip="Add a purchase credit ID">Add</asp:LinkButton>
                        <b><asp:Label ID="lblInvalidID" runat="server" Text="Invalid ID" Visible="false"></asp:Label></b>                               
                </td>
                <td style="background-color: #FFFFCC; height: 27px;vertical-align:top" colspan="2">
                       OpenAI Credit<br />
<asp:GridView ID="gvCreditForRobots" runat="server" Width="100%" onrowdatabound="gvCreditForRobots_RowDataBound"
    onrowcommand="gvCreditForRobots_RowCommand" AutoGenerateColumns="False" EnableModelValidation="True" 
    DataKeyNames="CREDIT_FOR_ROBOTS_ID" Visible="true">
    <Columns>
        <asp:BoundField HeaderText="Robot Credit ID" DataField="CREDIT_FOR_ROBOTS_ID">
        <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
            HorizontalAlign="Left" />
        <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
        </asp:BoundField>
        <asp:BoundField HeaderText="Purchase ID" DataField="CREDIT_PURCHASE_ID">
        <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
            HorizontalAlign="Left" />
        <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
        </asp:BoundField>
        <asp:BoundField HeaderText="Credit purchaser (ID)" DataField="CREDIT_PURCHASER" >
        <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
            HorizontalAlign="Left" />
        <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
        </asp:BoundField>
        <asp:BoundField HeaderText="Credit remaining" DataField="REMAINING" >
        <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
            HorizontalAlign="Left" />
        <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
        </asp:BoundField>
        <asp:ButtonField CommandName="REMOVE" HeaderText="Remove" 
            Text="Remove">
        <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
            HorizontalAlign="Left"/>
        <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
        </asp:ButtonField>                         
    </Columns>
</asp:GridView>

                </td>

            </tr>

            <tr>
                <td colspan="4" style="background-color: #B6C6D6; height: 27px;">British library codes (optional)&nbsp;&nbsp;
                                                            <asp:LinkButton ID="lbShowBLCodes" runat="server"
                                                                OnClick="lbShowBLCodes_Click">Show/Edit</asp:LinkButton>
                    <br />
                    <asp:Panel ID="pnlBriLibCodes" runat="server" Visible="False">

                        <table style="width: 100%;">
                            <tr>
                                <td style="width: 33%" valign="middle">Reviews added to the license will get these codes unless they already have 
                                                                        codes.<br />
                                    <br />
                                    Editing the license codes will not change the codes for reviews already in the 
                                                                        license. The codes for those reviews can be changed in the <b>Summary</b> page.</td>
                                <td style="width: 33%" valign="middle">Library privilege copies&nbsp;&nbsp; &nbsp;
                                                                        <asp:LinkButton ID="lbSaveBritLibPrivilege" runat="server"
                                                                            OnClick="lbSaveBritLibPrivilege_Click">Save</asp:LinkButton>
                                    <br />
                                    <asp:TextBox ID="tbBritLibPrivilegeAccountCode" runat="server" Width="50%"></asp:TextBox>
                                    Account code
                                                                        <br />
                                    <asp:TextBox ID="tbBritLibPrivilegeAuthCode" runat="server" Width="50%"></asp:TextBox>
                                    Authorisation code
                                                                        <br />
                                    <asp:TextBox ID="tbBritLibPrivilegeTxLine" runat="server" Width="50%"></asp:TextBox>
                                    TX line to export</td>
                                <td style="width: 33%" valign="middle">Copyright cleared copies&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;
                                                                        <asp:LinkButton ID="lbSaveBritLibCRCleared" runat="server"
                                                                            OnClick="lbSaveBritLibCopyrightCleard_Click">Save</asp:LinkButton>
                                    <br />
                                    <asp:TextBox ID="tbBritLibCRClearedAccountCode" runat="server" Width="50%"></asp:TextBox>
                                    Account code:<br />
                                    <asp:TextBox ID="tbBritLibCRClearedAuthCode" runat="server" Width="50%"></asp:TextBox>
                                    Authorisation code<br />
                                    <asp:TextBox ID="tbBritLibCRClearedTxLine" runat="server" Width="50%"></asp:TextBox>
                                    TX line to export</td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
        </table>
        <asp:Button ID="cmdSaveLicense" runat="server" OnClick="cmdSaveLicense_Click"
            Text="Save license" Width="120px" />
        &nbsp;&nbsp;
                                            <asp:LinkButton ID="lbCreatePackage" runat="server"
                                                OnClick="lbCreatePackage_Click">Create / renew package</asp:LinkButton>
        &nbsp;&nbsp;
                <asp:Label ID="lblLicenseMessage" runat="server" Font-Bold="False"
                    Text="Required fields *" Visible="False"></asp:Label>
        &nbsp;&nbsp;
            <asp:Button ID="Button1" runat="server" BackColor="White"
                BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px"
                OnClick="cmdPlaceDate_Click" Width="1px" />
        &nbsp;&nbsp;
        <asp:Button ID="lblPushBackForTesting" runat="server" Tooltip="'Valid from' and 'Date created' for all activated packages are pushed back by their length in months" Text ="*Push Back for testing*" OnClick="lblPushBackForTesting_Click"/>
        &nbsp;&nbsp;
        <asp:Button ID="lblPullForwardForTesting" runat="server" Tooltip="'Valid from' and 'Date created' for all activated packages are pulled forward by their length in months" Text ="*Pull Forward for testing*" OnClick="lblPullForwardForTesting_Click"/>
        <br />
        <asp:Panel ID="pnlChangeLicenseModel" runat="server" Visible="false">
            <div style="padding:10px">
            <table id="tblChange" border="1" cellpadding="1" cellspacing="1" width="100%">
            <tr>
                <td colspan="2" style="padding: 10px;background-color: #FFCCCC;">
                    <asp:Label ID="lblContinueMessage" runat="server" ></asp:Label>&nbsp;&nbsp;
                    (<asp:LinkButton ID="lbCancel" runat="server" OnClick="lbCancel_Click">cancel</asp:LinkButton>)
                </td>
            </tr>
            <tr>
                <td style="padding: 10px;background-color: #FFCCCC; width: 50%; vertical-align: top;">
                    <asp:Label ID="lblContinueMessage01" runat="server" ></asp:Label>
                    <br />
                    <asp:Button ID="lbChangeLatestPackage" runat="server" Text="Convert" OnClick="cmdChangeLatestPackage_Click"/>
                </td>
                <td style="padding: 10px;width: 50%; background-color: #FFCCCC; vertical-align: top;">
                    <asp:Label ID="lblContinueMessage02" runat="server" ></asp:Label>
                    <br />
                    <asp:Button ID="lblRenew" runat="server" Text="Renew" OnClick="lblRenew_Click"/>&nbsp;&nbsp; 
                    <asp:Label ID="lblSelectOfferMsg" runat="server" Text="You need to select an offer!" Visible="False" ForeColor="Blue" Font-Bold="True"></asp:Label>
                </td>
            </tr>
                <tr>
                    <td>

                    </td>
                    <td>

                    </td>
                </tr>
                </table>
            

                </div>
        </asp:Panel>
    </asp:Panel>


    <asp:Panel ID="pnlLicenseDetails" runat="server"
        Visible="False">
        <br />
        <asp:Label ID="lblPackageTitle" runat="server" Font-Bold="True"
            Text="Selected recent package"></asp:Label>
        &nbsp;&nbsp;&nbsp;
                                        <br />
        <table id="Table2" border="1" cellpadding="1" cellspacing="1" width="100%">
            <tr>
                <td style="background-color: #B6C6D6; width: 25%; height: 27px;">Number of accounts *</td>
                <td style="width: 25%; background-color: #FFFFCC; height: 27px;">
                    <asp:TextBox ID="tbNumberAccounts" runat="server"></asp:TextBox>
                </td>
                <td style="background-color: #B6C6D6; width: 25%; height: 27px;">Site license details ID</td>
                <td style="background-color: #FFFFCC; height: 27px;">&nbsp;
                                                    <asp:Label ID="lblSiteLicenseDetailsID" runat="server" Text="N/A"></asp:Label>
                    &nbsp;</td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Number of reviews *</td>
                <td style="width: 25%; background-color: #FFFFCC">
                    <asp:TextBox ID="tbNumberReviews" runat="server"></asp:TextBox>
                </td>
                <td style="background-color: #B6C6D6; width: 25%;">For sale ID</td>
                <td style="width: 25%; background-color: #FFFFCC">&nbsp;&nbsp;<asp:Label ID="lblForSaleID" runat="server" Text="N/A"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Total fee *</td>
                <td style="width: 25%; background-color: #FFFFCC">
                    <asp:TextBox ID="tbTotalFee" runat="server"></asp:TextBox>
                </td>
                <td style="background-color: #B6C6D6; width: 25%;">Price per month&nbsp;
                </td>
                <td style="width: 25%; background-color: #FFFFCC">&nbsp;
                                                    <asp:Label ID="lblPricePerMonth" runat="server" Text="N/A"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Number months *</td>
                <td style="width: 25%; background-color: #FFFFCC">
                    <asp:TextBox ID="tbNumberMonths" runat="server"></asp:TextBox>
                </td>
                <td style="background-color: #B6C6D6; width: 25%;">Package creation *</td>
                <td style="width: 25%; background-color: #FFFFCC">
                    <asp:TextBox ID="tbDatePackageCreated" runat="server"></asp:TextBox>
                    &nbsp;
                                                    <asp:ImageButton ID="IBCalendar4" runat="server"
                                                        ImageUrl="~/images/calbtn.gif" />
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Valid from (enter for Activation)</td>
                <td style="width: 25%; background-color: #99FF99">
                    <asp:TextBox ID="tbValidFrom" runat="server" Enabled="False"></asp:TextBox>
                    &nbsp;
                                                    <asp:ImageButton ID="IBCalendar2" runat="server" Enabled="False"
                                                        ImageUrl="~/images/calbtn.gif" />
                </td>
                <td style="background-color: #B6C6D6; width: 25%;">Expiry date (enter for Activation)</td>
                <td style="width: 25%; background-color: #99FF99">
                    <asp:TextBox ID="tbExpiryDate" runat="server" Enabled="False"></asp:TextBox>
                    &nbsp;
                                                    <asp:ImageButton ID="IBCalendar3" runat="server" Enabled="False"
                                                        ImageUrl="~/images/calbtn.gif" />
                    &nbsp;</td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Activated</td>
                <td style="width: 25%; background-color: #FFFFCC">&nbsp;&nbsp;
                                                    <asp:Label ID="lblIsActivated" runat="server" Text="No"></asp:Label>
                </td>
                <td style="background-color: #B6C6D6; width: 25%;">Reason for package change</td>
                <td style="width: 25%; background-color: #FFFFCC">
                    <asp:DropDownList ID="ddlExtensionType" runat="server"
                        DataTextField="EXTENSION_TYPE" DataValueField="EXTENSION_TYPE_ID"
                        Enabled="False">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Description of extension</td>
                <td colspan="3" style="background-color: #FFFFCC">
                    <asp:TextBox ID="tbDescription" runat="server" Width="90%" Enabled="false"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Extension history&nbsp;&nbsp;
                                                    <asp:LinkButton ID="lbExtensionHistory" runat="server" Enabled="False"
                                                        OnClick="lbExtensionHistory_Click">View</asp:LinkButton>
                </td>
                <td colspan="3" style="background-color: #FFFFCC">
                    <asp:GridView ID="gvExtensionHistory" runat="server"
                        AutoGenerateColumns="False" EnableModelValidation="True" Visible="False">
                        <Columns>
                            <asp:BoundField DataField="ID_EXTENDED" HeaderText="Package Extended">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="DATE_OF_EDIT" HeaderText="Date extended">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="OLD_EXPIRY_DATE" HeaderText="Old date">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="NEW_EXPIRY_DATE" HeaderText="New date">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="CONTACT_NAME" HeaderText="Extended by">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="EXTENSION_TYPE" HeaderText="Reason">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="EXTENSION_NOTES" HeaderText="Notes">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                        </Columns>
                    </asp:GridView>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 25%;">Package history&nbsp;&nbsp;
                                                    <asp:LinkButton ID="lbLicenseHistory" runat="server" Enabled="False"
                                                        OnClick="lbLicenseHistory_Click">View</asp:LinkButton>
                </td>
                <td colspan="3" style="background-color: #FFFFCC">
                    <asp:GridView ID="gvLicenseHistory" runat="server" AutoGenerateColumns="False"
                        EnableModelValidation="True" Visible="False">
                        <Columns>
                            <asp:BoundField DataField="SITE_LIC_DETAILS_ID" HeaderText="Package ID">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="CONTACT_ID" HeaderText="Changed by">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="AFFECTED_ID" HeaderText="Affected ID">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="CHANGE_TYPE" HeaderText="Change">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="REASON" HeaderText="Reason">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="DATE" HeaderText="Date">
                                <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px"
                                    HorizontalAlign="Left" />
                                <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:BoundField>
                        </Columns>
                    </asp:GridView>
                </td>
            </tr>
        </table>
        <asp:Button ID="cmdSaveLicenseDetails" runat="server"
            OnClick="cmdSaveLicenseDetails_Click" Text="Save license details"
            Width="140px" />
        &nbsp;&nbsp;
                                        <asp:Label ID="lblLicenseDetailsMessage" runat="server" Font-Bold="False"
                                            Text="Required fields *" Visible="False"></asp:Label>
        <br />
        <br />
    </asp:Panel>






    <asp:Panel ID="pnlAccountsAndReviews" runat="server" Visible="False">
        <table style="width: 100%;">
            <tr>
                <td style="background-color: #ffffff; width: 50%;">
                    <b>Accounts in latest package</b></td>
                <td style="background-color: #ffffff; width: 5%;">&nbsp;</td>
                <td style="background-color: #ffffff; width: 40%;">
                    <b>Reviews in latest package</b> <asp:Label ID="lblTooManyReviews" runat="server" Text="Too many reviews!" Visible="False" ForeColor="Red" Font-Bold="True"></asp:Label></td>
            </tr>
            <tr>
                <td style="background-color: #ffffff; width: 50%;" valign="top">
                    <asp:GridView ID="gvAccounts" runat="server"
                        AutoGenerateColumns="False"
                        DataKeyNames="EMAIL" EnableModelValidation="True"
                        OnRowCommand="gvAccounts_RowCommand" Width="100%"
                        OnRowDataBound="gvAccounts_RowDataBound">
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
                            <asp:BoundField DataField="LAST_ACCESS" HeaderText="Last access">
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:BoundField>
                            <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:BoundField>
                            <asp:ButtonField CommandName="REMOVE" HeaderText="Remove" Text="Remove">
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:ButtonField>
                        </Columns>
                    </asp:GridView>
                </td>
                <td style="background-color: #ffffff; width: 5%;" valign="top">&nbsp;</td>
                <td style="background-color: #ffffff; width: 40%;" valign="top">
                    <asp:GridView ID="gvReviews" runat="server"
                        AutoGenerateColumns="False"
                        DataKeyNames="REVIEW_ID" EnableModelValidation="True" Width="100%"
                        OnRowCommand="gvReviews_RowCommand" OnRowDataBound="gvReviews_RowDataBound">
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
                            <asp:ButtonField CommandName="MOVE" HeaderText="Archive" Text="Move">
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:ButtonField>
                        </Columns>
                    </asp:GridView>
                    (review removal restricted to ER4 adminstrators)</td>
            </tr>
            <tr>
                <td style="background-color: #ffffff; width: 50%; height: 16px;">Add an account</td>
                <td style="background-color: #ffffff; width: 5%; height: 16px;"></td>
                <td style="background-color: #ffffff; width: 40%; height: 16px;">Add a review&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </td>
            </tr>
            <tr>
                <td style="background-color: #ffffff; width: 50%; border-bottom: solid 1px black;">
                    <asp:TextBox ID="tbEmail" runat="server" Width="60%" Enabled="False">Enter email address</asp:TextBox>
                    &nbsp;&nbsp;
                            <asp:Button ID="cmdAddAccount" runat="server" Text="Add account"
                                OnClick="cmdAddAccount_Click" Style="width: 109px" Enabled="False" />
                    <br />
                    <asp:Label ID="lblAccountMsg" runat="server" Text="Label" Visible="False"></asp:Label>
                </td>
                <td style="background-color: #ffffff; width: 5%;">&nbsp;</td>
                <td style="background-color: #ffffff; width: 40%; border-bottom: solid 1px black;">
                    <asp:TextBox ID="tbReviewID" runat="server" Enabled="False">Enter Review ID</asp:TextBox>
                    &nbsp;&nbsp;
                            <asp:Button ID="cmdAddReview" runat="server" Text="Add review"
                                OnClick="cmdAddReview_Click" Enabled="False" />
                    <br />
                    <asp:Label ID="lblReviewMsg" runat="server" Text="Label" Visible="False"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="background-color: #ffffff; width: 50%; height: 18px;"></td>
                <td style="background-color: #ffffff; width: 5%; height: 18px;"></td>
                <td style="background-color: #ffffff; width: 40%; height: 18px;"></td>
            </tr>
            <tr>
                <td style="background-color: #ffffff; width: 50%;">
                    <b>Site license administrators</b></td>
                <td style="background-color: #ffffff; width: 5%;">&nbsp;</td>
                <td style="background-color: #ffffff; width: 40%;">
                    <asp:Label ID="lblPreviousReviews" runat="server" Text="Reviews in previous packages" Font-Bold="True" Visible="true"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="background-color: #ffffff; width: 50%;" valign="top">
                    <asp:GridView ID="gvLicenseAdms" runat="server" AutoGenerateColumns="False"
                        DataKeyNames="SITE_LIC_ADMIN_ID" EnableModelValidation="True"
                        OnRowCommand="gvLicenseAdms_RowCommand" OnRowDataBound="gvLicenseAdms_RowDataBound"
                        Width="100%">
                        <Columns>
                            <asp:BoundField DataField="SITE_LIC_ADMIN_ID" HeaderText="Admin ID" Visible="false" />
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
                <td style="background-color: #ffffff; width: 5%;">&nbsp;</td>
                <td style="background-color: #ffffff; width: 40%;" valign="top">
                    <asp:GridView ID="gvReviewsPastLicense" runat="server" AutoGenerateColumns="False"
                        DataKeyNames="REVIEW_ID" EnableModelValidation="True" Width="100%"
                        OnRowCommand="gvReviewsPastLicense_RowCommand" OnRowDataBound="gvReviewsPastLicense_RowDataBound">
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
                    <br />
                    <asp:Label ID="lblOldReviewMsg" runat="server" Text="" Visible="False"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="background-color: #ffffff; width: 50%; height: 17px;">Add an admin</td>
                <td style="background-color: #ffffff; width: 5%; height: 17px;"></td>
                <td style="background-color: #ffffff; width: 40%; height: 17px;"></td>
            </tr>
            <tr>
                <td style="background-color: #ffffff; width: 50%;">
                    <asp:TextBox ID="tbEmailAdm" runat="server" Width="60%">Enter email address</asp:TextBox>
                    &nbsp;&nbsp;
                            <asp:Button ID="cmdAddAdm" runat="server" OnClick="cmdAddAdm_Click"
                                Style="width: 109px" Text="Add admin" />
                </td>
                <td style="background-color: #ffffff; width: 5%;">&nbsp;</td>
                <td style="background-color: #ffffff; width: 40%;">&nbsp;</td>
            </tr>
            <tr>
                <td style="background-color: #ffffff; width: 50%;">
                    <asp:Label ID="lblAccountAdmMessage" runat="server" Text="Label"
                        Visible="False"></asp:Label>
                </td>
                <td style="background-color: #ffffff; width: 5%;">&nbsp;</td>
                <td style="background-color: #ffffff; width: 40%;">&nbsp;</td>
            </tr>
        </table>
        <br />
    </asp:Panel>
    <br />
    <br />

    <br />
    <br />
    <br />



</asp:Content>
