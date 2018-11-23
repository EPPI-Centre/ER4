<%@ Page Language="C#" AutoEventWireup="true" CodeFile="JumpToWPMUCL.aspx.cs" validateRequest ="false" Inherits="JumpToWPMUCL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body onload="document.forms.form1.submit()">
    
    <form id="form1" action="JumpToWPMUCL.aspx" method="post" runat="server">
    
        <%--<asp:HiddenField ID="msgid" runat="server" />--%>
        <asp:HiddenField ID="xml" runat="server" />
        <%--<input id="msgid2" type="hidden" runat="server" />--%>
        <br /><B>Transferring to the payment gateway... </B><br />
        Please wait for the next page to load.<br /><br />
        Please do not hit the "Refresh" or "Back" buttons on this 
        and the next pages, <br />
        "back" and "next" navigation buttons will be present inside the payment pages.
    
    </form>
</body>
</html>