import { Component, OnInit, OnDestroy, Inject, Input} from '@angular/core';
import { Router } from '@angular/router';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagPaper, MVCMagPaperListSelectionCriteria } from '../services/MAGClasses.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { Subscription } from 'rxjs';
import { Item } from '../services/ItemList.service';

@Component({
   
	selector: 'microsoftAcademicComp',
	templateUrl: './microsoftAcademic.component.html'  

})

export class microsoftAcademicComp implements OnInit, OnDestroy {

    constructor(private router: Router, 
        @Inject('BASE_URL') private _baseUrl: string,
        public _magAdvancedService: MAGAdvancedService,
        private _ItemCodingService: ItemCodingService
    ) { }

    @Input() item: Item = new Item;
    private _MagPaperList: MagPaper[] = [];
    private sub: Subscription = new Subscription();
    public magPaperId: number = 0;
    public magPaperRefId: number = 0;
    public currentMagPaperLinkItem: MagPaper = new MagPaper();
    ngOnInit() {

        console.log('Input item is there a linkedItem: ', JSON.stringify(this.item));
        console.log('check this structure: ', JSON.stringify(this._magAdvancedService.MagReferencesPaperList.papers));

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

        this._magAdvancedService.FetchMagPaperId(this.magPaperId).then(

            (result: MagPaper) => {

                this._magAdvancedService.PostFetchMagPaperCalls(result);
            });

    }
    public GetMagPaperRef(magPaperRefId: number) {

        this._magAdvancedService.FetchMagPaperId(magPaperRefId).then(

            (result: MagPaper) => {

                this._magAdvancedService.PostFetchMagPaperCalls(result);
            });
    }
    public CanGetMagPaper(): boolean {

        if (this.magPaperId != null && this.magPaperId > 0) {
            return true;
        } else {
            return false;
        }

    }
    public FetchMAGMatches() {
        let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        crit.listType = 'ItemMatchedPapersList';
        crit.iTEM_ID = this.item.itemId;
        this._magAdvancedService.FetchMagPaperList(crit).then(
            (res) => {
                if (res != null) {
                    console.log('fetchMAGMatches 1: ' + JSON.stringify(res));
                    this.MagPaperList = res.papers;
                } else {
                    console.log('fetchMAGMatches is empty: ' + JSON.stringify(res));
                    this.MagPaperList = [];
                }
            }
        )
    }
    public FindNewMAGMatches() {

        this._magAdvancedService.MagMatchItemsToPapers(this.item.itemId).then(
            (res) => {
                if (res != null) {
                    console.log('FindNewMAGMatches 1: ' + JSON.stringify(res));
                    this.MagPaperList = res;
                } else {
                    console.log('FindNewMAGMatches is empty: ' + JSON.stringify(res));
                    this.MagPaperList = [];
                }
            }
        )
    }
    public ClearMAGMatches() {

        let res: any = this._magAdvancedService.ClearMagMatchItemsToPapers(this.item.itemId);
        
        console.log('fetchMAGMatches is empty: ' + JSON.stringify(res));
        this._magAdvancedService.MagReferencesPaperList.papers = [];
        
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
        console.log(JSON.stringify(MagPapers));
        if (MagPapers.papers.length > 0) {

            var paper = MagPapers.papers.find((x) => x.paperId == paperId);

            if (paper != null) {
                this.currentMagPaperLinkItem = paper;
            } else {
                return;
            }


            if (match) {
                this.currentMagPaperLinkItem.manualTrueMatch = true;
            } else {
                this.currentMagPaperLinkItem.manualFalseMatch = true;
            }

            this._magAdvancedService.UpdateMagPaper(match, paperId, this.item.itemId).then(
                //(result: MagPaper) => {
                //        this.currentMagPaperLinkItem = result;
                //    }
            );

        }
      
    }
}


