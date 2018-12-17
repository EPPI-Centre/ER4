<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ArchieCallBack.aspx.cs" Inherits="WcfHostPortal.ArchieCallBack" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <script type="text/javascript" language="javascript">

        function checkIfDone() {
            var lbl = document.getElementById('<%=lblErrorMsg.ClientID%>');
            if (lbl.innerHTML == "") {
                //all good
                close();
            }
        }
    </script>
</head>
<body  onload="checkIfDone()">
    <form id="form1" runat="server">
    <div>
        <%--<asp:HiddenField ID="HiddenField1" runat="server" />
        <asp:HiddenField ID="HiddenField2" runat="server" />--%>
        <asp:Label runat="server" ID="lblErrorMsg" />
    </div>
    </form>
</body>
</html>
