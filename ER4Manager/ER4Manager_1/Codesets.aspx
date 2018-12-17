

<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Codesets.aspx.cs" Inherits="Codesets" Title="Codesets" %>
    

    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    

    
    
    
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">


         <script type="text/javascript">

             function SelectedSourceRootNode(sender, eventArgs) {
                 var selectedNode = eventArgs.get_node();
                 if (selectedNode.get_attributes().getAttribute('IS_ROOT') != 'Yes') {
                     if (document.getElementById('ctl00_ContentPlaceHolder1_cmdCopyPersonalCodeSet') != null) {
                         document.getElementById('ctl00_ContentPlaceHolder1_cmdCopyPersonalCodeSet').disabled = true;
                     }
                 }
                 else {
                     if (document.getElementById('ctl00_ContentPlaceHolder1_cmdCopyPersonalCodeSet') != null) {
                         document.getElementById('ctl00_ContentPlaceHolder1_cmdCopyPersonalCodeSet').disabled = false;
                     }
                 }
             }



             function SelectPublicRootNode(sender, eventArgs) 
             { 
                 var selectedNode = eventArgs.get_node();
                 if (selectedNode.get_attributes().getAttribute('IS_ROOT') != 'Yes') 
                 {
                     if (document.getElementById('ctl00_ContentPlaceHolder1_cmdCopyPublicCodeSet') != null) {
                         document.getElementById('ctl00_ContentPlaceHolder1_cmdCopyPublicCodeSet').disabled = true;
                     }    
                 }
                 else 
                 {
                     if (document.getElementById('ctl00_ContentPlaceHolder1_cmdCopyPublicCodeSet') != null) {
                         document.getElementById('ctl00_ContentPlaceHolder1_cmdCopyPublicCodeSet').disabled = false;
                     }
                 }
             }


             function SelectedNode(sender, eventArgs) // Used in crosstabs.aspx
             {
                 // we want the root node to be selected at all times
                 var tree1 = $find("<%= radTVSourceCodesets.ClientID %>");
                 var node = eventArgs.get_node();
                 var s = node.get_text();
                 var currentObject = node.get_parent();
                 while (currentObject != null) {
                     // get_parent() will return null when we reach the treeview
                     if (currentObject.get_parent() != null) {
                         s = currentObject.get_text();
                     }
                     currentObject = currentObject.get_parent();
                 }
                 var node1 = tree1.findNodeByText(s);
                 node1.set_selected(true);
             }


    </script>


        <div style="max-width:800px">

            <br />
            <asp:Panel ID="pnlEditShareable" runat="server" BorderColor="Black" 
                BorderStyle="Solid" BorderWidth="1px">
                <b>Instructions</b><br /> 1. Select the <b>source review </b>
                <br />
                2. Select a codeset from the source review<br /> 3. Select a <b>destination review</b>. This 
                can be the same as the source review if required.<br /> 4. Click on <b>Copy</b> 
                and a copy of the selected codeset will be placed in the source review.
                <br />
                <br />
                Public access codesets are also availble to copy into your review.<br />
                <br />
                The copy can be edited in EPPI-Reviewer 4 without affecting the original.<br />
                <asp:Panel ID="pnlChooseCodes" runat="server" BorderColor="Black" 
                    BorderStyle="Solid" BorderWidth="1px" Width="99%" 
                    BackColor="#E2E9EF">
                    <asp:Label ID="lblErrorMessage" runat="server" Text="Error" Visible="False" 
                        Font-Bold="True" ForeColor="Red"></asp:Label>
                    <br />
                    
                    <table width="100%">
                        <tr>
                            <td style="width: 40%" valign="top">
                                <b>Source review</b><br />
                                <asp:DropDownList ID="ddlSourceReview" runat="server" AutoPostBack="True" 
                                    DataTextField="REVIEW_NAME" DataValueField="REVIEW_ID" 
                                    onselectedindexchanged="ddlSourceReview_SelectedIndexChanged" Width="300px">
                                </asp:DropDownList>
                                <br />
                            </td>
                            <td valign="middle" align="center">
                                &nbsp;</td>
                            <td style="width: 40%" valign="top">
                                <b>Destination review</b><br />
                                <asp:DropDownList ID="ddlDestinationReview" runat="server" AutoPostBack="True" 
                                    DataTextField="REVIEW_NAME" DataValueField="REVIEW_ID" 
                                    onselectedindexchanged="ddlDestinationReview_SelectedIndexChanged" 
                                    Width="300px">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 40%" valign="top">
                                Codesets in this source review

                                 
                               
        <telerik:RadTreeView ID="radTVSourceCodesets" runat="server"
                OnNodeExpand="radTVSourceCodesets_NodeExpand" 
                onnodecollapse="radTVSourceCodesets_NodeCollapse" CssClass="TreeView" 
                 onclientnodeclicked="SelectedNode" Skin="Sunset" >          
        </telerik:RadTreeView>
    <!--OnClientNodeClicked="SelectedSourceRootNode"-->
                                 
                            </td>
                            <td align="center" valign="top">
                                <br />
                                <asp:Button ID="cmdCopyPersonalCodeSet" runat="server" 
                                    Text="Copy &gt;" Width="70px" Enabled="False" 
                                    onclick="cmdCopyPersonalCodeSet_Click" />
                                <br />
                                <asp:Button ID="cmdLinkPersonalCodeSet" runat="server" Enabled="False" Text="Link &gt;" 
                                    Width="70px" Visible="False" />
                                <br />
                                <asp:Label ID="lblLinkPersonalCodeset" runat="server" 
                                    Text="Only ER4 admins can link codesets" Visible="False"></asp:Label>
                                <br />
                            </td>
                            <td style="width: 40%" valign="top" rowspan="3">
                                Code sets in this destination review
                                

                                
                                <telerik:RadTreeView ID="radTVDestinationCodesets" runat="server" 
                                    CssClass="TreeView" 
                                    onnodecollapse="radTVDestinationCodesets_NodeCollapse" 
                                    OnNodeExpand="radTVDestinationCodesets_NodeExpand" Skin="Sunset">
                                </telerik:RadTreeView>

                                
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td style="border-bottom: solid 1px black; width: 40%" valign="top">
                                &nbsp;</td>
                            <td align="center" valign="top" style="border-bottom: solid 1px black;">
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td style="width: 40%" valign="top">
                                <br />
                                <b>Public access code sets</b>
                               
                                <telerik:RadTreeView ID="radTVPublicCodesets" runat="server" 
                                    CssClass="TreeView" 
                                    OnClientNodeClicked="SelectPublicRootNode" 
                                    onnodecollapse="radTVPublicCodesets_NodeCollapse" 
                                    OnNodeExpand="radTVPublicCodesets_NodeExpand" Skin="Sunset">
                                </telerik:RadTreeView>
                                <br />
                                If there are codesets you would like to be made<br /> publicly available please 
                                let us know.</td>
                            <td align="center" valign="top">
                                <br />
                                <br />
                                <asp:Button ID="cmdCopyPublicCodeSet" runat="server" Enabled="False" 
                                    onclick="cmdCopyPublicCodeSet_Click" Text="Copy &gt;" Width="70px" />
                                <br />
                                <asp:Button ID="cmdLinkPublicCodeSet" runat="server" Enabled="False" 
                                    Text="Link &gt;" Visible="False" 
                                    Width="70px" />
                                <br />
                                <asp:Label ID="lblLinkPublicCodeset" runat="server" 
                                    Text="Only ER4 admins can link codesets" Visible="False"></asp:Label>
                                <br />
                            </td>
                        </tr>
                    </table>
                    <br />
                </asp:Panel>
                <br />
            </asp:Panel>
            <br />
            <b>Shareable reviews you have purchased or have admin rights to</b>
                <asp:Label ID="lblShareableReviews" runat="server" 
                    Text="You have not purchased any shareable reviews." Visible="False"></asp:Label>
            <br />
            <asp:GridView 
                    ID="gvReview" runat="server" 
                    AutoGenerateColumns="False" Width="800px" 
                     DataKeyNames="REVIEW_ID" 
                EnableModelValidation="True">
                    <Columns>
                        <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                    </Columns>
                </asp:GridView>
            <br />
            <br />
                <b>Your non-shareable reviews</b>&nbsp;
                <asp:Label ID="lblNonShareableReviews" runat="server" 
                    Text="You do not have any non-shareable reviews." Visible="False"></asp:Label>
                <asp:GridView ID="gvReviewNonShareable" runat="server" 
                    AutoGenerateColumns="False" Width="800px" 
                DataKeyNames="REVIEW_ID" EnableModelValidation="True">
                    <Columns>
                        <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                        <HeaderStyle BackColor="#B6C6D6"  />
                        </asp:BoundField>
                        <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                        <HeaderStyle BackColor="#B6C6D6"  />
                        </asp:BoundField>
                    </Columns>
                </asp:GridView>
            <br />
            Public codeset review:
            <asp:Label ID="lblPublicCodesetReviewID" runat="server" Text="0"></asp:Label>
            <br />
            <br />
            <br />

            </div>
</asp:Content>
