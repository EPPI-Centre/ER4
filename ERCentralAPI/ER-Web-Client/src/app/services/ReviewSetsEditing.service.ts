import { Inject, Injectable, Output, EventEmitter } from '@angular/core';
import { Observable, of, race } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { iSetType, ReviewSetsService, ReviewSet, iReviewSet, SetAttribute, iAttributeSet, singleNode } from './ReviewSets.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Search } from './search.service';
import { ConfigService } from './config.service';

@Injectable({
    providedIn: 'root',
})

export class ReviewSetsEditingService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        private ReviewSetsService: ReviewSetsService,
      configService: ConfigService
    ) {
      super(configService);
    }
    @Output() PleaseRedrawTheTree = new EventEmitter(); //IMPORTANT! Should be called by all editing API calls that do not return a promise...
    //if an editing API call returns a promise, then what called has the responsibility of making sure changes are propagated.
    //frequently this is done by calling a larger refresh in ReviewSets service.

  private _SearchVisualiseData: any[] = [];
  public get SearchVisualiseData(): any[] {
		return this._SearchVisualiseData;
	}
  public set SearchVisualiseData(searches: any[]) {
		this._SearchVisualiseData = searches;
	}
    private _SetTypes: iSetType[] = [];
    public get SetTypes(): iSetType[]
    {
        return this._SetTypes;
    }
    private _ReviewSets4Copy: ReviewSet[] = [];
    public get ReviewSets4Copy(): ReviewSet[] {
        return this._ReviewSets4Copy;
    }
    public clearReReviewSets4Copy() {
        this._ReviewSets4Copy = [];
    }
    public FetchSetTypes() {
        this._BusyMethods.push("FetchSetTypes");
        this._httpC.get<iSetType[]>(this._baseUrl + 'api/Codeset/SetTypes').subscribe(
            (res) => {
                this._SetTypes = res;
                this.RemoveBusy("FetchSetTypes");
            }
            , error => {
                this.modalService.GenericError(error);
                this.RemoveBusy("FetchSetTypes");}
            
        );
    }
    private _ReadOnlyTemplateReviews: ReadOnlyTemplateReview[] = [];
    public get ReadOnlyTemplateReviews(): ReadOnlyTemplateReview[] {
        return this._ReadOnlyTemplateReviews;
    }
    public FetchReviewTemplates() {
        this._BusyMethods.push("FetchReviewTemplates");
        this._httpC.get<ReadOnlyTemplateReview[]>(this._baseUrl + 'api/Review/GetReadOnlyTemplateReviews').subscribe(
            (res) => {
                this._ReadOnlyTemplateReviews = res;
                this.RemoveBusy("FetchReviewTemplates");
            }
            , error => {
                this.modalService.GenericError(error);
                this.RemoveBusy("FetchReviewTemplates");
            }
        );
    }
    public async SaveReviewSet(rs: ReviewSet) {
        this._BusyMethods.push("SaveReviewSet");
        let rsC: ReviewSetUpdateCommand = {
            ReviewSetId: rs.reviewSetId,
            SetId: rs.set_id,
            AllowCodingEdits: rs.allowEditingCodeset,
            CodingIsFinal: rs.codingIsFinal,
            SetName: rs.set_name,
            setOrder: rs.order,
            setDescription: rs.description,
            usersCanEditURLs: rs.userCanEditURLs,
            SetTypeId: rs.setType ? rs.setType.setTypeId : -1
        }
        //console.log("saving reviewSet via command", rs, rsC);
        this._httpC.post<ReviewSetUpdateCommand>(this._baseUrl + 'api/Codeset/SaveReviewSet', rsC).subscribe(
            data => {
                this.RemoveBusy("SaveReviewSet");
                //this.ItemCodingItemAttributeSaveCommandExecuted.emit(data);
                //this._IsBusy = false;
                this.PleaseRedrawTheTree.emit();
            }, error => {
                this.RemoveBusy("SaveReviewSet");
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
            }
        );
    }
    public SaveNewReviewSet(rs: ReviewSet): Promise<iReviewSet | null> {
        this._BusyMethods.push("SaveNewReviewSet");
        let ErrMsg = "Something went wrong: it appears that the Coding Tool was not saved correctly. \r\n Reloading the review is probably wise. \r\n If the problem persists, please contact EPPISupport.";
        let rsC: ReviewSetUpdateCommand = {
            ReviewSetId: rs.reviewSetId,
            SetId: rs.set_id,
            AllowCodingEdits: rs.allowEditingCodeset,
            CodingIsFinal: rs.codingIsFinal,
            SetName: rs.set_name,
            setOrder: rs.order,
            setDescription: rs.description,
            usersCanEditURLs: rs.userCanEditURLs,
            SetTypeId: rs.setType ? rs.setType.setTypeId : -1
        }
        //console.log("saving reviewSet via command", rs, rsC);
        return this._httpC.post<iReviewSet>(this._baseUrl + 'api/Codeset/ReviewSetCreate', rsC).toPromise()
            .then((res) => { 
                this.RemoveBusy("SaveNewReviewSet"); 
                this.PleaseRedrawTheTree.emit();
                return res; },
            (err) => {
                this.RemoveBusy("SaveNewReviewSet");
                this.modalService.GenericErrorMessage(ErrMsg);
                return null;
            })
            .catch((err) => {
                this.modalService.GenericErrorMessage(ErrMsg);
                this.RemoveBusy("SaveNewReviewSet");
                return null;
            });
    }
    public AttributeOrSetDeleteCheck(SetId: number, AttributeSetId: number): Promise<AttributeSetDeleteWarningCommandResult> {//get how many items have coding in a codeset or section therein
        this._BusyMethods.push("AttributeOrSetDeleteCheck");
        let ErrMsg = "Something went wrong: could not check how many items would be affected. \r\n If the problem persists, please contact EPPISupport.";
        let body: AttributeOrSetDeleteCheckCommandJSON = {
            attributeSetId: AttributeSetId,
            setId: SetId
        };
        const errorRes: AttributeSetDeleteWarningCommandResult = {
            numItems: -1,
            numAllocations: -1
        }

        return this._httpC.post<AttributeSetDeleteWarningCommandResult>(this._baseUrl + 'api/Codeset/AttributeOrSetDeleteCheck', body).toPromise()
            .then(
            (result) => {
                    //console.log("ReviewSetCheckCodingStatus", result);
                    this.RemoveBusy("AttributeOrSetDeleteCheck");
                    return result;
                }
                , (error) => {
                    console.log("AttributeOrSetDeleteCheck Err", error);
                    this.RemoveBusy("AttributeOrSetDeleteCheck");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return errorRes;
                }
            )
            .catch(
                (error) => {
                    console.log("AttributeOrSetDeleteCheck catch", error);
                    this.RemoveBusy("AttributeOrSetDeleteCheck");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return errorRes;
                }
            );
    }
    public ReviewSetDelete(rSet: ReviewSet): Promise<ReviewSetDeleteCommand> {
        this._BusyMethods.push("ReviewSetDelete");
        let ErrMsg = "Something went wrong: could not check the coding status of this set. \r\n If the problem persists, please contact EPPISupport.";
        let command = {
            reviewSetId: rSet.reviewSetId,
            successful: false,
            setId: rSet.set_id,
            order: rSet.order
        };
        console.log(command);
        return this._httpC.post<ReviewSetDeleteCommand>(this._baseUrl + 'api/Codeset/ReviewSetDelete', command).toPromise()
            .then(
                (result) => {
                    this.RemoveBusy("ReviewSetDelete");
                    this.PleaseRedrawTheTree.emit();
                    if (!result.successful) this.modalService.GenericErrorMessage(ErrMsg);
                    return result;
                }
            , (error) => {
                console.log("ReviewSetDelete Err", error);
                this.RemoveBusy("ReviewSetDelete");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return command;
                }
            )
            .catch(
            (error) => {
                console.log("ReviewSetDelete catch", error);
                this.RemoveBusy("ReviewSetDelete");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return command;
                }
            );
    }
    
    public async MoveSetAttribute(attributeSetId: number,
        fromId: number,
        toId: number,
        attributeorder: number) {
        this._BusyMethods.push("MoveSetAttribute");
        let rsC: AttributeSetMoveCommand = {
            FromId: fromId,
            ToId: toId,
            AttributeSetId: attributeSetId,
            attributeOrder: attributeorder
        }
        //console.log("saving reviewSet via command", rs, rsC);
        return this._httpC.post<ReviewSetUpdateCommand>(this._baseUrl + 'api/Codeset/AttributeSetMove', rsC).toPromise().then(
            data => {
                this.RemoveBusy("MoveSetAttribute");
                //this.ItemCodingItemAttributeSaveCommandExecuted.emit(data);
                //this._IsBusy = false;
                //console.log("emit PleaseRedrawTheTree");
                this.PleaseRedrawTheTree.emit();
                return true;
            }, error => {
                this.RemoveBusy("MoveSetAttribute");
                console.log("Error moving Code: ", error);
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
                return false;
            }
        ).catch(
            caught => {
                this.RemoveBusy("MoveSetAttribute");
                console.log("Error moving Code: ", caught);
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
                return false;
            }
        );
    }

    public async MoveReviewSet(reviewSetId: number,
      newSetOrder: number) {
      this._BusyMethods.push("MoveReviewSet");
      let rsC: ReviewSetMoveCommand = {
        ReviewSetId: reviewSetId,
        ReviewSetOrder: newSetOrder
      }
      //console.log("saving reviewSet via command", rs, rsC);
      return this._httpC.post<ReviewSetMoveCommand>(this._baseUrl + 'api/Codeset/ReviewSetMove', rsC).toPromise().then(
        data => {
          this.RemoveBusy("MoveReviewSet");
          this.PleaseRedrawTheTree.emit();
          return true;
        }, error => {
          this.RemoveBusy("MoveReviewSet");
          console.log("Error moving Code: ", error);
          this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
          return false;
        }
      ).catch(
        caught => {
          this.RemoveBusy("MoveSetAttribute");
          console.log("Error moving Code: ", caught);
          this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
          return false;
        }
      );
    }

    CanMoveDown(node: singleNode): boolean {
        if (!this.ReviewSetsService.ReviewSets || this.ReviewSetsService.ReviewSets.length < 1) return false;
        else if (node.nodeType == 'ReviewSet') {
            //console.log("AAAAAAAAA", node);
            if (node.order != this.ReviewSetsService.ReviewSets.length - 1) return true;//even if the set can't be edited, I can still move it up or down...
            else return false;
        }
        else {//this is an attribute, more work needed...
            let SetAtt = node as SetAttribute;
            if (SetAtt) {
                //first of all: is the set editable?
                let MySet = this.ReviewSetsService.FindSetById(SetAtt.set_id);
                if (MySet) {
                    if (MySet.allowEditingCodeset == false) return false;//otherwise do the other checks...
                }
                else {
                    //ugh, shouldn't happen. Return false just in case...
                    return false;
                }
                if (SetAtt.parent == 0) {
                    //att is in the root.
                    if (MySet) {
                        if (SetAtt.order != MySet.attributes.length - 1) return true;
                        else return false;
                    }
                }
                else {
                    //att is inside the tree
                    let MyParent = this.ReviewSetsService.FindAttributeById(SetAtt.parent_attribute_id);
                    if (MyParent) {
                        //console.log("CanMoveDown", SetAtt.order, "PA_ID:" + MyParent.attribute_id, MyParent.attributes.length);
                        if (SetAtt.order != MyParent.attributes.length - 1) return true;
                        else return false;
                    }
                }
            }
        }
        return false;
    }
    CanMoveUp(node: singleNode): boolean {
        if (!this.ReviewSetsService.ReviewSets || this.ReviewSetsService.ReviewSets.length < 1) return false;
        else if (node.nodeType == 'ReviewSet') {
            if (node.order != 0 && this.ReviewSetsService.ReviewSets.length > 1) return true;
            else return false;
        }
        else {//this is an attribute, more work needed...
            let SetAtt = node as SetAttribute;
            if (SetAtt) {
                let MySet = this.ReviewSetsService.FindSetById(SetAtt.set_id);
                if (MySet) {
                    if (MySet.allowEditingCodeset == false) return false;//otherwise do the other checks...
                }
                else {
                    //ugh, shouldn't happen. Return false just in case...
                    return false;
                }
                if (SetAtt.parent == 0) {
                    //att is in the root.
                    let MySet = this.ReviewSetsService.FindSetById(SetAtt.set_id);
                    if (MySet) {
                        // console.log(MySet, SetAtt);
                        if (SetAtt.order != 0 && MySet.attributes.length > 1) return true;
                        else return false;
                    }
                }
                else {
                    //att is inside the tree
                    let MyParent = this.ReviewSetsService.FindAttributeById(SetAtt.parent_attribute_id);
                    if (MyParent) {
                        if (SetAtt.order != 0 && MyParent.attributes.length > 1) return true;
                        else return false;
                    }
                }
            }
        }
        return false;
    }

    IsACode(node: singleNode): boolean {
      if (node.nodeType == 'ReviewSet') {
        return false;
      }
      else {
        return true;
      }
    }

    async MoveCode(node: singleNode) {
      if (node.nodeType == 'ReviewSet') {
        // we shouldn't be here
      }
      else {
        let MyAtt = node as SetAttribute;
        if (MyAtt) {


          //await this.MoveUpAttributeFull(MyAtt);
        }
      }
    }


    async MoveUpNode(node: singleNode) {
        if (node.nodeType == 'ReviewSet') {
            let MySet = node as ReviewSet;
            if (MySet) await this.MoveUpSet(MySet);
        }
        else {
            let MyAtt = node as SetAttribute;
            if (MyAtt) {
                await this.MoveUpAttribute(MyAtt);
            }
        }
    }
    async MoveDownNode(node: singleNode) {
        if (node.nodeType == 'ReviewSet') {
            let MySet = node as ReviewSet;
            if (MySet) await this.MoveDownSet(MySet);
        }
        else {
            let MyAtt = node as SetAttribute;
            if (MyAtt) await this.MoveDownAttribute(MyAtt);
        }
    }
    async MoveUpNodeFull(node: singleNode) {
      if (node.nodeType == 'ReviewSet') {
        let MySet = node as ReviewSet;
        if (MySet) await this.MoveUpSetFull(MySet);
      }
      else {
        let MyAtt = node as SetAttribute;
        if (MyAtt) {
          await this.MoveUpAttributeFull(MyAtt);
        }
      }
    }
    async MoveDownNodeFull(node: singleNode) {
      if (node.nodeType == 'ReviewSet') {
        let MySet = node as ReviewSet;
        if (MySet) await this.MoveDownSetFull(MySet);
      }
      else {
        let MyAtt = node as SetAttribute;
        if (MyAtt) await this.MoveDownAttributeFull(MyAtt);
      }
    }
    private async MoveDownSet(rs: ReviewSet) {
        let index = this.ReviewSetsService.ReviewSets.findIndex(found => found.set_id == rs.set_id);
        if (index == -1 || index == this.ReviewSetsService.ReviewSets.length - 1) {
            //oh! should not happen... do nothing?
            return;
        }
        let swapper: ReviewSet = this.ReviewSetsService.ReviewSets[index + 1];
        rs.order++;
        await this.SaveReviewSet(rs);
        if (swapper) {
            swapper.order--;
            await this.SaveReviewSet(swapper);
        }
        //now sort by order;
        this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
            return s1.order - s2.order;
        });
    }
    private async MoveDownSetFull(rs: ReviewSet) {
      let index = this.ReviewSetsService.ReviewSets.findIndex(found => found.set_id == rs.set_id);
      if (index == -1 || index == this.ReviewSetsService.ReviewSets.length - 1) {
        //oh! should not happen... do nothing?
        return;
      }

      let lastPosition = this.ReviewSetsService.ReviewSets.length - 1;

      // save of the order of the selected set to the first position in the database
      await this.MoveReviewSet(rs.reviewSetId, lastPosition);

      // change of the order of the local values so it updats on the screen
      rs.order = lastPosition;

      let set: ReviewSet;
      // decrement 'order' for everything from index + 1 to the end
      for (let i = index + 1; i < this.ReviewSetsService.ReviewSets.length; i++) {
        set = this.ReviewSetsService.ReviewSets[i];
        set.order--
      }

      //now sort by order;
      this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
        return s1.order - s2.order;
      });
    }
    private async MoveUpSet(rs: ReviewSet) {
        //console.log("before:", rs, rs.order);
        let index = this.ReviewSetsService.ReviewSets.findIndex(found => found.set_id == rs.set_id);
        if (index <= 0) {
            //oh! should not happen... do nothing?
            return;
        }
        let swapper: ReviewSet = this.ReviewSetsService.ReviewSets[index - 1];
        //console.log("mid1:", rs, rs.order);
        rs.order = rs.order - 1;
        //console.log("mid2 :", rs, rs.order);
        await this.SaveReviewSet(rs);
        if (swapper) {
            swapper.order = swapper.order + 1;
            await this.SaveReviewSet(swapper);
        }
        //now sort by order;
        this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
            return s1.order - s2.order;
        });
    }
  private async MoveUpSetFull(rs: ReviewSet) {
      let index = this.ReviewSetsService.ReviewSets.findIndex(found => found.set_id == rs.set_id);
      if (index <= 0) {
        //oh! should not happen... do nothing?
        return;
      }

      // save of the order of the selected set to the first position in the database
      await this.MoveReviewSet(rs.reviewSetId, 0);

      // change the order of the local values so it updates on the screen
      rs.order = 0;

      let set: ReviewSet;
      // increment 'order' for everything from 0 to index - 1
      for (let i = 0; i < index; i++) {
        set = this.ReviewSetsService.ReviewSets[i];
        set.order++;
      }

      //now sort by order on the screen;
      this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
        return s1.order - s2.order;
      });
    }
    private async MoveDownAttribute(Att: SetAttribute) {
        //silently does nothing if data doesn't make sense
        let swapper: SetAttribute | null = null;
        let SortingParent: ReviewSet | SetAttribute | null = null;//used to update what user sees
        let index: number = -1;
        if (Att.parent_attribute_id == 0) {
            let Set: ReviewSet | null = this.ReviewSetsService.FindSetById(Att.set_id);
            if (!Set) return;
            index = Set.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
            if (index < 0) {
                //oh! should not happen... do nothing?
                console.log("MoveDownAttribute fail 1", index);
                return;
            }
            swapper = Set.attributes[index + 1];
            SortingParent = Set;
        }
        else {
            let Parent: SetAttribute | null = this.ReviewSetsService.FindAttributeById(Att.parent_attribute_id);
            if (!Parent) return;
            index = Parent.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
            if (index < 0) {
                //oh! should not happen... do nothing?
                return;
            }
            swapper = Parent.attributes[index + 1];
            if (!swapper) return;
            SortingParent = Parent;
        }
        if (!swapper || index < 0) return;
        await this.CheckChildrenOrder(SortingParent as singleNode);
        //all is good: do changes
        swapper.order = swapper.order - 1;
        Att.order = Att.order + 1;
        await this.MoveSetAttribute(Att.attributeSetId, Att.parent_attribute_id, Att.parent_attribute_id, Att.order);
        SortingParent.attributes.sort((s1, s2) => {
            return s1.order - s2.order;
        });
    }

    private async MoveUpAttribute(Att: SetAttribute) {
        let swapper: SetAttribute | null = null;
        let SortingParent: ReviewSet | SetAttribute | null = null;
        let index: number = -1;
        if (Att.parent_attribute_id == 0) {
            let Set: ReviewSet | null = this.ReviewSetsService.FindSetById(Att.set_id);
            if (!Set) return;
            index = Set.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
            if (index <= 0) {
                //oh! should not happen... do nothing?
                return;
            }
            swapper = Set.attributes[index - 1];
            SortingParent = Set;
        }
        else {
            let Parent: SetAttribute | null = this.ReviewSetsService.FindAttributeById(Att.parent_attribute_id);
            if (!Parent) return;
            index = Parent.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
            if (index <= 0) {
                //oh! should not happen... do nothing?
                return;
            }
            swapper = Parent.attributes[index - 1];
            if (!swapper) return;
            SortingParent = Parent;
        }
        if (!swapper || index < 0) return;
        await this.CheckChildrenOrder(SortingParent as singleNode);
        //all is good: do changes
        swapper.order = swapper.order + 1;
        Att.order = Att.order - 1;
        await this.MoveSetAttribute(Att.attributeSetId, Att.parent_attribute_id, Att.parent_attribute_id, Att.order);
        SortingParent.attributes.sort((s1, s2) => {
            return s1.order - s2.order;
        });
    }

  private async MoveDownAttributeFull(Att: SetAttribute) {
    let SortingParent: ReviewSet | SetAttribute | null = null;
    let index: number = -1;
    if (Att.parent_attribute_id == 0) { // just below the root so no parent
      let Set: ReviewSet | null = this.ReviewSetsService.FindSetById(Att.set_id);
      if (!Set) return;
      index = Set.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
      if (index < 0) {
        //oh! should not happen... do nothing?
        console.log("MoveDownAttribute fail 1", index);
        return;
      }
      SortingParent = Set;
    }
    else {
      let Parent: SetAttribute | null = this.ReviewSetsService.FindAttributeById(Att.parent_attribute_id);
      if (!Parent) return;
      index = Parent.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
      if (index < 0) {
        //oh! should not happen... do nothing?
        return;
      }
      SortingParent = Parent;
    }

    let endIndex = SortingParent.attributes.length - 1;

    await this.CheckChildrenOrder(SortingParent as singleNode);
    //all is good: do changes

    // update the order in the database
    let tmpAtt: SetAttribute;
    tmpAtt = SortingParent.attributes[index];
    await this.MoveSetAttribute(tmpAtt.attributeSetId, tmpAtt.parent_attribute_id, tmpAtt.parent_attribute_id, endIndex);

    // decrement 'order' for everything from index + 1 to the end so the screen updates
    for (let i = index + 1; i < SortingParent.attributes.length; i++) {
      SortingParent.attributes[i].attribute_order = i - 1;
      SortingParent.attributes[i].order = i - 1;
    }
    SortingParent.attributes[index].attribute_order = endIndex;
    SortingParent.attributes[index].order = endIndex;

    // now sort by attribute_order
    SortingParent.attributes.sort((s1, s2) => {
      return s1.attribute_order - s2.attribute_order;
    });

    // to redraw the chevrons
    await this.CheckChildrenOrder(SortingParent as singleNode);
  }

  private async MoveUpAttributeFull(Att: SetAttribute) {
      let SortingParent: ReviewSet | SetAttribute | null = null;//used to update what user sees
      let index: number = -1;
      if (Att.parent_attribute_id == 0) {
        let Set: ReviewSet | null = this.ReviewSetsService.FindSetById(Att.set_id);
        if (!Set) return;
        index = Set.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
        if (index <= 0) {
          //oh! should not happen... do nothing?
          return;
        }
        SortingParent = Set;
      }
      else {
        let Parent: SetAttribute | null = this.ReviewSetsService.FindAttributeById(Att.parent_attribute_id);
        if (!Parent) return;
        index = Parent.attributes.findIndex(found => found.attribute_id == Att.attribute_id);
        if (index <= 0) {
          //oh! should not happen... do nothing?
          return;
        }
        SortingParent = Parent;
      }

      await this.CheckChildrenOrder(SortingParent as singleNode);

      let tmpAtt: SetAttribute;
      tmpAtt = SortingParent.attributes[index];
      await this.MoveSetAttribute(tmpAtt.attributeSetId, tmpAtt.parent_attribute_id, tmpAtt.parent_attribute_id, 0);

      // increment 'order' for everything from from 0 to index - 1
      for (let i = 0; i < index; i++) {
        SortingParent.attributes[i].attribute_order = i + 1;
        SortingParent.attributes[i].order = i + 1;
      }
      SortingParent.attributes[index].attribute_order = 0;
      SortingParent.attributes[index].order = 0;

      // now sort by attribute_order
      SortingParent.attributes.sort((s1, s2) => {
        return s1.attribute_order - s2.attribute_order;
      });

      // to redraw the chevrons
      await this.CheckChildrenOrder(SortingParent as singleNode);
    }

    public async MoveSetAttributeInto(MovingAtt: SetAttribute, Destination: singleNode) {
        //Destination might be a ReviewSet or a SetAttribute!
        //first, we'll find Just the data needed to call the API, then we'll do changes locally IF API call succeeds...
        let fromId: number = MovingAtt.parent_attribute_id;
        let toId: number = 0; //zero if in the root, otherwise the attribute IDs of the parent. (fromId and toId)
        let order = 0;
        let dA: SetAttribute | null = null;
        let dS: ReviewSet | null = null;
        let fA: SetAttribute | null = null;
        let fS: ReviewSet | null = null;
        //we will try to do something, so we'll mark this service as busy (twice) - the one in here is because we want to let the reorg of the tree to happen while still busy
        this._BusyMethods.push("MoveSetAttributeInto");
        if (Destination.nodeType == "SetAttribute") {
            //destination is not the root.
            //console.log("move to a SetAttribute");
            dA = this.ReviewSetsService.FindAttributeById((Destination as SetAttribute).attribute_id);
            
            if (dA) {
                await this.CheckChildrenOrder(dA as singleNode);
                toId = dA.attribute_id;
                dA.attributes.sort((s1, s2) => {
                    return s1.order - s2.order;
                });
                if (dA.attributes.length == 0) order = 0;
                else order = dA.attributes[dA.attributes.length - 1].order + 1;
            }
            else {
                console.log("ERROR! Didn't find the destination (SetAttribute).");
                return false;
            }
        } else {
            //destination is the root (a ReviewSet).
            //console.log("move to the root");
            dS = this.ReviewSetsService.FindSetById(Destination.set_id);
            if (dS) {
                await this.CheckChildrenOrder(dS as singleNode);
                dS.attributes.sort((s1, s2) => {
                    return s1.order - s2.order;
                });
                if (dS.attributes.length == 0) order = 0;
                else order = dS.attributes[dS.attributes.length - 1].order + 1;
            }
            else {
                console.log("ERROR! Didn't find the destination (root).");
                return false;
            }
        }
        
        if (MovingAtt.parent_attribute_id == 0) {
            //coming from the root
            fS = this.ReviewSetsService.FindSetById(MovingAtt.set_id);
            if (fS) await this.CheckChildrenOrder(fS as singleNode);
        }
        else {
            //coming from a SetAttribute
            fA = this.ReviewSetsService.FindAttributeById(MovingAtt.parent_attribute_id);
            if (fA) await this.CheckChildrenOrder(fA as singleNode);
        }
        let result = await this.MoveSetAttribute(MovingAtt.attributeSetId, fromId, toId, order);
        if (result) {
            
            //we have 4 cases where we can do something (from and to have been found), 3 of these make sense
            if (fS != null && dS != null) {
                //moving from root to root, shouldn't really happen!
                //not sure what's best here, we'll return failure
                this.RemoveBusy("MoveSetAttributeInto");
                return false;
            }
            else if (fS != null && dA != null) {
                //moving from root to somewhere inside
                let ind = fS.attributes.findIndex(found => found.attribute_id == MovingAtt.attribute_id);
                if (ind == -1) {
                    this.RemoveBusy("MoveSetAttributeInto");
                    return false;
                }
                else {
                    fS.attributes.splice(ind, 1);//remove code from where it was
                    if (ind < fS.attributes.length) {
                        for (let ii = ind; ii < fS.attributes.length; ii++) {
                            fS.attributes[ii].order--;
                        }
                    }
                    fS.attributes.sort((s1, s2) => {
                        return s1.order - s2.order;
                    });
                }
                MovingAtt.parent_attribute_id = toId;
                MovingAtt.order = order;
                dA.attributes.push(MovingAtt);
                dA.attributes.sort((s1, s2) => {
                    return s1.order - s2.order;
                });
            }
            else if (fA != null && dS != null) {
                //moving from somewhere inside to root
                let ind = fA.attributes.findIndex(found => found.attribute_id == MovingAtt.attribute_id);
                if (ind == -1) {
                    this.RemoveBusy("MoveSetAttributeInto");
                    return false;
                }
                else {
                    fA.attributes.splice(ind, 1);
                    if (ind < fA.attributes.length) {
                        for (let ii = ind; ii < fA.attributes.length; ii++) {
                            fA.attributes[ii].order--;
                        }
                    }
                    fA.attributes.sort((s1, s2) => {
                        return s1.order - s2.order;
                    });
                }
                MovingAtt.parent_attribute_id = toId;
                MovingAtt.order = order;
                dS.attributes.push(MovingAtt);
                dS.attributes.sort((s1, s2) => {
                    return s1.order - s2.order;
                });
            }
            else if (fA != null && dA != null) {
                //moving from somewhere inside, to somewhere else, still inside
                let ind = fA.attributes.findIndex(found => found.attribute_id == MovingAtt.attribute_id);
                if (ind == -1) {
                    this.RemoveBusy("MoveSetAttributeInto");
                    return false;
                }
                else {
                    fA.attributes.splice(ind, 1);
                    if (ind < fA.attributes.length) {
                        for (let ii = ind; ii < fA.attributes.length; ii++) {
                            console.log("reducing order for:", fA.attributes[ii].name, fA.attributes[ii].order);
                            fA.attributes[ii].order--;
                        }
                    }
                    fA.attributes.sort((s1, s2) => {
                        return s1.order - s2.order;
                    });
                }
                MovingAtt.parent_attribute_id = toId;
                MovingAtt.order = order;
                dA.attributes.push(MovingAtt);
                dA.attributes.sort((s1, s2) => {
                    return s1.order - s2.order;
                });
                let tmp: SetAttribute[] = [];
                dA.attributes = dA.attributes.concat(tmp);
            }
            else {
                //didn't find a reference to either origin or destination, can't do this!
                this.RemoveBusy("MoveSetAttributeInto");
                return false;
            }
            this.PleaseRedrawTheTree.emit();
            this.RemoveBusy("MoveSetAttributeInto");
            return true;
        }
        else {
            //the API call failed (returned false)
            this.RemoveBusy("MoveSetAttributeInto");
            return false;
        }
    }

    async CheckChildrenOrder(node: singleNode) {
        let i: number = 0;
        let changedSomething: boolean = false;
        for (let sa of node.attributes) {
            if (sa.order != i) {
                sa.order = i;
                changedSomething = true;
                //do something to save change!
                let res: boolean = await this.UpdateAttribute(sa as SetAttribute);
                if (!res) break;
                //recursive CheckAttributeSet of attributes?
            }
            i++;
        }
        if (changedSomething) {
            //for sanity, sort by order;
            this.ReviewSetsService.ReviewSets = this.ReviewSetsService.ReviewSets.sort((s1, s2) => {
                return s1.order - s2.order;
            });
            this.PleaseRedrawTheTree.emit();
        }
    }


    public ReviewSetCheckCodingStatus(SetId: number): Promise<number> {//used to check how many incomplete items are here before moving to "normal" data entry
        this._BusyMethods.push("ReviewSetCheckCodingStatus");
        let ErrMsg = "Something went wrong: could not check the coding status of this set. \r\n If the problem persists, please contact EPPISupport.";
        let body = JSON.stringify({ Value: SetId });
        return this._httpC.post<number>(this._baseUrl + 'api/Codeset/ReviewSetCheckCodingStatus', body).toPromise()
            .then(
                (result) => {
                    //console.log("ReviewSetCheckCodingStatus", result);
                    this.RemoveBusy("ReviewSetCheckCodingStatus");
                    return result;
                }
                , (error) => {
                    console.log("ReviewSetCheckCodingStatus Err", error);
                    this.RemoveBusy("ReviewSetCheckCodingStatus");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return -1;
                }
            )
            .catch(
                (error) => {
                    console.log("ReviewSetCheckCodingStatus catch", error);
                    this.RemoveBusy("ReviewSetCheckCodingStatus");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return -1;
                }
            );
    }
	public SaveNewAttribute(Att: SetAttribute): Promise<SetAttribute | null> {
		//alert(JSON.stringify(Att));
        let ErrMsg = "Something went wrong: it appears that the Code was not saved correctly. \r\n Reloading the review is probably wise. \r\n If the problem persists, please contact EPPISupport.";
        if (Att.set_id < 1) {
            //bad! can't do this...
            this.modalService.GenericErrorMessage(ErrMsg);
            return new Promise<null>((resolve, reject) => { setTimeout(reject, 1, null) });
        }
        this._BusyMethods.push("SaveNewAttribute");
        let Data: Attribute4Saving = new Attribute4Saving();
        Data.attributeType = Att.attribute_type;
        Data.attributeTypeId = Att.attribute_type_id;
        Data.attributeName = Att.attribute_name;
        Data.contactId = this.ReviewerIdentityService.reviewerIdentity.userId;
        Data.attributeSetDescription = Att.attribute_set_desc;
        Data.parentAttributeId = Att.parent_attribute_id;
        Data.setId = Att.set_id;
        Data.attributeOrder = Att.order;
        //console.log("saving reviewSet via command", rs, rsC);
        return this._httpC.post<iAttributeSet>(this._baseUrl + 'api/Codeset/AttributeCreate', Data).toPromise()
            .then((res) => { 
                this.RemoveBusy("SaveNewAttribute"); 
                Att.attribute_id = res.attributeId;
                Att.attributeSetId = res.attributeSetId;
                this.PleaseRedrawTheTree.emit();
                return Att; 
            },
                (err) => {
                    this.RemoveBusy("SaveNewAttribute");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return null;
                })
            .catch((err) => {
                this.modalService.GenericErrorMessage(ErrMsg);
                this.RemoveBusy("SaveNewAttribute");
                return null;
            });
    }
    public UpdateAttribute(Att: SetAttribute): Promise<boolean> {
        this._BusyMethods.push("UpdateAttribute");
        let ErrMsg = "Something went wrong: it appears that the Code was not saved correctly. \r\n Reloading the review is probably wise. \r\n If the problem persists, please contact EPPISupport.";
        if (Att.set_id < 1) {
            //bad! can't do this...
            this.modalService.GenericErrorMessage(ErrMsg);
            this.RemoveBusy("UpdateAttribute");
            return new Promise<boolean>((resolve, reject) => { setTimeout(reject, 1, false) });
        }
        
        let Data: Attribute4Saving = new Attribute4Saving();
        Data.attributeId = Att.attribute_id;
        Data.attributeSetId = Att.attributeSetId;
        Data.attributeType = Att.attribute_type;
        Data.attributeTypeId = Att.attribute_type_id;
        Data.attributeName = Att.attribute_name;
        //Data.contactId = this.ReviewerIdentityService.reviewerIdentity.userId;
        Data.attributeSetDescription = Att.attribute_set_desc;
        Data.parentAttributeId = Att.parent_attribute_id;
        Data.setId = Att.set_id;
        Data.attributeOrder = Att.order;
        Data.extType = Att.extType;
        Data.extURL = Att.extURL;
        //console.log("saving reviewSet via command", rs, rsC);
        return this._httpC.post<boolean>(this._baseUrl + 'api/Codeset/AttributeUpdate', Data).toPromise()
            .then((res) => {
                this.RemoveBusy("UpdateAttribute");
                this.PleaseRedrawTheTree.emit();
                return res;
            },
            (err) => {
                console.log("Error Updating Attribute:", err);
                    this.RemoveBusy("UpdateAttribute");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return false;
                })
            .catch((err) => {
                console.log("Error Updating Attribute (catch):", err);
                this.modalService.GenericErrorMessage(ErrMsg);
                this.RemoveBusy("UpdateAttribute");
                return false;
            });
	}
	public CreateVisualiseCodeSet(visualiseTitle: string, visualiseSearchId: number,
		attribute_id: number, set_id: number): Promise<ClassifierCommand> {
		this._BusyMethods.push("CreateVisualiseCodeSet");
		let command: ClassifierCommand = new ClassifierCommand();

		command.attributeId = attribute_id;
		command.searchId = visualiseSearchId;
		command.searchName = visualiseTitle;
		command.setId = set_id;
				
		return this._httpC.post<ClassifierCommand>(this._baseUrl + 'api/CodeSet/CreateVisualiseCodeSet', command)
			.toPromise().then(

				(result) => {
					this.RemoveBusy("CreateVisualiseCodeSet");
					return result;
				},
				error => {

					this.RemoveBusy("CreateVisualiseCodeSet");
					this.modalService.GenericError(error);
					return command;

				}).catch(

					(error) => {
						console.log("ReviewSetCopy catch", error);
						this.RemoveBusy("ReviewSetCopy");
						this.modalService.GenericErrorMessage(error);
						return command;
					}
		);

	}

	public CreateVisualiseData(searchId: number): any[] {

		this._BusyMethods.push("CreateVisualiseData");
		let body = JSON.stringify({ searchId: searchId });

		this._httpC.post<any[]>(this._baseUrl + 'api/SearchList/CreateVisualiseData', body)
			.subscribe(result => {

				this.SearchVisualiseData = result;
				this.RemoveBusy("CreateVisualiseData");
				return result;

			},
				error => {
					this.RemoveBusy("CreateVisualiseData");
					this.modalService.GenericError(error);
					return [];
				}
		);

		return this.SearchVisualiseData;
	}

    public SetAttributeDelete(Att: SetAttribute): Promise<AttributeDeleteCommand> {
        this._BusyMethods.push("SetAttributeDelete");
        let ErrMsg = "Something went wrong: could not check the coding status of this code (and children). \r\n If the problem persists, please contact EPPISupport.";
        let command = {
            attributeSetId: Att.attributeSetId,
            attributeId: Att.attribute_id,
            parentAttributeId: Att.parent_attribute_id,
            attributeOrder: Att.order,
            successful: false
        };
        console.log(command);
        return this._httpC.post<AttributeDeleteCommand>(this._baseUrl + 'api/Codeset/AttributeDelete', command).toPromise()
            .then(
                (result) => {
                    this.RemoveBusy("SetAttributeDelete");
                    this.PleaseRedrawTheTree.emit();
                    if (!result.successful) this.modalService.GenericErrorMessage(ErrMsg);
                    return result;
                }
                , (error) => {
                    console.log("SetAttributeDelete Err", error);
                    this.RemoveBusy("SetAttributeDelete");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return command;
                }
            )
            .catch(
                (error) => {
                    console.log("SetAttributeDelete catch", error);
                    this.RemoveBusy("SetAttributeDelete");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return command;
                }
            );
    }

	public ReviewSetCopy(ReviewSetId: number, Order: number): Promise<ReviewSetCopyCommand>{
        this._BusyMethods.push("ReviewSetCopy");
        let ErrMsg = "Something went wrong: could not copy a Coding Tool. \r\n If the problem persists, please contact EPPISupport.";
        let command = new ReviewSetCopyCommand();
        command.order = Order;
        command.reviewSetId = ReviewSetId;
        console.log("ReviewSetCopy:", command);
        return this._httpC.post<ReviewSetCopyCommand>(this._baseUrl + 'api/Codeset/ReviewSetCopy', command).toPromise()
            .then(
                (result) => {
                    this.RemoveBusy("ReviewSetCopy");
                    if (!result) this.modalService.GenericErrorMessage(ErrMsg);
                    //console.log("I am returning this:", result);

                    return result;
                }
                , (error) => {
                    console.log("ReviewSetCopy Err", error);
                    this.RemoveBusy("ReviewSetCopy");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    command.reviewSetId = command.reviewSetId * -1;//signal it didn't work
                    return command;
                }
            )
            .catch(
                (error) => {
                    console.log("ReviewSetCopy catch", error);
                    this.RemoveBusy("ReviewSetCopy");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    command.reviewSetId = command.reviewSetId * -1;//signal it didn't work
                    return command;
                }
            );
    }
    public async ImportReviewTemplate(template: ReadOnlyTemplateReview) {
        if (!template || !template.reviewSetIds || template.reviewSetIds.length < 1) return; //nothing to be done!
        else {
            this._BusyMethods.push("ImportReviewTemplate");//gets removed in interimImportReviewTemplate, if all goes well...
            let ord: number = this.ReviewSetsService.ReviewSets.length;
            const rsid = template.reviewSetIds[0];
            await this.ReviewSetCopy(rsid, ord).then(
                async (value) => {
                    //console.log("ImportReviewTemplate", value);
                    await this.interimImportReviewTemplate(template, value, 0);
                    return;
                },
                (reject) => {
                    console.log("ImportReviewTemplate rejected!", reject);
                    this.RemoveBusy("ImportReviewTemplate");
                }
            ).catch(
                (error) => {
                    this.RemoveBusy("ImportReviewTemplate");
                    console.log("ImportReviewTemplate catch", error);
                }
            );
        }
    }
    private async interimImportReviewTemplate(template: ReadOnlyTemplateReview, rscComm: ReviewSetCopyCommand, currentInd: number) {
        //console.log("interimImportReviewTemplate", template, rscComm, currentInd);
        if (rscComm.reviewSetId < 1) {
            //an error happened (error was shown), stop here
            this.ReviewSetsService.GetReviewSets();
            this.RemoveBusy("ImportReviewTemplate");
            return;
        }
        else {
            currentInd = currentInd + 1;
            if (currentInd >= template.reviewSetIds.length) {
                //all done, stop here
                this.ReviewSetsService.GetReviewSets();
                this.RemoveBusy("ImportReviewTemplate");
                console.log("Importing codesets completed.");
                return;
            }
            else {
                const rsid = template.reviewSetIds[currentInd];
                await this.ReviewSetCopy(rsid, rscComm.order+1).then(
                    async (value) => {//RECURSION ALERT!
                        await this.interimImportReviewTemplate(template, value, currentInd);
                    }
                );
            }
        }
    }

    public FetchReviewSets4Copy(fetchPrivateSets: boolean) {
        this._BusyMethods.push("FetchReviewSets4Copy");
        let body = JSON.stringify({ Value: fetchPrivateSets });
        this._httpC.post<iReviewSet[]>(this._baseUrl + 'api/Codeset/GetReviewSetsForCopying', body).subscribe(
            (res) => {
                this._ReviewSets4Copy = ReviewSetsService.digestJSONarray(res);
                this.RemoveBusy("FetchReviewSets4Copy");
            }
            , error => {
                this.modalService.GenericError(error);
                this.RemoveBusy("FetchReviewSets4Copy");
            }
        );
    }
    public PerformClusterCommand(command: PerformClusterCommand) {
        this._BusyMethods.push("PerformClusterCommand");
        command.reviewSetIndex = this.ReviewSetsService.ReviewSets.length;
        this._httpC.post(this._baseUrl + 'api/Codeset/PerformClusterCommand', command).subscribe(
            () => {
                this.RemoveBusy("PerformClusterCommand");
                this.ReviewSetsService.GetReviewSets();
            }
            , error => {
                this.modalService.GenericError(error);
                this.RemoveBusy("PerformClusterCommand");
            }
        );
	}
	
	public RandomlyAssignCodeToItem(assignParameters: PerformRandomAllocateCommand) {

		// is there a need for busy methods here I would say yes...
		this._BusyMethods.push("RandomlyAssignCodeToItem");

		this._httpC.post<PerformRandomAllocateCommand>(this._baseUrl +
			'api/Codeset/PerformRandomAllocate', assignParameters)
			.subscribe(() => {

				// do not want to change the below but it seems
				// to return an array here
				this.ReviewSetsService.GetReviewSets();
				this.RemoveBusy("RandomlyAssignCodeToItem");

			},
				error => {
					this.modalService.GenericError(error);
					this.RemoveBusy("RandomlyAssignCodeToItem");
				}
				, () => {
					this.RemoveBusy("RandomlyAssignCodeToItem");
				}
			);
    }
    public async GetChangeDataEntryMessage(Set: ReviewSet, screeningCodeSetId: number): Promise<ChangeDataEntryMessage>{
        let res: ChangeDataEntryMessage = new ChangeDataEntryMessage();
            if (Set) {
                if (Set.set_id == screeningCodeSetId) {
                    res.DestinationDataEntryMode = "";
                    res.ChangeDataEntryModeMessage = "This set is your current Screening Set (used for Priority Screening).";
                    res.ChangeDataEntryModeMessage += "\r\nChanging the data entry mode would require to review/update the Priority Screening settings.";
                    res.ChangeDataEntryModeMessage += "\r\nUnfortuately this feature is not currently implemented in the current App.";
                    res.ChangeDataEntryModeMessage += "\r\nTo apply this change please use the full (Silverlight) version or EPPI-Reviewer 4.";
                    res.DestinationDataEntryMode = "";
                    res.CanChangeDataEntryMode = false;
                }
                else if (Set.codingIsFinal) {//moving to comparison data entry, easy!
                    res.DestinationDataEntryMode = "Comparison";
                    res.ChangeDataEntryModeMessage = "Are you sure you want to change to 'Comparison' data entry?";
                    res.ChangeDataEntryModeMessage += "\r\nThis implies that you will have multiple users coding the same item using this Coding Tool and then reconciling the disagreements.";
                    res.ChangeDataEntryModeMessage += "\r\nPlease ensure you have read the manual to check the implications of this.";
                    res.ItemsWithIncompleteCoding = 0;
                    res.CanChangeDataEntryMode = true;
                }
                else {//moving to normal data entry, need to check "troublesome items"
                    res.DestinationDataEntryMode = "Normal";
                    res.ChangeDataEntryModeMessage = "";
                    await this.ReviewSetCheckCodingStatus(Set.set_id).then(
                        success => {
                            //alert("did it");
                            res.ItemsWithIncompleteCoding = success;
                            if (res.ItemsWithIncompleteCoding > 0) {
                                res.ChangeDataEntryModeMessage = "You are about to change your data entry method to 'Normal', ";
                                res.ChangeDataEntryModeMessage += "but there are '" + res.ItemsWithIncompleteCoding + "' items that should be completed before you proceed. ";
                                res.ChangeDataEntryModeMessage += "You can view these incomplete items from the 'Review Home' screen.";
                                res.CanChangeDataEntryMode = true;
                            }
                            else if (res.ItemsWithIncompleteCoding == 0) {
                                res.ChangeDataEntryModeMessage = "You are about to change your data entry method to 'Normal'. \nThere are no potential data conflicts so it is safe to proceed.";
                                res.CanChangeDataEntryMode = true;
                            }
                            else {//error in the service, returned -1
                                res.ChangeDataEntryModeMessage = "Sorry, could not check coding status, thus, you should change the data entry mode. ";
                                res.ChangeDataEntryModeMessage += "If the problem persist, please contact EPPISupport.";
                                res.CanChangeDataEntryMode = false;
                            }
                        },
                        error => {
                            console.log("ERROR IN: ShowChangeDataEntryClicked API result", error);
                            res.ChangeDataEntryModeMessage = "Sorry, could not check coding status, thus, you should change the data entry mode. ";
                            res.ChangeDataEntryModeMessage += "If the problem persist, please contact EPPISupport.";
                            res.CanChangeDataEntryMode = false;
                        });
                }

            }
        return res;
        //this.ShowChangeDataEntry = true;
    }

}
export interface ReviewSetUpdateCommand
    //(int reviewSetId, int setId, bool allowCodingEdits, bool codingIsFinal, string setName, int SetOrder, string setDescription)
{
    ReviewSetId: number;
    SetId: number;
    AllowCodingEdits: boolean;
    CodingIsFinal: boolean;
    SetName: string;
    setOrder: number;
    setDescription: string;
    SetTypeId: number;
    usersCanEditURLs: boolean;
}

