import { Component,  OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MAGBrowserService } from '../services/MAGBrowser.service';


@Component({
    selector: 'MAGHeaderBar',
    templateUrl: './MAGHeaderBar.component.html',
    providers: []
    //styles: ["button.disabled {color:black; }"]
})
export class MAGHeaderBarComp implements OnInit {

    constructor(private router: Router,
        private _location: Location,
        private _magBrowserService: MAGBrowserService
    ) {

	}
    ngOnInit() {
	
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

//export const routerConfig: Routes = [
//    { path: 'BasicMAGFeatures', component: BasicMAGComp },
//    { path: 'AdvancedMAGFeatures', component: AdvancedMAGFeaturesComponent },
//    { path: 'MAGBrowser', component: MAGBrowser },
//    {

//        path: '',
//        redirectTo: '/home',
//        pathMatch: 'full'

//    },
//    {

//        path: '**',
//        redirectTo: '/home',
//        pathMatch: 'full'
//    }

//];




