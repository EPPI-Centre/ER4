import { Component, OnInit, ViewChild, EventEmitter, Output} from '@angular/core';
import { searchService } from '../services/search.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGAdvancedService, ClassifierContactModel,  MVCMagPaperListSelectionCriteria, MagSimulation } from '../services/magAdvanced.service';

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
        private router: Router

	) {

	}

    @Output() criteriaChange = new EventEmitter();
    @Output() MAGAllocationClicked = new EventEmitter();
    @ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;
    @ViewChild('WithOrWithoutCodeSelector2') WithOrWithoutCodeSelector2!: codesetSelectorComponent;
    public CurrentDropdownSelectedCode: singleNode | null = null;
    public CurrentDropdownSelectedCode2: singleNode | null = null;
    public ItemsWithCode: boolean = false;
    public MAGItems: any[] = [];
    public ShowPanel: boolean = false;
    public isCollapsed: boolean = true;
    public isCollapsed2: boolean = true;
    public ListSubType: string = '';
    public splitDataOn: string = 'Year';
    public SearchMethod: string = 'Recommendations';
    public SearchMethods: string[] = ['Citations',
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
    public SearchText: string = '';
    public UserDefinedClassifier: string = '';
    public magMatchedAll: number = 0;
    public magMatchedWithThisCode: number = 0;
    public magPaperId: number = 0;
    public currentClassifierContactModel: ClassifierContactModel = new ClassifierContactModel();
    public desc: string = '';
    public value: Date = new Date(2000, 2, 10);
    public searchAll: string = 'true';
    public magDate: string = 'true';
    public magSearchCheck: boolean = false;
    public magDateRadio: boolean = false;
    public magRCTRadio: string = 'NoFilter';
    public magMode: string = '';
    public filterOn: string = 'false';
    public ToggleMAGPanel(): void {
        this.ShowPanel = !this.ShowPanel;
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {
        return this._magAdvancedService.IsBusy;
    }
    public ShowGraphViewer: boolean = false;
    public ShowGraph() {

        this.ShowGraphViewer = true;
    }
    ngOnInit() {

        if (this._ReviewerIdentityServ.reviewerIdentity.userId == 0 ||
            this._ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else if (!this._ReviewerIdentityServ.HasWriteRights) {
            this.router.navigate(['Main']);
        }
        else {

            // maybe use getter setter pattern for this...!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            this.GetMagReviewMagInfoCommand();
            this.GetMagSimulationList();
            //probably do not need the below
            this.GetContactModelList();
        }
        
       
    }
    GetMagReviewMagInfoCommand() {

        this._magAdvancedService.FetchMagReviewMagInfo();
    }
    private _RunAlgorithmFirst: boolean = false;
    public CanAddSimulation(): boolean
    {
        return this._RunAlgorithmFirst == true;

    }
    public AddSimulation(): void {

        let newMagSimulation: MagSimulation = new MagSimulation();

        if (this.splitDataOn == 'Year') {

            // take the year from the date CONTROL !!!!!!!!!!!!!!!!!!!!!!
            newMagSimulation.year = 2016;

        } else if (this.splitDataOn == 'CreatedDate') {

            newMagSimulation.createdDate = new Date();

        } else if (this.splitDataOn == 'WithThisCode') {

            if (this.CurrentDropdownSelectedCode != null) {
                let att = this.CurrentDropdownSelectedCode as SetAttribute;
                newMagSimulation.withThisAttributeId = att.attribute_id;
            }
        }
        if (this.filterOn == 'true') {
            if (this.CurrentDropdownSelectedCode != null) {
                let att = this.CurrentDropdownSelectedCode as SetAttribute;
                newMagSimulation.filteredByAttributeId = att.attribute_id;
            }
        } else {
            //not yet implemented a filter on the UI to do                
            newMagSimulation.filteredByAttributeId = 0;
        }

        newMagSimulation.searchMethod = this.SearchMethod;
        newMagSimulation.networkStatistic = this.NetworkStat;
        if (this.StudyTypeClassifier != null) {
            newMagSimulation.studyTypeClassifier = this.StudyTypeClassifier;
        }
        if (this.UserDefinedClassifier != null) {
            // not yet implemented properly
            newMagSimulation.userClassifierModelId = 0; //this.UserDefinedClassifier;
        }
    
        newMagSimulation.status = "Pending";
        console.log(newMagSimulation);
        this._magAdvancedService.AddMagSimulation(newMagSimulation);

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

        this.router.navigate(['BasicMAGFeatures']);
    }

    public Back() {
       
    }
    public OpenMatchesInReview(listType: string) {

        this.ListSubType = listType;
        this.criteriaChange.emit();
        this.MAGAllocationClicked.emit();
    }

    public OpenResultsInReview(listType: string, magSimId: number) {

        this._magAdvancedService.ListDescription = listType;
        this._magAdvancedService.CurrentMagSimId = magSimId;
        this.ListSubType = listType;
        this.criteriaChange.emit();
        this.MAGAllocationClicked.emit();
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
    }
    public DeleteSimulation(item: MagSimulation) {

        this.ConfirmationDialogService.confirm("Deleting the selected MAG simulation",
            "Are you sure you want to delete MAG RUN:" + item.magSimulationId + "?", false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magAdvancedService.DeleteSimulation(item);
                }
            });
    }

    public GetMatchedMagIncludedList(): void {

        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "Included";
        criteria.pageSize = 20;

        this._magAdvancedService.FetchMagPaperList(criteria).then(
            (result: any) => {
                this.router.navigate(['MAGBrowser']);
            }
        );
       
    }
    public GetMatchedMagExcludedList() {

        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "Excluded";
        criteria.pageSize = 20;

        this._magAdvancedService.FetchMagPaperList(criteria).then(
            (result: any) => {
                this.router.navigate(['MAGBrowser']);
            }
        );

    }
    public GetMatchedMagAllList() {

        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "All";
        criteria.pageSize = 20;
        this._magAdvancedService.CurrentCriteria = criteria;
        this._magAdvancedService.FetchMagPaperList(criteria).then(
            (result: any) => {
                this.router.navigate(['MAGBrowser']);
            }
        );

    }
    public CanGetCodeMatches(): boolean {

        if (this.CurrentDropdownSelectedCode2 != null) {
            return true;
        } else {
            return false;
        }

    }
    public GetMatchedMagWithCodeList() {

        if (this.CurrentDropdownSelectedCode2 != null) {
            
            let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            criteria.listType = "ReviewMatchedPapersWithThisCode";
            var att = this.CurrentDropdownSelectedCode2 as SetAttribute;
            criteria.attributeIds = att.attribute_id.toString();
            criteria.pageSize = 20;
            console.log('got in here');

            this._magAdvancedService.FetchMagPaperList(criteria).then(
                (result: any) => {
                    this.router.navigate(['MAGBrowser']);
                }
            );
        }

    }
    public CanGetMagPaper(): boolean {

        if (this.magPaperId != null && this.magPaperId > 0) {
            return true;
        } else {
            return false;
        }

    }
    public GetMagPaper() {

        this._magAdvancedService.FetchMagPaperId(this.magPaperId);
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
    CloseCodeDropDown2() {
        if (this.WithOrWithoutCodeSelector2) {
            this.CurrentDropdownSelectedCode2 = this.WithOrWithoutCodeSelector2.SelectedNodeData;
        }
        this.isCollapsed2 = false;
    }
    Clear() {

        this.CurrentDropdownSelectedCode = {} as SetAttribute;
        this.CurrentDropdownSelectedCode2 = {} as SetAttribute;
        this.desc = '';
        this.ItemsWithCode = false;
        this.magDate = '';
        this.magMode = '';

    }
    public CanDeleteMAGRun() : boolean {
        // other params like existence need to be checked here!!!!!!!!!!!!!!!!!!!!!
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
    public GetMagSimulationList() {

        this._magAdvancedService.FetchMagSimulationList();
    }

}
	