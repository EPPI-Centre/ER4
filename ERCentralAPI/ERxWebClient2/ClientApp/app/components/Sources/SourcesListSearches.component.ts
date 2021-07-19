import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource } from '../services/sources.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { searchService } from '../services/search.service';


@Component({
    selector: 'SourcesListSearches',
    templateUrl: './SourcesListSearches.html',
    providers: []
})

export class SourcesListSearchesComponent implements OnInit {
    //some inspiration taken from: https://malcoded.com/posts/angular-file-upload-component-with-express
    constructor( 
        private ItemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private _sourcesService: SourcesService,
        public _searchService: searchService,
        private ConfirmationDialogService: ConfirmationDialogService,
        private ReviewerIdentityService: ReviewerIdentityService 
    ) {    }
    ngOnInit() {
    }
    nextSourceDropDownList(num: number, val: string) {

        this._searchService.cmdSearches._sourceIds = this._sourcesService.ReviewSources.map<string>(y => y.source_ID.toString()).join(',');
        this._searchService.cmdSearches._deleted = 'false'
        this._searchService.cmdSearches._included = 'false'
        this._searchService.cmdSearches._duplicates = 'false'
        switch (num) {

            case 1: {
                this._searchService.cmdSearches._title = 'All items in source';
                break;
            }
            case 2: {
                this._searchService.cmdSearches._title = 'Only included';
                this._searchService.cmdSearches._included = 'true';
                break;
            }
            case 3: {
                this._searchService.cmdSearches._title = 'Only excluded';
                this._searchService.cmdSearches._included = 'false';
                break;
            }
            case 4: {
                this._searchService.cmdSearches._title = 'Only deleted';
                this._searchService.cmdSearches._deleted = 'true'
                break;
            }
            case 5: {
                this._searchService.cmdSearches._title = 'Only duplicates';
                this._searchService.cmdSearches._duplicates = 'true'
                break;
            }
            default:
                break;
        }
        console.log('cmd: ', this._searchService.cmdSearches);
        this._searchService.CreateSearch(this._searchService.cmdSearches, 'SearchSources');
    }
    private checkAllValue: boolean = false;
    changeCheck(): void {

        this.checkAllValue = !this.checkAllValue
        if (this.checkAllValue) {
            for (let item = 0; item < this.ReviewSources.length; item++) {
                this.ReviewSources[item].isSelected = true;
            }
        } else {
            for (let item = 0; item < this.ReviewSources.length; item++) {
                this.ReviewSources[item].isSelected = false;
            }
        }
    }
    get ReviewSources(): ReadOnlySource[] {

        let sources: ReadOnlySource[] = this._sourcesService.ReviewSources.filter((v, i) => i != this._sourcesService.ReviewSources.length - 1);
        return sources;
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
        this._sourcesService.DeleteUndeleteSource(ros);
    }
}