export interface AttributeSetMoveCommand {
    FromId: number;
    ToId: number;
    AttributeSetId: number;
    attributeOrder: number;
}

export interface ReviewSetMoveCommand {
  ReviewSetId: number;
  ReviewSetOrder: number;
}

export interface ReviewSetDeleteCommand {
    reviewSetId: number;
    successful: boolean;
    setId: number;
    order: number;
}
export interface AttributeDeleteCommand {
    attributeSetId: number;
    attributeId: number;
    parentAttributeId: number;
    attributeOrder: number;
    successful: boolean;
}
export interface AttributeOrSetDeleteCheckCommandJSON {
    attributeSetId: number;
    setId: number;
}
export interface AttributeSetDeleteWarningCommandResult {
    numItems: number;
    numAllocations: number;
}
export class Attribute4Saving {
    attributeSetId: number = 0;
    attributeId: number = 0;
    attributeSetDescription: string = "";
    attributeType: string = "";
    attributeTypeId: number = 0;
    attributeName: string = "";
    contactId: number = 0;
    parentAttributeId: number = 0;
    originalAttributeID: number = 0;
    setId: number = 0;
    attributeOrder: number = 0;
    extURL: string = "";
    extType: string = "";
}
export interface ReadOnlyTemplateReview {
    templateId: number;
    templateName: string;
    templateDescription: string;
    reviewSetIds: number[];
}
export class ReviewSetCopyCommand {
    public reviewSetId: number = -1;
    public order: number = 0;
}
export class PerformClusterCommand {
    public itemList: string = "";
    public attributeSetList: string = "";
    public maxHierarchyDepth: number = 2;
    public minClusterSize: number = 0;
    public maxClusterSize: number = 0.35;
    public singleWordLabelWeight: number = 0.5;
    public minLabelLength: number = 1;
    public useUploadedDocs: boolean = false;
    public reviewSetIndex: number = 0;
}

