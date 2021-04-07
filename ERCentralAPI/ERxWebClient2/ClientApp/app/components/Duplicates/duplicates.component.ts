import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { DuplicatesService, iReadOnlyDuplicatesGroup, DuplicateGroupMember, MarkUnmarkItemAsDuplicate, ItemDuplicateGroup, GroupListSelectionCriteriaMVC, ItemDuplicateDirtyGroup, ItemDuplicateDirtyGroupMember, IncomingDirtyGroupMemberMVC } from '../services/duplicates.service';
import { Helpers, LocalSort } from '../helpers/HelperMethods';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { ItemListService } from '../services/ItemList.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { Group } from '@progress/kendo-drawing';
import { timeout } from 'rxjs/operators';


@Component({
    selector: 'DuplicatesComp',
    templateUrl: './duplicates.component.html',
    providers: [],
    styles: [`
                .bg-lev0 {    
                    background-color: #FFFFFF;
                }
               .bg-lev1 {    
                    background-color: #FFFDAD;
                }
               .bg-lev2 {    
                    background-color: #ffc520;
                }
               .bg-lev3 {    
                    background-color: #FF69B4;
                }
                .z-index-table {
                    border-collapse:separate; border-spacing:0px;
                }
                    
            `]

})

export class DuplicatesComponent implements OnInit, OnDestroy {
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
		private ReviewerIdentityServ: ReviewerIdentityService,
        private _notificationService: NotificationService,
        private ConfirmationDialogService: ConfirmationDialogService,
        private CodesetStatisticsService: CodesetStatisticsService,
        private DuplicatesService: DuplicatesService,
        private ItemListService: ItemListService,
        private eventsService: EventEmitterService
	) { }
    ngOnInit() {
        this.DuplicatesService.currentCount = 0;
        this.DuplicatesService.ToDoCount = 0;
        this.DuplicatesService.allDone = true;
        this.DuplicatesService.LocalSort = new LocalSort();
        this.Refresh();
    }
    public similarityCr: number = 1;
    public codedCr: number = 0;
    public docsCr: number = 0;
    public ActivePanel: string = "";
    public ItemIDsearchString: string = "";
    public ShowingMore: boolean = false;
    public ShowItemsList: boolean = false;
    public ShowGroupslistTools: boolean = false;
    public DirtyGroup: ItemDuplicateDirtyGroup | null = null;
    public AutoAdvance: boolean = true;
    //public lowThresholdWarningActive: boolean = "";
    public get lowThresholdWarningActive(): boolean {
        if (this.similarityCr < 0.8) return true;
        else return false;
    }
    private _HasAppliedChanges_veryPrivate: boolean = false;
    private get HasAppliedChanges(): boolean {
        return this._HasAppliedChanges_veryPrivate;
    }
    private set HasAppliedChanges(val: boolean) {
        if (val == true && this.ShowItemsList == true) {
            //we'd better refresh ths item list!!
            this.ItemListService.Refresh();
        }
        this._HasAppliedChanges_veryPrivate = val;
    }
    public get IsServiceBusy(): boolean {
        //console.log("mainfull IsServiceBusy", this.ItemListService, this.codesetStatsServ, this.SourcesService )
        return (
            this.DuplicatesService.IsBusy || this.ItemListService.IsBusy
            //|| this.ComparisonsService.IsBusy
        );
    }
    public get HasWriteRights(): boolean {
        return this.ReviewerIdentityServ.HasWriteRights;
    }
    public MarkAutomaticallyDDData: Array<any> =[{
        text: 'Advanced Mark Automatically',
        click: () => {
            this.AdvancedMarkAutomaticallyShow();
        }
    }];
    public ResetDDData: Array<any> = [{
        text: 'Hard Reset...',
        click: () => {
            this.HardReset();
        }
    }];
    public FindGroupsDDData: Array<any> = [{
        text: 'Find Groups by Item IDs',
        click: () => {
            this.ShowFindByItemIDsPanel();
        }
    }];
    public PagingDD: number[] = [
        20, 50, 100, 500, 1000, 2000, 5000
    ]
    public get ShowOrHideMoreToolbarText(): string {
        if (this.ShowingMore) return "Less...";
        else return "More..."
    }
    public get DuplicateGroups(): iReadOnlyDuplicatesGroup[] {
        return this.DuplicatesService.DuplicateGroups.PagedList;
    }
    public get CurrentPage(): number {
        return this.DuplicatesService.DuplicateGroups.CurrentPage;
    }
    public get TotPages(): number {
        return this.DuplicatesService.DuplicateGroups.TotPages;
    }
    public get TotGroups(): number {
        return this.DuplicatesService.DuplicateGroups.WholeList.length;
    }
    public get CurrentListHasPages(): boolean {
        return this.DuplicatesService.DuplicateGroups.HasPages;
    }
    public get CanPageUp(): boolean {
        return this.DuplicatesService.DuplicateGroups.CanPageUp;
    }
    public get CanPageDown(): boolean {
        return this.DuplicatesService.DuplicateGroups.CanPageDown;
    }
    public get PageSize(): number {
        return this.DuplicatesService.DuplicateGroups.PageSize;
    }
    public set PageSize(n: number) {
        this.DuplicatesService.DuplicateGroups.PageSize = n;
    }
    public PageUp() {
        this.DuplicatesService.DuplicateGroups.PageUp();
    }
    public PageDown() {
        this.DuplicatesService.DuplicateGroups.PageDown();
    }
    public FirstPage() {
        this.DuplicatesService.DuplicateGroups.CurrentPage = 1;
    }
    public LastPage() {
        this.DuplicatesService.DuplicateGroups.CurrentPage = this.DuplicatesService.DuplicateGroups.TotPages;
    }
    public async FindNextUnchecked() {
        
        await this.DuplicatesService.ActivateFirstNotCompletedGroup(true);
        await Helpers.Sleep(10);
        console.log("about to scroll:", this.CurrentGroup);
        if (this.CurrentGroup != null) {
            let el = document.getElementById("grp-" + this.CurrentGroup.groupID.toString());
            console.log("about to scroll2:", el);
            if (el) el.scrollIntoView();
        }
    }
    public get CompletedGroups(): number {
        return this.DuplicatesService.DuplicateGroups.WholeList.filter(found => found.isComplete == true).length;
    }
    public get CurrentGroup(): ItemDuplicateGroup | null {
        return this.DuplicatesService.CurrentGroup;
    }
    public get CurrentAutoGroup(): number {
        if (this.DuplicatesService.allDone) {
            if (this.ActivePanel !== "") {
                //console.log("before?", this.ActivePanel);
                this.ResetActivePanel();
                //console.log("after?");
            }
            //this.ActivePanel = "";
            this.DuplicatesService.currentCount = 0;
            this.DuplicatesService.ToDoCount = 0;
        }
        return this.DuplicatesService.currentCount + 1;
    }
    public ShowHideItemsList() {
        if (this.ShowItemsList == false && this.HasAppliedChanges == true) {
            //we need to refresh the items list!
            this.ItemListService.Refresh();
        }
        this.ShowItemsList = !this.ShowItemsList;
    }
    public async ResetActivePanel() {
        //small trick to avoid the ExpressionChangedAfterItHasBeenCheckedError dreaded error, sleep before resetting, to give time to NG to catch up on the visual side...
        await Helpers.Sleep(20);
        this.DirtyGroup = null;
        this.ActivePanel = "";
    }
    public get TotalAutoAutoGroups(): number {
        return this.DuplicatesService.ToDoCount;
    }

    public get DirtyMaster(): ItemDuplicateDirtyGroupMember | null {
        if (this.DirtyGroup == null) return null;
        else {
            return this.DirtyGroup.getMaster();
        }
    }
    public DirtyMemberStatus(member: ItemDuplicateDirtyGroupMember): string {
        const value = member.propertiesConverter;
        if (value == null ) return "ERROR: no data, please contact the support team!";
        if (value == 0) return "This Item does not belong to any group, you can use it as a master.";
        if (value == 1) return "This Item belongs to some other group(s): consider using the related group(s) instead of manually creating a new group.";
        if (value == 2) return "This Item is the Master of some other group: it can't be inserted into the new group. Consider using its own group instead of manually creating a new group.";
        else return "ERROR: data is inconsistent, please contact the support team!";         
    }

    public FetchGroup(groupId: number): void {
        this.DuplicatesService.FetchGroupDetails(groupId);
    }
    public MainListRowClass(group: iReadOnlyDuplicatesGroup): string {
        if (!this.DuplicatesService.CurrentGroup) return "";
        else {
            if (group.groupId == this.DuplicatesService.CurrentGroup.groupID && group.isComplete) {
                return "font-weight-bold bg-success";
            }
            else if (group.groupId == this.DuplicatesService.CurrentGroup.groupID && !group.isComplete) {
                return "font-weight-bold alert-primary";
            }
            else if (group.groupId !== this.DuplicatesService.CurrentGroup.groupID && group.isComplete) {
                return "alert-success";
            }
            else return "";
        }
    }
    public DistanceClass(a: string, b: string): string {
        if (a.length == 0 || b.length == 0) return "bg-lev0";
        if (a == b) return "bg-lev0";//we don't calculate the distance if the two strings are equal!!
        //next, 2 "tricks" to speed things when calculating the distance would take too long.
        if (a.length > 5000 || b.length > 5000) {
            //this is rare, but UI would grind to a halt, so we take a draconian stance: strongest visual cue. The two strings are different!
            return "bg-lev3";
        }
        if (a.length > 2000 || b.length > 2000) {
            //we still need to speed up things. LevDist cicles through all chars of a, times all chars for b so it becomes slow when strings are long!
            if (a.length > 2000) a = a.substring(0, 2000) + "t";
            if (b.length > 2000) b = b.substring(0, 2000) + "T";
            //the added "t" and "T" ensure the two truncated strings are never identical, as we do know they are different...
            //we'll compare only the first 2000 chars...
        }
        let dist = Helpers.LevDist(a, b);
        //console.log("Distance Class: ", dist);
        if (dist >= 1) return "bg-lev0";
        else if (dist > 0.9) return "bg-lev1";
        else if (dist > 0.75) return "bg-lev2";
        else return "bg-lev3";
    }
    public SortBy(fieldName: string) {
        //console.log("SortBy", fieldName);
        if (this.DuplicatesService.DuplicateGroups.WholeList.length == 0) return;
        for (let property of Object.getOwnPropertyNames(this.DuplicatesService.DuplicateGroups.WholeList[0])) {
            //console.log("SortByP", property);
            if (property == fieldName){
                if (this.DuplicatesService.LocalSort.SortBy == fieldName) {
                    //console.log("SortBy", 1);
                    this.DuplicatesService.LocalSort.Direction = !this.DuplicatesService.LocalSort.Direction;
                } else {
                    //console.log("SortBy", 2);
                    this.DuplicatesService.LocalSort.Direction = true;
                    this.DuplicatesService.LocalSort.SortBy = fieldName;
                }
                this.DuplicatesService.DoSort();
                break;
            }
        }
        
    }
    public SortingSymbol(fieldName: string): string {
        if (this.DuplicatesService.LocalSort.SortBy !== fieldName) return "";
        else if(this.DuplicatesService.LocalSort.Direction) return '&uarr;';
        else return '&darr;';
    }
    public get CurrentSort(): LocalSort {
        return this.DuplicatesService.LocalSort;
    }
    public MarkUnmarkItemAsDuplicate(member: DuplicateGroupMember, isDup: boolean) {
        let todo: MarkUnmarkItemAsDuplicate = new MarkUnmarkItemAsDuplicate(member.groupID, member.itemDuplicateId, isDup);
        this.DuplicatesService.MarkUnmarkMemberAsDuplicate(todo).then(() => {
            console.log("is auto mark on???", this.ActivePanel);
            this.HasAppliedChanges = true;
            if (this.AutoAdvance == true && this.CurrentGroup && this.CurrentGroup.members.length == this.CurrentGroup.members.filter(f => f.isChecked == true).length) {
                this.FindNextUnchecked();
            }
        });
    }
    
    public MarkAsMaster(member: DuplicateGroupMember) {
        let todo: MarkUnmarkItemAsDuplicate = new MarkUnmarkItemAsDuplicate(member.groupID, member.itemDuplicateId, false);
        this.DuplicatesService.MarkMemberAsMaster(todo).then(() => {
            this.HasAppliedChanges = true;
        });
    }
    public RemoveManualMember(itemId: number) {
        this.DuplicatesService.RemoveManualMember(itemId).then(() => {
            this.HasAppliedChanges = true;
        });
    }
    public Refresh() {
        if (this.DuplicatesService.DuplicateGroups.WholeList.length == 0) this.DuplicatesService.CurrentGroup = null;
        else if (
            this.DuplicatesService.CurrentGroup != null
            && this.DuplicatesService.DuplicateGroups.WholeList.findIndex(
                ff => this.DuplicatesService.CurrentGroup != null && ff.groupId == this.DuplicatesService.CurrentGroup.groupID
            ) == -1
        ) {//we have a list but current group isn't in it!
            this.DuplicatesService.CurrentGroup = null;
        }
        this.DuplicatesService.FetchGroups(false);
    }

    public GetNewDuplicates() {
        let innerMsg = ""
        let totalItems = this.CodesetStatisticsService.ReviewStats.itemsIncluded + this.CodesetStatisticsService.ReviewStats.itemsExcluded + this.CodesetStatisticsService.ReviewStats.itemsDeleted; 
        if (totalItems < 1000) {
            innerMsg = "Do you want to start finding new duplicates?";
        }
        if (totalItems >= 1000 && totalItems < 10000) {
            innerMsg = "<b>Do you want to start finding new duplicates?</b><br />This review contains " + totalItems +" items. We expect this will take several minutes.";
        }
        if (totalItems >= 10000 && totalItems < 20000) {
            innerMsg = "<b>Do you want to start finding new duplicates?</b><br />This review contains " + totalItems + " items. We expect this will take 20 minutes or more.";
        }
        if (totalItems >= 20000 && totalItems < 55000) {
            innerMsg = "<b>Do you want to start finding new duplicates?</b><br />This review contains " + totalItems + " items. We expect this will take quite some time, perhaps hours.";
        }
        if (totalItems >= 55000) {
            innerMsg = "<b>Do you want to start finding new duplicates?</b><br />This review contains " + totalItems + " items. We expect this will take quite some time, perhaps hours.";
            innerMsg += "<br /><b>Note:</b> The deduplication system cannot handle more 50K groups, if you expect you'll get more groups than this number, please contact EPPI-Support.";
        }
        this.ConfirmationDialogService.confirm("Start Get New Duplicates?", innerMsg, false, "")
            .then(
                (confirm: any) => {
                    if (confirm) {
                        this.DuplicatesService.FetchGroups(true);
                        this._notificationService.show({
                            content: "Please wait (up to 5 minutes), looking for new duplicates!",
                            animation: { type: 'slide', duration: 400 },
                            position: { horizontal: 'center', vertical: 'top' },
                            type: { style: "warning", icon: true },
                            hideAfter: 20000
                        });
                    }
                }
            )
            .catch(() => { });
    }
    public MarkAutomatically() {
        this.ConfirmationDialogService.confirm("Mark Automatically?", "This could take some time and you won't be able to use EPPI-Reviewer while it's happening. <br/ > However, you can cancel this process at any time", false, "")
            .then(
                (confirm: any) => {
                    if (confirm) {
                        this.StartMarkAutomatically();
                    }
                }
            )
            .catch(() => { });
        //this.ActivePanel = "MarkAutomatically";
        //this.DuplicatesService.MarkAutomatically(1, 0, 0);
    }

    public openConfirmationDialogAutoMatchWithLowThreshold() {

        this.ConfirmationDialogService.confirm('Please confirm', 'You are setting a low threshold that could erroneously mark some items as duplicates.' +
            '<br />Please type \'I confirm\' in the box below if you are sure you want to proceed.', true, "I confirm")
            .then(
                (confirm: any) => {
                    //console.log('Text entered is the following: ' + confirm + ' ' + this.eventsService.UserInput );
                    if (confirm && this.eventsService.UserInput.toLowerCase().trim() == 'i confirm') {
                        this.DoAdvancedMarkAutomatically();
                    }
                }
            )
            .catch();
    }
    private ShowFindByItemIDsPanel() {
        if (this.ActivePanel !== "FindByItemIDs") {
            this.ActivePanel = "FindByItemIDs";
        }
    }
    private async StartMarkAutomatically() {
        this.ShowItemsList = false;//close items: we don't have a way to ensure it will keep-up with mark-automatically!
        this.HasAppliedChanges = true;
        await Helpers.Sleep(20);
        this.ActivePanel = "Running Mark Automatically";
        await Helpers.Sleep(20);
        this.DuplicatesService.MarkAutomatically(1, 0, 0);
    }
    public StartAdvancedMarkAutomatically() {
        if (this.similarityCr >= 0.8) {
            this.DoAdvancedMarkAutomatically();
        }
        else {
            this.openConfirmationDialogAutoMatchWithLowThreshold();
        }
    }
    private async DoAdvancedMarkAutomatically() {
        this.ShowItemsList = false;//close items: we don't have a way to ensure it will keep-up with mark-automatically!
        this.HasAppliedChanges = true;
        await Helpers.Sleep(20);
        this.ActivePanel = "Running Mark Automatically";
        await Helpers.Sleep(20);
        this.DuplicatesService.MarkAutomatically(this.similarityCr, this.codedCr, this.docsCr);
    }
    public StopMarkAutomatically() {
        this.ActivePanel = "";
        this.DuplicatesService.currentCount = 0;
        this.DuplicatesService.ToDoCount = 0;
        this.DuplicatesService.allDone = true;
    }
    public async AdvancedMarkAutomaticallyShow() {
        this.ActivePanel = "AdvancedMarkAutomatically";
    }
    GoToManualMembers() {
        let el = document.getElementById('ManualMembersDiv');
        console.log("GoToManualMembers", el);
        if (el) el.scrollIntoView();
    }

    public SoftReset() {
        const confirmM = "I agree";
        let msg = "You can delete all duplicate groups and keep information about references already marked as duplicates.<br />";
        msg += "This will give you a fresh start to re-evaluate duplicates without losing the work you've done already.<br />"
        msg += "Note that documents already marked as duplicates will not be re-evaluated, and this will have a few consequences:<br />";
        msg += "<ol><li>When you 'Get new Duplicates' you should get a smaller number of groups as all 'completed' groups should not reappear.</li>";
        msg += "<li>Overlapping groups will not show up again.</li>";
        msg += "<li>Information about the old groups will be LOST! You will not be able to find out the similarity scores of items you have already marked as duplicates.</li></ol>";
        msg += "If you are really sure that you want to proceed, type 'I agree' in the box below and click 'OK'.";
        
        this.ConfirmationDialogService.confirm('Delete All Groups?', msg, true, confirmM, "OK", "Cancel", "lg")
            .then(
                (confirm: any) => {
                    if (confirm == true && this.eventsService.UserInput.toLowerCase().trim() == confirmM.toLowerCase()) {
                        this.DuplicatesService.DeleteAllGroups(false).then(() => {
                            this.HasAppliedChanges = true;
                        });
                    }
                }
            )
            .catch();
    }

    public HardReset() {
        const confirmM = "I Confirm";
        let msg = "You can delete all duplicate groups and <strong>also</strong> all information about what has / has not been marked as duplicates.<br />";
        msg += "Note that references already marked as duplicates will reappar (marked as Included).<br />"
        msg += "You might want to proceed with this rather radical choice in case:<br />";
        msg += "<ol><li>You have used the 'Advanced Mark Automatically' feature with too permissive thresholds and you have marked as duplicates too many false positives.";
        msg += "In this case, deleting all dedup data and starting over again is likely to be faster than manually looking for errors.</li>";
        msg += "<li>You have a large number of overlapping groups and you have not invested a lot of time in manually evaluating groups.";
        msg += "Getting a 100% fresh start will eliminate overlapping groups and allow you to re-run the automatic marking procedure with little waste of time.</li></ol> ";
        msg += "If you are really sure that you want to proceed, type 'I Confirm' in the box below and click 'OK'.";

        this.ConfirmationDialogService.confirm('Delete All Data about Duplicates?', msg, true, confirmM, "OK", "Cancel", "lg")
            .then(
                (confirm: any) => {
                    if (confirm == true && this.eventsService.UserInput.toLowerCase().trim() == confirmM.toLowerCase()) {
                        this.DuplicatesService.DeleteAllGroups(true).then(() => {
                            this.HasAppliedChanges = true;
                        });
                    }
                }
            )
            .catch();
    }

    public FindRelatedGroups() {
        if (this.CurrentGroup == null) return;
        else {
            this.DuplicatesService.FetchRelatedGroups(this.CurrentGroup.groupID);
        }
    }
    public FindByItemIDs() {
        if (this.ItemIDsearchString == "") return;
        let CheckedString: string = "";
        const check = this.ItemIDsearchString.split(",");
        for (let idSt of check) {
            idSt = idSt.trim();
            const id = Number(idSt);
            if (isNaN(id)) return;
            else if (id <= 0 || id == null || id == undefined) return;
            else if (!Number.isInteger(id)) return;
            if (CheckedString !== "") CheckedString += "," + idSt;
            else CheckedString = idSt;
        }
        //we did not return, so all elements in the comma-separated split and CheckedString can be used
        this.ItemIDsearchString = CheckedString;
        this.DuplicatesService.FetchGroupsByItemIds(CheckedString);
    }
    public DeleteCurrentGroup(GroupId: number) {
        if (this.CurrentGroup == null || this.CurrentGroup.groupID != GroupId) return;
        else {
            let dups = this.CurrentGroup.members.filter(f => f.isDuplicate == true).length;
            if (this.CurrentGroup.manualMembers) dups += this.CurrentGroup.manualMembers.length;
            let msg = 'You are deleting this group (Id = ' + GroupId + '). This action <strong>can\'t be undone</strong>!';
            if (dups == 0) {
                msg += '<BR />This groups does not contain items already marked as duplicates';
            } else if (dups == 1) {
                msg += '<BR />This groups contains one item marked as a duplicate. If you delete this group, this item will be marked as not a duplicate and will receive the "I" (for "Included") flag.';
            } else if (dups > 1) {
                msg += '<BR />This groups contains ' + dups.toString()
                    + ' items marked as a duplicates. If you delete this group, these items will be marked as not a duplicate and will receive the "I" (for "Included") flag.';
            }

            this.ConfirmationDialogService.confirm('Delete Group?', msg, false, "")
            .then(
                (confirm: any) => {
                    if (confirm == true) {
                        this.DuplicatesService.DeleteCurrentGroup(GroupId).then(() => {
                            this.HasAppliedChanges = true;
                        });
                    }
                }
            )
                .catch();
        }
    }

    public FindBySelectedItems() {

    }
    public get CanAddSelectedItemsToGroup(): boolean {
        if (!this.HasWriteRights) return false;
        else if (this.ItemListService.SelectedItems.length > 0 && this.CurrentGroup != null) return true;
        else return false;
    }
    public AddSelectedItemsToGroup() {
        if (this.CurrentGroup == null || this.ItemListService.SelectedItems.length == 0) return;
        else {
            let innerMsg: string = "Are you sure you want to add the <strong>" + this.ItemListService.SelectedItems.length.toString()
                + "</strong> selected items (below) to the current group (<strong> " + this.CurrentGroup.Master.shortTitle 
                + "</strong> - Id: " + this.CurrentGroup.groupID.toString() + ")?"
            this.ConfirmationDialogService.confirm("Manually add these items?", innerMsg, false, "")
            .then(
                (confirm: any) => {
                    if (confirm) {
                        this.DoAddSelectedItemsToGroup();
                    }
                }
            ).catch(() => { });
        }
    }
    private DoAddSelectedItemsToGroup() {
        if (this.CurrentGroup == null || this.ItemListService.SelectedItems.length == 0) return;
        let IDG: ItemDuplicateGroup = this.CurrentGroup; 
        let ItemsIds: string = "";
        const currmas: number = IDG.Master.itemId;
        for (let it of this.ItemListService.SelectedItems)
        {
            if (it.itemId != currmas) ItemsIds += it.itemId.toString() + ',';
        }
        ItemsIds = ItemsIds.substr(0, ItemsIds.length - 1);
        if (ItemsIds.length > 0) {
            let crit: GroupListSelectionCriteriaMVC = new GroupListSelectionCriteriaMVC();
            crit.groupId = IDG.groupID;
            crit.itemIds = ItemsIds;
            this.DuplicatesService.AddManualMembers(crit).then(() => {
                this.HasAppliedChanges = true;
            });
        }
    }
    public get CanGetDirtyGroup(): boolean {
        if (!this.HasWriteRights) return false;
        else if (this.ItemListService.SelectedItems.length > 0) return true;
        else return false;
    }
    public CanGoToThisItem(ID: number) {
        if (this.ItemListService.ItemList.items.findIndex(f => f.itemId == ID) != -1) return true;
        else return false;
    }
    public GetDirtyGroup() {
        let ItemsIds: string = "";
        for (let it of this.ItemListService.SelectedItems) {
            ItemsIds += it.itemId.toString() + ',';
        }
        ItemsIds = ItemsIds.substr(0, ItemsIds.length - 1);
        if (ItemsIds.length > 0) {
            this.DuplicatesService.FetchDirtyGroup(ItemsIds).then(res => {
                if (res.members.length > 0) {
                    this.DirtyGroup = res;
                    this.ActivePanel = "CreateGroup";
                } else { this.DirtyGroup = null;}
            });
        }
    }
    OpenItem(itemId: number) {
        if (itemId > 0) {
            this.router.navigate(['itemcoding', itemId]);
        }
    }
    public FindRelatedGroupsForDirtyGr() {
        if (this.DirtyGroup != null) {
            let Ids = "";
            for (let m of this.DirtyGroup.members) {
                Ids += m.itemId.toString() + ',';
            }
            Ids = Ids.substr(0, Ids.length - 1);
            if (Ids.length > 0) this.DuplicatesService.FetchGroupsByItemIds(Ids);
        }
    }
    public MakeMasterOfDirtyGroup(member: ItemDuplicateDirtyGroupMember) {
        if (this.DirtyGroup != null) this.DirtyGroup.setMaster(member.itemId)
    }
    public get CanCreateThisGroup(): boolean {
        if (this.HasWriteRights && this.DirtyGroup !== null && this.DirtyGroup.IsUsable) return true;
        else return false;
    }
    public CreateGroup() {
        if (!this.CanCreateThisGroup) return;
        else if (this.DirtyGroup != null) {
            let output: IncomingDirtyGroupMemberMVC[] = []
            for (let m of this.DirtyGroup.members) {
                let mm = new IncomingDirtyGroupMemberMVC();
                mm.isMaster = m.isMaster;
                mm.itemId = m.itemId;
                output.push(mm);
            }
            if (output.length > 1) this.DuplicatesService.CreateNewGroup(output).then(res => {
                this.ResetActivePanel();
                this.HasAppliedChanges = true;
            });
        }
    }

    ngOnDestroy() {
        this.Clear();
	}
    Clear() {
        //this.DuplicatesService.Clear();
	}
    BackToMain() {
        if (this.HasAppliedChanges) {
            this.CodesetStatisticsService.GetReviewStatisticsCountsCommand();
            this.ItemListService.Refresh();
        }
        //this.Clear();
        this.router.navigate(['Main']);
    }
	 
}
