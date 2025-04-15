<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="SiteLicense.aspx.cs" Inherits="SiteLicense" Title="Site license" %>






    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

        <script language="javascript" type="text/javascript">
        function openContactList(ID) 
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

            </script>

        <script src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
        <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js" integrity="sha384-ApNbgh9B+Y1QKtv3Rn7W3mgPxhU9K/ScQsAP7hUibX39j7fakFPskvXusvfa0b4Q" crossorigin="anonymous"></script>
        <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" integrity="sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl" crossorigin="anonymous"></script>




        <div>

        <asp:Panel ID="pnlMessage" runat="server" Visible="False">
            <asp:Label ID="lblNotASiteLicenseAdm" runat="server" Font-Bold="True" 
                ForeColor="Red" 
                
                Text="You are not a site license administrator so there is nothing to show. &lt;br&gt;Please go to the Setup page."></asp:Label>
        <br />
                                    </asp:Panel>

            <asp:Panel ID="pnlSiteLicenseExists" runat="server" Visible="False">

            <asp:Panel ID="pnlMultipleSiteLicense" runat="server" Visible="False">
                <asp:DropDownList ID="ddlYourSiteLicenses" runat="server" 
                    DataTextField="SITE_LIC_NAME" DataValueField="SITE_LIC_ID" 
                    onselectedindexchanged="ddlYourSiteLicenses_SelectedIndexChanged1" 
                    AutoPostBack="True">
                </asp:DropDownList>
                &nbsp;&nbsp; You are the administrator of multiple licenses<br /> <br />
            </asp:Panel>
                
                <asp:Panel ID="pnlSiteLicense" runat="server">
                    <b>Site license details</b>
                    &nbsp;&nbsp;
                    <button type="button" class="btn btn-outline-secondary btn-sm" data-toggle="collapse" data-target="#demo">Help</button>
                    <br />

                    <div class="row">
                        <div class="col-md-12">
                            <div class="collapse" id="demo">
                                <div class="card card-body bg-light">
                                    <asp:Label ID="lblLicenseDetailsHelp" runat="server" Text="" ></asp:Label>
                                </div>
                            </div>
                        </div>
                    </div>

                    <table ID="Table3" border="1" cellpadding="1" cellspacing="1" width="100%">
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                License name</td>
                            <td colspan="3" style="width: 75%; background-color: #E2E9EF">
                                &nbsp;<asp:Label ID="lblLicenseName" runat="server" Text="N/A"></asp:Label>
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Organisation *</td>
                            <td colspan="3" style="width: 75%; background-color: #E2E9EF">
                                <asp:TextBox ID="tbOrganisation" runat="server" Width="80%">N/A</asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Address *</td>
                            <td colspan="3" style="width: 75%; background-color: #E2E9EF">
                                <asp:TextBox ID="tbAddress" runat="server" Width="80%">N/A</asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Telephone *</td>
                            <td colspan="3" style="width: 75%; background-color: #E2E9EF">
                                <asp:TextBox ID="tbTelephone" runat="server" Width="80%">N/A</asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Notes</td>
                            <td colspan="3" style="width: 75%; background-color: #E2E9EF">
                                <asp:TextBox ID="tbNotes" runat="server" Width="80%">N/A</asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                <asp:Label ID="lblSiteLicenceID" runat="server" Text=""></asp:Label>
                            </td>
                            <td style="width: 25%; background-color: #E2E9EF">
                                &nbsp;<asp:Label ID="lblSiteLicID" runat="server" Text="N/A"></asp:Label>
                                &nbsp;</td>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                <asp:Label ID="lblLicenseModel" runat="server" Text="Licence review model"></asp:Label>
                            </td>
                            <td style="width: 25%; background-color: #E2E9EF">
                                &nbsp;<asp:Label ID="lblLicModel" runat="server" Text="Fixed"></asp:Label>
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Date created</td>
                            <td colspan="3" style="width: 75%; background-color: #E2E9EF">
                                &nbsp;<asp:Label ID="lblDateCreated" runat="server" Text="N/A"></asp:Label>
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Packages (with date created)</td>
                            <td colspan="3" style="width: 75%; background-color: #99FF99">
                                <asp:DropDownList ID="ddlPackages" runat="server" AutoPostBack="True" 
                                    DataTextField="DATE_CREATED" DataValueField="SITE_LIC_DETAILS_ID" 
                                    onselectedindexchanged="ddlPackages_SelectedIndexChanged" Width="50%">
                                </asp:DropDownList>
                                </td>
                        </tr>
                        <!--<tr>
                            <td colspan="4" style="background-color: #B6C6D6; ">
                                British library codes (optional)&nbsp;&nbsp;
                                <asp:LinkButton ID="lbShowBLCodes" runat="server" onclick="lbShowBLCodes_Click">Show/Edit</asp:LinkButton>
                                <asp:Panel ID="pnlBriLibCodes" runat="server" Visible="False">
                                    <table style="width:100%;">
                                        <tr>
                                            <td style="width: 33%" valign="middle">
                                                A Review added to the license will get these codes unless it already has codes.<br />
                                                <br />
                                                Editing the license codes will not change the codes for reviews already in the 
                                                license. The codes for those reviews can be changed in the <b>Summary</b> page.</td>
                                            <td style="width: 33%" valign="middle">
                                                Library privilege copies&nbsp;&nbsp; &nbsp;
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
                                            <td style="width: 33%" valign="middle">
                                                Copyright cleared copies&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;
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
                                    </table>
                                </asp:Panel>
                            </td>
                        </tr>-->
                        <tr>
                            <td colspan="4">
                                <asp:Panel runat="server" ID="pnlGPTcredit" visible="false">
                                    <table width="100%">
                                        <tr>
                                            <td style="background-color: #B6C6D6; width: 25%;">
                                                <b>ChapGPT credit</b></td>                                
                                                <td style="width: 25%; background-color: #E2E9EF; height: 27px;vertical-align:top">
                                                    <b>OpenAI GPT4</b><br />
                                                    Select a PurchaseID to give this Site License OpenAI GPT4 access<br />
                                                    <asp:DropDownList ID="ddlCreditPurchases" runat="server" 
                                                        DataTextField="CREDIT_ID_REMAINING" DataValueField="CREDIT_PURCHASE_ID" 
                                                        Enabled="True" AutoPostBack="True" OnSelectedIndexChanged="ddlCreditPurchases_SelectedIndexChanged">
                                                    </asp:DropDownList><br />
                                                    <!--<asp:TextBox ID="tbCreditPurchaseID" runat="server" Visible="true" Width="100px"></asp:TextBox>
                                                    &nbsp;
                                                    <asp:LinkButton ID="lbSavePurchaseCreditID" runat="server" onclick="lbSavePurchaseCreditID_Click" 
                                                            ToolTip="Add a purchase credit ID">Add</asp:LinkButton>
                                                    <b><asp:Label ID="lblInvalidID" runat="server" Text="Invalid ID" Visible="false"></asp:Label></b>-->                               
                                                </td>
                                                <td style="background-color: #E2E9EF; height: 27px;vertical-align:top" colspan="2">
                                                        OpenAI GPT4 Credit<br />
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
                                    </table>
                                </asp:Panel>

                            </td>
                        </tr>
                    </table>
                    <asp:Button ID="cmdSaveLicense" runat="server" onclick="cmdSaveLicense_Click" 
                        Text="Save" />
                    &nbsp;&nbsp;
                    <asp:Label ID="lblLicenseMessage" runat="server" Font-Bold="False" 
                        Text="Required fields *" Visible="False"></asp:Label>
                    &nbsp;
                    <asp:Button ID="cmdPlaceFunder" runat="server" BackColor="White" 
                        BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px" 
                        OnClick="cmdPlaceFunder_Click" style="font-weight: bold" Width="1px" /><br />
                </asp:Panel>

            <asp:Panel ID="pnlPackages" runat="server" Visible="False">


                <br />
                <asp:Label ID="lblDetailsHeading" runat="server" Font-Bold="True" 
                    Text="Packages"></asp:Label>
                <br />
                <table ID="Table2" border="1" cellpadding="1" cellspacing="1" width="100%">
                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%; ">
                            Number of accounts</td>
                        <td ;="" style="width: 25%; background-color: #E2E9EF; ">
                            &nbsp;<asp:Label ID="lblNumberAccounts" runat="server" Text="N/A"></asp:Label>
                        </td>
                        <td style="background-color: #B6C6D6; width: 25%; ">
                            Number of reviews</td>
                        <td ;="" style="width: 25%; background-color: #E2E9EF; ">
                            &nbsp;<asp:Label ID="lblNumberReviews" runat="server" Text="N/A"></asp:Label>
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%;">
                            Number months</td>
                        <td ;="" style="width: 25%; background-color: #E2E9EF">
                            &nbsp;<asp:Label ID="lblNumberMonths" runat="server" Text="N/A"></asp:Label>
                            &nbsp;</td>
                        <td style="background-color: #B6C6D6; width: 25%;">
                            Total fee</td>
                        <td ;="" style="width: 25%; background-color: #E2E9EF">
                            &nbsp;<asp:Label ID="lblTotalFee" runat="server" Text="N/A"></asp:Label>
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%;">
                            Activated</td>
                        <td ;="" style="width: 25%; background-color: #E2E9EF">
                            &nbsp;<asp:Label ID="lblIsActivated" runat="server" Text="No"></asp:Label>
                            &nbsp;</td>
                        <td style="background-color: #B6C6D6; width: 25%;">
                            Package ID</td>
                        <td ;="" style="width: 25%; background-color: #E2E9EF">
                            &nbsp;<asp:Label ID="lblSiteLicenseDetailsID" runat="server" Text="N/A"></asp:Label>
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%;">
                            Valid from&nbsp; (dd/mm/yyyy)</td>
                        <td ;="" style="width: 25%; background-color: #E2E9EF">
                            &nbsp;<asp:Label ID="lblValidFrom" runat="server" Text="N/A"></asp:Label>
                            &nbsp;</td>
                        <td style="background-color: #B6C6D6; width: 25%;">
                            Expiry date&nbsp; (dd/mm/yyyy)</td>
                        <td ;="" style="width: 25%; background-color: #E2E9EF">
                            &nbsp;<asp:Label ID="lblExpiryDate" runat="server" Text="N/A"></asp:Label>
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%;">
                            Package history&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                            <asp:LinkButton ID="lbPackageHistory" runat="server" Enabled="False" 
                                onclick="lbPackageHistory_Click">View</asp:LinkButton>
                        </td>
                        <td ;="" colspan="3" style="background-color: #E2E9EF">
                            <asp:GridView ID="gvLicenseHistory" runat="server" AutoGenerateColumns="False" 
                                EnableModelValidation="True" Visible="False">
                                <Columns>
                                    <asp:BoundField DataField="SITE_LIC_DETAILS_ID" HeaderText="Package ID">
                                    <HeaderStyle BackColor="#B6C6D6" BorderStyle="Solid" BorderWidth="1px" 
                                        HorizontalAlign="Left" />
                                    <ItemStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="CONTACT_ID" HeaderText="Changed by ID">
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
                    <tr>
                        <td style="background-color: #B6C6D6; width: 25%;">
                            Expiry extensions&nbsp;&nbsp;&nbsp;&nbsp;
                            <asp:LinkButton ID="lbExtensionHistory" runat="server" Enabled="False" 
                                onclick="lbExtensionHistory_Click">View</asp:LinkButton>
                        </td>
                        <td ;="" colspan="3" style="background-color: #E2E9EF">
                            <asp:GridView ID="gvExtensionHistory" runat="server" 
                                AutoGenerateColumns="False" EnableModelValidation="True" Visible="False">
                                <Columns>
                                    <asp:BoundField DataField="ID_EXTENDED" HeaderText="Package extended">
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
                </table>


            <br />
             </asp:Panel>

            <asp:Panel ID="pnlAccountsAndReviews" runat="server">
                <br />
                <table style="width:100%;">
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;">
                            <b>Accounts in latest license</b></td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;">
                            <b>Reviews in latest license</b></td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;" valign="top">
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
                        <td style="background-color: #ffffff; width: 10%;" valign="top">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;" valign="top">
                            <asp:GridView ID="gvReviews" runat="server" 
                                AutoGenerateColumns="False" 
                                DataKeyNames="REVIEW_ID" EnableModelValidation="True" Width="100%" OnRowDataBound="gvReviews_RowDataBound" OnRowCommand="gvReviews_RowCommand">
                                <Columns>
                                    <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:TemplateField HeaderText="Owner" Visible="False">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("CONTACT_NAME") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lbReviewOwner" runat="server">Review owner</asp:LinkButton>
                                        </ItemTemplate>
                                        <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:TemplateField>
                                    <asp:ButtonField CommandName="REMOVE" HeaderText="Remove" Text="Remove" Visible="False">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:ButtonField>
                                </Columns>
                            </asp:GridView>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%; height: 16px;">
                            Add an account</td>
                        <td style="background-color: #ffffff; width: 10%; height: 16px;">
                            </td>
                        <td style="background-color: #ffffff; width: 45%; height: 16px;">
                            Add a review</td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%; border-bottom:solid 1px black; " >
                            <asp:TextBox ID="tbEmail" runat="server" Width="60%">Enter email address</asp:TextBox>
                            &nbsp;&nbsp;
                            <asp:Button ID="cmdAddAccount" runat="server" Text="Add account" 
                                onclick="cmdAddAccount_Click" style="width: 109px" />
                            <br />
                            <asp:Label ID="lblAccountMsg" runat="server" Text="Label" Visible="False"></asp:Label>
                            <br />
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
                        <td style="background-color: #ffffff; width: 45%;">
                            <b>License admins</b></td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;">
                            <b>Reviews in previous licenses</b></td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;" valign="top">
                            <asp:GridView ID="gvLicenseAdms" runat="server" AutoGenerateColumns="False" 
                                DataKeyNames="SITE_LIC_ADMIN_ID" EnableModelValidation="True" 
                                onrowcommand="gvLicenseAdms_RowCommand" onrowdatabound="gvLicenseAdms_RowDataBound" 
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
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;" valign="top">
                            <asp:GridView ID="gvReviewsPastLicense" runat="server" AutoGenerateColumns="False" 
                                DataKeyNames="REVIEW_ID" EnableModelValidation="True" Width="100%" OnRowDataBound="gvReviewsPastLicense_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name">
                                    <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:BoundField>
                                    <asp:TemplateField HeaderText="Owner" Visible="False">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("CONTACT_NAME") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lbReviewOwnerPast" runat="server">Review owner</asp:LinkButton>
                                        </ItemTemplate>
                                        <HeaderStyle BackColor="#B6C6D6" />
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </td>
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
                        <td style="background-color: #ffffff; width: 45%; height: 17px;">
                            <asp:Label ID="lblAccountAdmMessage" runat="server" Text="Label" 
                                Visible="False"></asp:Label>
                        </td>
                        <td style="background-color: #ffffff; width: 10%; height: 17px;">
                            </td>
                        <td style="background-color: #ffffff; width: 45%; height: 17px;">
                            </td>
                    </tr>
                </table>
                <br />
            </asp:Panel>
            <br />
                                    </asp:Panel>
            <br />
            </div>



</asp:Content>


