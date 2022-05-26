import { Component,  OnInit, ViewChild, EventEmitter, Input, Output } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MagItemPaperInsertCommand, MagBrowseHistoryItem, MVCMagPaperListSelectionCriteria, MagRelatedPapersRun, MagSearch, MagPaper, MagFieldOfStudy, MagList } from '../services/MAGClasses.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { Helpers } from '../helpers/HelperMethods';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGTopicsService } from '../services/MAGTopics.service';

@Component({
    selector: 'MAGHeaderBar2',
    templateUrl: './MAGHeaderBar2.component.html',
    providers: []
})
export class MAGHeaderBar2Comp implements OnInit {

    constructor(private router: Router,
        private _location: Location,
        private _magBrowserService: MAGBrowserService,
        private _magAdvancedService: MAGAdvancedService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        public _notificationService: NotificationService,
        public _eventEmitterService: EventEmitterService,
        public _confirmationDialogService: ConfirmationDialogService,
        public _mAGBrowserHistoryService: MAGBrowserHistoryService,
        private _magTopicsService: MAGTopicsService
    ) {

    }

    
    ngOnInit() {
	
    }
    @Input() MustMatchItems: boolean = true;
    public Context: string = "RelatedPapers";
    @Output() PleaseGoTo = new EventEmitter<string>();
    @Output() PleaseGoBackHome = new EventEmitter<string>();
    @Output() IHaveImportedSomething = new EventEmitter<void>();
    //@Output() BackHome = new EventEmitter<string>(); 
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get isSiteAdmin(): boolean {
        return this._ReviewerIdentityServ.reviewerIdentity.isSiteAdmin;
    }
    BackHome() {
        //this.router.navigate(['Main']);
        this.PleaseGoBackHome.emit();
    }
    //public ShowSelectedPapers: string = "Selected (" + this._magBrowserService.SelectedPaperIds.length.toString() + ")";
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
        else if (this.MustMatchItems && destination != "matching" && destination != "MagSearch") return true;
        else return false;
    }
    public get CanGoForward(): boolean {
        if (this._mAGBrowserHistoryService.currentBrowsePosition < this._mAGBrowserHistoryService._MAGBrowserHistoryList.length - 1) return true;
        else return false;
    }
    public get CanGoBackwards(): boolean {
        if (this._mAGBrowserHistoryService.currentBrowsePosition > 0) return true;
        else return false;
    }
    public async Forward() {
        let res = await this._mAGBrowserHistoryService.NavigateToThisPoint(this._mAGBrowserHistoryService.currentBrowsePosition + 1);
        if (res != "") this.PleaseGoTo.emit(res);
        //this._location.forward();
    }
    public async Back() {
        let res = await this._mAGBrowserHistoryService.NavigateToThisPoint(this._mAGBrowserHistoryService.currentBrowsePosition - 1);
        if (res != "") this.PleaseGoTo.emit(res);
        //this._location.back();
    }
    
    public Selected() {
        if (this.Context == "MAGBrowser") {
            this._eventEmitterService.selectedButtonPressed.emit();
        } else {
            this.Context = "SelectedPapers";
            this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Browse topic: SelectedPapers "
                , "SelectedPapers", 0, "", "", 0, "", "", 0, "", "", 0));
            this._eventEmitterService.selectedButtonPressed.emit();
            //this.router.navigate(['MAGBrowser']).then(
            //    async (res) => {
            //        if (res) {
            //            await Helpers.Sleep(50);
            //            this._eventEmitterService.selectedButtonPressed.emit();
            //        }
            //        //this._eventEmitterService.tool = true;
            //    }
            //);
        }
    }
    public ClearSelected() {
        let msg: string = 'Are you sure you want to clear your list of ' + this._magBrowserService.selectedPapers.length + ' selected papers?';
        this._confirmationDialogService.confirm('Selected Papers', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this._magBrowserService.ClearSelected();
                }
            });
        
    }
    public ImportSelected() {

        let msg: string = 'Are you sure you want to import the ' + this._magBrowserService.selectedPapers.length + ' selected papers into your review?';
        this._confirmationDialogService.confirm('Import papers', msg, false, '')
            .then((confirm: any) => {
                if (confirm) {
                    this.ConfirmedImport();
                }
            });
    }
    public ConfirmedImport() {

        let notificationMsg: string = '';
        this._magBrowserService.ImportMagRelatedSelectedPapers(this._magBrowserService.selectedPapers).then(

            (result: MagItemPaperInsertCommand | void) => {
                if (result != null && result.nImported != null) {
                    if (result.nImported == this._magBrowserService.selectedPapers.length) {

                        notificationMsg += "Imported " + result.nImported + " out of " +
                            this._magBrowserService.selectedPapers.length + " items";
                        this.IHaveImportedSomething.emit();
                        
                    } else if (result.nImported != 0) {

                        notificationMsg += "Some of these items were already in your review.\n\nImported " +
                            result.nImported + " out of " + this._magBrowserService.selectedPapers.length +
                            " new items";
                        this.IHaveImportedSomething.emit();
                    }
                    else {
                        notificationMsg += "All of these records were already in your review.";
                    }
                    this._confirmationDialogService.showMAGRunMessage(notificationMsg);
                    this._magBrowserService.selectedPapers = [];
                    //this.RefreshLists(this._magBrowserService.SelectedPaperIds);
                }
            });
       
    }

    

    
    public MagSearch() {

        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Search and browse", "Search", 0, "", "", 0, "", "", 0, "", "", 0));
        //this.router.navigate(['MagSearch']);
        this.Context = "MagSearch";
    }
    public MagAdmin() {
        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("MAGAdmin", "Admin", 0, "", "", 0, "", "", 0, "", "", 0));
        //this.router.navigate(['MAGAdmin']);
        this.Context = "Admin";
    }
    public MatchingMAGItems() {
        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Match records", "matching", 0, "", "", 0, "", "", 0, "", "", 0))
        //this.router.navigate(['MatchingMAGItems']);
        this.Context = "matching";
    }
    public AutoUpdateHome() {
        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Bring review up-to-date", "RelatedPapers", 0, "", "", 0, "", "", 0, "", "", 0));
        //this.router.navigate(['BasicMAGFeatures']);
        this.Context = "RelatedPapers";
    }
    public KeepUpdated() {
        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Keep review up-to-date", "KeepUpdated", 0, "", "", 0, "", "", 0, "", "", 0));
        //this.router.navigate(['MAGKeepUpToDate']);
        this.Context = "KeepUpdated";
    }
    public ShowHistory() {
        this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("View browse history", "History", 0, "", "", 0, "", "", 0, "", "", 0));
        //this.router.navigate(['MAGBrowserHistory']);
        this.Context = "History";
    }
    public AdvancedFeatures() {
        this.Context = "Advanced";//simulations
        //this.router.navigate(['AdvancedMAGFeatures']);
    }
  
}




