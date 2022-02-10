<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="AssignCredit.aspx.cs" Inherits="AssignCredit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">




    <script type="text/javascript">

        function DisableControls() {
            var el = document.getElementById('<%= pnlToHide.ClientID %>');
            if (el) {
                el.style.display = "none";
            }
            var el = document.getElementById('jsSavingDiv');
            if (el) {
                el.style.display = "block";
            }
        }

        


    </script>

    <asp:Panel ID="pnlInstructions" runat="server" Visible="true">
        <table style="width: 100%;">
            <tr>
                <td colspan="2">Select a review to allocate credit towards the review and/or selected members of the review.
                </td>
            </tr>
            <tr>
                <td>Purchased credit:
                    <asp:Label ID="lblPurchasedCredit" runat="server" Text="£0" Font-Bold="True"></asp:Label>
                </td>
                <td>Remaining credit:
                    <asp:Label ID="lblRemainingCredit" runat="server" Text="£0" Font-Bold="True"></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <br />
                    To extend a review not listed click
                    <asp:LinkButton ID="lbReviewManual" runat="server" OnClick="lbReviewManual_Click">here</asp:LinkButton>.
                    <br />
                    To extend an account not in these reviews click
                    <asp:LinkButton ID="lbAccountManual" runat="server" OnClick="lbAccountManual_Click">here</asp:LinkButton>.
                </td>
            </tr>
        </table>
    </asp:Panel>

<div id="jsSavingDiv" style="display:none;padding: 1em; margin: 1em; border-collapse:collapse; background-color:#B6C6D6; border: 1px solid black;">
                Saving...
            </div>

