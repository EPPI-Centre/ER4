import { Component,  OnInit, ViewChild, EventEmitter, Input, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
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
import { MAGHeaderBar2Comp } from '../commonComponents/MAGHeaderBar2.component';
import { Subscription } from 'rxjs';

@Component({
    selector: 'MAG',
    templateUrl: './MAG.component.html',
    providers: []
})
export class MAGComp implements OnInit, OnDestroy {

    constructor(private router: Router,
        private route: ActivatedRoute,
        private MAGBrowserService: MAGBrowserService,
        private MAGAdvancedService: MAGAdvancedService,
        private ReviewerIdentityService: ReviewerIdentityService,
        private _notificationService: NotificationService,
        private _eventEmitterService: EventEmitterService,
        private MAGBrowserHistoryService: MAGBrowserHistoryService,
        private _confirmationDialogService: ConfirmationDialogService,
        private _mAGBrowserHistoryService: MAGBrowserHistoryService,
        private _magTopicsService: MAGTopicsService
    ) {

    }

    ngOnInit() {
        this.subItemIDinPath = this.route.params.subscribe(params => {
            console.log("subItemIDinPath sub triggrered");
            let idTxt = params['paperId'];
            if (idTxt != undefined) {
                //we are coming here, trying to fetch a specific PaperId
                let id = Helpers.SafeParseInt(idTxt);
                if (id !== null) {
                    this.MAGBrowserService.GetCompleteMagPaperById(id).then((res) => {
                        if (res == true) {
                            const p = this.MAGBrowserService.currentMagPaper;
                            this._mAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Browse paper: " + p.paperId.toString(), "PaperDetail",
                                p.paperId, p.fullRecord,
                                p.abstract, p.linkedITEM_ID, p.allLinks, p.findOnWeb, 0, "", "", 0));
                            this.MAGBrowserService.ParentTopic = "GoDirectlyToPaperDetails"; //used when we change path!
                            this.router.navigate(['MAG']);//we go back to the normal path, so to make things look "normal"
                            //the "else if" below is triggered when we re-reach this path.
                        }
                    });
                }
                
            } else if (this.MAGBrowserService.ParentTopic == "GoDirectlyToPaperDetails") {
                this.MAGBrowserService.ParentTopic = "";
                this.ChangeContext("PaperDetail");
            }
            else {
                this.MAGBrowserHistoryService.AddHistory(
                    new MagBrowseHistoryItem("Manage review updates / find related papers", "RelatedPapers", 0, "", "", 0, "", "", 0, "", "", 0));
            }
            if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();//no need to keep listening!
        });
    
    }
    ngOnDestroy() {
        if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();
    }
    @ViewChild('NavBar2') NavBar2!: MAGHeaderBar2Comp;

    private subItemIDinPath: Subscription | null = null;
    public get Context(): string {
        if (this.NavBar2) return this.NavBar2.Context;
        else return "MAG: unspecified page";
    }
    public ChangeContext(val: string) {
        console.log("Main MAG: context is changing (from, to)", this.Context, val);
        if (this.NavBar2) this.NavBar2.Context = val;
    }
    Back() {
        this.router.navigate(['Main']);
    }
  
}




