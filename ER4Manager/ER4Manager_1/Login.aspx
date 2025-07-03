<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>EPPI-Reviewer manager</title>
    <link href="MainStyleSheet.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
    function disableMe()
    {
        var button = document.getElementById('<%=cmdCreate.ClientID%>')
        button.disabled = 'true';
    }
    </script>
        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css" integrity="sha384-Gn5384xqQ1aoWXA+058RXPxPg6fy4IWvTNh0E263XmFcJlSAwiGgFAW/dAiS6JXm" crossorigin="anonymous"/>
        <script type="text/javascript" src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
        <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js" integrity="sha384-ApNbgh9B+Y1QKtv3Rn7W3mgPxhU9K/ScQsAP7hUibX39j7fakFPskvXusvfa0b4Q" crossorigin="anonymous"></script>
        <script type="text/javascript" src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" integrity="sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl" crossorigin="anonymous"></script>


    <style type="text/css">
        .style6
        {
            width: 200px;
        }
        .style8
        {
        }
        .style10
        {
            width: 177px;
        }
        .style11
        {
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Panel ID="pnlTop" runat="server" Visible="False">
            <table style="padding-right: 0px; padding-left: 0px; padding-bottom: 0px; margin: 0px;
            padding-top: 0px" width="100%">
                <tr>
                    <td style="width: 20%; color: #4544af; font-style: italic">
                        <strong>EPPI-Reviewer manager</strong></td>
                    <td align="center" style="font-size: large; width: 60%; color: #4544af">
                        <asp:Label ID="lblTitle" runat="server" Text="Login"></asp:Label>
                    </td>
                    <td align="right" style="width: 20%"> 
                        <br />
                        &nbsp;</td>
                </tr>
            </table>
            <br />
            <br />
        </asp:Panel>
        <asp:Panel ID="pnlChoose" runat="server">
            <!--<b>Welcome to the EPPI Reviewer account and review manager.</b><br />
            <br />
            The account and review manager will allow you to create and manage your 
            EPPI Reviewer accounts and reviews.
            <br />
            <br />-->
            <table ID="Table1" border="1" cellpadding="0" cellspacing="0" width="100%">
                <tr>
                    <td>

                            <asp:Label ID="lblMOTD" runat="server" Text="motd"></asp:Label>

                    </td>
                </tr>
            </table>
            <br />
            <!-- this layout is for a 844px wide iframe -->
            <table  cellpadding="1" cellspacing="0" width="100%">
                <tr>
                    <td style="width: 16%;"></td>
                    <td style="text-align:center;width: 30%;">
                        <asp:Button ID="cmdGoToLoginScreenNew" runat="server" class="btn btn-success"
                                onclick="cmdGoToLoginScreen_Click" Text="Login" /><br />
                        <asp:Label ID="lblAccessNew" runat="server" Font-Bold="False"  
                                Text="Manage your account and reviews"></asp:Label>
                    </td>
                    <td style="width: 4%;"></td>
                    <td style="text-align:center;width: 30%;">
                        <asp:Button ID="cmdNewAccountScreen0New" runat="server" class="btn btn-primary"
                            onclick="cmdNewAccountScreen_Click" Text="Create account" Enabled="False" /><br />
                        <asp:Label ID="lblNewAccountNew" runat="server" Font-Bold="False" 
                             Text="Create a new account"></asp:Label>
                    </td>
                    <td style="width: 20%;"></td>
                </tr>
            </table>
            <hr />
            <table  cellpadding="1" cellspacing="0" width="100%">
                <tr>
                    <td style="text-align:center;width: 30%;">
                        <asp:Button ID="cmdForgottenPw" runat="server" onclick="lnkbtForgottenpw_Click" 
                                   Text="Reset password"></asp:Button>
                    </td>
                    <td style="text-align:center;width: 37%;">
                        <asp:Button ID="cmdForgottenUname" runat="server" onclick="lnkbtForgottenUname_Click" 
                                   Text="Retrieve username" ></asp:Button>
                    </td>
                    <td style="text-align:center;width: 34%;">
                        <asp:Button ID="cmdForgottenToActivate" runat="server" onclick="lnkbtForgottenToActivate_Click" 
                                   Text="Resend an activation link"></asp:Button>
                    </td>
                </tr>
             </table>
            <!--<table ID="Table4" border="1" cellpadding="1" cellspacing="0" width="100%">
                <tr>
                    <td style="background-color: #99ccff; background-image: url('Images/Web2strip3.gif');">
                        
                            <span style="color: #FFFFFF">If you already have an EPPI-Reviewer account please click on <b>Login</b>.</span><br />
                            <br />
                            <asp:Button ID="cmdGoToLoginScreen" runat="server" 
                                onclick="cmdGoToLoginScreen_Click" Text="Login" />
                            &nbsp;
                            <asp:Label ID="lblAccess" runat="server" Font-Bold="False" ForeColor="White" 
                                Text="Access an existing account"></asp:Label>
                            &nbsp;<br />
                    </td>
                </tr>
                <tr>
                    <td style="background-color: #E2E9EF; ">
                        
                            <asp:LinkButton ID="lnkbtForgottenPw" runat="server" onclick="lnkbtForgottenpw_Click" 
                                   >Forgot your Password?</asp:LinkButton>&nbsp
                                   <asp:LinkButton ID="lnkbtForgottenUname" runat="server" onclick="lnkbtForgottenUname_Click" 
                                   >Forgot your Username?</asp:LinkButton>&nbsp
                                   <asp:LinkButton ID="lnkbtForgottenToActivate" runat="server" onclick="lnkbtForgottenToActivate_Click" 
                                   >Need to activate your account?</asp:LinkButton>&nbsp
                    </td>
                </tr>
            </table>-->
            <asp:Panel ID="pnlForgottenPassword" runat="server" Visible="False" 
                BackColor="#E2E9EF" BorderColor="Black" BorderWidth="1px"><br />
                <b>Forgotten your password?<br />
                </b>Please enter your username and email. If your username and email match our 
                records we will send your password to the specified address.<br />
                <asp:TextBox ID="tbUserName" runat="server" MaxLength="50">Username</asp:TextBox>
                &nbsp;&nbsp;
                <asp:TextBox ID="tbEmail0" runat="server">Email</asp:TextBox>
                &nbsp;&nbsp;
                <asp:Button ID="cmdSendPassword" runat="server" onclick="cmdSendPassword_Click" 
                    Text="Send password" />
                &nbsp;
                <asp:Label ID="lblNoPasswordSent" runat="server" Font-Bold="True" 
                    Text="The Username and password did not match our records" Visible="False" 
                    ForeColor="#FF3300"></asp:Label>
            </asp:Panel>
            <br /><br />
            <!--<table ID="Table5" border="1" cellpadding="1" cellspacing="0" width="100%">
                <tr>
                    <td style="background-image: url('Images/Web2strip3.gif');">

                        <span style="color: #FFFFFF">If you do not have an EPPI-Reviewer account you can create one by clicking on <b>New account</b>.</span><br />
                        <br />
                        <asp:Button ID="cmdNewAccountScreen0" runat="server" 
                            onclick="cmdNewAccountScreen_Click" Text="New account" Enabled="False" />
                        &nbsp;&nbsp;
                        <asp:Label ID="lblNewAccount" runat="server" Font-Bold="False" 
                            ForeColor="White" Text="Create a new account"></asp:Label>
                        &nbsp;<br />
                    </td>
                </tr>
            </table>-->
            <asp:Label ID="lblNewAccountTest" runat="server" Font-Bold="True" 
                Text="YOU ARE NOW TESTING THE NEW ACCOUNT FUNCTION   or   " Visible="False"></asp:Label>
            &nbsp;<asp:LinkButton ID="lbGoToSummary" runat="server" 
                onclick="lbGoToSummary_Click" Visible="False">Go to Summary page</asp:LinkButton>
            <br />
        </asp:Panel>
        <asp:Panel ID="pnlNewAccount" runat="server" Visible="False">
            <br />
            <strong>Please enter your details. All fields are required.</strong><br />
            <br />
             A &#39;Verify and activate&#39; email will be sent to your email address.&nbsp;<br /><br />
             <table ID="Table3" border="1" cellpadding="1" cellspacing="1" width="750">
                <tr>
                    <td style="background-color: #E2E9EF; width: 25%" class="style8">
                        First name</td>
                    <td style="width: 25%; background-color: #E2E9EF;" class="style6">
                        <asp:TextBox ID="tbFirstName" runat="server" CssClass="textbox" MaxLength="50" 
                            Width="90%"></asp:TextBox>
                    </td>
                    <td style="background-color: #E2E9EF; width: 25%">
                        Last name</td>
                    <td style="width: 25%; background-color: #E2E9EF;" class="style10">
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
                    <td style="background-color: #E2E9EF; width: 25%;" class="style8">
                        Email</td>
                    <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                        <asp:TextBox ID="tbNewEmail" runat="server" CssClass="textbox" Width="90%"></asp:TextBox>
                    </td>
                    <td style="background-color: #E2E9EF; width: 25%">
                        Re-enter email</td>
                    <td class="style10" style="width: 25%; background-color: #E2E9EF;">
                        <asp:TextBox ID="tbNewEmailConfirm" runat="server" CssClass="textbox" 
                            Width="90%"></asp:TextBox>
                    </td>
                </tr>
                 <tr>
                     <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                         Password</td>
                     <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                         <asp:TextBox ID="tbNewPassword" runat="server" CssClass="textbox" TextMode="Password" Width="90%"></asp:TextBox>
                     </td>
                     <td style="background-color: #E2E9EF; width: 25%">
                         Re-enter Password</td>
                     <td class="style10" style="width: 25%; background-color: #E2E9EF;">
                         <asp:TextBox ID="tbNewPassword1" runat="server" CssClass="textbox" 
                             Width="90%" TextMode="Password"></asp:TextBox>
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
            <!--</table>-->
            <!--<table ID="Table9" border="1" cellpadding="1" cellspacing="1" width="750">-->
                <!--<tr>
                    <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                        Please tell us about yourself.</td>
                    <td style="background-color: #FFFF00;" colspan="3">
                        We will <strong>not</strong> pass on this information. We just want to know who 
                        is using EPPI-Reviewer to help us direct its development.</td>
                </tr>-->
                <tr>
                    <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                        What is your area of research</td>
                    <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                    <asp:DropDownList ID="ddlAreaOfResearch" class="btn btn-default dropdown-toggle" Width="95%"
                        runat="server" >
                            <asp:ListItem Value="Please select...">Please select...</asp:ListItem>
                            <asp:ListItem Value="Agriculture Sciences">Agriculture Sciences</asp:ListItem>
                            <asp:ListItem Value="Biological Sciences">Biological Sciences</asp:ListItem>
                            <asp:ListItem Value="Computer Sciences">Computer Sciences</asp:ListItem>
                            <asp:ListItem Value="Economics">Economics</asp:ListItem>
                            <asp:ListItem Value="Education">Education</asp:ListItem>
                            <asp:ListItem Value="Engineering">Engineering</asp:ListItem>
                            <asp:ListItem Value="Environmental">Environmental Sciences</asp:ListItem>
                            <asp:ListItem Value="Health Sciences">Health Sciences</asp:ListItem>
                            <asp:ListItem Value="Medical">Medical</asp:ListItem>
                            <asp:ListItem Value="Pharmaceutical">Pharmaceutical</asp:ListItem>
                            <asp:ListItem Value="Policy research">Policy research</asp:ListItem>
                            <asp:ListItem Value="Social Sciences">Social Sciences</asp:ListItem>
                            <asp:ListItem Value="Other (please tell us)">Other (please tell us)</asp:ListItem>
            </asp:DropDownList>
                    </td>
                    <td colspan="2" rowspan="3" style="background-color: #E2E9EF; ">
                        Organisation / further information<br />
                        <asp:TextBox ID="tbDescription" runat="server" CssClass="textbox" Rows="4" 
                            TextMode="MultiLine" Width="95%"></asp:TextBox>
                    </td>
                </tr>
                <!--<tr>
                    <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                        Your position/profession</td>
                    <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                        <asp:DropDownList ID="ddlProfession" runat="server" Height="20px" 
                            Width="95%">
                            <asp:ListItem Value="Please select..." Selected="True">Please select...</asp:ListItem>
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
                </tr>-->
                <!--<tr>
                    <td class="style8" style="background-color: #E2E9EF; width: 25%;">
                        How did you hear about EPPI-Reviewer</td>
                    <td class="style6" style="width: 25%; background-color: #E2E9EF;">
                        <asp:DropDownList ID="ddlHearAboutUs" runat="server" Height="20px" Width="95%">
                            <asp:ListItem Selected="True">Please select</asp:ListItem>
                            <asp:ListItem>From project leader</asp:ListItem>
                            <asp:ListItem>Recommended</asp:ListItem>
                            <asp:ListItem>Read about it </asp:ListItem>
                            <asp:ListItem>Twitter</asp:ListItem>
                            <asp:ListItem>Personal investigation</asp:ListItem>
                            <asp:ListItem>Other (please tell us)</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>-->
            </table>
            <asp:Button ID="cmdCreate" runat="server" CssClass="button" 
                OnClick="cmdCreate_Click" Text="Create" OnClientClick="setTimeout(disableMe, 1);" />
&nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbIncludeExampleReview" runat="server" Enabled="False" 
                Text="Include example review" />
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <br />
                <asp:Label ID="lblMissingFields" runat="server" Font-Bold="True" 
                    Text="Please fill in all of the fields and drop down lists. Apostrophes (') are not allowed in Username and Emails." 
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
            <!--<p>
                <b>Username</b><br />

                The username must be at least 4 characters long and unique. Maximum length is 50 
                characters.</p>-->
            <!--<p>
                <b>Email</b><br />
         
                Must be valid and unique. <br />
            
                If you previously had an EPPI-Reviewer account (version 3 or 4 or Web) your email address might already be registered.<br />
            
                
                If you get a message saying your email address is already in use please contact 
                us at <a href="mailto:EPPISupport@ucl.ac.uk">EPPISupport@ucl.ac.uk</a>
                        <br /><b>Newsletter</b><br />
                    
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
                            Text=" before creating your account." />
                </p>-->
                    <p>                            <asp:LinkButton ID="lbLoginScreen" runat="server" 
                                onclick="lbReturnToLogin_Click">Login screen</asp:LinkButton>
                        </p>
                        <p>
                            &nbsp;</p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
               
                    <p>
                </p>
        </asp:Panel>
        <asp:Panel ID="pnlLogin" runat="server" Visible="False">
            <br />
            Please enter your Username and Password<br />
            <table id="Table2" border="1" cellpadding="1" cellspacing="1" style="width: 294px;
            height: 80px" width="294">
                <tr>
                    <td style="background-color: #E2E9EF">
                        Username</td>
                    <td style="width: 212px">
                        <asp:TextBox ID="tbUserID" runat="server" CssClass="textbox" MaxLength="50" 
                        Width="90%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td style="background-color: #E2E9EF">
                        Password</td>
                    <td style="width: 212px">
                        <asp:TextBox ID="tbPasswd" runat="server" CssClass="textbox"
                        TextMode="Password" Width="90%" MaxLength="50"></asp:TextBox>
                    </td>
                </tr>
            </table>
            <p>
                <asp:Button ID="cmdLogin" runat="server" class="btn btn-success" OnClick="cmdLogin_Click"
                Text="Login" />
                &nbsp; &nbsp;<asp:Label 
                ID="lblOutcome" runat="server" 
                Font-Bold="True" ForeColor="Red"></asp:Label>
            </p>
            <p>
                <asp:Button ID="cmdForgottenPw1" runat="server" onclick="lnkbtForgottenpw_Click" 
                    Text="Reset password"></asp:Button>&nbsp
                <asp:Button ID="cmdForgottenUname1" runat="server" onclick="lnkbtForgottenUname_Click" 
                    Text="Retrieve username"></asp:Button>&nbsp
                <asp:Button ID="cmdForgottenToActivate1" runat="server" onclick="lnkbtForgottenToActivate_Click" 
                    Text="Resend an activation link"></asp:Button>
                <!--<asp:LinkButton ID="lnkbtForgottenPw1" runat="server" onclick="lnkbtForgottenpw_Click" 
                    >Forgot your Password?</asp:LinkButton>-->&nbsp
                <!--<asp:LinkButton ID="lnkbtForgottenUname1" runat="server" onclick="lnkbtForgottenUname_Click" 
                    >Forgot your Username?</asp:LinkButton>-->&nbsp
                <!--<asp:LinkButton ID="lnkbtForgottenToActivate1" runat="server" onclick="lnkbtForgottenToActivate_Click" 
                    >Need to activate your account?</asp:LinkButton>-->&nbsp
            </p>
            <asp:Panel ID="pnlSendPassword" runat="server" Visible="False">
                <b>Forgotten you password?<br />
                </b>Please enter you username and email. If your username and email match our 
                records we will send your password to the specified address.<br />
                <asp:TextBox ID="tbUserID0" runat="server" CssClass="textbox" MaxLength="50">Username</asp:TextBox>
                &nbsp;&nbsp;
                <asp:TextBox ID="tbEmailCheck" runat="server" CssClass="textbox">Email</asp:TextBox>
                &nbsp;&nbsp;
                <asp:Button ID="cmdRetrieve" runat="server" onclick="cmdRetrieve_Click" 
                    Text="Send password" Enabled="False" />
&nbsp;
                <asp:Label ID="lblErrorMessage" runat="server" style="font-weight: 700" 
                    Text="Your username and email do not match our records." Visible="False" 
                    ForeColor="#FF3300"></asp:Label>
                <br />
            </asp:Panel>
            <p>
            </p>
            <p>
                &nbsp;</p>
            <p>
                <asp:LinkButton ID="lbNewAccountScreen" runat="server" 
                    onclick="lbNewAccountScreen_Click">New account screen</asp:LinkButton>
            </p>
        </asp:Panel>
        <asp:Panel ID="pnlLinkCreator" runat="server" Visible="False">
            <asp:Table ID="tableResetPW" runat="server">
                <asp:TableRow runat="server" style="font-weight: 700; color:#FFFFFF;">
                    <asp:TableCell ID="tc00" runat="server" style="background-image: url('Images/Web2strip3.gif'); ">Username</asp:TableCell>
                    <asp:TableCell ID="tcOR" runat="server" >&nbsp;</asp:TableCell>
                    <asp:TableCell ID="tc01" runat="server" style="background-image: url('Images/Web2strip3.gif'); ">Email</asp:TableCell>
                    <asp:TableCell ID="tc2" runat="server" >&nbsp;&nbsp;</asp:TableCell>
                    <asp:TableCell ID="tc02" runat="server" RowSpan="2">
                        <asp:Button ID="btForgottenPwLinkCreate" runat="server" 
                            Text="Reset Password" onclick="btForgottenPwLinkCreate_Click"  />
                        <asp:Button ID="btForgottenUnameEmailCreate" runat="server" 
                             Text="Retrieve Username" onclick="btForgottenUnameEmailCreate_Click" />
                        <asp:Button ID="btForgottenToActivateLinkCreate" runat="server" 
                             Text="Resend Activation Link" 
                            onclick="btForgottenToActivateLinkCreate_Click" />
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow runat="server">
                    <asp:TableCell ID="tc10" runat="server" >
                        <asp:TextBox ID="tbxUnameLinkCreate" runat="server" CssClass="textbox" MaxLength="50"></asp:TextBox>
                    </asp:TableCell  >
                    <asp:TableCell ID="tcOR1" runat="server" >&nbsp;&nbsp;And / Or&nbsp;&nbsp;</asp:TableCell  >
                    <asp:TableCell ID="tc11" runat="server" >
                        <asp:TextBox ID="tbxEmailLinkCreate" runat="server" CssClass="textbox"></asp:TextBox>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            <br />
            <asp:Label ID="lblInstructionsLinkCreate" runat="server" />
            <asp:Label ID="lblResultLinkCreate" runat="server" Visible="false" style="font-weight: 700" ForeColor="#FF3300"/>
            <p>
                <asp:HyperLink runat="server" ID="linkReturnOld"  NavigateUrl="~/Login.aspx" Text="Return" /> 
            </p>
        </asp:Panel>

        <asp:Panel ID="pnlResetPassword" runat="server" Visible="False">
            <asp:Table ID="tableResetPassword" runat="server">
                <asp:TableRow runat="server" style="font-weight: 700; color:#FFFFFF;">
                    <asp:TableCell ID="TableCell1" runat="server" style="background-image: url('Images/Web2strip3.gif'); ">
                        <asp:Label ID="TableCell1Text" runat="server" />
                    </asp:TableCell><asp:TableCell ID="TableCell2" runat="server" >&nbsp;&nbsp;</asp:TableCell><asp:TableCell ID="TableCell4" runat="server" RowSpan="2">
                        <asp:Button ID="btForgottenPasswordLinkCreate" runat="server" Text="Reset Password" onclick="btForgottenPasswordLinkCreate_Click"  />
                    </asp:TableCell></asp:TableRow><asp:TableRow runat="server">
                    <asp:TableCell ID="TableCell3" runat="server" >
                        <asp:TextBox ID="tbxEmailLinkCreateNew" runat="server" CssClass="textbox"></asp:TextBox>
                    </asp:TableCell></asp:TableRow></asp:Table><br /><asp:Label ID="lblInstructionsLinkCreateNew1" runat="server" />
                <asp:LinkButton runat="server" ID="lbChangeToUsername" onclick="lbChangeToUserName_Click" Text="here" >
                </asp:LinkButton><asp:LinkButton runat="server" ID="lbChangeToEmail" onclick="lnkbtForgottenpw_Click" Text="here" >
                    </asp:LinkButton><asp:Label ID="lblInstructionsLinkCreateNew2" runat="server" />
                <asp:Label ID="lblInstructionsLinkCreateNew3" runat="server" /> 
            <asp:Label ID="lblResultLinkCreate1" runat="server" Visible="false" style="font-weight: 700" ForeColor="#FF3300"/>
            <p>
                <asp:HyperLink runat="server" ID="linkReturn"  NavigateUrl="~/Login.aspx" Text="Return" /> 
            </p>
        </asp:Panel>

        <asp:Panel ID="pnlAccountCreated" runat="server" Visible="False">
            <p>Thank you <asp:Label runat="server" ID="lblCreatedFullName"></asp:Label>,</p><p>You have created a new EPPI-Reviewer account. </p><p>    The username is:<b> <asp:Label ID="lblCreatedUsername" runat="server"></asp:Label></b><br />The account email is: <b><asp:Label ID="lblCreatedEmail" runat="server"></asp:Label></b></p><p>A <strong>Verify and activate account</strong> email has been sent to you. In order to use 
                your account you <b>must activate it</b> by clicking the link included in the 
                email (<span class="style11">please check your inbox or your SPAM folders if you 
                cannot find the email</span>). <br />This link will only remain available for a limited amount of time. Once the link 
                expires, you can use the &quot;Need to activate your account?&quot; link in the Account 
                Manager logon page.<br /> <asp:Label ID="lblErrorCreatingExampleReview" runat="server" Visible="false" style="font-weight: 700" ForeColor="#FF3300"/>
            </p>
        </asp:Panel>
        
        <br />
        <br />
    </div>
    </form>
</body>
</html>