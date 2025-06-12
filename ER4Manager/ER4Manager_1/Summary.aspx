<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Summary.aspx.cs" Inherits="Summary" Title="Summary" %>
    <%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="Telerik" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <telerik:RadWindowManager ID="RadWindowManager1" runat="server" EnableShadow="true">
        </telerik:RadWindowManager>




        <b>Your account summary&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </b>
        Please note that all dates are dd/mm/yyyy
        <asp:GridView
            ID="gvReviewer" runat="server"
            AutoGenerateColumns="False" Width="800px" OnRowCommand="gvReviewer_RowCommand" CssClass="grviewFixedWidth"
            DataKeyNames="CONTACT_ID" OnRowEditing="gvReviewer_RowEditing"
            EnableModelValidation="True">
            <Columns>
                <asp:BoundField DataField="CONTACT_ID"
                    HeaderText="ContactID">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField HeaderText="Name" DataField="CONTACT_NAME">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField HeaderText="Email address"
                    DataField="EMAIL">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField DataField="LAST_LOGIN"
                    HeaderText="Last login">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField DataField="HOURS" HeaderText="Logged in (hrs)">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField HeaderText="Account created"
                    DataField="DATE_CREATED">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField DataField="EXPIRY_DATE"
                    HeaderText="Expiry date">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:ButtonField CommandName="EDT" HeaderText="Edit" Text="Edit">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:ButtonField>
            </Columns>
        </asp:GridView>
        <br />

        <asp:Panel ID="pnlContactDetails" runat="server" Visible="False"
            BackColor="#E2E9EF" BorderStyle="Solid" BorderWidth="1px">
            ContactID:
                   
        <asp:Label ID="lblContactID" runat="server" Text="N/A"></asp:Label>

            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                   
        <asp:CheckBox ID="cbSendNewsletter" runat="server" AutoPostBack="True"
            OnCheckedChanged="cbSendNewsletter_CheckedChanged" Text="Receive newsletter" />
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                      
        <table id="Table3" border="1" cellpadding="1" cellspacing="1" width="600">
            <tr>
                <td style="background-color: #B6C6D6; width: 140px; height: 27px;">Name</td>
                <td style="width: 170px; height: 27px;">
                    <asp:TextBox ID="tbName" runat="server" CssClass="textbox" MaxLength="100"
                        Width="90%"></asp:TextBox>
                </td>
                <td style="background-color: #B6C6D6; width: 130px; height: 27px;">Username</td>
                <td style="width: 170px; height: 27px;">
                    <asp:TextBox ID="tbUserName" runat="server" CssClass="textbox" MaxLength="50"
                        Width="90%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 130px">Email</td>
                <td style="width: 170px">
                    <asp:TextBox ID="tbEmail" runat="server" CssClass="textbox" Width="90%"></asp:TextBox>
                </td>
                <td style="background-color: #B6C6D6; width: 160px">Confirm email</td>
                <td style="width: 170px">
                    <asp:TextBox ID="tbEmailConfirm" runat="server" CssClass="textbox" Width="90%"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td style="background-color: #B6C6D6; width: 130px">New password</td>
                <td style="width: 170px">
                    <asp:TextBox ID="tbPassword" runat="server" CssClass="textbox" MaxLength="50"
                        Width="90%"></asp:TextBox>
                </td>
                <td style="background-color: #B6C6D6; width: 160px">Confirm new password</td>
                <td style="width: 170px">
                    <asp:TextBox ID="tbPasswordConfirm" runat="server" CssClass="textbox"
                        MaxLength="50" Width="90%"></asp:TextBox>
                </td>
            </tr>
        </table>
            <asp:Button ID="cmdSave" runat="server" CssClass="button"
                OnClick="cmdSave_Click" Text="Save" />
            &nbsp;&nbsp;
                   
        <asp:LinkButton ID="lbCancelAccountEdit" runat="server"
            OnClick="lbCancelAccountEdit_Click">Cancel</asp:LinkButton>
            &nbsp;&nbsp;&nbsp;
                   
        <asp:Label ID="lblPasswordMsg" runat="server"
            Text="To keep your existing password leave the password fields blank."></asp:Label>
            <br />
            <asp:Panel ID="pnlAccountMessages" runat="server" Visible="False"
                Style="margin-top: 0px">
                <asp:Label ID="lblMissingFields" runat="server" Font-Bold="True"
                    Text="Please fill in all of the fields. Apostrophes (') are not allowed in User names, Passwords and Emails."
                    Visible="False" ForeColor="Red"></asp:Label>
                <br />
                <asp:Label ID="lblUsername" runat="server" Font-Bold="True"
                    Text="User name is already in use. Please select another." Visible="False"
                    ForeColor="Red"></asp:Label>
                <br />
                <asp:Label ID="lblEmailAddress0" runat="server" Font-Bold="True"
                    Text="Email address is already in use. Please select another."
                    Visible="False" ForeColor="Red"></asp:Label>
            </asp:Panel>
            <asp:Label ID="lblNewPassword" runat="server" Font-Bold="True"
                Text="Passwords must be at least 8 characters and contain at least one lower case letter, &lt;br/&gt;one upper case letter, one digit and no spaces."
                Visible="False" ForeColor="Red"></asp:Label>
            <br />
        </asp:Panel>

        <asp:Panel ID="pnlOutstandingFees" runat="server" Visible="False">
            <b>Outstanding fees</b>&nbsp;&nbsp;Pay outstanding fees in the online shop
            <asp:GridView ID="gvOutstandingFees" runat="server" CssClass="grviewFixedWidth"
                AutoGenerateColumns="False" DataKeyNames="OUTSTANDING_FEE_ID"
                OnRowCommand="gvOutstandingFees_RowCommand" OnRowEditing="gvOutstandingFees_RowEditing"
                Width="800px" EnableModelValidation="True" OnRowDataBound="gvOutstandingFees_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="OUTSTANDING_FEE_ID" HeaderText="Fee ID">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="OUTSTANDING_FEE" HeaderText="Fee (£)">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="DATE_CREATED" HeaderText="Date created">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                </Columns>
            </asp:GridView>
            <br />
        </asp:Panel>



        <asp:Panel ID="pnlCreditPurchases" runat="server" Visible="True">
            <b>Your credit purchases</b>
            <asp:Label ID="lblCreditTransferInstructions1" runat="server" Text="" Visible="False" Font-Bold="true"></asp:Label>
                <asp:LinkButton ID="lbMoveCredit" runat="server" OnClick="lbMoveCredit_Click" Visible="false">transfer</asp:LinkButton>
            <asp:Label ID="lblCreditTransferInstructions2" runat="server" Text="" Visible="False"  Font-Bold="true"></asp:Label>
            
            <asp:Panel ID="pnlMoveCredit" runat="server" BackColor="#E2E9EF" BorderStyle="Solid" BorderWidth="1px" Visible="False">               
                Enter the amount to transfer, select the source and destination purchase ID and click Transfer.<br />
                Transfer (£)&nbsp;<asp:TextBox ID="tbAmountToTransfer" runat="server" CssClass="textbox" Width="40px"></asp:TextBox>
                from <asp:DropDownList ID="ddlSourcePurchaseID" runat="server" AutoPostBack="False"
                    DataTextField="PURCHASE_ID_SOURCE" DataValueField="PURCHASE_ID_SOURCE_REMAINING" Width="100px">
                </asp:DropDownList>
                to <asp:DropDownList ID="ddlDestinationPurchaseID" runat="server" AutoPostBack="False"
                    DataTextField="PURCHASE_ID_DESTINATION" DataValueField="PURCHASE_ID_DESTINATION_REMAINING" Width="110px">
                </asp:DropDownList>&nbsp;&nbsp;&nbsp;
                <asp:Button ID="cmdTransferCreditPurchase" runat="server" CssClass="button" OnClick="cmdTransferCreditPurchase_Click" Text="Transfer" />
                &nbsp;&nbsp;
                <asp:LinkButton ID="lbCancelTransfer" runat="server" OnClick="lbCancelTransfer_Click">Cancel</asp:LinkButton>
                &nbsp;&nbsp;
                <asp:Label ID="lblTransferCreditPurchaseResult" runat="server" style="padding: 2px; margin:1px; display:inline-block;" Text="There was an error" Font-Bold="True" Visible="false" BackColor="#FFCC99" BorderColor="Red"></asp:Label>
                <br /><br />
            </asp:Panel>

            <asp:GridView ID="gvCreditPurchases" runat="server" CssClass="grviewFixedWidth"
                AutoGenerateColumns="False" DataKeyNames="CREDIT_PURCHASE_ID"
                OnRowCommand="gvCreditPurchases_RowCommand" OnRowEditing="gvCreditPurchases_RowEditing"
                Width="800px" EnableModelValidation="True" OnRowDataBound="gvCreditPurchases_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="CREDIT_PURCHASE_ID" HeaderText="PurchaseID">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="DATE_PURCHASED" HeaderText="Date purchased">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="CREDIT_PURCHASED" HeaderText="Amount (£)">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="CREDIT_REMAINING" HeaderText="Remaining (£)">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:ButtonField CommandName="HISTORY" HeaderText="History" Text="history">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:ButtonField>
                    <asp:ButtonField CommandName="DETAILS" HeaderText="Assign" Text="assign">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:ButtonField>
                </Columns>
            </asp:GridView>
            <asp:Label ID="lblNoCreditPurchases" runat="server"
            Text="You do not have any credit purchases." Visible="False"></asp:Label>
            <br />
            <asp:Panel ID="pnlHistory" runat="server" 
                 BackColor="#E2E9EF" Visible="false" BorderStyle="Solid" BorderWidth="1px" BorderColor="#CCCCCC">
                <b>Extension history for credit purchase #</b>
                <asp:Label ID="lblCreditPurchaseID" runat="server" Text="N/A" Font-Bold="True"></asp:Label>
                &nbsp;&nbsp;
                <asp:LinkButton ID="lbHideHistory" runat="server" OnClick="lbHideHistory_Click">(hide)</asp:LinkButton>
                <asp:Label ID="lblRTCError" runat="server" style="padding: 2px; margin:1px; display:inline-block;" Text="There was an error" Font-Bold="True" Visible="false" BackColor="#FFCC99" BorderColor="Red"></asp:Label>
                &nbsp;<asp:GridView ID="gvCreditHistory" runat="server" AutoGenerateColumns="False" CssClass="grviewFixedWidth" DataKeyNames="CREDIT_EXTENSION_ID" EnableModelValidation="True" onRowDataBound="gvCreditHistory_RowDataBound" Width="800px">
                    <Columns>
                        <asp:BoundField DataField="CREDIT_EXTENSION_ID" HeaderText="ExtensionID" Visible="false">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="TYPE" HeaderText="Type">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="ID" HeaderText="ID">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="NAME" HeaderText="Name">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="DATE_EXTENDED" HeaderText="Date extended">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Months applied">
                            <HeaderStyle BackColor="#B6C6D6" />
                            <ItemTemplate>
                                <asp:Label ID="lblNumberMonths" runat="server" Visible="true"></asp:Label>
                                <asp:LinkButton ID="lbReturnToCreditMonths" runat="server" OnClick="lbReturnToCreditMonths_Click" Visible="true"
                                    Enabled="false"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="COST" HeaderText="Cost (£)">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="NUMBER_MONTHS" HeaderText="Months" Visible="false">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                    </Columns>
                </asp:GridView>
                You can <b>Return-To-Credit (RTC)</b> unused months for the "last" extension of a review or account.<br />
                <b>Note:</b> The "last" extension for a review or account could have been made using a credit purchase other than your own.
                <br />
            </asp:Panel>
            <br />
        </asp:Panel>

        <asp:Panel ID="pnlOrganisations" runat="server" Visible="False">
            <b>Your organisations</b>
            <asp:GridView ID="gvOrganisations" runat="server" CssClass="grviewFixedWidth"
                AutoGenerateColumns="False" DataKeyNames="ORGANISATION_ID"
                OnRowCommand="gvOrganisations_RowCommand" OnRowEditing="gvOrganisations_RowEditing"
                Width="800px" EnableModelValidation="True" OnRowDataBound="gvOrganisations_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="ORGANISATION_ID" HeaderText="OrganisationID">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="ORGANISATION_NAME" HeaderText="Organisation">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:ButtonField CommandName="REMOVE" HeaderText="Remove" Text="Remove">
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:ButtonField>
                </Columns>
            </asp:GridView>
            <br />
        </asp:Panel>





        <br />
            <b>Accounts you have purchased</b>
        <asp:Label ID="lblActivateInstructions" runat="server" Text="" Visible="False"></asp:Label><br />
        <asp:GridView ID="gvAccountPurchases" runat="server" CssClass="grviewFixedWidth"
            AutoGenerateColumns="False" DataKeyNames="CONTACT_ID"
            OnRowCommand="gvAccountPurchases_RowCommand" OnRowEditing="gvReviewer_RowEditing"
            Width="800px" EnableModelValidation="True">
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
                <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last login">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField DataField="DATE_CREATED" HeaderText="Account created">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField DataField="EXPIRY_DATE" HeaderText="Expiry date">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:ButtonField CommandName="EDIT" HeaderText="New" Text="Activate">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:ButtonField>
                <asp:ButtonField CommandName="TRANSFER" HeaderText="Existing" Text="Transfer">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:ButtonField>
            </Columns>
        </asp:GridView>

        <asp:Label ID="lblOtherPurchasedAccounts" runat="server"
            Text="You have not purchased any other accounts" Visible="False"></asp:Label>
        <br />

        <asp:Panel runat="server" ID="pnlConfirmRevokeGhostActivation" Visible="false">
            <span style="color: #CC0000"><strong>Are you sure you want to revoke this Ghost 
                    activation request?</strong></span><br />
            The activation link that was sent to the intended user will stop working, and you will be able to re-send a new activation email if/when necessary.<br />
            <asp:Button ID="cmdConfirmRevokeGhostActivation" runat="server"
                CssClass="button" Text="Revoke Activation"
                OnClick="cmdConfirmRevokeGhostActivation_Click" />
            <asp:LinkButton ID="lbCancelAccountEdit2" runat="server"
                OnClick="lbCancelAccountEdit_Click">Cancel</asp:LinkButton>
        </asp:Panel>
        &nbsp;<br />

        <asp:Panel ID="pnlActivateGhostForm" runat="server" Visible="False"
            BackColor="#E2E9EF" BorderStyle="Solid" BorderWidth="1px">

            <b>Activate Account</b><br />
            ContactID:
                    <asp:Label ID="lblActivateGhostContactID" runat="server" Text="N/A"></asp:Label>

            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    <br />
            To activate this account, please fill in the details below: a 'Complete activation' message will be sent to the address you'll indicate below.
            <br />
            The recipient will have <b>14 days to click</b> on the link provided and complete the account activation.
            <br />
            The account subscription will start at the time the activation is completed.<br />

            <table border="1" cellpadding="1" cellspacing="1" width="500">
                <tr>
                    <td style="background-color: #B6C6D6; width: 140px">Full Name</td>
                    <td style="width: 60%">
                        <asp:TextBox ID="tbActivateGhostFullName" runat="server" CssClass="textbox" MaxLength="100"
                            Width="95%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td style="background-color: #B6C6D6; width: 130px">Email</td>
                    <td style="width: 60%">
                        <asp:TextBox ID="tbActivateGhostEmail" runat="server" CssClass="textbox"
                            Width="95%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td style="background-color: #B6C6D6; width: 160px">Confirm email</td>
                    <td style="width: 60%">
                        <asp:TextBox ID="tbActivateGhostEmail1" runat="server" CssClass="textbox"
                            Width="95%"></asp:TextBox>
                    </td>
                </tr>
            </table>
            <asp:Button ID="cmdActivatGhost" runat="server" CssClass="button"
                Text="Activate" OnClick="cmdActivatGhost_Click" />
            &nbsp;&nbsp;
                    <asp:LinkButton ID="cmdCancelActivateAccount" runat="server" OnClick="cmdCancelActivateAccount_Click">Cancel</asp:LinkButton>
            &nbsp;&nbsp;&nbsp;
                    <br />
            <asp:Panel ID="pnlActivateGhostEmailinUse" runat="server" Visible="False"
                Style="margin-top: 0px">
                <table border="1" cellpadding="1" cellspacing="1" width="500" style="background-color: #B6C6E6;">
                    <tr>
                        <td>
                            <b style="font-weight: bold; color: Red;">The email address you indicated is already in use.</b><br />
                            Please click on the "Transfer Credit" button if you wish to transfer the account credits to the account already registered with the
                                 <b>'<asp:Label runat="server" ID="lblGhostAccountTranferCredit"></asp:Label>'</b> email address.

                        </td>
                        <td>
                            <asp:Button runat="server" ID="cmdTransferCredit" Text="Transfer Credit" CssClass="button"
                                OnClick="cmdTransferCredit_Click" />
                            <asp:HiddenField ID="hfGhostCreditTransferMonths" runat="server" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Label ID="lblActivateGhostMessages" runat="server" Font-Bold="True"
                Text="Passwords must be at least 8 characters and contain at least one lower case letter, &lt;br/&gt;one upper case letter, one digit and no spaces."
                Visible="False" ForeColor="Red"></asp:Label>
            <br />
        </asp:Panel>

        <asp:Panel ID="pnlActivateGhostIsDone" runat="server" Visible="False" EnableViewState="false"
            Style="margin-top: 0px">
            <b>All Done: an email was sent to the new account user.<br />
                You may want to alert the recipient and ask her/him to check their Inbox and Spam folders.</b><br />
                The email includes a link that will allow the new user to fill-in the remaining 
                account details (username, password, etc)<br />
                The link will remain active for
                <strong>14 days</strong>, you can generate a new &quot;activate account&quot; email at any 
                time, before or after the 14 days deadline.
        </asp:Panel>

        <asp:Panel ID="pnlActivateIntoExistingAccount" runat="server" Visible="False"
            Style="margin-top: 0px">
            <table border="1" cellpadding="1" cellspacing="1" width="500" style="background-color: #B6C6E6;">
                <tr>
                    <td>
                        <b>Rather than activating the new account (ID:</b>
                        <asp:Label ID="lblSourceGhostAccountID" runat="server" Font-Bold="True" Text=""></asp:Label><b>) 
                        you can transfer the</b>
                        <asp:Label ID="lblMonthsCredit" runat="server" Font-Bold="True" Text=""></asp:Label>
                        <b>month(s) credit to an exsting account</b> (1 month of credit is free, so
                        <asp:Label ID="lblMonthsCredit2" runat="server" Text=""></asp:Label> month(s) were paid for).
                        <br />
                        Enter the email of the existing account and then click on the "Transfer Credit" button.

                    </td>
                    <td>
                        

                    </td>
                </tr>
            </table>
            <table border="1" cellpadding="1" cellspacing="1" width="500" style="background-color: #B6C6E6;">
                <tr>
                    <td style="background-color: #B6C6D6; width: 130px">Email</td>
                    <td style="width: 60%">
                        <asp:TextBox ID="tbTransferEmail" runat="server" CssClass="textbox"
                            Width="95%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td style="background-color: #B6C6D6; width: 160px">Confirm email</td>
                    <td style="width: 60%">
                        <asp:TextBox ID="tbTransferEmailConfirmation" runat="server" CssClass="textbox"
                            Width="95%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                    <asp:Button runat="server" ID="cmdTransferAccountPurchase" Text="Transfer Credit" CssClass="button"
                        OnClick="cmdTransferAccountPurchase_Click" />
                        <asp:Label ID="lblTransferErrorMsg" runat="server" style="padding: 2px; margin:1px; display:inline-block;" Text="" Font-Bold="True" Visible="false" BackColor="#FFCC99" BorderColor="Red"></asp:Label>                        
                    </td>                   
                </tr>
            </table>
        </asp:Panel>

        <asp:Panel ID="pnlTransferFromGhostIsDone" runat="server" Visible="False" EnableViewState="false"
            Style="margin-top: 0px; background-color: yellow;">
            <b>All Done: a notification email was sent to the existing user.</b><br />
            The 'empty' account that held the credit was deleted and your billing record updated accordingly.
        </asp:Panel>
        &nbsp;<br />



    
</asp:Content>