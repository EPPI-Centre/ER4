import { Component,  OnInit, ViewChild, EventEmitter, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MagItemPaperInsertCommand, MagBrowseHistoryItem } from '../services/MAGClasses.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';

@Component({
    selector: 'MAGHeaderBar',
    templateUrl: './MAGHeaderBar.component.html',
    providers: []
})
export class MAGHeaderBarComp implements OnInit {

    constructor(private router: Router,
        private _location: Location,
        private _magBrowserService: MAGBrowserService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        public _notificationService: NotificationService,
        public _eventEmitterService: EventEmitterService,
        public _confirmationDialogService: ConfirmationDialogService,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService
    ) {

    }

    
    ngOnInit() {
	
    }
    @Input() Context: string | undefined;
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get isSiteAdmin(): boolean {
        return this._ReviewerIdentityServ.reviewerIdentity.isSiteAdmin;
    }
    public ShowSelectedPapers: string = "Selected (" + this._magBrowserService.SelectedPaperIds.length.toString() + ")";
    public  get SelectedItems() : boolean {

        if (this._magBrowserService.selectedPapers != null && 
            this._magBrowserService.selectedPapers.length >0 ) {
            return true;
        } else {
            return false;
        }
    }
    public DisableButton(destination: string) {
        if (this.Context == undefined || !this.HasWriteRights) return false;
        else if (this.Context == destination) return true;
        else return false;
    }
    public Forward() {
        
        this._location.forward();
    }
    public Back() {
        
        this._location.back();
    }
    public AdvancedFeatures() {

        this.router.navigate(['AdvancedMAGFeatures']);
    }
    public Selected() {
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("Browse topic: SelectedPapers "
            , "SelectedPapers", 0, "", "", 0, "", "",
            0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this._eventEmitterService.selectedButtonPressed.emit();
    }
    public ClearSelected() {
        let msg: string = 'Are you sure you want to clear the ' + this._magBrowserService.selectedPapers.length + '  selected MAG papers into your review?';
        this._confirmationDialogService.confirm('MAG Selected Papers', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magBrowserService.ClearSelected();
                }
            });
        
    }
    showMAGRunMessage(notifyMsg: string) {

        this._notificationService.show({
            content: notifyMsg,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
        });
    }
    public ImportSelected() {

        let msg: string = 'Are you sure you want to import the ' + this._magBrowserService.selectedPapers.length + '  selected MAG papers into your review?';
        this._confirmationDialogService.confirm('MAG Import', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this.ConfirmedImport();
                }
            });
    }
    public ConfirmedImport() {

        let notificationMsg: string = '';
        this._magBrowserService.ImportMagRelatedSelectedPapers(this._magBrowserService.SelectedPaperIds).then(

            (result: MagItemPaperInsertCommand | void) => {
                if (result != null && result.nImported != null) {
                    if (result.nImported == this._magBrowserService.SelectedPaperIds.length) {

                        notificationMsg += "Imported " + result.nImported + " out of " +
                            this._magBrowserService.SelectedPaperIds.length + " items";

                    } else if (result.nImported != 0) {

                        notificationMsg += "Some of these items were already in your review.\n\nImported " +
                            result.nImported + " out of " + this._magBrowserService.SelectedPaperIds.length +
                            " new items";
                    }
                    else {
                        notificationMsg += "All of these records were already in your review.";
                    }
                    this.showMAGRunMessage(notificationMsg);
                }
            });
       
    }
    public MagSearch() {

        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("MagSearch", "Search", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['MagSearch']);

    }
    public MagAdmin() {
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("MAGAdmin", "Admin", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['MAGAdmin']);
    }
    public MatchingMAGItems() {
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("Matching page", "matching", 0, "", "", 0, "", "", 0, "", "", 0)
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['MatchingMAGItems']);
    }
    public AutoUpdateHome() {
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("Manage review updates / find related papers", "RelatedPapers", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['BasicMAGFeatures']);
    }
    public ShowHistory() {
        let item: MagBrowseHistoryItem = new MagBrowseHistoryItem("View browse history", "History", 0, "", "", 0, "", "", 0, "", "", 0);
        this._mAGBrowserHistoryService.IncrementHistoryCount();
        this._mAGBrowserHistoryService.AddToBrowseHistory(item);
        this.router.navigate(['MAGBrowserHistory']);
    }
    public Admin() {
        this.router.navigate(['MAGAdmin']);
    }
  
}




