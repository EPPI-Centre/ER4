import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
    providedIn: 'root',
})

export class MAGClassesService  {

    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
    }
}

export class MagList {

    pagesize: number = 0;
    paperIds: string = '';//possibly remove this?
    pagecount: number = 0;
    pageindex: number = 0;
    totalItemCount: number = 0;
    papers: MagPaper[] = [];

}
export class MagItemPaperInsertCommand {
    //this is used in two scenarios: 1 (the native one) to import items in the review (multiple cases)
    //2 to get updated counts on "AutoUpdateRun" when changing thresholds (ahead of importing, you'd guess)
    paperIds: string = '';
    nImported: number = 0;
    sourceOfIds: string = '';
    magRelatedRunId: number = 0;
    magAutoUpdateRunId: number = 0;
    orderBy: string = '';
    autoUpdateScore: number = 0;
    studyTypeClassifierScore: number = 0;
    userClassifierScore: number = 0;
    topN: number = 0;
    filterJournal: string = '';
    filterDOI: string = '';
    filterURL: string = '';
}
export class MagRelatedPapersRun {

    magRelatedRunId: number = 0;
    userDescription: string = '';
    attributeId: number = 0;
    attributeName: string = '';
    allIncluded: boolean = false;
    dateRun: string = "";
    dateFrom: string = "";
    autoReRun: boolean = false;
    mode: string = '';
    filtered: string = '';
    status: string = '';
    userStatus: string = '';
    nPapers: number = 0;
    reviewIdId = 0;
}
export class MagRelatedPaperListSelectionCriteria {

    pageSize: number = 20;
    pageNumber: number = 0;
    listType: string = "MagRelatedPapersRunList";
    magRelatedRunId: number = 0;

}

export class MagReviewMagInfo {

    reviewId: number = 0;
    nInReviewIncluded: number = 0;
    nInReviewExcluded: number = 0;
    nMatchedAccuratelyIncluded: number = 0;
    nMatchedAccuratelyExcluded: number = 0;
    nRequiringManualCheckIncluded: number = 0;
    nRequiringManualCheckExcluded: number = 0;
    nNotMatchedIncluded: number = 0;
    nNotMatchedExcluded: number = 0;
    nPreviouslyMatched: number = 0;
}

export class MagPaperList {

    pageIndex: number = 0;
    totalItemCount: number = 0;
    pageSize: number = 0;
    isPageChanging: boolean = false;
    fieldOfStudyId: number = 0;
    paperId: number = 0;
    authorId: number = 0;
    magRelatedRunId: number = 0;
    paperIds: string = '';
    includedOrExcluded: string = '';
    attributeIds: string = '';
}



export class MVCMagFieldOfStudyListSelectionCriteria {

    fieldOfStudyId: number = 0;
    listType: string = '';
    paperIdList: string = '';
    SearchTextTopics: string = '';

}
export class MVCMagOrigPaperListSelectionCriteria {

    magPaperId: number = 0;
    iTEM_ID: number = 0;
    listType: string = '';
    fieldOfStudyId: number = 0;
    authorId: number = 0;
    magRelatedRunId: number = 0;
    paperIds: string = '';
    attributeIds: string = '';
    included: string = '';
    pageNumber: number = 0;
    pageSize: number = 0;
    numResults: number = 0;
    dateFrom: string = '';
    dateTo: string = '';
    magSearchText = '';


}
export class MVCMagPaperListSelectionCriteria {
    magPaperId: number = 0;
    iTEM_ID: number = 0;
    listType: string = '';
    fieldOfStudyId: number = 0;
    authorId: number = 0;
    magRelatedRunId: number = 0;
    paperIds: string = '';
    attributeIds: string = '';
    included: string = '';
    pageNumber: number = 0;
    pageSize: number = 0;
    numResults: number = 0;
    dateFrom: string = '';
    dateTo: string = '';
    magSearchText = '';
    magAutoUpdateRunId: number = 0;
    autoUpdateOrderBy: string = '';
    autoUpdateAutoUpdateScore: number = 0.20;//can't be less than 0.20!
    autoUpdateStudyTypeClassifierScore: number = 0;  
    autoUpdateUserClassifierScore: number = 0;
    autoUpdateUserTopN: number = 0;
}

export class MvcMagFieldOfStudyListSelectionCriteria {

    fieldOfStudyId: number = 0;
    listType: string = '';
    paperIdList: string = '';
    SearchTextTopics: string = '';

}

export class ClassifierContactModel {

