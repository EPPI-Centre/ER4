<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>


<ComponentArt:Menu
  ID="InsertSymbolMenu"
  RunAt="server"
  ContextMenu="custom"
  CssClass="symbol-menu"
  CollapseDuration="0"
  ShadowEnabled="false">
  
  <ClientEvents>
    <ContextMenuShow EventHandler="reset_symbol_title" />
    <ItemMouseOut EventHandler="reset_symbol_title" />
  </ClientEvents>

  <Items>
    <ComponentArt:MenuItem ClientTemplateId="InsertSymbolTemplate" />
  </Items>

  <ClientTemplates>
    <ComponentArt:ClientTemplate ID="InsertSymbolTemplate">
      <div id="##Parent.ClientControlId##_symbol-title" class="symboltitle"><span>Insert Symbol</span></div>
      <div id="##Parent.ClientControlId##_insert-symbol" class="insertsymbol">
        <div class="symbol-row"><a  href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Euro">&euro;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Pound">&pound;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Yen">&yen;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Copyright">&copy;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Registered trademark">&reg;</a></div>
        <div class="symbol-row"><a  href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Trademark">&#153;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Plus or minus">&plusmn;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Not equal to">&ne;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Less-than or equal to">&le;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Greater-than or equal to">&ge;</a></div>
        <div class="symbol-row"><a  href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Division">&divide;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Multiplication">&times;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Infinity">&infin;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Summation">&sum;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Integral">&int;</a></div>
        <div class="symbol-row"><a  href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Lower-case Mu">&mu;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Lower-case Alpha">&alpha;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Lower-case Beta">&beta;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Lower-case Pi">&pi;</a><a
                                    href="javascript:void(0);" onclick="insert_symbol(##Parent.ClientControlId##,this);" onmouseover="update_symbol_title(##Parent.ClientControlId##,this);" title="Upper-case Omega">&Omega;</a></div>
      </div>
      <div class="symbol-footer"></div>
    </ComponentArt:ClientTemplate>
  </ClientTemplates>

</ComponentArt:Menu>

<script type="text/javascript">

<%=InsertSymbolMenu.ClientID %>.ParentControlID = "<%=this.ClientID %>";

</script>