## **Zotero Getting Started from ErWeb for developers**

1.  The first step is to create a Zotero account at:
   [www.zotero.org](http://www.zotero.org)
   
2. Next register the ERWeb Application at:
   [www.zotero.org/oauth/apps](https://www.zotero.org/oauth/apps)
  
![Picture1](ReadmeImages/readmePic1.jpg?raw=true "Pic1")

3. After clicking register new Application as shown in the blue link above; the application should be setup as follows:

![Picture1](ReadmeImages/readmePic2.png?raw=true "Pic2")

⇒ Three fields should be filled in: (_The port number shown in the below URLs must be the same as in your localhost ERWeb application – in production these URLs need to be changed_)
 
 - [ ] Application Name: **_ERWeb_**
          
 - [ ]  Application Website: **_http://localhost:55910/Main_**
          
 - [ ] Callback URL:   [**_http://localhost:55910/api/Zotero/OauthVerifyGet_**](http://localhost:55910/api/Zotero/OauthVerifyGet)


4. Next click save to store application settings in Zotero.

5. Next, open the Zotero branch in Visual Studio and run the sql scripts; then build the project and set ERWeb as the startup project before pressing F5 to run.

6. Run ERWeb ensuring that the port number on localhost is the same as the one used in the settings URLs inside the Zotero application settings as set up in step 3, below the port number is 55910:

![Picture3](ReadmeImages/readmePic3.png?raw=true "Pic3")

7. After running ERWeb as normal; Click on the Zotero button added to the UI:

![Picture4](ReadmeImages/readmePic4.png?raw=true "Pic4")

8. Follow the authorisation instructions:

![Picture5](ReadmeImages/readmePic5.png?raw=true "Pic5")

9. Clicking okay will reveal another notification before sending you to Zotero

![Picture6](ReadmeImages/readmePic6.png?raw=true "Pic6")

10. Clicking okay will being you to the Zotero.org homepage

![Picture7](ReadmeImages/readmePic7.png?raw=true "Pic7")

11. Log into Zotero with your username and password; if not register for a free account and go back to step 1 above.

![Picture8](ReadmeImages/readmePic8.png?raw=true "Pic8")

12. Ensure that you do not accept default permissions as shown by the first button but click the second button and change permissions as shown in the next diagram for example:
 
![Picture9](ReadmeImages/readmePic9.png?raw=true "Pic9")

13. Click the Save Key.

14. Attempt to push items already in your review or add items to Zotero and pull these items (this part should be self explanatory) when you return to ERWeb:  

![Picture10](ReadmeImages/readmePic10.png?raw=true "Pic10")
