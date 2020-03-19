import { Component, OnInit, ViewChild, EventEmitter, Output} from '@angular/core';
import { searchService } from '../services/search.service';
import { MAGService, MagRelatedPapersRun } from '../services/mag.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGAdvancedService, ClassifierContactModel, MagCurrentInfo } from '../services/magAdvanced.service';
import { Criteria, ItemListService } from '../services/ItemList.service';
import { Subscription } from 'rxjs';
import { EventEmitterService } from '../services/EventEmitter.service';

@Component({
    selector: 'AdvancedMAGFeatures',
    templateUrl: './AdvancedMAGFeatures.component.html',
	providers: []
})

export class AdvancedMAGFeaturesComponent implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
		private _magAdvancedService: MAGAdvancedService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _itemListService: ItemListService,
        private _eventEmitterService: EventEmitterService,
        private router: Router

	) {

	}

//    public sub: Subscription = new Subscription();
    @Output() criteriaChange = new EventEmitter();
    @Output() MAGAllocationClicked = new EventEmitter();

    ngOnInit() {

        this.GetContactModelList();
        this.GetMatchedMagIncludedList();
        this.GetMatchedMagExcludedList();
        this.Clear();
    }
    private _RunAlgorithmFirst: boolean = false;
    public CanAddSimulation(): boolean
    {
        return this._RunAlgorithmFirst == true;

    }
    public AddSimulation(): void {

        alert('add simulation');

    }
    public GetContactModelList(): void {

       this._magAdvancedService.FetchClassifierContactModelList();
    }
    public RunMatchingAlgo() {

       let msg: string = 'Are you sure you want to match all the items in your review\n to Microsoft Academic records?';
        this.ConfirmationDialogService.confirm('MAG RUN ALERT', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magAdvancedService.RunMatchingAlgorithm();
                    this._RunAlgorithmFirst = true;
                }
            });
    }

    public AdvancedFeatures() {

        this.router.navigate(['AdvancedMAGFeatures']);

    }

    public AutoUpdateHome() {

        this.router.navigate(['MAGFeatures']);
    }

    public Back() {
       
    }
    public OpenMatchesInReview(listType: string) {

        this.ListSubType = listType;
        this.criteriaChange.emit();
        this.MAGAllocationClicked.emit();
    }
	@ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
	public CurrentDropdownSelectedCode: singleNode | null = null;
	public ItemsWithCode: boolean = false;
	public MAGItems: any[] = [];
	public ShowPanel: boolean = false;
    public isCollapsed: boolean = true;
    public ListSubType: string = '';
    public splitDataOn: string = 'Year';
    public SearchMethod: string = 'Recommendations';
    public SearchMethods: string[] = [  'Citations',
                                'Recommendations',
                                'Citations and recommendations',
        'Fields of study'];
    public NetworkStat: string = 'None';
    public NetworkStats: string[] = [
        'degree',
        'closeness',
        'eigenscore',
        'pagerank',
        'hubscore',
        'authscore',
        'alpha'
    ];
    public StudyTypeClassifier: string = 'None';
    public StudyTypeClassifiers: string[] = [
        'None',
        'RCT',
        'Cochrane RCT',
        'Economic evaluation',
        'Systematic review'
    ];
    public UserDefinedClassifier: string = '';
    public magMatchedAll: number = 0;
    public magMatchedWithThisCode: number = 0;
    public magPaperId: number = 0;
    public currentClassifierContactModel: ClassifierContactModel = new ClassifierContactModel();

    public GetMatchedMagIncludedList(): void {


        this._magAdvancedService.FetchMagPaperList(,"MagMatchesMatched");
        //"Showing: included items that are matched to at least one Microsoft Academic record";
        //let SelectionCritieraItemList: Criteria = new Criteria();
        //SelectionCritieraItemList.listType = "MagMatchesMatched";
        //SelectionCritieraItemList.onlyIncluded = true;
        //SelectionCritieraItemList.showDeleted = false;
        //SelectionCritieraItemList.attributeSetIdList = "";
        //SelectionCritieraItemList.pageNumber = 0;

        //this._itemListService.FetchWithCrit(SelectionCritieraItemList, "MagMatchesMatched");
        //return this._itemListService.ItemList.items.length;
             
    }
    public GetMatchedMagExcludedList() {

        this._magAdvancedService.FetchMAGMatchesWithCritEx("MagMatchesMatched");
        //let SelectionCritieraItemList: Criteria = new Criteria();
        //SelectionCritieraItemList.listType = "MagMatchesMatched";
        //SelectionCritieraItemList.onlyIncluded = false;
        //SelectionCritieraItemList.showDeleted = false;
        //SelectionCritieraItemList.attributeSetIdList = "";
        //SelectionCritieraItemList.pageNumber = 0;

        //this._itemListService.FetchWithCrit(SelectionCritieraItemList, "MagMatchesMatched");
        //this._itemListService.ListChanged.emit();

    }
    public GetMatchedMagAllList() {

        let SelectionCritieraItemList: Criteria = new Criteria();
        SelectionCritieraItemList.listType = "MagMatchesMatched";
        SelectionCritieraItemList.onlyIncluded = true;
        SelectionCritieraItemList.showDeleted = false;
        SelectionCritieraItemList.attributeSetIdList = "";
        SelectionCritieraItemList.pageNumber = 0;

        this._itemListService.FetchWithCrit(SelectionCritieraItemList, "MagMatchesMatched");

    }
    public MAGBrowser(listType: string) {

        if (listType == 'MatchedIncluded') {
            this.GetMatchedMagIncludedList();
        } else if (listType == 'MatchedExcluded') {
            this.GetMatchedMagExcludedList();
        } else if (listType == 'MatchedAll') {
            this.GetMatchedMagAllList();
        } else if (listType == 'MatchedWithThisCode') {
            this.GetMatchedMagWithCodeList();
        }
        // silly way for now...
        this.router.navigate(['MAGBrowser']);
 
    }
    public GetMatchedMagWithCodeList() {

        let SelectionCritieraItemList: Criteria = new Criteria();
        SelectionCritieraItemList.listType = "MagMatchesMatched";
        SelectionCritieraItemList.onlyIncluded = true;
        SelectionCritieraItemList.showDeleted = false;
        SelectionCritieraItemList.attributeSetIdList = "";
        SelectionCritieraItemList.pageNumber = 0;

        this._itemListService.FetchWithCrit(SelectionCritieraItemList, "MagMatchesMatched");

    }
    public CanGetMagPaper(): boolean {

        if (this.magPaperId != null && this.magPaperId > 0) {
            return true;
        } else {
            return false;
        }

    }
    public GetMagPaper() {

        this._magAdvancedService.FetchMagPaper(this.magPaperId);
        this.router.navigate(['MAGBrowser']);
    }
	CanOnlySelectRoots() {
		return true;
	}
	CloseCodeDropDown() {
		if (this.WithOrWithoutCodeSelector) {
			this.CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData;
		}
		this.isCollapsed = false;
    }
	public desc: string = '';
	public value: Date = new Date(2000, 2, 10);
	public searchAll: string = 'true';
	public magDate: string = 'true';
	public magSearchCheck: boolean = false;
	public magDateRadio: boolean = false;
    public magRCTRadio: string = 'NoFilter';
	public magMode: string = '';
	public ToggleMAGPanel(): void {
		this.ShowPanel = !this.ShowPanel;
	}
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }

	public IsServiceBusy(): boolean {

		return false;
	}
	public Selected(): void {

	}
    Clear() {

        this.CurrentDropdownSelectedCode = {} as SetAttribute;
        this.desc = '';
        this.ItemsWithCode = false;
        this.magDate = '';
        this.magMode = '';

    }
    public CanDeleteMAGRun() : boolean {

        return this.HasWriteRights;
    }

    public CanAddNewMAGSearch(): boolean {

        if (this.desc != '' && this.desc != null && this.HasWriteRights
            ) {
            return true;
        } else {
            return false;
        }
    }

	public ClickSearchMode(searchModeChoice: string) {

		switch (searchModeChoice) {

            case '1':
                this.magMode = 'Recommended by';
                break;
            case '2':
                this.magMode = 'That recommend';
                break;
            case '3':
                this.magMode = 'Recommendations';
                break;
            case '4':
                this.magMode = 'Bibliography';
                break;
            case '5':
                this.magMode = 'Cited by';
                break;
            case '6':
                this.magMode = 'Bi-Citation';
                break;
            case '7':
                this.magMode = 'Bi-Citation AND Recommendations';
                break;

            default:
                break;
		}
	}
 //   private _magCurrentInfo: MagCurrentInfo = new MagCurrentInfo;

 //   public get magCurrentInfo() {
        
 //       return this._magAdvancedService.MagCurrentInfo;
 //   }

 //   public set magCurrentInfo(value: MagCurrentInfo) {

 //       this._magCurrentInfo = value;
        
	//}

    public GetMagSimulationList() {

        this._magAdvancedService.FetchMagSimulationList();

    }
}
	