<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>

<ComponentArt:Menu
  ID="InsertTableMenu"
  RunAt="server"
  ContextMenu="custom"
  CssClass="insert-table-menu"
  CollapseDuration="0"
  ShadowEnabled="false">
  
  <ClientEvents>
    <ContextMenuShow EventHandler="insert_table_menu_show" />
  </ClientEvents>

  <Items>
    <ComponentArt:MenuItem ClientTemplateId="InsertTableTemplate" />
  </Items>

  <ClientTemplates>
    <ComponentArt:ClientTemplate ID="InsertTableTemplate">
      <div id="##Parent.ClientControlId##_table-title" class="inserttable-title"><span>Insert Table</span></div>
      <div class="insertinsert-table" onmouseover="mouse_over_table_cells(event,'##Parent.ClientControlId##');">
        <div id="##Parent.ClientControlId##_table-cells-highlight" class="inserttable-cells-highlight"></div>
        <div class="inserttable-cells"></div>
        <div id="##Parent.ClientControlId##_table-cells-events" class="inserttable-cells-events" onclick="insert_table(##Parent.ClientControlId##);" onmouseout="mouse_out_table_cells(event,'##Parent.ClientControlId##');" onmousemove="track_table_cells(event,'##Parent.ClientControlId##')"></div>
      </div>
      <div class="insert-table-footer"></div>
    </ComponentArt:ClientTemplate>
  </ClientTemplates>

</ComponentArt:Menu>

<script type="text/javascript">

<%=InsertTableMenu.ClientID %>.ParentControlID = "<%=this.ClientID %>";

</script>