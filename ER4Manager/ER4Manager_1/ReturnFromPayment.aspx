<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="ReturnFromPayment.aspx.cs" Inherits="ReturnFromPayment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">


    <p>
    Thank you for using our external payment processing service.</p>
	<p>You will receive an email confirmation if your purchase was completed. <br /><b>Please remember to check also the spam folder</b>.</p>
    <p style="font-size:1.3em; max-width:780px;"><B>Please close this tab/window manually</b> and click &quot;Purchase History&quot; on the window where the 
        purchase was initiated.</p>
     <!--<p>
        NOTE: this requires JavaScript to work, if you have disabled Javascript, please 
         close this window manually and click &quot;Purchase History&quot; on the window where the 
         purchase was initiated.</p>
 <p>
     <asp:Button ID="Button1" runat="server" Text="Close" />
 </p>-->


</asp:Content>

