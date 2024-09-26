import { Component, OnInit, OnDestroy, Inject, Input} from '@angular/core';
import { Router } from '@angular/router';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagPaper, MVCMagPaperListSelectionCriteria } from '../services/MAGClasses.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { Subscription } from 'rxjs';
import { Item, ItemListService } from '../services/ItemList.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { MAGBrowserHistory } from './MAGBrowserHistory.component';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';

@Component({
   
	selector: 'microsoftAcademicComp',
	templateUrl: './microsoftAcademic.component.html'  

})

export class microsoftAcademicComp implements OnInit, OnDestroy {

    constructor(private router: Router, 
        @Inject('BASE_URL') private _baseUrl: string,
        private _MAGBrowserService: MAGBrowserService,
        public _magAdvancedService: MAGAdvancedService,
        private _ItemCodingService: ItemCodingService,
        public _notificationService: NotificationService,
        public ItemListService: ItemListService,
        public ConfirmationDialogService: ConfirmationDialogService
    ) { }

    @Input() item: Item | undefined = new Item();
    private _MagPaperList: MagPaper[] = [];
    private sub: Subscription = new Subscription();
    public magPaperId: number = 0;
    public magPaperRefId: number = 0;
    public currentMagPaperLinkItem: MagPaper = new MagPaper();
    public foundMagPaper: boolean = false;
    public showAbstract: boolean = false;
    public FoundPaper: MagPaper = new MagPaper();
    ngOnInit() {

        this.sub = this._ItemCodingService.DataChanged.subscribe(
            () => {
                this._magAdvancedService.MagReferencesPaperList.papers = [];
                this.FetchMAGMatches();
                this.foundMagPaper = false;
                this.magPaperId = 0;
            }
        );
    }
    public get IsServiceBusy(): boolean {

        return this._magAdvancedService.IsBusy;
    }
    public GetMagPaper() {
        if (this.magPaperId > 0) this.router.navigate(['MAG', this.magPaperId]); 
    }
    public GetMagPaperRef(magPaperRefId: number) {
        if (magPaperRefId > 0) this.router.navigate(['MAG', magPaperRefId]); 
    }
    public enrichItemRecord(magPaperRefId: number) {
      if (this.item) {
        var MagPapers = this._magAdvancedService.MagReferencesPaperList;
        var paper = MagPapers.papers.find((x) => x.paperId == magPaperRefId);

        // at this point I am only updating certain fields
        // and only when the item record has no information

        if (paper) {
          // title
          if ((this.item.title == "") && (paper.paperTitle != null)) {
            this.item.title = paper.paperTitle;
          }
          // journal
          if ((this.item.parentTitle == "") && (paper.journal != null)) {
            this.item.parentTitle = paper.journal;
          }
          // year
          if ((this.item.year == "") && (paper.year != null)) {
            this.item.year = paper.year.toString();
          }
          // volume
          if ((this.item.volume == "") && (paper.volume != null)) {
            this.item.volume = paper.volume;
          }
          // issue
          if ((this.item.issue == "") && (paper.issue != null)) {
            this.item.issue = paper.issue;
          }
          // pages
          if ((this.item.pages == "") && (paper.firstPage != null) && (paper.lastPage != null)) {
            if (paper.firstPage == paper.lastPage) {
              this.item.pages = paper.firstPage;
            }
            else {
              this.item.pages = paper.firstPage + "-" + paper.lastPage;
            }
          }
          // doi
          if ((this.item.doi == "") && (paper.doi != null)) {
            this.item.doi = paper.doi;
          }
          // abstract
          if ((this.item.abstract == "") && (paper.abstract != null)) {
            this.item.abstract = paper.abstract;
          }

          // save the changes to item
          if (this.item) {
            this.ItemListService.UpdateItem(this.item);
          }


        }
      }     
    }

    public CanGetMagPaper(): boolean {

        if (this.magPaperId != null && this.magPaperId.toString().length > 4) {
            return true;
        } else {
            return false;
        }

  }

  public ShowHideAbstract() {

    if (this.showAbstract == true) {
      this.showAbstract = false;
    } else {
      this.showAbstract = true;
    }
  }

