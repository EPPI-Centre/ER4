<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Organisation.aspx.cs" Inherits="Organisation" %>




<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    
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


        <div>

        <asp:Panel ID="pnlMessage" runat="server" Visible="False">
            <asp:Label ID="lblNotAnOranisationAdm" runat="server" Font-Bold="True" 
                ForeColor="Red" 
                
                Text="You are not an organisation administrator so there is nothing to show. &lt;br&gt;Please go to the Setup page."></asp:Label>
        <br />
                                    </asp:Panel>

            <asp:Panel ID="pnlOrganisationExists" runat="server" Visible="False">

            <asp:Panel ID="pnlMultipleOrganisation" runat="server" Visible="False">
                <asp:DropDownList ID="ddlYourOrganisations" runat="server" 
                    DataTextField="ORGANISATION_NAME" DataValueField="ORGANISATION_ID" 
                    onselectedindexchanged="ddlYourOrganisations_SelectedIndexChanged" 
                    AutoPostBack="True">
                </asp:DropDownList>
                &nbsp;&nbsp; You are the administrator of multiple organisations<br /> <br />
            </asp:Panel>
                
                <asp:Panel ID="pnlOrganisation" runat="server">
                    <b>Organisation details</b><br />
                    <table ID="Table3" border="1" cellpadding="1" cellspacing="1" width="100%">
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Organisation name</td>
                            <td ;="" style="width: 75%; background-color: #E2E9EF">
                                &nbsp;<asp:Label ID="lblOrganisationName" runat="server" Text="N/A"></asp:Label>
                                &nbsp;</td>
                        </tr>
                        
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Address *</td>
                            <td ;="" style="width: 75%; background-color: #E2E9EF">
                                <asp:TextBox ID="tbAddress" runat="server" Width="80%">N/A</asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Telephone *</td>
                            <td ;="" style="width: 75%; background-color: #E2E9EF">
                                <asp:TextBox ID="tbTelephone" runat="server" Width="80%">N/A</asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Notes</td>
                            <td ;="" style="width: 75%; background-color: #E2E9EF">
                                <asp:TextBox ID="tbNotes" runat="server" Width="80%">N/A</asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                <asp:Label ID="lblOrgID1" runat="server" Text="Organisation ID"></asp:Label>
                            </td>
                            <td ;="" style="width: 75%; background-color: #E2E9EF">
                                &nbsp;<asp:Label ID="lblOrganisationID" runat="server" Text="N/A"></asp:Label>
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td style="background-color: #B6C6D6; width: 25%;">
                                Date created</td>
                            <td ;="" style="width: 75%; background-color: #E2E9EF">
                                &nbsp;<asp:Label ID="lblDateCreated" runat="server" Text="N/A"></asp:Label>
                                &nbsp;</td>
                        </tr>

                        
                    </table>
                    <asp:Button ID="cmdSaveOrganisation" runat="server" onclick="cmdSaveOrganisation_Click" 
                        Text="Save" />
                    &nbsp;&nbsp;
                    <asp:Label ID="lblOrganisationMessage" runat="server" Font-Bold="False" 
                        Text="Required fields *" Visible="False"></asp:Label>
                    &nbsp;
                    <asp:Button ID="cmdPlaceFunder" runat="server" BackColor="White" 
                        BorderColor="White" BorderStyle="None" ForeColor="White" Height="1px" 
                        OnClick="cmdPlaceFunder_Click" style="font-weight: bold" Width="1px" /><br />
                </asp:Panel>

            

            <asp:Panel ID="pnlAccountsAndReviews" runat="server">
                <br />
                <table style="width:100%;">
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;">
                            <b>Accounts in organisation</b></td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;">
                            <b>Reviews in Organisation</b></td>
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
                                DataKeyNames="REVIEW_ID" EnableModelValidation="True" Width="100%" OnRowDataBound="gvReviews_RowDataBound">
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
                            <asp:Button ID="cmdAddAccount" runat="server" OnClick="cmdAddAccount_Click2" Text="Add account" />
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
                                onclick="cmdAddReview_Click" Enabled="False" />
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
                            <b>Organisation admins</b></td>
                        <td style="background-color: #ffffff; width: 10%;">
                            &nbsp;</td>
                        <td style="background-color: #ffffff; width: 45%;">
                            </td>
                    </tr>
                    <tr>
                        <td style="background-color: #ffffff; width: 45%;" valign="top">
                            <asp:GridView ID="gvOrganisationAdms" runat="server" AutoGenerateColumns="False" 
                                DataKeyNames="EMAIL" EnableModelValidation="True" 
                                onrowcommand="gvOrganisationAdms_RowCommand" onrowdatabound="gvOrganisationAdms_RowDataBound" 
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

