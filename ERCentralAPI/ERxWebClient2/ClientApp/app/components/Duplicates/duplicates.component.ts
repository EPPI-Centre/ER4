import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { DuplicatesService, iReadOnlyDuplicatesGroup, DuplicateGroupMember, MarkUnmarkItemAsDuplicate } from '../services/duplicates.service';
import { Helpers } from '../helpers/HelperMethods';


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
		private _confirmationDialogService: ConfirmationDialogService,
		private ReviewerIdentityServ: ReviewerIdentityService,
        private _notificationService: NotificationService,
        private DuplicatesService: DuplicatesService
	) { }
    ngOnInit() {
        this.Refresh();
    }
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
    public FetchGroup(groupId: number): void {
        this.DuplicatesService.FetchGroupDetails(groupId);
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

    public MarkUnmarkItemAsDuplicate(member: DuplicateGroupMember, isDup: boolean) {
        let todo: MarkUnmarkItemAsDuplicate = new MarkUnmarkItemAsDuplicate(member.groupID, member.itemDuplicateId, isDup);
        this.DuplicatesService.MarkUnmarkMemberAsDuplicate(todo);
    }
    
    public MarkAsMaster(member: DuplicateGroupMember) {
        let todo: MarkUnmarkItemAsDuplicate = new MarkUnmarkItemAsDuplicate(member.groupID, member.itemDuplicateId, false);
        this.DuplicatesService.MarkMemberAsMaster(todo);
    }
    public Refresh() {
        this.DuplicatesService.FetchGroups(false);
    }
    public GetNewDuplicates() {
        this.DuplicatesService.FetchGroups(true);
    }
    public MarkAutomatically() {

    }
    AdvancedMarkAutomaticallyShow() {

    }
    ngOnDestroy() {
        this.Clear();
	}
	Clear() {
	}
    BackToMain() {
        this.router.navigate(['Main']);
    }
	 
}
