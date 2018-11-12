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
	@Output() frequenciesChanged = new EventEmitter();
	public crit: Criteria = new Criteria();

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

        this._FrequencyList = freq;
        this.Save();
    }

	public Fetch(selectedNodeData: any, selectedFilter?: any) {

		this.codeSelectedChanged.emit(selectedNodeData);
				
		//console.log('Inside the service now: ' + selectedFilter);
		if (selectedFilter != null) {

			this.crit.FilterAttributeId = selectedFilter;
		} else {

			this.crit.FilterAttributeId = -1;
		}

		if (selectedNodeData.nodeType == 'ReviewSet') {

			this.crit.AttributeId = '0';
			//this.crit.FilterAttributeId = -1;
			//need to get this from the page
			//this.crit.Included = true;
			this.crit.SetId = selectedNodeData.id.substr(2, selectedNodeData.id.length);

		} else {

			this.crit.AttributeId = selectedNodeData.id.substr(1, selectedNodeData.id.length);
			//this.crit.FilterAttributeId = -1;
			//this.crit.Included = true;
			this.crit.SetId = selectedNodeData.set_id;
		}

		this._httpC.post<Frequency[]>(this._baseUrl + 'api/Frequencies/GetFrequencies',
			this.crit).subscribe(result => {

					this.Frequencies = result;
					this.Save();
					console.log(result);
					this.frequenciesChanged.emit();
				}
		 );
    }

    public Save() {
		if (this._FrequencyList.length > 0)
			localStorage.setItem('Frequencies', JSON.stringify(this._FrequencyList));
		else if (localStorage.getItem('Frequencies'))
			localStorage.removeItem('Frequencies');
    }
}

export class Frequency {

	attributeId: number = 0;
	itemCount: string = "";
	attribute: string="";
	attributeSetId: number = 0;
	setId: number = 0;
	filterAttributeId: number = 0;
	isIncluded: boolean = false;
}

export class Criteria {
	
	AttributeId: string = '0';
	SetId: string ='0';
	Included: boolean = false;
	FilterAttributeId: number = 0;
	
}
