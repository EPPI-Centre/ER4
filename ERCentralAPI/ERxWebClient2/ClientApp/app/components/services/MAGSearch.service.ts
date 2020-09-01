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

    Fetch() {
        this._BusyMethods.push("Fetch");
		 this._httpC.get<MagSearch[]>(this._baseUrl + 'api/MAGSearchList/GetSearches')
             .subscribe(result => {
                 this.RemoveBusy("Fetch");
				
				this.MagSearchList = result;
             },
             error => {
                 this.RemoveBusy("Fetch");
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
				this.Fetch();
            }, error => {
                this.RemoveBusy("Delete");
                this.modalService.GenericError(error);
            }
		);
	}

 //   CreateSearch(cmd: SearchCodeCommand, apiStr: string) {

 //       this._BusyMethods.push("CreateSearch");
	//	apiStr = 'api/SearchList/' + apiStr;
	//	this._httpC.post<Search[]>(this._baseUrl + apiStr,
	//		cmd)

 //           .subscribe(result => {
 //               this.RemoveBusy("CreateSearch");
	//			this.Fetch();
 //           }, error => {
 //               this.RemoveBusy("CreateSearch");
 //               this.modalService.GenericError(error);
 //           }
	//		);
	//}
}