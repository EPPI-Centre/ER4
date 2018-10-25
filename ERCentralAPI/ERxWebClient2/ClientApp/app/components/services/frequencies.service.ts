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

	public Fetch(selectedNodeData: any) {

		this.codeSelectedChanged.emit(selectedNodeData);

		let crit: Criteria = new Criteria();
		console.log('stuff1' + selectedNodeData.id);
		if (selectedNodeData.nodeType == 'ReviewSet') {

			crit.AttributeId = '0';
			crit.FilterAttributeId = -1;
			//need to get this from the page
			crit.Included = true;
			crit.SetId = selectedNodeData.id.substr(2, selectedNodeData.id.length);
			//console.log('stuff2' + crit.AttributeId + ' ' + crit.SetId);

		} else {

			crit.AttributeId = selectedNodeData.id.substr(1, selectedNodeData.id.length);
			crit.FilterAttributeId = -1;
			crit.Included = true;
			crit.SetId = selectedNodeData.set_id;
			//console.log('stuff3' + crit.AttributeId + ' ' + crit.SetId);
		}

		console.log('stuff4' + crit.AttributeId + ' ' + crit.SetId);

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
	itemCount: string = "";
	attribute: string="";
	attributeSetId: string = "";
	setId: number = 0;
	filterAttributeId: string = "";
	isIncluded: boolean = false;
}

export class Criteria {
	
	AttributeId: string = '0';
	SetId: string ='0';
	Included: boolean = false;
	FilterAttributeId: number = 0;
	
}
