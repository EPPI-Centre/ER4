import { Injectable, EventEmitter } from '@angular/core';
import { singleNode } from './ReviewSets.service';
import { topicInfo } from './MAGClasses.service';

@Injectable()
export class EventEmitterService {
    public PleaseSelectItemsListTab = new EventEmitter();
    public criteriaMAGChange = new EventEmitter<string>();
	public criteriaComparisonChange = new EventEmitter();
	public reconDataChanged  = new EventEmitter();
	public nodeSelected: singleNode | null | undefined;
	public nodeName: string = '';
	public UserInput: string = '';
	public tool: boolean = false;
	public selectedButtonPressed: EventEmitter<boolean> = new EventEmitter();//passes "true" as value if it the listener needs to create the browsehistory entry
	//public getMatchedIncludedItemsEvent: EventEmitter<boolean> = new EventEmitter();
	//public getMatchedExcludedItemsEvent: EventEmitter<boolean> = new EventEmitter();
	//public getMatchedAllItemsEvent: EventEmitter<boolean> = new EventEmitter();
	//public getTopicsEvent: EventEmitter<topicInfo> = new EventEmitter<topicInfo>();
	public OpeningNewReview = new EventEmitter();
	public PleaseClearYourDataAndState = new EventEmitter();
	//public firstVisitMAGBrowserPage: boolean = true;	


	constructor() {

	}
}
