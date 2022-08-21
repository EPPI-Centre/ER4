import { AfterContentChecked, ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Subscription } from 'rxjs';
import { ZoteroHeaderBarComp } from '../commonComponents/ZoteroHeaderBar.component';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ZoteroService } from '../services/Zotero.service';
import { Group, ZoteroReviewCollectionList } from '../services/ZoteroClasses.service';

@Component({
    selector: 'ZoteroManager',
    templateUrl: './ZoteroManager.component.html',
    providers: []
})

export class ZoteroManagerComponent implements OnInit, AfterContentChecked {


    constructor(
        private route: ActivatedRoute,
        private _notificationService: NotificationService,
        private _zoteroService: ZoteroService,
        private _router: Router,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private cdr: ChangeDetectorRef
    ) {
        
    }
    ngAfterContentChecked(): void {
        this.cdr.detectChanges();
    }

    @ViewChild('NavBarZotero') NavBarZotero!: ZoteroHeaderBarComp;
    private error: any;
    private errorInPath: Subscription | null = null;
    public get HelpAndFeebackContext(): string {
        switch (this.NavBarZotero.Context) {
            case "ZoteroSetup":
                return "zotero\\zoteroSetup";
            case "ZoteroSync":
                return "zotero\\zoteroSync";
            default:
                return "zotero\\zotero";
        }
    }

    public get Context(): string {
        if (this.NavBarZotero) {
            return this.NavBarZotero.Context;
        }
        else return "Zotero: unspecified page";
    }
    public ChangeContext(val: any) {
        if (this.NavBarZotero) {
            this.NavBarZotero.Context = val;
        }
    }

    public GetCurrentGroup() : number {
        return this._zoteroService.currentGroupBeingSynced;
    }

    public get IsServiceBusy(): boolean {

        return this._zoteroService.IsBusy;
    }
    private currentReview: number = 0;
    private zoteroUserID: number = 0;
    private zoteroUserName: string = '';
    public isCollapsed = false;
    public zoteroLink: string = '';
    public reviewLinkText: string[] = [];
    public currentLinkedReviewId: string = '';
    public zoteroCollectionList: ZoteroReviewCollectionList = new ZoteroReviewCollectionList();
    public DetailsForSetId: number = 0;
    public ZoteroApiKeyResult: boolean = false;

    ngOnDestroy() {
        if (this.errorInPath) {
            this.errorInPath.unsubscribe();
        }
    }

    ngOnInit() {

        this.errorInPath = this.route.params.subscribe(async params => {
            this.error = params['error'];
            if (this.error === 'nogroups') {
                var contentError: string = 'You have either no groups created in Zotero as requested, or you have not selected read/write permissions';
                this._notificationService.show({
                    content: contentError,
                    animation: { type: 'slide', duration: 400 },
                    position: { horizontal: 'center', vertical: 'top' },
                    type: { style: "error", icon: true },
                    closable: true
                });
                this._router.navigate(['Main']);

            } else if (this.error === 'unathorised') {
                var contentError: string = 'Zotero sometimes fails with unauthorised, please try up to three attempts to resolve';
                this._notificationService.show({
                    content: contentError,
                    animation: { type: 'slide', duration: 400 },
                    position: { horizontal: 'center', vertical: 'top' },
                    type: { style: "error", icon: true },
                    closable: true
                });
                this._router.navigate(['Zotero']);
            }
            else {
                if (this._ReviewerIdentityServ.reviewerIdentity.userId == 0 ||
                    this._ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
                    this._router.navigate(['home']);
                }
                else if (!this._ReviewerIdentityServ.HasWriteRights) {
                    this._router.navigate(['Main']);
                }
                this.zoteroUserName = this._ReviewerIdentityServ.reviewerIdentity.name;
                this.zoteroUserID = this._ReviewerIdentityServ.reviewerIdentity.userId;
                this.currentLinkedReviewId = this._ReviewerIdentityServ.reviewerIdentity.reviewId.toString();
                this.currentReview = this._ReviewerIdentityServ.reviewerIdentity.reviewId;

                await this._zoteroService.CheckZoteroApiKey().then(
                  async (zoteroApiKeyResult) => {
                              console.log('result is: ', zoteroApiKeyResult);
                    if (zoteroApiKeyResult === true) {

                                console.log('got here 1');
                      this.ZoteroApiKeyResult = zoteroApiKeyResult;


                                await this._zoteroService.fetchGroupMetaData().then(
                                        async (groups: Group[]) => {

                                            console.log('why is this zero?: ' + JSON.stringify(groups));
                                            for (var i = 0; i < groups.length; i++) {
                                                var group = groups[i];
                                                if (group.groupBeingSynced > 0) {
                                                    this._zoteroService.currentGroupBeingSynced = group.groupBeingSynced;
                                                    this._zoteroService.editApiKeyPermissions = true;
                                                }
                                            }
                                        });

                                // ALREADY HAS A ZOTERO API KEY
                                await this.FetchLinkedReviewID();

                                if (this._zoteroService.editApiKeyPermissions) {
                                    // ALREADY HAS A SHARED GROUP WITH  PERMISSIONS ASSOCIATED WITH THIS API KEY
                                    //this.zoteroEditKeyLink = 'https://www.zotero.org/oauth/authorize?oauth_token=' + zoteroApiKey;
                                    this.ChangeContext("ZoteroSync");

                                } else {
                                    this.ChangeContext("ZoteroSetup");
                                }
                            } else {
                              console.log('No active Zotero API Key');
                            }
                        });               
            }
        });
    }

    public BackHome() {
        this._router.navigate(['Main']);
    }

    public async FetchLinkedReviewID(): Promise<void> {
        await this._zoteroService.FetchGroupToReviewLinks().then(
                async (zoteroReviewCollectionList: ZoteroReviewCollectionList) => {
                    this.zoteroCollectionList = zoteroReviewCollectionList;
                    if (zoteroReviewCollectionList.ZoteroReviewCollectionList.length > 0) {
                        this.currentLinkedReviewId = zoteroReviewCollectionList.ZoteroReviewCollectionList[0].revieW_ID.toString();
                    }
                });
    }
}
