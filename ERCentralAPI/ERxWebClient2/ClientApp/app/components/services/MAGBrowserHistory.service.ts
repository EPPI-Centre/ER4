import { Injectable, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs";
import { BusyAwareService } from "../helpers/BusyAwareService";
import { MagBrowseHistoryItem, MVCMagPaperListSelectionCriteria, MagPaper, MagSearch } from "./MAGClasses.service";
import { MAGBrowserService } from "./MAGBrowser.service";
import { MAGAdvancedService } from "./magAdvanced.service";
import { SetAttribute } from "./ReviewSets.service";
import { Helpers } from "../helpers/HelperMethods";
import { EventEmitterService } from './EventEmitter.service';

@Injectable({

    providedIn: 'root',

})

export class MAGBrowserHistoryService extends BusyAwareService implements OnDestroy {

    //public MAGSubscription: Subscription = new Subscription();
    public _MAGBrowserHistoryList: MagBrowseHistoryItem[] = [];
    constructor(private _magBrowserService: MAGBrowserService,
        private EventEmitterService: EventEmitterService,
        private _magAdvancedService: MAGAdvancedService
    ) {
        super();
        this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
        this.clearSub2 = this.EventEmitterService.OpeningNewReview.subscribe(() => { this.Clear(); });
    }

    ngOnDestroy() {
        console.log("Destroy MAGRelatedRunsService");
        if (this.clearSub != null) this.clearSub.unsubscribe();
        if (this.clearSub2 != null) this.clearSub2.unsubscribe();
    }
    private clearSub: Subscription | null = null;
    private clearSub2: Subscription | null = null;

    public currentBrowsePosition: number = 0;
    
    //public UnsubscribeMAGHistory() {

    //    this.MAGSubscription.unsubscribe();
    //}
    public IncrementHistoryCount() {
        if (this._MAGBrowserHistoryList != null && this._MAGBrowserHistoryList.length > 0) {
            this.currentBrowsePosition = this._MAGBrowserHistoryList.length - 1;
        } else if (this._MAGBrowserHistoryList.length == 0) this.currentBrowsePosition = 0;
    }
    public AddHistory(magBrowserHistoryItem: MagBrowseHistoryItem) {

        let item: MagBrowseHistoryItem = magBrowserHistoryItem;
        this.AddToBrowseHistory(item);
        this.IncrementHistoryCount();
    }

    private AddToBrowseHistory(item: MagBrowseHistoryItem ) {

        this._MAGBrowserHistoryList.push(item);
    }

    public FetchMAGBrowserHistory() {

        return this._MAGBrowserHistoryList;
    }



    public async NavigateToThisPoint(browsePosition: number): Promise<string> {
        if (browsePosition > -1) {
            //this.currentBrowsePosition = browsePosition;

            if (this._MAGBrowserHistoryList != null && browsePosition <= this._MAGBrowserHistoryList.length) {
                let mbh: MagBrowseHistoryItem = this._MAGBrowserHistoryList[browsePosition];
                console.log("trying to go to:", mbh.browseType, mbh);
                switch (mbh.browseType) {
                    case "History":
                        return this.ShowHistoryPage(browsePosition);
                        //break;
                    case "Advanced":
                        return this.ShowAdvancedPage(browsePosition);
                        //break;
                    case "Admin":
                        return this.ShowAdminPage(browsePosition);
                        //break;
                    case "RelatedPapers":
                        return this.ShowRelatedPapers(browsePosition);
                        //break;
                    case "matching":
                        return this.ShowMatching(browsePosition);
                        //break;
                    case "PaperDetail":
                        return await this.ShowPaperDetailsPage(mbh.paperId, mbh.paperFullRecord, mbh.paperAbstract, mbh.allLinks,
                            mbh.findOnWeb, mbh.linkedITEM_ID, browsePosition);
                        //break;
                    case "MatchesIncluded":
                        return this.ShowMAGMatchesPage("included", browsePosition);
                        //break;
                    case "MatchesExcluded":
                        return this.ShowMAGMatchesPage("excluded", browsePosition);
                        //break;
                    case "MatchesIncludedAndExcluded":
                        return this.ShowMAGMatchesPage("all", browsePosition);
                        //break;
                    case "ReviewMatchedPapersWithThisCode":
                        return await this.ShowAllWithThisCode(mbh.attributeIds, browsePosition);
                        //break;
                    case "MagRelatedPapersRunList":
                        return await this.ShowAutoIdentifiedMatches(mbh.magRelatedRunId, browsePosition);
                        //break;
                    case "BrowseTopic":
                        return this.ShowTopicPage(mbh.fieldOfStudyId, mbh.fieldOfStudy, browsePosition);
                        //break;
                    case "SelectedPapers":
                        return this.ShowSelectedPapersPage(browsePosition);
                        //break;
                    case "KeepUpdated":
                        return this.ShowKeepUpToDate(browsePosition);
                        //break;
                    case "MagAutoUpdateRunPapersList":
                        return await this.ShowAutoUpdateRunPapersList(browsePosition);
                        //break;
                    case "Search":
                        return this.ShowSearchPage(browsePosition);
                        //break;
                    case "MagSearchPapersList":
                        return this.ShowMagSearchPapersList(mbh.paperFullRecord, browsePosition);
                        //break;

                }
            }
        }
        return "";
    }
    public ShowHistoryPage(pos: number): string {
        this.currentBrowsePosition = pos;
        return "History";
        //this.PleaseGoTo.emit("History");
        //this.router.navigate(['MAGBrowserHistory']);
    }
    public ShowAdvancedPage(pos: number): string {
        this.currentBrowsePosition = pos;
        return "Advanced";
        //this.PleaseGoTo.emit("Advanced");
        //this.router.navigate(['AdvancedMAGFeatures']);
    }
    public ShowAdminPage(pos: number): string {
        this.currentBrowsePosition = pos;
        return "Admin";
        //this.PleaseGoTo.emit("Admin");
        //this.router.navigate(['MAGAdmin']);
    }
    public ShowMatching(pos: number): string {
        this.currentBrowsePosition = pos;
        return "matching";
        //this.PleaseGoTo.emit("matching");
        //this.router.navigate(['MatchingMAGItems']);
    }
    public ShowKeepUpToDate(pos: number): string {
        this.currentBrowsePosition = pos;
        return "KeepUpdated";
        //this.PleaseGoTo.emit("KeepUpdated");
        //this.router.navigate(['MAGKeepUpToDate']);
    }
    public async ShowPaperDetailsPage(paperId: number, paperFullRecord: string, paperAbstract: string, urls: string,
        findOnWeb: string, linkedITEM_ID: number, pos: number): Promise<string> {
        let res = await this._magBrowserService.GetCompleteMagPaperById(paperId);
        
        //.then(
        //    (result: boolean) => {
        //        if (result == true) {
        //            return "PaperDetail";
        //            //this.PleaseGoTo.emit("PaperDetail");
        //        }
        //        else return "";
        //    });
        if (res) {
            this.currentBrowsePosition = pos;
            return "PaperDetail";
            //this.PleaseGoTo.emit("PaperDetail");
        }
        else return "";
            
    }
    public ShowMAGMatchesPage(incOrExc: string, pos: number): string {
        if (incOrExc == 'included') {
            //this._eventEmitterService.getMatchedIncludedItemsEvent.emit();
        } else if (incOrExc == 'excluded') {
           // this._eventEmitterService.getMatchedExcludedItemsEvent.emit();
        } else if (incOrExc == 'all') {
            //this._eventEmitterService.getMatchedAllItemsEvent.emit();
        } else {
            //          there is an error
        }
        return "";
    }
    public async ShowAllWithThisCode(attributeId: string, pos: number): Promise<string> {

        let attIdn = Helpers.SafeParseInt(attributeId);
        if (attIdn != null && attIdn > 0) {
            let att: SetAttribute = new SetAttribute();
            att.attribute_id = attIdn;
            let res = await this._magBrowserService.GetMatchedMagWithCodeList(att);
            this.currentBrowsePosition = pos;
            return "ReviewMatchedPapersWithThisCode";
        }
        else return "";
    }
    public async ShowAutoIdentifiedMatches(magRelatedRunId: number, pos: number): Promise<string> {
        let res = this._magBrowserService.GetMagRelatedRunsListById(magRelatedRunId);
            //.then(
            //    () => {
            //        this.PleaseGoTo.emit("MagRelatedPapersRunList");
            //        //this.router.navigate(['MAGBrowser']);
            //    }
        //);
        this.currentBrowsePosition = pos;
        return "MagRelatedPapersRunList";
    }
    public ShowRelatedPapers(pos: number): string {
        //this._magBasicService.FetchMagRelatedPapersRunList();
        //this.router.navigate(['BasicMAGFeatures']);
        //this.PleaseGoTo.emit("RelatedPapers");
        this.currentBrowsePosition = pos;
        return "RelatedPapers";
    }
    public ShowTopicPage(fieldOfStudyId: number, fieldOfStudy: string, pos: number): string {

        this._magBrowserService.currentMagPaper = new MagPaper();
        //this._magBrowserService.WPChildTopics = [];
        //this._magBrowserService.WPParentTopics = [];
        this._magBrowserService.ParentTopic = '';
        //this.router.navigate(['MAGBrowser']);
        this.GetParentAndChildRelatedPapers(fieldOfStudy, fieldOfStudyId);
        this.currentBrowsePosition = pos;
        return "BrowseTopic";
    }
    public ShowSelectedPapersPage(pos: number): string {
        //this.PleaseGoTo.emit("SelectedPapers");
        this.currentBrowsePosition = pos;
        return "SelectedPapers";
        //this.router.navigate(['MAGBrowser']); 
        //this._magBrowserService.onTabSelect(2);
    }
    private async ShowAutoUpdateRunPapersList(pos: number): Promise<string> {
        let target = this._MAGBrowserHistoryList[pos];
        if (target != undefined && target != null) {
            let crit = target.toAutoUpdateListCrit;
            if (crit != null) {
                let res = await this._magBrowserService.GetMagOrigList(crit);
                if (res == true) {
                    this.currentBrowsePosition = pos;
                    return "MagAutoUpdateRunPapersList";
                    //this.PleaseGoTo.emit("MagAutoUpdateRunPapersList");
                }
                
                //.then(
                //    (res) => {
                //        if (typeof res !== "boolean") this.PleaseGoTo.emit("MagAutoUpdateRunPapersList");
                //        //this.router.navigate(['MAGBrowser']);
                //    }
                //);
            }
        }
        return "";
    }
    private ShowSearchPage(pos: number): string {
        //console.log("Going to Search Page...");
        this.currentBrowsePosition = pos;
        //this.PleaseGoTo.emit("MagSearch");
        return "MagSearch";
    }

    private ShowMagSearchPapersList(searchSt: string, pos: number): string {
        //console.log("Going to MagSearchPapersList Page...");
        let search = new MagSearch();
        search.magSearchText = searchSt;
        this._magBrowserService.GetMagItemsForSearch(search);
        this.currentBrowsePosition = pos;
        return "MagSearchPapersList";
    }

    public GetParentAndChildRelatedPapers(FieldOfStudy: string, FieldOfStudyId: number) {
        this._magBrowserService.ParentTopic = FieldOfStudy;
        this._magBrowserService.GetParentAndChildRelatedPapers(FieldOfStudy, FieldOfStudyId).then((r: boolean) => {
            //if (r == true) this.PleaseGoTo.emit("BrowseTopic");
        });
    }
    public Clear() {
        this.currentBrowsePosition = 0;
        this._MAGBrowserHistoryList = [];
    }
}