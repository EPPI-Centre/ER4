import { Component, OnInit, OnDestroy, Inject, Input} from '@angular/core';
import { Router } from '@angular/router';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagPaper, MVCMagPaperListSelectionCriteria } from '../services/MAGClasses.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { Subscription } from 'rxjs';
import { Item } from '../services/ItemList.service';
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
        public ConfirmationDialogService: ConfirmationDialogService
    ) { }

    @Input() item: Item | undefined = new Item();
    private _MagPaperList: MagPaper[] = [];
    private sub: Subscription = new Subscription();
    public magPaperId: number = 0;
    public magPaperRefId: number = 0;
    public currentMagPaperLinkItem: MagPaper = new MagPaper();
    public foundMagPaper: boolean = false;
    public FoundPaper: MagPaper = new MagPaper();
    ngOnInit() {

        this.sub = this._ItemCodingService.DataChanged.subscribe(
            () => {
                this._magAdvancedService.MagReferencesPaperList.papers = [];
                this.FetchMAGMatches();
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
    public CanGetMagPaper(): boolean {

        if (this.magPaperId != null && this.magPaperId.toString().length > 4) {
            return true;
        } else {
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


