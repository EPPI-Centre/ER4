import { Inject, Injectable, EventEmitter, Output, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Item } from './ItemList.service';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from './revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { LocalSort } from '../helpers/HelperMethods';
import { EventEmitterService } from './EventEmitter.service';
import { Subscription } from 'rxjs';

@Injectable({
    providedIn: 'root',
})

export class DuplicatesService extends BusyAwareService implements OnDestroy {

    constructor(
        private _http: HttpClient,
        private ReviewerIdentityServ: ReviewerIdentityService,
        private modalService: ModalService,
        private router: Router,
        private NotificationService: NotificationService,
        private EventEmitterService: EventEmitterService,
        @Inject('BASE_URL') private _baseUrl: string,

    ) {
        super();
        //console.log("On create DuplicatesService");
        this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
    }
    ngOnDestroy() {
        console.log("Destroy DuplicatesService");
        if (this.clearSub != null) this.clearSub.unsubscribe();
    }
    private clearSub: Subscription | null = null;

    public currentCount = 0; public allDone: boolean = false; public ToDoCount = 0;
    public DuplicateGroups: iReadOnlyDuplicatesGroup[] = [];
    public CurrentGroup: ItemDuplicateGroup | null = null;
    public LocalSort: LocalSort = new LocalSort();
    private waitPeriod = 3;//in minutes... time that people have to wait before we can FetchGroups if last attempt told us "execution still running"

