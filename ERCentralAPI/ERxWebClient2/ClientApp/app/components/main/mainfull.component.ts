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
import { ReviewSetsService } from '../services/ReviewSets.service';
import { CodesetStatisticsService, ReviewStatisticsCountsCommand } from '../services/codesetstatistics.service';
import { NgbTabset } from '@ng-bootstrap/ng-bootstrap';
import { frequenciesService } from '../services/frequencies.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { crosstabService } from '../services/crosstab.service';
import { searchService } from '../services/search.service';
import { SourcesService } from '../services/sources.service';
import { SelectEvent, TabStripComponent } from '@progress/kendo-angular-layout';



@Component({
    selector: 'mainComp',
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
        private _eventEmitter: EventEmitterService,
		private frequenciesService: frequenciesService
		, private crosstabService: crosstabService
        , private _searchService: searchService
        , private SourcesService: SourcesService
    ) {}
    @ViewChild('WorkAllocationContactList') workAllocationsComp!: WorkAllocationContactListComp;
    @ViewChild('tabstrip') public tabstrip!: TabStripComponent;
    //@ViewChild('tabset') tabset!: NgbTabset;
	@ViewChild('ItemList') ItemListComponent!: ItemListComp;

	
	tabsInitialized: boolean = false;

    public stats: ReviewStatisticsCountsCommand | null = null;
    public countDown: any | undefined;
    public count: number = 60;
    public isSourcesPanelCollapsed = false;
    public isReviewPanelCollapsed = false;
    public isWorkAllocationsPanelCollapsed = false;
    private statsSub: Subscription = new Subscription();
    private InstanceId: number = Math.random();
	public crossTabResult: any | 'none';
	//public selectedAttributeSetF: any | 'none';
	
    	
	dtTrigger: Subject<any> = new Subject();
	tabSelected: any = null;
	
	
    setTabSelected(tabSelect: SelectEvent) {
		//nothing for now, selectEvent is like this:
        //index: number
        //title: string
	}
	BuildModel() {
		this.router.navigate(['BuildModel']);

	}

    ngOnInit() {
        console.log("MainComp init:", this.InstanceId);
        this._eventEmitter.PleaseSelectItemsListTab.subscribe(
            () => {
                this.tabstrip.selectTab(1);
            }
        )


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

	
    public get ReviewPanelTogglingSymbol(): string {
        if (this.isReviewPanelCollapsed) return '&uarr;';
        else return '&darr;';
	}

    public get WorkAllocationsPanelTogglingSymbol(): string {
        if (this.isWorkAllocationsPanelCollapsed) return '&uarr;';
        else return '&darr;';
	}
    public get SourcesPanelTogglingSymbol(): string {
        if (this.isSourcesPanelCollapsed) return '&uarr;';
        else return '&darr;';
    }
	ngAfterViewInit() {
		this.tabsInitialized = true;
		console.log('tabs initialised');
	}
	IncludedItemsList() {
        this.IncludedItemsListNoTabChange();
		this.tabstrip.selectTab(1);
    }
    IncludedItemsListNoTabChange() {
        let cr: Criteria = new Criteria();
        cr.listType = 'StandardItemList';
        this.ItemListService.FetchWithCrit(cr, "Included Items");
    }

	ExcludedItemsList() {
		let cr: Criteria = new Criteria();
		cr.listType = 'StandardItemList';
		cr.onlyIncluded = false;
		this.ItemListService.FetchWithCrit(cr, "Excluded Items");
		console.log('selecting tab 2...');
		this.tabstrip.selectTab(1);
	}
	GoToItemList() {
		this.tabstrip.selectTab(1);
	}
	LoadWorkAllocList(workAlloc: WorkAllocation) {
		if (this.ItemListComponent) this.ItemListComponent.LoadWorkAllocList(workAlloc, this.workAllocationsComp.ListSubType);
		else console.log('attempt failed');
	}
	ngOnChanges() {
		if (this.tabsInitialized) {
			console.log('tabs experiment');

			this.tabstrip.selectTab(1);
		}
	}
    toggleReviewPanel() {
        this.isReviewPanelCollapsed = !this.isReviewPanelCollapsed;
    }
    toggleWorkAllocationsPanel() {
        this.isWorkAllocationsPanelCollapsed = !this.isWorkAllocationsPanelCollapsed;
    }
    toggleSourcesPanel() {
        if (!this.isSourcesPanelCollapsed) {
            this.SourcesService.FetchSources();
        }
        this.isSourcesPanelCollapsed = !this.isSourcesPanelCollapsed;
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
        console.log('Reload mainfull');
        this.reviewSetsService.GetReviewSets();
        this
        if (this.workAllocationsComp) this.workAllocationsComp.getWorkAllocationContactList();
        else console.log("work allocs comp is undef :-(");
        if (this.ItemListService.ListCriteria && this.ItemListService.ListCriteria.listType == "") 
            this.IncludedItemsListNoTabChange();
    }
    //GetStatsFromSubscription() {
    //    //we unsubscribe here as we won't use this again.
    //    if (this.statsSub) {
    //        console.log("(mainfull GetStatsFromSubscription) I should not happen more than once");
    //        this.statsSub.unsubscribe();
    //        this.statsSub = new Subscription();
    //    }
    //    this.GetStats();
    //}
    GetStats() {
        console.log('getting stats (mainfull):', this.InstanceId);
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
    EditCodeSets() {
        this.router.navigate(['EditCodeSets']);
    }
    GoToSources() {
        this.router.navigate(['sources']);
    }
    ImportCodesetClick() {
        this.router.navigate(['ImportCodesets']);
    }
    ngOnDestroy() {
        //this.Clear();
        if (this.subOpeningReview) {
            this.subOpeningReview.unsubscribe();			
        }
        if (this.statsSub) this.statsSub.unsubscribe();
    }
}

export class RadioButtonComp {
	IncEnc = true;
}


