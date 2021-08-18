import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService,  Criteria } from '../services/ItemList.service';
import { SourcesService, ReadOnlySource } from '../services/sources.service';
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
        this._searchService.selectedSourceDropDown = 'All items in source';
        this._searchService.cmdSearches._searchWhat = "AllItems";
    }

    nextSourceDropDownList(num: number, val: string) {
        this._searchService.selectedSourceDropDown = val;
                
        switch (num) {

            case 1: {
                this._searchService.cmdSearches._searchWhat = "AllItems";
                break;
            }
            case 2: {
                this._searchService.cmdSearches._searchWhat = "Included";
                break;
            }
            case 3: {
                this._searchService.cmdSearches._searchWhat = "Excluded";
                break;
            }
            case 4: {
                this._searchService.cmdSearches._searchWhat = "Deleted";
                break;
            }
            case 5: {
                this._searchService.cmdSearches._searchWhat = "Duplicates";
                break;
            }
            default:
                break;
        }
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
        //this._searchService.cmdSearches._sourceIds = this._sourcesService.ReviewSources.filter(x => x.isSelected == true).map<string>(y => y.source_ID.toString()).join(',');

    }
    get ReviewSources(): ReadOnlySource[] {
        let sources: ReadOnlySource[] = this._sourcesService.ReviewSources.filter((v, i) => i != this._sourcesService.ReviewSources.length - 1);
        //this._searchService.cmdSearches._sourceIds = this._sourcesService.ReviewSources.filter(x => x.isSelected == true).map<string>(y => y.source_ID.toString()).join(',');
        return sources;
    }
    public get HasWriteRights(): boolean {
        return this.ReviewerIdentityService.HasWriteRights;
    }
    DisplaySourcename(ROS: ReadOnlySource): string {
        if (ROS.source_Name == 'NN_SOURCELESS_NN' && ROS.source_ID == -1) return "Manually Created Items";
        else return ROS.source_Name;
    }
    
}
