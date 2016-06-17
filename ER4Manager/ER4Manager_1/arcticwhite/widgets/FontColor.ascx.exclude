<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<script runat="server">
  protected void Page_Load(object sender, EventArgs e)
  {
    FontColorMenu.ImagesBaseUrl = PrefixWithSkinFolderLocation(FontColorMenu.ImagesBaseUrl);
  }
  private string PrefixWithSkinFolderLocation(string str)
  {
    string prefix = this.Attributes["SkinFolderLocation"];
    return str.IndexOf(prefix) == 0 ? str : prefix + "/" + str;
  }
</script>

<ComponentArt:Menu
  ID="FontColorMenu"
  RunAt="server"
  ContextMenu="Custom"
  Orientation="Vertical"
  TopGroupExpandDirection="BelowLeft"
  CssClass="color-menu"
  ImagesBaseUrl="images/menus/color/"
  CollapseDuration="0"
  ShadowEnabled="false">
  
  <ItemLooks>
    <ComponentArt:ItemLook LookId="ColorMenuItem" CssClass="color-menu-item" HoverCssClass="color-menu-item-hover" />
    <ComponentArt:ItemLook LookId="ColorMenuFooter" CssClass="color-menu-footer" />
  </ItemLooks>

  <Items>
    <ComponentArt:MenuItem ClientTemplateId="SwatchesTemplate" />
    <ComponentArt:MenuItem ID="AddCustomColor" Text="Add Custom Color..." Look-LeftIconUrl="icon-custom.gif" Look-HoverLeftIconUrl="icon-custom-hover.gif" LookId="ColorMenuItem" ClientSideCommand="open_color_picker();" />
    <ComponentArt:MenuItem LookId="ColorMenuFooter" />
  </Items>

  <ClientEvents>
    <ContextMenuShow EventHandler="color_menu_show" />
    <ContextMenuHide EventHandler="color_menu_hide" />
    <ItemSelect EventHandler="color_menu_select" />
  </ClientEvents>

  <ClientTemplates>
    <ComponentArt:ClientTemplate Id="SwatchesTemplate">
      <div class="swatch-title-preset"><span>Preset Colors</span></div>

      <div><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/preset.gif" width="176" height="92" class="preset-swatches" border="0" /></div>

      <div class="swatch-title"><span>Standard Colors</span></div>
      <div><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/standard.gif" width="176" height="21" class="standard-swatches" border="0" /></div>

      <div class="swatch-title"><span>Custom Colors</span></div>
      <div class="custom-swatches">
        <div class="swatches-container">
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-0" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-1" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-2" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-3" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-4" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-5" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-6" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-7" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-8" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
          <div id="##DataItem.get_parentMenu().get_clientControlId()##_custom-9" class="swatch"><img alt="" src="## Parent.ParentEditor.SkinFolderLocation ##/images/menus/color/custom.gif" width="13" height="13" /></div>
        </div>
      </div>

      <div class="color-selected" id="##DataItem.get_parentMenu().get_clientControlId()##_color-selected">
        <div class="color-highlight" onmousemove="move_swatch_highlight(this,event);" onclick="select_swatch(this,## DataItem.get_parentMenu().get_clientControlId(); ##)"></div>
      </div>

    </ComponentArt:ClientTemplate>
  </ClientTemplates>
</ComponentArt:Menu>

<script type="text/javascript">

<%=FontColorMenu.ClientID %>.ParentControlID = "<%=FontColorMenu.ClientID %>";

</script>