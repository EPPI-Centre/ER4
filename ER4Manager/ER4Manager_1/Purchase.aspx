<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Purchase.aspx.cs" Inherits="Purchase" Title="Purchase" %>



<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div>

   <asp:Panel ID="pnlProceedToWPM" runat="server" Visible="False" >
                
                 
                <table align="center" cellpadding="18"
                    style="margin-top:15px; margin-bottom:15px; border: 2; border-style:solid; border-color: #527219; max-width:650px;">
                    <tr><td>You will now be transferred to the secured payment pages, this will open in a new window.<br />
                        Please click &quot;Continue&quot; to proceed:&nbsp;
                        </td>
                        <td>
                            <asp:Panel runat="server" ID="pnlContinueToWPMUCL" Visible="false">
                                <a  href="JumpToWPMUCL.aspx" target="_blank" id="ContinueToWPMUCL"><font class="ContinueToWPM">&nbsp;</font></a>
                            </asp:Panel>
                            <asp:Panel runat="server" ID="pnlContinueToWPM" Visible="true">
                                <a  href="JumpToWPM.aspx" target="_blank" id="ContinueToWPM"><font class="ContinueToWPM">&nbsp;</font></a>
                            </asp:Panel>
                        </td>
                    </tr>
                    <tr>
                    <td colspan="2"> The payment pages are hosted on a specialised third party system. <b>The EPPI-Centre will not receive, process, nor store your credit or debit card details.</b> 
                    All data are transmitted on an SSL encrypted connection. Our external provider implements the <b>3-D Secure™</b> protocol for extra-security.
                    </td>
                    </tr>
                </table>
    </asp:Panel>     
        
