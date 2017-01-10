<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="FilterBuilder.aspx.cs" Inherits="FilterBuilder" Title="FilterBuilder" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <br />
        <asp:DropDownList ID="DropDownList1" runat="server">
        </asp:DropDownList>
&nbsp;Filter name&nbsp;
        <asp:LinkButton ID="lbEditName" runat="server">Edit</asp:LinkButton>
        <br />
        <asp:Panel ID="Panel1" runat="server">
            <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
            &nbsp;
            <asp:LinkButton ID="lbSaveFilterName" runat="server">Save</asp:LinkButton>
            &nbsp;&nbsp;
            <asp:LinkButton ID="lbDeleteFilterName" runat="server">Delete</asp:LinkButton>
        </asp:Panel>
        <br />
        <br />


        </asp:Content>