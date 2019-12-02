import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { DuplicatesService, iReadOnlyDuplicatesGroup, DuplicateGroupMember, MarkUnmarkItemAsDuplicate } from '../services/duplicates.service';
import { Helpers, LocalSort } from '../helpers/HelperMethods';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { ItemListService } from '../services/ItemList.service';


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
        private ItemListService: ItemListService
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
    
    public get DuplicateGroups(): iReadOnlyDuplicatesGroup[] {
        return this.DuplicatesService.DuplicateGroups;
    }
    public get CompletedGroups(): number {
        return this.DuplicatesService.DuplicateGroups.filter(found => found.isComplete == true).length;
    }
    public get CurrentGroup() {
        return this.DuplicatesService.CurrentGroup;
    }
    public get CurrentAutoGroup(): number {
        if (this.DuplicatesService.allDone) {
            this.ActivePanel = "";
            this.DuplicatesService.currentCount = 0;
            this.DuplicatesService.ToDoCount = 0;
        }
        return this.DuplicatesService.currentCount + 1;
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
        console.log("SortBy", fieldName);
        if (this.DuplicatesService.DuplicateGroups.length == 0) return;
        for (let property of Object.getOwnPropertyNames(this.DuplicatesService.DuplicateGroups[0])) {
            //console.log("SortByP", property);
            if (property == fieldName){
                if (this.DuplicatesService.LocalSort.SortBy == fieldName) {
                    console.log("SortBy", 1);
                    this.DuplicatesService.LocalSort.Direction = !this.DuplicatesService.LocalSort.Direction;
                } else {
                    console.log("SortBy", 2);
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
                    }
                }
            )
            .catch(() => { });
    }
    public MarkAutomatically() {
        this.ConfirmationDialogService.confirm("Mark Automatically?", "This could take some time and you won't be able to use EPPI-Reviewer while it's happening. <br/ > Whowever, you can cancel this process at any time", false, "")
            .then(
                (confirm: any) => {
                    if (confirm) {
                        this.HasAppliedChanges = true;
                        this.StartMarkAutomatically();
                    }
                }
            )
            .catch(() => { });
        this.ActivePanel = "MarkAutomatically";
        //this.DuplicatesService.MarkAutomatically(1, 0, 0);
    }
    private async StartMarkAutomatically() {
        await Helpers.Sleep(20);
        this.ActivePanel = "Running Mark Automatically";
        await Helpers.Sleep(20);
        this.DuplicatesService.MarkAutomatically(1, 0, 0);
    }
    private async StartAdvancedMarkAutomatically() {
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
    AdvancedMarkAutomaticallyShow() {
        this.ActivePanel = "AdvancedMarkAutomatically";
    }
    GoToManualMembers() {
        let el = document.getElementById('ManualMembersDiv');
        console.log("GoToManualMembers", el);
        if (el) el.scrollIntoView();
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
