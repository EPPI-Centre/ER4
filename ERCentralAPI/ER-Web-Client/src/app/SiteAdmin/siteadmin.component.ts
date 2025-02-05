import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { FeedbackAndClientError, OnlineHelpContent, OnlineHelpContent1, OnlineHelpService } from '../services/onlinehelp.service';
import { GridDataResult, PageChangeEvent, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, process, CompositeFilterDescriptor, State } from '@progress/kendo-data-query';
import { Subscription } from 'rxjs';
import { EventEmitterService } from '../services/EventEmitter.service';
import { SelectEvent, TabStripComponent } from '@progress/kendo-angular-layout';
import { CKEditor4 } from 'ckeditor4-angular/ckeditor';
import { ModalService } from '../services/modal.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { SourcesService, iJSONreport4upolad } from '../services/sources.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';


@Component({
    selector: 'siteadmin',
    templateUrl: './siteadmin.component.html',
    providers: []
})

export class SiteAdminComponent implements OnInit {

  @ViewChild('content') private content: any;
  @ViewChild('tabstrip') public tabstrip!: TabStripComponent;

    constructor(private router: Router,
        private _httpC: HttpClient,
        private OnlineHelpService: OnlineHelpService,
        @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService,
      public eventEmitters: EventEmitterService,
      private modalService: ModalService,
      public _notificationService: NotificationService,
      private sourcesService: SourcesService,
      private confirmationDialogService: ConfirmationDialogService,
      private reviewInfoService: ReviewInfoService,

        private _onlineHelpService: OnlineHelpService
  ) { }



    ngOnInit() {
        this.subOpeningReview = this.eventEmitters.OpeningNewReview.subscribe(() => this.BackToMain());
        if (!this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) this.router.navigate(['home']);
      else this.OnlineHelpService.GetFeedbackMessageList();
      this.reader.onload = (e) => this.fileRead();
    }
    public Uname: string = "";
    public Pw: string = "";
    public revId: string = "";
    public LogTypeSelection: number = 0;


    private _ActivePanel: string = "Help";
    public ActivePanel: string = "Help";


    public selected?: ReadOnlyHelpPage;
    public selectedContext: string = "Select help context";
    public get HelpPages(): ReadOnlyHelpPage[] {
      return this._onlineHelpService.HelpPages;
  }

    DisplayFriendlyHelpPageNames(helpPageItem: ReadOnlyHelpPage): string {
      this.selectedContext  = helpPageItem.context_Name;
      return helpPageItem.context_Name;
    }


    subOpeningReview: Subscription | null = null;
    public get FeedbackMessageList(): FeedbackAndClientError[] {
        return this.OnlineHelpService.FeedbackMessageList;
    }
    SelectTab(i: number) {
      if (!this.tabstrip) return;
      else {
        let t = this.tabstrip.tabs.get(i);
        if (!t) return;
        let e = new SelectEvent(i, t.title);
        this.tabstrip.selectTab(i);
        this.onTabSelect(e);
      }
    }
    public get DataSource(): GridDataResult {
        return process(this.OnlineHelpService.FeedbackMessageList, this.state);
    }
    public state: State = {
        skip: 0,
        take: 10,
        
    };
    public dataStateChange(state: DataStateChangeEvent): void {
        this.state = state;
        this.DataSource; //= process(sampleProducts, this.state);
    }
    
    ShowDBSettingById(event: Event) {
      let helpId = parseInt((event.target as HTMLOptionElement).value);
      this.OnlineHelpService.FetchHelpContent(helpId.toString());
    }
    
    public get IsSiteAdmin(): boolean {
      if (this.ReviewerIdentityServ
          && this.ReviewerIdentityServ.reviewerIdentity
          && this.ReviewerIdentityServ.reviewerIdentity.userId > 0
          && this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated
          && this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) return true;
      else return false;
    }
    public get CanOpenRev(): boolean {
        if (this.Uname.trim().length < 2) return false;
        else if (this.Pw.trim().length < 6) return false;
        else if (this.revId.trim().length > 0) {
            let rid = parseInt(this.revId, 10)
            if (!isNaN(rid) && rid > 0) return true;
            else return false;
        }
        else return false;
    }
    OpenRev() {
        //this.LoggedStatus == "";
        let rid = parseInt(this.revId, 10)
        if (!isNaN(rid) && rid > 0) {
            //this.BackToMain();
            this.ReviewerIdentityServ.LoginReqSA(this.Uname, this.Pw, rid);
        }
    }
  public get IsServiceBusy(): boolean {
    if (this.OnlineHelpService.IsBusy) return true;
    else return false;
  }


/////////////////////////////////////////////////////////////////////////

