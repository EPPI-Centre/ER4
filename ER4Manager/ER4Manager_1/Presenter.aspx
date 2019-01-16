<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Presenter.aspx.cs" Inherits="Presenter" Title="Data viewer" %>
    
    <%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

     <script type="text/javascript">

         function openEditIntro(area, webDB_ID) {
             var iWidthOfWin = 800;
             var iHeightOfWin = 650;
             var iLocX = (screen.width - iWidthOfWin) / 2;
             var iLocY = (screen.height - iHeightOfWin) / 3;

             var strFeatures = "scrollbars=yes"
			           + ",width=" + iWidthOfWin
				       + ",height=" + iHeightOfWin
					   + ",screenX=" + iLocX
			            + ",screenY=" + iLocY
				       + ",left=" + iLocX
					   + ",top=" + iLocY
                       + ",resizable=no";

             var theURL = "PresenterEditIntro.aspx?Area=" + area;

             windowName = new String(Math.round(Math.random() * 100000));
             DetailsWindow = window.open(theURL, windowName, strFeatures);

         }

         function openEditHeaderImage(area, webDB_ID) {
             var iWidthOfWin = 800;
             var iHeightOfWin = 700;
             var iLocX = (screen.width - iWidthOfWin) / 2;
             var iLocY = (screen.height - iHeightOfWin) / 3;

             var strFeatures = "scrollbars=yes"
			           + ",width=" + iWidthOfWin
				       + ",height=" + iHeightOfWin
					   + ",screenX=" + iLocX
			            + ",screenY=" + iLocY
				       + ",left=" + iLocX
					   + ",top=" + iLocY
                       + ",resizable=no";

             var theURL = "PresenterEditHeaderImage.aspx?Area=" + area + "&WebDB_ID=" + webDB_ID; ;

             windowName = new String(Math.round(Math.random() * 100000));
             DetailsWindow = window.open(theURL, windowName, strFeatures);

         }




        function SelectedNode(sender, eventArgs) // Used in crosstabs.aspx
        {
            // we want the root node to be selected at all times
            var tree1 = $find("<%= RadTVReviewGuidelines.ClientID %>");
            var node = eventArgs.get_node();
            var s = node.get_text();
            var currentObject = node.get_parent();
            while (currentObject != null) 
            {
                // get_parent() will return null when we reach the treeview
                if (currentObject.get_parent() != null) 
                {
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
                BorderStyle="Solid" BorderWidth="1px" Visible="False" BackColor="#CCFFFF">
                &nbsp;Review:
                <asp:Label ID="lblReviewName" runat="server" Text="Review name" 
                    Font-Bold="True"></asp:Label>
                <br />
                &nbsp;Review Id:
                <asp:Label ID="lblReviewID" runat="server" Font-Bold="True" Text="Review ID"></asp:Label>
                <br />
                &nbsp;Webdatabase:
                <asp:DropDownList ID="ddlWebDatabases" runat="server" AutoPostBack="True" 
                    DataTextField="WEBDB_NAME" DataValueField="WEBDB_ID" 
                    onselectedindexchanged="ddlWebDatabases_SelectedIndexChanged" 
                    Width="500px">
                </asp:DropDownList>
                &nbsp;&nbsp;&nbsp;&nbsp;
                <asp:LinkButton ID="lbAddWebDatabase" runat="server" 
                    onclick="lbAddWebDatabase_Click">Create a web database</asp:LinkButton>
                
                &nbsp;&nbsp;
                <asp:LinkButton ID="lbDelete" runat="server" onclick="lbDelete_Click">Delete</asp:LinkButton>
                
                <asp:Panel ID="pnlNewWebdatabase" runat="server" Visible="False" 
                    ForeColor="Black" BackColor="#FFFFCC">
                    <asp:TextBox ID="tbNewWebDatabase0" runat="server" Width="300px" validateRequest="false">Enter new web database name</asp:TextBox>
                    &nbsp;&nbsp;&nbsp;
                    <asp:Button ID="cmdAdd" runat="server" Text="Create" onclick="cmdAdd_Click" />
                    &nbsp;&nbsp;&nbsp;
                    <asp:LinkButton ID="lbCancel" runat="server" onclick="lbCancel_Click">Cancel</asp:LinkButton>
                    &nbsp;
                    <asp:Label ID="lblMessage" runat="server" Font-Bold="True" ForeColor="Red" 
                        Text="This web database already exists" Visible="False"></asp:Label>
                </asp:Panel>
                <br />
                <asp:Panel ID="pnlIntroText" runat="server" BorderColor="Black" 
                    BorderStyle="Solid" BorderWidth="1px" Visible="False" Width="99%" 
                    BackColor="#FFFFCC">
                    <b>&nbsp;Web database name&nbsp;&nbsp;&nbsp;&nbsp;</b>
                    <asp:TextBox ID="tbWebDatabaseName" runat="server" Width="300px"></asp:TextBox>
                    &nbsp;&nbsp;&nbsp;
                    <asp:LinkButton ID="lbSaveWebDatabaseName" runat="server" 
                        onclick="lbSaveWebDatabaseName_Click">Save</asp:LinkButton>
                    &nbsp;&nbsp;
                    <br />
                    <br />
                    &nbsp;<b>Web database url:</b>&nbsp;&nbsp;
                    <asp:HyperLink ID="hlWebDBUrl" runat="server" Target="_blank">N/A</asp:HyperLink>
                    &nbsp;&nbsp; (click to preview)<br />
                    <br />
                    <b>&nbsp;Introduction text:</b>&nbsp;&nbsp;&nbsp;<asp:LinkButton ID="lbViewEditHeader" 
                        runat="server">Enter/Edit</asp:LinkButton>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbAdminEdit" runat="server" 
                        oncheckedchanged="cbAdminEdit_CheckedChanged" Text="Admin edit" 
                        Visible="False" AutoPostBack="True" />
                    &nbsp;&nbsp;&nbsp;<strong>Header image:</strong>&nbsp; &nbsp;<asp:LinkButton ID="lbViewEditHeaderImage" 
                        runat="server">Enter/Edit</asp:LinkButton>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:Label ID="lblReportText" runat="server" Font-Bold="True" 
                        Text="Report text:" Visible="False"></asp:Label>
                    &nbsp;&nbsp;
                    <asp:LinkButton ID="lbViewEditReportIntro" runat="server" Visible="False">View/Edit</asp:LinkButton>
                    <br />
                    <table style="width: 60%; margin-top: 0px;">
                        <tr>
                            <td style="width: 68%">
                                Does access to your web database require logging in?</td>
                            <td style="width: 65%">
                                <asp:RadioButtonList ID="rblRestrictAccess" runat="server" AutoPostBack="True" 
                                    onselectedindexchanged="rblRestrictAccess_SelectedIndexChanged" 
                                    RepeatDirection="Horizontal">
                                    <asp:ListItem Value="1">Yes</asp:ListItem>
                                    <asp:ListItem Selected="True" Value="0">No</asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                    </table>
                    <asp:Panel ID="pnlPassword" runat="server" Visible="False">
                        <asp:TextBox ID="tbUserName" runat="server" MaxLength="50"></asp:TextBox>
                        &nbsp;&nbsp; User name&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:TextBox ID="tbPassword" runat="server" MaxLength="50"></asp:TextBox>
                        &nbsp;&nbsp; Password&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:LinkButton ID="lbSaveLogin" runat="server" onclick="lbSaveLogin_Click">Save login</asp:LinkButton>
                        &nbsp;&nbsp;&nbsp;
                        <asp:Label ID="lblPasswordMessage" runat="server" 
                            Text="Please fill in both fields" Visible="False"></asp:Label>
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; &nbsp;
                    </asp:Panel>
                    <asp:Panel ID="pnlCrosstabs" runat="server">
                        &nbsp;<asp:CheckBox ID="cbDisplaySavedCrosstabs" runat="server" AutoPostBack="True" 
                            oncheckedchanged="cbDisplaySavedCrosstabs_CheckedChanged" 
                            Text="Display saved crosstabs" />
                        &nbsp;&nbsp;&nbsp;
                        <asp:CheckBox ID="cbSaveCrosstabs" runat="server" AutoPostBack="True" 
                            oncheckedchanged="cbSaveCrosstabs_CheckedChanged" 
                            Text="Allow crosstab saving" />
                    </asp:Panel>
                </asp:Panel>
                <asp:Panel ID="pnlChooseCodes" runat="server" BorderColor="Black" 
                    BorderStyle="Solid" BorderWidth="1px" Width="99%" Visible="False" 
                    BackColor="#CCFFCC">
                    <br />
                    
                    <table width="100%">
                        <tr>
                            <td style="width: 40%" valign="top">
                                C<strong>opy codesets into Data viewer</strong>&nbsp;<telerik:RadTreeView 
                                    ID="RadTVReviewGuidelines" runat="server" CssClass="TreeView" 
                                    onnodecollapse="RadTVReviewGuidelines_NodeCollapse" 
                                    OnNodeExpand="RadTVReviewGuidelines_NodeExpand" onclientnodeclicked="SelectedNode" 
                                    Skin="Sunset">
                                </telerik:RadTreeView>
                            </td>
                            <td valign="top" align="center">
                                <br />
                                <asp:Button ID="cmdCopy" runat="server" OnClick="cmdCopy_Click" 
                                    Text="Copy &gt;" Width="80px" />
                                <br />
                                <asp:Button ID="cmdDelete" runat="server" OnClick="cmdDelete_Click" 
                                    Text="&lt; Remove" Width="80px" />
                            </td>
                            <td style="width: 40%" valign="top">
                                <strong>Codesets in Data viewer</strong>
                                <telerik:RadTreeView ID="RadTVWebDatabase" runat="server" CssClass="TreeView" 
                                    onnodecollapse="RadTVWebDatabase_NodeCollapse" 
                                    OnNodeExpand="RadTVWebDatabase_NodeExpand" Skin="Sunset" 
                                    AllowNodeEditing="True" onnodeedit="RadTVWebDatabase_NodeEdit">
                                </telerik:RadTreeView>
                                <asp:CheckBox ID="cbShowCoding" runat="server" AutoPostBack="True" 
                                    Checked="True" oncheckedchanged="cbShowCoding_CheckedChanged" 
                                    Text="Display coding in Data viewer" 
                                    ToolTip="Coding will be displayed on study details page" />
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 40%" valign="top">
                                <strong>Select code to identify Data viewer items<telerik:RadTreeView 
                                    ID="RadTVIncludeCode" runat="server" CssClass="TreeView" 
                                    onnodecollapse="RadTVIncludeCode_NodeCollapse" 
                                    OnNodeExpand="RadTVIncludeCode_NodeExpand" Skin="Sunset">
                                </telerik:RadTreeView>
                                </strong>
                            </td>
                            <td align="center" valign="top">
                                <br />
                                <asp:Button ID="cmdCopyCode" runat="server" OnClick="cmdCopyCode_Click" 
                                    Text="Select &gt;" Width="80px" />
                                <br />
                                <asp:Button ID="cmdDeleteCode" runat="server" OnClick="cmdDeleteCode_Click" 
                                    Text="&lt; Remove" Width="80px" />
                            </td>
                            <td style="width: 40%" valign="top">
                                <strong>Include &#39;completed&#39; studies with this code</strong><br />
                                <div style="border: 1px solid #333333; margin: 2px; padding: 5px; background-color: #E9E9E9">
                                <asp:Label ID="lblIncludeCode" runat="server" 
                                    Text="No code selected (include all 'completed' studies)"></asp:Label>
                                </div>
                            </td>
                        </tr>
                    </table>
                    <br />
                    <b>
                    <asp:LinkButton ID="lbHideCodes" runat="server" onclick="lbHideCodes_Click">Hide</asp:LinkButton>
                    &nbsp;&nbsp;
                    </b>
                    <asp:Label ID="lblNoCopy" runat="server" Font-Bold="True" 
                        Text="This code does not have any child codes" Visible="False"></asp:Label>
                    <br />
                </asp:Panel>
                <br />
            </asp:Panel>
            <br />
            <br />
            <b>Shareable reviews you have purchased or have admin rights to</b>
                <asp:Label ID="lblShareableReviews" runat="server" 
                    Text="You have not purchased any shareable reviews." Visible="False"></asp:Label>
            <br />
            <asp:GridView 
                    ID="gvReview" runat="server" 
                    AutoGenerateColumns="False" Width="800px" onrowcommand="gvReview_RowCommand" 
                     DataKeyNames="REVIEW_ID" 
                EnableModelValidation="True" onrowediting="gvReview_RowEditing" OnRowDataBound="gvReview_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="URL" Visible="False">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server"></asp:Label>
                            </ItemTemplate>
                            <HeaderStyle BackColor="#B6C6D6" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="PASSWORD" HeaderText="Password required" 
                            Visible="False">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:ButtonField CommandName="SELECT" HeaderText="Create / edit web database" 
                            Text="Select">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:ButtonField>
                        <asp:ButtonField CommandName="EDIT" HeaderText="Edit text" Text="Edit" 
                            Visible="False">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
            <br />
            <br />
                <b>Your non-shareable reviews</b>&nbsp;
                <asp:Label ID="lblNonShareableReviews" runat="server" 
                    Text="You do not have any non-shareable reviews." Visible="False"></asp:Label>
                <asp:GridView ID="gvReviewNonShareable" runat="server" 
                    AutoGenerateColumns="False" Width="800px" 
                    onrowcommand="gvReviewNonShareable_RowCommand" 
                DataKeyNames="REVIEW_ID" EnableModelValidation="True" OnRowDataBound="gvReviewNonShareable_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="REVIEW_ID" HeaderText="ReviewID">
                        <HeaderStyle BackColor="#B6C6D6"  />
                        </asp:BoundField>
                        <asp:BoundField DataField="REVIEW_NAME" HeaderText="Name of review">
                        <HeaderStyle BackColor="#B6C6D6"  />
                        </asp:BoundField>
                        <asp:BoundField HeaderText="Password required" Visible="False">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:ButtonField CommandName="SELECT" HeaderText="Create / edit web database" 
                            Text="Select">
                        <HeaderStyle BackColor="#B6C6D6"  />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
                <br />
                <asp:Panel ID="pnlAdmin" runat="server" Visible="False">
                    Enter Review ID
                    <asp:TextBox ID="tbReviewID" runat="server"></asp:TextBox>
                    &nbsp; <b>&nbsp;
                    <asp:LinkButton ID="lbRetrieve" runat="server" onclick="lbRetrieve_Click">Retrieve</asp:LinkButton>
                    </b>
                </asp:Panel>
            <br />
            <br />
            <br />
            <br />
            <br />



    </div>
</asp:Content>
