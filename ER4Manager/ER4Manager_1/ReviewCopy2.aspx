<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="ReviewCopy2.aspx.cs" Inherits="ReviewCopy2" Title="ReviewCopy 2" %>
    
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <div style="max-width:800px">

            <br />
            <b>Copy single user review (do not try to copy a review with multiple users!)<br />
            This admin area can overide what it says in TB_MANAGEMENT_SETTINGS</b><br />
            <br />
            Source Review ID:
            <asp:TextBox ID="tbSourceReview" runat="server" Width="50px">846</asp:TextBox>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Default:
            <asp:Label ID="lblSourceReview" runat="server" Text="846"></asp:Label>
            &nbsp;(on live database)<br />
            <br />
            <asp:TextBox ID="tbItemsToCopy" runat="server" Width="100px"></asp:TextBox>
&nbsp;Attribute of items to copy or type &#39;0&#39; (zero) for all<br />
            <br />
            Destination Review ID:
            &nbsp;<asp:Label ID="lblDestinationReviewID" runat="server" Text="N/A"></asp:Label>
            <br />
            <br />
            ContactID:&nbsp;
            <asp:Label ID="lblContactID" runat="server" Text="Label"></asp:Label>
            <br />
&nbsp;<br />
            Message:
            <asp:Label ID="lblMessage" runat="server" Text="0"></asp:Label>
            <br />
            <br />
                            <asp:Button ID="cmdTestReviewCopy" runat="server" 
                                onclick="cmdTestReviewCopy_Click" Text="Copy review" 
                Enabled="False" />
                        &nbsp;
            <br />
            <br />
            <hr />
            <br />
            Individual steps in the copy process (must be run in order)<br />
            <br />
            <asp:Button ID="cmdStep01" runat="server" onclick="cmdStep01_Click" 
                Text="Step 1" Enabled="False" />
&nbsp;Create review, add contacts and roles<br />
            <br />
            <asp:Button ID="cmdStep03" runat="server" onclick="cmdStep03_Click" 
                Text="Step 3" Enabled="False" />
&nbsp;Copy items and place items in new review<br />
            <br />
            <asp:Button ID="cmdStep05" runat="server" onclick="cmdStep05_Click" 
                Text="Step 5" Enabled="False" />
&nbsp;Copy the duplicate information<br />
            <br />
            <asp:Button ID="cmdStep0709" runat="server" onclick="cmdStep0709_Click" 
                Text="Step 7 and 9" Enabled="False" />
&nbsp;Collects the codesets from source review <b>and</b> sets up copies of the codesets in 
            the new review<br />
            <br />
            <asp:Button ID="cmdStep0711" runat="server" onclick="cmdStep0711_Click" 
                Text="Step 7 and 11" Enabled="False" />
&nbsp;Collects the codesets from source review <b>and </b>copies the coding from the old 
            review to the new review<br />
            <br />
            <asp:Button ID="cmdStep13" runat="server" onclick="cmdStep13_Click" 
                Text="Step 13" Enabled="False" />
            &nbsp;Copy the work assignments, diagrams, searches, uploaded documents<br />
            <br />
            <asp:Button ID="cmdStep15" runat="server" onclick="cmdStep15_Click" 
                Text="Step 15" Enabled="False" />
            &nbsp;Copy reports, meta-analysis, links<br />
            <br />
            <br />
            <asp:Button ID="cmdCleanup" runat="server" onclick="cmdCleanup_Click" 
                Text="Cleanup" Enabled="False" />
&nbsp;removes links between old and new review<br />
            <asp:TextBox ID="tbDestinationReviewID" runat="server" Width="50px"></asp:TextBox>
&nbsp;Destination review ID.<br />
            <br />
            <hr />
            <br />
            <asp:Button ID="cmdGetItemAttrID" runat="server" 
                onclick="cmdGetItemAttrID_Click" Text="Get INFO" />
            <br />
            <asp:TextBox ID="tbAttrID" runat="server"></asp:TextBox>
&nbsp; enter Identifying ATTR_ID<br />
            <br />
            <asp:TextBox ID="tbInfo" runat="server" Rows="5" TextMode="MultiLine" 
                Width="99%"></asp:TextBox>
            <br />
            <br />
                        <br />
            <br />
            <br />
            <br />
            <br />
                        <br />
            <br />
            <br />

            </div>
</asp:Content>

