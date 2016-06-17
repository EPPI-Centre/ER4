<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="ReturnFromPayment.aspx.cs" Inherits="ReturnFromPayment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">


    <p>
    Thank you for using our external payment processing service.<br />
    Please click &quot;close&quot; to close this window and see the result.</p>
    <p>
        NOTE: this requires JavaScript to work, if you have disabled Javascript, please 
        close this window manually and click &quot;Purchase History&quot; on the window where the 
        purchase was initiated.</p>
<p>
    <asp:Button ID="Button1" runat="server" Text="Close" />
</p>


</asp:Content>

