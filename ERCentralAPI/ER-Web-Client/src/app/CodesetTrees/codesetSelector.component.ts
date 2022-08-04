import { Component, OnInit, Output, Input, OnDestroy, AfterViewInit, EventEmitter } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, iSetType, SetAttribute } from '../services/ReviewSets.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { TreeItem } from '@progress/kendo-angular-treeview';


@Component({
  selector: 'codesetSelector',
  styles: [],
  templateUrl: './codesetSelector.component.html'
})

export class codesetSelectorComponent implements OnInit, OnDestroy, AfterViewInit {
  constructor(private router: Router,
    private ReviewerIdentityServ: ReviewerIdentityService,
    private ReviewSetsService: ReviewSetsService,
    private _eventEmitterService: EventEmitterService

  ) { }

  @Input() MaxHeight: number = 400;
  @Input() rootsOnly: boolean = false;//obsolete
  @Input() IsMultiSelect: boolean = false;
  @Input() WhatIsSelectable: string = "All";
  //"All": can select any type of node
  //"AttributeSet":Codes(AttributeSet) only
  //"ReviewSet":Codesets(ReviewSet) only
  //"NodeWithChildren":Anything that does have children
  //"CanHaveChildren": any node that is allowed to contain children(future)
  public IsAdmin: boolean = false;

  public SelectedNodeData: singleNode | null = null;
  public SelectedNodesData: singleNode[] = [];
  public get SelectedCodeDescription(): string {
    return this.ReviewSetsService.SelectedCodeDescription;
  }
  //@ViewChild('tree') treeComponent!: TreeComponent;
  @Output() selectedNodeInTree: EventEmitter<null> = new EventEmitter();
  //@Input() attributesOnly: boolean = false;

  ngOnInit() {
    //console.log("Selector init");
    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
      this.router.navigate(['home']);
    }
    else {

      if (this.WhatIsSelectable == 'RandomAllocation') {

        this._nodes = [];
        for (let Rset of this.ReviewSetsService.ReviewSets) {
          if (Rset.setType.allowRandomAllocation && Rset.allowEditingCodeset) this._nodes.push(Rset);
        }
      }
      else if (this.WhatIsSelectable == 'ScreeningCodes') {
        this._nodes = this.ReviewSetsService.ReviewSets.filter(found => found.setType.setTypeName == "Screening");
      }
    }
  }

  ngAfterViewInit() {


  }

  private _nodes: singleNode[] = [];

  get nodes(): singleNode[]  {


    if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets
      && this.ReviewSetsService.ReviewSets.length > 0) {

      if (this.WhatIsSelectable == 'RandomAllocation' || this.WhatIsSelectable == 'ScreeningCodes') {
        //console.log("get nodes in Random alloc mode");
        return this._nodes;
      }
      else {
        //console.log("get nodes NOT in Random alloc mode");
        return this.ReviewSetsService.ReviewSets;
      }
    }
    else {
      return [];
    }
  }

  NodeSelected(event: TreeItem) {
    const node: singleNode = event.dataItem;
    if (this.WhatIsSelectable == "SetAttribute" && this.IsMultiSelect == false) {
      if (node.nodeType == "SetAttribute") {
        //console.log(JSON.stringify(node));
        //this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
        // and raise event to close the drop down
        this.SignalGoodNodeSelection(node);
      }
    }
    else if (this.WhatIsSelectable == 'ScreeningCodes' && this.IsMultiSelect == false) {
      if (node.nodeType == "SetAttribute") {
        const SA = (node as SetAttribute);
        if (SA) {
          if (SA.attribute_type_id == 10 || SA.attribute_type_id == 11) this.SignalGoodNodeSelection(node);
        }
      }
    }
    else if (this.WhatIsSelectable == 'RandomAllocation') {
      if (node != null) {
        let type: iSetType = {
          setTypeId: 0,
          setTypeName: '',
          setTypeDescription: '',
          allowComparison: false,
          allowRandomAllocation: false,
          maxDepth: 1,
          allowedCodeTypes: [],
          allowedSetTypesID4Paste: []
        };
        if (node.nodeType == 'ReviewSet') {

          let tempNode: ReviewSet = node as ReviewSet;
          type = tempNode.setType;
          let setTemp: any = JSON.stringify(tempNode.setType);
          //let setTypeName: any = JSON.parse(setTemp)["setTypeName"];
          let allowRandomAllocations: boolean = JSON.parse(setTemp)["allowRandomAllocation"];
          //console.log('allowRandomAllocation:=======================: ' + allowRandomAllocations);
          //if (setTypeName != null && setTypeName == 'Admininstation') {
          if (allowRandomAllocations) {

            this.SignalGoodNodeSelection(node);
          }
          //}

        }
        else {
          //we have removed children for sets that don't accept random allocations, 
          //so now we know the node selected is within a set that does accept them.
          //however, we need to check if the node can have children still.

          if (this.ReviewSetsService.ThisCodeCanHaveChildren(node)) this.SignalGoodNodeSelection(node);
          return;
        }
      }

    }
    else if (node.nodeType == "ReviewSet" && this.IsMultiSelect == false) {

      this.SelectedNodeData = node;
      this.selectedNodeInTree.emit();
      this._eventEmitterService.nodeSelected = node;

    } else if (node.nodeType == "SetAttribute" && this.IsMultiSelect == true) {
      console.log('you cannot use multiselect here 1');

    } else if (node.nodeType == "ReviewSet" && this.IsMultiSelect == true) {
      console.log('you cannot use multiselect here 2');

    } else if (this.IsMultiSelect == true) {
      console.log('you cannot use multiselect here 3');

    } else {
      this.SignalGoodNodeSelection(node);
    }
  }

  private SignalGoodNodeSelection(node: singleNode) {
    this.SelectedNodeData = node;
    this.selectedNodeInTree.emit();
    this._eventEmitterService.nodeSelected = node;
  }

  ngOnDestroy() {
  }
}
