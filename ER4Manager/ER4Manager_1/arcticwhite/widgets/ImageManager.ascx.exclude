<%@ Control Language="C#" %>
<%@ Register Assembly="ComponentArt.Web.UI" Namespace="ComponentArt.Web.UI" TagPrefix="ComponentArt" %>
<%@ Import Namespace="System.IO" %>

<script runat="server">

    protected string _root;

    private void Page_Load(object sender, System.EventArgs e)
    {
        ImageUpload.FileInputImageUrl = PrefixWithSkinFolderLocation("images/image_manager/_browse.png");
        ImageUpload.FileInputHoverImageUrl = PrefixWithSkinFolderLocation("images/image_manager/_browse-h.png");
        ImageUpload.AllowedFileExtensions = ((ComponentArt.Web.UI.Editor)this.Parent).ImageFileFilter;
        ImageUpload.TempFileFolder = Server.MapPath(((ComponentArt.Web.UI.Editor)this.Parent).UploadTempPath);
        ImageUpload.CallbackParameter = ((ComponentArt.Web.UI.Editor)this.Parent).UploadPath;
        ImageUpload.Uploaded += new Upload.UploadedEventHandler(ImageUpload_OnUploaded);

        FileBrowser1.LineImagesFolderUrl = PrefixWithSkinFolderLocation("images/treeview/lines");
        FileBrowser1.ImagesBaseUrl = PrefixWithSkinFolderLocation("images/treeview");
        FileBrowser2.LineImagesFolderUrl = PrefixWithSkinFolderLocation("images/treeview/lines/");
        FileBrowser2.ImagesBaseUrl = PrefixWithSkinFolderLocation("images/treeview");

        _root = Directory.Exists(Server.MapPath(((ComponentArt.Web.UI.Editor)this.Parent).UploadPath)) ? Server.MapPath(((ComponentArt.Web.UI.Editor)this.Parent).UploadPath) : Server.MapPath("~/");
        string rootFolder = _root;
        ComponentArt.Web.UI.TreeViewNode rootNode = new ComponentArt.Web.UI.TreeViewNode();
        rootNode.Text = "/";
        rootNode.Enabled = false;
        rootNode.ClientTemplateId = "RootTemplate";
        rootNode.ID = "d" + Guid.NewGuid().ToString().Substring(0, 10).Replace("-", "");
        rootNode.ContentCallbackUrl = "XmlFromFileSystem.aspx?dir=" + Server.UrlEncode(_root);
        FileBrowser1.Nodes.Add(rootNode);

        rootNode = new ComponentArt.Web.UI.TreeViewNode();
        rootNode.Text = "/";
        rootNode.Expanded = true;
        rootNode.ID = "d" + Guid.NewGuid().ToString().Substring(0, 10).Replace("-", "");
        FileBrowser2.Nodes.Add(rootNode);
        rootNode.ClientTemplateId = "RootTemplate1";
        BuildDirectory(rootFolder, rootNode, true);
    }

    private void BuildDirectory(string dirPath, ComponentArt.Web.UI.TreeViewNode parentNode, bool path)
    {
        string[] subDirectories = Directory.GetDirectories(dirPath);
        foreach (string directory in subDirectories)
        {
            string[] parts = directory.Split('\\');
            string name = parts[parts.Length - 1];
            ComponentArt.Web.UI.TreeViewNode node = new ComponentArt.Web.UI.TreeViewNode();
            node.Text = name;
            if(!path) node.ClientTemplateId = "FolderTemplate";
            node.ID = "d" + Guid.NewGuid().ToString().Substring(0, 10).Replace("-", "");
            if (path)
            {
                node.ContentCallbackUrl = "XmlFromFileSystem.aspx?path=1&dir=" + Server.UrlEncode(directory);
            }
            else
            {
                node.ContentCallbackUrl = "XmlFromFileSystem.aspx?dir=" + Server.UrlEncode(directory);
            }
            parentNode.Nodes.Add(node);
        }

        if (path) return;

        string[] files = Directory.GetFiles(dirPath);
        foreach (string file in files)
        {
            string allowedextensions = ((ComponentArt.Web.UI.Editor)this.Parent).ImageFileFilter;
            FileInfo fi = new FileInfo(file);

            if (allowedextensions.ToLower().IndexOf(fi.Extension.ToLower().Substring(1)) > -1)
            {
                string[] parts = file.Split('\\');
                string name = parts[parts.Length - 1];
                ComponentArt.Web.UI.TreeViewNode node = new ComponentArt.Web.UI.TreeViewNode();
                node.Text = name;
                node.ID = "f" + Guid.NewGuid().ToString().Substring(0, 10).Replace("-", "");
                node.Value = (fi.Length > 1024) ? (fi.Length / 1024).ToString() : "1";

                switch (fi.Extension.ToLower())
                {
                    case ".gif": node.ImageUrl = "gif.gif"; break;
                    case ".jpg": node.ImageUrl = "jpg.gif"; break;
                    case ".png":
                    case ".bmp":
                    case ".pcx":
                    case ".tiff": node.ImageUrl = "image.gif"; break;
                    default: node.ImageUrl = "file.gif"; break;
                }
                parentNode.Nodes.Add(node);
            }
        }
    }

    private void ImageUpload_OnUploaded(object sender, ComponentArt.Web.UI.UploadUploadedEventArgs args)
    {
        UploadedFileInfoCollection files = args.UploadedFiles;

        foreach (UploadedFileInfo fileInfo in files)
        {
            string path = ImageUpload.CallbackParameter;
            string savePath = Server.MapPath(Path.Combine(path,fileInfo.FileName));

            if (savePath.StartsWith(Server.MapPath(((ComponentArt.Web.UI.Editor)this.Parent).UploadPath)))
            {
                fileInfo.SaveAs(savePath);
            }
        }
    }

    private string PrefixWithSkinFolderLocation(string str)
    {
      string prefix = this.Attributes["SkinFolderLocation"];
      return str.IndexOf(prefix) == 0 ? str : prefix + "/" + str;
    }
  
