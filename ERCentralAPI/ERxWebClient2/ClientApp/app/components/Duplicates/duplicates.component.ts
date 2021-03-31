import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { DuplicatesService, iReadOnlyDuplicatesGroup, DuplicateGroupMember, MarkUnmarkItemAsDuplicate, ItemDuplicateGroup } from '../services/duplicates.service';
import { Helpers, LocalSort } from '../helpers/HelperMethods';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { ItemListService } from '../services/ItemList.service';
import { EventEmitterService } from '../services/EventEmitter.service';


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
    //public lowThresholdWarningActive: boolean = "";
    public get lowThresholdWarningActive(): boolean {
        if (this.similarityCr < 0.8) return true;
        else return false;
    }
    private HasAppliedChanges: boolean = false;
    public get IsServiceBusy(): boolean {
        //console.log("mainfull IsServiceBusy", this.ItemListService, this.codesetStatsServ, this.SourcesService )
        return (
            this.DuplicatesService.IsBusy
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
    public get ShowOrHideMoreToolbarText(): string {
        if (this.ShowingMore) return "Less...";
        else return "More..."
    }
    public get DuplicateGroups(): iReadOnlyDuplicatesGroup[] {
        return this.DuplicatesService.DuplicateGroups;
    }
    public get CompletedGroups(): number {
        return this.DuplicatesService.DuplicateGroups.filter(found => found.isComplete == true).length;
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
    public async ResetActivePanel() {
        //small trick to avoid the ExpressionChangedAfterItHasBeenCheckedError dreaded error, sleep before resetting, to give time to NG to catch up on the visual side...
        await Helpers.Sleep(20);
        this.ActivePanel = "";
    }
    public get TotalAutoAutoGroups(): number {
        return this.DuplicatesService.ToDoCount;
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
        let dist = Helpers.LevDist(a, b);
        //console.log("Distance Class: ", dist);
        if (dist >= 1) return "bg-lev0";
        else if (dist > 0.9) return "bg-lev1";
        else if (dist > 0.75) return "bg-lev2";
        else return "bg-lev3";
    }
    public SortBy(fieldName: string) {
        //console.log("SortBy", fieldName);
        if (this.DuplicatesService.DuplicateGroups.length == 0) return;
        for (let property of Object.getOwnPropertyNames(this.DuplicatesService.DuplicateGroups[0])) {
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
        this.DuplicatesService.MarkUnmarkMemberAsDuplicate(todo);
        this.HasAppliedChanges = true;
    }
    
    public MarkAsMaster(member: DuplicateGroupMember) {
        let todo: MarkUnmarkItemAsDuplicate = new MarkUnmarkItemAsDuplicate(member.groupID, member.itemDuplicateId, false);
        this.DuplicatesService.MarkMemberAsMaster(todo);
        this.HasAppliedChanges = true;
    }
    public RemoveManualMember(itemId: number) {
        this.DuplicatesService.RemoveManualMember(itemId);
    }
    public Refresh() {
        if (this.DuplicatesService.DuplicateGroups.length == 0) this.DuplicatesService.CurrentGroup = null;
        else if (
            this.DuplicatesService.CurrentGroup != null
            && this.DuplicatesService.DuplicateGroups.findIndex(
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
                        this.HasAppliedChanges = true;
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
                        this.HasAppliedChanges = true;
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
                        this.DuplicatesService.DeleteAllGroups(false);
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
                        this.DuplicatesService.DeleteAllGroups(true);
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
                        this.DuplicatesService.DeleteCurrentGroup(GroupId);
                    }
                }
            )
                .catch();
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
