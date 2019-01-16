import {  Inject, Injectable} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({

    providedIn: 'root',

})

export class searchService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
        ) {
        super();
    }



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

	//private _isBusy: boolean = false;
	//public get isBusy(): boolean {
	//	//console.log('Search list, isbusy? ' + this._isBusy);
	//	return this._isBusy;
	//}

    Fetch() {
        this._BusyMethods.push("Fetch");
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
             , () => {
                 this.RemoveBusy("Fetch");
             }
		 );
	}
    private Clear() {
        //this.crit = new CriteriaSearch();
        //this._isBusy = false;
    }
	public removeHandler({ sender, dataItem }: { sender: any, dataItem: any}) {
		
		let searchId: string = this.searchToBeDeleted;
		if (searchId != '') {
			this.Delete(searchId);
		}
		sender.cancelCell();

	}
	
	Delete(value: string) {

        this._BusyMethods.push("Delete");
		let body = JSON.stringify({ Value: value });
		this._httpC.post<string>(this._baseUrl + 'api/SearchList/DeleteSearch',
			body)
			.subscribe(result => {

					let tmpIndex: any = this.SearchList.findIndex(x => x.searchId == Number(this.searchToBeDeleted));
					this.SearchList.splice(tmpIndex, 1);
					this.Fetch();
			}, error => { this.modalService.GenericError(error); }
            , () => {
                this.RemoveBusy("Delete");
            }
			
		);

	}

	CreateSearch(cmd: SearchCodeCommand, apiStr: string) {
        this._BusyMethods.push("CreateSearch");
		apiStr = 'api/SearchList/' + apiStr;
		this._httpC.post<Search[]>(this._baseUrl + apiStr,
			cmd)

			.subscribe(result => {
				this.Fetch();
			}, error => { this.modalService.GenericError(error); }
            , () => {
                this.RemoveBusy("CreateSearch");
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

}
