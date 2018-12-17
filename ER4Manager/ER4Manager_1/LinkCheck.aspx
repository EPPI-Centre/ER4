<%@ Page Language="C#" AutoEventWireup="true" CodeFile="LinkCheck.aspx.cs" Inherits="LinkCheck" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="MainStyleSheet.css" rel="stylesheet" type="text/css" />
    <title>EPPI Reviewer 4: account manager verification page</title>
    <script type="text/javascript">
        function disableMe() {
            var button = document.getElementById('<%=cmdActivateGhost.ClientID%>')
            button.disabled = 'true';
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table width="800" border="0">
        <tr>
        <td>
            <asp:Panel ID="PnlAnyError" Visible="false" runat="server">
                <strong>ERROR</strong>: we are sorry but something went wrong.<br /> This could 
                be because you reached this page by clicking a link that is now expired. For 
                security reasons most of the links we send via email will remain valid for a 
                limited amount of time.
                <br />
                <br />
                If this is the case, please use the
                <a href="http://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2935">Account Manager</a> 
                to re-create the appropriate Email (reset password, activate account, etc).<br /> 
                <br />
                Alternatively, if you reached this error while <em>using</em> this page, please 
                get in touch with <a href="mailto:EPPISupport@ucl.ac.uk">EPPISupport@ucl.ac.uk</a> 
                and briefly describe how you generated the error. We will be eager to help.</asp:Panel>
            <asp:Panel ID="PnlConfirmEmail" Visible="false" runat="server">
                Thank you.<br /> Your email address has been verified and your EPPI-Reviewer 
                account is now ready for use.
                <br />
                You can log onto the main <strong>EPPI-Reviewer 4</strong> program from this 
                address: <a href='http://eppi.ioe.ac.uk/eppireviewer4/'>http://eppi.ioe.ac.uk/eppireviewer4/</a>. <br />
                You may also log onto the <b>Review and Account Manager</b> here: <a href='http://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2935'>Account Manager</a>. <br />
            <asp:Label ID="lblErrorCreatingExampleReview" runat="server" Visible="false" style="font-weight: 700" ForeColor="#FF3300"/>
            </asp:Panel>
            <asp:Panel ID="PnlResetPassword" Visible="false" runat="server">
            <h2>EPPI-Reviewer 4 Password Reset Page.</h2>
            <asp:Label runat="server" ID="lblresetPWinstructions">Please enter your account details below, all fields are required!</asp:Label>
            <asp:Table ID="tableResetPW" runat="server" Width="750px">
                <asp:TableRow ID="TableRow1" runat="server" style="font-weight: 700; color:#FFFFFF;">
                    <asp:TableCell ID="TableCell1" runat="server" style="background-image: url('Images/Web2strip3.gif'); text-align:right; ">&nbsp;Username&nbsp;</asp:TableCell>
                    <asp:TableCell ID="TableCell2" runat="server" style="min-width:170px;" >
                        <asp:TextBox ID="tbxUnamePwReset" runat="server"   width="95%" MaxLength="50"></asp:TextBox>
                    </asp:TableCell  >
                </asp:TableRow>
               <asp:TableRow ID="TableRow3" runat="server" style="font-weight: 700; color:#FFFFFF;">
                    <asp:TableCell ID="TableCell3" runat="server" style="background-image: url('Images/Web2strip3.gif'); text-align:right; ">&nbsp;New Password&nbsp;</asp:TableCell>
                    <asp:TableCell ID="TableCell4" runat="server" style="min-width:170px;" >
                        <asp:TextBox ID="tbxNewPw1" runat="server" width="95%" MaxLength="50" TextMode="Password"></asp:TextBox>
                    </asp:TableCell>
            
                </asp:TableRow>
                <asp:TableRow style="font-weight: 700; color:#FFFFFF;">
                    <asp:TableCell ID="TableCell5" runat="server" style="background-image: url('Images/Web2strip3.gif'); text-align:right; ">&nbsp;Confirm new password&nbsp;</asp:TableCell>
                    <asp:TableCell ID="TableCell6" runat="server" style="min-width:170px;">
                        <asp:TextBox ID="tbxNewPw2" runat="server"  width="95%" TextMode="Password"></asp:TextBox>
                    </asp:TableCell>
                    </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>&nbsp;</asp:TableCell>
                    <asp:TableCell ID="TableCell7" runat="server" style="vertical-align:middle;" >
                        <asp:Button ID="btPasswordReset" runat="server"  style="vertical-align:middle; "
                            Text="Reset Password" onclick="btPasswordReset_Click"   />
                            <asp:CheckBox ID="cbShowResetPassword" runat="server" AutoPostBack="True" 
                             oncheckedchanged="cbShowResetPassword_CheckedChanged" Text="Show Passwords" />
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            <asp:Label runat="server" ID="lblPwResetResult" style="font-weight: 700" Visible="False" 
                            ForeColor="#FF3300" />
                <asp:Panel ID="PnlResetPWfinalMsg" runat="server" Visible="false">
                    You can log on the main <strong>EPPI-Reviewer 4</strong> program from this 
                    address: <a href="http://eppi.ioe.ac.uk/eppireviewer4/">
                    http://eppi.ioe.ac.uk/eppireviewer4/</a>.
                    <br />
                    You may also log on the <b>Review and Account Manager</b> here:
                    <a href="http://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2935">Account Manager</a>.
                    <br />
                </asp:Panel>
            </asp:Panel>
            <asp:Panel ID="PnlActivateGhost" Visible="false" runat="server" BorderWidth="0px">
                 <h2>EPPI-Reviewer 4 Account Activation Page.</h2>

                    Contact ID =
                    <asp:Label ID="lblContactID" runat="server" Text="N/A"></asp:Label>
                <br />
                Your account comes with a 
                    <asp:Label ID="lblCredit" runat="server" Text="N/A"></asp:Label> months subscription. If you activate it now it will expire on 
                    <b><asp:Label ID="lblExpiryForecast" runat="server" Text="N/A"/></b>.
                <br />
                <strong>Please enter your details. All fields are required.</strong><br />
                    <table ID="Table3" border="1" cellpadding="1" cellspacing="1" width="750">
                        <tr>
                            <td class="style8" style="background-color: #E2E9EF; width: 25%">
                                First name</td>
                            <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                                <asp:TextBox ID="tbFirstName" runat="server" CssClass="textbox" MaxLength="50" 
                                    Width="90%"></asp:TextBox>
                            </td>
                            <td style="background-color: #E2E9EF; width: 20%">
                                Last name</td>
                            <td class="style10" style="width: 30%; background-color: #E2E9EF;">
                                <asp:TextBox ID="tbLastName" runat="server" CssClass="textbox" MaxLength="50" 
                                    Width="90%"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                                Username</td>
                            <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                                <asp:TextBox ID="tbNewUserName" runat="server" CssClass="textbox" 
                                    MaxLength="50" Width="90%"></asp:TextBox>
                            </td>
                            <td colspan="2" style="background-color: #E2E9EF; width: 25%;">
                                The username must be at least 4 characters long and unique. Maximum length is 50 
                                characters.<br /> <strong>Remember your username!</strong> You will need it to 
                                login.</td>
                        </tr>
                        <tr>
                            <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                                Email</td>
                            <td class="style6" colspan="3" style="background-color: #E2E9EF;"  >
                                <asp:TextBox ID="tbNewEmail" runat="server" CssClass="textbox" Width="80%"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td style="background-color: #E2E9EF; width: 25%">
                                Re-enter email</td>
                            <td class="style10" colspan="3" style="width: 25%; background-color: #E2E9EF;">
                                <asp:TextBox ID="tbNewEmailConfirm" runat="server" CssClass="textbox" 
                                    Width="80%"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                                Password</td>
                            <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                                <asp:TextBox ID="tbNewPassword" runat="server" CssClass="textbox" 
                                    TextMode="Password" Width="90%"></asp:TextBox>
                            </td>
                            <td style="background-color: #E2E9EF; width: 20%">
                                Re-enter Password</td>
                            <td class="style10" style="width: 30%; background-color: #E2E9EF;">
                                <asp:TextBox ID="tbNewPassword1" runat="server" CssClass="textbox" 
                                    TextMode="Password" Width="90%"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="style8" colspan="4" style="background-color: #E2E9EF; ">
                                Passwords must be at least <strong>8 characters</strong> and contain and at 
                                least one <strong>one lower case</strong> letter, <strong>one upper case letter, 
                                one digit</strong> and no spaces.
                                <asp:CheckBox ID="cbShowPassword" runat="server" AutoPostBack="True" 
                                    oncheckedchanged="cbShowPassword_CheckedChanged" Text="Show Password" />
                            </td>
                        </tr>
                        <tr>
                            <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                                Please tell us about yourself.</td>
                            <td colspan="3" style="background-color: #FFFF00;">
                                We will <strong>not</strong> pass on this information. We just want to know who 
                                is using EPPI-Reviewer to help us direct its development.</td>
                        </tr>
                        <tr>
                            <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                                What is your area of research</td>
                            <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                                <asp:DropDownList ID="ddlAreaOfResearch" runat="server" Height="20px" 
                                    Width="95%">
                                    <asp:ListItem Selected="True" Value="Please select...">Please select...</asp:ListItem>
                                    <asp:ListItem>Agriculture Sciences</asp:ListItem>
                                    <asp:ListItem>Biological Sciences</asp:ListItem>
                                    <asp:ListItem>Computer Sciences</asp:ListItem>
                                    <asp:ListItem Value="Economics">Economics</asp:ListItem>
                                    <asp:ListItem>Education</asp:ListItem>
                                    <asp:ListItem>Engineering</asp:ListItem>
                                    <asp:ListItem>Environmental Sciences</asp:ListItem>
                                    <asp:ListItem>Health Sciences</asp:ListItem>
                                    <asp:ListItem>Medical</asp:ListItem>
                                    <asp:ListItem>Pharmaceutical</asp:ListItem>
                                    <asp:ListItem>Policy research</asp:ListItem>
                                    <asp:ListItem>Social Sciences</asp:ListItem>
                                    <asp:ListItem Value="Other (please tell us)">Other (please tell us)</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td colspan="2" rowspan="3" style="background-color: #E2E9EF; ">
                                Information / Organisation / Institute / University / Company<br />
                                <asp:TextBox ID="tbDescription" runat="server" CssClass="textbox" Rows="4" 
                                    TextMode="MultiLine" Width="95%"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                                Your position/profession</td>
                            <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                                <asp:DropDownList ID="ddlProfession" runat="server" Height="20px" Width="95%">
                                    <asp:ListItem Selected="True" Value="Please select...">Please select...</asp:ListItem>
                                    <asp:ListItem>Administrator</asp:ListItem>
                                    <asp:ListItem Value="Student">Student</asp:ListItem>
                                    <asp:ListItem>University researcher</asp:ListItem>
                                    <asp:ListItem>Government researcher</asp:ListItem>
                                    <asp:ListItem>NGO researcher</asp:ListItem>
                                    <asp:ListItem>Independant researcher</asp:ListItem>
                                    <asp:ListItem>Private industry research</asp:ListItem>
                                    <asp:ListItem>Other (please tell us)</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                                How did you hear about EPPI-Reviewer</td>
                            <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                                <asp:DropDownList ID="ddlHearAboutUs" runat="server" Height="20px" Width="95%">
                                    <asp:ListItem>Please select</asp:ListItem>
                                    <asp:ListItem Selected="True">From project leader</asp:ListItem>
                                    <asp:ListItem>Recommended</asp:ListItem>
                                    <asp:ListItem>Read about it </asp:ListItem>
                                    <asp:ListItem>Twitter</asp:ListItem>
                                    <asp:ListItem>Personal investigation</asp:ListItem>
                                    <asp:ListItem>Other (please tell us)</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                    </table>
                    <asp:Button ID="cmdActivateGhost" runat="server" CssClass="button" 
                        OnClick="cmdActivateGhost_Click" Text="Activate" OnClientClick="setTimeout(disableMe, 1);"/>
                    &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbIncludeExampleReview" runat="server" Enabled="True" 
                        Text="Include example review" Checked="true" />
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    <br />
                    <asp:Label ID="lblMissingFields" runat="server" Font-Bold="True" 
                        Text="Please fill in all of the fields and drop down lists. Apostrophes (') are not allowed in Usernames and Emails." 
                    Visible="False" ForeColor="Red"></asp:Label>
           
                &nbsp;<br />
                <asp:Label ID="lblUsername" runat="server" Font-Bold="True" ForeColor="Red" 
                    Text="Username is already in use. Please select another." Visible="False"></asp:Label>
                <br />
                <asp:Label ID="lblEmailAddress" runat="server" Font-Bold="True" ForeColor="Red" 
                    Text="Email address is already in use. Please select another." Visible="False"></asp:Label>
                &nbsp;<br />
                <asp:Label ID="lblNewPassword" runat="server" Font-Bold="True" 
                    Text="Passwords must be at least 8 characters and contain and at least one one lower case letter, one upper case letter, one digit and no spaces." 
                    Visible="False" ForeColor="Red"></asp:Label>
                <p>
                    <b>Username</b><br />

                    The username must be at least 4 characters long and unique. Maximum length is 50 
                    characters.</p>
                <p>
                    <b>Email</b><br />
         
                    Must be valid and unique. <br />
            
                    If you previously had an EPPI-Reviewer account (version 3 or 4) your email 
                    address might already be registered.<br />
            
            
                    If you get a message saying your email address is already in use please contact 
                    us at <a href="mailto:EPPISupport@ucl.ac.uk">EPPISupport@ucl.ac.uk</a>
                    </p>
                    <p>
                            <b>Newsletter</b><br />
                    
                            We would like you to receive our EPPI-Reviewer newsletter which contains news 
                            about EPPI-Reviewer including new features and helpful tips and tricks. This is 
                            only sent occasionally and we will not spam your inbox. You can view previous 
                            newsletters
                            <asp:HyperLink ID="hlNewsletter" runat="server" Font-Bold="True" 
                                ForeColor="Blue" 
                                NavigateUrl="http://eppi.ioe.ac.uk/cms/Default.aspx?tabid=3330" Target="_blank">here</asp:HyperLink>
                            &nbsp;to see what you would be missing.
                            <br />
                            You can opt out an any time but if you prefer to not receive this useful 
                            information you can uncheck this box
                            <asp:CheckBox ID="cbSendNewsletter" runat="server" Checked="True" 
                                Text=" before activating your account." />
                    </p>

            </asp:Panel>

        </td>
        <td valign="top"><img src="Images/EPPI-ReviewerLogo.png" alt="EPPI-Reviewer Logo" style="vertical-align:top" /></td>
        </tr>
    </table>
    
    
    </div>
    </form>
</body>
</html>