    public FetchGroups(FetchNew: boolean) {
        this._BusyMethods.push("FetchGroups");
        //check if we're allowed to do this, otherwise send user back to main
        let lastcheckJSON = localStorage.getItem('DedupRev' + this.ReviewerIdentityServ.reviewerIdentity.reviewId);
        //console.log("Fetch groups ongoing check (J): ", lastcheckJSON);
        if (lastcheckJSON) {
            let lastcheck = new Date(+lastcheckJSON);
            let now = new Date();
            console.log("Fetch groups ongoing check (d): ", lastcheck, now);
            let diff = now.getTime() - lastcheck.getTime();//in milliseconds...
            diff = diff / (1000 * 60);
            console.log("Fetch groups ongoing check: ", lastcheck, diff);
            if (diff < this.waitPeriod) {
                //we don't allow this user needs to wait and try again.
                let diff2 = Math.round(this.waitPeriod - diff);//how long does the user need to wait?
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
                    position: { horizontal: 'center', vertical: 'top' },
                    animation: { type: 'slide', duration: 400 },
                    type: { style: 'error', icon: true },
                    closable: true
                });
                //.GenericErrorMessage("Sorry, execution might still be running, you need to wait for " + endMsg);
                this.RemoveBusy("FetchGroups");
                this.BackToMain();
                return ;
            } else {
                localStorage.removeItem('DedupRev' + this.ReviewerIdentityServ.reviewerIdentity.reviewId);
            }
        }

        let body = JSON.stringify({ Value: FetchNew });

        return this._http.post<iReadOnlyDuplicatesGroup[]>(this._baseUrl + 'api/Duplicates/FetchGroups',
            body).toPromise().then(result => {
                this.DuplicateGroups = result;
                //console.log(result);
                this.DoSort();
                this.ActivateFirstNotCompletedGroup();
                this.RemoveBusy("FetchGroups");
            }, error => {
                console.log("FetchGroups error", error, FetchNew);
                this.RemoveBusy("FetchGroups");
                if (error.status == 500 && error.error == "DataPortal.Fetch failed (Execution still Running)") {
                    //execution running, prevent client from checking again for 3m;
                    this.SetWaitPeriod();
                    this.BackToMain();
                } else {
                    this.modalService.GenericError(error);
                    this.BackToMain();//just in case, this will force a refresh which is safer...
                }
            }
            );
        //return currentItem.arms;
    }

    public FetchRelatedGroups(groupId: number) {
        let crit: GroupListSelectionCriteriaMVC = new GroupListSelectionCriteriaMVC();
        crit.groupId = groupId;
        this.FetchGroupsWithCriteria(crit);
    }
    public FetchGroupsByItemIds(ItemIds: string) {
        let crit: GroupListSelectionCriteriaMVC = new GroupListSelectionCriteriaMVC();
        crit.itemIds = ItemIds;
        this.FetchGroupsWithCriteria(crit);
    }
    private FetchGroupsWithCriteria(crit: GroupListSelectionCriteriaMVC) {
        this._BusyMethods.push("FetchGroupsWithCriteria");
        this._http.post<iReadOnlyDuplicatesGroup[]>(this._baseUrl + 'api/Duplicates/FetchGroupsWithCriteria',
            crit).subscribe(result => {
                this.DuplicateGroups = result;
                //console.log(result);
                this.DoSort();
                this.ActivateFirstNotCompletedGroup();
                this.RemoveBusy("FetchGroupsWithCriteria");
                
            }, error => {
                console.log("FetchGroups error", error, crit);
                this.RemoveBusy("FetchGroupsWithCriteria");
                if (error.status == 500 && error.error == "DataPortal.Fetch failed (Execution still Running)") {
                    //execution running, prevent client from checking again for 3m;
                    this.SetWaitPeriod();
                    this.BackToMain();
                } else {
                    this.modalService.GenericError(error);
                    //this.BackToMain();//just in case, this will force a refresh which is safer...
                }
            });
    }
    private SetWaitPeriod() {
        let now = new Date();
        localStorage.setItem('DedupRev' + this.ReviewerIdentityServ.reviewerIdentity.reviewId, JSON.stringify(now.getTime()));
        this.NotificationService.show({
            content: "Sorry, the \"Get New Duplicates\" execution might be running, you need to wait for " + this.waitPeriod + " minutes and try again. If execution lasts more than a few hours, please contact EPPISupport.",
            hideAfter: 3000,
            position: { horizontal: 'center', vertical: 'top' },
            animation: { type: 'slide', duration: 400 },
            type: { style: 'error', icon: true },
            closable: true
        });
    }
    private ActivateFirstNotCompletedGroup() {
        if (this.DuplicateGroups.length > 0) {
            if (this.CurrentGroup == null || this.DuplicateGroups.findIndex(ff => this.CurrentGroup != null && ff.groupId == this.CurrentGroup.groupID) == -1) {
                const todo = this.DuplicateGroups.findIndex(found => found.isComplete == false);
                if (todo > 0) this.FetchGroupDetails(this.DuplicateGroups[todo].groupId);
                else this.FetchGroupDetails(this.DuplicateGroups[0].groupId);
            }
        } else {
            this.CurrentGroup == new ItemDuplicateGroup();
        }
    }


    public async FetchGroupDetails(groupId: number) {
        if (this.CurrentGroup && this.CurrentGroup.groupID == groupId && this.CurrentGroup.members.length > 0) return;//no need, we already have the details...
        //await Helpers.Sleep(50);
        this._BusyMethods.push("FetchGroupDetails");
        //await Helpers.Sleep(50);
        let body = JSON.stringify({ Value: groupId });
        return this._http.post<iItemDuplicateGroup>(this._baseUrl + 'api/Duplicates/FetchGroupDetails',
            body).toPromise().then(result => {
                let res = new ItemDuplicateGroup(result);
                this.CurrentGroup = res;
                //console.log(result);
                this.RemoveBusy("FetchGroupDetails");
                return this.CurrentGroup;
            }, error => {
                this.CurrentGroup = null;
                this.modalService.GenericError(error);
                this.RemoveBusy("FetchGroupDetails");
                return this.CurrentGroup;
            });
    }
    public  MarkUnmarkMemberAsDuplicate(toDo: MarkUnmarkItemAsDuplicate) {
        this._BusyMethods.push("MarkUnmarkMemberAsDuplicate");
        return this._http.post<iItemDuplicateGroup>(this._baseUrl + 'api/Duplicates/MarkUnmarkMemberAsDuplicate',
            toDo).toPromise().then(result => {
                //if (this.CurrentGroup) {
                //    for (let affected of toDo.itemDuplicateIds) {
                //        let found = this.CurrentGroup.members.find(ff => ff.itemDuplicateId == affected);
                //        if (found) {
                //            found.isDuplicate = toDo.isDuplicate;
                //            found.isChecked = true;
                //            if (this.CurrentGroup.members.length == this.CurrentGroup.members.filter(ff => ff.isChecked == true).length) {
                //                //whole group is checked, find it in the main list and update...
                //                let smallGr = this.DuplicateGroups.find(ff => ff.groupId == toDo.groupId);
                //                if (smallGr) {
                //                    smallGr.isComplete = true;
                //                }
                //            }
                //        }
                //    }
                //}
                this.RemoveBusy("MarkUnmarkMemberAsDuplicate");
                let smallGr = this.DuplicateGroups.find(f => f.groupId == result.groupID);
                this.CurrentGroup = new ItemDuplicateGroup(result);
                if (smallGr && (this.CurrentGroup.members.length == this.CurrentGroup.members.filter(ff => ff.isChecked == true).length))
                    smallGr.isComplete = true;
            }, error => {
                console.log("MarkUnmarkMemberAsDuplicate error", error);
                this.modalService.GenericError(error);
                this.RemoveBusy("MarkUnmarkMemberAsDuplicate");
            });
    }
    public MarkMemberAsMaster(toDo: MarkUnmarkItemAsDuplicate) {
        this._BusyMethods.push("MarkMemberAsMaster");
        this._http.post<ItemDuplicateGroup>(this._baseUrl + 'api/Duplicates/MarkMemberAsMaster',
            toDo).subscribe(result => {
                //if (this.CurrentGroup) {
                //    //whole group is not checked anymore!
                //    let smallGr = this.DuplicateGroups.find(ff => ff.groupId == toDo.groupId);
                //    if (smallGr) {
                //        smallGr.isComplete = false;
                //    }
                //    let gm = this.CurrentGroup.Master;
                //    let member = this.CurrentGroup.members.find(ff => ff.itemDuplicateId == toDo.itemDuplicateIds[0]);
                //    if (gm.itemDuplicateId !== 0) {
                //        gm.isMaster = false;
                //        gm.isChecked = false;
                //        gm.isDuplicate = false;
                //    }
                //    if (member) {
                //        member.isMaster = true;
                //        member.isChecked = true;
                //        member.isDuplicate = false;
                //    }
                //    if (gm.itemDuplicateId == 0 || !member) {
                //        this.FetchGroupDetails(toDo.groupId);
                //    }
                //}
                this.RemoveBusy("MarkMemberAsMaster");
                let smallGr = this.DuplicateGroups.find(f => f.groupId == result.groupID);
                this.CurrentGroup = new ItemDuplicateGroup(result);
                if (smallGr) smallGr.isComplete = false;
            }, error => {
                console.log("MarkMemberAsMaster error", error);
                this.RemoveBusy("MarkMemberAsMaster");
                this.modalService.GenericError(error);
            });
    }

    public DeleteCurrentGroup(GroupId: number) {
        if (this.CurrentGroup == null) return;
        else {
            this._BusyMethods.push("DeleteCurrentGroup");
            let toDo = JSON.stringify({ Value: GroupId });
            this._http.post<number>(this._baseUrl + 'api/Duplicates/DeleteGroup',
                toDo).subscribe(result => {
                    this.RemoveBusy("DeleteCurrentGroup");
                    this.CurrentGroup = null;
                    this.FetchGroups(false);
                }, error => {
                    console.log("DeleteCurrentGroup error", error);
                    this.RemoveBusy("DeleteCurrentGroup");
                    this.modalService.GenericError(error);
                });

        }
    }

    public DeleteAllGroups(DeleteAllDedupData: boolean) {
        
        this._BusyMethods.push("DeleteAllGroups");
        let toDo = JSON.stringify({ Value: DeleteAllDedupData });
        this._http.post(this._baseUrl + 'api/Duplicates/DeleteAllGroups',
            toDo).subscribe((result: any) => {
                this.RemoveBusy("DeleteAllGroups");
                this.CurrentGroup = null;
                this.FetchGroups(false);
            }, error => {
                console.log("DeleteAllGroups error", error);
                this.RemoveBusy("DeleteAllGroups");
                this.modalService.GenericError(error);
            });

        
    }

    public async MarkAutomatically(similarity: number, coded: number, docs: number) {
        //await Helpers.Sleep(50);
        this._BusyMethods.push("MarkAutomatically");
        this.allDone = false;
        this.currentCount = 0; this.ToDoCount = 0;
        console.log("MarkAutomatically");
        await this.FetchGroups(false);
        //console.log("MarkAutomatically1");
        if (this.DuplicateGroups.length == 0) {
            this.RemoveBusy("MarkAutomatically");
            return;
        }
        let ToDo = this.DuplicateGroups.filter(ff => ff.isComplete == false);
        this.ToDoCount = ToDo.length;
        while (this.currentCount < this.ToDoCount && this.allDone == false) {
            //console.log("getting group", ToDo[this.currentCount].groupId);
            let cGr = await this.FetchGroupDetails(ToDo[this.currentCount].groupId);
            //console.log("cGr", cGr);
            //console.log("got group", ToDo[this.currentCount].groupId);
            if (this.CurrentGroup && this.CurrentGroup.isEditable) {
                //let toSave = false;
                let IDs: number[] = [];
                for (let cm of this.CurrentGroup.JustMembers) {
                    if (
                        !cm.isMaster
                        && !cm.isDuplicate
                        && !cm.isChecked
                        && !cm.isExported
                        && cm.similarityScore >= similarity
                        && cm.codedCount <= coded
                        && cm.docCount <= docs
                        && this.CurrentGroup.HasOriginalMaster
                    ) {
                        cm.isChecked = true;
                        cm.isDuplicate = true;
                        IDs.push(cm.itemDuplicateId);
                    }
                }
                if (IDs.length > 0) {
                    //console.log("Will update grp: " + this.CurrentGroup.groupID + " " + (this.currentCount + 1) + " of " + this.ToDoCount);
                    let command: MarkUnmarkItemAsDuplicate = new MarkUnmarkItemAsDuplicate(this.CurrentGroup.groupID, 0, true);
                    command.itemDuplicateIds = IDs;
                    await this.MarkUnmarkMemberAsDuplicate(command);
                    //console.log("Updated grp: " + this.CurrentGroup.groupID + " " + (this.currentCount + 1) + " of " + this.ToDoCount);
                }
                this.currentCount++;
            } else {
                this.currentCount++;
            }
        }
        this.allDone = true;
        this.RemoveBusy("MarkAutomatically");
        //await Helpers.Sleep(50);
    }
    public RemoveManualMember(itemId: number) {
        if (!this.CurrentGroup || this.CurrentGroup.manualMembers.findIndex(ff => ff.itemId == itemId) == -1) return;
        this._BusyMethods.push("RemoveManualMember");
        let toDo = {
            groupId: this.CurrentGroup.groupID,
            itemId: itemId
        }
        this._http.post(this._baseUrl + 'api/Duplicates/RemoveManualMember',
            toDo).subscribe(result => {
                if (this.CurrentGroup && this.CurrentGroup.groupID == toDo.groupId) {
                    //remove manual member from client!
                    let ind = this.CurrentGroup.manualMembers.findIndex(ff => ff.itemId == itemId);
                    if (ind != -1) {
                        this.CurrentGroup.manualMembers.splice(ind, 1);
                    }
                }
                this.RemoveBusy("RemoveManualMember");
            }, error => {
                console.log("RemoveManualMember error", error);
                this.RemoveBusy("RemoveManualMember");
                this.modalService.GenericError(error);
            });
    }
    public DoSort() {
        //console.log("doSort", this.LocalSort);
        if (this.DuplicateGroups.length == 0 || this.LocalSort.SortBy == "") return;
        for (let property of Object.getOwnPropertyNames(this.DuplicateGroups[0])) {
            //console.log("doSort2", property);
            if (property == this.LocalSort.SortBy) {
                this.DuplicateGroups.sort((a: any, b: any) => {
                    if (this.LocalSort.Direction) {
                        if (a[property] > b[property]) {
                            return 1;
                        } else if (a[property] < b[property]) {
                            return -1;
                        } else {
                            return 0;
                        }
                    } else {
                        if (a[property] > b[property]) {
                            return -1;
                        } else if (a[property] < b[property]) {
                            return 1;
                        } else {
                            return 0;
                        }
                    }
                });
                break;
            }
            
        }
    }

    private BackToMain() {
        this.router.navigate(['Main']);
    }
    public Clear() {
        this.allDone = true;
        this.currentCount = 0;
        this.ToDoCount = 0;
        this.DuplicateGroups = [];
        this.CurrentGroup = null;
        this.LocalSort = new LocalSort();
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
    addItems: number[];
    removeItemID: number;
    isEditable: boolean;
    members: iDuplicateGroupMember[];
    manualMembers: iManualGroupMember[];
}
export interface iManualGroupMember {
    authors: string;
    groupID: 235935
    itemId: 218455
    month: string;
    parentAuthors: string;
    parentTitle: string;
    shortTitle: string;
    source: string;
    title: string;
    typeName: string;
    year: string;
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
            this.isEditable = iItemDGroup.isEditable;
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
    isEditable: boolean = false;
    members: DuplicateGroupMember[] = [];
    manualMembers: iManualGroupMember[] = [];
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
        this.itemDuplicateIds.push(memberId);
        this.isDuplicate = isDup;
    }
    groupId: number;
    itemDuplicateIds: number[] = [];
    isDuplicate: boolean;
}
export class GroupListSelectionCriteriaMVC {
    public groupId: number = 0;
    public itemIds: string = "0";
    }


