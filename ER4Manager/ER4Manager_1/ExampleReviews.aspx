
<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="ExampleReviews.aspx.cs" Inherits="ExampleReviews" Title="Example reviews" %>


    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <div style="max-width:800px">


                    <br />
                    <br />
                    <asp:Button ID="cmdCopyNSReview" runat="server" onclick="cmdCopyNSReview_Click" 
                        Text=" Copy review" />
&nbsp; To copy an example non-shareable review to your account click <b>Copy review.</b><br />
                    <br />
                    All new user accounts created after June 1, 2012 have an example review created 
                    under their name. If you created
                    <br />
                    your user account before this date, and you wish to have this example review, 
                    please click <b>Copy review</b>.<br />
                    <br />
                    <asp:Label ID="lblMessage" runat="server" Text="Error" Visible="False"></asp:Label>
                    <br />
                    <br />


                    <br />
            <br />
            <br />

            </div>
</asp:Content>
