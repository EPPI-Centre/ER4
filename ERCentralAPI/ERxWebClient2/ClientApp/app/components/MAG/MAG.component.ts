import { Component,  OnInit, ViewChild, EventEmitter, Input, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import {  MagBrowseHistoryItem } from '../services/MAGClasses.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { Helpers } from '../helpers/HelperMethods';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGHeaderBar2Comp } from '../commonComponents/MAGHeaderBar2.component';
import { Subscription } from 'rxjs';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MAGSimulationService } from '../services/MAGSimulation.service';
import { magSearchService } from '../services/MAGSearch.service';
import { MAGAdminService } from '../services/MAGAdmin.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { ItemListService } from '../services/ItemList.service';
import { ClassifierService } from '../services/classifier.service';

@Component({
    selector: 'MAG',
    templateUrl: './MAG.component.html',
    providers: []
})
export class MAGComp implements OnInit, OnDestroy {

    constructor(private router: Router,
        private route: ActivatedRoute,
        private MAGBrowserService: MAGBrowserService,
        private MAGRelatedRunsService: MAGRelatedRunsService,
        private MAGAdvancedService: MAGAdvancedService,
        private MAGSimulationService: MAGSimulationService,
        private ReviewerIdentityService: ReviewerIdentityService,
        private MAGBrowserHistoryService: MAGBrowserHistoryService,
        private magSearchService: magSearchService,
        private MAGAdminService: MAGAdminService,
        private CodesetStatisticsService: CodesetStatisticsService,
        private ItemListService: ItemListService,
        private classifierService: ClassifierService
    ) {

    }

