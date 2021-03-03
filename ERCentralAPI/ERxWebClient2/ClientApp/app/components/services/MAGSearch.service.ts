import {  Inject, Injectable} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { MagSearch } from './MAGClasses.service';

@Injectable({
    providedIn: 'root',
})

export class magSearchService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
        ) {
        super();
    }
    public get MagSearchList() {
        return this._MagSearchList;
    }
    public set MagSearchList(value: MagSearch[]) {
        this._MagSearchList = value;
    }
    private _MagSearchList: MagSearch[] = [];
    public MAGSearchToBeDeleted: MagSearch = new MagSearch();

    FetchMAGSearchList() {
        this._BusyMethods.push("FetchMagSearchList");
        this._httpC.get<MagSearch[]>(this._baseUrl + 'api/MAGSearchList/FetchMagSearchList')
             .subscribe(result => {
                 this.RemoveBusy("FetchMagSearchList");
                 console.log('inside fetch', result);
                 for (var i = 0; i < result.length; i++) {
                     result[i].add = false;
                 }
				 this.MagSearchList = result;
             },
             error => {
                 this.RemoveBusy("FetchMagSearchList");
                 this.modalService.GenericError(error);
             }
		 );
	}
	
	Delete(magSearches: MagSearch[]) {

        this._BusyMethods.push("Delete");
        return this._httpC.post<MagSearch[]>(this._baseUrl + 'api/MAGSearchList/DeleteMagSearch',
            magSearches).toPromise()
			.then(result => {
                this.RemoveBusy("Delete");
                for (var i = 0; i < magSearches.length; i++) {
                    let magSearchToDeleteId: number = magSearches[i].magSearchId;
                    if (magSearchToDeleteId > -1) {
                        let tmpIndex: any = this.MagSearchList.findIndex(x => x.magSearchId == magSearchToDeleteId);
                        this.MagSearchList.splice(tmpIndex, 1);
                        tmpIndex = -1;
                    }
                }   
                return this.MagSearchList;
            }, error => {
                this.RemoveBusy("Delete");
                this.modalService.GenericError(error);
            }
		);
	}
    ReRunMagSearch(searchText: string, magSearchText: string) {

        this._BusyMethods.push("ReRunMagSearch");
        let body = JSON.stringify({
            searchText: searchText, magSearchText: magSearchText
        });
        return this._httpC.post<MagSearch[]>(this._baseUrl + 'api/MAGSearchList/ReRunMagSearch',
            body).toPromise()
            .then(

                (result: MagSearch[]) => {
                    this.RemoveBusy("ReRunMagSearch");
                    this.MagSearchList = result;
                    return this.MagSearchList;

                }, error => {
                    this.RemoveBusy("ReRunMagSearch");
                    this.modalService.GenericError(error);
                }
            );
    }

    CreateMagSearch(wordsInSelection: number, dateLimitSelection: number, publicationTypeSelection: number,
        magSearchInput: string, magSearchDate1: Date, magSearchDate2: Date, magSearchCurrentTopic: string) {

        this._BusyMethods.push("CreateMagSearch");
        let body = JSON.stringify({
            wordsInSelection: wordsInSelection, dateLimitSelection: dateLimitSelection, publicationTypeSelection: publicationTypeSelection,
            magSearchInput: magSearchInput, magSearchDate1: magSearchDate1, magSearchDate2: magSearchDate2,
            magSearchCurrentTopic: magSearchCurrentTopic});
         return this._httpC.post<MagSearch>(this._baseUrl + 'api/MAGSearchList/CreateMagSearch',
            body).toPromise()

            .then(

                (result: MagSearch) => {
                     this.RemoveBusy("CreateMagSearch");
                    this.MagSearchList.push(result);
                    return this.MagSearchList;
               
            }, error => {
                    this.RemoveBusy("CreateMagSearch");
                this.modalService.GenericError(error);
            }
		);
    }

    CombineSearches(magSearchListCombine: MagSearch[], logicalOperator: string) {

        this._BusyMethods.push("CombineSearches");
        let body = JSON.stringify({
            magSearchListCombine: magSearchListCombine, logicalOperator: logicalOperator });
        return this._httpC.post<MagSearch>(this._baseUrl + 'api/MAGSearchList/CombineMagSearches',
            body).toPromise()

            .then(result => {
                this.RemoveBusy("CombineSearches");
                this.MagSearchList.push(result);
                return this.MagSearchList;
            }, error => {
                    this.RemoveBusy("CombineSearches");
                this.modalService.GenericError(error);
            }
            );

    }

    ImportMagSearches(magSearchText: string, searchText: string): Promise<any> {

        this._BusyMethods.push("ImportMagSearches");
        let body = JSON.stringify({
            magSearchText: magSearchText, searchText: searchText
        });
        return this._httpC.post<MagSearch[]>(this._baseUrl + 'api/MAGSearchList/ImportMagSearchPapers',
            body).toPromise()

            .then(result => {
                this.RemoveBusy("ImportMagSearches");
                console.log(result);
                return result;                

            }, error => {
                    this.RemoveBusy("ImportMagSearches");
                this.modalService.GenericError(error);
            }
            );
    }
    public Clear() {
        this._MagSearchList = [];
        this.MAGSearchToBeDeleted = new MagSearch();
    }
}