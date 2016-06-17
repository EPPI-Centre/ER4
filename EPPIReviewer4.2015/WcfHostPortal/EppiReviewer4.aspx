<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EppiReviewer4.aspx.cs" Inherits="WcfHostPortal.EppiReviewer4StartPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" style="height:100%;">

<head id="Head1" runat="server">
    <title>EPPI-Reviewer 4.0 beta 2</title>
    <link rel="SHORTCUT ICON" type="image/gif" href="eppi.ico"/>
    <script type="text/javascript">

        function ShowPopup(content) {
            newwindow = window.open('', 'name', 'height=500,width=700,resizable=yes,scrollbars = yes');
            var doc = newwindow.document;
            doc.write('<html><body>');
            doc.write(content);
            doc.write('<p><a href="javascript:self.close()">Close window</a></p></body></html>');
            doc.close();
        }

        function Refresh() {
            window.location.reload()
        }

        function LoadURLNewWindow(url) {
            window.open(url);
        }

        var slCtl = null;
        function pluginLoaded(sender) {
            slCtl = sender.getHost();
        }
        function PassCommand() {
            slCtl.Content.jsb.SignalAuthenticationIsDone();
        }
        function PopUpAndMonitor(url) {
            var win = window.open(url, "_blank");
            var pollTimer = window.setInterval(function () {
                if (win.closed !== false) { // !== is required for compatibility with Opera
                    window.clearInterval(pollTimer);
                    PassCommand();
                }
            }, 200);
        }
    </script>
</head>
<body style="height:100%;margin:0;">
    <form id="form1" runat="server" style="height:100%;">
        
        <div style="height:100%;width:100%; text-align:center;">
            <object data="data:application/x-silverlight-2," id="Xaml1" type="application/x-silverlight-2"
			width="100%" height="100%">
			<param name="source" runat="server" id="idSource" value="ClientBin/EppiReviewer4.xap" />
			<param name="background" value="#FFFFFF" />
            <param name="onLoad" value="pluginLoaded"/>
			<!-- Windowless mode set to false breaks the QSF under Safari -->
			<% if (Request.Browser.IsBrowser("Safari"))
                { Response.Write("<!--param name='Windowless' value='true' /-->"); }
			 %>
			<param name="minRuntimeVersion" value="5.0.61118.0" />
            <param name="autoUpgrade" value="true" />
            <a href="http://www.microsoft.com/getsilverlight/" style="text-decoration:none; text-align:center">
 			  &nbsp;<img src="http://go.microsoft.com/fwlink/?LinkId=161376" align="middle" alt="Get Microsoft Silverlight" style="border-style:none"/> &nbsp;
              
		  </a>
          <table  cellpadding="2" cellspacing="8" style="width:99%; border-width:0px; margin:5px;">
                <tr><th colspan="2" style="text-align:center;margin-left:5%;font-family: verdana; font-size:15px;" >
                    If you have problems installing Silverlight…
                    </th>
               </tr>
               <tr><td colspan="2">
               <table  cellpadding="2" cellspacing="1" style="width:550px; border-width:0px; margin:1px;" align="center">
                <tr><td style="background-color:#44A6dd; color:White; font-weight:bold; font-family: verdana; font-size:15px; text-align:center;">
                On Windows 10:
                </td>
               </tr>
               <tr> 
               <td valign="top" style="vertical-align:top;">
               <div style="text-align:left;margin:2px;font-family: verdana; font-size:11px; vertical-align:top;">
                The new <b>Edge</b> browser does not support Silverlight. However, <b>Internet Explorer is present in all Windows 10 machines</b>:<br />
                1. Type "Internet Explorer" in the Cortana/Search box. <br />
                2. Right click on the "Internet Explorer" result.<br />
                3. To add as a tile on your Start Menu click "Pin to Start." <br />
                4. To keep it on your taskbar click "Pin to taskbar."<br />
                If the above doesn't work, you may need to enable the "Internet Explorer" feature, this is done via <i>"Control panel\Programs\Programs and Features"</i> and "Turn Windows features on or off".
               </div></td>
               </tr>
               </table>
               </td></tr>
               <tr>
               <td style="background-color:#44A6dd; color:White; font-weight:bold; font-family: verdana; font-size:15px; text-align:center;">
               Windows users
               </td>
               <td style="background-color:#44A6dd; color:White; font-weight:bold; font-family: verdana; font-size:15px; text-align:center">
               Mac users
               </td>
               </tr>
               <tr> 
               <td valign="top" style="vertical-align:top;">
                <div style="text-align:left;margin-left:5%;font-family: verdana; font-size:12px; vertical-align:top;"> 
                <ol>
                <li>Go to the <a href="http://www.microsoft.com/getsilverlight/" target="_blank">Get Silverlight</a> page.</li>
                <li>Install Silverlight following the on-screen instructions.</li>
                <li>When you get an "Installation completed" message, shut down (quit) all browser windows. (If in doubt, restart your machine) </li>
                <li>Go back to the <a href="http://www.microsoft.com/getsilverlight/" target="_blank">Get Silverlight</a> page.
                 <ul style="list-style-type:none; margin-left: 0; padding-left: 1em;">
                    <li>- You should now see a "You are ready to use Microsoft Silverlight" message.</li>
                    <li>- If the message is not there please try installing again and then restart your machine afterwards.</li>
                 </ul></li>
                 <li>When the <a href="http://www.microsoft.com/getsilverlight/" target="_blank">Get Silverlight</a> page does show the "You are ready to use Microsoft Silverlight" message, EPPI-Reviewer 4 will work. </li>  
                 
                 </ol>
                 <b>N.B. Silverlight support on Chrome has been disabled.</b><br />
                 On Windows computers, EPPI-Reviewer can be accessed via Internet Explorer and Firefox.<br />
                 </div>       
               </td>
               <td>
                <div style="text-align:left;margin-left:5%;font-family: verdana; font-size:12px; vertical-align:top;">
                <b>N.B. Silverlight is not supported by Chrome on Mac platforms (though does work in Windows).</b><br />
                <ol>
                 <li>Go to the <a href="http://www.microsoft.com/getsilverlight/" target="_blank">Get Silverlight</a> page.</li>
                <li>Install Silverlight following the on-screen instructions.</li>
                <li>When you get an "Installation completed" message, shut down (quit) all browser windows.
                <br /> <b>N.B. closing the visible windows is not enough, you will need to "Quit" Safari and/or Firefox.</b> (If in doubt, restart your machine)
                </li>
                <li>Go back to the <a href="http://www.microsoft.com/getsilverlight/" target="_blank">Get Silverlight</a> page.
                <ul style="list-style-type:none; margin-left: 0; padding-left: 1em;">
                    <li>- You should now see a "You are ready to use Microsoft Silverlight" message.</li>
                    <li>- If the message is not there please try installing again and then restart your machine afterwards.</li>
                </ul></li>
                <li>When the <a href="http://www.microsoft.com/getsilverlight/" target="_blank">Get Silverlight</a> page shows the
                         "You are ready to use Microsoft Silverlight" message, EPPI-Reviewer 4 will work.</li>
                </ol>
                </div>
               </td>
               </tr>
         
                </table>
		</object>
        </div>
    </form>
</body>
</html>