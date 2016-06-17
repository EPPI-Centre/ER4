<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<ComponentArt:Menu
  ID="UndoMenu"
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
    <ComponentArt:MenuItem ClientTemplateId="UndoTemplate" />
  </Items>
  <ClientTemplates>
    <ComponentArt:ClientTemplate ID="UndoTemplate">
      <div class="undo-list">
        <span>Undo</span>
        <div class="list">## UndoMenuHtml(Parent); ##</div>
        <div class="undo-footer"></div>
      </div>
    </ComponentArt:ClientTemplate>
  </ClientTemplates>
</ComponentArt:Menu>

