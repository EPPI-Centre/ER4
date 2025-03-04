<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="AccessSettings.aspx.cs" Inherits="AccessSettings" Title="AccessSettings" %>
        <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

        <div>


                                <br />
                                <br />
                                <br />
                                <b>Access management</b><table style="width: 80%;">
                                    <tr>
                                        <td class="style1">
                                            Enable account creation</td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblAccountCreation" runat="server" AutoPostBack="True" 
                                                onselectedindexchanged="rblAccountCreation_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style1">
                                            Enable password reminder</td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblPasswordReminder" runat="server" 
                                                AutoPostBack="True" 
                                                onselectedindexchanged="rblPasswordReminder_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style1">
                                            Enable purchase</td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblPurchase" runat="server" AutoPostBack="True" 
                                                onselectedindexchanged="rblPurchase_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style1">
                                            Enable adm all<br />
                                        </td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblAdmEnable" runat="server" AutoPostBack="True" 
                                                onselectedindexchanged="rblAdmEnable_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Selected="True" Value="True">True</asp:ListItem>
                                                <asp:ListItem>False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style1" valign="top">
                                            Enable insert example review<br />
                                            (this enables the checkbox on<br />
                                            the &#39;New account&#39; page)</td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblExampleReview" runat="server" AutoPostBack="True" 
                                                onselectedindexchanged="rblExampleReview_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                            &nbsp; Example ReviewID:
                                            <asp:TextBox ID="tbExampleReview" runat="server" Width="50px">0</asp:TextBox>
                                            &nbsp;<br />
                                            <asp:LinkButton ID="lbSave" runat="server" onclick="lbSave_Click">Save change</asp:LinkButton>
                                            &nbsp;(single user, non-shareable review only!)</td>
                                    </tr>
                                    <tr>
                                        <td class="style1" valign="top">
                                            Enable example review copy<br />
                                            (this enables the button on the<br />
                                            &#39;Example review&#39; page )</td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblExampleReviewCopy" runat="server" 
                                                AutoPostBack="True" 
                                                onselectedindexchanged="rblExampleReviewCopy_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style1" valign="top">
                                            Enable Data presenter page<br />
                                            (makes the &#39;Data presenter&#39;
                                            <br />
                                            page visible.</td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblDataPresenter" runat="server" AutoPostBack="True" 
                                                onselectedindexchanged="rblDataPresenter_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style1" valign="top">
                                            Enable the option to turn on
                                            <br />
                                            Priority Screening.</td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblPriorityScreeningEnableEnabler" runat="server" AutoPostBack="True" 
                                                onselectedindexchanged="rblPriorityScreeningEnableEnabler_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style1" valign="top">
                                            Enable users to apply credit for ChatGPT
                                            </td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblChatGPTEnableEnabler" runat="server" AutoPostBack="True" 
                                                onselectedindexchanged="rblChatGPTEnableEnabler_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style1" valign="top">
                                            Enable credit option in online shop.</td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblEnableShopCredit" runat="server" AutoPostBack="True" 
                                                onselectedindexchanged="rblEnableShopCredit_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style1" valign="top">
                                            Enable outstanding fee option in online shop.</td>
                                        <td class="style2">
                                            <asp:RadioButtonList ID="rblEnableShopDebit" runat="server" AutoPostBack="True" 
                                                onselectedindexchanged="rblEnableShopDebit_SelectedIndexChanged" 
                                                RepeatDirection="Horizontal">
                                                <asp:ListItem Value="True">True</asp:ListItem>
                                                <asp:ListItem Selected="True">False</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                </table>
                                <br />
                                <asp:TextBox ID="tbContactID" runat="server"></asp:TextBox>
&nbsp;<asp:LinkButton ID="lbEnterAs" runat="server" onclick="lbEnterAs_Click">Enter</asp:LinkButton>
&nbsp;as this CONTACT_ID (this will also set IsAdm to false)<br />
                                <br />
                                <br />


                                <br />
        
        
                                </div>
</asp:Content>