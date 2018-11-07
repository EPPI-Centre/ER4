import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy, EventEmitter, Output } from '@angular/core';
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
import { frequenciesComp } from '../Frequencies/frequencies.component';
import { searchService } from '../services/search.service';
import { InfoBoxModalContent } from '../reviewsets/reviewsets.component';


@Component({
    selector: 'mainfull',
    templateUrl: './mainfull.component.html'
    ,styles: [`
                
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
        private reviewSetsService: ReviewSetsService,
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
	public selectedNodeData: any | 'none';
	public selectedAttributeSet: any | 'none';
	public selectedNodeDataX: any | 'none';
	public selectedNodeDataY: any | 'none';
	public radioData: any;

	show(value: any) {

		this._eventEmitter.showData(value);
		
	}
	test() {
		alert('hello again');
	}
	clearReviewSet() {
		this.selectedNodeData = null;
	}
	clearAttributeSet() {
		this.selectedAttributeSet = null;
	}
    dtOptions: DataTables.Settings = {};
    dtTrigger: Subject<any> = new Subject();
	alertT() {
		this.tabset.select('ItemListTab');
	}
	setXaxis() {
		this.selectedNodeDataX = this.selectedNodeData;
	}
	setYaxis() {
		this.selectedNodeDataY = this.selectedNodeData;
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

		this._eventEmitter.dataStr.subscribe(

			(data: any) => {

				if (data != null) {

					//console.log(data.name + ' ====> ' + data.nodeType);
					if (data.nodeType != 'ReviewSet') {

						this.selectedAttributeSet = data;
						console.log(data.attribute_id);

					} else {

						this.selectedNodeData = data;
					}
				}
			}
		)

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

		this.reviewSetsService.GetReviewSets();
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

	fetchFrequencies(selectedNodeData: any, selectedFilter: any) {
		
		if (!selectedNodeData || selectedNodeData == undefined) {

			alert('Please select a code from the tree');

		} else {

			console.log(selectedNodeData.name);
			// need to filter data before calling the below Fetch			
			this.frequenciesService.Fetch(selectedNodeData, selectedFilter);
		
		}
	}

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

	changeRadioValue(IncEnc: any) {

		this.frequenciesService.crit.Included = IncEnc;
	}
	

	Reload() {
        this.Clear();
        this.reviewSetsService.GetReviewSets();
        if (this.workAllocationsComp) this.workAllocationsComp.getWorkAllocationContactList();
        else console.log("work allocs comp is undef :-(");
    }
    GetStats() {
        this.codesetStatsServ.GetReviewStatisticsCountsCommand();
        this.codesetStatsServ.GetReviewSetsCodingCounts(true, this.dtTrigger);
    }
    Clear() {
        this.ItemListService.SaveItems(new ItemList(), new Criteria());
        //this.codesetStatsServ.
        this.reviewSetsService.Clear();
        //this.dtTrigger.unsubscribe();
        if (this.statsSub) this.statsSub.unsubscribe();
        this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(
            () => this.GetStats()
        );
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
        if (this.subOpeningReview) {
            this.subOpeningReview.unsubscribe();
			
        }
    }
}

export class RadioButtonComp {
	IncEnc = true;
}
@Component({
	selector: 'ngbd-SearchesModal-content',
	templateUrl: './SearchesModal.component.html'
})
export class SearchesModalContent {

	@ViewChild('SearchesModal')

	//InfoBoxText!: ElementRef;

	private canWrite: boolean = true;
	public reviewSetList: any = null;

	public get IsReadOnly(): boolean {

		return this.canWrite;

	}
	constructor(public activeModal: NgbActiveModal,
		private reviewSetsService: ReviewSetsService,
		private EventEmitterService: EventEmitterService
	) { }

	test() {
		alert('hello again');
	}

	public nextDropDownList(num: number) {

		console.log('got here');
		//this.EventEmitterService.dropDownSelect(num);

		switch (num) {

			case 1: {
				alert('you hit 1!');
				this.reviewSetList = this.reviewSetsService.ReviewSets;
				break;
			}
			case 2: {
				//statements; 
				break;
			}
			case 3: {
				//statements; 
				break;
			}
			case 4: {
				//statements; 
				break;
			}
			case 5: {
				//statements; 
				break;
			}
			case 6: {
				//statements; 
				break;
			}
			case 7: {
				//statements; 
				break;
			}
			case 8: {
				//statements; 
				break;
			}
			case 9: {
				//statements; 
				break;
			}
			case 10: {
				//statements; 
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