  public GetButtonText(): string {
    if (this.showAbstract == true) {
      return "Hide abstract(s)";
    } else {
      return "Show abstract(s)";
    }
    
  }


  public canShowAbstract(): boolean {

    if (this.showAbstract == true) {
      return true;
    } else {
      return false;
    }

  }

  public ChangeToPercent(score: number): string {
    var percent = Math.round(score * 100).toString();
    return percent + "%";
  }


  public GetPages(magPaper:  MagPaper): string {

    var magPaperFirstPage = "";
    var magPaperLastPage = "";
    if (magPaper.firstPage == null)
      magPaperFirstPage = "";
    else {
      magPaperFirstPage = magPaper.firstPage;
    }
    if (magPaper.lastPage == null)
      magPaperLastPage = "";
    else {
      magPaperLastPage = magPaper.lastPage;
    }

    if (magPaperFirstPage == magPaperLastPage) {
      return magPaperFirstPage;
    }
    else {
      return magPaperFirstPage + "-" + magPaperLastPage;
    }

  }

  public areTheFieldsDifferent(itemField: string, magField: null | string): boolean {

    if (magField == null)
      magField = "";

    if (itemField.trim().toLowerCase() == magField.trim().toLowerCase()) {
      return false;
    }
    else {
      return true;
    }
  }

