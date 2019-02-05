import { Component, Inject, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { timer, Subject, Subscription, Observable } from 'rxjs';
import { take, map, takeUntil } from 'rxjs/operators';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { OnlineHelpService } from '../services/onlinehelp.service';
import { trigger, state, style, transition, animate, keyframes } from '@angular/animations';



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
    ],
})

export class HeaderComponent implements OnInit {

   

    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService,
        private OnlineHelpService: OnlineHelpService,
        private ReviewSetsService: ReviewSetsService,
        private ItemListService: ItemListService,
        private workAllocationContactListService: WorkAllocationContactListService
    ) {    }

    @Input() PageTitle: string | undefined;
    @Input() Context: string | undefined;
    public ActivePanel: string = "";
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
        this.ItemListService.SaveItems(new ItemList(), new Criteria());
        this.ReviewSetsService.Clear();
        this.workAllocationContactListService.workAllocations = [];
        //this.workAllocationContactListService.Save();
    }
    Logout() {
        this.Clear();
        this.router.navigate(['home']);
    }
    ShowHideFeedback() {
        if (this.ActivePanel == "Feedback") {
            this.ActivePanel = "";
        }
        else {
            this.ActivePanel = "Feedback";
        }
    }
    ShowHideHelp() {
        if (this.ActivePanel == "Help"
            || !this.Context
            || this.Context == '') {
            this.ActivePanel = "";
        }
        else if (this.Context && this.Context !== '') {
            this.ActivePanel = "Help";
            this.OnlineHelpService.FetchHelpContent(this.Context);
        }
    }

    ngOnInit() {
    }
}
