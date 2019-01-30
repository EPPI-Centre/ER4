import {  Inject, Injectable} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Observable } from 'rxjs';

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

	private _SearchVisualiseData!: Observable<any>;

	public get SearchVisualiseData(): Observable<any> {

		return this._SearchVisualiseData;

	}

	public set SearchVisualiseData(searches: Observable<any>) {
		this._SearchVisualiseData = searches;
		//this.searchesChanged.emit();
	}

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

	CreateVisualiseCodeSet(visualiseTitle : string, visualiseSearchId : number,
		node: any) :Promise<any> {


		alert(visualiseTitle + ' ' + visualiseSearchId + ' ' +
			node.attribute_id + ' ' + node.set_id);

		//this._BusyMethods.push("CreateVisualiseCodeSet");

		let body = JSON.stringify({
			searchName: visualiseTitle, searchId: visualiseSearchId,
			attributeId: node.attribute_id, setId: node.set_id
		});

		console.log(body);

		return this._httpC.post<any>(this._baseUrl + 'api/SearchList/CreateVisualiseCodeSet', body)
			.toPromise();

		//result => {

		//		console.log('CreateVisualiseCodeSet' + JSON.stringify(result));
		//		//this.SearchVisualiseData = result;
		//		this.RemoveBusy("CreateVisualiseCodeSet");
		//		//return result;

		//	},
		//		error => {
		//			this.RemoveBusy("CreateVisualiseCodeSet");
		//			this.modalService.GenericError(error);
		//			this.Clear();
		//		}
		//	);
		//return this.SearchVisualiseData;

	}

	CreateVisualiseData(searchId: number) : Observable<any> {
		
		this._BusyMethods.push("CreateVisualiseData");
		let body = JSON.stringify({ searchId: searchId });
		//alert('got in here..:' + body);
		this._httpC.post<Observable<any>>(this._baseUrl + 'api/SearchList/CreateVisualiseData', body)
			.subscribe(result => {

				//console.log('alkjshdf askljdfh' + JSON.stringify(result));
				this.SearchVisualiseData = result;
				this.RemoveBusy("CreateVisualiseData");
				return result;
		
			},
				error => {
					this.RemoveBusy("CreateVisualiseData");
					this.modalService.GenericError(error);
					this.Clear();
				}
		);
		return this.SearchVisualiseData;
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
    private Clear() {
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
