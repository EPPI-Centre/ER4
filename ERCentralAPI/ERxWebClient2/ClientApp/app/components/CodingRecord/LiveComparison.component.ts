import { Component, OnInit, OnDestroy, Input, Inject} from '@angular/core';
import { Router } from '@angular/router';
import { ItemListService,  Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute, ItemAttributeFullTextDetails } from '../services/ItemCoding.service';
import { ReviewSetsService, ReviewSet, SetAttribute} from '../services/ReviewSets.service';
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
        
    }
    public AttsToReportOn: SetAttribute[] = [];
    public get CodesByReviewers(): LiveComparisonWrapper[] | null {
        let RelevantItmSets = this.ItemCodingService.ItemCodingList.filter(found => this.ReviewSetsService.selectedNode && found.setId == this.ReviewSetsService.selectedNode.set_id);
        let Result: LiveComparisonWrapper[] = [];
        if (RelevantItmSets && RelevantItmSets.length > 0) {
            this.AttsToReportOn = [];
            if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "ReviewSet") {
                this.AttsToReportOn = (this.ReviewSetsService.selectedNode as ReviewSet).attributes;
            }
            else if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "SetAttribute") {
                this.AttsToReportOn = (this.ReviewSetsService.selectedNode as SetAttribute).attributes;
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
                if (Currentcontact.ROIAs.length > 0) Result.push(Currentcontact);
            }
        }
        if (Result.length == 0) return null;
        else return Result;
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
	}
}
export class LiveComparisonWrapper {
    contactId: number = 0;
    contactName: string = "";
    codingComplete: boolean = false;
    ROIAs : ReadOnlyItemAttribute[] = []
}