</script>

<ComponentArt:Dialog
  ID="ImageManagerDialog"
  RunAt="server"
  AllowDrag="true"
  AllowResize="false"
  Modal="false"
  Alignment="MiddleCentre"
  Width="600"
  Height="298"
  ContentCssClass="im_dlg-img">
  <Content>

 <div>
  <div class="im_dlg-img im_tabbed">

    <div class="im_ttl" onmousedown="<%=ImageManagerDialog.ClientID %>.StartDrag(event);">
      <div class="im_ttlt">
        <div class="im_ttlt-l"></div>
        <div class="im_ttlt-m">
          <a class="close" href="javascript:void(0);" onclick="<%=ImageManagerDialog.ClientID %>.close();this.blur();return false;"></a>
          <span>Image Manager</span>
        </div>
        <div class="im_ttlt-r"></div>
      </div>
      <div class="im_ttlb">
        <div class="im_ttlb-l"></div>
        <div class="im_ttlb-m"></div>
        <div class="im_ttlb-r"></div>
      </div>
    </div>

    <div class="im_ts">
      <div class="im_ts-l"></div>
      <div class="im_ts-m">
        <a href="javascript:void(0);" class="tab-sel" onclick="<%=ImageManagerPages.ClientID %>.setPageIndex(0);change_tabs(this,'tab-sel','tab');this.blur();return false;" title="Browse">
          <span class="l"></span>
          <span class="m"><img src="<%=this.Attributes["SkinFolderLocation"] %>/images/_blank.png" width="16" height="16" alt="Browse Icon" class="icon browse" /><h4>Browse</h4></span>
          <span class="r"></span>
        </a>

        <a href="javascript:void(0);" class="tab" onclick="<%=ImageManagerPages.ClientID %>.setPageIndex(1);change_tabs(this,'tab-sel','tab');this.blur();return false;" title="Properties">
          <span class="l"></span>
          <span class="m"><img src="<%=this.Attributes["SkinFolderLocation"] %>/images/_blank.png" width="16" height="16" alt="Browse Icon" class="icon props" /><h4>Properties</h4></span>
          <span class="r"></span>
        </a>

        <a href="javascript:void(0);" class="tab" onclick="<%=ImageManagerPages.ClientID %>.setPageIndex(2);change_tabs(this,'tab-sel','tab');this.blur();return false;" title="Upload">
          <span class="l"></span>
          <span class="m"><img src="<%=this.Attributes["SkinFolderLocation"] %>/images/_blank.png" width="16" height="16" alt="Upload Icon" class="icon upload" /><h4>Upload</h4></span>
          <span class="r"></span>
        </a>
      </div>
      <div class="im_ts-r"></div>
    </div>

    <div class="im_con">
      <div class="im_con-m">

