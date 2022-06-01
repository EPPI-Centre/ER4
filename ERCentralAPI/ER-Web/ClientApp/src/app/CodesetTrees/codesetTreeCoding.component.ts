import { Component, Inject, OnInit, Output, EventEmitter, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, Attribute } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute, ItemSetCompleteCommand } from '../services/ReviewSets.service';
import { ITreeOptions, TreeComponent } from '@circlon/angular-tree-component';
import { NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ArmTimepointLinkListService } from '../services/ArmTimepointLinkList.service';
//import { ITreeNode } from 'angular-tree-component/dist/defs/api';
import { ItemCodingService, ItemAttPDFCodingCrit } from '../services/ItemCoding.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { OutcomesService } from '../services/outcomes.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { Subscription } from 'rxjs';
import { TreeItem } from '@progress/kendo-angular-treeview';

@Component({
  selector: 'codesetTreeCoding',
  styles: [`.bt-infoBox {    
                    padding: .08rem .12rem .12rem .12rem;
                    margin-bottom: .12rem;
                    font-size: .875rem;
                    line-height: 1.2;
                    border-radius: .2rem;
                }
			.no-select{    
				-webkit-user-select: none;
				cursor:not-allowed; /*makes it even more obvious*/
				}
        `],
  templateUrl: './codesetTreeCoding.component.html'
})
export class CodesetTreeCodingComponent implements OnInit, OnDestroy {
  constructor(private router: Router,
    private _httpC: HttpClient,
    @Inject('BASE_URL') private _baseUrl: string,
    private ReviewerIdentityServ: ReviewerIdentityService,
    private ReviewSetsService: ReviewSetsService,
    private ItemCodingService: ItemCodingService,
    private modalService: NgbModal,
    private ItemDocsService: ItemDocsService,
    private armsService: ArmTimepointLinkListService,
    private ReviewInfoService: ReviewInfoService,
    private NotificationService: NotificationService,
    private _ItemCodingService: ItemCodingService,
    private _outcomeService: OutcomesService,
    private ReviewSetsEditingService: ReviewSetsEditingService
  ) { }
  //@ViewChild('ConfirmDeleteCoding') private ConfirmDeleteCoding: any;
  @ViewChild('ManualModal') private ManualModal: any;
  public showManualModal: boolean = false;
  @Input() InitiateFetchPDFCoding = false;
  @Input() Context: string = "CodingFull";
  subRedrawTree: Subscription | null = null;