    modelId: number = 0;
    modelTitle: string = '';
    contactId: number = 0;
    reviewId: number = 0;
    reviewName: string = '';
    contactName: string = '';
    attributeOn: string = '';
    attributeNotOn: string = '';
    accuracy: number = 0;
    auc: number = 0;
    precision: number = 0;
    recall: number = 0;

}

export class MagFieldOfStudy {

    fieldOfStudyId: number = 0;
    rank: number = 0;
    normalizedName: string = '';
    displayName: string = '';
    mainType: string = '';
    level: number = 0;
    paperCount: number = 0;
    citationCount: number = 0;
    createdDate: Date = new Date();
    num_times: number = 0;
    externalMagLink: string = '';

}

export class MagPaper {

    externalMagLink: string = '';
    fullRecord: string = '';
    shortRecord: string = '';
    paperId: number = -1;
    doi: string = '';
    docType: string = '';
    paperTitle: string = '';
    originalTitle: string = '';
    bookTitle: string = '';
    year: number = 0;
    smartDate: Date = new Date();
    journalId: number = 0;
    journal: string = '';
    conferenceSeriesId: number = 0;
    conferenceInstanceId: number = 0;
    volume: string = '';
    issue: string = '';
    firstPage: string = '';
    lastPage: string = '';
    referenceCount: number = 0;
    references: number = 0;
    citationCount: number = 0;
    estimatedCitationCount: number = 0;
    createdDate: number = 0;
    authors: string = '';
    urls: string = '';
    pdfLinks: string = '';
    allLinks: string = '';
    linkedITEM_ID: number = 0;
    isSelected: boolean = false;
    canBeSelected: string = 'false';
    abstract: string = '';
    autoMatchScore: number = 0;
    manualTrueMatch: boolean = false;
    manualFalseMatch: boolean = false;
    findOnWeb: string = '';
}

export class MagCurrentInfoShort {

    currentAvailability: string = '';
    lastUpdated: Date = new Date(2000, 2, 10);

}

export class MagSimulation {

    magSimulationId: number = 0;
    reviewId: number = 0;
    year: number = 0;
    yearEnd: number = 0;
    createdEndDate: Date = new Date();
    createdDate: Date = new Date();
    withThisAttributeId: number = 0;
    filteredByAttributeId: number = 0;
    searchMethod: string = '';
    fosThreshold: number = 0;
    scoreThreshold: number = 0;
    thresholds: string = '';
    networkStatistic: string = '';
    studyTypeClassifier: string = '';
    userClassifierModelId: number = 0;
    userClassifierReviewId: number = 0;
    status: string = '';
    withThisAttribute: string = '';
    filteredByAttribute: string = '';
    userClassifierModel: string = '';
    tp: number = 0;
    fp: number = 0;
    fn: number = 0;
    tn: number = 0;
}
export class KeyValue {
    constructor(k: string, v: string) {
        this.key = k;
        this.value = v;
    }
    key: string;
    value: string;
}
export class TopicLink {

    displayName: string = '';
    fontSize: number = 0;
    callToFOS: string = '';
    fieldOfStudyId: number = 0;
}
export class MagCheckContReviewRunningCommand {

    isRunningMessage: string = '';

}
export class MAGBlobCommand {

    releaseNotes: string = '';
    latestMagSasUri: string = '';
    latestMAGName: string = '';
    previousMAGName: string ='';

}

export class MAGLog {

    magLogId: number = 0;
    jobType: string = '';
    jobStatus: string = '';
    jobMessage: string = '';
    contactName: string = '';
    contactId: number = 0;
    timeSubmitted: Date = new Date();
    timeUpdated: Date = new Date();

}
export class MAGLogList {

    magLogList: MAGLog[] = [];
}
export class MAGReview {

    reviewId: number = 0;
    name: string = '';

}
export class MAGReviewList {

    magReviewList: MAGReview[] = [];
}
export class MagCurrentInfo {

    magCurrentInfoId: number = 0;
    magFolder: string = '';
    matchingAvailable: boolean = false;
    magOnline: boolean = false;
    whenLive: string = '';
    makesEndPoint: string = '';
    makesDeploymentStatus: string = '';
}

