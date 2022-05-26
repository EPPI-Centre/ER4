import { Component, OnInit, OnDestroy, Input, Inject} from '@angular/core';
import { Router } from '@angular/router';
import { ItemListService,  Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute, ItemAttributeFullTextDetails } from '../services/ItemCoding.service';
import { ReviewSetsService, ReviewSet, SetAttribute, singleNode} from '../services/ReviewSets.service';
import { encodeBase64, saveAs } from '@progress/kendo-file-saver';
import { Subscription } from 'rxjs';
import { ComparisonsService } from '../services/comparisons.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { error } from '@angular/compiler/src/util';
import { Helpers } from '../helpers/HelperMethods';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { CurrencyIndex } from '@angular/common/src/i18n/locale_data';

@Component({
   
    selector: 'LiveComparisonComp',
    templateUrl: './LiveComparison.component.html'  

})

export class LiveComparisonComp implements OnInit, OnDestroy {

    constructor(private router: Router, 
        @Inject('BASE_URL') private _baseUrl: string,
        public ItemListService: ItemListService,
        private ComparisonsService: ComparisonsService,
        private ItemCodingService: ItemCodingService,
        private ReviewSetsService: ReviewSetsService,
        private NotificationService: NotificationService
    ) { }
    ngOnInit() {
        this.ItemCodingServiceDataChanged = this.ItemCodingService.DataChanged.subscribe(
            () => { let pointless = this.CodesByReviewers; }
        );
    }
    @Input() item: Item | undefined;
    private ItemCodingServiceDataChanged: Subscription | null = null;


    private liveComparisonContent: LiveComparisonContent = new LiveComparisonContent();
    public get AttsToReportOn(): SetAttribute[] {
        return this.liveComparisonContent.AttsToReportOn;
    }
    //public AttsToReportOn: SetAttribute[] = [];
    public get CodesByReviewers(): LiveComparisonWrapper[] {
        //console.log('CodesByReviewers');
        if (this.ReviewSetsService.selectedNode == null || this.ReviewSetsService.selectedNode.set_id < 1) {
            //nothing to compare with...
            console.log('CodesByReviewers: remove stale results');
            if (this.liveComparisonContent.AttsToReportOn.length > 0) this.liveComparisonContent.AttsToReportOn = [];
            if (this.liveComparisonContent.NodeToReportOn) this.liveComparisonContent.NodeToReportOn = null;
            if (this.liveComparisonContent.results.length > 0) this.liveComparisonContent.results = [];
            //if (this.liveComparisonContent.itemId != 0) this.liveComparisonContent.itemId = 0;
            return this.liveComparisonContent.results;
        }
        else if (this.ReviewSetsService.selectedNode == this.liveComparisonContent.NodeToReportOn
            && this.liveComparisonContent.item == this.item){
            //no change: return what we have already.
            //console.log('CodesByReviewers NO Change');
            return this.liveComparisonContent.results;
        }
        //we need to do work...
        console.log('CodesByReviewers: doing work...');
        this.liveComparisonContent.NodeToReportOn = this.ReviewSetsService.selectedNode;
        this.liveComparisonContent.results = [];
        this.liveComparisonContent.AttsToReportOn = [];
        if (this.ItemCodingService.IsBusy) {
            console.log("CodesByReviewers: still getting codings...")
            return this.liveComparisonContent.results;
        }
        this.liveComparisonContent.item = this.item;
        if (this.liveComparisonContent.item == undefined) {
            console.log('CodesByReviewers: no item!');
            return this.liveComparisonContent.results;
        }
        let RelevantItmSets = this.ItemCodingService.ItemCodingList.filter(found => this.ReviewSetsService.selectedNode && found.setId == this.ReviewSetsService.selectedNode.set_id);
        //let Result: LiveComparisonWrapper[] = [];
        if (RelevantItmSets && RelevantItmSets.length > 0) {
            this.liveComparisonContent.AttsToReportOn = [];
            if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "ReviewSet") {
                this.liveComparisonContent.AttsToReportOn = (this.ReviewSetsService.selectedNode as ReviewSet).attributes;
            }
            else if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "SetAttribute") {
                this.liveComparisonContent.AttsToReportOn = (this.ReviewSetsService.selectedNode as SetAttribute).attributes;
            }
            for (let ItmS of RelevantItmSets) {
                let Currentcontact: LiveComparisonWrapper = new LiveComparisonWrapper();
                Currentcontact.contactId = ItmS.contactId;
                Currentcontact.contactName = ItmS.contactName;
                Currentcontact.codingComplete = ItmS.isCompleted;
                for (let AttToReportOn of this.AttsToReportOn) {
                    let res = ItmS.itemAttributesList.filter(found => found.attributeId == AttToReportOn.attribute_id);
                    
                    Currentcontact.ROIAs = Currentcontact.ROIAs.concat(res);
                }
                if (Currentcontact.ROIAs.length > 0) this.liveComparisonContent.results.push(Currentcontact);
            }
        }
        //if (this.liveComparisonContent.results.length == 0) return null;
        return this.liveComparisonContent.results;
    }
    public AttNameFromId(AttId: number) {
        let res = AttId.toString();
        let RealRes = this.AttsToReportOn.find(found => found.attribute_id == AttId);
        if (RealRes) return RealRes.attribute_name;
        else return "Unknown Attribute (ID:" + res + ")";
    }
    ToggleHideShow() {
        this.ItemCodingService.ToggleLiveComparison.emit();
    }
    ngOnDestroy() {
        //console.log('killing live comparisons comp');
        if (this.ItemCodingServiceDataChanged) this.ItemCodingServiceDataChanged.unsubscribe();
    }
}
export class LiveComparisonWrapper {
    contactId: number = 0;
    contactName: string = "";
    codingComplete: boolean = false;
    ROIAs : ReadOnlyItemAttribute[] = []
}
export class LiveComparisonContent {
    AttsToReportOn: SetAttribute[] = [];
    NodeToReportOn: singleNode | null = null;
    results: LiveComparisonWrapper[] = [];
    item: Item | undefined;
}










