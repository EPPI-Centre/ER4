import {  Inject, Injectable, EventEmitter, Output} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { ITreeNode, TreeNode } from 'angular-tree-component/dist/defs/api';
import { singleNode } from './ReviewSets.service';
import { Observable } from 'rxjs';
import { RequestOptions } from '@angular/http';
import { WebApiObservableService } from './httpQuery.service';

@Injectable({

    providedIn: 'root',

})

export class frequenciesService {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private webApiObservableService: WebApiObservableService,
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

	public Fetch(selectedNodeData: singleNode) {

		let crit: Criteria = new Criteria();
		crit.AttributeId = 0;
		crit.FilterAttributeId = -1;
		crit.Included = true;
		crit.SetId = 27;

		//this.webApiObservableService.getServiceWithComplexObjectAsQueryString(

		//let body = JSON.stringify({ Value: crit });
		this._httpC.post<Frequency[]>(this._baseUrl + 'api/Frequencies/GetFrequencies',
			crit).subscribe(result => {

					this.Frequencies = result;
					console.log(result);

				}
			);

		//if (selectedNodeData.nodeType == 'ReviewSet') {

		//	return this._httpC.get<Frequency[]>(this._baseUrl + 'api/Frequencies/GetFrequencies',
		//		{
		//			params: {
		//				set_id: '27',
		//				//attribute_id: '0',
		//				//isIncluded: 'true',
		//				//filterAttributeId: '-1'
						
		//			}
		//		})
		//		.subscribe(result => {

		//			this.Frequencies = result;
		//			console.log(result);

		//		}, error => { this.modalService.GenericError(error); }
		//	);

		//} else {

		//	alert('not implemented yet');
		//}

		//console.log('attempted to fetch freq: ' + selectedNodeData.nodeType);

		//let type: string = '';
		//let set_id: number = 27;
		//let attribute_id = 0;
		//let isIncluded = true;
		//let filterAttributeId = -1;
		//let body = JSON.stringify({ set_id,  attribute_id, isIncluded ,  filterAttributeId});

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

export class Criteria {
	
	AttributeId: number = 0;
	SetId: number = 0;
	Included: boolean = false;
	FilterAttributeId: number = 0;
	
}