export class MagBrowseHistoryItem {
    constructor(title: string, browseType: string, paperId: number, paperFullRecord: string,
        paperAbstract: string, linkedITEM_ID: number, allLinks: string, findOnWeb: string, fieldOfStudyId: number,
        fieldOfStudy: string, attributeIds: string, magRelatedRunId: number) {
        this.title = title;
        this.browseType = browseType;
        this.paperId = paperId;
        this.paperFullRecord = paperFullRecord;
        this.paperAbstract = paperAbstract;
        this.linkedITEM_ID = linkedITEM_ID;
        this.allLinks = allLinks;
        this.findOnWeb = findOnWeb;
        this.fieldOfStudyId = fieldOfStudyId;
        this.fieldOfStudy = fieldOfStudy;
        this.attributeIds = attributeIds;
        this.magRelatedRunId = magRelatedRunId;
    }
    public static MakeFromAutoUpdateListCrit(crit :MVCMagPaperListSelectionCriteria): MagBrowseHistoryItem {
        let res = new MagBrowseHistoryItem("List: Auto Update Run results", "MagAutoUpdateRunPapersList", 0, "", "", 0, "", "", 0, "", "", 0);
        res.magAutoUpdateRunId = crit.magAutoUpdateRunId;
        res.autoUpdateOrderBy = crit.autoUpdateOrderBy;
        res.autoUpdateAutoUpdateScore = crit.autoUpdateAutoUpdateScore;
        res.autoUpdateStudyTypeClassifierScore =  crit.autoUpdateStudyTypeClassifierScore
        res.autoUpdateUserClassifierScore = crit.autoUpdateUserClassifierScore;
        res.autoUpdateUserTopN = crit.autoUpdateUserTopN;
        return res;
    }
    public get toAutoUpdateListCrit(): MVCMagPaperListSelectionCriteria | null {
        if (this.browseType != "MagAutoUpdateRunPapersList") return null;
        let res: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        res.listType = this.browseType;
        res.magAutoUpdateRunId = this.magAutoUpdateRunId;
        res.autoUpdateOrderBy = this.autoUpdateOrderBy;
        res.autoUpdateAutoUpdateScore = this.autoUpdateAutoUpdateScore;
        res.autoUpdateStudyTypeClassifierScore = this.autoUpdateStudyTypeClassifierScore
        res.autoUpdateUserClassifierScore = this.autoUpdateUserClassifierScore;
        res.autoUpdateUserTopN = this.autoUpdateUserTopN;
        return res;
    }
    title: string = '';
    browseType: string = '';
    paperId: number = 0;
    paperFullRecord: string = '';
    paperAbstract: string = '';
    fieldOfStudyId: number = 0;
    fieldOfStudy: string = '';
    attributeIds: string = '';
    magRelatedRunId: number = 0;
    linkedITEM_ID: number = 0;
    uRLs: string = '';
    allLinks: string = '';
    findOnWeb: string = '';
    contactId: number = 0;
    dateBrowsed: Date = new Date();
    magAutoUpdateRunId: number = 0;
    autoUpdateOrderBy: string = "";
    autoUpdateAutoUpdateScore: number = 0;
    autoUpdateStudyTypeClassifierScore: number = 0;
    autoUpdateUserClassifierScore: number = 0;
    autoUpdateUserTopN: number = 0;
}

export class topicInfo {

    fieldOfStudyId: number = 0;
    fieldOfStudy: string = '';

}

export class ContReviewPipeLineCommand {

    previousVersion : string = '';
    magFolder : string = '';
    editScoreThreshold: number = 0;
    editFoSThreshold: number = 0;
    specificFolder : string = '';
    magLogId: number = 0;
    editReviewSampleSize: number = 0;

}

export class MagSearch {

    magSearchId: number = 0;
    reviewId: number = 0;
    contactId: number = 0;
    searchText: string = '';
    searchNo: number = 0;
    hitsNo: number = 0;
    searchDate: Date = new Date();
    magFolder: string = '';
    magSearchText: string = '';
    contactName: string = '';
    add: boolean = false;
}

export interface MagAutoUpdateRun {
    magAutoUpdateRunId: number;
    reviewIdId: number;
    userDescription: string;
    attributeId: number;
    attributeName: string;
    allIncluded: boolean;
    studyTypeClassifier: string;
    userClassifierDescription: string;
    userClassifierModelId: number;
    userClassifierModelReviewId: number;
    dateRun: string;
    nPapers: number;
    magAutoUpdateId: number;
    magVersion: string;

}
export interface MagAutoUpdate {
    allIncluded: boolean;
    attributeId: number;
    attributeName: string;
    autoReRun: boolean;
    magAutoUpdateId: number;
    reviewIdId: number;
    userDescription: string;
}

export class MagAutoUpdateVisualiseSelectionCriteria {
    magAutoUpdateRunId: number = -1;
    field: string = "";
}
export interface MagAutoUpdateVisualise {
    count: number;
    range: string;
}
export class MagAddClassifierScoresCommand {
    public magAutoUpdateRunId: number = 0;
    public topN: number = 0;
    public studyTypeClassifier: string = "";
    public userClassifierModelId: number = 0;
    public userClassifierReviewId: number = 0;
}
