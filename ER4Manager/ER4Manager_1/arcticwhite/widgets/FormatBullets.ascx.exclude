<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<ComponentArt:Menu
  ID="BulletsMenu"
  RunAt="server"
  ContextMenu="custom"
  Orientation="Vertical"
  CssClass="bullets-menu"
  TopGroupExpandDirection="BelowLeft"
  TopGroupItemSpacing="2"
  ShadowEnabled="false">
  <Items>
    <ComponentArt:MenuItem ClientTemplateId="BulletsTemplate" ListCssClass="ul1" ListCssClassDepth="3" />
    <ComponentArt:MenuItem Enabled="false" Look-CssClass="bullets-break" />
    <ComponentArt:MenuItem ClientTemplateId="BulletsTemplate" ListCssClass="ul2" ListCssClassDepth="3" />
    <ComponentArt:MenuItem Enabled="false" Look-CssClass="bullets-break" />
    <ComponentArt:MenuItem ClientTemplateId="BulletsTemplate" ListCssClass="ul3" ListCssClassDepth="3" />
  </Items>
  <ClientTemplates>
    <ComponentArt:ClientTemplate ID="BulletsTemplate">## ListMenuItemHtml('ul', DataItem.getProperty("ListCssClass"), DataItem.getProperty("ListCssClassDepth")) ##</ComponentArt:ClientTemplate>
  </ClientTemplates>
  <ClientEvents>
    <ItemSelect EventHandler="handleBulletsMenuItemSelect" />
  </ClientEvents>
</ComponentArt:Menu>