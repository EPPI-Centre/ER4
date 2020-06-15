import { Component, OnInit, OnDestroy, Inject, Input} from '@angular/core';
import { Router } from '@angular/router';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagPaper } from '../services/MAGClasses.service';
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
    ngOnInit() {



        this.sub = this._ItemCodingService.DataChanged.subscribe(
            () => {
                this._magAdvancedService.MagReferencesPaperList = [];
                this.FetchMAGMatches();
            }
        );
    }
    public get IsServiceBusy(): boolean {

        return this._magAdvancedService.IsBusy;
    }
    public GetMagPaper() {

        this._magAdvancedService.FetchMagPaperId(this.magPaperId).then(

            () => { this.router.navigate(['MAGBrowser']); }

        );
    }
    public GetMagPaperRef(magPaperRefId: number) {

        this._magAdvancedService.FetchMagPaperId(magPaperRefId).then(

            () => { this.router.navigate(['MAGBrowser']); }

        );
    }
    public CanGetMagPaper(): boolean {

        if (this.magPaperId != null && this.magPaperId > 0) {
            return true;
        } else {
            return false;
        }

    }
    public FetchMAGMatches() {

        let res: any = this._magAdvancedService.MagMatchItemsToPapers(this.item.itemId);
        if (res != null) {
            console.log('fetchMAGMatches: ' + JSON.stringify(res));
            this.MagPaperList = res;
        } else {
            console.log('fetchMAGMatches is empty: ' + JSON.stringify(res));
            this.MagPaperList = [];
        }
    }
    public ClearMAGMatches() {

        let res: any = this._magAdvancedService.ClearMagMatchItemsToPapers(this.item.itemId);
        
        console.log('fetchMAGMatches is empty: ' + JSON.stringify(res));
        this._magAdvancedService.MagReferencesPaperList = [];
        
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
    public UpdateMagPaper(matchCorrect: boolean, paperId: number) {

        //this._magAdvancedService.UpdateMagPaper(matchCorrect, paperId);

    }
}


