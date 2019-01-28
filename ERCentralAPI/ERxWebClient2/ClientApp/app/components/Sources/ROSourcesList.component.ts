import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource } from '../services/sources.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { EventEmitterService } from '../services/EventEmitter.service';


@Component({
    selector: 'ROSourcesList',
    templateUrl: './ROSourcesList.component.html',
    providers: []
})

export class ROSourcesListComponent implements OnInit {
    //some inspiration taken from: https://malcoded.com/posts/angular-file-upload-component-with-express
    constructor( 
        private ItemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private SourcesService: SourcesService
    ) {    }
    ngOnInit() {
    }
    
    get ReviewSources(): ReadOnlySource[] {
        return this.SourcesService.ReviewSources;
    }
    DisplaySourcename(ROS: ReadOnlySource): string {
        if (ROS.source_Name == 'NN_SOURCELESS_NN' && ROS.source_ID == -1) return "Manually Created Items";
        else return ROS.source_Name;
    }
    ListSource(ros: ReadOnlySource) {
        let cr = new Criteria();
        //cr.onlyIncluded = false;// included ignore for sources
        //cr.showDeleted = true; // deleted ignore for sources
        cr.attributeSetIdList = "";
        cr.sourceId = ros.source_ID;
        cr.listType = "StandardItemList";
        let ListDescription: string = ((ros.source_Name == "NN_SOURCELESS_NN" && ros.source_ID == -1) ? "Manually Created (Sourceless) Items." : ros.source_Name);
        this.ItemListService.FetchWithCrit(cr, ListDescription);
        this._eventEmitter.PleaseSelectItemsListTab.emit();
    }
    ToggleDelSource(ros: ReadOnlySource) {
        //we should really show a "are you sure?" dialog...
        if ((ros.source_Name == "NN_SOURCELESS_NN" && ros.source_ID == -1) || ros.source_ID > 0) this.SourcesService.DeleteUndeleteSource(ros);
    }
}
