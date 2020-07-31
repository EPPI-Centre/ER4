import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { searchService } from '../services/search.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router, NavigationEnd } from '@angular/router';
import { ClassifierContactModel,  MagSimulation, TopicLink, MagBrowseHistoryItem } from '../services/MAGClasses.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { interval, Subscription } from 'rxjs';
import { NotificationService } from '@progress/kendo-angular-notification';
import { MAGSimulationService } from '../services/MAGSimulation.service';


@Component({
    selector: 'AdvancedMAGFeatures',
    templateUrl: './AdvancedMAGFeatures.component.html',
    providers: []
})

export class AdvancedMAGFeaturesComponent implements OnInit, OnDestroy {

    //history: NavigationEnd[] = [];
    constructor(private ConfirmationDialogService: ConfirmationDialogService,
        private _magSimulationService: MAGSimulationService,
        public _magAdvancedService: MAGAdvancedService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _eventEmitter: EventEmitterService,
        private _mAGBrowserHistoryService: MAGBrowserHistoryService,
        private _location: Location,
        private _notificationService: NotificationService,
        private router: Router,
        public _magAdminService: MAGAdvancedService

    ) {

        //this.history = this._routingStateService.getHistory();
    }
    private subsc: Subscription = new Subscription();
    
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
            let magBrowseItem: MagBrowseHistoryItem = new MagBrowseHistoryItem("Advanced", "Advanced", 0,
                "", "", 0, "", "", 0, "", "", 0);
            this._mAGBrowserHistoryService.IncrementHistoryCount();
            this._mAGBrowserHistoryService.AddToBrowseHistory(magBrowseItem);
        }
    }
    ngOnDestroy() {

        this.subsc.unsubscribe();

    }
    @ViewChild('WithOrWithoutCodeSelector3') WithOrWithoutCodeSelector3!: codesetSelectorComponent;

    public CurrentDropdownSelectedCode3: singleNode | null = null;
    public CurrentDropdownSelectedCode2: singleNode | null = null;
    public ItemsWithCode: boolean = false;
    public ShowPanel: boolean = false;
    public dropdownBasic2: boolean = false;
    public dropdownBasic3: boolean = false;
    public isCollapsed2: boolean = false;
    public isCollapsed3: boolean = false;
    public ListSubType: string = '';
    public splitDataOn: string = 'Year';
    public SearchMethod: string = 'Recommendations';
    public SearchMethods: string[] = ['Bi-Citation',
        'Recommendations',
        'Bi-Citation AND Recommendations',
        'Extended Network'];
    public StudyTypeClassifier: string = 'None';
    public StudyTypeClassifiers: string[] = [
        'None',
        'RCT',
        'Cochrane RCT',
        'Economic evaluation',
        'Systematic review'
    ];
    public SearchTextTopics: TopicLink[] = [];
    public SearchTextTopicsResults: TopicLink[] = [];
    public UserDefinedClassifier: string = '';
    public magMatchedAll: number = 0;
    public magPaperId: number = 0;
    public currentClassifierContactModel: ClassifierContactModel = new ClassifierContactModel();
    public description: string = '';
    public kendoDateValue: Date = new Date();
    public kendoEndDateValue: Date = new Date();
    public magDate: string = 'true';
    public magMode: string = '';
    public filterOn: string = 'false';
    public ScoreThreshold: number = 0;
    public FoSThreshold: number = 0;
    public stepScore: number = 0.01;
    public stepFoS: number = 0.05;

    public get MagSimulationList(): MagSimulation[] {
        return this._magSimulationService.MagSimulationList;
    }

    private ShowMAGSimulationMessage(notifyMsg: string) {

        this._notificationService.show({
            content: notifyMsg,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
        });
    }
    public Back() {
        this._location.back();
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
    public GetMagReviewMagInfoCommand() {

        this._magAdvancedService.FetchMagReviewMagInfo();
    }
    public CanAddSimulation(): boolean {
        return this._magAdvancedService._RunAlgorithmFirst == true;

    }    
    private CheckContReviewPipelineState(): boolean {

        this._magAdvancedService.CheckContReviewPipelineState().then(

                (result: boolean) => { return result; }
            );
        return false;
    }
    private AddActualSimulation(): void {

        let newMagSimulation: MagSimulation = new MagSimulation();
        if (this.splitDataOn == 'Year' || this.splitDataOn == 'CreatedDate') {

            newMagSimulation.year = this.kendoDateValue.getFullYear();
            newMagSimulation.yearEnd = this.kendoEndDateValue.getFullYear();
            newMagSimulation.createdDate = this.kendoDateValue;
            newMagSimulation.createdEndDate = this.kendoEndDateValue;
            console.log(JSON.stringify(newMagSimulation));

        } else if (this.splitDataOn == 'WithThisCode') {

            if (this.CurrentDropdownSelectedCode2 != null) {
                let att = this.CurrentDropdownSelectedCode2 as SetAttribute;
                newMagSimulation.withThisAttributeId = att.attribute_id;
                newMagSimulation.withThisAttribute = att.attribute_name;
            }
        }
        //console.log('here', this.filterOn);
        if (this.filterOn == 'true') {
            if (this.CurrentDropdownSelectedCode2 != null) {
                //console.log('here2');
                let att = this.CurrentDropdownSelectedCode2 as SetAttribute;
                //console.log('here3', att);
                newMagSimulation.filteredByAttributeId = att.attribute_id;
                newMagSimulation.filteredByAttribute = att.attribute_name;
            }
        }
        newMagSimulation.searchMethod = this.SearchMethod;
        newMagSimulation.fosThreshold = this.FoSThreshold;
        newMagSimulation.scoreThreshold = this.ScoreThreshold;
        //newMagSimulation.networkStatistic = this.NetworkStat;
        if (this.StudyTypeClassifier != null) {
            newMagSimulation.studyTypeClassifier = this.StudyTypeClassifier;
        }
        if (this.UserDefinedClassifier != null) {

            newMagSimulation.userClassifierModel = this.currentClassifierContactModel.modelTitle;
            newMagSimulation.userClassifierModelId = this.currentClassifierContactModel.modelId;
            newMagSimulation.userClassifierReviewId = this.currentClassifierContactModel.reviewId;
        }
        newMagSimulation.status = "Pending";
        //console.log(newMagSimulation);

        let msg: string = 'Are you sure you want to create a new MAG Simulation?';
        this.ConfirmationDialogService.confirm('MAG Simulation', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magSimulationService.AddMagSimulation(newMagSimulation);
                }
            });
    }

    public AddSimulation(): void {

        let pipelineRunning: boolean = this.CheckContReviewPipelineState();
        if (pipelineRunning) {

            let msg: string = 'Sorry, another pipeline is currently running';
            this.ShowMAGSimulationMessage(msg);
            return;

        } else {

            this.AddActualSimulation();
        }
    }
    public GetClassifierContactModelList(): void {
        this._magAdvancedService.FetchClassifierContactModelList();
    }
    public OpenResultsInReview(listType: string, magSimId: number) {

        if (listType != null) {
            this._magAdvancedService.ListDescription = listType;
            this._magAdvancedService.CurrentMagSimId = magSimId;
            this.ListSubType = listType;
            this._eventEmitter.criteriaMAGChange.emit(listType);
        }
    }
    public DeleteSimulation(item: MagSimulation) {
        if (item != null) {
            this.ConfirmationDialogService.confirm("Deleting the selected MAG simulation",
                "Are you sure you want to delete MAG RUN:" + item.magSimulationId + "?", false, '')
                .then((confirm: any) => {
                    if (confirm) {
                        this._magSimulationService.DeleteSimulation(item);
                    }
                });
        }
    }
    CloseCodeDropDown3() {
        if (this.WithOrWithoutCodeSelector3) {
            this.CurrentDropdownSelectedCode3 = this.WithOrWithoutCodeSelector3.SelectedNodeData;
        }
        this.isCollapsed3 = false;
    }
    Clear() {

        this.CurrentDropdownSelectedCode2 = {} as SetAttribute;
        this.CurrentDropdownSelectedCode3 = {} as SetAttribute;
        this.description = '';
        this.ItemsWithCode = false;
        this.magDate = '';
        this.magMode = '';

    }
    public GetMagSimulationList() {
        this._magSimulationService.FetchMagSimulationList();
    }
}