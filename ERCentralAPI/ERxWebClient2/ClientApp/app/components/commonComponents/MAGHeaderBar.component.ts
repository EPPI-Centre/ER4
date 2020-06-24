import { Component,  OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MagItemPaperInsertCommand } from '../services/MAGClasses.service';
import { NotificationService } from '@progress/kendo-angular-notification';


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
        public _notificationService: NotificationService
    ) {

	}
    ngOnInit() {
	
    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public SelectedItems() : boolean {

        if (this._magBrowserService.selectedPapers != null && 
            this._magBrowserService.selectedPapers.length >0 ) {
            return false;
        } else {
            return true;
        }
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
        alert('not implemented');
    }
    public ClearSelected() {
        this._magBrowserService.ClearSelected();
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
    public MatchingMAGItems() {
        this.router.navigate(['MatchingMAGItems']);
    }
    public AutoUpdateHome() {
        this.router.navigate(['BasicMAGFeatures']);
    }
    public ShowHistory() {

        this.router.navigate(['MAGBrowserHistory']);
    }
    public Admin() {
        this.router.navigate(['MAGAdmin']);
    }
  
}




