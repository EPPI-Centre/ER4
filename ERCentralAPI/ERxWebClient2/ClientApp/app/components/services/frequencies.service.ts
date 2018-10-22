import {  Inject, Injectable, EventEmitter, Output} from '@angular/core';
import { Subject } from 'rxjs';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';


@Injectable({
    providedIn: 'root',
})

export class frequenciesService {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
        ) { }
    
	private _FrequencyList: Frequency[] = [];
	@Output() codeSelectedChanged = new EventEmitter();

	public get Frequencies(): Frequency[] {
        if (this._FrequencyList.length == 0) {

			const FrequenciesJson = localStorage.getItem('Frequencies');
			let Frequencies: Frequency[] = FrequenciesJson !== null ? JSON.parse(FrequenciesJson) : [];
			if (Frequencies == undefined || Frequencies == null || Frequencies.length == 0) {
                return this._FrequencyList;
            }
            else {
				this._FrequencyList = Frequencies;
            }
        }
		return this._FrequencyList;

    }
    
	public set Frequencies(freq: Frequency[]) {

        //for (var i = 0; i < ror.length; i++) {

        //    var temp = new ReadOnlyReview(ror[i].reviewId,
        //        ror[i].reviewName,
        //        ror[i].reviewOwner,
        //        ror[i].contactReviewRoles,
        //        ror[i].lastAccess);

        //    this.ReadOnlyReviews[i] = temp;

        //}

        this._FrequencyList = freq;

        //this.Save();
    }
    
        
    public Fetch() {

		console.log('attempted to fetch freq');

		return this._httpC.get<Frequency[]>(this._baseUrl + 'api/Frequencies/GetFrequencies')
           
            .subscribe(result => {

                //for (var i = 0; i < result.length; i++) {

                //    var temp = new ReadOnlyReview(result[i].reviewId,
                //        result[i].reviewName,
                //        result[i].reviewOwner,
                //         result[i].contactReviewRoles,
                //        result[i].lastAccess);

                //    this.ReadOnlyReviews[i] = temp;
                  
                //}


				this.Frequencies = result;
    //            dtTrigger.next();
                
                console.log(result);
          
        }, error => { this.modalService.GenericError(error); }
          
        );
    }


    //public Save() {
    //    if (this._ReviewList.length > 0)
    //        localStorage.setItem('ReadOnlyReviews', JSON.stringify(this._ReviewList));
    //    else if (localStorage.getItem('ReadOnlyReviews'))//to be confirmed!! 
    //        localStorage.removeItem('ReadOnlyReviews');
    //}
}

export class Frequency {

	attributeId: number = 0;
	itemCount: number = 0;
	attribute: string="";
	attributeSetId: number = 0;
	setId: string = "";
	filterAttributeId: number = 0;
	isIncluded: boolean = false;
}