<ComponentArt:MultiPage Id="ImageManagerPages" RunAt="server" CssClass="mp">
  <ComponentArt:PageView Id="BrowsePage" RunAt="server" CssClass="pv">

    <div class="im_browse-l">
      <div class="tb">
        <div class="btns">
          <a href="javascript:void(0);" onclick="this.blur();return false;" class="reload"></a>
          <div class="sep"></div>
          <a href="javascript:void(0);" onclick="this.blur();return false;" class="folder"></a>
          <div class="sep"></div>
          <a href="javascript:void(0);" onclick="this.blur();return false;" class="delete"></a>
        </div>
      </div>
      <a style="float:right;" href="javascript:void(0);" class="preview-url" title="Preview this image" onclick="im_load_image('imagebrowse_preview',document.getElementById('image_url').value);"></a>
      <div class="path" id="image_path"></div>
      <div class="tree">
        <div class="hdr">
          <div class="file"><span>Select file:</span></div>
          <div class="size"><span>File size:</span></div>
        </div>
        <div class="con">
          <ComponentArt:TreeView
            NodeClientTemplateId="FileTemplate"
            Id="FileBrowser1"
            RunAt="server"
            CssClass="none"
            ExtendNodeCells="true"
            ShowLines="true">
            <ClientTemplates>
              <ComponentArt:ClientTemplate Id="FileTemplate">
                <div class="node">
                  <div class="item img"><a href="javascript:void(0);" onclick="populate_image_path('##DataItem.GetProperty('ID')##',FileBrowser1,'image_path');this.blur();return false;">## DataItem.GetProperty("Text"); ##</a></div>
                  <div style="float:right;width:70px;line-height:20px;text-align:left;overflow:hidden;">## DataItem.GetProperty("Value"); ##</div>
                </div>
              </ComponentArt:ClientTemplate>

              <ComponentArt:ClientTemplate Id="FolderTemplate">
                <div class="node">
                  <div class="item fldr"><a href="javascript:void(0);" onclick="populate_image_path('##DataItem.GetProperty('ID')##',FileBrowser1,'image_path');this.blur();return false;">## DataItem.GetProperty("Text"); ##</a></div>
                </div>
              </ComponentArt:ClientTemplate>

              <ComponentArt:ClientTemplate Id="RootTemplate">
                <div class="node">
                  <div class="item root"><a href="javascript:void(0);" onclick="populate_image_path('##DataItem.GetProperty('ID')##',FileBrowser1,'image_path');this.blur();return false;">## DataItem.GetProperty("Text"); ##</a></div>
                </div>
              </ComponentArt:ClientTemplate>
            </ClientTemplates>
            <ClientEvents>
                <Load EventHandler="FileBrowser1_load" />
            </ClientEvents>
          </ComponentArt:TreeView>
        </div>
      </div>
    </div>

    <div class="im_browse-r">
      <div id="imagebrowse_preview" style="position:relative;" class="preview" >
              <div class="status" style="width:100%;height:100%;background:none;position:absolute;left:0;top:0;">Preview</div>
              <div class="thumbnail" style="width:100%;height:100%;background:none;position:absolute;left:0;top:0;">&nbsp;</div>
      </div>

      <div class="props">
        <div><span class="alt">Image Alt Text:</span><span class="input"><input id="<%=this.ClientID %>_image_desc" onchange="document.getElementById('<%=this.ClientID %>_image_alt').value=this.value;" type="text" class="img-alt" /></span></div>
        <div>
          <span class="w">Width:</span><span class="input"><input id="imagebrowser_image_width" type="text" class="img-w" /></span>
          <span class="h">Height:</span><span class="input"><input id="imagebrowser_image_height" type="text" class="img-h" /></span>
        </div>
      </div>
    </div>

  </ComponentArt:PageView>

  <ComponentArt:PageView Id="PropertiesPage" RunAt="server" CssClass="pv">

    <div class="im_props-l">
      <div class="prop">
        <a style="float:right;" href="javascript:void(0);" class="preview-url" title="Preview this image" onclick="im_load_image('imageproperties_preview',document.getElementById('image_url').value);"></a>
        <span class="lbl">Image URL:</span><input id="image_url" type="text" class="img-url" style="width:200px;" />
      </div>
      <div class="prop">
        <span class="lbl">Image Alt Text:</span><input id="<%=this.ClientID %>_image_alt" type="text" onchange="document.getElementById('<%=this.ClientID %>_image_desc').value=this.value;" class="img-alt" />
      </div>
      <div class="prop">
        <span class="lbl">Width:</span><input id="imageproperties_image_width" type="text" class="img-w s" />
        <span class="lbl s">Height:</span><input id="imageproperties_image_height"  type="text" class="img-h s" />
      </div>
      <div class="prop">
        <span class="lbl">Border Width:</span><input id="<%=this.ClientID %>_image_border" type="text" class="img-brd-w xs" />
        <span class="lbl">Border Color:</span><input value="N/A" type="text" class="img-brd-c xs" />
      </div>
      <div class="prop dbl">
        <div class="row"><span class="lbl">Image Alignment:</span><input id="<%=this.ClientID %>_image_align" type="text" class="img-align" /></div>
        <div class="row">
          <span class="lbl">Horizontal Spacing:</span><input id="<%=this.ClientID %>_image_hspace" type="text" class="img-hspace xs" />
          <span class="lbl">Vertical Spacing:</span><input id="<%=this.ClientID %>_image_vspace" type="text" class="img-vspace xs" />
        </div>
      </div>
      <div class="prop dbl last">
        <div class="row"><span class="lbl">CSS Class:</span><input id="<%=this.ClientID %>_image_cssclass" type="text" class="img-css" /></div>
        <div class="row"><span class="lbl">Long Description:</span><input value="N/A" type="text" class="img-desc" /></div>
      </div>
    </div>

    <div class="im_props-r">
      <div id="imageproperties_preview" style="position:relative;" class="preview">
              <div class="status" style="width:100%;height:100%;background:none;position:absolute;left:0;top:0;">Preview</div>
              <div class="thumbnail" style="width:100%;height:100%;background:none;position:absolute;left:0;top:0;">&nbsp;</div>
      </div>
    </div>

  </ComponentArt:PageView>

