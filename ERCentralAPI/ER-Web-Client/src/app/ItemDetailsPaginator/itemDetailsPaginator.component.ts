import { Component, OnInit, EventEmitter, Output, OnDestroy, Input, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Item } from '../services/ItemList.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

@Component({
  selector: 'itemDetailsPaginator',
  templateUrl: './itemDetailsPaginator.component.html',
  providers: [],
  styles: [`
                button.disabled {
                    color:black; 
                    }
            `]
})

export class itemDetailsPaginatorComp implements OnInit, OnDestroy, AfterViewInit {

  constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
    public ItemListService: ItemListService
    , private ItemCodingService: ItemCodingService,
    private reviewInfoService: ReviewInfoService,
    public PriorityScreeningService: PriorityScreeningService,
    private ConfirmationDialogService: ConfirmationDialogService,
    public ItemDocsService: ItemDocsService
  ) { }

  //public item?: Item;
  //public itemId = new Subject<number>();
  public get itemID(): number {
    if (this.item) return this.item.itemId;
    else return -1;
  }
  public get IsListFromSearch(): boolean {
    return this.PriorityScreeningService.UsingListFromSearch;
  }

  private subGotScreeningItem: Subscription | null = null;
  @Output() ItemChanged = new EventEmitter();
  @Output() GoToNextScreeningItemClicked = new EventEmitter();
  @Input() IsScreening: boolean = false;
  @Input() item: Item | undefined;
  @Input() Context: string = "CodingFull";
  @Input() HasOutcomeUnsavedChanges: boolean = false;
  private OutcomeDiscardChangesTitle = "Discard outcome changes?"
  private OutcomeDiscardChangesContent = "You have unsaved outcome changes. Changing item will <strong>discard</strong> these changes.<br /><strong>Continue?</strong>"

  ngOnInit() {
    //if (this.item) this.itemID = this.item.itemId;
  }


  ngAfterViewInit() {
    // child is set
  }


  public CanGoToPreviousScreening(): boolean {

    if (this.PriorityScreeningService.CurrentItemIndex > 0) {
      if (this.PriorityScreeningService.CurrentItemIndex == this.PriorityScreeningService.ScreenedItemIds.length - 1) return true;//last item
      else {
        //we're inside the queue, and thus we're not allowed to leave this item if it doesn't have a code from the screening tool
        return this.CanMoveToNextInScreening();
      }
    }
    return false;
  }

  public get PreviouslyScreenedCount(): number {
    let res: number = 0;


    //if we can't go to next, it's because the current item isn't screened, so we shouldn't count it.
    //but when we are already in the last items of the list of "seen items", then this is accounted for by LastItemInTheQueueIsDone already
    //instead, when we're somewhere inside the queue, we need to account for both things!
    let WeAreInTheLastItem = this.PriorityScreeningService.ScreenedItemIds[this.PriorityScreeningService.ScreenedItemIds.length - 1] == this.PriorityScreeningService.CurrentItem.itemId;

    //console.log("PreviouslyScreenedCount", this.PriorityScreeningService.ScreenedItemIds
    //  , this.PriorityScreeningService.LastItemInTheQueueIsDone
    //  , WeAreInTheLastItem
    //  , this.PriorityScreeningService.CurrentItem.itemId);

    if (this.PriorityScreeningService.LastItemInTheQueueIsDone) {
      
      res = this.PriorityScreeningService.ScreenedItemIds.length;
      if (!WeAreInTheLastItem && !this.CanMoveToNextInScreening()) res--;
    }
    else {
      res = this.PriorityScreeningService.ScreenedItemIds.length - 1;
      if (!WeAreInTheLastItem && !this.CanMoveToNextInScreening()) res--;
    }
    
    return res;
  }

  public CanMoveToNextInScreening(): boolean {

    let ItemSetIndex = this.ItemCodingService.ItemCodingList.findIndex(cset =>
      cset.setId == this.reviewInfoService.ReviewInfo.screeningCodeSetId
      && (cset.contactId == this.ReviewerIdentityServ.reviewerIdentity.userId || cset.isCompleted));
    if (ItemSetIndex == -1) return false;
    else return true;
  }
  public prevScreeningItem() {
    if (this.HasOutcomeUnsavedChanges) {
      this.ConfirmationDialogService.confirm(this.OutcomeDiscardChangesTitle, this.OutcomeDiscardChangesContent, false, '')
        .then((confirm: any) => {
          if (confirm) {
            this.InnerprevScreeningItem();
          }
        });
    } else this.InnerprevScreeningItem();
  }
  private InnerprevScreeningItem() {
    //this.WipeHighlights();
    if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
    this.IsScreening = true;
    this.ItemDocsService.Clear();
    this.PriorityScreeningService.PreviousItem();
  }

  public GotScreeningItem() {
    //this.item = this.PriorityScreeningService.CurrentItem;
    //this.itemID = this.item.itemId;
    //this.GetItemCoding();
  }
  public GetScreeningItem() {
    if (this.HasOutcomeUnsavedChanges) {
      this.ConfirmationDialogService.confirm(this.OutcomeDiscardChangesTitle, this.OutcomeDiscardChangesContent, false, '')
        .then((confirm: any) => {
          if (confirm) {
            this.innerGetScreeningItem();
          }
        });
    } else this.innerGetScreeningItem();
  }
  private innerGetScreeningItem() {
    //if (this.PriorityScreeningService.CheckForRaicWork(this.ItemCodingService.ItemCodingList) && this.item) {
    //  this.PriorityScreeningService.RaicFindAndDoWorkFromSimulateNextItem(this.item.itemId);
    //}
    this.GoToNextScreeningItemClicked.emit();
  }
  hasPrevious(): boolean {

    return this.ItemListService.hasPrevious(this.itemID);
  }
  public get MyHumanIndex(): number {
    //console.log('MyHumanIndex called', this.itemID);
    if (this.ItemListService.ItemList.items.findIndex(found => found.itemId == this.itemID) == -1) {
      return 1;
    } else {
      return this.ItemListService.ItemList.items.findIndex(found => found.itemId == this.itemID) + 1;
    }
  }
  private _hasNext: boolean | null = null;
  hasNext(): boolean {
    return this.ItemListService.hasNext(this.itemID);
  }
  firstItem() {
    if (this.item) {
      if (this.HasOutcomeUnsavedChanges) {
        this.ConfirmationDialogService.confirm(this.OutcomeDiscardChangesTitle, this.OutcomeDiscardChangesContent, false, '')
          .then((confirm: any) => {
            if (confirm) {
              this.goToItem(this.ItemListService.getFirst());
            }
          });
      } else this.goToItem(this.ItemListService.getFirst());
    }
  }
  prevItem() {
    if (this.item) {
      const iid = this.item.itemId;
      if (this.HasOutcomeUnsavedChanges) {
        this.ConfirmationDialogService.confirm(this.OutcomeDiscardChangesTitle, this.OutcomeDiscardChangesContent, false, '')
          .then((confirm: any) => {
            if (confirm) {
              this.goToItem(this.ItemListService.getPrevious(iid));
            }
          });
      } else this.goToItem(this.ItemListService.getPrevious(iid));
    }
  }
  nextItem() {
    if (this.item) {
      const iid = this.item.itemId;
      if (this.HasOutcomeUnsavedChanges) {
        this.ConfirmationDialogService.confirm(this.OutcomeDiscardChangesTitle, this.OutcomeDiscardChangesContent, false, '')
          .then((confirm: any) => {
            if (confirm) {
              this.goToItem(this.ItemListService.getNext(iid));
            }
          });
      } else this.goToItem(this.ItemListService.getNext(iid));
    }
  }
  lastItem() {
    if (this.item) {
      if (this.HasOutcomeUnsavedChanges) {
        this.ConfirmationDialogService.confirm(this.OutcomeDiscardChangesTitle, this.OutcomeDiscardChangesContent, false, '')
          .then((confirm: any) => {
            if (confirm) {
              this.goToItem(this.ItemListService.getLast());
            }
          });
      } else this.goToItem(this.ItemListService.getLast());
    }
  }

  lastPage() {

    this.ItemListService.FetchLastPage();
  }

  goToItem(item: Item) {
    if (this.PriorityScreeningService.ShouldCheckForRaicWork(this.ItemCodingService.ItemCodingList) && this.item) {
      this.PriorityScreeningService.RaicFindAndDoWorkFromUITrigger(this.item.itemId);
    }
    //this.WipeHighlights();
    if (this.Context == 'FullUI') this.router.navigate(['itemcoding', item.itemId]);
    else if (this.Context == 'CodingOnly') this.router.navigate(['itemcodingOnly', item.itemId]);
    this.item = item;
    //if (this.item.itemId != this.itemID) {

    //	this.itemID = this.item.itemId;
    //      }

    this.ItemChanged.emit();
    //this.GetItemCoding();
  }
  BackToMain() {
    this.router.navigate(['Main']);
  }

  ngOnDestroy() {
    //console.log('killing coding comp');

  }
}

