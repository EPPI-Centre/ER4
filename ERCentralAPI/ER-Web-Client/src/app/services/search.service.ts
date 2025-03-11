import {  Inject, Injectable, OnDestroy, OnInit} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { EventEmitterService } from './EventEmitter.service';
import { lastValueFrom, Subscription } from 'rxjs';
import { ConfigService } from './config.service';

@Injectable({

    providedIn: 'root',

})

export class searchService extends BusyAwareService implements OnDestroy {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private EventEmitterService: EventEmitterService,
      configService: ConfigService
	) {
      super(configService);
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
	public selectedSourceDropDown: string = '';


	//@Output() searchesChanged = new EventEmitter();
    //public crit: CriteriaSearch = new CriteriaSearch();
	public searchToBeUpdated: string = '';//WHY string?

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
		this.searchToBeUpdated = "";
		this.cmdSearches = new SearchCodeCommand();
        //this.crit = new CriteriaSearch();
        //this._isBusy = false;
    }
	
	
	Delete(value: string) {

        this._BusyMethods.push("Delete");
		let body = JSON.stringify({ Value: value });
		lastValueFrom( this._httpC.post<string>(this._baseUrl + 'api/SearchList/DeleteSearch',
			body)).then(result => {
                this.RemoveBusy("Delete");
				this.Fetch();
            }, error => {
                this.RemoveBusy("Delete");
                this.modalService.GenericError(error);
            }
    ).catch(caught => {
      this.RemoveBusy("Delete");
      this.modalService.GenericError(caught);
    });
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


	public async UpdateSearchName(searchName: string, searchId: string): Promise<boolean> {
		this._BusyMethods.push("UpdateSearchName");
		let _SearchName: SearchNameUpdate = { SearchId: searchId, SearchName: searchName};
		let body = JSON.stringify(_SearchName);

    return lastValueFrom(this._httpC.post<Search>(this._baseUrl + 'api/SearchList/UpdateSearchName',
			body))
			.then(
				(result) => {
					this.RemoveBusy("UpdateSearchName");

					// just update that line (rather than reloading all searches).
					let parsedInt: number = parseInt(searchId);
					let tmpIndex: number = this.SearchList.findIndex(x => x.searchId == parsedInt);
					if (tmpIndex > -1) this.SearchList[tmpIndex].title = _SearchName.SearchName;
					return true;
				}, error => {
					this.modalService.GenericError(error); //actual error
					//this.modalService.GenericErrorMessage("There was an error updating the search name. Please contact eppisupport@ucl.ac.uk");
					this.RemoveBusy("UpdateSearchName");
					return false;
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
	public _sourceIds: string = '';
	public _searchWhat: string = '';
}

export interface SearchNameUpdate {
	SearchId: string;
	SearchName: string;
}




