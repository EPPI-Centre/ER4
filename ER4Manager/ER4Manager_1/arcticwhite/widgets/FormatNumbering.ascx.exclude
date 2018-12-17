<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<ComponentArt:Menu
  ID="NumberingMenu"
  RunAt="server"
  ContextMenu="custom"
  Orientation="Vertical"
  CssClass="bullets-menu"
  TopGroupExpandDirection="BelowLeft"
  TopGroupItemSpacing="2"
  ShadowEnabled="false">
  <Items>
    <ComponentArt:MenuItem ClientTemplateId="NumberingTemplate" ListCssClass="ol1" ListCssClassDepth="3" />
    <ComponentArt:MenuItem Enabled="false" Look-CssClass="bullets-break" />
    <ComponentArt:MenuItem ClientTemplateId="NumberingTemplate" ListCssClass="ol2" ListCssClassDepth="2" />
    <ComponentArt:MenuItem Enabled="false" Look-CssClass="bullets-break" />
    <ComponentArt:MenuItem ClientTemplateId="NumberingTemplate" ListCssClass="ol3" ListCssClassDepth="5" />
    <ComponentArt:MenuItem Enabled="false" Look-CssClass="bullets-break" />
    <ComponentArt:MenuItem ClientTemplateId="NumberingTemplate" ListCssClass="ol4" ListCssClassDepth="5" />
    <ComponentArt:MenuItem Enabled="false" Look-CssClass="bullets-break" />
    <ComponentArt:MenuItem ClientTemplateId="NumberingTemplate" ListCssClass="ol5" ListCssClassDepth="3" />
  </Items>
  <ClientTemplates>
    <ComponentArt:ClientTemplate ID="NumberingTemplate">## ListMenuItemHtml('ol', DataItem.getProperty("ListCssClass"), DataItem.getProperty("ListCssClassDepth")) ##</ComponentArt:ClientTemplate>
  </ClientTemplates>
  <ClientEvents>
    <ItemSelect EventHandler="handleNumberingMenuItemSelect" />
  </ClientEvents>
</ComponentArt:Menu>
