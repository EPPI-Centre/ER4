<%@ Page Language="C#" AutoEventWireup="true" CodeFile="NewsletterView.aspx.cs" Inherits="NewsletterView" %>
<%@ Register assembly="Telerik.Web.UI, Version=2014.2.724.35, Culture=neutral" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Edit email</title>
    <link href="MainStyleSheet.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="arcticwhite/css/style.css" />
</head>
<body>
    <form id="form2" runat="server">
    <telerik:RadScriptManager runat="server" ID="RadScriptManager1" />


        <div>
            <br />
        
        
        <asp:Button ID="cmdSave" runat="server" Text="Save" OnClick="cmdSave_Click" />
        
        &nbsp;&nbsp;&nbsp;
            <asp:LinkButton ID="lbClose" runat="server" onclick="lbClose_Click">Close</asp:LinkButton>
&nbsp;&nbsp;&nbsp; Newsletter ID:
            <asp:Label ID="lblNewsletterID" runat="server" Font-Bold="True" Text="N/A"></asp:Label>
            <br />

            <telerik:RadEditor runat="server" ID="RadEditor" Height="600px" Width="750px">
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
</telerik:RadEditor>



        
        
            <br />
        
        </div>
    </form>
</body>
</html>

