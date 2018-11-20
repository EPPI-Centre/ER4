import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy, EventEmitter, Output, Input } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationContactList.service'
import { Criteria, ItemList } from '../services/ItemList.service'
import { WorkAllocationContactListComp } from '../WorkAllocationContactList/workAllocationContactListComp.component';
import { ItemListService } from '../services/ItemList.service'
import { ItemListComp } from '../ItemList/itemListComp.component';
import { timer, Subject, Subscription } from 'rxjs'; 
import { ReviewSetsService, singleNode } from '../services/ReviewSets.service';
import { CodesetStatisticsService, ReviewStatisticsCountsCommand } from '../services/codesetstatistics.service';
import { NgbTabset, NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { frequenciesService } from '../services/frequencies.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';
import { crosstabService } from '../services/crosstab.service';
import { searchService, SearchCodeCommand } from '../services/search.service';
import { InfoBoxModalContent } from '../reviewsets/reviewsets.component';


@Component({
    selector: 'mainfull',
    templateUrl: './mainfull.component.html'
    ,styles: [`
                .pane-content { padding: 0em 1em; margin: 1;}
               .ReviewsBg {
                    background-color:#f1f1f8 !important; 
                }
        `]
     ,providers: []

})
export class MainFullReviewComponent implements OnInit, OnDestroy {
    constructor(private router: Router,
        public ReviewerIdentityServ: ReviewerIdentityService,
        //private ReviewInfoService: ReviewInfoService,
        public reviewSetsService: ReviewSetsService,
        @Inject('BASE_URL') private _baseUrl: string,
        private _httpC: HttpClient,
        private ItemListService: ItemListService,
		private codesetStatsServ: CodesetStatisticsService,
		private _eventEmitter: EventEmitterService
		, private frequenciesService: frequenciesService
		, private crosstabService: crosstabService
		, private searchService: searchService,
		private modalService: NgbModal
    ) {

    }

    @ViewChild('WorkAllocationContactList') workAllocationsComp!: WorkAllocationContactListComp;
    @ViewChild('tabset') tabset!: NgbTabset;
	@ViewChild('ItemList') ItemListComponent!: ItemListComp;
	
	tabsInitialized: boolean = false;

    public stats: ReviewStatisticsCountsCommand | null = null;
    public countDown: any | undefined;
    public count: number = 60;
    public isReviewPanelCollapsed = false;
    public isWorkAllocationsPanelCollapsed = false;
    private statsSub: Subscription = new Subscription();
    
	public crossTabResult: any | 'none';
	//public selectedAttributeSetF: any | 'none';
	public selectedAttributeSetX: any | 'none';
	public selectedNodeDataX: any | 'none';
	public selectedNodeDataY: any | 'none';
    
	
	

    dtOptions: DataTables.Settings = {};
	dtTrigger: Subject<any> = new Subject();
	tabSelected: any = null;
	alertT() {
		this.tabset.select('ItemListTab');
	}
	setXaxis() {
		//if (this.selectedNodeData != null )
		//this.selectedNodeDataX = this.selectedNodeData;
	}
	setYaxis() {
		//if (this.selectedNodeData != null )
		//this.selectedNodeDataY = this.selectedNodeData;

	}
	setXFilter() {

		//if (this.selectedNodeData != null && this.selectedNodeData.nodeType != 'ReviewSet') {

		//	this.selectedAttributeSetX = this.selectedNodeData;
		//}
	}
	clearXFilter() {

		this.selectedAttributeSetX = null;
	}
	
	setTabSelected(tab: any) {

		this.tabSelected = tab;
		this._eventEmitter.tabSelected(tab);

		//alert(JSON.stringify(tab));
		//alert(message);
	}

	openNewSearchModal() {
	

		let modalComp = this.modalService.open(SearchesModalContent, { size: 'lg', centered: true });
	
			modalComp.componentInstance.InfoBoxTextInput = 'tester';
			modalComp.componentInstance.focus(null);

			modalComp.result.then(() => {

				//data.additionalText = infoTxt;
				//if (!data.isSelected) {

					
				//	this.CheckBoxClickedAfterCheck('InfoboxTextAdded', data);
				//}
				//else {
		
				//	this.CheckBoxClickedAfterCheck('InfoboxTextUpdate', data);
				//}
			},
				() => {

					alert('testing 123 correct');
				}
			);
	}

	
	

	ngOnInit() {

		

		this._eventEmitter.tabSelectEventf.subscribe(

			(data: any) => {
				
				this.tabset.select('ItemListTab');
			}
		)
		
        this.dtOptions = {
            pagingType: 'full_numbers',
            paging: false,
            searching: false,
            scrollY: "350px"
		};

        //this.reviewSetsService.GetReviewSets();
        this.subOpeningReview = this.ReviewerIdentityServ.OpeningNewReview.subscribe(() => this.Reload());
        this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
            () => this.GetStats()
        );
		if (this.codesetStatsServ.ReviewStats.itemsIncluded == -1
			|| (this.reviewSetsService.ReviewSets == undefined && this.codesetStatsServ.tmpCodesets == undefined)
            || (this.reviewSetsService.ReviewSets.length > 0 && this.codesetStatsServ.tmpCodesets.length == 0)
		) this.Reload();
		//this.searchService.Fetch();
    }

	//fetchFrequencies(selectedNodeDataF: any, selectedFilter: any) {
		
	//	if (!selectedNodeDataF || selectedNodeDataF == undefined) {

	//		alert('Please select a code from the tree');

	//	} else {

	//		console.log(selectedNodeDataF.name);
	//		// need to filter data before calling the below Fetch	
 //           this.frequenciesService.crit.Included = this.freqIncEx == 'true';
	//		this.frequenciesService.Fetch(selectedNodeDataF, selectedFilter);
		
	//	}
	//}

	fetchCrossTabs(selectedNodeDataX: any, selectedNodeDataY: any, selectedFilter: any) {

		if (!selectedNodeDataX || selectedNodeDataX == undefined || !selectedNodeDataY
			|| selectedNodeDataY == undefined) {

			alert('Please select both sets from the code tree');

		} else {

			//if (selectedNodeDataX.nodeType == 'ReviewSet') {
			//	let test = JSON.stringify(selectedNodeDataX.attributes);
			//	console.log('testing here1: ' + test);
			//}
			//if (selectedNodeDataY.nodeType == 'ReviewSet') {
			//	let test2 = JSON.stringify(selectedNodeDataY.attributes);
			//	console.log('testing here2: ' + test2);
			//}			

			this.crossTabResult = this.crosstabService.Fetch(selectedNodeDataX, selectedNodeDataY, selectedFilter);

		}
	}

    public get ReviewPanelTogglingSymbol(): string {
        if (this.isReviewPanelCollapsed) return '&uarr;';
        else return '&darr;';
	}

    public get WorkAllocationsPanelTogglingSymbol(): string {
        if (this.isWorkAllocationsPanelCollapsed) return '&uarr;';
        else return '&darr;';
	}

	ngAfterViewInit() {
		this.tabsInitialized = true;
		console.log('tabs initialised');
	}
	IncludedItemsList() {
		let cr: Criteria = new Criteria();
		cr.listType = 'StandardItemList';
		this.ItemListService.FetchWithCrit(cr, "Included Items");
	
		this.tabset.select('ItemListTab');
	}
	ExcludedItemsList() {
		let cr: Criteria = new Criteria();
		cr.listType = 'StandardItemList';
		cr.onlyIncluded = false;
		this.ItemListService.FetchWithCrit(cr, "Excluded Items");
		console.log('selecting tab 2...');
		this.tabset.select('ItemListTab');
	}
	GoToItemList() {
		this.tabset.select('ItemListTab');
	}
	LoadWorkAllocList(workAlloc: WorkAllocation) {
		if (this.ItemListComponent) this.ItemListComponent.LoadWorkAllocList(workAlloc, this.workAllocationsComp.ListSubType);
		else console.log('attempt failed');
	}
	ngOnChanges() {
		if (this.tabsInitialized) {
			console.log('tabs experiment');

			this.tabset.select('ItemListTab');
		}
	}
    toggleReviewPanel() {
        this.isReviewPanelCollapsed = !this.isReviewPanelCollapsed;
    }
    toggleWorkAllocationsPanel() {
        this.isWorkAllocationsPanelCollapsed = !this.isWorkAllocationsPanelCollapsed;
    }
    getDaysLeftAccount() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftAccount;
    }
    getDaysLeftReview() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftReview;
    }
    onLogin(u: string, p:string) {

        this.ReviewerIdentityServ.LoginReq(u, p);
        
    };
    subOpeningReview: Subscription | null = null;

	
	

	Reload() {
        this.Clear();
        console.log('get rev sets in mainfull');
        this.reviewSetsService.GetReviewSets();
        if (this.workAllocationsComp) this.workAllocationsComp.getWorkAllocationContactList();
        else console.log("work allocs comp is undef :-(");
    }
    GetStats() {
        console.log('getting stats (mainfull)');
        this.codesetStatsServ.GetReviewStatisticsCountsCommand();
        this.codesetStatsServ.GetReviewSetsCodingCounts(true, this.dtTrigger);
    }
    Clear() {
        console.log('Clear in mainfull');
        this.ItemListService.SaveItems(new ItemList(), new Criteria());
        //this.codesetStatsServ.
        this.reviewSetsService.Clear();
        this.codesetStatsServ.Clear();
        //this.dtTrigger.unsubscribe();
        //if (this.statsSub) this.statsSub.unsubscribe();
        //this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
        //    () => this.GetStats()
        //);
    }
  
    MyInfoMessage(): string {
        let msg: string  = "Your account expires on: ";
        let revPart: string = "";
        let AccExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.accountExpiration).toLocaleDateString();
        if (this.ReviewerIdentityServ.getDaysLeftReview() == -999999) {//review is private
            revPart = " | Current review is private (does not expire).";
        }
        else {
            let RevExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.reviewExpiration).toLocaleDateString();
            revPart = " | Current(shared) review expires on " + RevExp + ".";
        }
        msg += AccExp + revPart;
        return msg;
       
    }

    ngOnDestroy() {
        this.Clear();
        if (this.subOpeningReview) {
            this.subOpeningReview.unsubscribe();			
        }
        if (this.statsSub) this.statsSub.unsubscribe();
    }
}

