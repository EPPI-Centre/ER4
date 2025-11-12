import { Component, OnInit, OnDestroy, ViewChild, AfterViewInit, HostListener } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute } from '../services/ItemCoding.service';
import { ReviewSetsService, ItemAttributeSaveCommand, SetAttribute } from '../services/ReviewSets.service';
import { CodesetTreeCodingComponent, CheckBoxClickedEventData } from '../CodesetTrees/codesetTreeCoding.component';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ArmTimepointLinkListService } from '../services/ArmTimepointLinkList.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { SelectEvent, TabStripComponent } from '@progress/kendo-angular-layout';
import { PdfTronContainer } from '../PDFTron/pdftroncontainer.component';
import { Helpers } from '../helpers/HelperMethods';
import { OutcomesComponent } from '../Outcomes/outcomes.component';
import { OutcomesService } from '../services/outcomes.service';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';


@Component({

  selector: 'itemcoding',
  templateUrl: './coding.component.html',
  //providers: [ReviewerIdentityService]
  providers: [],
  styles: [`
                button.disabled {
                    color:black; 
                    }

                .vertical-text {
                    position: fixed;
                    top: 50%;
                    z-index:2002;
                    transform: rotate(90deg);
                    left: -23px;
                    float: left;
                }
                .vertical-text-R {
                    position: fixed;
                    top: 50%;
                    z-index:2002;
                    transform: rotate(90deg);
                    right: -18px;
                    float: left;
                }
                .codesInSmallScreen {
                 position:absolute; 
                 left: 0; z-index:2000;
                  top: 106px;
                transition: transform 0.31s;
                transform-origin:left;
                }
                .codesInSmallScreen.hide {
                  transform:scaleX(0);
                }
                .codesInSmallScreen.show {
                  width:99.5%;
                  height:90%;
                  max-height:814px;
                  transform:0;
                }
                .CodesBtnL {
                    position: fixed;
                    top: 45%;
                    z-index:999;
                }
                .CodesBtnR {
                    position: fixed;
                    top: 45vh;
                    z-index:999;
                    right: 0.05em;
                }
                .CodesBtnContent {
                    writing-mode: vertical-lr;
                    font-size:0.9rem;
                }
                .CodesBtnContent span {
                    margin-left: -10px;
                    font-size:18px;
                }
               
            `]

})
export class ItemCodingComp implements OnInit, OnDestroy, AfterViewInit {

  constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
    public ItemListService: ItemListService
    , private route: ActivatedRoute, private ItemCodingService: ItemCodingService,
    private ReviewSetsService: ReviewSetsService,
    private reviewInfoService: ReviewInfoService,
    public PriorityScreeningService: PriorityScreeningService
    , private ReviewerTermsService: ReviewerTermsService,
    public ItemDocsService: ItemDocsService,
    private armservice: ArmTimepointLinkListService,
    private notificationService: NotificationService,
    private _outcomeService: OutcomesService
  ) { }
  //     .codesInSmallScreen.collapse{
  //    display: block!important;
  //    transition: all .25s ease -in -out;
  //}

  //                .codesInSmallScreen.collapse {
  //    opacity: 0;
  //    height: 0;
  //}

  //                .codesInSmallScreen.collapse.show {
  //    opacity: 1;
  //    height: 100 %;
  //}

  ngOnInit() {
    this.RefreshTerms();
    this.innerWidth = window.innerWidth;
    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
      this.router.navigate(['home']);
    }
    else {
      this.armservice.armChangedEE.subscribe(() => {
        if (this.armservice.SelectedArm) this.SetArmCoding(this.armservice.SelectedArm.itemArmId);
        else this.SetArmCoding(0);
      });
      this.subItemIDinPath = this.route.params.subscribe((params: any) => {
        this.itemString = params['itemId'];
        this.GetItem();
        console.log('coding kjkhjkhk: ' + this.itemID);
      });
      this.ItemCodingServiceDataChanged = this.ItemCodingService.DataChanged.subscribe(

        () => {
          if (this.ItemCodingService && this.ItemCodingService.ItemCodingList) {
            //console.log('data changed event caught');
            this.SetCoding();
          }
        }
      );
      this.subCodingCheckBoxClickedEvent = this.ReviewSetsService.ItemCodingCheckBoxClickedEvent.subscribe((data: CheckBoxClickedEventData) => this.ItemAttributeSave(data));
      this.subGotPDFforViewing = this.ItemDocsService.GotDocument.subscribe(() => this.CheckAndMoveToPDFTab());
    }
  }

  @ViewChild('OutcomesCmp')
  private OutcomesCmpRef!: OutcomesComponent;
  @ViewChild('cmp') private ArmsCompRef!: any;
  @ViewChild('pdftroncontainer') private pdftroncontainer!: PdfTronContainer;

  private subItemIDinPath: Subscription | null = null;
  private subCodingCheckBoxClickedEvent: Subscription | null = null;
  private ItemCodingServiceDataChanged: Subscription | null = null;
  private ItemArmsDataChanged: Subscription | null = null;
  private subGotPDFforViewing: Subscription | null = null;
  public get itemID(): number {
    if (this.item) return this.item.itemId;
    else return -1;
  }
  private itemString: string = '0';
  public item?: Item;
  public ShowOutComes: boolean = false;
  private subGotScreeningItem: Subscription | null = null;
  public IsScreening: boolean = false;
  public get ShowHighlights(): boolean {
    return this.ReviewerIdentityServ.userOptions.ShowHighlight;
  }
  public set ShowHighlights(val: boolean) {
    this.ReviewerIdentityServ.userOptions.ShowHighlight = val;
    this.ReviewerIdentityServ.SaveOptions();//otherwise they won't persist...
  }
  public HAbstract: string = "";
  public HTitle: string = "";
  public HelpAndFeebackContext: string = "(codingui)itemdetails";
  @ViewChild('ItemDetailsCmp') private ItemDetailsCompRef!: any;
  @ViewChild('codesetTreeCoding') public codesetTreeCoding!: CodesetTreeCodingComponent;
  @ViewChild('tabstripCoding') public tabstrip!: TabStripComponent;
  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;

  public innerWidth: any = 900;
  private readonly innerWidthThreshold: number = 768;
  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    if (this.codesetTreeCoding && this.codesetTreeCoding.showManualModal == true
      && this.innerWidth >= this.innerWidthThreshold
      && window.innerWidth < this.innerWidthThreshold) {
      this.ShowCodesInSmallScreen = true;
    }
    this.innerWidth = window.innerWidth;
  }
  IsSmallScreen(): boolean {
    if (this.innerWidth && this.innerWidth < this.innerWidthThreshold) {
      return true;
    }
    else return false;
  }
  IsServiceBusy(): boolean {
    if (this.reviewInfoService.IsBusy || this._outcomeService.IsBusy || this.ItemDocsService.IsBusy
      || this.ReviewerTermsService.IsBusy) {
      return true;
    }
    else return false;
  }
  public IsServiceBusyTerms(): boolean {
    if (this.ReviewerTermsService.IsBusy) return true;
    else return false;
  }

  public IsServiceBusy4PDF(): boolean {
    if (this.ItemCodingService.IsBusy
      || this.ReviewSetsService.IsBusy
      //|| this.armservice.IsBusy
      || this.ItemDocsService.IsBusy
    ) return true;
    else return false;
  }

  public HotKeysOn: boolean = false;

  public RefreshTerms() {

    //this.SetHighlights();
    this.ReviewerTermsService.Fetch();
  }
  public ShowCodesInSmallScreen: boolean = false;
  private _ShowCodesInSmallScreenOverriden: boolean = false;
  public ShowHideCodes() {
    this.ShowCodesInSmallScreen = !this.ShowCodesInSmallScreen;
  }
  public get HasTermList(): boolean {
    if (!this.ReviewerTermsService || !this.ReviewerTermsService.TermsList || !(this.ReviewerTermsService.TermsList.length > 0)) return false;
    else return true;
  }
  ngAfterViewInit() {
    // child is set
    //if (this.ArmsCompRef) this.ArmsCompRef.CurrentItem = this.item;
  }
  SelectTab(i: number) {
    if (!this.tabstrip) return;
    else {
      let t = this.tabstrip.tabs.get(i);
      if (!t) return;
      let e = new SelectEvent(i, t.title);
      this.tabstrip.selectTab(i);
      this.onTabSelect(e);
    }
  }
  onTabSelect(e: SelectEvent) {

    if (e.title == 'Item Details') {
      this.HelpAndFeebackContext = "(codingui)itemdetails";
    }
    else if (e.title == 'Study Arms') {
      this.HelpAndFeebackContext = "(codingui)itemdetails\\arms";
    }
    else if (e.title == 'PDF') {
      //if (this.HasDocForView && this.pdftroncontainer.currentDocId !== this.ItemDocsService.CurrentDocId) {
      //this.pdftroncontainer.loadDoc();//only load it if it's not there already
      //}
      this.HelpAndFeebackContext = "(codingui)itemdetails\\pdf";//no record in DB for the help!!
    }
    else {
      this.HelpAndFeebackContext = "(codingui)itemdetails";
    }
  }

  RemoveCodeModalOpenedIncoming() {
    //console.log("opened");
    if (this.ShowCodesInSmallScreen == false && this.IsSmallScreen() == true) {
      //codes flap is closed, but screen is small, so the dialog to confirm "RemoveCode" would be hidden:
      //we'll open the codes screen automatically
      this._ShowCodesInSmallScreenOverriden = true;
      this.ShowCodesInSmallScreen = true;
    }
  }
  RemoveCodeModalClosedIncoming() {
    //console.log("closed");
    if (this._ShowCodesInSmallScreenOverriden == true) {
      //we automatically opened the codes flap, and now the dialog to confirm "RemoveCode" has been closed:
      //we'll automatically re-close the codes-flap...
      this._ShowCodesInSmallScreenOverriden = false;
      this.ShowCodesInSmallScreen = false;
    }
  }


  public get HasDocForView(): boolean {
    //console.log("hasDocForView", this.ItemDocsService.CurrentDoc);
    if (this.ItemDocsService.CurrentDoc) return true;
    else return false;
  }
  async CheckAndMoveToPDFTab() {
    //console.log("CheckAndMoveToPDFTab", this.ItemDocsService.CurrentDoc, this.tabstrip);
    if (this.HasDocForView) {
      //console.log("CheckAndMoveToPDFTab2");
      if (this.pdftroncontainer) this.pdftroncontainer.loadDoc();
      if (this.tabstrip) {
        //console.log("CheckAndMoveToPDFTab3");
        await Helpers.Sleep(50);//we need to give the UI thread the time to catch up and "un-disable" the tab.
        this.SelectTab(1);
      }
    }
  }
  public get ShouldFetchPDFCoding(): boolean {//tells the tree component whether we should go fetch the PDF coding details...
    if (this.HelpAndFeebackContext == "itemdetails\\pdf") return true;
    else if (this.ItemDocsService._itemDocs.filter(found => found.itemDocumentId == this.ItemDocsService.CurrentDocId).length > 0) return true;
    else return false;
  }
  public get CheckBoxAutoAdvanceVal(): boolean {
    return this.ReviewerIdentityServ.userOptions.AutoAdvance;
  }
  public set CheckBoxAutoAdvanceVal(val: boolean) {
    this.ReviewerIdentityServ.userOptions.AutoAdvance = val;
  }
  onSubmit(f: string) {
  }
  //@Output() criteriaChange = new EventEmitter();
  //public ListSubType: string = "";
  public ShowingOutComes() {
    this.ShowOutComes = !this.ShowOutComes;
  }
  

  public get HasOutcomeUnsavedChanges(): boolean {
    if (this.ShowOutComes == false) return false;
    else return this._outcomeService.currentOutcomeHasChanges;
  }
  public GetItem() {
    this.ShowOutComes = false;
    this.WipeHighlights();
    this.ItemDocsService.Clear();
    if (this.itemString == 'PriorityScreening') {
      if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
      this.IsScreening = true;
      this.PriorityScreeningService.UsingListFromSearch = false;
      this.PriorityScreeningService.NextItem();
    }
    else if (this.itemString == 'ScreeningFromList') {
      if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
      this.IsScreening = true;
      this.PriorityScreeningService.UsingListFromSearch = true;
      this.PriorityScreeningService.NextItem();
    }
    else {
      //this.itemID = +this.itemString;
      this.item = this.ItemListService.getItem(+this.itemString);

      this.IsScreening = false;
      this.GetItemCoding();
      //this.ItemListService.eventChange(this.itemID);
      console.log('fill in arms here teseroo1');

    }
  }

  public HasPreviousScreening(): boolean {
    //console.log('CanMoveToPInScreening' + this.PriorityScreeningService.CurrentItemIndex);
    if (this.PriorityScreeningService.CurrentItemIndex > 0) return true;
    return false;
  }
  public CanMoveToNextInScreening(): boolean {

    let ItemSetIndex = this.ItemCodingService.ItemCodingList.findIndex(cset =>
      cset.setId == this.reviewInfoService.ReviewInfo.screeningCodeSetId
      && (cset.contactId == this.ReviewerIdentityServ.reviewerIdentity.userId || cset.isCompleted));
    if (ItemSetIndex == -1) return false;
    else return true;
  }
  public prevScreeningItem() {
    this.WipeHighlights();
    if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
    this.IsScreening = true;
    this.PriorityScreeningService.PreviousItem();
  }
  public GotScreeningItem() {
    //console.log('got Screening Item');
    this.item = this.PriorityScreeningService.CurrentItem;
    //this.itemID = this.item.itemId;
    this.GetItemCoding();
  }
  private GetItemCoding() {
    //console.log('sdjghklsdjghfjklh ' + this.itemID);
    this.ItemDocsService.FetchDocList(this.itemID);
    if (this.item) {
      if (this.ArmsCompRef) {
        this.ArmsCompRef.CurrentItem = this.item;
      }
      this.armservice.FetchAll(this.item);
      //this.armservice.Fetchtimepoints(this.item);
    }
    //this._outcomeService.outcomesChangedEE.emit();
    this.ItemCodingService.Fetch(this.itemID);

  }
  SetCoding() {
    //console.log('set coding');
    this.SetHighlights();
    this.ReviewSetsService.clearItemData();
    if (this.ItemCodingService.ItemCodingList.length == 0) return;//no need to add codes that don't exist.
    if (this.armservice.SelectedArm) this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, this.armservice.SelectedArm.itemArmId);
    else this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, 0);
  }
  SetArmCoding(armId: number) {
    //console.log('change Arm');
    this.ReviewSetsService.clearItemData();
    this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, armId);
  }

  private _hasPrevious: boolean | null = null;
  hasPrevious(): boolean {

    if (!this.item || !this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.items || this.ItemListService.ItemList.items.length < 1) {
      //console.log('NO!');
      return false;
    }
    else if (this._hasPrevious === null) {
      this._hasPrevious = this.ItemListService.hasPrevious(this.item.itemId);
    }
    //console.log('has prev? ' + this._hasPrevious);
    return this._hasPrevious;
  }
  MyHumanIndex(): number {
    return this.ItemListService.ItemList.items.findIndex(found => found.itemId == this.itemID) + 1;
  }
  private _hasNext: boolean | null = null;
  hasNext(): boolean {
    if (!this.item || !this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.items || this.ItemListService.ItemList.items.length < 1) {
      return false;
    }
    else if (this._hasNext === null) {
      this._hasNext = this.ItemListService.hasNext(this.item.itemId);
    }
    return this._hasNext;
  }
  firstItem() {
    this.WipeHighlights();

    if (this.item) this.goToItem(this.ItemListService.getFirst());

  }
  prevItem() {

    this.WipeHighlights();
    if (this.item) {
      console.log('inside previous coding item component part' + this.item.itemId);

      this.goToItem(this.ItemListService.getPrevious(this.item.itemId));
    }
  }
  nextItem() {

    this.WipeHighlights();
    if (this.item) {
      this.goToItem(this.ItemListService.getNext(this.item.itemId));
      console.log('inside next coding item component part' + this.item.itemId);
      //this.valueChange.next(this.item.itemId);
    }
  }
  lastItem() {
    this.WipeHighlights();
    if (this.item) this.goToItem(this.ItemListService.getLast());
  }
  clearItemData() {
    this._hasNext = null;
    this._hasPrevious = null;
    this.item = undefined;
    //this.itemID = -1;
    this.ItemCodingService.ItemCodingList = [];
    if (this.ReviewSetsService) {
      this.ReviewSetsService.clearItemData();
    }
  }
  goToItem(item: Item) {
    this.WipeHighlights();
    this.clearItemData();
    console.log('what do you need me to do?' + item.itemId);
    this.router.navigate(['itemcodingOnly', item.itemId]);
    this.item = item;
    //if (this.item.itemId != this.itemID) {

    //    this.itemID = this.item.itemId;
    //}
    //this.GetItemCoding();
  }
  BackToMain() {
    if (this.item) {
      if (
        this.IsScreening
        || this.PriorityScreeningService.ShouldCheckForRaicWork(this.ItemCodingService.ItemCodingList)
      )
        this.PriorityScreeningService.RaicFindAndDoWorkFromUITrigger(this.item.itemId);
    }
    this.clearItemData();
    this.router.navigate(['MainCodingOnly']);
  }
  ItemAttributeSave(data: CheckBoxClickedEventData) {

    let SubSuccess: Subscription;
    let SubError: Subscription;//see https://medium.com/thecodecampus-knowledge/the-easiest-way-to-unsubscribe-from-observables-in-angular-5abde80a5ae3
    //console.log(data.AttId);
    let attribute: SetAttribute | null = this.ReviewSetsService.FindAttributeById(data.AttId);

    if (!attribute) {
      //problem: we don't know what to code!
      alert('Could not find the code we want to add to this item, please reload: something is not right with the data.');
      return;
    }

    let itemSet: ItemSet | null = this.ItemCodingService.FindItemSetBySetId(attribute.set_id);

    let cmd: ItemAttributeSaveCommand = new ItemAttributeSaveCommand();
    if (itemSet) {
      //we have an item set to use, so put the relevant data in cmd. Otherwise, default value is fine.
      cmd.itemSetId = itemSet.itemSetId;
    }
    cmd.itemId = this.itemID;
    cmd.additionalText = data.additionalText;
    cmd.itemArmId = data.armId;
    cmd.setId = attribute.set_id;
    //console.log(attribute.set_id);
    cmd.attributeId = data.AttId;
    cmd.revInfo = this.reviewInfoService.ReviewInfo;
    let itemAtt: ReadOnlyItemAttribute | null = null;
    if ((data.event.target && data.event.target.checked) || data.event == 'InfoboxTextAdded') {
      //add new code to item
      //console.log('cmd.saveType = "Insert"');
      cmd.saveType = "Insert";
    }
    else if (data.event == 'InfoboxTextUpdate' && itemSet) {
      //console.log('cmd.saveType = "Update"');
      cmd.saveType = "Update";
      for (let Candidate of itemSet.itemAttributesList) {
        if (Candidate.attributeId == cmd.attributeId
          && Candidate.armId == cmd.itemArmId) {
          itemAtt = Candidate;
          break;
        }
      }
      if (!itemAtt) {
        //problem: we can't remove an item att, if we can't find it!
        alert('Sorry: can\'t find the Attribute to update...');
        return;
      }
      cmd.itemAttributeId = itemAtt.itemAttributeId;
    }
    else if (!data.event.target.checked && itemSet) {
      //we delete the coding
      cmd.saveType = "Delete";

      for (let Candidate of itemSet.itemAttributesList) {
        if (Candidate.attributeId == cmd.attributeId
          && Candidate.armId == cmd.itemArmId) {
          itemAtt = Candidate;
          break;
        }
      }
      if (!itemAtt) {
        //problem: we can't remove an item att, if we can't find it!
        alert('Sorry: can\'t find the Attribute to remove...');
        return;
      }
      cmd.itemAttributeId = itemAtt.itemAttributeId;
    }
    SubError = this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.subscribe((cmdErr: any) => {
      this.ReviewSetsService.ItemCodingItemAttributeSaveCommandHandled();
      //do something if command ended with an error
      //console.log('Error handling');
      alert("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
      //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.unsubscribe();
      //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.unsubscribe();
    });
    SubSuccess = this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.subscribe((cmdResult: ItemAttributeSaveCommand) => {
      //update local version of the coding...

      if (cmd.saveType == "Insert" || cmd.saveType == "Update") {
        this.ItemCodingService.ApplyInsertOrUpdateItemAttribute(cmdResult, itemSet);

        if (this.IsScreening && cmd.saveType == "Insert" && this.PriorityScreeningService.CurrentItemIndex == this.PriorityScreeningService.ScreenedItemIds.length - 1) {
          //snippet to tell the PS service that the last item in the list of screening items has been coded.
          //this is used to count "items screened in this session" accurately
          let ItemSetIndex = this.ItemCodingService.ItemCodingList.findIndex(cset =>
            cset.setId == this.reviewInfoService.ReviewInfo.screeningCodeSetId
            && (cset.contactId == this.ReviewerIdentityServ.reviewerIdentity.userId || cset.isCompleted));
          if (ItemSetIndex > -1) {
            this.PriorityScreeningService.LastItemInTheQueueIsDone = true;
          }
        }
      }
      else if (cmd.saveType == "Delete") {
        this.ItemCodingService.ApplyDeleteItemAttribute(itemSet, itemAtt);

        if (this.IsScreening && this.PriorityScreeningService.CurrentItemIndex == this.PriorityScreeningService.ScreenedItemIds.length - 1) {
          //snippet to tell the PS service that the last item in the list of screening items has NOT been coded.
          //this is used to count "items screened in this session" accurately
          let ItemSetIndex = this.ItemCodingService.ItemCodingList.findIndex(cset =>
            cset.setId == this.reviewInfoService.ReviewInfo.screeningCodeSetId
            && (cset.contactId == this.ReviewerIdentityServ.reviewerIdentity.userId || cset.isCompleted));
          if (ItemSetIndex == -1) {
            this.PriorityScreeningService.LastItemInTheQueueIsDone = false;
          }
        }
      }

      this.SetCoding();
      if (this.PriorityScreeningService.CheckForNeedOfLockingThisItem(this.ItemCodingService.ItemCodingList, this.IsScreening, cmdResult)) {
        //console.log("We should lock this item: " + this.itemID);
        this.PriorityScreeningService.PleaseLockThisItem(this.itemID);
      }
      //else { console.log("We should NOT lock this item: " + this.itemID); }
      this.ReviewSetsService.ItemCodingItemAttributeSaveCommandHandled();
      console.log('set dest');
      SubSuccess.unsubscribe();
      SubError.unsubscribe();
      if (cmd.saveType == "Insert" && this.CheckBoxAutoAdvanceVal) {
        //auto advance is on, we want to go to the next item
        if (this.IsSmallScreen() && this.ShowCodesInSmallScreen) this.ShowCodesInSmallScreen = false;
        if (attribute) {
          this.notificationService.show({
            content: "Code just added: " + attribute.name,
            position: { horizontal: 'left', vertical: 'bottom' },
            animation: { type: 'fade', duration: 500 },
            type: { style: 'none', icon: false },
            hideAfter: 3000
          });
        }
        if (!this.IsScreening && this.hasNext()) this.nextItem();
        else if (this.IsScreening) this.GetItem();//in screening mode, this uses the screening service to receive the next item
        if (this.tabstrip) {
          this.SelectTab(0);//in CodingOnly, we always go to ItemDetails whenever we move item...
        }
      }
      //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.unsubscribe();
      //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.unsubscribe();
    });
    //console.log("canwrite:" + this.ReviewSetsService.CanWrite);
    this.ReviewSetsService.ExecuteItemAttributeSaveCommand(cmd, this.ItemCodingService.ItemCodingList);
  }
  toHTML(text: string): string {
    return text.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
  }

  ItemChanged() {
    if (this.tabstrip) this.SelectTab(0);
    this.WipeHighlights();
    this.SetHighlights();
  }

  ngOnDestroy() {
    //console.log('killing coding comp');
    if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();
    if (this.ItemCodingServiceDataChanged) this.ItemCodingServiceDataChanged.unsubscribe();
    if (this.subCodingCheckBoxClickedEvent) this.subCodingCheckBoxClickedEvent.unsubscribe();
    if (this.subGotScreeningItem) this.subGotScreeningItem.unsubscribe();
    if (this.subGotPDFforViewing) this.subGotPDFforViewing.unsubscribe();
    this._outcomeService.Clear();
  }
  WipeHighlights() {
    if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.WipeHighlights();
  }
  SetHighlights() {
    if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.SetHighlights();
  }
  ShowHighlightsClicked() {
    if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.ShowHighlightsClicked();
  }


}