  ngOnInit() {
    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
      this.router.navigate(['home']);
    }
    else {
      //if (this.ReviewInfoService.Contacts.length == 0) this.ReviewInfoService.FetchReviewMembers();
      this.ItemCodingService.DataChanged.subscribe(() => { this.CancelCompleteUncomplete(); });
      this.subRedrawTree = this.ReviewSetsEditingService.PleaseRedrawTheTree.subscribe(
        () => { this.UpdateTree(); }
      );
      //console.log("Review Ticket: " + this.ReviewerIdentityServ.reviewerIdentity.ticket);
      //let modalComp = this.modalService.open(InfoBoxModalContent);
      //modalComp.close();
      //this.GetReviewSets();
    }
  }

  options: ITreeOptions = {
    childrenField: 'attributes',
    displayField: 'name',
    allowDrag: false,
  }
  @ViewChild('tree') treeComponent!: TreeComponent;
  public ShowCompleteUncompletePanelForSetId: number = 0;
  public ItemSetProxy: MinimalItemSet = new MinimalItemSet();
  public ItemSetReference: MinimalItemSet = new MinimalItemSet();
  public get IsCodingOnly(): boolean {
    return this.Context == "CodingOnly";
  }
  public get CanSaveItemSet(): boolean {
    if (this.IsCodingOnly) return false;
    else if (!this.ReviewerIdentityServ.HasWriteRights) return false;
    else if (
      (//something did change
        this.ItemSetProxy.IsCompleted != this.ItemSetReference.IsCompleted
        || this.ItemSetProxy.IsLocked != this.ItemSetReference.IsLocked
      )
      //AND we're looking at the correct ItemSetId
      && this.ItemSetProxy.ItemSetId == this.ItemSetReference.ItemSetId
    ) {
      return true;
    }
    else return false;
  }
  public get IsServiceBusy(): boolean {
    //console.log("mainfull IsServiceBusy", this.ItemListService, this.codesetStatsServ, this.SourcesService )
    return (this.ReviewSetsService.IsBusy ||
      this.ItemCodingService.IsBusy ||
      this.armsService.IsBusy ||
      this.ReviewInfoService.IsBusy ||
      this._outcomeService.IsBusy);
  }
  public get IsReviewSetsServiceBusy(): boolean {
    //console.log("mainfull IsServiceBusy", this.ItemListService, this.codesetStatsServ, this.SourcesService )
    return (this.ReviewSetsService.IsBusy);
  }
  public CanWriteCoding(data: singleNode) {
    return this.ReviewSetsService.CanWriteCoding(data)
  }
  public attributeType: string = '';
  public outcomesPresent: boolean = false;
  public checkOutComes(data: singleNode): boolean {
    var selectedNode: boolean = false;
    var itemSetId = 0;
    if (data.nodeType == 'ReviewSet') {
      return false;
    } else {
      let node = data as SetAttribute;
      var nodeAttType = node.attribute_type;
      this.attributeType = nodeAttType;
      var itemSet = this._ItemCodingService.FindItemSetBySetId(node.set_id);
      if (itemSet) {
        itemSetId = itemSet.itemSetId
      }

      selectedNode = data.isSelected;
    }
    let okayAttType: boolean = false;
    if (nodeAttType == 'Outcome'
      || nodeAttType == 'Intervention'
      || nodeAttType == 'Comparison') {
      okayAttType = true;
    }
    if (okayAttType
    ) {

      return true;
    } else {
      return false;
    }
  }
  public OutcomePanel: boolean = false;
  public openOutcomePanel(data: singleNode) {

    let node = data as SetAttribute;
    if (node != null && node.isSelected) {
      this._outcomeService.outcomesChangedEE.emit(node);
    }
  }
  //nodes: singleNode[] = [];
  get nodes(): singleNode[] | null {
    //console.log('Getting codetree nodes');
    if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) {
      //for (var i = 0; i < this.ReviewSetsService.ReviewSets.length; i++) {

      //	console.log('found my nodes: ' + this.ReviewSetsService.ReviewSets[i] + '\n');
      //}

      return this.ReviewSetsService.ReviewSets;
    }
    else {
      //console.log('NO nodes');
      return null;
    }
  }

  CheckBoxClicked(event: any, data: singleNode,) {
    let checkPassed: boolean = true;
    if (event.target) checkPassed = event.target.checked;//if we ticked the checkbox, it's OK to carry on, otherwise we need to check
    if (!checkPassed) {
      this.DeletingData = data;
      this.DeletingEvent = event;
      //all this seems necessary because I could not suppress the error discussed here:
      //https://blog.angularindepth.com/everything-you-need-to-know-about-the-expressionchangedafterithasbeencheckederror-error-e3fd9ce7dbb4
      this.showManualModal = true;
    }
    else this.CheckBoxClickedAfterCheck(event, data);
  }
  private DeletingEvent: any;
  private DeletingData: singleNode | null = null;
  DeleteCodingConfirmed() {
    if (this.DeletingData) {
      this.DeletingData.isSelected = false;
      this.CheckBoxClickedAfterCheck(this.DeletingEvent, this.DeletingData);
    }
    this.DeletingEvent = undefined;
    this.DeletingData = null;
    this.showManualModal = false;
  }
  DeleteCodingCancelled() {
    if (this.DeletingData) this.DeletingData.isSelected = true;
    this.DeletingEvent = undefined;
    this.DeletingData = null;
    this.showManualModal = false;
  }
  CheckBoxClickedAfterCheck(event: any, data: singleNode) {
    let evdata: CheckBoxClickedEventData = new CheckBoxClickedEventData();
    evdata.event = event;
    evdata.armId = this.armsService.SelectedArm == null ? 0 : this.armsService.SelectedArm.itemArmId;
    evdata.AttId = +data.id.replace('A', '');
    evdata.additionalText = data.additionalText;
    this.ReviewSetsService.PassItemCodingCeckboxChangedEvent(evdata);
  }

  CompleteUncompleteShowPanel(node: singleNode) {
    //alert('Complete/uncomplete clicked - Sorry, this feature is not implemented yet.');
    this.ItemSetProxy = new MinimalItemSet();
    this.ItemSetReference = new MinimalItemSet();
    if (node.nodeType != "ReviewSet") {
      this.ShowCompleteUncompletePanelForSetId = 0;
      return;//not a codeset, can't un/complete or lock...
    }
    let rSet = node as ReviewSet;
    if (!rSet || rSet.ItemSetId == 0) {
      this.ShowCompleteUncompletePanelForSetId = 0;
      return;//couldn't cast to codeset OR codeset doesn't have codes applied.
    }
    else if (this.ShowCompleteUncompletePanelForSetId == rSet.set_id) {
      this.ShowCompleteUncompletePanelForSetId = 0;
      return;//panel was open, let's close it.
    }
    else {//we are showing the dialog
      this.ItemSetProxy.ItemSetId = rSet.ItemSetId;
      this.ItemSetProxy.IsLocked = rSet.itemSetIsLocked;
      this.ItemSetProxy.IsCompleted = rSet.codingComplete;
      this.ItemSetProxy.CanBeInComparison = rSet.setType.allowComparison;
      this.ItemSetProxy.set_id = rSet.set_id;
      this.ItemSetProxy.set_name = rSet.set_name;
      this.ItemSetReference.ItemSetId = rSet.ItemSetId;
      this.ItemSetReference.IsLocked = rSet.itemSetIsLocked;
      this.ItemSetReference.IsCompleted = rSet.codingComplete;
      this.ItemSetReference.set_id = rSet.set_id;
      const itemSet = this.ItemCodingService.FindItemSetByItemSetId(rSet.ItemSetId);
      if (itemSet) {
        this.ItemSetProxy.CodedBy = itemSet.contactName;
        this.ItemSetProxy.CodedById = itemSet.contactId;
      }
      this.ShowCompleteUncompletePanelForSetId = rSet.set_id;
    }
  }
  CancelCompleteUncomplete() {
    this.ItemSetProxy = new MinimalItemSet();
    this.ItemSetReference = new MinimalItemSet();
    this.ShowCompleteUncompletePanelForSetId = 0;
  }
  ApplyCompleteUncomplete(data: singleNode) {
    //do something and then:
    let cmd: ItemSetCompleteCommand = new ItemSetCompleteCommand();
    cmd.itemSetId = this.ItemSetProxy.ItemSetId;
    cmd.isLocked = this.ItemSetProxy.IsLocked;
    cmd.complete = this.ItemSetProxy.IsCompleted;
    this.ReviewSetsService.ExecuteItemSetCompleteCommand(cmd).then(
      result => {
        if (result) {
          //we still need to update data in ItemCodingService
          let iSet = this.ItemCodingService.FindItemSetByItemSetId(this.ItemSetProxy.ItemSetId);
          if (iSet) {
            iSet.isLocked = this.ItemSetProxy.IsLocked;
            iSet.isCompleted = this.ItemSetProxy.IsCompleted;
            if (!iSet.isCompleted && iSet.contactId != this.ReviewerIdentityServ.reviewerIdentity.userId) {
              //user un-completed somebody else's version, so we might need to show a coding (the one that belongs to the user!)...
              this.ReviewSetsService.clearItemData();
              if (this.armsService.SelectedArm) this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, this.armsService.SelectedArm.itemArmId);
              else this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, 0);
              this.FetchPDFHighlights();//after setting the coding data, we might need to re-fetch the PDF highlights, checks on whether that's likely to be need happen in there.

            }
            this.NotificationService.show(
              {
                content: "Changes saved for coding on \"" + this.ItemSetProxy.set_name + "\".",
                position: { horizontal: 'left', vertical: 'bottom' },
                animation: { type: 'fade', duration: 500 },
                type: { style: 'none', icon: false },
                hideAfter: 3000
              }
            );
          }
          else {
            //something is amiss, but data was saved...
            this.NotificationService.show({
              content: "Changes saved, but couldn't apply them here. Please reload this item to avoid the risk of corrupting data.",
              animation: { type: 'slide', duration: 400 },
              position: { horizontal: 'center', vertical: 'top' },
              type: { style: "error", icon: true },
              closable: true
            });
          }
        }
      }
    );
    this.ShowCompleteUncompletePanelForSetId = 0;
  }
  openInfoBox(data: singleNode) {
    //const tmp: any = new InfoBoxModalContent();
    let modalComp = this.modalService.open(InfoBoxModalContent);

    modalComp.componentInstance.InfoBoxTextInput = data.additionalText;
    modalComp.componentInstance.focus(this.ReviewSetsService.CanWriteCoding(data));
    //let tBox = this.renderer.selectRootElement('#InfoBoxText');
    //tBox.innerText = modalComp.componentInstance.InfoBoxTextInput;

    modalComp.result.then((infoTxt) => {
      data.additionalText = infoTxt;
      if (!data.isSelected) {


        this.CheckBoxClickedAfterCheck('InfoboxTextAdded', data);//checkbox is not ticked: we are adding this code
      }
      else {

        this.CheckBoxClickedAfterCheck('InfoboxTextUpdate', data);// checkbox is ticked: we are editing text in infobox
      }
    },
      () => {
        //alert('dismissed: ' + AttId)
      }
    );

  }
  public SelectedNodeData: singleNode | null = null;
  public SelectedCodeDescription: string = "";

  NodeSelected(event: TreeItem) {
    let node: singleNode = event.dataItem;
    //alert('in node: ' + node.name)
    this.ReviewSetsService.selectedNode = node;
    this.SelectedNodeData = node;
    this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
    this.FetchPDFHighlights();
  }
  FetchPDFHighlights() {
    const att = this.SelectedNodeData as SetAttribute;
    if (att && att.nodeType == "SetAttribute") {
      this.ItemCodingService.SelectedSetAttribute = att;
      if (this.InitiateFetchPDFCoding && att.isSelected && this.ItemDocsService.CurrentDocId !== 0) {
        const ROatt = this.ItemCodingService.FindROItemAttributeByAttribute(att);
        console.log("we might need to fetch PDF coding", ROatt);
        if (ROatt) this.ItemCodingService.FetchItemAttPDFCoding(new ItemAttPDFCodingCrit(this.ItemDocsService.CurrentDocId, ROatt.itemAttributeId));
        else {
          this.ItemCodingService.ClearItemAttPDFCoding();
        }
      }
      else {
        this.ItemCodingService.ClearItemAttPDFCoding();
      }
    } else {
      this.ItemCodingService.ClearItemAttPDFCoding();
      this.ItemCodingService.SelectedSetAttribute = null;//remove selection, PDF should not load highlights.
    }
  }

  public UpdateTree() {
    if (this.treeComponent) this.treeComponent.treeModel.update();
    else console.log("Update tree failed, no tree component!");
  }

  ngOnDestroy() {
    //this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
    if (this.subRedrawTree) this.subRedrawTree.unsubscribe();
    //console.log('killing reviewSets comp');
  }
}

