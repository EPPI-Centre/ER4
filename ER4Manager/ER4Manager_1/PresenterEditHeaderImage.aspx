<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PresenterEditHeaderImage.aspx.cs" Inherits="PresenterEditHeaderImage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Edit header image</title>
    <link href="MainStyleSheet.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="arcticwhite/css/style.css" />
    <style type="text/css"></style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
               <strong>Header images and links</strong><br />
               Web Database name:
            <asp:Label ID="lblWebDbName" runat="server" Font-Bold="True" Text="N/A"></asp:Label>
            &nbsp;&nbsp;&nbsp;&nbsp;<br />
               Web Database ID:
            <asp:Label ID="lblWebDB_ID" runat="server" Font-Bold="True" Text="N/A"></asp:Label>
               <br />
               <br />
                        <asp:Image ID="imgHeaderImage1" runat="server" />
                    <br />
            <br />
               Image 1 details (left side)<br />
            <table style="width:100%;" class="imageBox">
                <tr>
                    <td>
                        &nbsp;</td>
                    <td>
                        <input id="fDocument1" runat="server" class="button" name="fDocument1" 
                            style="font-family: verdanna" type="file" />&nbsp;&nbsp;
                        <asp:Button ID="cmdUploadImage1" runat="server" onclick="cmdUploadImage1_Click" 
                            Text="Upload" />
&nbsp;&nbsp; <asp:Label ID="lblMessage1" runat="server" Font-Bold="True" ForeColor="Red" Text="." 
                            Visible="False"></asp:Label>
                        &nbsp;Upload header image
                        1&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:LinkButton ID="lbDeleteImage1" runat="server" 
                            onclick="lbDeleteImage1_Click">Delete image 1</asp:LinkButton>
            
       
                        <br />
                        <asp:TextBox ID="tbImageURL1" runat="server" Rows="1" Width="75%" 
                            BackColor="#FFFFCC"></asp:TextBox>
                        Enter image url<br />
                        <asp:LinkButton ID="lbSaveLink1" runat="server" 
                            onclick="lbSaveLink1_Click">Save url</asp:LinkButton>
            
       
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        Max image size =
                         <strong>
                        250Kb&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </strong>Images will be reduced to max height = 
                        <strong>
                        80 px </strong>or max width =
                        <strong>
                        300 px</strong></td>
                    <td>
                        &nbsp;</td>
                </tr>
                </table>
               <br />
                        <asp:Image ID="imgHeaderImage2" runat="server" />
                    <br />
               <br />
               Image 2 details (centre)<br />
            <table style="width:100%;" class="imageBox">
                <tr>
                    <td>
                        &nbsp;</td>
                    <td>
                        <input id="fDocument2" runat="server" class="button" name="fDocument2" 
                            style="font-family: verdanna" type="file" />&nbsp;&nbsp;
                        <asp:Button ID="cmdUploadImage2" runat="server" onclick="cmdUploadImage2_Click" 
                            Text="Upload" />
&nbsp;&nbsp; <asp:Label ID="lblMessage2" runat="server" Font-Bold="True" ForeColor="Red" Text="." 
                            Visible="False"></asp:Label>
                        &nbsp;Upload header image
                        2&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:LinkButton ID="lbDeleteImage2" runat="server" 
                            onclick="lbDeleteImage2_Click">Delete image 2</asp:LinkButton>
            
       
                        <br />
                        <asp:TextBox ID="tbImageURL2" runat="server" Rows="1" Width="75%" 
                            BackColor="#FFFFCC"></asp:TextBox>
                        Enter image url<br />
                        <asp:LinkButton ID="lbSaveLink2" runat="server" 
                            onclick="lbSaveLink2_Click">Save url</asp:LinkButton>
            
       
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        Max image size =
                         <strong>
                        250Kb&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </strong>Images will be reduced to max height = 
                        <strong>
                        80 px </strong>or max width =
                        <strong>
                        300 px</strong></td>
                    <td>
                        &nbsp;</td>
                </tr>
                </table>
               <br />
                        <asp:Image ID="imgHeaderImage3" runat="server" />
                    <br />
               <br />
               Image 3 details (right side)<br />
            <table style="width:100%;" class="imageBox">
                <tr>
                    <td>
                        &nbsp;</td>
                    <td>
                        <input id="fDocument3" runat="server" class="button" name="fDocument3" 
                            style="font-family: verdanna" type="file" />&nbsp;&nbsp;
                        <asp:Button ID="cmdUploadImage3" runat="server" onclick="cmdUploadImage3_Click1" 
                            Text="Upload" />
&nbsp;&nbsp; <asp:Label ID="lblMessage3" runat="server" Font-Bold="True" ForeColor="Red" Text="." 
                            Visible="False"></asp:Label>
                        &nbsp;Upload header image
                        3&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:LinkButton ID="lbDeleteImage3" runat="server" 
                            onclick="lbDeleteImage3_Click1">Delete image 3</asp:LinkButton>
            
       
                        <br />
                        <asp:TextBox ID="tbImageURL3" runat="server" Rows="1" Width="75%" 
                            BackColor="#FFFFCC"></asp:TextBox>
                        Enter image url<br />
                        <asp:LinkButton ID="lbSaveLink3" runat="server" 
                            onclick="lbSaveLink3_Click1">Save url</asp:LinkButton>
            
       
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        Max image size =
                         <strong>
                        250Kb&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </strong>Images will be reduced to max height = 
                        <strong>
                        80 px </strong>or max width =
                        <strong>
                        300 px</strong></td>
                    <td>
                       &nbsp;</td>
                </tr>
                </table>
            <br />
            <table style="width:100%;">
                <tr>
                    <td>
            &nbsp;<asp:LinkButton ID="lbClose" runat="server" onclick="lbClose_Click">Close window</asp:LinkButton>
            
       
    </div>
    </form>
</body>
</html>
