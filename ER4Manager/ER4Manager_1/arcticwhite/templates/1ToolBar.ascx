<%@ Control Language="C#" ClassName="EditorTemplate" %>       
        
<div class="wrapper" style="width:735px;height:330px;">
  <div class="tb" style="width:100%;height:35px;">
    <div class="tb-l" style="width:4px;height:100%;"></div>
    <div class="tb-m" style="width:727px;height:auto;">

      <div class="tb-c" style="width:auto;height:25px;">
        $$ToolBars[0]$$
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
