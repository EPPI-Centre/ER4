import { Component,  OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';


@Component({
    selector: 'MAGHeaderBar',
    templateUrl: './MAGHeaderBar.component.html',
    providers: []
})
export class MAGHeaderBarComp implements OnInit {

    constructor(private router: Router,
        private _location: Location,
        private _magBrowserService: MAGBrowserService,
        private _ReviewerIdentityServ: ReviewerIdentityService
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
    public ImportSelected() {
        alert('not implemented');
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




