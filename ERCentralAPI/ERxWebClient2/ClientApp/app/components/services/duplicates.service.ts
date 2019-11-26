import { Inject, Injectable, EventEmitter, Output, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item } from './ItemList.service';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from './revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { NumericDictionary } from 'lodash';

@Injectable({
    providedIn: 'root',
})

export class DuplicatesService extends BusyAwareService implements OnInit  {

    constructor(
		private _http: HttpClient,
        private ReviewerIdentityServ: ReviewerIdentityService,
        private modalService: ModalService,
        private router: Router,
        private NotificationService: NotificationService,
        @Inject('BASE_URL') private _baseUrl: string,

    ) {
        super();
    }
	ngOnInit() {
        //this.FetchGroups(false);
	}
    public DuplicateGroups: iReadOnlyDuplicatesGroup[] = [];
    public CurrentGroup: ItemDuplicateGroup | null = null;
    public FetchGroups(FetchNew: boolean) {
        this._BusyMethods.push("FetchGroups");
        //check if we're allowed to do this, otherwise send user back to main
        let lastcheckJSON = localStorage.getItem('DedupRev' + this.ReviewerIdentityServ.reviewerIdentity.reviewId);
        console.log("Fetch groups ongoing check (J): ", lastcheckJSON);
        if (lastcheckJSON) {
            let lastcheck = new Date(+lastcheckJSON);
            let now = new Date();
            console.log("Fetch groups ongoing check (d): ", lastcheck, now);
            let diff = now.getTime() - lastcheck.getTime();//in milliseconds...
            diff = diff / (1000 * 60);
            console.log("Fetch groups ongoing check: ", lastcheck, diff);
            if (diff < 5) {
                //we don't allow this user needs to wait and try again.
                let diff2 = Math.round(5 - diff);//how long does the user need to wait?
                let endMsg = "";
                if (diff2 == 0) {
                    endMsg = "less than one minute.";
                }
                else if (diff2 == 1) {
                    endMsg = "approximately one minute.";
                }
                else {
                    endMsg = "about " + diff2 + " minutes."
                }
                this.NotificationService.show({
                    content: "Sorry, the \"Get New Duplicates\" execution might still be running, you need to wait for " + endMsg
                        + " If execution lasts more than a few hours, please contact EPPISupport.",
                    hideAfter: 3000,
                    position: { horizontal: 'center', vertical: 'top' },
                    animation: { type: 'slide', duration: 400 },
                    type: { style: 'error', icon: true },
                    closable: true
                });
                    //.GenericErrorMessage("Sorry, execution might still be running, you need to wait for " + endMsg);
                this.RemoveBusy("FetchGroups");
                this.BackToMain();
                return;
            } else {
                localStorage.removeItem('DedupRev' + this.ReviewerIdentityServ.reviewerIdentity.reviewId);
            }
        }

        let body = JSON.stringify({ Value: FetchNew });

        this._http.post<iReadOnlyDuplicatesGroup[]>(this._baseUrl + 'api/Duplicates/FetchGroups',
            body).subscribe(result => {
                this.DuplicateGroups = result;
                //console.log(result);
                this.RemoveBusy("FetchGroups");
                if (this.DuplicateGroups.length > 0) {
                    if (this.CurrentGroup == null) {
                        const todo = this.DuplicateGroups.findIndex(found => found.isComplete == false);
                        if (todo > 0) this.FetchGroupDetails(this.DuplicateGroups[todo].groupId);
                        else this.FetchGroupDetails(this.DuplicateGroups[0].groupId);
                    }
                } else {
                    this.CurrentGroup == new ItemDuplicateGroup();
                }
            }, error => {
                console.log("FetchGroups error", error, FetchNew);
                this.RemoveBusy("FetchGroups");
                if (error.status == 500 && error.error == "DataPortal.Fetch failed (Execution still Running)") {
                    //execution running, prevent client from checking again for 5m;
                    let now = new Date();
                    localStorage.setItem('DedupRev' + this.ReviewerIdentityServ.reviewerIdentity.reviewId, JSON.stringify(now.getTime()));
                    this.NotificationService.show({
                        content: "Sorry, the \"Get New Duplicates\" execution might still be running, you need to wait for 5 minutes and try again. If execution lasts more than a few hours, please contact EPPISupport.",
                        hideAfter: 3000,
                        position: { horizontal: 'center', vertical: 'top' },
                        animation: { type: 'slide', duration: 400 },
                        type: { style: 'error', icon: true },
                        closable: true
                    });
                    this.BackToMain();
                } else {
                    this.modalService.GenericError(error);
                }
			}
			);
			//return currentItem.arms;
	}
    public FetchGroupDetails(groupId: number) {
        this._BusyMethods.push("FetchGroupDetails");
        let body = JSON.stringify({ Value: groupId });
        this._http.post<iItemDuplicateGroup>(this._baseUrl + 'api/Duplicates/FetchGroupDetails',
            body).subscribe(result => {
                let res = new ItemDuplicateGroup(result);
                this.CurrentGroup = res;
                //console.log(result);
                this.RemoveBusy("FetchGroupDetails");
            }, error => {
                this.modalService.GenericError(error);
                this.RemoveBusy("FetchGroupDetails");
            });
    }
    public MarkUnmarkMemberAsDuplicate(toDo: MarkUnmarkItemAsDuplicate) {
        this._BusyMethods.push("MarkUnmarkMemberAsDuplicate");
        this._http.post(this._baseUrl + 'api/Duplicates/MarkUnmarkMemberAsDuplicate',
            toDo).subscribe(result => {
                if (this.CurrentGroup) {
                    let found = this.CurrentGroup.members.find(ff => ff.itemDuplicateId == toDo.itemDuplicateId);
                    if (found) {
                        found.isDuplicate = toDo.isDuplicate;
                        found.isChecked = true;
                        if (this.CurrentGroup.members.length == this.CurrentGroup.members.filter(ff => ff.isChecked == true).length) {
                            //whole group is checked, find it in the main list and update...
                            let smallGr = this.DuplicateGroups.find(ff => ff.groupId == toDo.groupId);
                            if (smallGr) {
                                smallGr.isComplete = true;
                            }
                        }
                    }
                }
                
                this.RemoveBusy("MarkUnmarkMemberAsDuplicate");
            }, error => {
                console.log("MarkUnmarkMemberAsDuplicate error", error);
                this.modalService.GenericError(error);
                this.RemoveBusy("MarkUnmarkMemberAsDuplicate");
            });
    }
    public MarkMemberAsMaster(toDo: MarkUnmarkItemAsDuplicate) {
        this._BusyMethods.push("MarkMemberAsMaster");
        this._http.post(this._baseUrl + 'api/Duplicates/MarkMemberAsMaster',
            toDo).subscribe(result => {
                if (this.CurrentGroup) {
                //whole group is not checked anymore!
                    let smallGr = this.DuplicateGroups.find(ff => ff.groupId == toDo.groupId);
                    if (smallGr) {
                        smallGr.isComplete = false;
                    }
                    let gm = this.CurrentGroup.Master;
                    let member = this.CurrentGroup.members.find(ff => ff.itemDuplicateId == toDo.itemDuplicateId);
                    if (gm.itemDuplicateId !== 0 ) {
                        gm.isMaster = false;
                        gm.isChecked = false;
                        gm.isDuplicate = false;
                    }
                    if (member) {
                        member.isMaster = true;
                        member.isChecked = true;
                        member.isDuplicate = false;
                    }
                    if (gm.itemDuplicateId == 0 || !member) {
                        this.FetchGroupDetails(toDo.groupId);
                    }
                }
                this.RemoveBusy("MarkMemberAsMaster");
            }, error => {
                console.log("MarkMemberAsMaster error", error);
                this.modalService.GenericError(error);
                this.RemoveBusy("MarkMemberAsMaster");
            });
    }
    BackToMain() {
        this.router.navigate(['Main']);
    }

}
export interface iReadOnlyDuplicatesGroup {
    groupId: number;
    isComplete: boolean;
    masterItemId: number;
    //reviewId: number,
    shortTitle: string;
}