export class ClassifierCommand {

	public searchName: string = '';
	public searchId: number = 0;
	public attributeId: number = 0;
	public setId: number = 0;

}

export class PerformRandomAllocateCommand {
	FilterType: string = '';
	attributeIdFilter: number = 0;
	setIdFilter: number = 0
	attributeId: number = 0;
	setId: number = 0;
	howMany: number = 0;
	numericRandomSample: number = 0;
	RandomSampleIncluded: string = '';
}

export interface singleNode4move extends singleNode {
    CanMoveBranchInHere: boolean;
}
export class ReviewSet4Move extends ReviewSet implements singleNode4move {
    constructor(reviewSet: ReviewSet, movingBrach: singleNode) {
        super();
        this.MovingBrach = movingBrach;
        this.set_id = reviewSet.set_id;
        this.reviewSetId = reviewSet.reviewSetId;
        this.set_name = reviewSet.set_name;
        this.order = reviewSet.order;
        this.codingIsFinal = reviewSet.codingIsFinal;
        this.allowEditingCodeset = reviewSet.allowEditingCodeset;
        this.description = reviewSet.description;
        this.nodeType = reviewSet.nodeType;
        //console.log("type or the root (ReviewSet4Move):", this.nodeType);
        this.setType = reviewSet.setType;
        this.attributes = [];
        this.MovingBrachDepth = this.FindDepthOfBranch(this.MovingBrach, 1);
        this._canMoveBranchInHere = (this.MovingBrachDepth <= this.setType.maxDepth);
        this._alreadyIsTheParent = movingBrach.parent == 0;
        for (let att of reviewSet.attributes) {
            let newA = new SetAttribute4Move(att, 1, this.MovingBrachDepth, this.setType.maxDepth, false, movingBrach.id, movingBrach.parent);
            this.attributes.push(newA);
        }


    }
    private FindDepthOfBranch(branch: singleNode, startingDepth: number): number {//RECURSIVE!!!
        //console.log("FindDepthOfBranch (branch, starting Depth)", branch, startingDepth);
        if (branch.attributes.length > 0) {
            startingDepth++;
            let tmpMax = startingDepth;
            for (let a of branch.attributes) {
                let Al = this.FindDepthOfBranch(a, startingDepth);
                if (Al > tmpMax) tmpMax = Al;
            }
            startingDepth = tmpMax;
        }
        //console.log('FindDepthOfBranch', startingDepth);
        return startingDepth;
    }
    private MovingBrach: singleNode;
    private MovingBrachDepth: number;
    private _canMoveBranchInHere: boolean;
    private _alreadyIsTheParent: boolean;
    get CanMoveBranchInHere(): boolean {
        if (this._alreadyIsTheParent) return false;
        return this._canMoveBranchInHere;
    }
}
export class SetAttribute4Move extends SetAttribute implements singleNode4move {
    constructor(setAttribute: SetAttribute, currentDepth: number, movingBranchDepth: number, maxDepth: number, alreadyCant: boolean, movingBranchRootId: string, movingBranchParentId: number) {
        super();
        this.attribute_id = setAttribute.attribute_id;
        this.attribute_name = setAttribute.attribute_name;
        this.order = setAttribute.order;
        this.attribute_type = setAttribute.attribute_type;
        this.attribute_type_id = setAttribute.attribute_type_id;
        this.nodeType = setAttribute.nodeType;
        this.attribute_set_desc = setAttribute.attribute_set_desc;
        this.attributeSetId = setAttribute.attributeSetId;
        this.parent_attribute_id = setAttribute.parent_attribute_id;
        this.attribute_desc = setAttribute.attribute_desc;
        this.set_id = setAttribute.set_id;
        this.attribute_order = setAttribute.attribute_order;
        if (this.attribute_id == movingBranchParentId) this._alreadyIsTheParent = true;
        else this._alreadyIsTheParent = false;
        if (alreadyCant) this._canMoveBranchInHere = false;
        else if (currentDepth + movingBranchDepth > maxDepth) this._canMoveBranchInHere = false;
        else if ("A" + this.attribute_id == movingBranchRootId) this._canMoveBranchInHere = false;
        else this._canMoveBranchInHere = true;

        for (let att of setAttribute.attributes) {
            let newA = new SetAttribute4Move(att, currentDepth + 1, movingBranchDepth, maxDepth, !this._canMoveBranchInHere, movingBranchRootId, movingBranchParentId);
            this.attributes.push(newA);
        }
    }
    private _canMoveBranchInHere: boolean;
    private _alreadyIsTheParent: boolean;
    get CanMoveBranchInHere(): boolean {
        //console.log('can move in here:', this.name, this._canMoveBranchInHere, this._alreadyIsTheParent);
        if (this._alreadyIsTheParent) return false;
        return this._canMoveBranchInHere;
    }
}
export class ChangeDataEntryMessage {
    ItemsWithIncompleteCoding: number = -1;
    DestinationDataEntryMode: string = "";
    ChangeDataEntryModeMessage: string = "Sorry could not check if you can do this";
    CanChangeDataEntryMode: boolean = false;
}

