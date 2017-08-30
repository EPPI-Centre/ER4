<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Summary.aspx.cs" Inherits="Summary" Title="Summary" %>
    <%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="Telerik" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <telerik:RadWindowManager ID="RadWindowManager1" runat="server" EnableShadow="true">
        </telerik:RadWindowManager>
    

    
            
                <b>Your account summary&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </b>
                Please note that all dates are dd/mm/yyyy<asp:GridView 
                    ID="gvReviewer" runat="server" 
    AutoGenerateColumns="False" Width="800px" onrowcommand="gvReviewer_RowCommand" CssClass="grviewFixedWidth"
                    DataKeyNames="CONTACT_ID" onrowediting="gvReviewer_RowEditing" 
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
                <br /><asp:Panel ID="pnlContactDetails" runat="server" Visible="False" 
                    BackColor="#E2E9EF" BorderStyle="Solid" BorderWidth="1px">
                    
                    
                    ContactID:
                    <asp:Label ID="lblContactID" runat="server" Text="N/A"></asp:Label>
                    
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    <asp:CheckBox ID="cbSendNewsletter" runat="server" AutoPostBack="True" 
                        oncheckedchanged="cbSendNewsletter_CheckedChanged" Text="Receive newsletter" />
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    
                    <table ID="Table3" border="1" cellpadding="1" cellspacing="1" width="600">
                        <tr>
                            <td style="background-color: #B6C6D6; width: 140px; height: 27px;">
                                Name</td>
                            <td style="width: 170px; height: 27px;">
                                <asp:TextBox ID="tbName" runat="server" CssClass="textbox" MaxLength="100" 
                                    Width="90%"></asp:TextBox>
                            </td>
                            <td style="background-color: #B6C6D6; width: 130px; height: 27px;">
                                Username</td>
                            <td style="width: 170px; height: 27px;">
                                <asp:TextBox ID="tbUserName" runat="server" CssClass="textbox" MaxLength="50" 
                                    Width="90%"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 130px">
                                Email</td>
                            <td style="width: 170px">
                                <asp:TextBox ID="tbEmail" runat="server" CssClass="textbox" Width="90%"></asp:TextBox>
                            </td>
                            <td style="background-color: #B6C6D6; width: 160px">
                                Confirm email</td>
                            <td style="width: 170px">
                                <asp:TextBox ID="tbEmailConfirm" runat="server" CssClass="textbox" Width="90%"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 130px">
                                New password</td>
                            <td style="width: 170px">
                                <asp:TextBox ID="tbPassword" runat="server" CssClass="textbox" MaxLength="50" 
                                    Width="90%"></asp:TextBox>
                            </td>
                            <td style="background-color: #B6C6D6; width: 160px">
                                Confirm new password</td>
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
                        onclick="lbCancelAccountEdit_Click">Cancel</asp:LinkButton>
                    &nbsp;&nbsp;&nbsp;
                    <asp:Label ID="lblPasswordMsg" runat="server" 
                        Text="To keep your existing password leave the password fields blank."></asp:Label>
                    <br />
                    <asp:Panel ID="pnlAccountMessages" runat="server" Visible="False" 
                        style="margin-top: 0px">
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
                <b>Accounts you have purchased</b>
                <asp:GridView ID="gvAccountPurchases" runat="server" CssClass="grviewFixedWidth" 
                    AutoGenerateColumns="False" DataKeyNames="CONTACT_ID" 
                    onrowcommand="gvAccountPurchases_RowCommand"  OnRowEditing="gvReviewer_RowEditing"
                    Width="800px" EnableModelValidation="True" >
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
                        <asp:ButtonField CommandName="EDIT" HeaderText="Edit" Text="Activate">
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
                        onclick="cmdConfirmRevokeGhostActivation_Click" />
                    <asp:LinkButton ID="lbCancelAccountEdit2" runat="server" 
                        onclick="lbCancelAccountEdit_Click">Cancel</asp:LinkButton>
                </asp:Panel>
                &nbsp;<br />
                <asp:Panel ID="pnlActivateGhostForm" runat="server" Visible="False" 
                    BackColor="#E2E9EF" BorderStyle="Solid" BorderWidth="1px">
                    
                    <b>Activate Account</b><br />
                    ContactID:
                    <asp:Label ID="lblActivateGhostContactID" runat="server" Text="N/A"></asp:Label>
                    
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    <br />
                    To activate this account, please fill in the details below: a 'Complete activation' message will be sent to the address you'll indicate below. <br />
                    The recipient will have <b>14 days to click</b> on the link provided and complete the account activation. <br />
                    The account subscription will start at the time the activation is completed.<br />
                    
                    <table border="1" cellpadding="1" cellspacing="1" width="500">
                        <tr>
                            <td style="background-color: #B6C6D6; width: 140px">
                                 Full Name</td>
                            <td style="width: 60%">
                                <asp:TextBox ID="tbActivateGhostFullName" runat="server" CssClass="textbox" MaxLength="100" 
                                    Width="95%"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 130px">
                                Email</td>
                            <td style="width: 60%">
                                <asp:TextBox ID="tbActivateGhostEmail" runat="server" CssClass="textbox" 
                                    Width="95%"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 160px">
                                Confirm email</td>
                            <td style="width: 60%">
                                <asp:TextBox ID="tbActivateGhostEmail1" runat="server" CssClass="textbox" 
                                    Width="95%"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                    <asp:Button ID="cmdActivatGhost" runat="server" CssClass="button" 
                         Text="Activate" onclick="cmdActivatGhost_Click" />
                    &nbsp;&nbsp;
                    <asp:LinkButton ID="cmdCancelActivateAccount" runat="server" onclick="cmdCancelActivateAccount_Click">Cancel</asp:LinkButton>
                    &nbsp;&nbsp;&nbsp;
                    <br />
                    <asp:Panel ID="pnlActivateGhostEmailinUse" runat="server" Visible="False" 
                        style="margin-top: 0px">
                        <table border="1" cellpadding="1" cellspacing="1" width="500" style="background-color: #B6C6E6;">
                        <tr>
                            <td >
                                 <b style="font-weight:bold; color:Red;">The email address you indicated is already in use.</b><br />
                                 Please click on the "Transfer Credit" button if you wish to transfer the account credits to the account already registered with the
                                 <b>'<asp:Label runat="server" ID="lblGhostAccountTranferCredit"></asp:Label>'</b> email address.

                            </td>
                            <td >
                                <asp:Button runat="server" ID="cmdTransferCredit" Text="Transfer Credit" CssClass="button"
                                    onclick="cmdTransferCredit_Click" />
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
                        style="margin-top: 0px">
                        <b>All Done: an email was sent to the new account user.<br /> 
                        You may want to 
                        alert the recipient and ask her/him to check their Inbox and Spam folders.</b><br />
                        The email includes a link that will allow the new user to fill-in the remaining 
                        account details (username, password, etc)<br /> The link will remain active for
                        <strong>14 days</strong>, you can generate a new &quot;activate account&quot; email at any 
                        time, before or after the 14 days deadline.</asp:Panel>
                <asp:Panel ID="pnlTransferFromGhostIsDone" runat="server" Visible="False" EnableViewState="false"
                        style="margin-top: 0px; background-color:yellow;">
                        <b>All Done: a notification email was sent to the existing user.</b><br /> 
                        The 'empty' account that held the credit was deleted and your billing record updated accordingly.
                        </asp:Panel>
                &nbsp;<br />
                <b>Shareable reviews you have purchased&nbsp;or have adminstrative rights to</b><asp:GridView 
                    ID="gvReview" runat="server" CssClass="grviewFixedWidth"
                    AutoGenerateColumns="False" Width="800px" onrowcommand="gvReview_RowCommand" 
                    DataKeyNames="REVIEW_ID">
                    <Columns>
                        <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="DATE_CREATED" HeaderText="Date review created">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last login by this reviewer">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>    
                        <asp:BoundField DataField="EXPIRY_DATE" HeaderText="Expiry date">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:ButtonField CommandName="EDT" HeaderText="Edit" Text="Edit">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
                <asp:Label ID="lblShareableReviews" runat="server" 
                    Text="You have not purchased any shareable reviews." Visible="False"></asp:Label>
                <br />
                <asp:Panel ID="pnlEditShareableReview" runat="server" Visible="False">
                    
                   
                    <asp:Panel ID="pnlReviewDetails" runat="server" BorderStyle="Solid" 
                        BorderWidth="1px" BackColor="#E2E9EF">
                        <table id="Table5" border="1" cellpadding="1" cellspacing="1" width="600">
                        <tr>
                            <td style="background-color: #B6C6D6; width: 15%">
                                Review #</td>
                            <td style="width: 85%">
                                <asp:Label ID="lblShareableReviewNumber" runat="server" Text="Number"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 15%">
                                Review title</td>
                            <td style="width: 85%">
                                <asp:TextBox ID="tbShareableReviewName" runat="server" CssClass="textbox" 
                                    MaxLength="1000" Width="98%" EnableViewState="False"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                        <table style="width:100%;">
                            <tr>
                                <td style="width: 40%">
                                    <asp:Button ID="cmdSaveShareableReview" runat="server" onclick="cmdSaveShareableReview_Click" Text="Save" />
                                    &nbsp;&nbsp;
                                    <asp:LinkButton ID="lbCancelReviewDetailsEdit" runat="server" onclick="lbCancelReviewDetailsEdit_Click">Cancel</asp:LinkButton>
                                </td>
                                <td style="text-align: right; font-weight: bold;">
                                    <asp:Label ID="lblPSShareableReview" runat="server" Text="Priority screening" Visible="False"></asp:Label>
                                </td>
                                <td style="text-align: left; width: 40%;">
                                    <asp:RadioButtonList ID="rblPSShareableEnable" runat="server" AutoPostBack="True" onselectedindexchanged="rblPSShareableEnable_SelectedIndexChanged" RepeatDirection="Horizontal" Visible="False">
                                        <asp:ListItem Value="True">On</asp:ListItem>
                                        <asp:ListItem Selected="True" Value="False">Off</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                            </tr>
                            <tr>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:LinkButton ID="lbBritLibCodesShared" runat="server" onclick="lbBritLibCodesShared_Click">BL codes</asp:LinkButton>
                                </td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                            </tr>
                        </table>
                        <asp:Panel ID="pnlBritLibCodesShared" runat="server" BorderColor="#999999" BorderStyle="Solid" BorderWidth="2px" Visible="False" Width="792px">
                            If you have document ordering codes from the <b>British Library</b> you can enter them here.
                            <asp:LinkButton ID="lbCancelBLShared" runat="server" onclick="lbCancelBLShared_Click">(cancel)</asp:LinkButton>
                            <br />
                            <table style="width:100%;">
                                <tr>
                                    <td colspan="2">Library privilege codes&nbsp;&nbsp;
                                        <asp:LinkButton ID="lbSaveBritLibLPCodesShared" runat="server" onclick="lbSaveBritLibLPCodesShared_Click">Save</asp:LinkButton>
                                    </td>
                                    <td style="width: 25%">Copyright cleared codes&nbsp;&nbsp;
                                        <asp:LinkButton ID="lbSaveBritLibCCCodesShared" runat="server" onclick="lbSaveBritLibCCCodesShared_Click">Save</asp:LinkButton>
                                    </td>
                                    <td style="width: 25%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td style="width: 25%">
                                        <asp:TextBox ID="tbLPC_ACC_Share" runat="server" Width="90%"></asp:TextBox>
                                    </td>
                                    <td style="width: 25%">Account code</td>
                                    <td style="width: 25%">
                                        <asp:TextBox ID="tbCCC_ACC_Share" runat="server" Width="90%"></asp:TextBox>
                                    </td>
                                    <td style="width: 25%">Account code</td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:TextBox ID="tbLPC_AUT_Share" runat="server" Width="90%"></asp:TextBox>
                                    </td>
                                    <td>Authorisation code</td>
                                    <td>
                                        <asp:TextBox ID="tbCCC_AUT_Share" runat="server" Width="90%"></asp:TextBox>
                                    </td>
                                    <td>Authorisation code</td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:TextBox ID="tbLPC_TX_Share" runat="server" Width="90%"></asp:TextBox>
                                    </td>
                                    <td>TX line to export</td>
                                    <td>
                                        <asp:TextBox ID="tbCCC_TX_Share" runat="server" Width="90%"></asp:TextBox>
                                    </td>
                                    <td>TX line to export</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <b>Members of this review</b>&nbsp;&nbsp;&nbsp;
                    <asp:LinkButton ID="lbInviteReviewer" runat="server" 
                        onclick="lbInviteReviewer_Click">Send invitation</asp:LinkButton>
                    <asp:GridView ID="gvMembersOfReview" runat="server" AutoGenerateColumns="False" 
                        Width="800px" DataKeyNames="CONTACT_ID"  CssClass="grviewFixedWidth"
                            onrowcommand="gvMembersOfReview_RowCommand" 
                            onrowdatabound="gvMembersOfReview_RowDataBound" 
                            EnableModelValidation="True">
                        <Columns>
                            <asp:BoundField DataField="CONTACT_ID" HeaderText="Contact ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                            </asp:BoundField>
                            <asp:BoundField DataField="CONTACT_NAME" HeaderText="Reviewer">
                            <HeaderStyle BackColor="#B6C6D6" />
                            </asp:BoundField>
                            <asp:BoundField DataField="EMAIL" HeaderText="Email">
                            <HeaderStyle BackColor="#B6C6D6" />
                            </asp:BoundField>
                            <asp:BoundField HeaderText="Last access" DataField="LAST_LOGIN">
                            <HeaderStyle BackColor="#B6C6D6" />
                            </asp:BoundField>
                            <asp:TemplateField HeaderText="Coding only">
                                <EditItemTemplate>
                                    <asp:CheckBox ID="CheckBox1" runat="server" />
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbCodingOnly" runat="server" AutoPostBack="True" 
                                        oncheckedchanged="cbCodingOnly_CheckedChanged" />
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Read only">
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbReadOnly" runat="server" AutoPostBack="True" 
                                        oncheckedchanged="cbReadOnly_CheckedChanged" />
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Review admin">
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbReviewAdmin" runat="server" AutoPostBack="True" 
                                        oncheckedchanged="cbReviewAdmin_CheckedChanged" />
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:TemplateField>
                            <asp:ButtonField HeaderText="Remove from review" Text="Remove" 
                                CommandName="REMOVE">
                            <HeaderStyle BackColor="#B6C6D6" />
                            </asp:ButtonField>
                        </Columns>
                    </asp:GridView>
                    <br />
                    <asp:Panel ID="pnlInviteReviewer" runat="server" Visible="False">
                        <asp:TextBox ID="tbInvite" runat="server" Width="200px"></asp:TextBox>
                        &nbsp;&nbsp;
                        <asp:Button ID="cmdInvite" runat="server" Text="Invite" 
                            onclick="cmdInvite_Click" />
                        &nbsp;
                        <asp:Label ID="lblInviteMsg" runat="server" Font-Bold="True" 
                            Text="Invalid account" Visible="False" ForeColor="Red"></asp:Label>
                        &nbsp;<br />
                        Enter a users email address and select <b>Invite</b>.
                        <br />
                        If the account is valid it will be placed in the review and an email send to the 
                        account holder.
                    </asp:Panel>
                        
                    </asp:Panel>


                </asp:Panel>
                
                
                <br />
                <b>Your non-shareable reviews </b>(you create non-shareable reviews within 
                EPPI-Reviewer 4)<br />
                <asp:GridView ID="gvReviewNonShareable" runat="server" 
                    AutoGenerateColumns="False" Width="800px"  CssClass="grviewFixedWidth"
                    onrowcommand="gvReviewNonShareable_RowCommand" 
                    onrowediting="gvReviewNonShareable_RowEditing" DataKeyNames="REVIEW_ID">
                    <Columns>
                        <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                        <HeaderStyle BackColor="#B6C6D6" CssClass="gvReviewNonShareableCol1" />
                        </asp:BoundField>
                        <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                        <HeaderStyle BackColor="#B6C6D6" CssClass="gvReviewNonShareableCol2" />
                        </asp:BoundField>
                        <asp:BoundField DataField="DATE_CREATED" HeaderText="Date created">
                        <HeaderStyle BackColor="#B6C6D6" CssClass="gvReviewNonShareableCol3" />
                        </asp:BoundField>
                        <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last login by this reviewer">
                        <HeaderStyle BackColor="#B6C6D6" CssClass="gvReviewNonShareableCol4" />
                        </asp:BoundField>
                        <asp:ButtonField CommandName="EDT" HeaderText="Edit" Text="Edit">
                        <HeaderStyle BackColor="#B6C6D6" CssClass="gvReviewNonShareableCol5" />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
                <asp:Label ID="lblNonShareableReviews" runat="server" 
                    Text="You do not own any non-shareable reviews." Visible="False"></asp:Label>
                <br />
                <asp:Panel ID="pnlEditNonShareableReview" runat="server" Visible="False" 
                    BackColor="#E2E9EF" BorderStyle="Solid" BorderWidth="1px">
                    <table ID="Table4" border="1" cellpadding="1" cellspacing="1" width="600">
                        <tr>
                            <td style="background-color: #B6C6D6; width: 15%">
                                Review #</td>
                            <td style="width: 60%">
                                <asp:Label ID="lblNonShareableReviewNumber" runat="server" Text="Number"></asp:Label>
                            </td>
                            <td align="center" style="width: 15%">
                                <asp:LinkButton ID="lbDeleteReview" runat="server">Delete</asp:LinkButton>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 15%">
                                Review title</td>
                            <td style="width: 85%" colspan="2">
                                <asp:TextBox ID="tbReviewName" runat="server" CssClass="textbox" MaxLength="1000" 
                                    Width="98%" EnableViewState="False"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                    <table style="width:100%;">
                            <tr>
                                <td style="width: 40%">
                                    <asp:Button ID="cmdSaveNonShareableReview" runat="server" onclick="cmdSaveNonShareableReview_Click" Text="Save" />
                                    &nbsp;&nbsp;
                                    <asp:LinkButton ID="lbCancelNSReviewDetailsEdit" runat="server" onclick="lbCancelNSReviewDetailsEdit_Click">Cancel</asp:LinkButton>
                                </td>
                                <td style="text-align: right; font-weight: bold;">
                                    <asp:Label ID="lblPSNonShareableEnable" runat="server" Text="Priority screening" Visible="False"></asp:Label>
                                </td>
                                <td style="text-align: left; width: 40%;">
                                    <asp:RadioButtonList ID="rblPSNonShareableEnable" runat="server" AutoPostBack="True" onselectedindexchanged="rblPSNonShareableEnable_SelectedIndexChanged" RepeatDirection="Horizontal" Visible="False">
                                        <asp:ListItem Value="True">On</asp:ListItem>
                                        <asp:ListItem Selected="True" Value="False">Off</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                            </tr>
                            <tr>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:LinkButton ID="lbBritLibCodesNonShared" runat="server" onclick="lbBritLibCodesNonShared_Click">BL codes</asp:LinkButton>
                                    &nbsp;&nbsp;&nbsp;
                                    <asp:LinkButton ID="lbChangeToShareable" runat="server" onclick="lbChangeToShareable_Click" Visible="False">Change to a shareable review</asp:LinkButton>
                                </td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                            </tr>
                        </table>
                   
                    <asp:Panel ID="pnlBritLibCodesNonShared" runat="server" Visible="False" 
                        Width="792px" BorderColor="#999999" BorderStyle="Solid" BorderWidth="2px" 
                        ScrollBars="Horizontal">
                        If you have document ordering codes from the <b>British Library</b> you can 
                        enter them here.
                        <asp:LinkButton ID="lbCancelBLNonShared" runat="server" 
                            onclick="lbCancelBLNonShared_Click">(cancel)</asp:LinkButton>
                        <br />
                        <table style="width:100%;">
                            <tr>
                                <td colspan="2">
                                    Library privilege codes&nbsp;&nbsp;
                                    <asp:LinkButton ID="lbSaveBritLibLPCodesNonShared" runat="server" 
                                        onclick="lbSaveBritLibLPCodesNonShared_Click">Save</asp:LinkButton>
                                </td>
                                <td style="width: 25%">
                                    Copyright cleared codes&nbsp;&nbsp;
                                    <asp:LinkButton ID="lbSaveBritLibCCCodesNonShared" runat="server" 
                                        onclick="lbSaveBritLibCCCodesNonShared_Click">Save</asp:LinkButton>
                                </td>
                                <td style="width: 25%">
                                    &nbsp;</td>
                            </tr>
                            <tr>
                                <td style="width: 25%">
                                    <asp:TextBox ID="tbLPC_ACC_NonShare" runat="server" Width="90%"></asp:TextBox>
                                </td>
                                <td style="width: 25%">
                                    Account code</td>
                                <td style="width: 25%">
                                    <asp:TextBox ID="tbCCC_ACC_NonShare" runat="server" Width="90%"></asp:TextBox>
                                </td>
                                <td style="width: 25%">
                                    Account code</td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:TextBox ID="tbLPC_AUT_NonShare" runat="server" Width="90%"></asp:TextBox>
                                </td>
                                <td>
                                    Authorisation code</td>
                                <td>
                                    <asp:TextBox ID="tbCCC_AUT_NonShare" runat="server" Width="90%"></asp:TextBox>
                                </td>
                                <td>
                                    Authorisation code</td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:TextBox ID="tbLPC_TX_NonShare" runat="server" Width="90%"></asp:TextBox>
                                </td>
                                <td>
                                    TX line to export</td>
                                <td>
                                    <asp:TextBox ID="tbCCC_TX_NonShare" runat="server" Width="90%"></asp:TextBox>
                                </td>
                                <td>
                                    TX line to export</td>
                            </tr>
                            <tr>
                                <td>
                                    &nbsp;</td>
                                <td>
                                    &nbsp;</td>
                                <td>
                                    &nbsp;</td>
                                <td>
                                    &nbsp;</td>
                            </tr>
                        </table>
                    </asp:Panel>
                    
                    <asp:Panel ID="pnlChangeToShareable" runat="server" Visible="False">
                        Making the review shareable is not available at this time.<br />
                    </asp:Panel>
                    
                </asp:Panel>
                
                <br />
                <b>Other shareable reviews you are a member of</b><br />
                <asp:GridView 
                    ID="gvReviewShareableMember" runat="server" AutoGenerateColumns="False" 
                     onrowcommand="gvReviewShareableMember_RowCommand"
                    DataKeyNames="REVIEW_ID" CssClass="grviewFixedWidth" 
                    onrowdatabound="gvReviewShareableMember_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                        <HeaderStyle BackColor="#B6C6D6" 
                             />
                        </asp:BoundField>
                        <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                        <HeaderStyle BackColor="#B6C6D6"  
                            />
                        </asp:BoundField>
                        <asp:BoundField HeaderText="Review owner" DataField="REVIEW_OWNER">
                        <HeaderStyle BackColor="#B6C6D6"
                             />
                        </asp:BoundField>
                        <asp:BoundField DataField="DATE_CREATED" HeaderText="Date review created">
                        <HeaderStyle BackColor="#B6C6D6" 
                             />
                        </asp:BoundField>
                        <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last login by this reviewer">
                        <HeaderStyle BackColor="#B6C6D6" 
                             />
                        </asp:BoundField>
                        <asp:ButtonField CommandName="REMOVE" HeaderText="Remove from review" 
                            Text="Remove">
                        <HeaderStyle BackColor="#B6C6D6" 
                            />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
                
                <asp:Label ID="lblNonShareableReviewsMember" runat="server" 
                    
                    Text="You are not a member of any other shareable reviews." 
                    Visible="False"></asp:Label>
                <br />
                <br />
        <asp:Panel ID="pnlCochraneReviews" runat="server" Visible="False">
            &nbsp;<br /> <b>Prospective Cochrane reviews</b><asp:GridView ID="gvReviewCochrane" runat="server" 
                AutoGenerateColumns="False" CssClass="grviewFixedWidth" 
                DataKeyNames="REVIEW_ID" onrowcommand="gvReviewCochrane_RowCommand" Width="800px" 
                EnableModelValidation="True">
                <Columns>
                    <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="DATE_CREATED" HeaderText="Date review created">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last login by this reviewer">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="EXPIRY_DATE" HeaderText="Expiry date" 
                        Visible="False">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="ARCHIE_ID" HeaderText="Archie ID">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:ButtonField CommandName="EDITROW" HeaderText="Edit" Text="Edit">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:ButtonField>
                </Columns>
            </asp:GridView>

            <asp:Panel ID="pnlEditShareableReviewCochrane" runat="server" Visible="False">
            <br />
            <asp:Panel ID="pnlReviewDetailsCochrane" runat="server" BackColor="#E2E9EF" 
                BorderStyle="Solid" BorderWidth="1px">
                <table ID="Table6" border="1" cellpadding="1" cellspacing="1" width="600">
                    <tr>
                        <td style="background-color: #B6C6D6; width: 15%; height: 18px;">
                            Review #</td>
                        <td style="width: 85%; height: 18px;">
                            <asp:Label ID="lblShareableReviewNumberCochrane" runat="server" Text="Number"></asp:Label>
                            &nbsp;<asp:Label ID="lblFID" runat="server" Text="Label" Visible="False"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color: #B6C6D6; width: 15%">
                            Review title</td>
                        <td style="width: 85%">
                            <asp:TextBox ID="tbShareableReviewNameCochrane" runat="server" CssClass="textbox" 
                                EnableViewState="False" MaxLength="1000" Width="98%"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                <table style="width: 100%;">
                    <tr>
                        <td style="width: 40%">
                            <asp:Button ID="cmdSaveShareableReviewCochrane" runat="server" onclick="cmdSaveShareableReviewCochrane_Click" Text="Save" />
                            &nbsp;&nbsp;
                            <asp:LinkButton ID="lbCancelReviewDetailsEdit0" runat="server" onclick="lbCancelReviewDetailsEdit0_Click">Cancel</asp:LinkButton>
                        </td>
                        <td style="text-align: right; font-weight: bold;">
                            <asp:Label ID="lblPSProsCochraneReviewEnable" runat="server" Text="Priority screening" Visible="False"></asp:Label>
                        </td>
                        <td style="text-align: left; width: 40%;">
                            <asp:RadioButtonList ID="rblPSProsCochraneReviewEnable" runat="server" AutoPostBack="True" OnSelectedIndexChanged="rblPSProsCochraneReviewEnable_SelectedIndexChanged" RepeatDirection="Horizontal" Visible="False">
                                <asp:ListItem Value="True">On</asp:ListItem>
                                <asp:ListItem Selected="True" Value="False">Off</asp:ListItem>
                            </asp:RadioButtonList>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>
                            <asp:LinkButton ID="lbBritLibCodesProCochrane" runat="server" onclick="lbBritLibCodesProCochrane_Click">BL codes</asp:LinkButton>
                            &nbsp;&nbsp;&nbsp; </td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                </table>
                <asp:Panel ID="pnlBritLibCodesProCochrane" runat="server" BorderColor="#999999" BorderStyle="Solid" BorderWidth="2px" Visible="False" Width="792px">
                    If you have document ordering codes from the <b>British Library</b> you can enter them here.
                    <asp:LinkButton ID="lbCancelBLProCochrane" runat="server" onclick="lbCancelBLProCochrane_Click">(cancel)</asp:LinkButton>
                    <br />
                    <table style="width: 100%;">
                        <tr>
                            <td colspan="2">Library privilege codes&nbsp;&nbsp;
                                <asp:LinkButton ID="lbSaveBritLibLPCodesProCochrane" runat="server" onclick="lbSaveBritLibLPCodesProCochrane_Click">Save</asp:LinkButton>
                            </td>
                            <td style="width: 25%">Copyright cleared codes&nbsp;&nbsp;
                                <asp:LinkButton ID="lbSaveBritLibCCCodesProCochrane" runat="server" onclick="lbSaveBritLibCCCodesProCochrane_Click">Save</asp:LinkButton>
                            </td>
                            <td style="width: 25%">&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="width: 25%">
                                <asp:TextBox ID="tbLPC_ACC_ProCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td style="width: 25%">Account code</td>
                            <td style="width: 25%">
                                <asp:TextBox ID="tbCCC_ACC_ProCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td style="width: 25%">Account code</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="tbLPC_AUT_ProCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td>Authorisation code</td>
                            <td>
                                <asp:TextBox ID="tbCCC_AUT_ProCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td>Authorisation code</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="tbLPC_TX_ProCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td>TX line to export</td>
                            <td>
                                <asp:TextBox ID="tbCCC_TX_ProCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td>TX line to export</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </asp:Panel>
                <br />
                <b>Members of this review</b>&nbsp;&nbsp;&nbsp;
                <asp:LinkButton ID="lbInviteReviewerCochrane" runat="server" 
                    onclick="lbInviteReviewerCochrane_Click" Visible="False">Send invitation</asp:LinkButton>
                <asp:GridView ID="gvMembersOfReviewCochrane" runat="server" 
                    AutoGenerateColumns="False" CssClass="grviewFixedWidth" 
                    DataKeyNames="CONTACT_ID" EnableModelValidation="True" 
                    onrowcommand="gvMembersOfReviewCochrane_RowCommand" 
                    onrowdatabound="gvMembersOfReviewCochrane_RowDataBound" Width="800px">
                    <Columns>
                        <asp:BoundField DataField="CONTACT_ID" HeaderText="Contact ID">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="CONTACT_NAME" HeaderText="Reviewer">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EMAIL" HeaderText="Email">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last access">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Coding only?">
                            <EditItemTemplate>
                                <asp:CheckBox ID="CheckBox2" runat="server" />
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="cbCodingOnlyCochrane" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbCodingOnlyCochrane_CheckedChanged" />
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Read only">
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbReadOnlyCochrane" runat="server" AutoPostBack="True" 
                                        oncheckedchanged="cbReadOnlyCochrane_CheckedChanged" />
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:TemplateField>
                        <asp:TemplateField HeaderText="Review admin">
                            <ItemTemplate>
                                <asp:CheckBox ID="cbReviewAdminCochrane" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbReviewAdminCochrane_CheckedChanged" />
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:ButtonField CommandName="REMOVE" HeaderText="Remove from review" 
                            Text="Remove">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
                <br />
                <asp:Panel ID="pnlInviteReviewerCochrane" runat="server" Visible="False">
                    <asp:TextBox ID="tbInviteCochrane" runat="server" Width="200px"></asp:TextBox>
                    &nbsp;&nbsp;
                    <asp:Button ID="cmdInviteCochrane" runat="server" onclick="cmdInviteCochrane_Click" 
                        Text="Invite" />
                    &nbsp;
                    <asp:Label ID="lblInviteMsgCochrane" runat="server" Font-Bold="True" ForeColor="Red" 
                        Text="Invalid account" Visible="False"></asp:Label>
                    &nbsp;<br /> Enter a users email address and select <b>Invite</b>.
                    <br />
                    If the account is valid it will be placed in the review and an email send to the 
                    account holder.
                </asp:Panel>
            </asp:Panel>
            </asp:Panel>
            <asp:Label ID="lblShareableReviewsCochrane" runat="server" 
                Text="You have not purchased any shareable reviews." Visible="False"></asp:Label>
            <br />
        </asp:Panel>
        <asp:Panel ID="pnlCochraneReviewsFull" runat="server" Visible="False">
            &nbsp;<br /> <b>Full Cochrane reviews</b><asp:GridView 
                ID="gvReviewCochraneFull" runat="server" 
                AutoGenerateColumns="False" CssClass="grviewFixedWidth" 
                DataKeyNames="REVIEW_ID" onrowcommand="gvReviewCochraneFull_RowCommand" Width="800px" 
                EnableModelValidation="True">
                <Columns>
                    <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="DATE_CREATED" HeaderText="Date review created">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last login by this reviewer">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="EXPIRY_DATE" HeaderText="Expiry date" 
                        Visible="False">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="ARCHIE_ID" HeaderText="Archie ID">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:ButtonField CommandName="EDITROW" HeaderText="Edit" Text="Edit">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:ButtonField>
                </Columns>
            </asp:GridView>

            <asp:Panel ID="pnlEditShareableReviewCochraneFull" runat="server" 
                Visible="False">
            <br />
            <asp:Panel ID="pnlReviewDetailsCochraneFull" runat="server" BackColor="#E2E9EF" 
                BorderStyle="Solid" BorderWidth="1px">
                <table ID="Table7" border="1" cellpadding="1" cellspacing="1" width="600">
                    <tr>
                        <td style="background-color: #B6C6D6; width: 15%; height: 18px;">
                            Review #</td>
                        <td style="width: 85%; height: 18px;">
                            <asp:Label ID="lblShareableReviewNumberCochraneFull" runat="server" 
                                Text="Number"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color: #B6C6D6; width: 15%">
                            Review title</td>
                        <td style="width: 85%">
                            <asp:TextBox ID="tbShareableReviewNameCochraneFull" runat="server" CssClass="textbox" 
                                EnableViewState="False" MaxLength="1000" Width="98%"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                <table style="width: 100%;">
                    <tr>
                        <td style="width: 40%">
                            <asp:Button ID="cmdSaveShareableReviewCochraneFull" runat="server" onclick="cmdSaveShareableReviewCochraneFull_Click" Text="Save" />
                            &nbsp;&nbsp;
                            <asp:LinkButton ID="lbCancelReviewDetailsEdit2" runat="server" onclick="lbCancelReviewDetailsEdit2_Click">Cancel</asp:LinkButton>
                        </td>
                        <td style="text-align: right; font-weight: bold;">
                            <asp:Label ID="lblPSFullCochraneReviewEnable" runat="server" Text="Priority screening" Visible="False"></asp:Label>
                        </td>
                        <td style="text-align: left; width: 40%;">
                            <asp:RadioButtonList ID="rblPSFullCochraneReviewEnable" runat="server" AutoPostBack="True" OnSelectedIndexChanged="rblPSFullCochraneReviewEnable_SelectedIndexChanged" RepeatDirection="Horizontal" Visible="False">
                                <asp:ListItem Value="True">On</asp:ListItem>
                                <asp:ListItem Selected="True" Value="False">Off</asp:ListItem>
                            </asp:RadioButtonList>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>
                            <asp:LinkButton ID="lbBritLibCodesFullCochrane" runat="server" onclick="lbBritLibCodesFullCochrane_Click">BL codes</asp:LinkButton>
                            &nbsp;&nbsp;&nbsp; </td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                </table>
                <asp:Panel ID="pnlBritLibCodesFullCochrane" runat="server" BorderColor="#999999" BorderStyle="Solid" BorderWidth="2px" Visible="False" Width="800px">
                    If you have document ordering codes from the <b>British Library</b> you can enter them here.
                    <asp:LinkButton ID="lbCancelBLFullCochrane" runat="server" onclick="lbCancelBLFullCochrane_Click">(cancel)</asp:LinkButton>
                    <br />
                    <table style="width: 100%;">
                        <tr>
                            <td colspan="2">Library privilege codes&nbsp;&nbsp;
                                <asp:LinkButton ID="lbSaveBritLibLPCodesFullCochrane" runat="server" onclick="lbSaveBritLibLPCodesFullCochrane_Click">Save</asp:LinkButton>
                            </td>
                            <td style="width: 25%">Copyright cleared codes&nbsp;&nbsp;
                                <asp:LinkButton ID="lbSaveBritLibCCCodesFullCochrane" runat="server" onclick="lbSaveBritLibCCCodesFullCochrane_Click">Save</asp:LinkButton>
                            </td>
                            <td style="width: 25%">&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="width: 25%">
                                <asp:TextBox ID="tbLPC_ACC_FullCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td style="width: 25%">Account code</td>
                            <td style="width: 25%">
                                <asp:TextBox ID="tbCCC_ACC_FullCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td style="width: 25%">Account code</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="tbLPC_AUT_FullCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td>Authorisation code</td>
                            <td>
                                <asp:TextBox ID="tbCCC_AUT_FullCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td>Authorisation code</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="tbLPC_TX_FullCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td>TX line to export</td>
                            <td>
                                <asp:TextBox ID="tbCCC_TX_FullCochrane" runat="server" Width="90%"></asp:TextBox>
                            </td>
                            <td>TX line to export</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </asp:Panel>
                <b>Members of this review</b>&nbsp;&nbsp;&nbsp;
                <asp:LinkButton ID="lbInviteReviewerCochraneFull" runat="server" 
                    onclick="lbInviteReviewerCochraneFull_Click" Visible="False">Send invitation</asp:LinkButton>
                <asp:GridView ID="gvMembersOfReviewCochraneFull" runat="server" 
                    AutoGenerateColumns="False" CssClass="grviewFixedWidth" 
                    DataKeyNames="CONTACT_ID" EnableModelValidation="True" 
                    onrowcommand="gvMembersOfReviewCochraneFull_RowCommand" 
                    onrowdatabound="gvMembersOfReviewCochraneFull_RowDataBound" Width="800px">
                    <Columns>
                        <asp:BoundField DataField="CONTACT_ID" HeaderText="Contact ID">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="CONTACT_NAME" HeaderText="Reviewer">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EMAIL" HeaderText="Email">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last access">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Coding only?">
                            <EditItemTemplate>
                                <asp:CheckBox ID="CheckBox3" runat="server" />
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="cbCodingOnlyCochraneFull" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbCodingOnlyCochraneFull_CheckedChanged" />
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Read only">
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbReadOnlyCochraneFull" runat="server" AutoPostBack="True" 
                                        oncheckedchanged="cbReadOnlyCochraneFull_CheckedChanged" />
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:TemplateField>
                        <asp:TemplateField HeaderText="Review admin">
                            <ItemTemplate>
                                <asp:CheckBox ID="cbReviewAdminCochraneFull" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbReviewAdminCochraneFull_CheckedChanged" />
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:ButtonField CommandName="REMOVE" HeaderText="Remove from review" 
                            Text="Remove" Visible="False">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
                <br />
                <asp:Panel ID="pnlInviteReviewerCochraneFull" runat="server" Visible="False">
                    <asp:TextBox ID="tbInviteCochraneFull" runat="server" Width="200px"></asp:TextBox>
                    &nbsp;&nbsp;
                    <asp:Button ID="cmdInviteCochraneFull" runat="server" onclick="cmdInviteCochraneFull_Click" 
                        Text="Invite" />
                    &nbsp;
                    <asp:Label ID="lblInviteMsgCochraneFull" runat="server" Font-Bold="True" ForeColor="Red" 
                        Text="Invalid account" Visible="False"></asp:Label>
                    &nbsp;<br /> Enter a users email address and select <b>Invite</b>.
                    <br />
                    If the account is valid it will be placed in the review and an email send to the 
                    account holder.
                </asp:Panel>
            </asp:Panel>
            </asp:Panel>
            <asp:Label ID="lblShareableReviewsCochraneFull" runat="server" 
                Text="You have not purchased any shareable reviews." Visible="False"></asp:Label>
            <br />
        </asp:Panel>
                <br />
            
    

    
    <br />
    
    
</asp:Content>