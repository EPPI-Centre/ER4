import { Component, Inject, OnInit, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewSetsService, SetAttribute, singleNode } from '../services/ReviewSets.service';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSetsEditingService, PerformClusterCommand } from '../services/ReviewSetsEditing.service';


@Component({
    selector: 'RunLingo3G',
    templateUrl: './runlingo3g.component.html',
    providers: []
})

export class RunLingo3G implements OnInit {

   

    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService,
        private ReviewSetsEditingService: ReviewSetsEditingService,
        private ItemListService: ItemListService,
        private ReviewSetsService: ReviewSetsService
    ) {    }

    ngOnInit() {
    }
    ngOnDestroy() {
    }
    public Command = new PerformClusterCommand();
    public ClusterWhat: string = "All Selected Items";
    public get ShowCodes(): boolean {
        if (this.ClusterWhat == "Items with this Code") return true;
        else return false;
    }
    public get SelectedCode(): singleNode | null {
        if (this.ReviewSetsService.selectedNode && this.ReviewSetsService.selectedNode.nodeType == "SetAttribute") return this.ReviewSetsService.selectedNode;
        else return null;
    }
    @Output() PleaseCloseMe = new EventEmitter();
    @Output() PleaseOpenTheCodes = new EventEmitter();
    ExecuteCommand() {
        this.Command.attributeSetList = "";
        this.Command.maxHierarchyDepth = Math.round(this.Command.maxHierarchyDepth);
        this.Command.minLabelLength = Math.round(this.Command.minLabelLength);
        if (this.ClusterWhat == "All Included Items") {
            this.Command.itemList = "";
        }
        else if (this.ClusterWhat == "All Selected Items" && this.ItemListService.SelectedItems.length > 1) {
            let arr: string[] = [];
            for (let it of this.ItemListService.SelectedItems) {
                arr.push(it.itemId.toString());
            }
            this.Command.itemList = arr.join(",");
            console.log("itemList" , this.Command.itemList);
            if (this.Command.itemList == "") return;
        }
        else if (this.ClusterWhat == "Items with this Code") {
            this.Command.itemList = "";
            if (this.SelectedCode) {
                let aSet = this.SelectedCode as SetAttribute;
                if (aSet && aSet.attributeSetId && aSet.attributeSetId > 0) {
                    this.Command.attributeSetList = aSet.attributeSetId.toString();
                }
                else return;
            }
            else return;
        }
        else return;
        this.PleaseOpenTheCodes.emit();
        this.ReviewSetsEditingService.PerformClusterCommand(this.Command);
        this.PleaseCloseMe.emit();
    }
    CancelActivity() {
        this.PleaseCloseMe.emit();
    }
  ClusterWhatChanged(event: Event) {
    let value = (event.target as HTMLOptionElement).value;
    this.ClusterWhat = value;
  }
    public get CanExecuteCommand(): boolean {
        if (this.ClusterWhat == "All Selected Items") {
            if (this.ItemListService.SelectedItems.length > 1) return true;
            else return false;
        }
        else if (this.ClusterWhat == "All Included Items") {
            return true;
        }
        else if (this.ClusterWhat == "Items with this Code") {
            if (this.SelectedCode) return true;
            else return false;
        }
        else return false;
    }
}
