import { Injectable, EventEmitter } from '@angular/core';
import { singleNode } from './ReviewSets.service';

@Injectable()
export class EventEmitterService {
    public PleaseSelectItemsListTab = new EventEmitter();
    public criteriaMAGChange = new EventEmitter<string>();
    public MAGAllocationClicked = new EventEmitter();
	public criteriaComparisonChange = new EventEmitter();
	public reconDataChanged  = new EventEmitter();
	public nodeSelected: singleNode | null | undefined;
	public nodeName: string = '';
	public UserInput: string = '';
    public CloseReportsSectionEmitter = new EventEmitter();
	//public allocateRelevantItems = new EventEmitter();
	//public configurableReports = new EventEmitter();
	

	public tester() {

		alert('event emitter getting called here!!');
	}

	constructor() {

	}
}