<asp:Panel ID="pnlToHide" runat="server">
    <asp:Panel ID="pnlAssign" runat="server" BackColor="#E2E9EF" Visible="false">
        <table style="border-style: solid solid none solid; border-width: medium; width: 100%;">
            <tr>
                <td>Select the number of months for extending the review and/or account(s).<br />
                    When finished click the <b>Complete</b> button.
                </td>
                <td>Available
                </td>
                <td>Total
                </td>
                <td>Remaining
                </td>
            </tr>

            <tr>
                <td>
                    <asp:Button ID="cmdComplete" runat="server" Text="Complete" 
                        OnClick="cmdComplete_Click" Enabled="False" onClientClick="javascript:DisableControls();" UseSubmitBehavior="false" />
                    &nbsp;&nbsp; 
                    <asp:LinkButton ID="lbCancel" runat="server" OnClick="lbCancel_Click">(cancel)</asp:LinkButton>
                    &nbsp;&nbsp;
                    <asp:Label runat="server" ID="lblExtensionError" Visible="false" Font-Bold="True" ForeColor="Red" Text="Missing extension"></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblAvailable" runat="server" Text="£0" Font-Bold="True"></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblTotal" runat="server" Text="£0" Font-Bold="True"></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblRemaining" runat="server" Text="£0" Font-Bold="True"></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <hr />
                </td>
            </tr>
        </table>
    </asp:Panel>

    <asp:Panel ID="pnlReviewManual" runat="server" Visible="false">
        <table>
            <tr>
                <td rowspan="2" style="width: 170px;">Find the review&nbsp;</td>
                <td style="width: 70px;">Review ID</td>
                <td rowspan="2" valign="bottom">
                    <asp:Button ID="cmdAddExistingReview" runat="server" Text="Find" Enabled="true"
                        OnClick="cmdAddExistingReview_Click" />
                    &nbsp;&nbsp;
                    <asp:Label runat="server" ID="lblReviewMsg" Visible="false" Font-Bold="True" ForeColor="Red" Text="No review found"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="width: 70px;">
                    <asp:TextBox ID="tbReviewID" runat="server" Style="width: 68px;"></asp:TextBox>
                </td>
            </tr>
        </table>


    </asp:Panel>

    <asp:Panel ID="pnlAccountManual" runat="server" Visible="false">
        <table>
            <tr>
                <td rowspan="2" style="width: 170px;">Find the user account&nbsp;</td>
                <td style="width: 70px;">Account ID</td>
                <td style="width: 170px;">E-Mail</td>
                <td rowspan="2" valign="bottom">
                    <asp:Button ID="cmdAddExistingAccount" runat="server" Text="Find" Enabled="true"
                        OnClick="cmdAddExistingAccount_Click" />
                    &nbsp;&nbsp;
                    <asp:Label runat="server" ID="lblAccountMsg" Visible="false" Font-Bold="True" ForeColor="Red" Text="No account found"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="width: 70px;">
                    <asp:TextBox ID="tbAddAccountID" runat="server" Style="width: 68px;"></asp:TextBox>
                </td>
                <td style="width: 170px;">
                    <asp:TextBox ID="tbAddAccountEmail" runat="server" Style="width: 168px;"></asp:TextBox></td>
            </tr>
        </table>
    </asp:Panel>



    <asp:Panel ID="pnlReviewDetails" runat="server" BackColor="#E2E9EF" Visible="false">
        <table style="border-style: none solid none solid; border-width: medium; width: 100%;">
            <tr>
                <td style="height: 18px">Review:
                                <asp:Label ID="lblReviewID" runat="server" Text="0" Visible="false"></asp:Label>
                </td>
                <td style="height: 18px">Expiry date:
                </td>
                <td style="height: 18px">Extension:
                </td>
                <td style="height: 18px">Cost:
                </td>
            </tr>

            <tr>
                <td>
                    <asp:Label ID="lblReviewName" runat="server" Text="Label" Font-Bold="True"></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblExpiryDate" runat="server" Text="Label"></asp:Label>
                </td>
                <td>
                    <asp:DropDownList ID="ddlExtendReview" runat="server" AutoPostBack="True"
                        OnSelectedIndexChanged="ddlExtendReview_SelectedIndexChanged" Font-Bold="False">
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
                </td>
                <td>
                    <asp:Label ID="lblReviewCost" runat="server" Text="£0"></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <br />
                </td>
            </tr>
        </table>
    </asp:Panel>

    <asp:Panel ID="pnlReviewMembers" runat="server" BackColor="#E2E9EF" Visible="false">
        <table style="border-style: none solid solid solid; border-width: medium; width: 100%;">
            <tr>
                <td>
                    <asp:GridView ID="gvMembersOfReview" runat="server"
                        AutoGenerateColumns="False" DataKeyNames="CONTACT_ID" Width="100%">
                        <Columns>
                            <asp:BoundField DataField="CONTACT_ID" HeaderText="Contact ID">
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:BoundField>
                            <asp:BoundField DataField="CONTACT_NAME" HeaderText="Name">
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
                                        OnSelectedIndexChanged="ddlExtendAccount_SelectedIndexChanged">
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
                            <asp:BoundField DataField="COST" HeaderText="Cost (£)">
                                <HeaderStyle BackColor="#B6C6D6" />
                                <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                        </Columns>
                    </asp:GridView>
                    <br />
                </td>
            </tr>
        </table>
    </asp:Panel>

    <asp:Panel ID="pnlBottom" runat="server" BackColor="#E2E9EF" Visible="false">
        <table style="border-style: solid none none none; border-width: medium; width: 100%;">
            <tr>
                <td></td>
            </tr>
        </table>
    </asp:Panel>
    <br />

</asp:Panel>


    <b>Reviews you have adminstrative rights to</b>
    <asp:GridView
        ID="gvReview" runat="server" CssClass="grviewFixedWidth"
        AutoGenerateColumns="False" Width="800px" OnRowCommand="gvReview_RowCommand"
        DataKeyNames="REVIEW_ID">
        <Columns>
            <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                <HeaderStyle BackColor="#B6C6D6" />
            </asp:BoundField>
            <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                <HeaderStyle BackColor="#B6C6D6" />
            </asp:BoundField>
            <asp:BoundField DataField="EXPIRY_DATE" HeaderText="Expiry date">
                <HeaderStyle BackColor="#B6C6D6" />
            </asp:BoundField>
            <asp:ButtonField CommandName="EDT" HeaderText="More details" Text="details" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                <HeaderStyle BackColor="#B6C6D6" HorizontalAlign="Center" />
            </asp:ButtonField>
        </Columns>
    </asp:GridView>
    <br />
    <br />
</asp:Content>

