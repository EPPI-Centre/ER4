<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<script runat="server">
  protected void Page_Load(object sender, EventArgs e)
  {
    LinkAnchor.DropImageUrl = PrefixWithSkinFolderLocation(LinkAnchor.DropImageUrl);
    LinkAnchor.DropHoverImageUrl = PrefixWithSkinFolderLocation(LinkAnchor.DropHoverImageUrl);
    LinkType.DropImageUrl = PrefixWithSkinFolderLocation(LinkType.DropImageUrl);
    LinkType.DropHoverImageUrl = PrefixWithSkinFolderLocation(LinkType.DropHoverImageUrl);
    LinkCssClass.DropImageUrl = PrefixWithSkinFolderLocation(LinkCssClass.DropImageUrl);
    LinkCssClass.DropHoverImageUrl = PrefixWithSkinFolderLocation(LinkCssClass.DropHoverImageUrl);
    EmailCssClass.DropImageUrl = PrefixWithSkinFolderLocation(EmailCssClass.DropImageUrl);
    EmailCssClass.DropHoverImageUrl = PrefixWithSkinFolderLocation(EmailCssClass.DropHoverImageUrl);
  }
  private string PrefixWithSkinFolderLocation(string str)
  {
    string prefix = this.Attributes["SkinFolderLocation"];
    return str.IndexOf(prefix) == 0 ? str : prefix + "/" + str;
  }
</script>

