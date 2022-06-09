import {  Inject, Injectable, EventEmitter, Output} from '@angular/core';
import { HttpClient   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ConfigService } from './config.service';

@Injectable({

    providedIn: 'root',

})

export class frequenciesService extends BusyAwareService  {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
      configService: ConfigService
    ) { super(configService); }
    
	private _FrequencyList: Frequency[] = [];
	@Output() codeSelectedChanged = new EventEmitter();
	@Output() frequenciesChanged = new EventEmitter();
	public crit: CriteriaFrequency = new CriteriaFrequency();

	public get Frequencies(): Frequency[] {
   //     if (this._FrequencyList.length == 0) {

			//const FrequenciesJson = localStorage.getItem('Frequencies');
			//let Frequencies: Frequency[] = FrequenciesJson !== null ? JSON.parse(FrequenciesJson) : [];
			//if (Frequencies == undefined || Frequencies == null || Frequencies.length == 0) {
   //             return this._FrequencyList;
   //         }
   //         else {
			//	this._FrequencyList = Frequencies;
   //         }
   //     }
		return this._FrequencyList;

    }
    
	public set Frequencies(freq: Frequency[]) {

        this._FrequencyList = freq;
        //this.Save();
    }

	public Fetch(selectedNodeData: any, selectedFilter: number | null) {
        this._BusyMethods.push("Fetch");
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
             this.crit)
             .subscribe(result => {
                 this.Frequencies = result;
                 //this.Save();
                 //console.log(result);
                 this.frequenciesChanged.emit();
                 this.RemoveBusy("Fetch");
				},
             (error) => {
                 console.log("Frequency fetch error:", error);
                 this.RemoveBusy("Fetch");
                 this.modalService.GenericError(error);
             }
		 );
    }

    public Clear() {
        this._FrequencyList = [];
        this.crit =  new CriteriaFrequency();
    }
}

export class Frequency {

	attributeId: number = 0;
	itemCount: string = "";
	attribute: string="";
	attributeSetId: number = 0;
	setId: number = 0;
	filterAttributeId: number = 0;
	isIncluded: boolean | null = false;
}

export class CriteriaFrequency {
	
	AttributeId: string = '0';
	SetId: string ='0';
	Included: boolean | null = false;
	FilterAttributeId: number = 0;
	
}
