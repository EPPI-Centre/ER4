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
    private ShowingFilteredGroups: boolean = false;
    public currentCount = 0; public allDone: boolean = false; public ToDoCount = 0;
    public DuplicateGroups: PagedDuplicateGroupsList = new PagedDuplicateGroupsList();
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
                this.DuplicateGroups.WholeList = result;
                //console.log(result);
                this.ShowingFilteredGroups = false;
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
                this.DuplicateGroups.WholeList = result;
                //console.log(result);
                this.ShowingFilteredGroups = true;
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
    public async ActivateFirstNotCompletedGroup(force_it: boolean = false) {
        if (this.DuplicateGroups.WholeList.length > 0) {
            if (force_it == true
                ||
                this.CurrentGroup == null
                || this.DuplicateGroups.WholeList.findIndex(ff => this.CurrentGroup != null && ff.groupId == this.CurrentGroup.groupID) == -1) {
                const todo = this.DuplicateGroups.WholeList.findIndex(found => found.isComplete == false);
                if (todo >= 0) {
                    await this.FetchGroupDetails(this.DuplicateGroups.WholeList[todo].groupId);
                    if (this.DuplicateGroups.HasPages) this.DuplicateGroups.CurrentPage = Math.ceil(todo / this.DuplicateGroups.PageSize);//ensure the activated group appears in the current page
                }
                else {
                    //console.log("ActivateFirstNotCompletedGroup safety", todo);
                    await this.FetchGroupDetails(this.DuplicateGroups.WholeList[0].groupId);
                    this.DuplicateGroups.CurrentPage = 1;
                }
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
    public  MarkUnmarkMemberAsDuplicate(toDo: MarkUnmarkItemAsDuplicate): Promise<void> {
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
                let smallGr = this.DuplicateGroups.WholeList.find(f => f.groupId == result.groupID);
                this.CurrentGroup = new ItemDuplicateGroup(result);
                if (smallGr && (this.CurrentGroup.members.length == this.CurrentGroup.members.filter(ff => ff.isChecked == true).length))
                    smallGr.isComplete = true;
            }, error => {
                console.log("MarkUnmarkMemberAsDuplicate error", error);
                this.modalService.GenericError(error);
                this.RemoveBusy("MarkUnmarkMemberAsDuplicate");
            });
    }
    public MarkMemberAsMaster(toDo: MarkUnmarkItemAsDuplicate): Promise<void>{
        this._BusyMethods.push("MarkMemberAsMaster");
        return this._http.post<ItemDuplicateGroup>(this._baseUrl + 'api/Duplicates/MarkMemberAsMaster',
            toDo).toPromise().then(result => {
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
                let smallGr = this.DuplicateGroups.WholeList.find(f => f.groupId == result.groupID);
                this.CurrentGroup = new ItemDuplicateGroup(result);
                if (smallGr) smallGr.isComplete = false;
            }, error => {
                console.log("MarkMemberAsMaster error", error);
                this.RemoveBusy("MarkMemberAsMaster");
                this.modalService.GenericError(error);
            });
    }

    public DeleteCurrentGroup(GroupId: number): Promise<void> {
        if (this.CurrentGroup == null) return new Promise<void>(() => { });
        else {
            this._BusyMethods.push("DeleteCurrentGroup");
            let toDo = JSON.stringify({ Value: GroupId });
            return this._http.post<number>(this._baseUrl + 'api/Duplicates/DeleteGroup',
                toDo).toPromise().then(result => {
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

    public DeleteAllGroups(DeleteAllDedupData: boolean): Promise<void> {
        
        this._BusyMethods.push("DeleteAllGroups");
        let toDo = JSON.stringify({ Value: DeleteAllDedupData });
        return this._http.post(this._baseUrl + 'api/Duplicates/DeleteAllGroups',
            toDo).toPromise().then((result: any) => {
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
        if (this.DuplicateGroups.WholeList.length == 0) {
            this.RemoveBusy("MarkAutomatically");
            return;
        }
        let ToDo = this.DuplicateGroups.WholeList.filter(ff => ff.isComplete == false);
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
    public RemoveManualMember(itemId: number): Promise<void> {
        if (!this.CurrentGroup || this.CurrentGroup.manualMembers.findIndex(ff => ff.itemId == itemId) == -1) return new Promise<void>(() => { });
        this._BusyMethods.push("RemoveManualMember");
        let toDo = {
            groupId: this.CurrentGroup.groupID,
            itemId: itemId
        }
        return this._http.post(this._baseUrl + 'api/Duplicates/RemoveManualMember',
            toDo).toPromise().then(result => {
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

    public AddManualMembers(crit: GroupListSelectionCriteriaMVC): Promise<void> {
        this._BusyMethods.push("AddManualMembers");
        return this._http.post<iItemDuplicateGroup>(this._baseUrl + 'api/Duplicates/AddManualMembers',
            crit).toPromise().then(result => {
                let res = new ItemDuplicateGroup(result);
                this.CurrentGroup = res;
                this.RemoveBusy("AddManualMembers");

            }, error => {
                console.log("FetchGroups error", error, crit);
                    this.RemoveBusy("AddManualMembers");
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
    public FetchDirtyGroup(IdsList: string): Promise<ItemDuplicateDirtyGroup> {
        this._BusyMethods.push("FetchDirtyGroup");
        //await Helpers.Sleep(50);
        let body = JSON.stringify({ Value: IdsList });
        return this._http.post<iItemDuplicateDirtyGroup>(this._baseUrl + 'api/Duplicates/FetchDirtyGroup',
            body).toPromise().then(result => {
                let res = new ItemDuplicateDirtyGroup(result);
                this.RemoveBusy("FetchDirtyGroup");
                return res;
            }, error => {
                this.modalService.GenericError(error);
                this.RemoveBusy("FetchDirtyGroup");
                    return new ItemDuplicateDirtyGroup({ members: [] });
            });
    }
    public CreateNewGroup(crit: IncomingDirtyGroupMemberMVC[]): Promise<boolean> {
        this._BusyMethods.push("CreateNewGroup");
        return this._http.post<iReadOnlyDuplicatesGroup[]>(this._baseUrl + 'api/Duplicates/CreateNewGroup',
            crit).toPromise().then(result => {
                if (!this.ShowingFilteredGroups) {
                    //if we're showing all groups we add the new group to the list
                    if (result.length == 1) {
                        //easy: only one group in the current list contains the master of the created group
                        this.DuplicateGroups.WholeList.unshift(result[0]);
                        this.NotificationService.show({
                            content: "Group was created and added on top of the list of groups.",
                            position: { horizontal: 'center', vertical: 'top' },
                            animation: { type: 'slide', duration: 400 },
                            type: { style: 'info', icon: true },
                            closable: true
                        });
                    }
                    else if (result.length > 1) {
                        let maxid: number = 0;
                        for (let gr of result) {
                            if (gr.groupId > maxid) maxid = gr.groupId;
                        }
                        let i: number = result.findIndex(f => f.groupId == maxid);
                        if (i != -1) {
                            this.DuplicateGroups.WholeList.unshift(result[i]);
                            this.NotificationService.show({
                                content: "Group was created and added on top of the list of groups.",
                                position: { horizontal: 'center', vertical: 'top' },
                                animation: { type: 'slide', duration: 400 },
                                type: { style: 'info', icon: true },
                                closable: true
                            });
                        }
                    }
                }
                else {
                    //we inform the user that she needs to refresh the results to get to see the new group.
                    this.NotificationService.show({
                        content: "Group was created, please click 'refresh' to retrieve it.",
                        position: { horizontal: 'center', vertical: 'top' },
                        animation: { type: 'slide', duration: 400 },
                        type: { style: 'warning', icon: true },
                        closable: true
                    });
                }
                this.RemoveBusy("CreateNewGroup");
                return true;

            }, error => {
                    console.log("FetchGroups error", error, crit);
                    this.RemoveBusy("CreateNewGroup");
                    this.modalService.GenericError(error);
                    return false;
            });
    }
    public DoSort() {
        //console.log("doSort", this.LocalSort);
        if (this.DuplicateGroups.WholeList.length == 0 || this.LocalSort.SortBy == "") return;
        for (let property of Object.getOwnPropertyNames(this.DuplicateGroups.WholeList[0])) {
            //console.log("doSort2", property);
            if (property == this.LocalSort.SortBy) {
                this.DuplicateGroups.WholeList.sort((a: any, b: any) => {
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
        this.DuplicateGroups.WholeList = [];
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

export interface iItemDuplicateDirtyGroup {
    members: ItemDuplicateDirtyGroupMember[];
}
export class ItemDuplicateDirtyGroup {
    constructor(iGroup: iItemDuplicateDirtyGroup) {
        this.members = iGroup.members;
    }
    public getMaster(): ItemDuplicateDirtyGroupMember | null{
        for(let gm of this.members)
        {
            if (gm.isMaster) return gm;
        }
        return null;
    }
    public setMaster(ID: number): boolean {
        let canDo: boolean = false;
        for(let m of this.members)
        {
            if (m.itemId == ID && m.isAvailable) {
                canDo = true;
                break;
            }
        }
        if (!canDo) return false;
        let chk:number = 0;
        for (let m of this.members)
        {
            if (m.itemId == ID && m.isAvailable) {
                m.isMaster = true;
                chk++;
            }
            else if (m.isMaster) {
                m.isMaster = false;
                chk++;
            }
            if (chk == 2) return true;
        }
        return false;
    }
    public get MembersOnly(): ItemDuplicateDirtyGroupMember[] {
        return this.members.filter(f => f.isMaster == false);
    } 
    public get IsUsable(): boolean
    {
        let countM: number = 0;
        let countValidMembers: number  = 0;
        for(let m of this.members)
        {
            if (m.isMaster) countM++;
            if (m.isAvailable) countValidMembers++;
        }
        if (countM == 1 && countValidMembers > 1) return true;
        return false;
    }
    public get HasExportedMembers(): boolean {
        for (let m of this.members) {
            if (m.isExported) return true;
        }
        return false;
    }
    members: ItemDuplicateDirtyGroupMember[];
}
export interface ItemDuplicateDirtyGroupMember {
    itemId: number;
    title: string;
    parentTitle: string;
    shortTitle: string;
    year: string;
    month: string;
    authors: string;
    parentAuthors: string;
    isMaster: boolean;
    isExported: boolean;
    isEditable: boolean;
    codedCount: number;
    docCount: number;
    source: string;
    typeName: string;
    relatedGroupsCount: number;
    isAvailable: boolean;
    propertiesConverter: number;
}
export class IncomingDirtyGroupMemberMVC {
    itemId: number = -1;
    isMaster: boolean = false;
}

export class PagedDuplicateGroupsList {

    private _WholeList: iReadOnlyDuplicatesGroup[] = [];
    private _TotPages: number = -1;
    private _PageSize: number = 1000;
    public get WholeList(): iReadOnlyDuplicatesGroup[] {
        return this._WholeList;
    }
    public set WholeList(list: iReadOnlyDuplicatesGroup[]) {
        this.CurrentPage = 1;
        this._TotPages = -1;
        this._WholeList = list;
    }
    public get PageSize(): number {
        return this._PageSize;
    }
    public set PageSize(n: number) {
        this._PageSize = n;
        this._TotPages = -1;
    }
    public get TotPages(): number {
        if (this._TotPages == -1) {
            if (this.WholeList.length == 0) this._TotPages = 0;
            else this._TotPages = Math.ceil(this.WholeList.length / this.PageSize);
        }
        return this._TotPages;
    }
    public CurrentPage: number = 1;
    public get PagedList(): iReadOnlyDuplicatesGroup[] {
        if (this.WholeList.length <= this.PageSize) return this.WholeList;
        const StartIndex: number = (this.CurrentPage - 1) * this.PageSize;
        if (this.CurrentPage !== this.TotPages) {
            return this.WholeList.slice(StartIndex, StartIndex + this.PageSize);
        } else {
            return this.WholeList.slice(StartIndex);
        }
    }
    public PageUp() {
        if (this.CurrentPage < this.TotPages) this.CurrentPage++;
    }
    public PageDown() {
        if (this.CurrentPage > 1) this.CurrentPage--;
    }
    public get CanPageUp(): boolean {
        return this.CurrentPage < this.TotPages;
    }
    public get CanPageDown(): boolean {
        return this.CurrentPage > 1;
    }
    public get HasPages(): boolean {
        return this.TotPages > 1;
    }
}
    


