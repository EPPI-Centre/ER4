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
	
    public MagSearchList: MagSearch[] = [];
    public MAGSearchToBeDeleted: MagSearch = new MagSearch();

    FetchMAGSearchList() {
        this._BusyMethods.push("FetchMagSearchList");
        this._httpC.get<MagSearch[]>(this._baseUrl + 'api/MAGSearchList/FetchMagSearchList')
             .subscribe(result => {
                 this.RemoveBusy("FetchMagSearchList");
                 console.log('inside fetch', result);
				this.MagSearchList = result;
             },
             error => {
                 this.RemoveBusy("FetchMagSearchList");
                 this.modalService.GenericError(error);
             }
		 );
	}
	
	Delete(value: string) {

        this._BusyMethods.push("Delete");
		let body = JSON.stringify({ Value: value });
		this._httpC.post<string>(this._baseUrl + 'api/MAGSearchList/DeleteSearch',
			body)
			.subscribe(result => {
                this.RemoveBusy("Delete");
                let tmpIndex: any = this.MagSearchList.findIndex(x => x.magSearchId == Number(this.MAGSearchToBeDeleted));
                this.MagSearchList.splice(tmpIndex, 1);
				this.FetchMAGSearchList();
            }, error => {
                this.RemoveBusy("Delete");
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
        this._httpC.post<MagSearch[]>(this._baseUrl + 'api/MAGSearchList/CreateMagSearch',
            body)

            .subscribe(result => {
                this.RemoveBusy("CreateMagSearch");
                this.MagSearchList = result;
                this.FetchMAGSearchList();
            }, error => {
                    this.RemoveBusy("CreateMagSearch");
                this.modalService.GenericError(error);
            }
		);
	}
}