<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<script runat="server">
  protected void Page_Load(object sender, EventArgs e)
  {
    ComboBoxFontSize.DropImageUrl = PrefixWithSkinFolderLocation(ComboBoxFontSize.DropImageUrl);
    ComboBoxFontSize.DropHoverImageUrl = PrefixWithSkinFolderLocation(ComboBoxFontSize.DropHoverImageUrl);
  }
  private string PrefixWithSkinFolderLocation(string str)
  {
    string prefix = this.Attributes["SkinFolderLocation"];
    return str.IndexOf(prefix) == 0 ? str : prefix + "/" + str;
  }
</script>

<ComponentArt:ComboBox
  ID="ComboBoxFontSize"
  RunAt="server"
  Width="53"
  Height="24"
  ItemCssClass="size-item"
  ItemHoverCssClass="size-item-hover"
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
  DropDownWidth="64"
  DropDownHeight="160"
  DropDownCssClass="menu"
  DropDownContentCssClass="size-content"
  SelectedIndex="4"
  >
  <DropDownHeader>
    <div class="size-header"></div>
  </DropDownHeader>

  <DropDownFooter>
    <div class="size-footer"></div>
  </DropDownFooter>

  <Items>
    <ComponentArt:ComboBoxItem Text="8" Value="8px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="9" Value="9px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="10" Value="10px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="11" Value="11px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="12" Value="12px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="14" Value="14px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="16" Value="16px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="18" Value="18px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="20" Value="20px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="22" Value="22px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="24" Value="24px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="26" Value="26px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="28" Value="28px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="36" Value="36px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="48" Value="48px" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="size-break" />
    <ComponentArt:ComboBoxItem Text="72" Value="72px" />
  </Items>
  </ComponentArt:ComboBox>