class MinimalItemSet {
  set_id: number = 0;
  set_name: string = "";
  ItemSetId: number = 0;
  CanBeInComparison = false;
  public get CanChangeCompletion(): boolean {
    if (this.CanBeInComparison) return true;
    else return (!this.IsCompleted);//a coding that is not completed can always be completed, even if the codeset can't be put in comparison mode.
  }
  IsCompleted: boolean = false;
  IsLocked: boolean = false;
  CodedBy: string = "N/A";
  CodedById: number = 0;
}

//another class!!!
export class CheckBoxClickedEventData {
  event: any | null = null;
  AttId: number = 0;
  additionalText: string = "";
  armId: number = 0;
}


//EVEN more: small separate component for the infobox modal.
@Component({
  selector: 'ngbd-InfoBoxModal-content',
  templateUrl: './InfoBoxModal.component.html'
})
export class InfoBoxModalContent {
  @ViewChild('InfoBoxText')
  InfoBoxText!: ElementRef;

  @Input() InfoBoxTextInput: string = "";
  private canWrite: boolean = true;
  public get IsReadOnly(): boolean {
    //console.log('Is read only???');
    return this.canWrite;
    //return this.ReviewSetsService.CanWriteCoding(new SetAttribute());//.CanWrite;
  }
  constructor(public activeModal: NgbActiveModal, private ReviewSetsService: ReviewSetsService) { }
  public focus(canWrite: boolean) {
    //console.log("infobox focussing...", this.InfoBoxText);
    this.canWrite = canWrite;
    setTimeout(() => this.InfoBoxText.nativeElement.focus(), 1);
    //this.InfoBoxText.nativeElement.focus();

  }
}



