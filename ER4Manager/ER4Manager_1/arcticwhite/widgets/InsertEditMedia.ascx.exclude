<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<script runat="server">
  protected void Page_Load(object sender, EventArgs e)
  {
    ImageAlignment.DropImageUrl = PrefixWithSkinFolderLocation(ImageAlignment.DropImageUrl);
    ImageAlignment.DropHoverImageUrl = PrefixWithSkinFolderLocation(ImageAlignment.DropHoverImageUrl);
  }
  private string PrefixWithSkinFolderLocation(string str)
  {
    string prefix = this.Attributes["SkinFolderLocation"];
    return str.IndexOf(prefix) == 0 ? str : prefix + "/" + str;
  }
</script>

<ComponentArt:Dialog
  ID="MediaDialog"
  RunAt="server"
  AllowDrag="true"
  AllowResize="false"
  Modal="false"
  Alignment="MiddleCentre"
  Width="600"
  Height="298"
  ContentCssClass="dlg-media tabbed">
  <ClientEvents>
    <OnShow EventHandler="MediaDialog_OnShow" />
    <OnClose EventHandler="MediaDialog_OnClose" />
  </ClientEvents>

  <Content>
    <div class="ttl" onmousedown="<%=MediaDialog.ClientID%>.StartDrag(event);">
      <div class="ttlt">
        <div class="ttlt-l"></div>
        <div class="ttlt-m">
          <a class="close" href="javascript:void(0);" onclick="<%=MediaDialog.ClientID%>.close();this.blur();return false;"></a>
          <span>Insert / Edit Media</span>
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
        <a href="javascript:void(0);" class="tab-sel" onclick="toggle_dialog_tab(this,<%=MediaDialog.ClientID%>,0);this.blur();" title="Edit image details"><img src="<%=this.Attributes["SkinFolderLocation"]%>/images/_blank.png" width="16" height="16" alt="Image icon" class="icon image" /><span class="image">Image</span></a>
      </div>
      <div class="ts-r"></div>
    </div>

    <div class="con">
      <div class="con-l"></div>
      <div class="con-m">

        <ComponentArt:MultiPage Id="MediaPages" RunAt="server">
          <ComponentArt:PageView RunAt="server" id="Image">
            <div class="fields">
              <div class="row top" title="Required"><span class="label">Image URL:</span><input type="text" class="image-url" id="<%=this.ClientID%>_image_url" /><a href="javascript:void(0);" onclick="load_image(this,<%=MediaDialog.ClientID%>);this.blur();" class="preview-url" title="Preview this image"></a></div>

              <div class="row" title="Required"><span class="label">Image Description:</span><input type="text" class="image-desc" id="<%=this.ClientID%>_image_desc" /></div>

              <div class="row" title="Required">
              <span class="label">Width:</span><input type="text" class="image-width" id="<%=this.ClientID%>_image_width" />
              <span class="label-narrow">Height:</span><input type="text" class="image-height float-left" id="<%=this.ClientID%>_image_height" />
              </div>

              <div class="row" title="Optional">
                <span class="label">Alignment:</span>
                <span class="combo">
                  <ComponentArt:ComboBox
                    ID="ImageAlignment"
                    RunAt="server"
                    Width="104"
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
                    SelectedIndex="0">

                    <DropDownFooter><div class="ddn-ftr"></div></DropDownFooter>

                    <Items>
                      <ComponentArt:ComboBoxItem Text="(Default)" Value="" />
                      <ComponentArt:ComboBoxItem Text="Baseline" Value="baseline" />
                      <ComponentArt:ComboBoxItem Text="Top" Value="top" />
                      <ComponentArt:ComboBoxItem Text="Middle" Value="middle" />
                      <ComponentArt:ComboBoxItem Text="Bottom" Value="bottom" />
                      <ComponentArt:ComboBoxItem Text="TextTop" Value="texttop" />
                      <ComponentArt:ComboBoxItem Text="Absolute middle" Value="absmiddle" />
                      <ComponentArt:ComboBoxItem Text="Absolute bottom" Value="absbottom" />
                      <ComponentArt:ComboBoxItem Text="Left" Value="left" />
                      <ComponentArt:ComboBoxItem Text="Right" Value="right" />
                    </Items>
                  </ComponentArt:ComboBox>

                </span>
                <span class="label-narrow">Border:</span><input type="text" class="image-border float-left" id="<%=this.ClientID%>_image_border" />
              </div>

              <div class="hr"><span></span></div>

              <div class="row" title="Optional">
                <span class="label">Horizontal Spacing:</span><input type="text" class="image-hspace" id="<%=this.ClientID%>_image_hspace" />
                <span class="label">Vertical Spacing:</span><input type="text" class="image-vspace float-left" id="<%=this.ClientID%>_image_vspace" />
              </div>

              <div class="row" title="Optional"><span class="label">CSS Class:</span><input id="<%=this.ClientID%>_image_cssclass" type="text" class="image-class" /></div>
              <div class="row" title="Optional"><span class="label">Inline CSS:</span><input id="<%=this.ClientID%>_image_inline" type="text" class="image-inline" /></div>
            </div>

            <div class="image-preview">
              <div class="status">Preview</div>
              <div class="thumbnail"></div>
            </div>
          </ComponentArt:PageView>

          <ComponentArt:PageView ID="PageView1" RunAt="server">
            <div style="width:582px;line-height:216px;text-align:center;">Flash: Coming soon</div>
          </componentArt:PageView>


          <ComponentArt:PageView ID="PageView2" RunAt="server">
            <div style="width:582px;line-height:216px;text-align:center;">Video: Coming soon</div>
          </ComponentArt:PageView>
        </ComponentArt:MultiPage>

      </div>
      <div class="con-r"></div>
    </div>

    <div class="ftr">
      <div class="ftr-l"></div>
      <div class="ftr-m">
        <a onclick="<%=MediaDialog.ClientID%>.close(true);this.blur();return false;" href="javascript:void(0);" class="btn70 f-r"><span>Insert</span></a>
        <a onclick="<%=MediaDialog.ClientID%>.close();this.blur();return false;" href="javascript:void(0);" class="btn70 f-r first"><span>Close</span></a>
      </div>
      <div class="ftr-r"></div>
    </div>
  </Content>
</ComponentArt:Dialog>

<script type="text/javascript">

<%=MediaDialog.ClientID%>.ParentControlID = "<%=this.ClientID%>";

</script>