export class RadioButtonComp {
	IncEnc = true;
}
@Component({
	selector: 'ngbd-SearchesModal-content',
	templateUrl: './SearchesModal.component.html'
})
export class SearchesModalContent implements SearchCodeCommand {

	@ViewChild('SearchesModal')

	//InfoBoxText!: ElementRef;

	private canWrite: boolean = true;
	public dropDownList: any = null;
	public showTextBox: boolean = false;
	public showDropDown2: boolean = true;
	public selectedSearchDropDown: string = '';
	public nodeSelected: boolean = false;
	public selectedNodeDataName: string = '';

	_title: string = '';
	_answers: string = '';
	_included: boolean = false;
	_withCodes: boolean = false;;
	_searchId: number = 0;

	public get IsReadOnly(): boolean {

		return this.canWrite;

	}
	constructor(public activeModal: NgbActiveModal,
		private reviewSetsService: ReviewSetsService,
		private _eventEmitter: EventEmitterService,
		private _searchService: searchService
	) { }

	test() {

		alert('hello again');

	}

	public cmdSearches: SearchCodeCommand = {

		_title : '',
		_answers : '',
		_included : false,
		_withCodes : false,
		_searchId : 0
	};

	callSearches(selectedSearchDropDown: string, searchBool: boolean) {

		// api call to SearchListController for the SearchCodes
		this.cmdSearches._title = selectedSearchDropDown
			//'Not coded with: control (comparison TP)';
		this.cmdSearches._answers = ''; //'83962';
		this.cmdSearches._included =  Boolean(searchBool);
		this.cmdSearches._withCodes = false;
		this.cmdSearches._searchId = 0;

		console.log('variables: ' + selectedSearchDropDown + ', ' + searchBool);

		this._searchService.FetchSearchCodes(this.cmdSearches);

		this.activeModal.dismiss();
	}