<ComponentArt:PageView Id="UploadPage" RunAt="server" CssClass="pv">
    <div class="im_upload-l">
      <div class="sel">
        <div class="browse">
          <ComponentArt:Upload
            ID="ImageUpload"
            RunAt="server"
            MaximumFileCount="1"
            AutoPostBack="false"
            FileInputClientTemplateId="FileInputTemplate"
            ProgressClientTemplateId="ProgressTemplate"
            ProgressDomElementId="upload-status"
          >
            <ClientEvents>
              <FileChange EventHandler="file_change" />
              <UploadEnd EventHandler="upload_end" />
            </ClientEvents>

            <ClientTemplates>
              <ComponentArt:ClientTemplate ID="FileInputTemplate">
                <span>Select File to Upload:</span>
                <div class="file">
                  <div class="## DataItem.FileName ? "filename" : "filename empty"; ##"><input value="## DataItem.FileName ##" onfocus="this.blur();" /></div>
                  <a href="javascript:void(0);" onclick="this.blur();return false;" class="browse" title="Browse for a file">#$FileInputImage</a>
                </div>
              </ComponentArt:ClientTemplate>

              <ComponentArt:ClientTemplate ID="ProgressTemplate">
                <div class="info">
                  <h3>Uploading File: <span class="red">## DataItem.CurrentFile ##</span></h3>
                  <div class="prog">
                    <div class="con">
                      <div class="bar" style="width:## get_percentage(DataItem.Progress); ##%;"></div>
                    </div>
                  </div>

                  <p><span class="red">## format_file_size(DataItem.ReceivedBytes); ##</span> of <span class="red">## format_file_size(DataItem.TotalBytes); ##</span> (## get_percentage(DataItem.Progress); ##%) Uploaded</p>
                </div>
              </ComponentArt:ClientTemplate>
            </ClientTemplates>
          </ComponentArt:Upload>
        </div>

        <div class="btn">
          <a href="javascript:void(0);" onclick="init_upload(this,ImageUpload);this.blur();return false;" class="up-d"></a>
        </div>
      </div>
    </div>

    <div class="im_upload-r">
      <div class="tb">
        <div class="btns">
          <a href="javascript:void(0);" onclick="this.blur();return false;" class="reload"></a>
          <div class="sep"></div>
          <a href="javascript:void(0);" onclick="this.blur();return false;" class="folder"></a>
          <div class="sep"></div>
          <a href="javascript:void(0);" onclick="this.blur();return false;" class="delete"></a>
        </div>

        <span>File Destination:</span>
      </div>
      <div class="dest">
        <ComponentArt:TreeView
          NodeClientTemplateId="FolderTemplate1"
          Id="FileBrowser2"
            RunAt="server"
            CssClass="none"
            ExtendNodeCells="true"
            ShowLines="true">
            <ClientTemplates>
              <ComponentArt:ClientTemplate Id="FolderTemplate1">
                <div class="node">
                  <div class="item fldr"><a href="javascript:void(0);" onclick="ImageUpload.CallbackParameter = populate_image_path('##DataItem.GetProperty('ID')##',FileBrowser2,'upload_path');this.blur();return false;">## DataItem.GetProperty("Text"); ##</a></div>
                </div>
              </ComponentArt:ClientTemplate>

              <ComponentArt:ClientTemplate Id="RootTemplate1">
                <div class="node">
                  <div class="item root"><a href="javascript:void(0);" onclick="ImageUpload.CallbackParameter = populate_image_path('##DataItem.GetProperty('ID')##',FileBrowser2,'upload_path');this.blur();return false;">## DataItem.GetProperty("Text"); ##</a></div>
                </div>
              </ComponentArt:ClientTemplate>
            </ClientTemplates>
          </ComponentArt:TreeView>
      </div>
      </div>

      <div class="path" style="padding-left:5px;width:286px;" id="upload_path"></div>
    </div>
  </ComponentArt:PageView>

  <ComponentArt:PageView Id="UploadStatusPage" RunAt="server" CssClass="pv">
    <div class="stat" id="upload-status"></div>
  </ComponentArt:PageView>
