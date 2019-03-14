import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource } from '../services/sources.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';


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
        private SourcesService: SourcesService,
        private ConfirmationDialogService: ConfirmationDialogService,
        private ReviewerIdentityService: ReviewerIdentityService 
    ) {    }
    ngOnInit() {
    }
    
    get ReviewSources(): ReadOnlySource[] {
        return this.SourcesService.ReviewSources;
    }
    public get HasWriteRights(): boolean {
        return this.ReviewerIdentityService.HasWriteRights;
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
        if ((ros.source_Name == "NN_SOURCELESS_NN" && ros.source_ID == -1) || ros.source_ID > 0) {
            let msg: string;
            if (ros.isDeleted) {
                msg = "Are you sure you want to undelete the selected Source? <br /><br /> Items within the source <b>will be marked as 'Included'</b>, with the exception of duplicates."
            }
            else {
                msg = "Are you sure you want to delete the selected Source? <br /><br />Information about items state (<b>Included, Exluded or Deleted</b>) will be lost."
            }
            this.openConfirmationDialogDeleteUnDeleteSource(ros, msg);
        } 
    }
    public openConfirmationDialogDeleteUnDeleteSource(ros: ReadOnlySource, msg: string ) {
        
        this.ConfirmationDialogService.confirm('Please confirm', msg, false, '')
            .then(
                (confirmed: any) => {
                    console.log('User confirmed source (un/)delete:', confirmed);
                    if (confirmed) {
                        this.ActuallyDeleteUndeleteSource(ros);
                    } else {
                        //alert('did not confirm');
                    }
                }
            )
            .catch(() => {
                //console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)');
            });
    }
    
    ActuallyDeleteUndeleteSource(ros: ReadOnlySource) {
        this.SourcesService.DeleteUndeleteSource(ros);
    }
}
