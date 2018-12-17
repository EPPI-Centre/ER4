<%@ Control Language="C#" ClassName="EditorTemplate" %>

<%--
  Editor wrapper template (CSS class = .wrapper): This specifically-sized DIV contains the ToolBar, Editor Area and Status Bar DIVs as follows:

  *********************************
  * ToolBar (.tb)
    - Left (.tb-l): Left edge
    - Middle (.tb-m): The ToolBar goes here
    - Right (.tb-r): Right edge

  * Editor (.e)
    - Top (.et)
      > Left (.et-l): Top-left corner
      > Middle (.et-m): Top-middle horizontally-tiling edge
        - Toolbar containers (.tb-c): These containe the individual ToolBars; they're assigned an inline width for pixel-perfection
      > Right (.et-r): Top-right corner
    - Middle (.em)
      > Left (.em-l): Left vertically-tiling edge
      > Middle (.em-m): The Editor Area goes here
      > Right (.em-r): Right vertically-tiling edge
    - Bottom (.eb)
      > Left (.eb-l): Bottom-left corner
      > Middle (.eb-m): Bottom-middle horizontally-tiling edge
      > Right (.eb-r): Bottom-right corner

  * Status Bar (.sb)
    - Left (.sb-l): Left edge
    - Middle (.sb-m): The mode-switching buttons [design / source view], breadcrumbs and wordcount area go here
    - Right (.sb-r): Right edge
  *********************************
--%>
<div class="wrapper" style="width:735px;height:330px;">
  <div class="tb" style="width:100%;height:104px;">
    <div class="tb-l" style="width:4px;height:100%;"></div>
    <div class="tb-m" style="width:727px;height:auto;">

      <div class="tb-c" style="width:auto;height:25px;">
        $$ToolBars[0]$$
      </div>
      <div class="tb-c" style="width:auto;height:25px;">
        $$ToolBars[1]$$
      </div>
      <div class="tb-c" style="width:auto;height:25px;">
        $$ToolBars[2]$$
      </div>

    </div>
    <div class="tb-r" style="width:4px;height:100%;"></div>
  </div>

  <div class="e" style="width:100%;height:269px;">
    <div class="et" style="width:100%;height:8px;">
      <div class="et-l" style="width:9px;height:8px;"><span></span></div>
      <div class="et-m" style="width:717px;height:8px;"><span></span></div>
      <div class="et-r" style="width:9px;height:8px;"><span></span></div>
    </div>

    <div class="em" style="width:100%;height:256px;">
      <div class="em-l" style="width:9px;height:100%;"></div>
      <div class="em-m" style="width:717px;height:100%;">$$editorarea$$</div>
      <div class="em-r" style="width:9px;height:100%;"></div>
    </div>

    <div class="eb" style="width:100%;height:5px;">
      <div class="eb-l" style="width:9px;height:5px;"><span></span></div>
      <div class="eb-m" style="width:717px;height:5px;"><span></span></div>
      <div class="eb-r" style="width:9px;height:5px;"><span></span></div>
    </div>
  </div>

  <div class="sb" style="width:100%;height:31px;">
    <div class="sb-l" style="width:9px;height:31px;"></div>
    <div class="sb-m" style="width:717px;height:31px;">
      <div class="mode">
        <a onclick="$$parent$$.DesignMode();select_statusbar_button(this);this.blur();return false;" href="javascript:void(0);" class="button-selected"><span><img src="<%= this.Attributes["SkinFolderLocation"] %>/images/_blank.png" style="display:block;" width="16" height="16" alt="Design Mode" class="icon design" />Design</span></a>
        <a onclick="$$parent$$.SourceMode();select_statusbar_button(this);this.blur();return false;" href="javascript:void(0);" class="button"         ><span><img src="<%= this.Attributes["SkinFolderLocation"] %>/images/_blank.png" style="display:block;" width="16" height="16" alt="HTML Mode"   class="icon html"   />HTML</span></a>
      </div>

      <div class="path">
        <div class="path-l">Path:</div>
        <div class="path-m">$$breadcrumbs$$</div>
        <div class="path-r"></div>
      </div>

      <div class="count">
        <div class="words" title="Word Count">
          <div class="lbl">Words:</div>
          <div class="val">$$wordcount$$</div>
        </div>
        <div class="chars" title="Character Count">
          <div class="lbl">Chars:</div>
          <div class="val">$$charactercount$$</div>
        </div>
      </div>
    </div>
    <div class="sb-r" style="width:9px;height:31px;"></div>
  </div>
</div>
<%-- /Wrapper --%>