<%@ Control Language="C#" %>

<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>


<ComponentArt:Dialog
  ID="FindDialog"
  RunAt="server"
  AllowDrag="true"
  AllowResize="false"
  Modal="false"
  Alignment="MiddleCentre"
  Width="430"
  Height="126"
  ContentCssClass="dlg-find tabbed">
  <ClientEvents>
    <OnShow EventHandler="FindDialog_OnShow" />
    <OnClose EventHandler="FindDialog_OnClose" />
  </ClientEvents>
  <Content>
    <div class="ttl" onmousedown="<%=FindDialog.ClientID%>.StartDrag(event);">
      <div class="ttlt">
        <div class="ttlt-l"></div>
        <div class="ttlt-m">
          <a class="close" href="javascript:void(0);" onclick="<%=FindDialog.ClientID%>.close();this.blur();return false;"></a>
          <span>Find &amp; Replace</span>
        </div>
        <div class="ttlt-r"></div>
      </div>
      <div class="ttlb">
        <div class="ttlb-l"><span></span></div>
        <div class="ttlb-m"><span></span></div>
        <div class="ttlb-r"><span></span></div>
      </div>
    </div>

    <div class="ts">
      <div class="ts-l"></div>
      <div class="ts-m">
        <a href="javascript:void(0);" class="tab-sel" onclick="toggle_dialog_tab(this,<%=FindDialog.ClientID%>,0,'tab-sel','tab');this.blur();" title="Find"><img src="<%=this.Attributes["SkinFolderLocation"]%>/images/_blank.png" width="16" height="16" alt="Find icon" class="icon find" /><span class="image">Find</span></a>
        <a href="javascript:void(0);" class="tab" onclick="toggle_dialog_tab(this,<%=FindDialog.ClientID%>,1,'tab-sel','tab');this.blur();" title="Replace"><img src="<%=this.Attributes["SkinFolderLocation"]%>/images/_blank.png" width="16" height="16" alt="Replace icon" class="icon replace" /><span class="image">Replace</span></a>
      </div>
      <div class="ts-r"></div>
    </div>

    <div class="con">
      <div class="con-l find"></div>
      <div class="con-m find">

        <ComponentArt:MultiPage ID="FindPages" RunAt="server">
          <ComponentArt:PageView ID="PageView1" RunAt="server">
            <div class="fields">
              <div class="row top"><span class="label">Find what:</span><input id="<%=this.ClientID%>_find_text" type="text" class="find-text" NAME="find_text"/></div>

              <div class="row">
                <span class="label dir">Direction:</span>
                <a class="rad find-up" href="javascript:void(0);" onclick="toggle_radio(this,'rad-sel','rad');this.blur();return false;">Up</a>
                <a class="rad-sel find-down" href="javascript:void(0);" onclick="toggle_radio(this,'rad-sel','rad');this.blur();return false;">Down</a>
              </div>

              <div class="row widgets"><a id="<%=this.ClientID%>_find_matchwholeword" class="chk find-match-word" href="javascript:void(0);" onclick="toggle_checkbox(this,'chk-sel','chk');this.blur();return false;">Match whole word only</a></div>
              <div class="row widgets"><a id="<%=this.ClientID%>_find_matchcase" class="chk find-match-case" href="javascript:void(0);" onclick="toggle_checkbox(this,'chk-sel','chk');this.blur();return false;">Match case</a></div>
            </div>

            <div class="btns">
              <a onclick="FindDialog_Find(<%=FindDialog.ClientID%>);this.blur();return false;" href="javascript:void(0);" class="btn105 float-right"><span>Find Next</span></a>
            </div>
          </ComponentArt:PageView>

          <ComponentArt:PageView ID="PageView2" RunAt="server">
          <div class="fields">
            <div class="row top"><span class="label">Find what:</span><input id="<%=this.ClientID%>_replace_text" type="text" class="replace-find-text" NAME="replace_text"/></div>
            <div class="row"><span class="label">Replace with:</span><input id="<%=this.ClientID%>_replace_replacetext" type="text" class="replace-text" NAME="replace_replacetext"/></div>

            <div class="row">
              <span class="label" style="">Direction:</span>
              <a class="rad replace-up" href="javascript:void(0);" onclick="toggle_radio(this,'rad-sel','rad');this.blur();return false;">Up</a>
              <a class="rad-sel replace-down" href="javascript:void(0);" onclick="toggle_radio(this,'rad-sel','rad');this.blur();return false;">Down</a>
            </div>

            <div class="row widgets"><a id="<%=this.ClientID%>_replace_matchwholeword" class="chk replace-match-word" href="javascript:void(0);" onclick="toggle_checkbox(this,'chk-sel','chk');this.blur();return false;">Match whole word only</a></div>
            <div class="row widgets"><a id="<%=this.ClientID%>_replace_matchcase" class="chk replace-match-case" href="javascript:void(0);" onclick="toggle_checkbox(this,'chk-sel','chk');this.blur();return false;">Match case</a></div>
          </div>

          <div class="btns">
            <a onclick="FindDialog_FindReplace(<%=FindDialog.ClientID%>);this.blur();return false;" href="javascript:void(0);" class="btn105"><span>Find Next</span></a>
            <a onclick="FindDialog_Replace(<%=FindDialog.ClientID%>);this.blur();return false;" href="javascript:void(0);" class="btn105"><span>Replace</span></a>
            <a onclick="FindDialog_ReplaceAll(<%=FindDialog.ClientID%>);this.blur();return false;" href="javascript:void(0);" class="btn105"><span>Replace All</span></a>
          </div>
          </ComponentArt:PageView>
        </ComponentArt:MultiPage>

      </div>
      <div class="con-r find"></div>
    </div>

    <div class="ftr">
      <div class="ftr-l"></div>
      <div class="ftr-m">
        <a onclick="<%=FindDialog.ClientID%>.close();this.blur();return false;" href="javascript:void(0);" class="btn70 f-r"><span>Close</span></a>
      </div>
      <div class="ftr-r"></div>
    </div>
  </Content>
</ComponentArt:Dialog>