import {  Inject, Injectable, OnDestroy, OnInit} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { Subscription } from 'rxjs';

@Injectable({

    providedIn: 'root',

})

export class searchService extends BusyAwareService implements OnDestroy {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private EventEmitterService: EventEmitterService,
        @Inject('BASE_URL') private _baseUrl: string
	) {
		super();
		console.log("On create search service");
		this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
    }
	
	ngOnDestroy() {
		console.log("Destroy search service");
		if (this.clearSub != null) this.clearSub.unsubscribe();
	}
	private clearSub: Subscription | null = null;
	public cmdSearches: SearchCodeCommand = new SearchCodeCommand();

	private _SearchList: Search[] = [];
	


	//@Output() searchesChanged = new EventEmitter();
    //public crit: CriteriaSearch = new CriteriaSearch();
	public searchToBeDeleted: string = '';//WHY string?

	public get SearchList(): Search[] {

		return this._SearchList;

    }

	public set SearchList(searches: Search[]) {
		this._SearchList = searches;
        //this.searchesChanged.emit();
	}



    Fetch() {
        this._BusyMethods.push("Fetch");
		 this._httpC.get<Search[]>(this._baseUrl + 'api/SearchList/GetSearches')
             .subscribe(result => {
                 this.RemoveBusy("Fetch");
				
				this.SearchList = result;
				//this.searchesChanged.emit();
             },
             error => {
                 this.RemoveBusy("Fetch");
                 this.modalService.GenericError(error);
                 this.Clear();
             }
		 );
	}
	public Clear() {
		console.log("clear in Searches Service");
		this._SearchList = [];
		this.searchToBeDeleted = "";
		this.cmdSearches = new SearchCodeCommand();
        //this.crit = new CriteriaSearch();
        //this._isBusy = false;
    }
	
	
	Delete(value: string) {

        this._BusyMethods.push("Delete");
		let body = JSON.stringify({ Value: value });
		this._httpC.post<string>(this._baseUrl + 'api/SearchList/DeleteSearch',
			body)
			.subscribe(result => {
                this.RemoveBusy("Delete");
				let tmpIndex: any = this.SearchList.findIndex(x => x.searchId == Number(this.searchToBeDeleted));
				this.SearchList.splice(tmpIndex, 1);
				this.Fetch();
            }, error => {
                this.RemoveBusy("Delete");
                this.modalService.GenericError(error);
            }
		);
	}

	CreateSearch(cmd: SearchCodeCommand, apiStr: string) {
        this._BusyMethods.push("CreateSearch");
		apiStr = 'api/SearchList/' + apiStr;
		this._httpC.post<Search[]>(this._baseUrl + apiStr,
			cmd)

            .subscribe(result => {
                this.RemoveBusy("CreateSearch");
				this.Fetch();
            }, error => {
                this.RemoveBusy("CreateSearch");
                this.modalService.GenericError(error);
            }
			);
	}
}

export class Search {

	searchNo: number = 0;
	selected: boolean = false;
	searchId: number = 0;
	hitsNo: number = 0;
	title: string = '';
	searchDate: string = '';
	contactName: string = '';
	isClassifierResult: boolean = false;
	add: boolean = false;
}

export class SearchCodeCommand {

	public _searches: string = '';
	public _logicType: string = '';
	public _setID: number = 0;
	public _searchText: string = '';
	public _IDs: string = '';
	public _title: string = '';
	public _answers: string = '';
	public _included: string = 'false';
	public _withCodes: string = 'false';
	public _searchId: number = 0;
	public _contactId: number = 0;
	public _contactName: string = '';
	public _searchType: string = '';
	public _scoreOne: number = 0;
	public _scoreTwo: number = 0;

}