    ngOnInit() {
        this.subItemIDinPath = this.route.params.subscribe(params => {
            console.log("subItemIDinPath sub triggrered");
            let idTxt = params['paperId'];
            if (idTxt != undefined) {
                //we are coming here, trying to fetch a specific PaperId (usually from the ItemDetails page)
                let id = Helpers.SafeParseInt(idTxt);
                if (id !== null) {
                    this.MAGBrowserService.GetCompleteMagPaperById(id).then((res) => {
                        if (res == true) {
                            const p = this.MAGBrowserService.currentMagPaper;
                            this.MAGBrowserHistoryService.AddHistory(new MagBrowseHistoryItem("Browse paper: " + p.paperId.toString(), "PaperDetail",
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
                    new MagBrowseHistoryItem("Bring review up-to-date", "RelatedPapers", 0, "", "", 0, "", "", 0, "", "", 0));
            }
            if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();//no need to keep listening!

            this.LoadMAGwideData()
            
        });
    
    }
    private LoadMAGwideData() {
        //multiple API calls. We don't wait for one to end before doing the next, but we do wait 40-150ms before starting the next call.
        this.MAGAdvancedService.FetchMagReviewMagInfo();
        setTimeout(() => {
            this.MAGRelatedRunsService.FetchMagRelatedPapersRunList();
            setTimeout(() => {
                this.MAGRelatedRunsService.GetMagAutoUpdateList(true);
                setTimeout(() => {
                    this.MAGAdminService.FetchMagCurrentInfo();
                    setTimeout(() => {
                        this.magSearchService.FetchMAGSearchList();
                        setTimeout(() => {
                            this.classifierService.FetchClassifierContactModelList(this.ReviewerIdentityService.reviewerIdentity.userId);
                            setTimeout(() => {
                                this.MAGSimulationService.FetchMagSimulationList();
                            }, 150);
                        }, 150);
                    }, 150);
                }, 120);
            }, 100);
        }, 40);
    }
    ngOnDestroy() {
        if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();
        //console.log("clearing MAG services data...");
        this.MAGRelatedRunsService.Clear();
        this.MAGAdvancedService.Clear();
        this.MAGAdminService.Clear();
        this.magSearchService.Clear();
        this.MAGSimulationService.Clear();
    }
    @ViewChild('NavBar2') NavBar2!: MAGHeaderBar2Comp;
    public IHaveImportedSomething: boolean = false;
    private subItemIDinPath: Subscription | null = null;

    public HelpAndFeebackContext: string = "openalex\\bringuptodate";

    public get Context(): string {
        if (this.NavBar2) {
            switch (this.NavBar2.Context) {
                case "RelatedPapers":
                    this.HelpAndFeebackContext = "openalex\\bringuptodate";
                    break;
                case "KeepUpdated":
                    this.HelpAndFeebackContext = "openalex\\keepupdated";
                    break;
                case "Advanced":
                    this.HelpAndFeebackContext = "openalex\\advanced";
                    break;
                case "History":
                    this.HelpAndFeebackContext = "openalex\\history";
                    break;
                case "matching":
                    this.HelpAndFeebackContext = "openalex\\matching";
                    break;
                case "Admin":
                    this.HelpAndFeebackContext = "openalex\\admin";
                    break;
                case "MagSearch":
                    this.HelpAndFeebackContext = "openalex\\search";
                    break;
                default:
                    // RelatedPapers
                    this.HelpAndFeebackContext = "openalex\\bringuptodate";
            }

            return this.NavBar2.Context;
        }
        else return "MAG: unspecified page";
    }
    public ChangeContext(val: string) {
        //console.log("Main MAG: context is changing (from, to)", this.Context, val);
        if (this.NavBar2) {
            switch (this.NavBar2.Context) {
                case "RelatedPapers":
                    this.HelpAndFeebackContext = "openalex\\bringuptodate";
                    break;
                case "KeepUpdated":
                    this.HelpAndFeebackContext = "openalex\\keepupdated";
                    break;
                case "Advanced":
                    this.HelpAndFeebackContext = "openalex\\advanced";
                    break;
                case "History":
                    this.HelpAndFeebackContext = "openalex\\history";
                    break;
                case "matching":
                    this.HelpAndFeebackContext = "openalex\\matching";
                    break;
                case "Admin":
                    this.HelpAndFeebackContext = "openalex\\admin";
                    break;
                case "MagSearch":
                    this.HelpAndFeebackContext = "openalex\\search";
                    break;
                default:
                    // RelatedPapers
                    this.HelpAndFeebackContext = "openalex\\bringuptodate";
            }
            this.NavBar2.Context = val;
        }
    }
    public get MustMatchItems(): boolean {
        if (this.MAGAdvancedService.AdvancedReviewInfo.nMatchedAccuratelyIncluded + this.MAGAdvancedService.AdvancedReviewInfo.nMatchedAccuratelyExcluded > 0) return false;
        else if (this.MAGAdvancedService.AdvancedReviewInfo.reviewId > 0) {
            if (this.Context != 'matching' && this.Context != 'MagSearch' && this.Context != 'MagSearchPapersList' && this.Context != 'SelectedPapers'
                && this.Context != 'BrowseTopic' && this.Context != 'PaperDetail' && this.Context != 'BrowseTopic') {
                //we go to the matching page, unless we're in MagSearch or any one of the "browse" contexts.
                setTimeout(() => this.ChangeContext('matching'), 20);
                //the delay is to avoid the dreaded expressionChangedAfterItHasBeenCheckedError...
            }
            return true;
        }
        return false;
    }
    public get MagFolder(): string {
        return this.MAGAdminService.MagCurrentInfo.magFolder;
    }
    public get MatchedCount(): number {
        return this.MAGAdvancedService.AdvancedReviewInfo.nMatchedAccuratelyExcluded + this.MAGAdvancedService.AdvancedReviewInfo.nMatchedAccuratelyIncluded; 
    }
    BackHome() {
        if (this.IHaveImportedSomething) {
            this.ItemListService.Refresh();
            this.CodesetStatisticsService.GetReviewStatisticsCountsCommand(false);
        }
        this.router.navigate(['Main']);
    }
  
}




