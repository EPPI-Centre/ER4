Use ReviewerAdmin
GO

declare @Content nvarchar(max) = '
<b>New license</b><br><br>
(1) Complete the fields marked as * and click <b>Save license</b>.<br>
&nbsp;License types:<br>
&nbsp;&nbsp;<b>Removeable</b> model - license admins can add and remove reviews from the license.<br>
&nbsp;&nbsp;<b>Fixed</b> model - license admins can add but not remove reviews from license.<br>
&nbsp;Once the license is created you can also set the <b>Change review owner</b> option. This allows the<br>
&nbsp;&nbsp;&nbsp;license admin to change the owner of any reviews in the license.<br>
<br>
(2) Click <b>Create / renew package</b> to enter the license package parameters.<br>
&nbsp;Complete the fields marked as * and click <b>Save license details</b>.<br>
&nbsp;<b>Total fee</b> must be evenly divisible by the <b>Number months</b> value.<br>
&nbsp;This package will appear as an <b>Offer</b> in the Packages dropdown menu.<br>
<br>
(3) To activate the license select the <b>Offer</b> package, set the <b>Valid from</b> and <b>Expiry date</b> fields,<br>
&nbsp;choose a <b>Reason for package change</b> and click <b>Save license details</b>.<br>
&nbsp;The package will now appear as the <b>Latest</b> package in the Packages dropdown menu.<br> 
&nbsp;The license admin can now access and populate the license with reviews and user accounts.<br>
<br> 
<b>Edit license</b><br><br>
You can edit any of the paramaters in the license or a license package.<br> 
If changing a license package parameter be sure to select a <b>Reason for package change</b> before clicking <b>Save license details</b>.<br>
If you are changing the license model (<b>Fixed</b> vs <b>Removeable</b>), a panel will appear after clicking <b>Save license</b>. In this panel are details <br>
on how to proceed depending on whether you are converting the license using the <i>Latest package</i> or making the change as part of a <i>Renewal</i>.<br>
Clicking on a <b>move</b> link in the "Reviews in latest package" table will move the review to the "reviews in previous package" table.<br>
<br> 
<b>Renew license</b><br><br>
(1) Click <b>Create / renew package</b> to enter the offer package paramenters.<br>
&nbsp;Complete the fields marked as * and click <b>Save license details</b>.<br>
&nbsp;<b>Total fee</b> must be evenly divisible by the <b>Number months</b>.<br>
&nbsp;This package will appear as an <b>Offer</b> in the Packages dropdown menu.<br>
(2) To activate the offer package, select the <b>Offer</b> package (in the Packages dropdown menu), set the <b>Valid from</b> and <b>Expiry date</b> fields,<br>
&nbsp;choose a <b>Reason for package change</b> and click <b>Save license details</b>.<br>
&nbsp;The package will now appear as the <b>Latest</b> package in the Packages dropdown menu.<br> 
&nbsp;The license admin can now access and populate the license with reviews and user accounts.<br>
<br>
<b>Renewing a license with/without a model change</b><br><br>
When renewing, older reviews are moved depending on the license model.<br>
 1. <b>Fixed</b> to <b>Fixed</b> renewal  - all reviews in the "latest" package move to the "reviews in previous package" table.<br>
 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The "Reviews in latest package" table will be emptied.<br>
 2. <b>Fixed</b> to <b>Removeable</b> renewal- all reviews in the "latest" package move to the "reviews in previous package" table.<br>
 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The "Reviews in latest package" table will be emptied.<br>
 3. <b>Removeable</b> to <b>Removeable</b> renewal- the reviews in the "latest" package (pre-renewal) move to the new "latest" package (post-renewal).<br>
 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If there are fewer slots than reviews a <b>message appears letting the EPPIAdmin know that they may need to remove some reviews</b>.<br>
 4. <b>Removeable</b> to <b>Fixed</b> renewal - move all reviews in "latest" package to "reviews in previous package" table.<br> 
 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "Reviews in latest package" table will be emptied.<br>
 <br>
'
UPDATE TB_MANAGMENT_EMAILS
SET [EMAIL_MESSAGE] = @Content
	WHERE [EMAIL_ID] = 9
GO