  public areDOIFieldsDifferent(itemField: string, magField: null | string): boolean {
    // we want only look at the doi text and ignore any http info
    if (magField == null)
      magField = "";

    // remove any http stuff
    itemField = itemField.replace(/http:\/\/|https:\/\//g, '');
    magField = magField.replace(/http:\/\/|https:\/\//g, '');

    // remove any doi.org stuff
    itemField = itemField.replace(/doi.org\//g, '');
    magField = magField.replace(/doi.org\//g, '');

    if (magField.toLowerCase().trim() == itemField.toLowerCase().trim())
      return false;
    else {
      return true;
    }
  }

  public areTheTitlesDifferent(itemField: string, magField: null | string): boolean {

    if (magField == null)
      magField = "";

    // Sometimes the ER or OA title has a '.' at the end so removed for comparisons.
    if (itemField.includes(".", itemField.length - 1)) {
      itemField = itemField.substring(0,itemField.length - 1)
    }
    if (magField.includes(".", magField.length - 1)) {
      magField = magField.substring(0, magField.length - 1)
    }

    // put everything to lowercase and trim
    var itemText = itemField.trim().toLowerCase();
    var magText = magField.trim().toLowerCase();

    // remove the various apostraphes and single quotes that are used interchangebly
    // and mess up the comparisons
    itemText = itemText.replace(/'|’/g, '');
    magText = magText.replace(/'|’/g, '');


    // there are many types of dashes so remove them all
    // hyphen (-)
    // En dash (–)
    // Em dash (—)
    // mystery one (‐) that seems to be in OA

    itemText = itemText.replace(/-|–|—|‐/g, '');
    magText = magText.replace(/-|–|—|‐/g, '');

    if (itemText == magText) {
      return false;
    }
    else {
      return true;
    }

  }

  public areThePagesDifferent(itemPages: string, magPaperFirstPage: null | string, magPaperLastPage: null | string): boolean {
       
    if (magPaperFirstPage == null)
      magPaperFirstPage = "";
    if (magPaperLastPage == null)
      magPaperLastPage = ""; 

    var magPages = "";
    // remove all empty spaces using RE
    if (magPaperFirstPage.replace(/\s/g, "") == "")
      magPages = "";
    else if (magPaperFirstPage.replace(/\s/g, "") == magPaperLastPage.replace(/\s/g, ""))
      magPages = magPaperFirstPage.replace(/\s/g, "");
    else
      magPages = magPaperFirstPage.replace(/\s/g, "") + "-" + magPaperLastPage.replace(/\s/g, "");
    

    if ((itemPages.replace(/\s/g, "").length > 0) && (magPages.length == 0)) {
      // there is an item page but no magPage
      return true;
    } else if ((itemPages.replace(/\s/g, "").length == 0) && (magPages.length > 0)) {
      // this is no item page but there is a magPage
      return true
    } else if (itemPages.replace(/\s/g, "") != magPages) {      
      return true
    } else {
      // default is that they are the same
      return false;
    }
  }


  public FetchMAGMatches() {
    if (!this.item) return;
        let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        crit.listType = 'ItemMatchedPapersList';
        crit.iTEM_ID = this.item.itemId;
        this._magAdvancedService.FetchMagPaperMagList(crit).then(
            (res) => {
                if (res != null) {
                   
                    this.MagPaperList = res.papers;
                } else {
                    
                    this.MagPaperList = [];
                }
            }
        )
    }
    public FindNewMAGMatches() {
      if (!this.item) return;
        this._magAdvancedService.MagMatchItemsToPapers(this.item.itemId).then(
            (res) => {
                if (res != null) {
                  
                    this.MagPaperList = res;
                } else {
                  
                    this.MagPaperList = [];
                }
            }
        )
    }
    public ClearMAGMatches() {

        this.ConfirmationDialogService.confirm("Are you sure you wish to clear all matching for this item?", "", false, "")
            .then(
                (confirm: any) => {
                    if (confirm && this.item) {
                        let res: any = this._magAdvancedService.ClearMagMatchItemsToPapers(this.item.itemId);

                        this._magAdvancedService.MagReferencesPaperList.papers = [];
                        this.magPaperId = 0;
                        this.foundMagPaper = false;
                        this._notificationService.show({
                            content: "Clearing all matches for specific item!",
                            animation: { type: 'slide', duration: 400 },
                            position: { horizontal: 'center', vertical: 'top' },
                            type: { style: "warning", icon: true },
                            hideAfter: 20000
                        });
                    }
                }
            )

    }
    public get MagPaperList() {

        return this._MagPaperList;

    }
    public set MagPaperList(value: MagPaper[]) {

        this._MagPaperList = value

    }
    BackToMain() {

        this.router.navigate(['Main']);
    }
	ngOnDestroy() {

        this.sub.unsubscribe();
    }
    
    public UpdateMagPaper(match: boolean, paperId: number) {

        var MagPapers = this._magAdvancedService.MagReferencesPaperList;
        //console.log(JSON.stringify(MagPapers));
        if (MagPapers.papers.length > 0) {

            var paper = MagPapers.papers.find((x) => x.paperId == paperId);

            if (paper != null) {
                this.currentMagPaperLinkItem = paper;
            } else {
                return;
            }


            if (match) {
                this.currentMagPaperLinkItem.manualTrueMatch = true;
                this.currentMagPaperLinkItem.manualFalseMatch = false;
            } else {
                this.currentMagPaperLinkItem.manualFalseMatch = true;
                this.currentMagPaperLinkItem.manualTrueMatch = false;
            }

          if (this.item) {
            this._magAdvancedService.UpdateMagPaper(match, paperId, this.item.itemId).then(
              () => {
                this.FetchMAGMatches();
              }
            );
          }
        }
    }

    public GetMagPaperForPage() {
        //if (this.magPaperId > 0) this.router.navigate(['MAG', this.magPaperId]);
        this._MAGBrowserService.FetchJustMagPaperRecordById(this.magPaperId).then(
            (res: boolean) => {
                if (res && this._MAGBrowserService.currentMagPaper.paperId == this.magPaperId) {
                    this.foundMagPaper = true;
                    this.FoundPaper = this._MAGBrowserService.currentMagPaper;
                }
            }
        );
    }

    public UpdateMagPaperFound(match: boolean) {

            this.foundMagPaper = false;

        if (this.FoundPaper  != null) {
            this.currentMagPaperLinkItem = this.FoundPaper ;
            } else {
                return;
            }


            if (match) {
                this.currentMagPaperLinkItem.manualTrueMatch = true;
                this.currentMagPaperLinkItem.manualFalseMatch = false;
            } else {
                this.currentMagPaperLinkItem.manualFalseMatch = true;
                this.currentMagPaperLinkItem.manualTrueMatch = false;
      }
      if (this.item) {
        this._magAdvancedService.UpdateMagPaper(match, this.FoundPaper.paperId, this.item.itemId).then(
          () => {
            this.FetchMAGMatches();
          }
        );
      }
      
    }


}