export interface iDuplicateGroupMember {
    itemDuplicateId: number;
    itemId: number;
    title: string;
    parentTitle: string;
    shortTitle: string;
    year: string;
    month: string;
    authors: string;
    parentAuthors: string;
    isDuplicate: boolean;
    isChecked: boolean;
    isMaster: boolean;
    isExported: boolean;
    isEditable: boolean;
    codedCount: number;
    docCount: number;
    groupID: number;
    source: string;
    typeName: string;
    pages: string;
    similarityScore: number;
}
export interface iItemDuplicateGroup {
    groupID: number;
    originalMasterID: number;
    addGroupID: number;
    addItems: number[],
    removeItemID: number
    members: iDuplicateGroupMember[];
    manualMembers: any;
}

export class DuplicateGroupMember {
    constructor(iDupMember?: iDuplicateGroupMember) {
        if (iDupMember) {
            this.itemDuplicateId = iDupMember.itemDuplicateId; 
            this.itemId = iDupMember.itemId;
            this.title = iDupMember.title;
            this.parentTitle = iDupMember.parentTitle;
            this.shortTitle = iDupMember.shortTitle;
            this.year = iDupMember.year;
            this.month = iDupMember.month;
            this.authors = iDupMember.authors;
            this.parentAuthors = iDupMember.parentAuthors;
            this.isDuplicate = iDupMember.isDuplicate;
            this.isChecked = iDupMember.isChecked;
            this.isMaster = iDupMember.isMaster;
            this.isExported = iDupMember.isExported;
            this.isEditable = iDupMember.isEditable;
            this.codedCount = iDupMember.codedCount;
            this.docCount = iDupMember.docCount;
            this.groupID = iDupMember.groupID;
            this.source = iDupMember.source;
            this.typeName = iDupMember.typeName;
            this.pages = iDupMember.pages;
            this.similarityScore = iDupMember.similarityScore;
        }
    }
    itemDuplicateId: number = 0;
    itemId: number = 0;
    title: string = "";
    parentTitle: string = "";
    shortTitle: string = "";
    year: string = "";
    month: string = "";
    authors: string = "";
    parentAuthors: string = "";
    isDuplicate: boolean = false;
    isChecked: boolean = false;
    isMaster: boolean = false;
    isExported: boolean = false;
    isEditable: boolean = false;
    codedCount: number = -1;
    docCount: number = -1;
    groupID: number = 0;
    source: string = "";
    typeName: string = "";
    pages: string = "";
    similarityScore: number = 0;
}
export class ItemDuplicateGroup {
    constructor(iItemDGroup?: iItemDuplicateGroup) {
        if (iItemDGroup) {
            this.groupID = iItemDGroup.groupID;
            this.originalMasterID = iItemDGroup.originalMasterID;
            this.addGroupID = 0;
            this.addItems = [];
            this.removeItemID = 0;
            this.members = [];
            this.manualMembers = iItemDGroup.manualMembers;
            for (let iMember of iItemDGroup.members) {
                let Member = new DuplicateGroupMember(iMember);
                this.members.push(Member);
            }
        }
    }
    groupID: number = 0;
    originalMasterID: number = 0;
    addGroupID: number = 0;
    addItems: number[] = [];
    removeItemID: number = 0;
    members: DuplicateGroupMember[] = [];
    manualMembers: any = null;
    public get Master(): DuplicateGroupMember {
        for (let mb of this.members) {
            if (mb.isMaster) return mb;
        }
        return new DuplicateGroupMember();
    }
    public get JustMembers(): DuplicateGroupMember[] {
        return this.members.filter(found => found.isMaster == false);
    }
    public get HasOriginalMaster(): boolean {
        if (this.originalMasterID != 0 && this.originalMasterID == this.Master.itemId) return true;
        else return false;
    }
    public SimilarityForMember(member: DuplicateGroupMember) : number {
        if (this.HasOriginalMaster && this.members.find(ff => ff.itemDuplicateId == member.itemDuplicateId)) return member.similarityScore;
        else return NaN;
    }
}
export class MarkUnmarkItemAsDuplicate {
    constructor(grId: number, memberId: number, isDup: boolean) {
        this.groupId = grId;
        this.itemDuplicateId = memberId;
        this.isDuplicate = isDup;
    }
    groupId: number;
    itemDuplicateId: number;
    isDuplicate: boolean;
}


