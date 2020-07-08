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
    paperIds: string = '';
    pagecount: number = 0;
    pageindex: number = 0;
    totalItemCount: number = 0;
    papers: MagPaper[] = [];

}
export class MagItemPaperInsertCommand {

    paperIds: string = '';
    nImported: number = 0;
    sourceOfIds: string = '';
    magRelatedRunId: number = 0;

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
    paperId: number = 0;
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
    linkedITEM_ID: number = 0;
    isSelected: boolean = false;
    canBeSelected: boolean = false;
    abstract: string = '';
    autoMatchScore: number = 0;
    manualTrueMatch: boolean = false;
    manualFalseMatch: boolean = false;
    findOnWeb: string = '';
}

export class MagCurrentInfo {

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

