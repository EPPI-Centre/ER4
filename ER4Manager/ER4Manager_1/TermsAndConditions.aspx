<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="TermsAndConditions.aspx.cs" Inherits="TermsAndConditions" Title="TermsAndConditions" %>

    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

    
    
    
        <div>
        

                                &nbsp;<br />
                                <b>Terms and agreements</b><br />
                <asp:TextBox ID="tbTandA" runat="server" Rows="20" TextMode="MultiLine" 
                    Width="800px" BorderColor="Black" BorderStyle="Solid" 
                    BorderWidth="1px" ReadOnly="True"></asp:TextBox>
                                <br />
                                <br />
&nbsp;<input id="fDocument" runat="server" class="button" name="fDocument" style="font-family: verdanna" 
                                    type="file" />&nbsp;&nbsp;&nbsp;
                                <asp:Button ID="cmdUploadFile" runat="server" CssClass="button" 
                                    OnClick="cmdUploadFile_Click" Text="Upload file" Width="100px" />
&nbsp;&nbsp;&nbsp; This will upload a new T &amp; A file. It should be an html file.<br />
&nbsp;&nbsp;
                                <br />
                                <br />
                                <asp:Button ID="cmdSaveEdits" runat="server" onclick="cmdSaveEdits_Click" 
                                    Text="Save edits" Enabled="False" />
&nbsp;&nbsp; If you have edited the displayed text and wish to save it select <strong>Save edits</strong>. This 
                                will create a new file.<br />
                                <br />
                                <b>I need to put an html editor in here before we can do onscreen edits!</b><br />
                                <br />
&nbsp;
                                <br />
                                
                                <br />
        
        
                                </div>
</asp:Content>


