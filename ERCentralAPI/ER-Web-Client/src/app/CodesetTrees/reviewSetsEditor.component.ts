import { Component, Inject, OnInit, Input, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, iSetType, ReviewSet, singleNode, kvAllowedAttributeType, SetAttribute } from '../services/ReviewSets.service';
import { ReviewSetsEditingService, ChangeDataEntryMessage, ReviewSetCopyCommand } from '../services/ReviewSetsEditing.service';
import { CodesetTreeEditComponent } from './codesetTreeEdit.component';
import { EditCodeComp } from './editcode.component';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { Subscription } from 'rxjs';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

@Component({
  selector: 'ReviewSetsEditor',
  templateUrl: './reviewSetsEditor.component.html',
  providers: [],
  styles: [`.k-switch-label-off {    
                    color:black;
                }
        `],
})

export class ReviewSetsEditorComponent implements OnInit, OnDestroy {

  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private ReviewerIdentityServ: ReviewerIdentityService,
    private ReviewSetsService: ReviewSetsService,
    private ReviewSetsEditingService: ReviewSetsEditingService,
    private confirmationDialogService: ConfirmationDialogService,
    private ReviewInfoService: ReviewInfoService
  ) { }
  ngOnInit() {
    this.subRedrawTree = this.ReviewSetsEditingService.PleaseRedrawTheTree.subscribe(
      () => { this.RefreshLocalTree(); }
    );
    if (this.ReviewSetsEditingService.SetTypes.length == 0) {
      this.ReviewSetsEditingService.FetchSetTypes();
    }
    if (this.ReviewInfoService.ReviewInfo.reviewId == 0) {
      this.ReviewInfoService.Fetch();
    }

    //this._ActivityPanelName = this.treeEditorComponent.ActivityPanelName;

    //this._ActivityPanelName = 'MoveCode';
    /*
    if (this.editCodeComp.EditCodeActivity != null) {
      this._ActivityPanelName = 'MoveCode';
    }
    else {
      this._ActivityPanelName = 'MoveCode';
    }*/


  }
  @ViewChild('treeEditorComponent') treeEditorComponent!: CodesetTreeEditComponent;
  @ViewChild('editCodeComp') editCodeComp!: EditCodeComp;
  @ViewChild('CodeTypeSelect') CodeTypeSelect: any;

  subRedrawTree: Subscription | null = null;
  public get HelpAndFeebackContext(): string {
    if (this._ActivityPanelName == 'ImportCodesets') return 'importcodesets';
    else return "editcodesets"
  }
  private _ActivityPanelName: string = "";
  private _test: string = "";

  public ShowPanelContext: string = "";

  public get ActivityPanelName() {
      return this._ActivityPanelName;
  }

  private _EditCodeActivity: string = "";
  public get EditCodeActivity() {
    return this._EditCodeActivity;
  }
  public NewSetSelectedTypeId: number = 3;
  public get ReviewSets(): ReviewSet[] {
    return this.ReviewSetsService.ReviewSets;
  }
  public get SetTypes(): iSetType[] {
    return this.ReviewSetsEditingService.SetTypes;
  }
  public get NewSetSelectedTypeDescription(): string {
    let found = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
    if (found) return found.setTypeDescription;
    else return "";
  }
  private _NewReviewSet: ReviewSet = new ReviewSet();
  public get NewReviewSet(): ReviewSet {
    return this._NewReviewSet;
  }
  private _NewCode: SetAttribute = new SetAttribute();
  public get NewCode(): SetAttribute {
    return this._NewCode;
  }
  public get CurrentNode(): singleNode | null {
    if (!this.ReviewSetsService.selectedNode) return null;
    else return this.ReviewSetsService.selectedNode;
  }
  public get CurrentNodeId(): number | null {
    if (!this.CurrentNode) return null;
    else if (this.CurrentNode.nodeType == 'ReviewSet') {
      return (this.CurrentNode as ReviewSet).set_id;
    } else if (this.CurrentNode.nodeType == 'SetAttribute') {
      return (this.CurrentNode as SetAttribute).attribute_id;
    }
    return null;
  }
  public get CurrentNodeAsReviewSet(): ReviewSet | null {
    return this.ReviewSetsService.selectedNode as ReviewSet;
  }
  public get CurrentReviewSetCanEditUrls(): boolean {
    if (!this.ReviewSetsService.selectedNode) return false;
    const rs = this.ReviewSetsService.selectedNode as ReviewSet;
    if (!rs) return false;
    else return rs.userCanEditURLs;
  }
  IsServiceBusy(): boolean {
    if (this.ReviewSetsService.IsBusy || this.ReviewSetsEditingService.IsBusy || this.ReviewInfoService.IsBusy) return true;
    else return false;
  }
  CanWrite(): boolean {
    //console.log('CanWrite', this.ReviewerIdentityServ.HasWriteRights, this.IsServiceBusy());
    if (this.ReviewerIdentityServ.HasWriteRights && !this.IsServiceBusy()) {
      //console.log('CanWrite', true);
      return true;
    }
    else {
      //console.log('CanWrite', false);
      return false;
    }
  }
  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }
  private _appliedCodes: number = -1;
  public get appliedCodes(): number {
    return this._appliedCodes;
  }
  private _AllocationsAffected: number = -1;
  public get AllocationsAffected(): number {
    return this._AllocationsAffected;
  }
  ShowDeleteCodeset: boolean = false;
  ShowDeleteCodesetClicked() {
    //console.log('0');
    if (this.ActivityPanelName == "EditCodeset") {
      if (this.CurrentNode) {
        //console.log('1');
        let Set: ReviewSet = this.CurrentNode as ReviewSet;
        if (Set) {
          //console.log('2');
          this._appliedCodes = -1;
          this._AllocationsAffected = -1;
          this.ReviewSetsEditingService.AttributeOrSetDeleteCheck(Set.set_id, 0).then(
            success => {
              //alert("did it");
              this.ShowDeleteCodeset = true;
              this._appliedCodes = success.numItems;
              this._AllocationsAffected = success.numAllocations;
              //return result;
            },
            error => {
              //alert("Sorry, creating the new codeset failed.");
              //this.modalService.GenericErrorMessage(ErrMsg);
            });
        }
      }
    }
    //else if (this.ActivityPanelName == "EditCode" && this._UpdatingCode) {
    //    this._appliedCodes = -1;
    //    this.ShowDeleteCodeset = true;
    //    this.ReviewSetsEditingService.AttributeOrSetDeleteCheck(0, this._UpdatingCode.attributeSetId).then(
    //        success => {
    //            //alert("did it");
    //            this._appliedCodes = success.NumItems;
    //            //return result;
    //        },
    //        error => {
    //            //alert("Sorry, creating the new codeset failed.");
    //            //this.modalService.GenericErrorMessage(ErrMsg);
    //        });
    //}

  }
  HideDeleteCodeset() {
    this.ShowDeleteCodeset = false;
    this._appliedCodes = -1;
    this._AllocationsAffected = -1
  }
  DoDeleteCodeset() {
    if (!this.CurrentNode) return;
    let Set: ReviewSet = this.CurrentNode as ReviewSet;
    if (!Set) return;
    console.log("will delete:", Set);
    this.ReviewSetsEditingService.ReviewSetDelete(Set)
      .then(
        success => {
          //alert("did it");
          this.ReviewSetsService.selectedNode = null;
          this.ReviewSetsService.GetReviewSets();
          this.CancelActivity();
          //return result;
        },
        error => {
          this.ReviewSetsService.selectedNode = null;
          this.ReviewSetsService.GetReviewSets();
          this.CancelActivity();
          //alert("Sorry, creating the new codeset failed.");
          //this.modalService.GenericErrorMessage(ErrMsg);
        });
  }

  CopyCodesetClicked() {
    const source = this.CurrentNodeAsReviewSet;
    if (source == null) return;
    //there is at least one set - the one we're copying, so the below is safe...
    const order: number = this.ReviewSets[this.ReviewSets.length - 1].order + 1;
    this.confirmationDialogService.confirm("Create a Copy?",
      "This will create a new <strong>copy</strong> of the coding tool:<br>"
      + "<div class='w-100 p-0 mx-0 my-2 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
      + source.set_name + "</strong></div>"
      + "To avoid confusion, please <strong>edit name and description</strong> of the copied Tool as soon as possible.",
      false, "").then(
        (res: any) => {
          if (res == true) {
            this.ReviewSetsEditingService.ReviewSetCopy(source.reviewSetId, order).then(
              (res2) => {
                this.CopyCodesetIsDone(res2);
              }
            )
          }
        }
      ).catch(() => { });
  }
  private CopyCodesetIsDone(ReviewSetCopyCmd: ReviewSetCopyCommand) {
    if (ReviewSetCopyCmd.reviewSetId > 0) {
      this.CancelActivity();
      this.ReviewSetsService.GetReviewSets().then(() => {
        this.RefreshLocalTree();
        this.ReviewSetsService.selectedNode = this.ReviewSetsService.ReviewSets[this.ReviewSetsService.ReviewSets.length - 1];
      });
      //focus on the newly created tool, while we're there
      
    }
  }

  RefreshLocalTree() {
    console.log("RefreshLocalTree (reviewSets editor)");
    if (this.treeEditorComponent) {
      this.treeEditorComponent.RefreshLocalTree();
    }
  }
  UpdateCodeSet() {
    if (!this.CurrentNode) return;
    let Set: ReviewSet = this.CurrentNode as ReviewSet;
    if (!Set) return;
    console.log("will update:", Set, this.CurrentNode);
    this.ReviewSetsEditingService.SaveReviewSet(Set);
    this.CancelActivity();
  }
  public ShowChangeDataEntry: boolean = false;
  public DestinationDataEntryMode: string = "";
  public ChangeDataEntryModeMessage = "";
  private _CanChangeDataEntryMode: boolean = false;
  public get CanChangeDataEntryMode(): boolean {
    return (this._CanChangeDataEntryMode && this.CanWrite() && this._ItemsWithIncompleteCoding != -1);
  }
  private _ItemsWithIncompleteCoding: number = -1;
  public get ItemsWithIncompleteCoding(): number {
    return this._ItemsWithIncompleteCoding;
  }
  public get AllowedChildTypes(): kvAllowedAttributeType[] {
    return this.ReviewSetsService.AllowedChildTypesOfSelectedNode;
  }
  public get CurrentCodeLevel(): number {
    if (!this.CurrentNode) return -1;//??
    else if (this.CurrentNode.nodeType == "ReviewSet") return 0;
    else if (this.CurrentNode.nodeType == "SetAttribute") return this.ReviewSetsService.AttributeCurrentLevel(this.ReviewSetsService.selectedNode as SetAttribute);
    else return -1;//??
  }
  public get CurrentCodeCanHaveChildren(): boolean {
    //safety first, if anything didn't work as expexcted return false;
    if (!this.CanWrite()) return false;
    else {
      return this.ReviewSetsService.CurrentCodeCanHaveChildren;
      //end of bit that goes into "ReviewSetsService.CanNodeHaveChildren(node: singleNode): boolean"
    }
  }
  public SetTypeCanChangeDataEntryMode(): boolean {
    let Set: ReviewSet = this.CurrentNode as ReviewSet;
    if (Set && Set.setType && Set.setType.allowComparison) return true;
    else return false;
  }
  public NewSetTypeCanChangeDataEntryMode(): boolean {
    if (this.NewReviewSet && !this.NewReviewSet.setType) {
      let found = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
      if (found) this._NewReviewSet.setType = found;
    }
    if (this.NewReviewSet && this.NewReviewSet.setType && this.NewReviewSet.setType.allowComparison) return true;

    else return false;
  }
  async ShowChangeDataEntryClicked() {
    this._ItemsWithIncompleteCoding = -1;
    if (this.CurrentNode) {
      let Set: ReviewSet = this.CurrentNode as ReviewSet;
      if (Set) {
        this.ShowChangeDataEntry = true;
        const res: ChangeDataEntryMessage = await this.ReviewSetsEditingService.GetChangeDataEntryMessage(Set, this.ReviewInfoService.ReviewInfo.screeningCodeSetId);
        this.DestinationDataEntryMode = res.DestinationDataEntryMode;
        this.ChangeDataEntryModeMessage = res.ChangeDataEntryModeMessage;
        this._CanChangeDataEntryMode = res.CanChangeDataEntryMode;
        this._ItemsWithIncompleteCoding = res.ItemsWithIncompleteCoding;
      }
    }
    //this.ShowChangeDataEntry = true;
  }
  //ShowChangeDataEntryClicked() {
  //    this._ItemsWithIncompleteCoding = -1;
  //    if (this.CurrentNode) {
  //        let Set: ReviewSet = this.CurrentNode as ReviewSet;
  //        if (Set) {
  //            this.ShowChangeDataEntry = true;
  //            if (Set.set_id == this.ReviewInfoService.ReviewInfo.screeningCodeSetId) {
  //                this.DestinationDataEntryMode = "";
  //                this.ChangeDataEntryModeMessage = "This set is your current Screening Set (used for Priority Screening).";
  //                this.ChangeDataEntryModeMessage += "\r\nChanging the data entry mode would require to review/update the Priority Screening settings.";
  //                this.ChangeDataEntryModeMessage += "\r\nUnfortuately this feature is not currently implemented in the current App.";
  //                this.ChangeDataEntryModeMessage += "\r\nTo apply this change please use the full (Silverlight) version or EPPI-Reviewer 4.";
  //                this.DestinationDataEntryMode = "";
  //                this._CanChangeDataEntryMode = false;
  //                return;
  //            }
  //            else if (Set.codingIsFinal) {//moving to comparison data entry, easy!
  //                this.DestinationDataEntryMode = "Comparison";
  //                this.ChangeDataEntryModeMessage = "Are you sure you want to change to 'Comparison' data entry?";
  //                this.ChangeDataEntryModeMessage += "\r\nThis implies that you will have multiple users coding the same item using this Coding Tool and then reconciling the disagreements.";
  //                this.ChangeDataEntryModeMessage += "\r\nPlease ensure you have read the manual to check the implications of this.";
  //                this._ItemsWithIncompleteCoding = 0;
  //                this._CanChangeDataEntryMode = true;
  //            }
  //            else {//moving to normal data entry, need to check "troublesome items"
  //                this.DestinationDataEntryMode = "Normal";
  //                this.ChangeDataEntryModeMessage = "";
  //                this.ReviewSetsEditingService.ReviewSetCheckCodingStatus(Set.set_id).then(
  //                    success => {
  //                        //alert("did it");
  //                        this._ItemsWithIncompleteCoding = success;
  //                        if (this._ItemsWithIncompleteCoding > 0) {
  //                            this.ChangeDataEntryModeMessage = "You are about to change your data entry method to 'Normal', ";
  //                            this.ChangeDataEntryModeMessage += "but there are '" + this._ItemsWithIncompleteCoding + "' items that should be completed before you proceed. ";
  //                            this.ChangeDataEntryModeMessage += "You can view these incomplete items from the 'Review Home' screen.";
  //                            this._CanChangeDataEntryMode = true;
  //                        }
  //                        else if (this._ItemsWithIncompleteCoding == 0) {
  //                            this.ChangeDataEntryModeMessage = "You are about to change your data entry method to 'Normal'. \nThere are no potential data conflicts so it is safe to proceed.";
  //                            this._CanChangeDataEntryMode = true;
  //                        }
  //                        else {//error in the service, returned -1
  //                            this.ChangeDataEntryModeMessage = "Sorry, could not check coding status, thus, you should change the data entry mode. ";
  //                            this.ChangeDataEntryModeMessage += "If the problem persist, please contact EPPISupport.";
  //                            this._CanChangeDataEntryMode = false;
  //                        }
  //                    },
  //                    error => {
  //                        console.log("ERROR IN: ShowChangeDataEntryClicked API result", error);
  //                        this.ChangeDataEntryModeMessage = "Sorry, could not check coding status, thus, you should change the data entry mode. ";
  //                        this.ChangeDataEntryModeMessage += "If the problem persist, please contact EPPISupport.";
  //                        this._CanChangeDataEntryMode = false;
  //                    });
  //            }

  //        }
  //    }
  //    //this.ShowChangeDataEntry = true;
  //}
  DoChangeDataEntry() {
    if (!this.CurrentNode || !this._CanChangeDataEntryMode) return;
    let Set: ReviewSet = this.CurrentNode as ReviewSet;
    if (!Set) return;
    Set.codingIsFinal = !Set.codingIsFinal;
    console.log("will update DataEntry:", Set, this.CurrentNode);
    this.ReviewSetsEditingService.SaveReviewSet(Set);
    this.CancelActivity();
  }
  HideChangeDataEntry() {
    this.ShowChangeDataEntry = false;
    this.ChangeDataEntryModeMessage = "";
    this._CanChangeDataEntryMode = false;
  }
  CanChangeSelectedCode(): boolean {
    //console.log('CanChangeSelectedCode', this.ActivityPanelName);
    if (this.ActivityPanelName == "") return true;
    else {
      //console.log('CanChangeSelectedCode', this.ActivityPanelName, false);
      return false;
    }
  }
  public get CanEditSelectedNode(): boolean {
    if (!this.CanWrite()) return false;
    else return this.ReviewSetsService.CanEditSelectedNode;
  }
  SetIsSelected(): boolean {
    if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "ReviewSet") return true;
    else return false;
  }
  AttributeIsSelected(): boolean {
    if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "SetAttribute") return true;
    else return false;
  }
  ShowActivity(activityName: string) {
    const oldActivity = this._ActivityPanelName;
    if (oldActivity != "") this.CancelActivity(true);
    if (oldActivity != activityName) this._ActivityPanelName = activityName;
  }
  ShowMovePanel() {
    this._ActivityPanelName = "EditCode";
    this.ShowPanelContext = "MoveCode";
  }
  async CancelActivity(refreshTree: boolean | null = null) {
    console.log("CancelActivity", refreshTree);
    this.ShowChangeDataEntry = false;
    this.ShowDeleteCodeset = false;
    this._appliedCodes = -1;
    this._AllocationsAffected = -1;
    this._ItemsWithIncompleteCoding = -1;
    this._CanChangeDataEntryMode = false;
    this.DestinationDataEntryMode = "";
    this.ChangeDataEntryModeMessage = "";
    this._ActivityPanelName = "";
    //this.treeEditorComponent.ChangeActivityPanelName();
    this.ShowPanelContext = "";
    if (refreshTree) {
      if (this.ReviewSetsService.selectedNode) {
        let IsSet: boolean = this.ReviewSetsService.selectedNode.nodeType == "ReviewSet";
        let Id: number = -1;
        if (IsSet) Id = (this.ReviewSetsService.selectedNode as ReviewSet).set_id;
        else Id = (this.ReviewSetsService.selectedNode as SetAttribute).attribute_id;
        this.ReviewSetsService.selectedNode = null;
        await this.ReviewSetsService.GetReviewSets(false);
        console.log("trying to reselect: ", Id);
        if (IsSet) this.ReviewSetsService.selectedNode = this.ReviewSetsService.FindSetById(Id);
        else this.ReviewSetsService.selectedNode = this.ReviewSetsService.FindAttributeById(Id);
      }
    }
  }
  CodesetTypeChanged(event: Event) {
    let typeId = parseInt((event.target as HTMLOptionElement).value);
    if (isNaN(typeId)) return;
    this.NewSetSelectedTypeId = typeId;
    let current = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
    if (current) {
      this._NewReviewSet.setType = current;
      if (!current.allowComparison) this._NewReviewSet.codingIsFinal = true;
    }
  }
  IsNewSetNameValid() {
    if (this._NewReviewSet.set_name.trim() != "") return true;
    else return false;
  }
  IsNewCodeNameValid() {
    if (this.ActivityPanelName == 'AddCode') {
      if (this._NewCode.attribute_name.trim() != "") return true;
      else return false;
    }
    else return false;
  }
  IsEditingNodeNameValid() {
    if (this.CurrentNode && this.CurrentNode.name.trim().length > 0) return true;
    else return false;
  }

  public get CurrentCodesetDataEntryMode(): string {
    if (!this.CurrentNode) return "";
    let Set: ReviewSet = this.CurrentNode as ReviewSet;
    if (!Set) return "";
    if (Set.codingIsFinal) return "Normal";
    else return "Comparison";
  }
  CreateNewCodeSet() {
    console.log("will create:", this._NewReviewSet);
    if (!this._NewReviewSet.setType || this._NewReviewSet.setType.setTypeId == 0) {
      console.log('looking for set type...');
      let found = this.ReviewSetsEditingService.SetTypes.find(found => found.setTypeId == this.NewSetSelectedTypeId);
      if (found) this._NewReviewSet.setType = found;
      else return;
    }
    this._NewReviewSet.order = this.ReviewSetsService.ReviewSets.length;
    this.ReviewSetsEditingService.SaveNewReviewSet(this._NewReviewSet)
      .then(
        success => {
          //alert("did it");
          let result: ReviewSet = new ReviewSet();
          if (success) result = ReviewSetsService.digestJsonReviewSet(success);
          //if (result.set_id < 1) this.modalService.GenericErrorMessage(ErrMsg);
          this.ReviewSetsService.ReviewSets.push(result);
          if (this.treeEditorComponent) this.treeEditorComponent.RefreshLocalTree();
          this._NewReviewSet = new ReviewSet();
          this.CancelActivity();
          //return result;
        },
        error => {
          //alert("Sorry, creating the new Coding Tool failed.");
          //this.modalService.GenericErrorMessage(ErrMsg);
        });
  }

  CreateNewCode() {
    if (this.CurrentNode) {
      this._NewCode.order = this.CurrentNode.attributes.length;

      if (this.CurrentNode.nodeType == "ReviewSet") {
        this._NewCode.set_id = (this.CurrentNode as ReviewSet).set_id;
        this._NewCode.parent_attribute_id = 0;
      }
      else if (this.CurrentNode.nodeType == "SetAttribute") {
        this._NewCode.set_id = (this.CurrentNode as SetAttribute).set_id;
        this._NewCode.parent_attribute_id = (this.CurrentNode as SetAttribute).attribute_id;
      }
    }
    else {
      this._NewReviewSet.order = 0;
    }
    //console.log("What?", this.CodeTypeSelect);//, this.CodeTypeSelect.selectedOptions, this.CodeTypeSelect.selectedOptions.length);
    console.log("What?", this.CodeTypeSelect, this.CodeTypeSelect.nativeElement.selectedOptions, this.CodeTypeSelect.nativeElement.selectedOptions.length);
    if (this.CodeTypeSelect && this.CodeTypeSelect.nativeElement.selectedOptions && this.CodeTypeSelect.nativeElement.selectedOptions.length > 0) {
      this._NewCode.attribute_type_id = this.CodeTypeSelect.nativeElement.selectedOptions[0].value;
      this._NewCode.attribute_type = this.CodeTypeSelect.nativeElement.selectedOptions[0].text;
    }
    else {
      this._NewCode.attribute_type_id = 1;//non selectable HARDCODED WARNING!
      this._NewCode.attribute_type = "Not selectable(no checkbox)";
    }

    console.log("will create:", this._NewCode, this.CodeTypeSelect);
    this.ReviewSetsEditingService.SaveNewAttribute(this._NewCode)
      .then(
        success => {
          //alert("did it");
          if (success && this.CurrentNode) {
            this.CurrentNode.attributes.push(success);
            if (this.treeEditorComponent) this.treeEditorComponent.RefreshLocalTree();
          }
          //eventual error messages have been shown by the service...
          this._NewCode = new SetAttribute();
          this.CancelActivity();
          //return result;
        },
        error => {
          this.CancelActivity();
          console.log("error saving new code:", error, this._NewCode);
          //alert("Sorry, creating the new Coding Tool failed.");
          //this.modalService.GenericErrorMessage(ErrMsg);
        })
      .catch(
        error => {
          console.log("error(catch) saving new code:", error, this._NewCode);
          this.CancelActivity();
        }
      );
  }
  private _UpdatingCode: SetAttribute | null = null;
  public get UpdatingCode(): SetAttribute | null {

    if (this.ActivityPanelName != 'EditCode') {
      this._UpdatingCode = null;
    }
    else if (this.CurrentNode == null) this._UpdatingCode = null;
    else if (this.CurrentNode.nodeType == "SetAttribute") {
      this._UpdatingCode = this.CurrentNode as SetAttribute;
    }

    //console.log("get UpdatingCode" , this._UpdatingCode);
    return this._UpdatingCode;
  }

  onSubmit(): boolean {
    console.log("ReviewSets Editor onSubmit");
    return false;
  }

  BackToMain() {
    this.ReviewSetsService.GetReviewSets(false);
    this.router.navigate(['Main']);
  }
  ngOnDestroy() {
    if (this.subRedrawTree != null) this.subRedrawTree.unsubscribe();
  }
}