  public TmpCurrentContextHelp: string = "";
  public OrigCurrentContextHelp: string = "";
  public enableSave: boolean = false;


  CanSaveHelp() {
    if (this.enableSave) {
      return true;
    } else return false;
  }


  public get CurrentContextHelp(): string {
    //console.log("CurrentContextHelp", this.OnlineHelpService.CurrentHTMLHelp);
    if (this.OnlineHelpService.IsBusy) return "";
    else {
      return this.OnlineHelpService.CurrentHTMLHelp;
    }
  }


  public model = {
    editorData: "",
  };


  public helpContent: string | null = null;
  //public ContextSelection: number = 0;
  public context = "";
  public editingHelp = "";


  public RetrieveHelpNew(event: Event) {
    if (this.selected != null) {
      if (this.selected.context_Name != "Select help context") {
        this.OnlineHelpService.FetchHelpContent(this.selected.context_Name);
        if (this.CurrentContextHelp == null) {
          // there is no data
          this.OnlineHelpService.FetchHelpContent("");
        }
      }
      else {
        // user selected '0' again so no data
        this.OnlineHelpService.FetchHelpContent("");
      }
    }
  }



  public onDataChange(event: CKEditor4.EventInfo) {
    var test = event.editor.getData();
    //this.TmpCurrentContextHelp = this.model.editorData;
    this.TmpCurrentContextHelp = event.editor.getData();
    if (this.OrigCurrentContextHelp == event.editor.getData()) {
      // things are unchanged from origninal...
      this.enableSave = false;
    }
    else {
      // things are different...
      this.enableSave = true;
    }
  }


  Edit() {
    if (this.showEdit == true) {
      // this is a 'cancel'
      this.showEdit = false;
      this.TmpCurrentContextHelp = "";
      this.OrigCurrentContextHelp = "";
      this.showEdit = false;

      //this.OnlineHelpService.FetchHelpContent("0");
      //this.ContextSelection = 0;
      //this.OnlineHelpService.FetchHelpPageList();
    }
    else {
      // this is an 'edit'
      this.enableSave = false;
      this.showEdit = true;     
      this.OrigCurrentContextHelp = this.CurrentContextHelp;
      this.model.editorData = this.CurrentContextHelp;
      this.TmpCurrentContextHelp = this.model.editorData;

    }    
  }


  public GetText(): string {
    if (this.showEdit == true) {
      return this.model.editorData;
    }
    else {
      return this.CurrentContextHelp;
    }      
  }


  Save() {
    if (this.selected != null) {
      let help: OnlineHelpContent1 = new OnlineHelpContent1();
      help.context = this.selected.context_Name;
      help.helpHTML = this.model.editorData;
      this.OnlineHelpService.UpdateHelpContent(help);
      this.showEdit = false;

      // reset the dropdown
      this.OnlineHelpService.FetchHelpPageList();
      this.OnlineHelpService.FetchHelpContent("");

      // don't reset the dropdown
      //this.OnlineHelpService.FetchHelpPageList();
      //this.OnlineHelpService.FetchHelpContent(this.selected.context_Name);


    }
  }


