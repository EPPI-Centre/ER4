import { Component, OnInit, OnDestroy, Inject, Input} from '@angular/core';
import { Router } from '@angular/router';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MagPaper } from '../services/MAGClasses.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { Subscription } from 'rxjs';

@Component({
   
	selector: 'microsoftAcademicComp',
	templateUrl: './microsoftAcademic.component.html'  

})

export class microsoftAcademicComp implements OnInit, OnDestroy {

    constructor(private router: Router, 
        @Inject('BASE_URL') private _baseUrl: string,
        private _magAdvancedService: MAGAdvancedService,
        private _ItemCodingService: ItemCodingService
    ) { }

    @Input() ItemID: number = 0;
    private _MagPaperList: MagPaper[] = [];
    private sub: Subscription = new Subscription();
    ngOnInit() {
        this.sub = this._ItemCodingService.DataChanged.subscribe(
            () => {
                this._magAdvancedService.MagReferencesPaperList = [];
                this.fetchMAGMatches();
            }
        );
    }
    public get IsServiceBusy(): boolean {

        return this._magAdvancedService.IsBusy;
    }
    private fetchMAGMatches() {

        let res: any = this._magAdvancedService.MagMatchItemsToPapers(this.ItemID);
        if (res != null) {
            console.log('fetchMAGMatches: ' + JSON.stringify(res));
            this.MagPaperList = res;
            console.log('this.MagPaperList is: ' + JSON.stringify(this.MagPaperList));
        }
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
}


