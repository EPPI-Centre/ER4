import { Component, Inject, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { timer, Subject, Subscription, Observable } from 'rxjs';
import { take, map, takeUntil } from 'rxjs/operators';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { WorkAllocationListService } from '../services/WorkAllocationList.service';
import { OnlineHelpService, FeedbackAndClientError4Create } from '../services/onlinehelp.service';
import { trigger, state, style, transition, animate, keyframes } from '@angular/animations';
import { NotificationService } from '@progress/kendo-angular-notification';



@Component({
    selector: 'HeaderComponent',
    templateUrl: './header.component.html',
    providers: [],
    animations: [
        trigger('HelpAppear', [
            state('hidden', style({
                transform: 'scaleY(0)',
                'height': '0px',
                transformOrigin: 'top',
                'overflow': 'hidden',
                opacity: 0
            })),
            state('visible', style({
                transform: 'scaleY(1)',
                'height': '*',
                transformOrigin: 'top',
                'overflow': 'auto',
                opacity: 1
            })),
            transition('hidden => visible', [
                animate('0.5s')
            ]),
            transition('visible => hidden', [
                animate('0.2s')
            ]),
        ]),
        trigger('PanelAppear', [
            state('hidden', style({
                'height': '0px',
                'overflow': 'hidden',
                opacity: 0
            })),
            state('visible', style({
                'height': '*',
                'overflow': 'auto',
                opacity: 1
            })),
            transition('hidden => visible', [
                animate('0.5s')
            ]),
            transition('visible => hidden', [
                animate('0.2s')
            ]),
        ]),
        trigger('PanelFeedbackAppear', [
            state('collapse', style({
                'height': '0px',
                'overflow': 'hidden',
                opacity: 0
            })),
            state('expand', style({
                'height': '*',
                'overflow': 'auto',
                opacity: 1
            })),
            transition('collapse => expand', [
                animate('0.5s')
            ]),
            transition('expand => collapse', [
                animate('0.2s')
            ]),
        ]),
    ],
})

export class HeaderComponent implements OnInit {

   

    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService,
        private OnlineHelpService: OnlineHelpService,
        private ReviewSetsService: ReviewSetsService,
        private ItemListService: ItemListService,
        private workAllocationListService: WorkAllocationListService,
        private notificationService: NotificationService
    ) {    }

    @Input() PageTitle: string | undefined;
    @Input() Context: string | undefined;
    private _ActivePanel: string = "";
    public set ActivePanel(val: string) {
        this._ActivePanel = val;
    }
    public get ActivePanel(): string {
        //console.log("closing help?:", this.OnlineHelpService.CurrentContext, this.Context);
        if (this._ActivePanel == "Help" && !this.IsServiceBusy && this.OnlineHelpService.CurrentContext != this.Context) {
            
            this._ActivePanel = "";
        }
        return this._ActivePanel;
    }
    public UserFeedback: string = "";

    public get NameOfUser(): string {
        return this.ReviewerIdentityServ.reviewerIdentity.name;
    }
    public get IsReadOnly(): boolean {
        if (this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated && this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0
            && this.ReviewerIdentityServ.reviewerIdentity.roles.indexOf('ReadOnlyUser') == -1) {
            //special case: user is authenticated, but not in a review and does not have the ReadOnlyUser role
            return false;
        }
        else {
            return !this.ReviewerIdentityServ.HasWriteRights;
        }
    }
    public get IsServiceBusy() : boolean {
        if (this.OnlineHelpService.IsBusy) return true;
        else return false;
    }
    public get CurrentContextHelp(): string {
        //console.log("CurrentContextHelp", this.OnlineHelpService.CurrentHTMLHelp);
        if (this.OnlineHelpService.IsBusy) return "";
        else {
            return this.OnlineHelpService.CurrentHTMLHelp;
        }
    }
    ngOnDestroy() {
    }
    Clear() {
  //      this.ItemListService.SaveItems(new ItemList(), new Criteria());
  //      this.ReviewSetsService.Clear();
		//this.workAllocationListService.Clear();
        //this.workAllocationContactListService.Save();
    }
    Logout() {
        this.Clear();
        this.ReviewerIdentityServ.LogOut();
        this.router.navigate(['home']);
    }
    ShowHideFeedback() {
        if (this.ActivePanel == "Feedback"
            || !this.Context
            || this.Context == '') {
            this._ActivePanel = "";
        }
        else if (this.Context && this.Context !== '') {
            //console.log("Feedback");
            this._ActivePanel = "Feedback";
        }
    }
    ShowHideHelp() {
        if (this.ActivePanel == "Help"
            || !this.Context
            || this.Context == '') {
            this._ActivePanel = "";
        }
        else if (this.Context && this.Context !== '') {
            this._ActivePanel = "Help";
            this.OnlineHelpService.FetchHelpContent(this.Context);
        }
    }
    SendFeedback() {
        let fb: FeedbackAndClientError4Create = new FeedbackAndClientError4Create();
        fb.context = (this.Context ? this.Context : "");
        fb.contactId = this.ReviewerIdentityServ.reviewerIdentity.userId;
        fb.isError = false;
        fb.message = this.UserFeedback;
        this.OnlineHelpService.CreateFeedbackMessage(fb);
        this.UserFeedback = "";
        this._ActivePanel = "";
        this.notificationService.show({
            content: "Thanks for your feed-back!",
            animation: { type: 'slide', duration: 800 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: 'success', icon: true },
            hideAfter: 2000
        });
    }
    ngOnInit() {
    }
}
