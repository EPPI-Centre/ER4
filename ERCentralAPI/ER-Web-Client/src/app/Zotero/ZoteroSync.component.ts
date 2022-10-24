import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { NotificationService } from '@progress/kendo-angular-notification';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { CodesetStatisticsService, StatsCompletion } from '../services/codesetstatistics.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSet, ReviewSetsService, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { ZoteroService } from '../services/Zotero.service';
import { SyncState, ZoteroItem, ZoteroERWebReviewItem} from '../services/ZoteroClasses.service';

@Component({
  selector: 'ZoteroSync',
  templateUrl: './ZoteroSync.component.html',
  providers: []
})

export class ZoteroSyncComponent implements OnInit, OnDestroy {
  constructor(
    private _notificationService: NotificationService,
    public _zoteroService: ZoteroService,
    private _confirmationDialogService: ConfirmationDialogService,
    private _ReviewerIdentityServ: ReviewerIdentityService,
    private _codesetStatsServ: CodesetStatisticsService,
    private _reviewSetsService: ReviewSetsService
  ) {

  }

  @ViewChild('WithOrWithoutCodeSelector') WithOrWithoutCodeSelector!: codesetSelectorComponent;

  public CurrentDropdownSelectedCode: singleNode | null = null;
  public isCollapsed = false;

  public dropdownBasic1: boolean = false;


  ngOnInit() {
    if (this._reviewSetsService.ReviewSets.length == 0) this._reviewSetsService.GetReviewSets(false);
    this.fetchZoteroObjectVersions();
  }
  public get HasWriteRights(): boolean {
    return this._ReviewerIdentityServ.HasWriteRights;
  }

  //public get HasAdminRights(): boolean {
  //  return this._ReviewerIdentityServ.HasAdminRights;
  //}

  public get ObjectZoteroList(): ZoteroItem[] {
    return this._zoteroService.ZoteroItems;
  }

  public get ZoteroERWebReviewItemList(): ZoteroERWebReviewItem[] {
    return this._zoteroService.ZoteroERWebReviewItemList;
  }

  public get ItemsToPushCount(): number {
    return this._zoteroService.ZoteroERWebReviewItemList.filter(f => f.syncState == SyncState.canPush || f.HasPdfToPush).length;
  }
  public get ItemsToPullCount(): number {
    return this.ObjectZoteroList.filter(f => f.syncState == SyncState.canPull || f.HasAttachmentsToPull).length;
  }

  //public get NameOfCurrentLibrary() {
  //  return this._zoteroService.NameOfCurrentLibrary;
  //}

  public get IsServiceBusy(): boolean {
    return this._zoteroService.IsBusy || this._codesetStatsServ.IsBusy;
  }

  public CloseCodeDropDown() {
    if (this.WithOrWithoutCodeSelector !== null) {
      this.CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
      this.isCollapsed = false;
    }
  }

  public getErWebObjects() {
    if (!this._zoteroService.hasPermissions) {
            this._notificationService.show({
                content: "You must select a group on the setup page in order to check the current sync status",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "info", icon: true },
                closable: true
            });
            return;
    } else {
        this.Clear();
      let CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
      if (CurrentDropdownSelectedCode !== null) {
        this._zoteroService.fetchZoteroERWebReviewItemListAsync(CurrentDropdownSelectedCode.attribute_id.toString());
      }   
    }
  }

  fetchZoteroObjectVersions() {
    this._zoteroService.fetchZoteroObjectVersionsAsync();
  }

  async PullConfirmZoteroItems(): Promise<void> {
    if (this.ItemsToPullCount < 1 || this.HasWriteRights == false) return;
    this._confirmationDialogService.confirm("Pull Items from Zotero?",
      "When Pulling, any new items will be imported as a new source. Existing items will be updated.<br />"
      + "You are about to pull " + this.ItemsToPullCount + " items.", false, ''
    ).then((confirmed: any) => {
      if (confirmed) this.PullZoteroItems();
    });
  }

  async PullZoteroItems() {
    let toPull = this._zoteroService.ZoteroItems.filter(f => (f.syncState == SyncState.canPull || f.HasAttachmentsToPull));
    let pulling: ZoteroERWebReviewItem[] = [];
    for (let tp of toPull) {
      let zERi = tp.ToZoteroERWebReviewItem();
      //we need to remove all PDFs that do NOT need to be pulled
      const Atts = zERi.pdfList;
      zERi.pdfList = [];
      for (let p of Atts) {
        if (p.syncState == SyncState.canPull) zERi.pdfList.push(p);
      }
      pulling.push(zERi);
    }
    const res1 = await this._zoteroService.PullTheseItems(pulling);
    if (res1 == true) {
      this.RefreshBothTables();
    }
  }
  public async RefreshBothTables() {
    const res2 = await this._zoteroService.fetchZoteroObjectVersionsAsync();
    if (res2 == true) {
      let CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
      if (CurrentDropdownSelectedCode !== null) {
        this._zoteroService.fetchZoteroERWebReviewItemListAsync(CurrentDropdownSelectedCode.attribute_id.toString());
      }
    }
  }
  async PushERWebItems() {
    const res1 = await this._zoteroService.PushZoteroErWebReviewItemList();
    if (res1 == true) {
      this.RefreshBothTables();
    }
  }
  ngOnDestroy() {    
  }

  Clear() {
  }

}