</ComponentArt:MultiPage>
      </div>
    </div>

    <div class="im_ftr">
      <div class="im_ftr-l"></div>
      <div class="im_ftr-m">
        <div class="btns">
          <a onclick="<%=ImageManagerDialog.ClientID %>.close(true);this.blur();return false;" href="javascript:void(0);" rel="close">
            <span class="l"></span>
            <span class="m" style="padding:0 17px;">OK</span>
            <span class="r"></span>
          </a>
        </div>

        <div class="btns-l">
          <a onclick="<%=ImageManagerDialog.ClientID %>.close();this.blur();return false;" href="javascript:void(0);" rel="cancel">
            <span class="l"></span>
            <span class="m" style="padding:0 8px;">Cancel</span>
            <span class="r"></span>
          </a>
        </div>
      </div>
      <div class="im_ftr-r"></div>
    </div>
  </div>
</div>

  </Content>
    <ClientEvents>
    <OnShow EventHandler="ImageManagerDialog_OnShow" />
    <OnClose EventHandler="ImageManagerDialog_OnClose" />
  </ClientEvents>
</ComponentArt:Dialog>

<script type="text/javascript">

<%=ImageManagerDialog.ClientID %>.ParentControlID = "<%=this.ClientID %>";
<%=ImageManagerDialog.ClientID %>.UploadPath = "<%=this.Attributes["EditorUploadPath"] %>";
window.EditorApplicationPath = "<%=HttpContext.Current.Request.ApplicationPath %>";

</script>