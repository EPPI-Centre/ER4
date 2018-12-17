<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PresenterEditIntro.aspx.cs" Inherits="PresenterEditIntro" %>
<%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
<%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Edit introduction text</title>
    <link href="MainStyleSheet.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="arcticwhite/css/style.css" />
    <style type="text/css">
        .button
{
  font-family: verdana; 
  font-size:small;
  color:#3F3F3F;  
}
        </style>
    <script language="javascript" type="text/javascript">

    </script>
</head>
<body>
    <form id="form1" runat="server">
        <telerik:RadScriptManager runat="server" ID="RadScriptManager1" />

        <div>
            <strong>Edit Introduction text&nbsp;</strong><br />
            Web Database name: 
            <asp:Label ID="lblWebDbName1" runat="server" Font-Bold="True" Text="N/A"></asp:Label>
            &nbsp;&nbsp;&nbsp;&nbsp;<br />
            Web Database ID:
            <asp:Label ID="lblWebDB_ID" runat="server" Font-Bold="True" Text="N/A"></asp:Label>
            <br />
            <br />
            <asp:Panel ID="pnlUserEdit" runat="server">
            
            <br />
            <table style="width:100%;" border="1">
                <tr>
                    <td>
            <asp:Button ID="cmdSave" runat="server" Text="Save" onclick="cmdSave_Click1" />
            &nbsp;&nbsp;&nbsp;&nbsp; 
            <asp:LinkButton ID="lbClose" runat="server" onclick="lbClose_Click">Close</asp:LinkButton>
            
       
       
       
       
       
       
                    </td>
                    <td>
            <strong>Note</strong>: - HTML tags will not be accepted due to security concerns.<br />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; - Written web addresses starting with &quot;http://&quot; or &quot;https://&quot; will be 
                        turned into hyperlinks.<br /> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; -&nbsp; square bracketed <strong>[link 
                        text]</strong> preceeding the web address will be recognised<br /> 
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ex:&nbsp; [EPPI-Centre]http://eppi.ioe.ac.uk will produce 
                        <a href="http://eppi.ioe.ac.uk" alt="eppi.ioe.ac.uk">EPPI-Centre</a>
                        <br />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; - All Headings and Paragraphs are optional.</td>
                </tr>
            </table>
                <table style="width: 95%;">
                    <tr>
                        <td width="20%">
                            &nbsp;</td>
                        <td width="50%">
                            <asp:Label ID="lblWebDbName" runat="server" Font-Bold="True" Font-Size="Medium" 
                                ForeColor="#0066FF" Text="N/A" Visible="False"></asp:Label>
                        </td>
                        <td align="right" width="30%">
                            <asp:Image ID="imgHeaderImage1" runat="server" Visible="False" />
                        </td>
                        <td align="right" width="30%">
                            <asp:Image ID="imgHeaderImage2" runat="server" Visible="False" />
                        </td>
                    </tr>
                </table>
                <table border="0" cellpadding="0" cellspacing="0" style="width: 95%;">
                    <tr>
                        <td width="20%">
                            &nbsp;</td>
                        <td width="80%">
                            <img id="IMG1" alt="menuLeft" 
                            src="Images/menu2.jpg" border="0" />
                        </td>
                    </tr>
                </table>
                <table border="0" cellpadding="0" cellspacing="0" style="width:95%;">
                    <tr>
                        <td width="20%">
                            &nbsp;<td class="CellTL" width="3%">
                                &nbsp;<td class="CellTop" width="70%">
                                    &nbsp;</td>
                                <td class="CellTR" width="3%">
                                    &nbsp;</td>
                                <td width="4%">
                                    &nbsp;</td>
                            </td>
                        </td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 1</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading1" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Font-Bold="True" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 1 text<br />
                            <asp:CheckBox ID="cbIndent1" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent1_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet1" runat="server" AutoPostBack="True" Enabled="False" 
                                oncheckedchanged="cbBullet1_CheckedChanged" Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph1" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 2</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading2" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 2 text<br />
                            <asp:CheckBox ID="cbIndent2" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent2_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet2" runat="server" AutoPostBack="True" Enabled="False" 
                                oncheckedchanged="cbBullet2_CheckedChanged" Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph2" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 3</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading3" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 3 text<br />
                            <asp:CheckBox ID="cbIndent3" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent3_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet3" runat="server" AutoPostBack="True" Enabled="False" 
                                oncheckedchanged="cbBullet3_CheckedChanged" Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph3" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            CellRight</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 4</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading4" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 4 text<br />
                            <asp:CheckBox ID="cbIndent4" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent4_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet4" runat="server" AutoPostBack="True" Enabled="False" 
                                oncheckedchanged="cbBullet4_CheckedChanged" Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph4" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 5</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading5" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 5 text<br />
                            <asp:CheckBox ID="cbIndent5" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent5_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet5" runat="server" AutoPostBack="True" Enabled="False" 
                                oncheckedchanged="cbBullet5_CheckedChanged" Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph5" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 6</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading6" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 6 text<br />
                            <asp:CheckBox ID="cbIndent6" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent6_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet6" runat="server" AutoPostBack="True" Enabled="False" 
                                oncheckedchanged="cbBullet6_CheckedChanged" Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph6" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 7</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading7" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 7 text<br />
                            <asp:CheckBox ID="cbIndent7" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent7_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet7" runat="server" AutoPostBack="True" Enabled="False" 
                                oncheckedchanged="cbBullet7_CheckedChanged" Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph7" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 8</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading8" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 8 text<br />
                            <asp:CheckBox ID="cbIndent8" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent8_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet8" runat="server" AutoPostBack="True" Enabled="False" 
                                oncheckedchanged="cbBullet8_CheckedChanged" Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph8" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 9</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading9" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 9 text<br />
                            <asp:CheckBox ID="cbIndent9" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent9_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet9" runat="server" AutoPostBack="True" Enabled="False" 
                                oncheckedchanged="cbBullet9_CheckedChanged" Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph9" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 10</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading10" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td width="20%">
                            Paragraph 10 text<br />
                            <asp:CheckBox ID="cbIndent10" runat="server" AutoPostBack="True" 
                                oncheckedchanged="cbIndent10_CheckedChanged" Text="Indent" />
                            <br />
                            <asp:CheckBox ID="cbBullet10" runat="server" AutoPostBack="True" 
                                Enabled="False" oncheckedchanged="cbBullet10_CheckedChanged" 
                                Text="with bullet" />
                        </td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbParagraph10" runat="server" BackColor="#CCFFCC" 
                                CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                    </tr>
                    </tr>
                    <tr>
                        <td width="20%">
                            <strong>Heading 11</strong></td>
                        <td class="CellLeft" width="3%">
                            &nbsp;</td>
                        <td style="width: 70%">
                            <asp:TextBox ID="tbHeading11" runat="server" BackColor="#FFFFCC" 
                                CssClass="HeaderBox" Rows="1" Width="99%" Font-Bold="True"></asp:TextBox>
                        </td>
                        <td class="CellRight" width="3%">
                            &nbsp;</td>
                        <td width="4%">
                            &nbsp;</td>
                        <tr>
                            <td width="20%">
                                Paragraph 11 text<br />
                                <asp:CheckBox ID="cbIndent11" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent11_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet11" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet11_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph11" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                <strong>Heading 12</strong></td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbHeading12" runat="server" BackColor="#FFFFCC" 
                                    CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                Paragraph 12 text<br />
                                <asp:CheckBox ID="cbIndent12" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent12_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet12" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet12_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph12" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                <strong>Heading 13</strong></td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbHeading13" runat="server" BackColor="#FFFFCC" 
                                    CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                Paragraph 13 text<br />
                                <asp:CheckBox ID="cbIndent13" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent13_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet13" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet13_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph13" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                CellRight</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                <strong>Heading 14</strong></td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbHeading14" runat="server" BackColor="#FFFFCC" 
                                    CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                Paragraph 14 text<br />
                                <asp:CheckBox ID="cbIndent14" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent14_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet14" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet14_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph14" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                <strong>Heading 15</strong></td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbHeading15" runat="server" BackColor="#FFFFCC" 
                                    CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                Paragraph 15 text<br />
                                <asp:CheckBox ID="cbIndent15" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent15_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet15" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet15_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph15" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                <strong>Heading 16</strong></td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbHeading16" runat="server" BackColor="#FFFFCC" 
                                    CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                Paragraph 16 text<br />
                                <asp:CheckBox ID="cbIndent16" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent16_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet16" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet16_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph16" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                <strong>Heading 17</strong></td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbHeading17" runat="server" BackColor="#FFFFCC" 
                                    CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                Paragraph 17 text<br />
                                <asp:CheckBox ID="cbIndent17" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent17_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet17" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet17_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph17" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                <strong>Heading 18</strong></td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbHeading18" runat="server" BackColor="#FFFFCC" 
                                    CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                Paragraph 18 text<br />
                                <asp:CheckBox ID="cbIndent18" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent18_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet18" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet18_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph18" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                <strong>Heading 19</strong></td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbHeading19" runat="server" BackColor="#FFFFCC" 
                                    CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                Paragraph 19 text<br />
                                <asp:CheckBox ID="cbIndent19" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent19_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet19" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet19_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph19" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                <strong>Heading 20</strong></td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbHeading20" runat="server" BackColor="#FFFFCC" 
                                    CssClass="HeaderBox" Rows="1" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                Paragraph 20 text<br />
                                <asp:CheckBox ID="cbIndent20" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbIndent20_CheckedChanged" Text="Indent" />
                                <br />
                                <asp:CheckBox ID="cbBullet20" runat="server" AutoPostBack="True" 
                                    Enabled="False" oncheckedchanged="cbBullet20_CheckedChanged" 
                                    Text="with bullet" />
                            </td>
                            <td class="CellLeft" width="3%">
                                &nbsp;</td>
                            <td style="width: 70%">
                                <asp:TextBox ID="tbParagraph20" runat="server" BackColor="#CCFFCC" 
                                    CssClass="ParagraphBox" Rows="3" TextMode="MultiLine" Width="99%"></asp:TextBox>
                            </td>
                            <td class="CellRight" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td width="20%">
                                &nbsp;</td>
                            <td class="CellBL" width="3%">
                                &nbsp;</td>
                            <td class="CellBottom" style="width: 70%">
                                &nbsp;</td>
                            <td class="CellBR" width="3%">
                                &nbsp;</td>
                            <td width="4%">
                                &nbsp;</td>
                        </tr>
                    </tr>
                </table>
                <br />
            </asp:Panel>
            <asp:Panel ID="pnlAdminEdit" runat="server" Visible="False">
            
                <br />
                <asp:Button ID="cmdSaveAdmin" runat="server" onclick="cmdSaveAdmin_Click" 
                    Text="Save" Enabled="False" />
                &nbsp;&nbsp;
                <asp:LinkButton ID="lbClose0" runat="server" onclick="lbClose_Click">Close</asp:LinkButton>
                &nbsp;&nbsp;
                <asp:CheckBox ID="cbUserEditIntro" runat="server" AutoPostBack="True" 
                    oncheckedchanged="cbUserEditIntro_CheckedChanged" Text="View user edit intro" />
                <telerik:RadEditor ID="RadEditor" runat="server" Height="600px" Width="750px">
                    <Tools>
                        <telerik:EditorToolGroup>
                            <telerik:EditorTool Name="Bold" />
                            <telerik:EditorTool Name="Italic" />
                            <telerik:EditorTool Name="Underline" />
                            <telerik:EditorTool Name="Indent" />
                            <telerik:EditorTool Name="Outdent" />
                            <telerik:EditorTool Name="InsertUnorderedList" />
                            <telerik:EditorTool Name="InsertOrderedList" />
                            <telerik:EditorTool Name="Cut" />
                            <telerik:EditorTool Name="Copy" />
                            <telerik:EditorTool Name="Paste" />
                        </telerik:EditorToolGroup>
                    </Tools>
                </telerik:RadEditor>
                <br />
            </asp:Panel>
            

        </div>
    </form>
</body>
</html>

