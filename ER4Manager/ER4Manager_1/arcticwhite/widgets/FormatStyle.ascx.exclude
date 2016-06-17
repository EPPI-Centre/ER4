<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<script runat="server">
  protected void Page_Load(object sender, EventArgs e)
  {
    ComboBoxStyle.DropImageUrl = PrefixWithSkinFolderLocation(ComboBoxStyle.DropImageUrl);
    ComboBoxStyle.DropHoverImageUrl = PrefixWithSkinFolderLocation(ComboBoxStyle.DropHoverImageUrl);
  }
  private string PrefixWithSkinFolderLocation(string str)
  {
    string prefix = this.Attributes["SkinFolderLocation"];
    return str.IndexOf(prefix) == 0 ? str : prefix + "/" + str;
  }
</script>

<ComponentArt:ComboBox
  ID="ComboBoxStyle"
  RunAt="server"
  AutoFilter="false"
  Width="83"
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
  SelectedIndex="2">
  
  <DropDownHeader>
    <div class="menu-header"></div>
  </DropDownHeader>

  <DropDownFooter>
    <div class="menu-footer"></div>
  </DropDownFooter>

  <Items>
    <ComponentArt:ComboBoxItem ClientTemplateId="StyleItemTemplate" Text="Normal" Value="Normal" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="style-break" />
    <ComponentArt:ComboBoxItem ClientTemplateId="StyleItemTemplate" Text="Heading 1" Value="Heading 1" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="style-break" />
    <ComponentArt:ComboBoxItem ClientTemplateId="StyleItemTemplate" Text="Heading 2" Value="Heading 2" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="style-break" />
    <ComponentArt:ComboBoxItem ClientTemplateId="StyleItemTemplate" Text="Heading 3" Value="Heading 3" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="style-break" />
    <ComponentArt:ComboBoxItem ClientTemplateId="StyleItemTemplate" Text="Bold" Value="Bold" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="style-break" />
    <ComponentArt:ComboBoxItem ClientTemplateId="StyleItemTemplate" Text="Italic" Value="Italic" />
    <ComponentArt:ComboBoxItem Enabled="false" CssClass="style-break" />
    <ComponentArt:ComboBoxItem ClientTemplateId="StyleItemTemplate" Text="Bold + Italic" Value="Bold + Italic" />
  </Items>

  <ClientTemplates>
    <ComponentArt:ClientTemplate ID="StyleItemTemplate">## StyleDropDownItemHTML(Parent.ParentEditor,DataItem) ##</ComponentArt:ClientTemplate>
  </ClientTemplates>
</ComponentArt:ComboBox>
