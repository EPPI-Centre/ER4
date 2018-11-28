import {  Inject, Injectable, EventEmitter, Output} from '@angular/core';
import { HttpClient   } from '@angular/common/http';

@Injectable({

    providedIn: 'root',

})

export class searchService {

    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string
        ) { }
    
	private _SearchList: Search[] = [];
	@Output() searchesChanged = new EventEmitter();
    public crit: CriteriaSearch = new CriteriaSearch();
	public searchToBeDeleted: string = '';

	public get SearchList(): Search[] {

		return this._SearchList;

    }
    
	public set SearchList(searches: Search[]) {
		this._SearchList = searches;
        this.searchesChanged.emit();
	}

	private _isBusy: boolean = false;
	public get isBusy(): boolean {
		console.log('Search list, isbusy? ' + this._isBusy);
		return this._isBusy;
	}

	Fetch() {

		 this._httpC.post<Search[]>(this._baseUrl + 'api/SearchList/GetSearches',
			this.crit)

			.subscribe(result => {

					console.log('alkjshdf askljdfh' + JSON.stringify(result));
					this.SearchList = result;
					this.searchesChanged.emit();
				}
		 );
	}

	public removeHandler({ sender, dataItem }: { sender: any, dataItem: any}) {
		
		let searchId: string = this.searchToBeDeleted;
		if (searchId != '') {
			this.Delete(searchId);
		}
		sender.cancelCell();

	}
	
	Delete(value: string) {

		let body = JSON.stringify({ Value: value });
		this._httpC.post<string>(this._baseUrl + 'api/SearchList/DeleteSearch',
			body)
			.subscribe(result => {

					let tmpIndex: any = this.SearchList.findIndex(x => x.searchId == Number(this.searchToBeDeleted));
					this.SearchList.splice(tmpIndex, 1);
					this.Fetch();
				}
		);
	}

	FetchSearchGeneric(cmd: SearchCodeCommand, apiStr: string) {

		apiStr = 'api/SearchList/' + apiStr;
		this._httpC.post<Search[]>(this._baseUrl + apiStr,
			cmd)

			.subscribe(result => {
					this.Fetch();
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
}

export class CriteriaSearch {
	
	AttributeId: string = '0';
	SetId: string ='0';
	Included: boolean = false;
	FilterAttributeId: number = 0;
	
}

export interface SearchCodeCommand {

	_setID: number;
	_searchText: string;
	_IDs: string;
	_title: string;
	_answers: string ;
	_included: boolean ;
	_withCodes: boolean ;
	_searchId: number ;

}
