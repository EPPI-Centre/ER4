import {  Inject, Injectable, EventEmitter, Output} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';

@Injectable({

    providedIn: 'root',

})

export class searchService {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
        ) { }
    
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

	private _isBusy: boolean = false;
	public get isBusy(): boolean {
		//console.log('Search list, isbusy? ' + this._isBusy);
		return this._isBusy;
	}

    Fetch() {
        this._isBusy = true;
		 this._httpC.get<Search[]>(this._baseUrl + 'api/SearchList/GetSearches')
			.subscribe(result => {
					console.log('alkjshdf askljdfh' + JSON.stringify(result));
					this.SearchList = result;
					//this.searchesChanged.emit();
             },
             error => {
                 this.modalService.GenericError(error);
                 this.Clear();
             }
             , () => { this._isBusy = false;}
		 );
	}
    private Clear() {
        //this.crit = new CriteriaSearch();
        this._isBusy = false;
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

	CreateSearch(cmd: SearchCodeCommand, apiStr: string) {

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
