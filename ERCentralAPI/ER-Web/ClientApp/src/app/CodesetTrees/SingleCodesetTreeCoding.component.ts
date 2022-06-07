import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, SetAttribute } from '../services/ReviewSets.service';
import { ITreeOptions, TreeComponent } from '@circlon/angular-tree-component';
import { OutcomesService, OutcomeItemAttribute, Outcome } from '../services/outcomes.service';
import { CheckboxControlValueAccessor } from '@angular/forms';
import { TreeItem } from '@progress/kendo-angular-treeview';


@Component({
  selector: 'SingleCodesetTreeCoding',
  styles: [],
  templateUrl: './SingleCodesetTreeCoding.component.html'
})

export class SingleCodesetTreeCodingComponent implements OnInit, OnDestroy {
  constructor(private router: Router,
    private ReviewerIdentityServ: ReviewerIdentityService,
    private ReviewSetsService: ReviewSetsService,
    private _outcomeService: OutcomesService,

  ) { }

  @Input() MaxHeight: number = 800;
  @Input() currentOutcome: Outcome = new Outcome();

  ngOnInit() {

    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
      this.router.navigate(['home']);
    }
  }
  public get IsServiceBusy(): boolean {
    return this.ReviewSetsService.IsBusy;
  }

  public CanWriteCoding(data: singleNode): boolean {
    return this.ReviewSetsService.CanWriteCoding(data);
  }
  CheckBoxClicked(event: Event, data: singleNode,) {
    let checked = (event.target as HTMLInputElement).checked;
    if (data.nodeType != "SetAttribute") return;

    let Att = data as SetAttribute;
    let index = this.currentOutcome.outcomeCodes.outcomeItemAttributesList.findIndex(found => found.attributeId == Att.attribute_id);
    if (checked) {
      if (index == -1) {
        //add it to outcomeCodes.outcomeItemAttributesList
        console.log('got in here...');
        let outcomeItemAttribute: OutcomeItemAttribute = {
          outcomeItemAttributeId: 0,
          outcomeId: this.currentOutcome.outcomeId,
          attributeId: Att.attribute_id,
          additionalText: "",
          attributeName: Att.attribute_name
        };
        this.currentOutcome.outcomeCodes.outcomeItemAttributesList.push(outcomeItemAttribute);
        console.log('we have codes length: ', this.currentOutcome.outcomeCodes.outcomeItemAttributesList);
      }
      else {
        //uh? It's there already!
        console.log("didn't add attribute to outcome - was already there.", Att);
      }
    }
    else {//splice
      if (index == -1) {
        //uh? It's not there already!
        console.log("didn't remove attribute to outcome - wasn't already there.", Att);
      }
      else {
        this.currentOutcome.outcomeCodes.outcomeItemAttributesList.splice(index, 1);
      }
    }
  }

  IsAttributeInOutcome(data: singleNode): boolean {
    if (data.nodeType != "SetAttribute") return false; //check this! || this._outcomeService.currentOutcome.itemSetId < 1
    let Att = data as SetAttribute;
    //console.log(Att);
    //console.log(this.currentOutcome.outcomeCodes.outcomeItemAttributesList);
    let index = this.currentOutcome.outcomeCodes.outcomeItemAttributesList.findIndex(found => found.attributeId == Att.attribute_id);
    if (index < 0) return false;
    else return true;
  }

  get nodes(): singleNode[] | null {
    if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) {
      return this.ReviewSetsService.ReviewSets.filter(x => x.ItemSetId == this._outcomeService.ItemSetId);
    }
    else {
      return null;
    }
  }

  NodeSelected(event: TreeItem) {
    let node = event.dataItem as singleNode;
    this.ReviewSetsService.selectedNode = node;
  }

  ngOnDestroy() {

  }
}
