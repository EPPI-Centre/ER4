import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { NotificationService } from '@progress/kendo-angular-notification';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { CustomSorting, LocalSort } from '../helpers/CustomSorting';
import { CodesetStatisticsService, StatsCompletion } from '../services/codesetstatistics.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSet, ReviewSetsService, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { ZoteroService } from '../services/Zotero.service';
import { SyncState, ZoteroItem, ZoteroERWebReviewItem} from '../services/ZoteroClasses.service';

@Component({
  selector: 'ZoteroSync',
  templateUrl: './ZoteroSync.component.html',
  providers: [],
  styles: []
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
    this._zoteroService.CheckAndFetchZoteroItems().then(() => { const t = this.TotPages2; });
  }
  public get HasWriteRights(): boolean {
    return this._ReviewerIdentityServ.HasWriteRights;
  }

  public get BusyMessage(): string {
    return this._zoteroService.BusyMessage;
  }

  public get ZoteroItems(): ZoteroItem[] {
    return this._zoteroService.ZoteroItems;
  }

  public get ZoteroERWebReviewItemList(): ZoteroERWebReviewItem[] {
    return this._zoteroService.ZoteroERWebReviewItemList;
  }

  public get ItemsToPushCount(): number {
    return this._zoteroService.ZoteroERWebReviewItemList.filter(f => f.syncState == SyncState.canPush || f.HasPdfToPush).length;
  }
  public get ItemsToPullCount(): number {
    return this.ZoteroItems.filter(f => f.syncState == SyncState.canPull || f.HasAttachmentsToPull).length;
  }

  public get NameOfCurrentLibrary() {
    return this._zoteroService.NameOfCurrentLibrary;
  }

  public get IsServiceBusy(): boolean {
    return this._zoteroService.IsBusy || this._codesetStatsServ.IsBusy;
  }


  public showItemKeys: boolean = false;
  public showItemTitles: boolean = false;
  public showItemShortTitles: boolean = true;
  public showZoteroTitles: boolean = false;
  public showZoteroShortTitles: boolean = true;

  public showRebuildExplanation: boolean = false;

  private _TotPages1: number = -1;
  private _TotPages2: number = -1;
  private _PageSize1: number = 100;
  private _PageSize2: number = 100;
  public get PageSize1(): number {
    return this._PageSize1;
  }
  public get PageSize2(): number {
    return this._PageSize2;
  }
  public set PageSize1(n: number) {
    this._PageSize1 = n;
    this._TotPages1 = -1;
    if (this.CurrentPage1 > this.TotPages1) this.CurrentPage1 = this.TotPages1;
  }
  public set PageSize2(n: number) {
    this._PageSize2 = n;
    this._TotPages2 = -1;
    if (this.CurrentPage2 > this.TotPages2) this.CurrentPage2 = this.TotPages2;
  }
  public get TotPages1(): number {
    if (this._TotPages1 < 1) {
      if (this.FilteredList1.length == 0) this._TotPages1 = 0;
      else this._TotPages1 = Math.ceil(this.FilteredList1.length / this.PageSize1);
    }
    return this._TotPages1;
  }
  public get TotPages2(): number {
    if (this._TotPages2 < 1) {
      if (this.FilteredList2.length == 0) this._TotPages2 = 0;
      else this._TotPages2 = Math.ceil(this.FilteredList2.length / this.PageSize2);
    }
    return this._TotPages2;
  }
  public CurrentPage1: number = 1;
  public CurrentPage2: number = 1;
  public PagingDD1: number[] = [
    20, 50, 100, 500, 1000, 2000, 5000
  ];
  public PagingDD2: number[] = [
    20, 50, 100, 500, 1000, 2000, 5000
  ];

  public get PagedList1(): ZoteroERWebReviewItem[] {
    const res = this.FilteredList1;
    if (res.length <= this.PageSize1) return res;
    if (this.CurrentPage1 > this.TotPages1) this.CurrentPage1 = this.TotPages1;
    const StartIndex: number = (this.CurrentPage1 - 1) * this.PageSize1;
    if (this.CurrentPage1 !== this.TotPages1) {
      return res.slice(StartIndex, StartIndex + this.PageSize1);
    } else {
      return res.slice(StartIndex);
    }
  }

  public get PagedList2(): ZoteroItem[] {
    const res = this.FilteredList2;
    if (res.length <= this.PageSize2) return res;
    if (this.CurrentPage2 > this.TotPages2) this.CurrentPage2 = this.TotPages2;
    const StartIndex: number = (this.CurrentPage2 - 1) * this.PageSize2;
    if (this.CurrentPage2 !== this.TotPages2) {
      return res.slice(StartIndex, StartIndex + this.PageSize2);
    } else {
      return res.slice(StartIndex);
    }
  }

  public get FilteredList1(): ZoteroERWebReviewItem[] {
    let res: ZoteroERWebReviewItem[] = this._zoteroService.ZoteroERWebReviewItemList;
    switch (this.ActiveFilter1) {
      case "No filter":
        break;
      case "Can Push":
        res = res.filter(f => f.syncState == SyncState.canPush);
        break;
      case "Up To Date":
        res = res.filter(f => f.syncState == SyncState.upToDate);
        break;
      case "With docs":
        res = res.filter(f => f.HasPdf == true);
        break;
      case "Without docs":
        res = res.filter(f => f.HasPdf == false);
        break;
      case "With docs to push":
        res = res.filter(f => f.HasPdfToPush == true);
        break;
    }
    return res;
  }
  public get FilteredList2(): ZoteroItem[] {
    let res: ZoteroItem[] = this._zoteroService.ZoteroItems;
    switch (this.ActiveFilter2) {
      case "No filter":
        break;
      case "Can Pull":
        res = res.filter(f => f.syncState == SyncState.canPull);
        break;
      case "Up To Date":
        res = res.filter(f => f.syncState == SyncState.upToDate);
        break;
      case "With docs":
        res = res.filter(f => f.HasAttachments == true);
        break;
      case "Without docs":
        res = res.filter(f => f.HasAttachments == false);
        break;
      case "With docs to pull":
        res = res.filter(f => f.HasAttachmentsToPull == true);
        break;
    }
    return res;
  }

  public get SelectedList1(): ZoteroERWebReviewItem[] {
    let res: ZoteroERWebReviewItem[] = this._zoteroService.ZoteroERWebReviewItemList.filter(
      f => f.ClientSelected == true && (f.syncState == SyncState.canPush || f.HasPdfToPush)
    );
    return res;
  }
  public get SelectedList2(): ZoteroItem[] {
    let res: ZoteroItem[] = this._zoteroService.ZoteroItems.filter(
      f => f.ClientSelected == true && (f.syncState == SyncState.canPull || f.HasAttachmentsToPull)
    );
    return res;
  }

  public FirstPage1() {
    this.CurrentPage1 = 1;
  }
  public FirstPage2() {
    this.CurrentPage2 = 1;
  }
  public PageDown1() {
    if (this.CurrentPage1 > 1) this.CurrentPage1--;
  }
  public PageDown2() {
    if (this.CurrentPage2 > 1) this.CurrentPage2--;
  }
  public PageUp1() {
    if (this.CurrentPage1 < this.TotPages1) this.CurrentPage1++;
  }
  public PageUp2() {
    if (this.CurrentPage2 < this.TotPages2) this.CurrentPage2++;
  }
  public LastPage1() {
    this.CurrentPage1 = this.TotPages1;
  }
  public LastPage2() {
    this.CurrentPage2 = this.TotPages2;
  }
  public get CanPageUp1(): boolean {
    //console.log("can pg up", this.CurrentPage1, this.TotPages1)
    return this.CurrentPage1 < this.TotPages1;
  }
  public get CanPageUp2(): boolean {
    return this.CurrentPage2 < this.TotPages2;
  }
  public get CanPageDown1(): boolean {
    //console.log("can pg dw", this.CurrentPage1, this.TotPages1)
    return this.CurrentPage1 > 1;
  }
  public get CanPageDown2(): boolean {
    return this.CurrentPage2 > 1;
  }
  private LocalSort1: LocalSort = new LocalSort();
  private LocalSort2: LocalSort = new LocalSort();
  public SortBy1(field: string) {
    this._zoteroService.ZoteroERWebReviewItemList = CustomSorting.SortBy(field, this._zoteroService.ZoteroERWebReviewItemList, this.LocalSort1);
  }
  public SortBy2(field: string) {
    this._zoteroService.ZoteroItems = CustomSorting.SortBy(field, this._zoteroService.ZoteroItems, this.LocalSort2);
  }
  public SortingSymbol1(fieldName: string): string {
    return CustomSorting.SortingSymbol(fieldName, this.LocalSort1);
  }
  public SortingSymbol2(fieldName: string): string {
    return CustomSorting.SortingSymbol(fieldName, this.LocalSort2);
  }

  public ShowMore1: boolean = false;
  public ShowMore2: boolean = false;
  public get ShowMore1txt(): string {
    if (this.ShowMore1) return "Less...";
    else return "More...";
  }
  public get ShowMore2txt(): string {
    if (this.ShowMore2) return "Less...";
    else return "More...";
  }
  public Filter1DD: string[] = [
    "No filter",
    "Can Push",
    "Up To Date",
    "With docs",
    "Without docs",
    "With docs to push"
  ];
  public Filter2DD: string[] = [
    "No filter",
    "Can Pull",
    "Up To Date",
    "With docs",
    "Without docs",
    "With docs to pull"
  ];

  private _ActiveFilter1: string = "No filter";
  private _ActiveFilter2: string = "No filter";

  public get ActiveFilter1(): string {
    return this._ActiveFilter1;
  }
  public set ActiveFilter1(val: string) {
    this._ActiveFilter1 = val;
    this._TotPages1 = -1;
  }

  public get ActiveFilter2(): string {
    return this._ActiveFilter2;
  }
  public set ActiveFilter2(val: string) {
    this._ActiveFilter2 = val;
    this._TotPages2 = -1;
  }

  public get HasSelections1(): boolean {
    return this._zoteroService.ZoteroERWebReviewItemList.findIndex(f => f.ClientSelected == true) != -1;
  }
  public get HasSelections2(): boolean {
    return this._zoteroService.ZoteroItems.findIndex(f => f.ClientSelected == true) != -1;
  }
  public get HasSelectionsDetail1(): number {
    const selectedCount = this._zoteroService.ZoteroERWebReviewItemList.filter(f => f.ClientSelected == true).length;
    if (selectedCount == 0) return 0;
    const selectableCount = this._zoteroService.ZoteroERWebReviewItemList.filter(f => f.syncState == SyncState.canPush || f.HasPdfToPush).length;
    if (selectedCount != selectableCount) return 1; //partial selection
    else return 2;
  }
  public get HasSelectionsDetail2(): number {
    const selectedCount = this._zoteroService.ZoteroItems.filter(f => f.ClientSelected == true).length;
    if (selectedCount == 0) return 0;
    const selectableCount = this._zoteroService.ZoteroItems.filter(f => f.syncState == SyncState.canPull || f.HasAttachmentsToPull).length;
    if (selectedCount != selectableCount) return 1; //partial selection
    else return 2;
  }

  public SelectAll1() {
    const list = this.FilteredList1.filter(f => f.syncState == SyncState.canPush || f.HasPdfToPush);
    for (let itm of list) {
      itm.ClientSelected = true;
    }
  }
  public SelectAll2() {
    const list = this.FilteredList2.filter(f => f.syncState == SyncState.canPull || f.HasAttachmentsToPull);
    for (let itm of list) {
      itm.ClientSelected = true;
    }
  }

  public UnSelectAll1() {
    const list = this._zoteroService.ZoteroERWebReviewItemList;
    for (let itm of list) {
      itm.ClientSelected = false;
    }
  }
  public UnSelectAll2() {
    const list = this._zoteroService.ZoteroItems;
    for (let itm of list) {
      itm.ClientSelected = false;
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
      this._TotPages1 = -1;
      this.CurrentPage1 = 1;
      this.Clear();
      let CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
      if (CurrentDropdownSelectedCode !== null) {
        this._zoteroService.fetchZoteroERWebReviewItemListAsync(CurrentDropdownSelectedCode.attribute_id.toString(), this.LocalSort1);
      }   
    }
  }

  public CloseCodeDropDown() {
    if (this.WithOrWithoutCodeSelector !== null) {
      this.CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
      this.isCollapsed = false;
      this.getErWebObjects();//we directly get sync data for "Items with this code".
    }
  }

  async PullConfirmZoteroItems(): Promise<void> {
    if (this.ItemsToPullCount < 1 || this.HasWriteRights == false) return;
    this._confirmationDialogService.confirm("Pull Items from Zotero?",
      "When Pulling, any new items will be imported as a new source. Existing items will be updated.<br />"
      + "You are about to pull " + (this.HasSelections2 ? this.SelectedList2.length : this.ItemsToPullCount) + " items.", false, ''
    ).then((confirmed: any) => {
      if (confirmed) this.PullZoteroItems();
    });
  }

  async PullZoteroItems() {
    let toPull: ZoteroItem[] = [];
    if (this.HasSelections2) {
      toPull = this.SelectedList2;
    }
    else {
      toPull = this._zoteroService.ZoteroItems.filter(f => (f.syncState == SyncState.canPull || f.HasAttachmentsToPull));
    }
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
    if (pulling.length > 0) {
      const res1 = await this._zoteroService.PullTheseItems(pulling);
      if (res1 == true) {
        this.RefreshBothTables();
      } else {
        this.RefreshBothTables();
      }
    }
  }

  public async RefreshBothTables() {
    this._TotPages2 = -1;
    const res2 = await this._zoteroService.CheckAndFetchZoteroItems(true);
    if (res2 == true) {
      this._zoteroService.ZoteroItems = CustomSorting.DoSort(this._zoteroService.ZoteroItems, this.LocalSort2);
      let CurrentDropdownSelectedCode = this.WithOrWithoutCodeSelector.SelectedNodeData as SetAttribute;
      if (CurrentDropdownSelectedCode !== null) {
        this._zoteroService.fetchZoteroERWebReviewItemListAsync(CurrentDropdownSelectedCode.attribute_id.toString(), this.LocalSort1);
      }
    }
  }
  async PushERWebItems() {
    const res1 = await this._zoteroService.PushZoteroErWebReviewItemList();
    if (res1 == true) {
      this.RefreshBothTables();
    } else {
      this.RefreshBothTables();
    }
  }

  async RebuildItemConnections() {
    const res1 = await this._zoteroService.RebuildItemConnections();
    if (res1 == true) {
      this.RefreshBothTables();
    } else {
      this.RefreshBothTables();
    }
  }

  ngOnDestroy() {    
  }

  Clear() {
  }

}