<asp:Panel ID="pnTermsAndConditions" runat="server" Visible="False" BackColor="#E2E9EF" 
            BorderColor="Black" BorderStyle="Solid" BorderWidth="1px">
                <b>Terms and Conditions</b><br />
                <br />
                
                <div style="text-align: left;height : 400px; width : auto;overflow : auto;">
                    <asp:Table ID="tbTerms" runat="server" BorderWidth="1px" CellSpacing="0" 
                        Font-Size="Smaller" GridLines="Both" Width="770px">
                    </asp:Table>
                </div>
                
                <br />
                <table style="width:100%;">
                    <tr>
                        <td>
                            <asp:Button ID="cmdAgree0" runat="server" onclick="cmdAgree_Click" 
                                Text="Agree" />
                            &nbsp;&nbsp; <b>Clicking on Agree indicates the purchaser agrees with these conditions.</b></td>
                        <td style="text-align: right">
                            <asp:Button ID="cmdDisagree0" runat="server" onclick="cmdDisagree_Click" 
                                Text="Cancel" />
                        </td>
                    </tr>
                </table>
                <br />
                </asp:Panel>
        <asp:Panel ID="pnlContactAddress" runat="server" 
        Visible="False" BorderStyle="Solid" BorderWidth="1px" ScrollBars="Horizontal" 
            Width="800px" BackImageUrl="#E2E9EF">
            <br />
            <b>Contact details</b><br />
            <table  border="1" cellpadding="1" cellspacing="1" width="100%">
                <tr>
                    <td style="background-color: #B6C6D6; width: 16%">
                        Name</td>
                    <td style="width: 66%">
                        &nbsp;
                        <asp:Label ID="lblName" runat="server" Text="Name"></asp:Label>
                    </td>
                    <td class="gvReviewNonShareableCol4" rowspan="5" style="width: 13%">
                        Please enter your <strong>full billing address</strong>. We need this 
                        information in order to prepare your bill.<br />
                        <br />
                        Please make sure that you select the correct country: this is needed in order to 
                        calculate VAT.<br />
                        <br />
                        Click <strong>Verify/Save</strong> to continue.</td>
                </tr>
                <tr>
                    <td style="background-color: #B6C6D6; width: 16%">
                        Organization (optional)</td>
                    <td style="width: 66%">
                        <asp:TextBox ID="tbOrganization" runat="server" CssClass="textbox" Width="95%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td style="background-color: #B6C6D6; width: 16%">
                        Full Postal address</td>
                    <td style="width: 66%">
                        <asp:TextBox ID="tbPostalAddress" runat="server" CssClass="textbox" 
                            Width="95%" Rows="5" TextMode="MultiLine" Wrap="False"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td style="background-color: #B6C6D6; width: 16%">
                        Country</td>
                    <td style="width: 66%">
                        <asp:DropDownList ID="ddlCountries" runat="server" 
                            DataTextField="COUNTRY_NAME" DataValueField="COUNTRY_ID">
                        </asp:DropDownList>
                    </td>
                </tr>
                <!--<tr style=" height:0px; min-height:0px">
                    <td style="background-color: #99ccff; width: 16%; height:0px; min-height:0px">
                        EU VAT registration number</td>
                    <td style="width: 69%; height:0px; min-height:0px">
                        <asp:TextBox ID="tbEuVatNumber" runat="server">Enter if available</asp:TextBox>
                        &nbsp; for EU countries</td>
                </tr>-->
                <tr>
                    <td style="background-color: #B6C6D6; width: 16%">
                        Email address</td>
                    <td style="width: 66%">
                        &nbsp;
                        <asp:Label ID="lblEmailAddress" runat="server" Text="email"></asp:Label>
                    </td>
                </tr>
            </table>
            <br />
            
            <table style="width:100%;">
                <tr>
                    <td>
                        <b><asp:Button ID="cmdSaveVerify" runat="server" onclick="cmdSaveVerify_Click" 
                            Text="Verify / Save" />
                        &nbsp;&nbsp; &nbsp;<asp:Label ID="lblContactDetails" runat="server" Font-Bold="True" 
                            Text="Please verify or edit your contact details"></asp:Label></b>
                    </td>
                    <td style="text-align: right">
                        <asp:Button ID="cmdCancel" runat="server" onclick="cmdCancel_Click" 
                            Text="Cancel" />
                    </td>
                </tr>
            </table>
            <br />
        </asp:Panel>


        <asp:Panel ID="pnlPurchaseDebit" runat="server" Visible="False">
            <br />
            <b>Outstanding fees:</b> from previous unpaid account and review extensions.
             <br />
            <table style="background-color: #E2E9EF" width="510px">
                <tr>
                    <td colspan="2">
                        <asp:Label ID="Label1" runat="server" BackColor="#FF9966"
                            Text="Label" Visible="False" ForeColor="Black"></asp:Label>
                    </td>
                </tr>

                <tr>
                    <td style="width: 170px;">Fee (GBP) </td>
                    <td><b>£</b>
                        <asp:Label runat="server" ID="lblOutstandingFee" Font-Bold="True" Text="0"></asp:Label>
                        <b>.00</b>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </asp:Panel>



        <asp:Panel ID="pnlPurchaseCredit" runat="server" Visible="False">
            <br />
            <b>Purchase credit:</b> can be applied to any user account or review at any time.
             <br />
            <table style="background-color: #E2E9EF">
                <tr>
                    <td colspan="3">
                        <asp:Label ID="LblAddCreditResult" runat="server" BackColor="#FF9966"
                            Text="Label" Visible="False" ForeColor="Black"></asp:Label>
                    </td>
                </tr>

                <tr>
                    <td rowspan="2" style="width: 170px;">Add credit in £ (GBP)<br />(excluding VAT) </td>
                    <td style="width: 250px;">Enter value in <b>£5</b> increments</td>
                    <td valign="bottom" rowspan="2">
                        <asp:Button ID="BTAddCreditPurchase" runat="server" Text="Add/Update" Enabled="true"
                            OnClick="BTAddCreditPurchase_Click" /></td>
                </tr>
                <tr>

                    <td>
                        <asp:TextBox ID="TBCredit" runat="server" Width="248px"></asp:TextBox></td>
                </tr>
            </table>

        </asp:Panel>


            <asp:Panel ID="pnlPurchasedAccounts" runat="server" Visible="False">
                <b>
                <br />
                Existing purchased accounts&nbsp;&nbsp;&nbsp; </b>
                <asp:LinkButton ID="lbNewAccount" runat="server" onclick="lbNewAccount_Click">Add new account</asp:LinkButton>
                &nbsp;&nbsp; Please note that all dates are dd/mm/yyyy
                <asp:GridView ID="gvPurchasedAccounts" runat="server" 
                    AutoGenerateColumns="False" DataKeyNames="CONTACT_ID">
                    <Columns>
                        <asp:BoundField DataField="CONTACT_ID" HeaderText="Contact ID">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="CONTACT_NAME" HeaderText="Name">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="CREATOR_ID" HeaderText="Purchaser">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="DATE_CREATED" HeaderText="Date created">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="EXPIRY_DATE" HeaderText="Expiry date">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="MONTHS_CREDIT" HeaderText="Months credit" 
                            Visible="False">
                        <HeaderStyle BackColor="#B6C6D6" />
                        <ItemStyle HorizontalAlign="Right" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Extend by">
                            <ItemTemplate>
                                <asp:DropDownList ID="ddlExtendAccount" runat="server" AutoPostBack="True" 
                                    onselectedindexchanged="ddlExtendAccount_SelectedIndexChanged">
                                    <asp:ListItem Value="0">0 months</asp:ListItem>
                                    <asp:ListItem Value="1">1 month</asp:ListItem>
                                    <asp:ListItem Value="2">2 months</asp:ListItem>
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
                                    <asp:ListItem Value="15">15 months</asp:ListItem>
                                    <asp:ListItem Value="18">18 months</asp:ListItem>
                                    <asp:ListItem Value="21">21 months</asp:ListItem>
                                    <asp:ListItem Value="24">24 months</asp:ListItem>
                                </asp:DropDownList>
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="COST" HeaderText="Cost in £">
                        <HeaderStyle BackColor="#B6C6D6" />
                        <ItemStyle HorizontalAlign="Right" />
                        </asp:BoundField>
                    </Columns>
                </asp:GridView>
                
                <table style="background-color:#E2E9EF">
                <tr>
                    <td colspan="3" style="height: 17px">
                        <asp:Label ID="LblAddAccountResult" runat="server" BackColor="#FF9966" 
                            Text="Label" Visible="False" ForeColor="Black"></asp:Label>
                    </td>
                </tr>
                    <tr>
                        <td rowspan="2"  style="width:170px;">Add other existing account&nbsp;</td><td style="width:70px;">Account ID</td><td style="width:170px;">E-Mail</td>
                        <td rowspan="2" valign="bottom">
                            <asp:Button ID="BTAddExistingAccount" runat="server" Text="Verify/Add" Enabled="true"
                                onclick="BTAddExistingAccount_Click" />
                        </td>
                    </tr>
                    <tr>
                        <td style="width:70px;"><asp:TextBox ID="TbxAddAccountID" runat="server"  style="width:68px;"></asp:TextBox></td><td style="width:170px;"><asp:TextBox ID="TbxAddAccountEMail" runat="server"  style="width:168px;"></asp:TextBox></td>
                    </tr>
                </table>
                
                
                <br />
        </asp:Panel>
        
            
            <asp:Panel ID="pnlPurchaseAccount" runat="server" Visible="False">
                <b>New accounts:</b> New, additional accounts that you are about to purchase (if any).
                <asp:GridView ID="gvNewAccounts" runat="server" 
                    AutoGenerateColumns="False" DataKeyNames="Bill_Line_ID" 
                    onrowcommand="gvRemoveGhost_RowCommand" EnableModelValidation="True" 
                    onselectedindexchanged="dllExtendGhostAccount">
                    <Columns>
                        <asp:BoundField DataField="Bill_Line_ID" HeaderText="Bill Line ID">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        
                        <asp:BoundField DataField="CREATOR_ID" HeaderText="Purchaser">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Start date" Visible="False">
                            <ItemTemplate>
                                <asp:DropDownList ID="ddlStartDate" runat="server" AutoPostBack="True" 
                                    onselectedindexchanged="ddlStartDate_SelectedIndexChanged">
                                    <asp:ListItem Value="TODAY">Today</asp:ListItem>
                                    <asp:ListItem Value="FUTURE">Future</asp:ListItem>
                                </asp:DropDownList>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("DATE_CREATED") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="No. months">
                            <ItemTemplate>
                                <asp:DropDownList ID="ddlExtendTmpAccount" runat="server" AutoPostBack="True" 
                                    onselectedindexchanged="dllExtendGhostAccount">
                                    <asp:ListItem Value="0">0 months</asp:ListItem>
                                    <asp:ListItem Value="1">1 month</asp:ListItem>
                                    <asp:ListItem Value="2">2 months</asp:ListItem>
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
                                    <asp:ListItem Value="15">15 months</asp:ListItem>
                                    <asp:ListItem Value="18">18 months</asp:ListItem>
                                    <asp:ListItem Value="21">21 months</asp:ListItem>
                                    <asp:ListItem Value="24">24 months</asp:ListItem>
                                </asp:DropDownList>
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="COST" HeaderText="Cost in £">
                        <HeaderStyle BackColor="#B6C6D6" />
                        <ItemStyle HorizontalAlign="Right" />
                        </asp:BoundField>
                        <asp:ButtonField HeaderText="Remove" Text="Remove" CommandName="REMOVE">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
                <br />
            </asp:Panel>
            
                        
        <asp:Panel ID="pnlExistingReviews" runat="server" Visible="False">
            <b>Existing reviews</b>&nbsp;&nbsp;
            <asp:LinkButton ID="lbNewReview" runat="server" onclick="lbNewReview_Click">Add new review</asp:LinkButton>
            <br />
            This table lists all your reviews. There is no charge for private reviews (ones that only you can access), <br />
            a fee is payable for reviews that are shared between reviewers.
            <br />
            <asp:GridView ID="gvPurchasedReviews" runat="server" 
                AutoGenerateColumns="False" DataKeyNames="REVIEW_ID" 
                CssClass="grviewFixedWidth">
                <Columns>
                    <asp:BoundField DataField="REVIEW_ID" HeaderText="Review ID">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="FUNDER_ID" HeaderText="Purchaser">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="DATE_CREATED" HeaderText="Date created">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="EXPIRY_DATE" HeaderText="Expiry date">
                    <HeaderStyle BackColor="#B6C6D6" />
                    </asp:BoundField>
                    <asp:BoundField DataField="MONTHS_CREDIT" HeaderText="Months credit" 
                        Visible="False">
                    <HeaderStyle BackColor="#B6C6D6" />
                    <ItemStyle HorizontalAlign="Right" />
                    </asp:BoundField>
                    <asp:TemplateField HeaderText="Extend by">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlExtendReview" runat="server" AutoPostBack="True" 
                                onselectedindexchanged="ddlExtendReview_SelectedIndexChanged">
                                <asp:ListItem Value="0">0 months</asp:ListItem>
                                    <asp:ListItem Value="1">1 month</asp:ListItem>
                                    <asp:ListItem Value="2">2 months</asp:ListItem>
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
                                    <asp:ListItem Value="15">15 months</asp:ListItem>
                                    <asp:ListItem Value="18">18 months</asp:ListItem>
                                    <asp:ListItem Value="21">21 months</asp:ListItem>
                                    <asp:ListItem Value="24">24 months</asp:ListItem>
                            </asp:DropDownList>
                        </ItemTemplate>
                        <HeaderStyle BackColor="#B6C6D6" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="COST" HeaderText="Cost in £">
                    <HeaderStyle BackColor="#B6C6D6" />
                    <ItemStyle HorizontalAlign="Right" />
                    </asp:BoundField>
                </Columns>
            </asp:GridView>
            
            
            
            <table  style="background-color:#E2E9EF">
            <tr>
                <td colspan="3" style="height: 18px">
                    <asp:Label ID="LblAddReviewResult" runat="server" BackColor="#FF9966" 
                        Text="Label" Visible="False" ForeColor="Black"></asp:Label>
                </td>
           </tr> 
                
                    <tr>
                        <td rowspan="2" style="width:170px;">Add other existing Review</td><td style="width:60px;">Review ID</td><td style="width:300px;">Review Name</td>
                        <td valign="bottom" rowspan="2">
                            <asp:Button ID="BTAddExistingReview" runat="server" Text="Verify & Add" Enabled="true" 
                                onclick="BTAddExistingReview_Click"  /></td>
                    </tr>
                    <tr>
                        <td style="width:60px;"><asp:TextBox ID="TbxAddReviewID" runat="server" style="width:58px;"></asp:TextBox></td>
                        <td><asp:TextBox ID="TbxAddReviewName" runat="server" Width="298px"></asp:TextBox></td>
                    </tr>
            </table>
            <br />
        </asp:Panel>
        
            <asp:Panel ID="pnlPurchaseReview" runat="server" Visible="False">
                <b>New Reviews:</b> New, shareable reviews that you are about to purchase (if any).
                &nbsp;<asp:GridView ID="gvNewReviews" runat="server" AutoGenerateColumns="False" 
                    DataKeyNames="BILL_LINE_ID" onrowcommand="gvRemoveGhost_RowCommand" 
                    EnableModelValidation="True">
                    <Columns>
                        <asp:BoundField DataField="BILL_LINE_ID" HeaderText="Bill Line ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="CREATOR_ID" HeaderText="Purchaser">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Start date" Visible="False">
                            <ItemTemplate>
                                <asp:DropDownList ID="ddlStartDate1" runat="server" AutoPostBack="True" 
                                    onselectedindexchanged="ddlStartDate1_SelectedIndexChanged">
                                    <asp:ListItem Value="TODAY">Today</asp:ListItem>
                                    <asp:ListItem Value="FUTURE">Future</asp:ListItem>
                                </asp:DropDownList>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("DATE_CREATED") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="No. months">
                            <ItemTemplate>
                                <asp:DropDownList ID="ddlExtendTmpReview" runat="server" AutoPostBack="True" 
                                    onselectedindexchanged="dllExtendGhostAccount">
                                    <asp:ListItem Value="0">0 months</asp:ListItem>
                                    <asp:ListItem Value="1">1 month</asp:ListItem>
                                    <asp:ListItem Value="2">2 months</asp:ListItem>
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
                                    <asp:ListItem Value="15">15 months</asp:ListItem>
                                    <asp:ListItem Value="18">18 months</asp:ListItem>
                                    <asp:ListItem Value="21">21 months</asp:ListItem>
                                    <asp:ListItem Value="24">24 months</asp:ListItem>
                                </asp:DropDownList>
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="COST" HeaderText="Cost in £">
                            <HeaderStyle BackColor="#B6C6D6" />
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:BoundField>
                        <asp:ButtonField CommandName="REMOVE" HeaderText="Remove" Text="Remove">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
                <br />
            </asp:Panel>



            <asp:Panel ID="pnlTotals" runat="server" Visible="False">
                <table style="width:90%;">
                    <tr>
                        <td width="25%" style="height: 16px">
                            Account fees</td>
                        <td width="15%" style="height: 16px; text-align: right;">
                            <asp:Label ID="lblAccountFees" runat="server" Text="£0"></asp:Label>
                        </td>
                        <td width="2%" style="height: 16px">
                            &nbsp;</td>
                        <td width="18%" style="height: 16px">
                            &nbsp;</td>
                        <td width="30">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="25%" style="height: 17px">
                            Review fees</td>
                        <td width="15%" style="text-align: right; height: 17px;">
                            <asp:Label ID="lblReviewFees" runat="server" Text="£0"></asp:Label>
                        </td>
                        <td width="2%" style="height: 17px">
                            </td>
                        <td width="18%" style="height: 17px">
                            </td>
                        <td width="30">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="25%" style="height: 17px">
                            Credit fees</td>
                        <td width="15%" style="text-align: right; height: 17px;">
                            <asp:Label ID="lblCreditfee" runat="server" Text="£0"></asp:Label>
                        </td>
                        <td width="2%" style="height: 17px">
                            </td>
                        <td width="18%" style="height: 17px">
                            </td>
                        <td width="30">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="25%" style="height: 17px">
                            Outstanding fees</td>
                        <td width="15%" style="text-align: right; height: 17px;">
                            <asp:Label ID="lblTotalOutstandingFee" runat="server" Text="£0"></asp:Label>
                        </td>
                        <td width="2%" style="height: 17px">
                            </td>
                        <td width="18%" style="height: 17px">
                            </td>
                        <td width="30">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="25%">
                            Nominal fee</td>
                        <td width="15%" style="text-align: right;">
                            <asp:Label ID="lblNominalFee" runat="server" Text="£0"></asp:Label>
                        </td>
                        <td width="2%">
                            &nbsp;</td>
                        <td width="18%">
                            &nbsp;</td>
                        <td width="30">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="25%">
                            <asp:Label ID="lblVatMessage" runat="server" Text="VAT  tax" Visible="False"></asp:Label>
                            </td>
                        <td width="15%" style="text-align: right;">
                            <asp:Label ID="lblVatTotal" runat="server" Text="£0" Visible="False"></asp:Label>
                        </td>
                        <td width="2%">
                            &nbsp;</td>
                        <td width="18%">
                            <asp:Label ID="lblVatPercentage" runat="server" Text="0%" Visible="False"></asp:Label>
                        </td>
                        <td width="30">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td bgcolor="#B6C6D6" width="25%">
                            Total fee</td>
                        <td width="15%" style="text-align: right;">
                            <asp:Label ID="lblTotalFees" runat="server" Font-Bold="True" Text="£0"></asp:Label>
                        </td>
                        <td width="2%">
                            &nbsp;</td>
                        <td width="18%">
                            <asp:Button ID="cmdPurchase" runat="server" onclick="cmdPurchase_Click" 
                                Text="Purchase" Enabled="False" />
                        </td>
                        <td width="30">
                            <asp:Label ID="lblMinAmount" runat="server" Text="Minimum purchase is £30" 
                                style="font-weight: 700"></asp:Label>
                        </td>
                    </tr>
                </table>
    </asp:Panel>
            <br />
            
            <br />
            <br />
            

        
            </div>
</asp:Content>
