<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<ComponentArt:Menu
  ID="RedoMenu"
  RunAt="server"
  ContextMenu="custom"
  Orientation="Vertical"
  CssClass="menu"
  ExpandDuration="0"
  CollapseDuration="0"
  TopGroupExpandDirection="BelowLeft"
  TopGroupExpandOffsetY="-3"
  ShadowEnabled="false">
  <Items>
    <ComponentArt:MenuItem ClientTemplateId="RedoTemplate" />
  </Items>
  <ClientTemplates>
    <ComponentArt:ClientTemplate ID="RedoTemplate">
      <div class="undo-list">
        <span>Redo</span>
        <div class="list">## RedoMenuHtml(Parent); ##</div>
        <div class="undo-footer"></div>
      </div>
    </ComponentArt:ClientTemplate>
  </ClientTemplates>
</ComponentArt:Menu>
