import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Subscription } from 'rxjs';
import { ZoteroHeaderBarComp } from '../commonComponents/ZoteroHeaderBar.component';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ZoteroService } from '../services/Zotero.service';
import { Group, ZoteroReviewCollectionList } from '../services/ZoteroClasses.service';

@Component({
    selector: 'ZoteroManager',
    templateUrl: './ZoteroManager.component.html',
    providers: []
})

export class ZoteroManagerComponent implements AfterViewInit, OnDestroy {


    constructor(
        private route: ActivatedRoute,
        private _notificationService: NotificationService,
        private _zoteroService: ZoteroService,
        private _router: Router,
        private _ReviewerIdentityServ: ReviewerIdentityService
    ) {
        
    }
  ngAfterViewInit() { //ngOnInit() {
    if (this._ReviewerIdentityServ.reviewerIdentity.userId == 0 ||
      this._ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
      this._router.navigate(['home']);
    }
    else {
      this.errorInPathSub = this.route.queryParams.subscribe(async params => {
        const err: string = params['error'];
        if (err && err.length > 0) {
          //we received an error in the querystring coming back from the oAuth loop,
          //we put this error in the service and go to the setup page, where "what to do" logic will make the "setup" decisions
          this._zoteroService.SetError(err);
          this.ChangeContext("ZoteroSetup");
        }
        else {

          await this._zoteroService.CheckZoteroPermissions().then(
            () => {

              if (this._zoteroService.hasPermissions) {
                // ALREADY HAS A SHARED GROUP WITH  PERMISSIONS ASSOCIATED WITH THIS API KEY
                this.ChangeContext("ZoteroSync");

              } else {
                this.ChangeContext("ZoteroSetup");
              }
            });
        }
        //if (err === 'nogroups') {
        //  var contentError: string = 'You have either no groups created in Zotero as requested, or you have not selected read/write permissions';
        //  this._notificationService.show({
        //    content: contentError,
        //    animation: { type: 'slide', duration: 400 },
        //    position: { horizontal: 'center', vertical: 'top' },
        //    type: { style: "error", icon: true },
        //    closable: true
        //  });
        //  this._zoteroService.SetError(err);
        //   this.ChangeContext("ZoteroSetup");
        //}
        //else if (err === 'unauthorised') {
        //  var contentError: string = 'Zotero sometimes fails with unauthorised, please try up to three attempts to resolve';
        //  this._notificationService.show({
        //    content: contentError,
        //    animation: { type: 'slide', duration: 400 },
        //    position: { horizontal: 'center', vertical: 'top' },
        //    type: { style: "error", icon: true },
        //    closable: true
        //  });
        //  this._zoteroService.SetError(err);
        //  this.ChangeContext("ZoteroSetup");
        //}
      });
    }
  }

    @ViewChild('NavBarZotero') NavBarZotero!: ZoteroHeaderBarComp;
    private errorInPathSub: Subscription | null = null;
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
        else return "Loading...";
    }
  public ChangeContext(val: any) {
    //console.log("ChangeContext", val);
      if (this.NavBarZotero) {
        setTimeout(() => { this.NavBarZotero.Context = val; }, 20);//we do it "out of thread" with a little delay, to avoid the "value changed after checking" error)
      }
      else {
        //when this.NavBarZotero doesn't exist, it's normally because we're still initialising.
        setTimeout(() => {
          if (this.NavBarZotero) {
            this.NavBarZotero.Context = val;
          }
        }, 800);//wait almost one second before trying again...
      }
    }

    public GetCurrentGroup() : number {
        return this._zoteroService.currentGroupBeingSynced;
    }
    public isCollapsed = false;
    public zoteroLink: string = '';
    public reviewLinkText: string[] = [];
    public currentLinkedReviewId: string = '';
    public zoteroCollectionList: ZoteroReviewCollectionList = new ZoteroReviewCollectionList();
    public DetailsForSetId: number = 0;

    ngOnDestroy() {
      if (this.errorInPathSub) {
        this.errorInPathSub.unsubscribe();
      }
      this._zoteroService.Clear();
    }
    public BackHome() {
        this._router.navigate(['Main']);
    }
}
