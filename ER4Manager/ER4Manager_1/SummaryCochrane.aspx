<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="SummaryCochrane.aspx.cs" Inherits="SummaryCochrane" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="Telerik" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css" integrity="sha384-Gn5384xqQ1aoWXA+058RXPxPg6fy4IWvTNh0E263XmFcJlSAwiGgFAW/dAiS6JXm" crossorigin="anonymous"/>
    <script type="text/javascript" src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js" integrity="sha384-ApNbgh9B+Y1QKtv3Rn7W3mgPxhU9K/ScQsAP7hUibX39j7fakFPskvXusvfa0b4Q" crossorigin="anonymous"></script>
    <script type="text/javascript" src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" integrity="sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl" crossorigin="anonymous"></script>

    <script type="text/javascript">


    function DisableControls() {
        var el = document.getElementById('<%= pnlReviewDetailsCochrane.ClientID %>'); 
        if (el) {
            el.style.display = "none";
            var el2 = document.getElementById('jsSavingDiv');
            if (el2) {
                el2.style.display = "block";
            } 
        }
        var el3 = document.getElementById('<%= pnlReviewDetailsCochraneFull.ClientID %>');
        if (el3) {
            el3.style.display = "none";
            var el4 = document.getElementById('jsSavingDiv2');
            if (el4) {
                el4.style.display = "block";
            }
        }
    }

    </script>


    <telerik:RadWindowManager ID="RadWindowManager1" runat="server" EnableShadow="true">
    </telerik:RadWindowManager>

        <asp:Panel ID="pnlCochraneReviews" runat="server" Visible="True">
            &nbsp;<br />
            <b>Prospective Cochrane reviews</b><asp:GridView ID="gvReviewCochrane" runat="server"
                AutoGenerateColumns="False" CssClass="grviewFixedWidth"
                DataKeyNames="REVIEW_ID" OnRowCommand="gvReviewCochrane_RowCommand" Width="800px"
                EnableModelValidation="True">
                <Columns>
                    <asp:BoundField DataField="REVIEW_ID" HeaderText="ID">
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
                <div id="jsSavingDiv" style="display:none;padding: 1em; margin: 1em; border-collapse:collapse; background-color:#B6C6D6; border: 1px solid black;">
                    Saving...
                </div>
                <asp:Panel ID="pnlReviewDetailsCochrane" runat="server" BackColor="#E2E9EF"
                    BorderStyle="Solid" BorderWidth="1px">
                    <table id="Table6" border="1" cellpadding="1" cellspacing="1" width="600">
                        <tr>
                            <td style="background-color: #B6C6D6; width: 15%; height: 18px;">Review #</td>
                            <td style="width: 85%; height: 18px;">
                                <asp:Label ID="lblShareableReviewNumberCochrane" runat="server" Text="Number"></asp:Label>
                                &nbsp;<asp:Label ID="lblFID" runat="server" Text="Label" Visible="False"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 15%">Review title</td>
                            <td style="width: 85%">
                                <asp:TextBox ID="tbShareableReviewNameCochrane" runat="server" CssClass="textbox"
                                    EnableViewState="False" MaxLength="1000" Width="98%"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                    <table style="width: 100%;">
                        <tr>
                            <td style="width: 40%">
                                <asp:Button ID="cmdSaveShareableReviewCochrane" runat="server" OnClick="cmdSaveShareableReviewCochrane_Click" Text="Save" />
                                &nbsp;&nbsp;
                            <asp:LinkButton ID="lbCancelReviewDetailsEdit0" runat="server" OnClick="lbCancelReviewDetailsEdit0_Click">Cancel/close</asp:LinkButton>
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
                        <!--<tr>
                            <td>
                                <asp:LinkButton ID="lbBritLibCodesProCochrane" runat="server" OnClick="lbBritLibCodesProCochrane_Click">BL codes</asp:LinkButton>
                                &nbsp;&nbsp;&nbsp; </td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>-->
                    </table>

                    <asp:Panel ID="pnlBritLibCodesProCochrane" runat="server" BorderColor="#999999" BorderStyle="Solid" BorderWidth="2px" Visible="False" Width="792px">
                        If you have document ordering codes from the <b>British Library</b> you can enter them here.
                    <asp:LinkButton ID="lbCancelBLProCochrane" runat="server" OnClick="lbCancelBLProCochrane_Click">(cancel)</asp:LinkButton>
                        <br />
                        <table style="width: 100%;">
                            <tr>
                                <td colspan="2">Library privilege codes&nbsp;&nbsp;
                                <asp:LinkButton ID="lbSaveBritLibLPCodesProCochrane" runat="server" OnClick="lbSaveBritLibLPCodesProCochrane_Click">Save</asp:LinkButton>
                                </td>
                                <td style="width: 25%">Copyright cleared codes&nbsp;&nbsp;
                                <asp:LinkButton ID="lbSaveBritLibCCCodesProCochrane" runat="server" OnClick="lbSaveBritLibCCCodesProCochrane_Click">Save</asp:LinkButton>
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
                    OnClick="lbInviteReviewerCochrane_Click" Visible="False">Send invitation</asp:LinkButton>
                    <asp:GridView ID="gvMembersOfReviewCochrane" runat="server"
                        AutoGenerateColumns="False" CssClass="grviewFixedWidth"
                        DataKeyNames="CONTACT_ID" EnableModelValidation="True"
                        OnRowCommand="gvMembersOfReviewCochrane_RowCommand"
                        OnRowDataBound="gvMembersOfReviewCochrane_RowDataBound" Width="800px">
                        <Columns>
                            <asp:BoundField DataField="CONTACT_ID" HeaderText="ID">
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
                                        OnCheckedChanged="cbCodingOnlyCochrane_CheckedChanged" />
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Read only">
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbReadOnlyCochrane" runat="server" AutoPostBack="True"
                                        OnCheckedChanged="cbReadOnlyCochrane_CheckedChanged" />
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Review admin">
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbReviewAdminCochrane" runat="server" AutoPostBack="True"
                                        OnCheckedChanged="cbReviewAdminCochrane_CheckedChanged" />
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
                    <asp:Button ID="cmdInviteCochrane" runat="server" OnClick="cmdInviteCochrane_Click"
                        Text="Invite" />
                        &nbsp;
                    <asp:Label ID="lblInviteMsgCochrane" runat="server" Font-Bold="True" ForeColor="Red"
                        Text="Invalid account" Visible="False"></asp:Label>
                        &nbsp;<br />
                        Enter a users email address and select <b>Invite</b>.
                    <br />
                        If the account is valid it will be placed in the review and an email send to the 
                    account holder.
                    </asp:Panel>


                    <table style="width: 100%;">
                        <tr>
                            <td style="width: 100%;">
                                <panel ID="pnlGPTcredit" runat="server" visible="false">
                                    <table style="width: 100%;">
                                        <tr>
                                            <td style="width: 40%; vertical-align:top;border: 1px;padding:3px">
                                                <b>LLM Coding access</b><br />
                                                <asp:DropDownList ID="ddlCreditPurchasesProspC" runat="server" 
                                                    DataTextField="CREDIT_ID_REMAINING" DataValueField="CREDIT_PURCHASE_ID" 
                                                    Enabled="True" AutoPostBack="True" OnSelectedIndexChanged="ddlCreditPurchasesProspC_SelectedIndexChanged">
                                                </asp:DropDownList><br />
                                                If listed, select a PurchaseID from the dropdown menu.<br />
                                                To use LLM Coding you must apply credit to your review.<br />
                                                Credit can be purchased in the Purchase tab.
                                            </td>
                                            <td style="width: 60%; vertical-align:top;border: 1px;padding:3px">
                                                <b><asp:Label ID="lblChatGPTCreditTableHeading" runat="server" Text="LLM Coding Credit" Visible="False"></asp:Label></b>
                                                <br />
                                                <asp:GridView ID="gvCreditForRobotsProspC" runat="server" Width="100%" onrowdatabound="gvCreditForRobotsProspC_RowDataBound"
                                                    onrowcommand="gvCreditForRobotsProspC_RowCommand" AutoGenerateColumns="False" EnableModelValidation="True" 
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
                                </panel>
                            </td>
                        </tr>
                    </table>

                </asp:Panel>
            </asp:Panel>
            <asp:Label ID="lblShareableReviewsCochrane" runat="server"
                Text="You are not in any prospective Cochrane reviews in EPPI-Reviewer." Visible="False"></asp:Label>
            <br />
        </asp:Panel>
        <asp:Panel ID="pnlCochraneReviewsFull" runat="server" Visible="True">
            &nbsp;<br />
            <b>Full Cochrane reviews</b><asp:GridView
                ID="gvReviewCochraneFull" runat="server"
                AutoGenerateColumns="False" CssClass="grviewFixedWidth"
                DataKeyNames="REVIEW_ID" OnRowCommand="gvReviewCochraneFull_RowCommand" Width="800px"
                EnableModelValidation="True">
                <Columns>
                    <asp:BoundField DataField="REVIEW_ID" HeaderText="ID">
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
                <div id="jsSavingDiv2" style="display:none;padding: 1em; margin: 1em; border-collapse:collapse; background-color:#B6C6D6; border: 1px solid black;">
                    Saving...
                </div>
                <asp:Panel ID="pnlReviewDetailsCochraneFull" runat="server" BackColor="#E2E9EF"
                    BorderStyle="Solid" BorderWidth="1px">
                    <table id="Table7" border="1" cellpadding="1" cellspacing="1" width="600">
                        <tr>
                            <td style="background-color: #B6C6D6; width: 15%; height: 18px;">Review #</td>
                            <td style="width: 85%; height: 18px;">
                                <asp:Label ID="lblShareableReviewNumberCochraneFull" runat="server"
                                    Text="Number"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 15%">Review title</td>
                            <td style="width: 85%">
                                <asp:TextBox ID="tbShareableReviewNameCochraneFull" runat="server" CssClass="textbox"
                                    EnableViewState="False" MaxLength="1000" Width="98%"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                    <table style="width: 100%;">
                        <tr>
                            <td style="width: 40%">
                                <asp:Button ID="cmdSaveShareableReviewCochraneFull" runat="server" OnClick="cmdSaveShareableReviewCochraneFull_Click" Text="Save" />
                                &nbsp;&nbsp;
                            <asp:LinkButton ID="lbCancelReviewDetailsEdit2" runat="server" OnClick="lbCancelReviewDetailsEdit2_Click">Cancel/close</asp:LinkButton>
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
                                <!--<asp:LinkButton ID="lbBritLibCodesFullCochrane" runat="server" OnClick="lbBritLibCodesFullCochrane_Click">BL codes</asp:LinkButton>-->
                                &nbsp;&nbsp;&nbsp; </td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                    <asp:Panel ID="pnlBritLibCodesFullCochrane" runat="server" BorderColor="#999999" BorderStyle="Solid" BorderWidth="2px" Visible="False" Width="800px">
                        If you have document ordering codes from the <b>British Library</b> you can enter them here.
                    <asp:LinkButton ID="lbCancelBLFullCochrane" runat="server" OnClick="lbCancelBLFullCochrane_Click">(cancel)</asp:LinkButton>
                        <br />
                        <table style="width: 100%;">
                            <tr>
                                <td colspan="2">Library privilege codes&nbsp;&nbsp;
                                <asp:LinkButton ID="lbSaveBritLibLPCodesFullCochrane" runat="server" OnClick="lbSaveBritLibLPCodesFullCochrane_Click">Save</asp:LinkButton>
                                </td>
                                <td style="width: 25%">Copyright cleared codes&nbsp;&nbsp;
                                <asp:LinkButton ID="lbSaveBritLibCCCodesFullCochrane" runat="server" OnClick="lbSaveBritLibCCCodesFullCochrane_Click">Save</asp:LinkButton>
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
                    OnClick="lbInviteReviewerCochraneFull_Click" Visible="False">Send invitation</asp:LinkButton>
                    <asp:GridView ID="gvMembersOfReviewCochraneFull" runat="server"
                        AutoGenerateColumns="False" CssClass="grviewFixedWidth"
                        DataKeyNames="CONTACT_ID" EnableModelValidation="True"
                        OnRowCommand="gvMembersOfReviewCochraneFull_RowCommand"
                        OnRowDataBound="gvMembersOfReviewCochraneFull_RowDataBound" Width="800px">
                        <Columns>
                            <asp:BoundField DataField="CONTACT_ID" HeaderText="ID">
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
                                        OnCheckedChanged="cbCodingOnlyCochraneFull_CheckedChanged" />
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Read only">
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbReadOnlyCochraneFull" runat="server" AutoPostBack="True"
                                        OnCheckedChanged="cbReadOnlyCochraneFull_CheckedChanged" />
                                </ItemTemplate>
                                <HeaderStyle BackColor="#B6C6D6" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Review admin">
                                <ItemTemplate>
                                    <asp:CheckBox ID="cbReviewAdminCochraneFull" runat="server" AutoPostBack="True"
                                        OnCheckedChanged="cbReviewAdminCochraneFull_CheckedChanged" />
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
                    <asp:Button ID="cmdInviteCochraneFull" runat="server" OnClick="cmdInviteCochraneFull_Click"
                        Text="Invite" />
                        &nbsp;
                    <asp:Label ID="lblInviteMsgCochraneFull" runat="server" Font-Bold="True" ForeColor="Red"
                        Text="Invalid account" Visible="False"></asp:Label>
                        &nbsp;<br />
                        Enter a users email address and select <b>Invite</b>.
                    <br />
                        If the account is valid it will be placed in the review and an email send to the 
                    account holder.
                    </asp:Panel>
                    <table style="width: 100%;background-color: #E2E9EF;">
                        <tr>
                            <td style="width: 100%;">
                                <panel ID="pnlGPTcreditCochrane" runat="server" visible="false">
                                    <table style="width: 100%;">
                                        <tr>
                                            <td style="width: 40%; vertical-align:top;border: 1px;padding:3px">
                                                <b>LLM Coding access</b><br />
                                                <asp:DropDownList ID="ddlCreditPurchasesCochrane" runat="server" 
                                                    DataTextField="CREDIT_ID_REMAINING" DataValueField="CREDIT_PURCHASE_ID" 
                                                    Enabled="True" AutoPostBack="True" OnSelectedIndexChanged="ddlCreditPurchasesCochrane_SelectedIndexChanged">
                                                </asp:DropDownList><br />
                                                If listed, select a PurchaseID from the dropdown menu.<br />
                                                To use LLM Coding you must apply credit to your review.<br />
                                                Credit can be purchased in the Purchase tab. 
                                            </td>
                                            <td style="width: 60%; vertical-align:top;border: 1px;padding:3px">
                                                <b><asp:Label ID="lblChatGPTCreditTableHeadingCochrane" runat="server" Text="LLM Coding Credit" Visible="False"></asp:Label></b>
                                                <br />
                                                <asp:GridView ID="gvCreditForRobotsCochrane" runat="server" Width="100%" onrowdatabound="gvCreditForRobotsCochrane_RowDataBound"
                                                    onrowcommand="gvCreditForRobotsCochrane_RowCommand" AutoGenerateColumns="False" EnableModelValidation="True" 
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
                                </panel>
                            </td>
                        </tr>
                    </table>

                </asp:Panel>
                
            </asp:Panel>

        </asp:Panel>
        

                <asp:Label ID="lblShareableReviewsCochraneFull" runat="server"
                Text="You are not in any Cochrane reviews in EPPI-Reviewer." Visible="False"></asp:Label>
            <br />


</asp:Content>

