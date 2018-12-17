<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<script runat="server">
  protected void Page_Load(object sender, EventArgs e)
  {
    ComboBoxFontFace.DropImageUrl = PrefixWithSkinFolderLocation(ComboBoxFontFace.DropImageUrl);
    ComboBoxFontFace.DropHoverImageUrl = PrefixWithSkinFolderLocation(ComboBoxFontFace.DropHoverImageUrl);
  }
  private string PrefixWithSkinFolderLocation(string str)
  {
    string prefix = this.Attributes["SkinFolderLocation"];
    return str.IndexOf(prefix) == 0 ? str : prefix + "/" + str;
  }
</script>

<ComponentArt:ComboBox
  ID="ComboBoxFontFace"
  ChildControlId="ComboBoxFontFace"
  RunAt="server"
  Width="120"
  Height="24"
  ItemCssClass="menu-item"
  ItemHoverCssClass="menu-item-hover"
  CssClass="combobox"
  HoverCssClass="combobox-hover"
  FocusedCssClass="combobox-hover"
  TextBoxCssClass="combobox-textfield"
  TextBoxHoverCssClass="combobox-textfield-hover"
  DropImageUrl="images/combobox/menu.png"
  DropHoverImageUrl="images/combobox/menu-hover.png"
  DropImageWidth="17"
  DropImageHeight="24"
  KeyboardEnabled="false"
  TextBoxEnabled="false"
  DropDownResizingMode="bottom"
  DropDownWidth="170"
  DropDownHeight="160"
  DropDownCssClass="menu"
  DropDownContentCssClass="menu-content"
  SelectedIndex="6"
  >
  <DropDownFooter>
    <div class="menu-footer"></div>
  </DropDownFooter>

  <Items>
    <ComponentArt:ComboBoxItem Text="Arial" Value="arial" ClientTemplateId="FontFaceItemTemplate" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="menu-break" />
    <ComponentArt:ComboBoxItem Text="Arial Black" Value="'arial black'" ClientTemplateId="FontFaceItemTemplate" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="menu-break" />
    <ComponentArt:ComboBoxItem Text="Comic Sans MS" Value="'comic sans ms'" ClientTemplateId="FontFaceItemTemplate" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="menu-break" />
    <ComponentArt:ComboBoxItem Text="Courier New" Value="courier new" ClientTemplateId="FontFaceItemTemplate" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="menu-break" />
    <ComponentArt:ComboBoxItem Text="Garamond" Value="garamond" ClientTemplateId="FontFaceItemTemplate" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="menu-break" />
    <ComponentArt:ComboBoxItem Text="Georgia" Value="georgia" ClientTemplateId="FontFaceItemTemplate" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="menu-break" />
    <ComponentArt:ComboBoxItem Text="Tahoma" Value="tahoma" ClientTemplateId="FontFaceItemTemplate" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="menu-break" />
    <ComponentArt:ComboBoxItem Text="Times New Roman" Value="'times new roman'" ClientTemplateId="FontFaceItemTemplate" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="menu-break" />
    <ComponentArt:ComboBoxItem Text="Trebuchet MS" Value="'trebuchet ms'" ClientTemplateId="FontFaceItemTemplate" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="menu-break" />
    <ComponentArt:ComboBoxItem Text="Verdana" Value="verdana" ClientTemplateId="FontFaceItemTemplate" />
  </Items>

  <ClientTemplates>
    <ComponentArt:ClientTemplate ID="FontFaceItemTemplate">
      <img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/font/icon.png" width="12" height="15" class="menu-icon" />
      <span style="float:left;font-family:## DataItem.get_value() ##;font-size:11px;vertical-align:middle;line-height:30px;font-weight:normal;">## DataItem.get_text() ##</span>
    </ComponentArt:ClientTemplate>
  </ClientTemplates>
</ComponentArt:ComboBox>
