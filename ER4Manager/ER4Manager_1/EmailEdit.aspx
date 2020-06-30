<%@ Page Language="C#" AutoEventWireup="true" CodeFile="EmailEdit.aspx.cs" Inherits="EmailEdit" %>
<%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Edit email</title>
    <link href="MainStyleSheet.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="arcticwhite/css/style.css" />
</head>
<body>
    <form id="form1" runat="server">
    <telerik:RadScriptManager runat="server" ID="RadScriptManager1" />

        <div>
            <br />

            <table width="680px">
                <tr>
                    <td>
                        <asp:Button ID="cmdSave" runat="server" Text="Save" OnClick="cmdSave_Click" />

                        &nbsp;&nbsp;&nbsp;
            <asp:LinkButton ID="lbClose" runat="server" OnClick="lbClose_Click">Close</asp:LinkButton>
                        &nbsp;&nbsp;&nbsp;
                        </td>
                    <td>

                        Email name:
            <asp:TextBox ID="tbEmailName" runat="server">N/A</asp:TextBox>
                        &nbsp;&nbsp; Email ID:
            <asp:Label ID="lblEmailID" runat="server" Font-Bold="True" Text="N/A"></asp:Label>
                        &nbsp;
                    </td>
                </tr>
            </table>
            <br />

            <telerik:RadEditor runat="server" ID="RadEditor" Height="350px" 
                MaxHtmlLength="4000">
    <Tools>
        <telerik:EditorToolGroup> 
             <telerik:EditorTool Name="Bold" />
             <telerik:EditorTool Name="Italic" />
             <telerik:EditorTool Name="Underline" />
             <telerik:EditorTool Name="Indent" />
             <telerik:EditorTool Name="Outdent" />
             <telerik:EditorTool Name="InsertUnorderedList" />
             <telerik:EditorTool Name="InsertOrderedList" />    
             <telerik:EditorTool Name="Cut"/>
             <telerik:EditorTool Name="Copy"/>
             <telerik:EditorTool Name="Paste"/>
         </telerik:EditorToolGroup>
    </Tools>
<Content>
</Content>

<TrackChangesSettings CanAcceptTrackChanges="False"></TrackChangesSettings>
</telerik:RadEditor>



        
        
            <br />
            <asp:LinkButton ID="lbSendTestEmail" runat="server" 
                onclick="lbSendTestEmail_Click" Enabled="True">Send test </asp:LinkButton>
            This will send a test message to <a href="mailto:EPPISupport@ucl.ac.uk">
            EPPISupport@ucl.ac.uk</a><br />
            <br />
        
        </div>
    </form>
</body>
</html>
