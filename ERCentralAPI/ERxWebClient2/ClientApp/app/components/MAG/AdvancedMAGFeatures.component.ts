import { Component, OnInit, ViewChild} from '@angular/core';
import { searchService } from '../services/search.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ClassifierContactModel,  MVCMagPaperListSelectionCriteria, MagSimulation } from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MVCMagFieldOfStudyListSelectionCriteria } from '../services/MAGClasses.service';

@Component({
    selector: 'AdvancedMAGFeatures',
    templateUrl: './AdvancedMAGFeatures.component.html',
	providers: []
})

export class AdvancedMAGFeaturesComponent implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
        public _magAdvancedService: MAGAdvancedService,
        private _magBrowserService: MAGBrowserService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _eventEmitter: EventEmitterService,
        private router: Router

	) {

	}
    public Selected() {

    }
    public ClearSelected() {

    }
    public ImportSelected() {

    }
    public AutoUpdateHome() {

    }
    public ShowHistory() {

    }
    public Admin() {

    }
    @ViewChild('WithOrWithoutCodeSelector3') WithOrWithoutCodeSelector3!: codesetSelectorComponent;
    @ViewChild('WithOrWithoutCodeSelector2') WithOrWithoutCodeSelector2!: codesetSelectorComponent;
    public CurrentDropdownSelectedCode3: singleNode | null = null;
    public CurrentDropdownSelectedCode2: singleNode | null = null;
    public ItemsWithCode: boolean = false;
    public MAGItems: any[] = [];
    public ShowPanel: boolean = false;
    public dropdownBasic2: boolean = false;
    public dropdownBasic3: boolean = false;
    public isCollapsed2: boolean = false;
    public isCollapsed3: boolean = false;
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
    public kendoDateValue: Date = new Date();
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

        this.ShowGraphViewer = !this.ShowGraphViewer;
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
            this.GetMagReviewMagInfoCommand();
            this.GetMagSimulationList();
            this.GetClassifierContactModelList();
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

            console.log(this.kendoDateValue.getFullYear());
            newMagSimulation.year = this.kendoDateValue.getFullYear();

        } else if (this.splitDataOn == 'CreatedDate') {

            newMagSimulation.createdDate = this.kendoDateValue;

        } else if (this.splitDataOn == 'WithThisCode') {

            if (this.CurrentDropdownSelectedCode2 != null) {
                let att = this.CurrentDropdownSelectedCode2 as SetAttribute;
                newMagSimulation.withThisAttributeId = att.attribute_id;
                newMagSimulation.withThisAttribute = att.attribute_name;
            }
        }
        console.log('here', this.filterOn);
        if (this.filterOn == 'true') {
            if (this.CurrentDropdownSelectedCode2 != null) {
                console.log('here2');
                let att = this.CurrentDropdownSelectedCode2 as SetAttribute;
                console.log('here3', att);
                newMagSimulation.filteredByAttributeId = att.attribute_id;
                newMagSimulation.filteredByAttribute = att.attribute_name;
            }
        }
        newMagSimulation.searchMethod = this.SearchMethod;
        newMagSimulation.networkStatistic = this.NetworkStat;
        if (this.StudyTypeClassifier != null) {
            newMagSimulation.studyTypeClassifier = this.StudyTypeClassifier;
        }
        if (this.UserDefinedClassifier != null) {

            newMagSimulation.userClassifierModel = this.currentClassifierContactModel.modelTitle;
            newMagSimulation.userClassifierModelId = this.currentClassifierContactModel.modelId;
        }
        newMagSimulation.status = "Pending";
        console.log(newMagSimulation);

        let msg: string = 'Are you sure you want to create a new MAG Simulation?';
        this.ConfirmationDialogService.confirm('MAG Simulation', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magAdvancedService.AddMagSimulation(newMagSimulation);
                   
                }
            });
    }
    public GetClassifierContactModelList(): void {

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

    public Back() {
        this.router.navigate(['BasicMAGFeatures']);
    }

    public OpenMatchesInReview(listType: string) {

        this.ListSubType = listType;
        this._eventEmitter.criteriaMAGChange.emit(listType);
        this._eventEmitter.MAGAllocationClicked.emit();
    }

    public OpenResultsInReview(listType: string, magSimId: number) {

        this._magAdvancedService.ListDescription = listType;
        this._magAdvancedService.CurrentMagSimId = magSimId;
        this.ListSubType = listType;
        this._eventEmitter.criteriaMAGChange.emit(listType);
        this._eventEmitter.MAGAllocationClicked.emit();
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

        this._magBrowserService.FetchWithCrit(criteria, "ReviewMatchedPapers").then(
        //this._magAdvancedService.FetchMagPaperList(criteria).then(
            () => {

                let criteria2: MVCMagFieldOfStudyListSelectionCriteria = new MVCMagFieldOfStudyListSelectionCriteria();
                criteria2.fieldOfStudyId = 0;
                criteria2.listType = 'PaperFieldOfStudyList';
                criteria2.paperIdList = this._magBrowserService.ListCriteria.paperIds;
                criteria2.searchText = ''; //TODO this will be populated by the user..
                this._magBrowserService.FetchMagFieldOfStudyList(criteria2).then(

                    () => { this.router.navigate(['MAGBrowser']); }
                );
            }
        );
       
    }
    public GetMatchedMagExcludedList() {

        let criteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        criteria.listType = "ReviewMatchedPapers";
        criteria.included = "Excluded";
        criteria.pageSize = 20;

        this._magAdvancedService.FetchMagPaperList(criteria).then(
            () => {
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
            () => {
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
                () => {
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

        this._magAdvancedService.FetchMagPaperId(this.magPaperId).then(

            () => { this.router.navigate(['MAGBrowser']); }
             
        );
    }
	CanOnlySelectRoots() {
		return true;
	}
	CloseCodeDropDown3() {
		if (this.WithOrWithoutCodeSelector3) {
			this.CurrentDropdownSelectedCode3 = this.WithOrWithoutCodeSelector3.SelectedNodeData;
		}
		this.isCollapsed3 = false;
    }
    CloseCodeDropDown2() {
        if (this.WithOrWithoutCodeSelector2) {
            this.CurrentDropdownSelectedCode2 = this.WithOrWithoutCodeSelector2.SelectedNodeData;
        }
        this.isCollapsed2 = false;
    }
    Clear() {

        this.CurrentDropdownSelectedCode2 = {} as SetAttribute;
        this.CurrentDropdownSelectedCode3 = {} as SetAttribute;
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
	