	public nextDropDownList(num: number, val: string) {

		//console.log('got here');
		this.showDropDown2 = true;
		this.showTextBox = false;
		this.selectedSearchDropDown = val;
		switch (num) {

			case 1: {
				this.dropDownList = this.reviewSetsService.ReviewSets;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			case 2: {
				//statements; 
				this.dropDownList = this.reviewSetsService.ReviewSets;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			case 3: {
				//With these internal IDs (comma separated) show text box
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = false;
				this.showTextBox = true;
				break;
			}
			case 4: {
				//statements; 
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = true;
				this.showTextBox = true;
				break;
			}
			case 5: {
				//that have at least one code from this set
				this._eventEmitter.nodeSelected = false;
				this.showTextBox = false;
				this.showDropDown2 = true;
				this.dropDownList = this.reviewSetsService.ReviewSets;
				break;
			}
			case 6: {
				//that don't have any codes from this set
				this._eventEmitter.nodeSelected = false;
				this.showTextBox = false;
				this.showDropDown2 = true;
				this.dropDownList = this.reviewSetsService.ReviewSets;
				break;
			}
			case 7: {
				//statements; 
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = true;
				this.showTextBox = false;
				break;
			}
			case 8: {
				//statements; 
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			case 9: {
				//statements;
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			case 10: {
				//statements; 
				this._eventEmitter.nodeSelected = false;
				this.showDropDown2 = false;
				this.showTextBox = false;
				break;
			}
			default: {
				//statements; 
				break;
			}
		}
	}

	public focus(canWrite: boolean) {
		this.canWrite = canWrite;
		//this.InfoBoxText.nativeElement.focus();
	}
}