  public showEdit: boolean = false;
  public get ShowEdit(): boolean {
    return this.showEdit;
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////

  BackToMain() {
        this.router.navigate(['Main']);
    }
    ngOnDestroy() {
  }


  onTabSelect(e: SelectEvent) {

    if (e.title == 'Help') {
      //this.OnlineHelpService.FetchHelpContentList();
      this.OnlineHelpService.FetchHelpContent("0");
      //this.ContextSelection = 0;
      this.sourcesService.FetchSources();
      this.OnlineHelpService.FetchHelpPageList();
    }
    else {

    }
  }


  @ViewChild('file') file: any;
  private currentFileName: string = "";
  public importCommand: iJSONreport4upolad = this.EmptyImportCommand;
  public ItemsCount4Import: number = 0;
  public CodingTools4Import: string[] = [];

  public get busyImporting(): boolean {
    return this.sourcesService.IsBusy;
  }
  public get canImport(): boolean {
    if (this.importCommand.content == "") return false;
    if (this.importCommand.fileName == "") return false;
    if (this.CodingTools4Import.length == 0 && this.ItemsCount4Import == 0) return false;
    return true;
  }
  public get CurrentReviewName(): string {
    return this.reviewInfoService.ReviewInfo.reviewName;
  }

  public ClearJSONCache() {
    this.ItemsCount4Import = 0;
    this.CodingTools4Import = [];
    this.importCommand = this.EmptyImportCommand;
  }
  private get EmptyImportCommand(): iJSONreport4upolad {
    return {
      content: "",
      fileName: "",
      importWhat: "",
      returnMessage: "",
    }; 
  }
  addFile() {
    //console.log('oo');
    if (this.file) this.file.nativeElement.click();
  }
  private reader = new FileReader();
  onFilesAdded() {
    const files: { [key: string]: File } = this.file.nativeElement.files;
    const file: File = files[0];
    console.log("onFilesAdded", file.size, file.name);
    if (file) {
      //if (file.size > 31457280 && 1 !== 1) {
      if (file.size > 52428800) {
        //console.log("onFilesAdded", file.size, file.name);
        this.modalService.GenericErrorMessage("Sorry, the <strong>maximum</strong> file size is <strong>50MB</strong>. Please select a smaller file.");
      }
      else {
        this.currentFileName = file.name;
        //reader.onload = function (e) {
        //    if (reader.result) {
        //        fileContent = reader.result as string;
        //        console.log(fileContent.length);
        //    }
        //}
        this.reader.readAsText(file);
      }
    }
  }

  private fileRead() {
    this.ClearJSONCache();
    //console.log("File read 1st check:", this.reader.result);
    if (this.reader.result) {
      const fileContent: string = this.reader.result as string;
      //console.log("fileRead: " + fileContent.length);
      let filename = "";
      if (this.currentFileName) filename = this.currentFileName.trim();
      else {
        return;
      }
      this.importCommand = {
        content: fileContent,
        importWhat: "",
        fileName: filename,
        returnMessage: ""
      };
      const parsed = JSON.parse(fileContent);
      if (parsed) {
        if (parsed.CodeSets && parsed.CodeSets.length > 0) {
          for (let Cset of parsed.CodeSets) {
            if (Cset && Cset.SetName) {
              this.CodingTools4Import.push(Cset.SetName);
            }
          }
        }
        if (parsed.References && parsed.References.length > 0) {
          this.ItemsCount4Import = parsed.References.length;
        }
      }
    }
  }

  public ImportJson() {
    if (!this.canImport) return;
    this.confirmationDialogService.confirm("Import JSON Report?"
      , "Are you sure you want to import this (<strong>"
      + this.importCommand.fileName + "</strong>) JSON report?<br>"
      + "Data will be imported in this reivew:<br>"
      + "<div class='w-100 p-0 mx-0 my-2 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
      + this.CurrentReviewName + "</strong></div>"
      , false, '')
      .then((confirm: any) => {
        if (confirm) {
          this.doImport();
        }
      });
  }

  private doImport() {
    if (!this.canImport) return;
    this.sourcesService.ImportJsonReport(this.importCommand).then(
      (result) => {
        if (result) {
          this._notificationService.show({
            content: "Imported! Please refresh Coding tools and Stats (in this order) to check.",
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "warning", icon: true },
            hideAfter: 10000
          });
          this.ClearJSONCache();
        }
      }
    );
  }
}

export interface ReadOnlySource {
  source_ID: number;
  source_Name: string;
  total_Items: number;
  deleted_Items: number;
  duplicates: number;
  isDeleted: boolean;
}

export interface ReadOnlyHelpPage {
  helpPage_ID: number;
  context_Name: string;
}




