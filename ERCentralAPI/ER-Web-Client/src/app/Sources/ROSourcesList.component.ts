import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource } from '../services/sources.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Helpers } from '../helpers/HelperMethods';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';


@Component({
    selector: 'ROSourcesList',
    templateUrl: './ROSourcesList.component.html',
    providers: [],
    styles: [
        `@keyframes oscillate {
          0%   {transform:rotate(35deg);}
          50% {transform:rotate(-35deg);}
          100% {transform:rotate(35deg);}
        }`
    ]
})

export class ROSourcesListComponent implements OnInit {
    //some inspiration taken from: https://malcoded.com/posts/angular-file-upload-component-with-express
    constructor( 
        private ItemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private SourcesService: SourcesService,
        private ConfirmationDialogService: ConfirmationDialogService,
        private ReviewerIdentityService: ReviewerIdentityService,
        @Inject('BASE_URL') private _baseUrl: string,
    ) {    }
    ngOnInit() {
    }
    public get GettingSourceReport(): boolean {
        return this.SourcesService.SourceReportIsRunning;
    }
    public get ReportProgress(): string {
        return this.SourcesService.ProgressOfSourcesReport;
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
                msg = "Are you sure you want to undelete the<br> <b>\"" + ros.source_Name + "\"</b> source?<br/>Items within the source <b> will be marked as 'Included' </b>, with the exception of duplicates."
            }
            else {
                msg = "Are you sure you want to delete the<br> <b>\"" + ros.source_Name + "\"</b> source?<br/>Information about items state (<b>Included, Exluded or Deleted</b>) will be lost."
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

    public async CreateSourceReport() {
        if (this.ReviewSources != null) {

            let ReportParameter: string = "allSources";
            let report: string = await this.SourcesService.GetSourceReport(ReportParameter);
            if (report != "") {//report comes back empty if there was an error - error messages will be shown by the service
                const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(report, this._baseUrl, "Source Table"));
                saveAs(dataURI, "Source table.html");
                //Helpers.OpenInNewWindow(report, this._baseUrl);
            }
        }      
    }
    CancelSourcesReport() {
        this.SourcesService.StopSourcesReport();
    }
}




