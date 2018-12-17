<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="Emails.aspx.cs" Inherits="Emails" Title="Emails" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

       <script type="text/javascript">

           function openEditEmail(emailID) {
               var iWidthOfWin = 780;
               var iHeightOfWin = 450;
               var iLocX = (screen.width - iWidthOfWin) / 2;
               var iLocY = (screen.height - iHeightOfWin) / 3;

               var strFeatures = "scrollbars=yes"
			           + ",width=" + iWidthOfWin
				       + ",height=" + iHeightOfWin
					   + ",screenX=" + iLocX
			            + ",screenY=" + iLocY
				       + ",left=" + iLocX
					   + ",top=" + iLocY;

               var theURL = "EmailEdit.aspx?EmailID=" + emailID;

               windowName = new String(Math.round(Math.random() * 100000));
               DetailsWindow = window.open(theURL, windowName, strFeatures);

           }
			
		</script>



        <br />
        <asp:GridView ID="gvEmails" runat="server" AutoGenerateColumns="False" 
                                    EnableModelValidation="True" Width="99%" 
            DataKeyNames="EMAIL_ID" onrowcommand="gvEmails_RowCommand" 
            onrowdatabound="gvEmails_RowDataBound">
                                    <Columns>
                                        <asp:BoundField DataField="EMAIL_ID" HeaderText="Email ID">
                                        <HeaderStyle BackColor="#B6C6D6" />
                                        <ItemStyle Width="20%" />
                                        </asp:BoundField>
                                        <asp:BoundField DataField="EMAIL_NAME" HeaderText="Email name">
                                        <HeaderStyle BackColor="#B6C6D6" />
                                        <ItemStyle Width="60%" />
                                        </asp:BoundField>
                                        <asp:TemplateField HeaderText="View/Edit">
                                            <ItemTemplate>
                                                <asp:HyperLink ID="hlEditEmail" runat="server">view/edit</asp:HyperLink>
                                            </ItemTemplate>
                                            <HeaderStyle BackColor="#B6C6D6" />
                                            <ItemStyle Width="20%" />
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>

        <br />
        Note: The emails contain variables that match the code in Utils.cs so they must 
        not be changed. You can recogise the variables as<br />
        they end in the word &#39;Here&#39;. For example FullNameHere, PasswordHere, etc.<br />
        <br />
        <br />
    <br />

        
        <script language="javascript" type="text/javascript">
// <![CDATA[

            function TextArea1_onclick() {

            }

// ]]>
        </script>
</asp:Content>



