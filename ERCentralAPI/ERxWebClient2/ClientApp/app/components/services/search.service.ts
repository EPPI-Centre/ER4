import {  Inject, Injectable, EventEmitter, Output} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { Observable } from 'rxjs';

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
		if (this._SearchList.length == 0) {

			const SearchListJson = localStorage.getItem('SearchList');
			let SearchList: Search[] = SearchListJson !== null ? JSON.parse(SearchListJson) : [];
			if (SearchList == undefined || SearchList == null || SearchList.length == 0) {
				return this._SearchList;
            }
            else {
				this._SearchList = SearchList;
            }
        }
		return this._SearchList;

    }
    
	public set SearchList(searches: Search[]) {
		this._SearchList = searches;
        this.Save();
        this.searchesChanged.emit();
    }

	Fetch() {

		 this._httpC.post<Search[]>(this._baseUrl + 'api/SearchList/GetSearches',
			this.crit)

			.subscribe(result => {

					console.log('AAAAgot inside searches: ' + this.crit.SetId);
					this.SearchList = result;
					console.log(this._SearchList.length);
					this.Save();
				
					this.searchesChanged.emit();

				//return result;

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


	Delete(_searches: string) {

		this._httpC.post<number>(this._baseUrl + 'api/SearchList/DeleteSearch',
			_searches)

			.subscribe(result => {

					let tmpIndex: any = this.SearchList.findIndex(x => x.searchId == Number(this.searchToBeDeleted));
					this.SearchList.splice(tmpIndex, 1);
					console.log(this._SearchList.length);
					this.Save();

				}
			);
	}


	FetchSearchCodes(cmd: SearchCodeCommand) {

		console.log(cmd);
		this._httpC.post<Search[]>(this._baseUrl + 'api/SearchList/SearchCodes',
			cmd)

			.subscribe(result => {

				console.log('silly call to the server again: ' + JSON.stringify(result));

				//this.crit.AttributeId = result;
				//this.crit.FilterAttributeId = result;
				//this.crit.Included = result;
				//this.crit.SetId = result;

				this.Fetch();

				//this.SearchList = result;
				//console.log(this._SearchList.length);
				//this.Save();
				//console.log(result);
				//this.searchesChanged.emit();

				//return result;

			}
		);
	}

    public Save() {
		if (this._SearchList.length > 0)
			localStorage.setItem('SearchList', JSON.stringify(this._SearchList));
		else if (localStorage.getItem('SearchList'))
			localStorage.removeItem('SearchList');
    }
}

export class Search {

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

	_title: string;
	_answers: string ;
	_included: boolean ;
	_withCodes: boolean ;
	_searchId: number ;

}