<ComponentArt:Dialog
  ID="HyperlinkDialog"
  RunAt="server"
  AllowDrag="true"
  AllowResize="false"
  Modal="false"
  Alignment="MiddleCentre"
  Width="442"
  Height="155"
  ContentCssClass="dlg-link tabbed">
  <ClientEvents>
    <OnShow EventHandler="HyperlinkDialog_OnShow" />
    <OnClose EventHandler="HyperlinkDialog_OnClose" />
  </ClientEvents>
  <Content>

    <div class="ttl" onmousedown="<%=HyperlinkDialog.ClientID%>.StartDrag(event);">
      <div class="ttlt">
        <div class="ttlt-l"></div>
        <div class="ttlt-m">
          <a class="close" href="javascript:void(0);" onclick="<%=HyperlinkDialog.ClientID%>.close();this.blur();return false;"></a>
          <span>Insert / Edit Hyperlink</span>
        </div>
        <div class="ttlt-r"></div>
      </div>
      <div class="ttlb">
        <div class="ttlb-l"><span></span></div>
        <div class="ttlb-m"><span></span></div>
        <div class="ttlb-r"><span></span></div>
      </div>
    </div>

    <div class="ts">
      <div class="ts-l"></div>
      <div class="ts-m">
        <a href="javascript:void(0);" class="tab-sel" onclick="toggle_dialog_tab(this,<%=HyperlinkDialog.ClientID%>,0,'tab-sel','tab');this.blur();" title="Edit link details"><img src="<%=this.Attributes["SkinFolderLocation"]%>/images/_blank.png" width="16" height="16" alt="Link icon" class="icon link" /><span class="link">Link</span></a>
        <a href="javascript:void(0);" class="tab" onclick="toggle_dialog_tab(this,<%=HyperlinkDialog.ClientID%>,1,'tab-sel','tab');this.blur();" title="Edit anchor details"><img src="<%=this.Attributes["SkinFolderLocation"]%>/images/_blank.png" width="16" height="16" alt="Anchor icon" class="icon anchor" /><span class="anchor">Anchor</span></a>
        <a href="javascript:void(0);" class="tab" onclick="toggle_dialog_tab(this,<%=HyperlinkDialog.ClientID%>,2,'tab-sel','tab');this.blur();" title="Edit email details"><img src="<%=this.Attributes["SkinFolderLocation"]%>/images/_blank.png" width="16" height="16" alt="Email icon" class="icon email" /><span class="email">Email</span></a>
      </div>
      <div class="ts-r"></div>
    </div>

    <div class="con">
      <div class="con-l link-basic"></div>
      <div class="con-m link-basic">

      <ComponentArt:MultiPage ID="HyperlinkPages" RunAt="server">
        <ComponentArt:PageView ID="Link" RunAt="server">
          <div class="row top" title="Required">
            <span class="label">Link URL:</span><input id="<%=this.ClientID%>_link_url" type="text" class="link-url" /><a href="javascript:void(0);" onclick="check_url(this);this.blur();" class="preview-url" title="Preview URL in new window"></a>
          </div>

          <div class="row">
            <span class="label">Link Text:</span><input type="text" id="<%=this.ClientID%>_link_text" class="link-text" />
            <span class="label-narrow" title="Optional">Target:</span><input id="<%=this.ClientID%>_link_target" type="text" class="link-target f-l" title="Optional" />
          </div>

          <div class="hr"><span style="display:none;">.</span></div>

          <div class="row">
            <span class="label" title="Optional">Tooltip:</span><input id="<%=this.ClientID%>_link_tooltip" type="text" class="link-tooltip" disabled="disabled" />
            <span class="label-narrow">Anchor:</span>
            <span class="combo">
              <ComponentArt:ComboBox
                ID="LinkAnchor"
                RunAt="server"
                Width="121"
                Height="18"
                ItemCssClass="ddn-item"
                ItemHoverCssClass="ddn-item-hover"
                CssClass="dlg-combobox"
                TextBoxCssClass="txt"
                DropImageUrl="images/ddn.png"
                DropHoverImageUrl="images/ddn-hover.png"
                DropDownResizingMode="bottom"
                DropDownWidth="190"
                DropDownHeight="160"
                DropDownCssClass="ddn"
                DropDownContentCssClass="ddn-con"
                SelectedIndex="6"
                Enabled="false">
                <DropDownFooter><div class="ddn-ftr"><span></span></div></DropDownFooter>
                <Items>
                  <ComponentArt:ComboBoxItem Text="anchor #1" />
                </Items>
              </ComponentArt:ComboBox>
            </span>
          </div>

          <div class="row">
            <span class="label" title="Select a protocol for the hyperlink">Type:</span>
            <span class="combo">
              <ComponentArt:ComboBox
                ID="LinkType"
                RunAt="server"
                Width="121"
                Height="18"
                ItemCssClass="ddn-item"
                ItemHoverCssClass="ddn-item-hover"
                CssClass="dlg-combobox"
                TextBoxCssClass="txt cur"
                DropImageUrl="images/ddn.png"
                DropHoverImageUrl="images/ddn-hover.png"
                KeyboardEnabled="false"
                TextBoxEnabled="false"
                DropDownResizingMode="bottom"
                DropDownWidth="190"
                DropDownHeight="160"
                DropDownCssClass="ddn"
                DropDownContentCssClass="ddn-con"
                ItemClientTemplateId="LinkTypeItemTemplate"
                SelectedIndex="3"
                Enabled="false">
                <ClientEvents>
                  <Change EventHandler="link_type_change" />
                </ClientEvents>

              <DropDownFooter><div class="ddn-ftr"><span></span></div></DropDownFooter>

              <Items>
                <ComponentArt:ComboBoxItem Text="(Other)" Value="" />
                <ComponentArt:ComboBoxItem Text="Local File" Value="file://" />
                <ComponentArt:ComboBoxItem Text="FTP" Value="ftp://" />
                <ComponentArt:ComboBoxItem Text="HTTP" Value="http://" />
                <ComponentArt:ComboBoxItem Text="HTTP (Secure)" Value="https://" />
                <ComponentArt:ComboBoxItem Text="Javascript" Value="javascript:" />
                <ComponentArt:ComboBoxItem Text="Telnet" Value="telnet:" />
                <ComponentArt:ComboBoxItem Text="Usenet" Value="news:" />
              </Items>

              <ClientTemplates>
                <ComponentArt:ClientTemplate ID="LinkTypeItemTemplate">
                            <span title="## (DataItem.get_text() == "(Other)") ? "Other: Please specify" : DataItem.get_text() + " (" + DataItem.get_value() + ")" ##">## DataItem.get_text() ##</span>
                </ComponentArt:ClientTemplate>
              </ClientTemplates>
            </ComponentArt:ComboBox>
            </span>
            <input type="text" id="<%=this.ClientID%>_link_type_other" class="link-type-other f-l disabled" disabled="disabled" />
          </div>

          <div class="row" title="Optional">
            <span class="label">CSS Class:</span>
            <span class="combo">
              <ComponentArt:ComboBox
                ID="LinkCssClass"
                RunAt="server"
                Width="121"
                Height="18"
                ItemCssClass="ddn-item"
                ItemHoverCssClass="ddn-item-hover"
                CssClass="dlg-combobox"
                TextBoxCssClass="txt cur"
                DropImageUrl="images/ddn.png"
                DropHoverImageUrl="images/ddn-hover.png"
                KeyboardEnabled="false"
                TextBoxEnabled="false"
                DropDownResizingMode="bottom"
                DropDownWidth="190"
                DropDownHeight="160"
                DropDownCssClass="ddn"
                DropDownContentCssClass="ddn-con">
                <DropDownFooter><div class="ddn-ftr"><span></span></div></DropDownFooter>

                <Items>
                  <ComponentArt:ComboBoxItem Text="css class 1" Value="cssClass1" />
                  <ComponentArt:ComboBoxItem Text="css class 2" Value="cssClass2" />
                  <ComponentArt:ComboBoxItem Text="css class 3" Value="cssClass3" />
                  <ComponentArt:ComboBoxItem Text="css class 4" Value="cssClass4" />
                  <ComponentArt:ComboBoxItem Text="css class 5" Value="cssClass5" />
                  <ComponentArt:ComboBoxItem Text="css class 6" Value="cssClass6" />
                </Items>
              </ComponentArt:ComboBox>
            </span>
          </div>
        </ComponentArt:PageView>

        <ComponentArt:PageView ID="Anchor" RunAt="server">
          <div class="row top"><span class="label">Name:</span><input type="text" id="<%=this.ClientID%>_anchor_name" class="anchor-name" /></div>
          <div class="row"><span class="label">Text:</span><input type="text" id="<%=this.ClientID%>_anchor_text" class="anchor-text" /></div>
        </ComponentArt:PageView>

        <ComponentArt:PageView  id="Email" RunAt="server">
          <div class="row top"><span class="label">Address:</span><input type="text" id="<%=this.ClientID%>_email_address" class="email-address" /></div>
          <div class="row"><span class="label">Subject:</span><input type="text" id="<%=this.ClientID%>_email_subject" class="email-subject" /></div>
          <div class="row" title="Optional">
            <span class="label">Link Text:</span><input type="text" id="<%=this.ClientID%>_email_text" class="email-text" />
            <span class="label-narrow">Class:</span>
            <span class="combo">
              <ComponentArt:ComboBox
                ID="EmailCssClass"
                RunAt="server"
                Width="121"
                Height="18"
                ItemCssClass="ddn-item"
                ItemHoverCssClass="ddn-item-hover"
                CssClass="dlg-combobox"
                TextBoxCssClass="txt cur"
                DropImageUrl="images/ddn.png"
                DropHoverImageUrl="images/ddn-hover.png"
                KeyboardEnabled="false"
                TextBoxEnabled="false"
                DropDownResizingMode="bottom"
                DropDownWidth="190"
                DropDownHeight="160"
                DropDownCssClass="ddn"
                DropDownContentCssClass="ddn-con">
                <DropDownFooter><div class="ddn-ftr"></div></DropDownFooter>

                <Items>
                  <ComponentArt:ComboBoxItem Text="css class 1" Value="cssClass1" />
                  <ComponentArt:ComboBoxItem Text="css class 2" Value="cssClass2" />
                  <ComponentArt:ComboBoxItem Text="css class 3" Value="cssClass3" />
                  <ComponentArt:ComboBoxItem Text="css class 4" Value="cssClass4" />
                  <ComponentArt:ComboBoxItem Text="css class 5" Value="cssClass5" />
                  <ComponentArt:ComboBoxItem Text="css class 6" Value="cssClass6" />
                </Items>
              </ComponentArt:ComboBox>
            </span>
          </div>
        </ComponentArt:PageView>
      </ComponentArt:MultiPage>
      </div>
      <div class="con-r link-basic"></div>
    </div>

    <div class="ftr">
      <div class="ftr-l"></div>
      <div class="ftr-m">
        <a onclick="toggle_hyperlink_options(this,<%=HyperlinkDialog.ClientID%>);this.blur();return false;" href="javascript:void(0);" id="<%=this.ClientID%>_hyperlink-options" class="btn70 f-l"><span class="advanced">Advanced</span></a>
        <a onclick="<%=HyperlinkDialog.ClientID%>.close(true);this.blur();return false;" href="javascript:void(0);" class="btn70 f-r"><span>Insert</span></a>
        <a onclick="<%=HyperlinkDialog.ClientID%>.close();this.blur();return false;" href="javascript:void(0);" class="btn70 f-r first"><span>Close</span></a>
      </div>
      <div class="ftr-r"></div>
    </div>
  </Content>
</ComponentArt:Dialog>












<script type="text/javascript">

<%=HyperlinkDialog.ClientID %>.ParentControlID = "<%=this.ClientID %>";

function ComboBoxInit(combobox)
{
    combobox.ParentControlID = "<%=this.ClientID %>";
}

</script>