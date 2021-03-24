<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="SummaryReviews.aspx.cs" Inherits="SummaryReviews" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="Telerik" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <telerik:RadWindowManager ID="RadWindowManager1" runat="server" EnableShadow="true">
    </telerik:RadWindowManager>



    


        <b>Shareable reviews you have purchased&nbsp;or have administrative rights to</b><asp:GridView
            ID="gvReview" runat="server" CssClass="grviewFixedWidth"
            AutoGenerateColumns="False" Width="800px" OnRowCommand="gvReview_RowCommand" 
            DataKeyNames="REVIEW_ID" OnPageIndexChanging="gvReview_PageIndexChanging" PageSize="15" AllowPaging="True" OnRowDataBound="gvReview_RowDataBound">
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
            <PagerSettings Mode="Numeric" Position="TopAndBottom" />
            <PagerStyle Font-Bold="False" Font-Size="Larger" HorizontalAlign="Center" />
        </asp:GridView>

        <asp:Label ID="lblShareableReviews" runat="server"
            Text="You have not purchased any shareable reviews." Visible="False"></asp:Label>
        <br />
        <asp:Panel ID="pnlEditShareableReview" runat="server" Visible="False">


            <asp:Panel ID="pnlReviewDetails" runat="server" BorderStyle="Solid"
                BorderWidth="1px" BackColor="#E2E9EF">
                <table id="Table5" border="1" cellpadding="1" cellspacing="1" width="600">
                    <tr>
                        <td style="background-color: #B6C6D6; width: 15%">Review #</td>
                        <td style="width: 85%">
                            <asp:Label ID="lblShareableReviewNumber" runat="server" Text="Number"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color: #B6C6D6; width: 15%">Review title</td>
                        <td style="width: 85%">
                            <asp:TextBox ID="tbShareableReviewName" runat="server" CssClass="textbox"
                                MaxLength="1000" Width="98%" EnableViewState="False"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                <table style="width: 100%;">
                    <tr>
                        <td style="width: 40%; height: 25px;">
                            <asp:Button ID="cmdSaveShareableReview" runat="server" OnClick="cmdSaveShareableReview_Click" Text="Save" />
                            &nbsp;&nbsp;
                                    <asp:LinkButton ID="lbCancelReviewDetailsEdit" runat="server" OnClick="lbCancelReviewDetailsEdit_Click">Cancel/close</asp:LinkButton>
                        </td>
                        <td style="text-align: right; font-weight: bold; height: 25px;">
                            <asp:Label ID="lblPSShareableReview" runat="server" Text="Priority screening" Visible="False"></asp:Label>
                        </td>
                        <td style="text-align: left; width: 40%; height: 25px;">
                            <asp:RadioButtonList ID="rblPSShareableEnable" runat="server" AutoPostBack="True" OnSelectedIndexChanged="rblPSShareableEnable_SelectedIndexChanged" RepeatDirection="Horizontal" Visible="False">
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
                            <asp:LinkButton ID="lbBritLibCodesShared" runat="server" OnClick="lbBritLibCodesShared_Click">BL codes</asp:LinkButton>
                        </td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                </table>

                <asp:Panel ID="pnlBritLibCodesShared" runat="server" BorderColor="#999999" BorderStyle="Solid" BorderWidth="2px" Visible="False" Width="792px">
                    If you have document ordering codes from the <b>British Library</b> you can enter them here.
                            <asp:LinkButton ID="lbCancelBLShared" runat="server" OnClick="lbCancelBLShared_Click">(cancel)</asp:LinkButton>
                    <br />
                    <table style="width: 100%;">
                        <tr>
                            <td colspan="2">Library privilege codes&nbsp;&nbsp;
                                        <asp:LinkButton ID="lbSaveBritLibLPCodesShared" runat="server" OnClick="lbSaveBritLibLPCodesShared_Click">Save</asp:LinkButton>
                            </td>
                            <td style="width: 25%">Copyright cleared codes&nbsp;&nbsp;
                                        <asp:LinkButton ID="lbSaveBritLibCCCodesShared" runat="server" OnClick="lbSaveBritLibCCCodesShared_Click">Save</asp:LinkButton>
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
                        OnClick="lbInviteReviewer_Click">Send invitation</asp:LinkButton>
                <asp:GridView ID="gvMembersOfReview" runat="server" AutoGenerateColumns="False"
                    Width="800px" DataKeyNames="CONTACT_ID" CssClass="grviewFixedWidth"
                    OnRowCommand="gvMembersOfReview_RowCommand"
                    OnRowDataBound="gvMembersOfReview_RowDataBound"
                    EnableModelValidation="True">
                    <Columns>
                        <asp:BoundField DataField="CONTACT_ID" HeaderText="Contact ID">
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="CONTACT_NAME" HeaderText="Reviewer (expiry date)">
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
                                    OnCheckedChanged="cbCodingOnly_CheckedChanged" />
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Read only">
                            <ItemTemplate>
                                <asp:CheckBox ID="cbReadOnly" runat="server" AutoPostBack="True"
                                    OnCheckedChanged="cbReadOnly_CheckedChanged" />
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Review admin">
                            <ItemTemplate>
                                <asp:CheckBox ID="cbReviewAdmin" runat="server" AutoPostBack="True"
                                    OnCheckedChanged="cbReviewAdmin_CheckedChanged" />
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
                            OnClick="cmdInvite_Click" />
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
            AutoGenerateColumns="False" Width="800px" CssClass="grviewFixedWidth"
            OnRowCommand="gvReviewNonShareable_RowCommand"
            OnRowEditing="gvReviewNonShareable_RowEditing" DataKeyNames="REVIEW_ID" PageSize="15" AllowPaging="True" OnPageIndexChanging="gvReviewNonShareable_PageIndexChanging">
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
            <PagerSettings Position="TopAndBottom" />
            <PagerStyle Font-Size="Larger" HorizontalAlign="Center" />
        </asp:GridView>
        <asp:Label ID="lblNonShareableReviews" runat="server"
            Text="You do not own any non-shareable reviews." Visible="False"></asp:Label>
        <br />
        <asp:Panel ID="pnlEditNonShareableReview" runat="server" Visible="False"
            BackColor="#E2E9EF" BorderStyle="Solid" BorderWidth="1px">
            <table id="Table4" border="1" cellpadding="1" cellspacing="1" width="600">
                <tr>
                    <td style="background-color: #B6C6D6; width: 15%">Review #</td>
                    <td style="width: 60%">
                        <asp:Label ID="lblNonShareableReviewNumber" runat="server" Text="Number"></asp:Label>
                    </td>
                    <td align="center" style="width: 15%">
                        <asp:LinkButton ID="lbDeleteReview" runat="server">Delete</asp:LinkButton>
                    </td>
                </tr>
                <tr>
                    <td style="background-color: #B6C6D6; width: 15%">Review title</td>
                    <td style="width: 85%" colspan="2">
                        <asp:TextBox ID="tbReviewName" runat="server" CssClass="textbox" MaxLength="1000"
                            Width="98%" EnableViewState="False"></asp:TextBox>
                    </td>
                </tr>
            </table>
            <table style="width: 100%;">
                <tr>
                    <td style="width: 40%">
                        <asp:Button ID="cmdSaveNonShareableReview" runat="server" OnClick="cmdSaveNonShareableReview_Click" Text="Save" />
                        &nbsp;&nbsp;
                                    <asp:LinkButton ID="lbCancelNSReviewDetailsEdit" runat="server" OnClick="lbCancelNSReviewDetailsEdit_Click">Cancel/close</asp:LinkButton>
                    </td>
                    <td style="text-align: right; font-weight: bold;">
                        <asp:Label ID="lblPSNonShareableEnable" runat="server" Text="Priority screening" Visible="False"></asp:Label>
                    </td>
                    <td style="text-align: left; width: 40%;">
                        <asp:RadioButtonList ID="rblPSNonShareableEnable" runat="server" AutoPostBack="True" OnSelectedIndexChanged="rblPSNonShareableEnable_SelectedIndexChanged" RepeatDirection="Horizontal" Visible="False">
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
                        <asp:LinkButton ID="lbBritLibCodesNonShared" runat="server" OnClick="lbBritLibCodesNonShared_Click">BL codes</asp:LinkButton>
                        &nbsp;&nbsp;&nbsp;
                                    <asp:LinkButton ID="lbChangeToShareable" runat="server" OnClick="lbChangeToShareable_Click" Visible="False">Change to a shareable review</asp:LinkButton>
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
                            OnClick="lbCancelBLNonShared_Click">(cancel)</asp:LinkButton>
                <br />
                <table style="width: 100%;">
                    <tr>
                        <td colspan="2">Library privilege codes&nbsp;&nbsp;
                                    <asp:LinkButton ID="lbSaveBritLibLPCodesNonShared" runat="server"
                                        OnClick="lbSaveBritLibLPCodesNonShared_Click">Save</asp:LinkButton>
                        </td>
                        <td style="width: 25%">Copyright cleared codes&nbsp;&nbsp;
                                    <asp:LinkButton ID="lbSaveBritLibCCCodesNonShared" runat="server"
                                        OnClick="lbSaveBritLibCCCodesNonShared_Click">Save</asp:LinkButton>
                        </td>
                        <td style="width: 25%">&nbsp;</td>
                    </tr>
                    <tr>
                        <td style="width: 25%">
                            <asp:TextBox ID="tbLPC_ACC_NonShare" runat="server" Width="90%"></asp:TextBox>
                        </td>
                        <td style="width: 25%">Account code</td>
                        <td style="width: 25%">
                            <asp:TextBox ID="tbCCC_ACC_NonShare" runat="server" Width="90%"></asp:TextBox>
                        </td>
                        <td style="width: 25%">Account code</td>
                    </tr>
                    <tr>
                        <td>
                            <asp:TextBox ID="tbLPC_AUT_NonShare" runat="server" Width="90%"></asp:TextBox>
                        </td>
                        <td>Authorisation code</td>
                        <td>
                            <asp:TextBox ID="tbCCC_AUT_NonShare" runat="server" Width="90%"></asp:TextBox>
                        </td>
                        <td>Authorisation code</td>
                    </tr>
                    <tr>
                        <td>
                            <asp:TextBox ID="tbLPC_TX_NonShare" runat="server" Width="90%"></asp:TextBox>
                        </td>
                        <td>TX line to export</td>
                        <td>
                            <asp:TextBox ID="tbCCC_TX_NonShare" runat="server" Width="90%"></asp:TextBox>
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

            <asp:Panel ID="pnlChangeToShareable" runat="server" Visible="False">
                Making the review shareable is not available at this time.<br />
            </asp:Panel>

        </asp:Panel>

        <br />
        <b>Other shareable reviews you are a member of</b><br />
        <asp:GridView
            ID="gvReviewShareableMember" runat="server" AutoGenerateColumns="False"
            OnRowCommand="gvReviewShareableMember_RowCommand"
            DataKeyNames="REVIEW_ID" CssClass="grviewFixedWidth"
            OnRowDataBound="gvReviewShareableMember_RowDataBound" PageSize="5" AllowPaging="True" OnPageIndexChanging="gvReviewShareableMember_PageIndexChanging1">
            <Columns>
                <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField HeaderText="Review owner" DataField="REVIEW_OWNER">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField DataField="DATE_CREATED" HeaderText="Date review created">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:BoundField DataField="LAST_LOGIN" HeaderText="Last login by this reviewer">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:BoundField>
                <asp:ButtonField CommandName="REMOVE" HeaderText="Remove from review"
                    Text="Remove">
                    <HeaderStyle BackColor="#B6C6D6" />
                </asp:ButtonField>
            </Columns>
            <PagerSettings Position="TopAndBottom" />
            <PagerStyle Font-Size="Larger" HorizontalAlign="Center" />
        </asp:GridView>

        <asp:Label ID="lblNonShareableReviewsMember" runat="server"
            Text="You are not a member of any other shareable reviews."
            Visible="False"></asp:Label>
        <br />



</asp:Content>

