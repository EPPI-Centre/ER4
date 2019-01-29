import { Component, Inject, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { timer, Subject, Subscription, Observable } from 'rxjs';
import { take, map, takeUntil } from 'rxjs/operators';
import { ReviewSetsService, SetAttribute } from '../services/ReviewSets.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { ReviewSetsEditingService, PerformClusterCommand } from '../services/ReviewSetsEditing.service';
import { codesetSelectorComponent } from './codesetSelector.component';


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
        private ItemListService: ItemListService
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

    @Output() PleaseCloseMe = new EventEmitter();
    @ViewChild('CodeSelector') CodeSelector!: codesetSelectorComponent;
    ExecuteCommand() {
        this.Command.attributeSetList = "";
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
            let aSet = this.CodeSelector.SelectedNodeData as SetAttribute;
            if (aSet && aSet.attributeSetId && aSet.attributeSetId > 0) {
                this.Command.attributeSetList = aSet.attributeSetId.toString();
            }
            else return;
        }
        else return;
        this.ReviewSetsEditingService.PerformClusterCommand(this.Command);
        this.PleaseCloseMe.emit();
    }
    CancelActivity() {
        this.PleaseCloseMe.emit();
    }
    ClusterWhatChanged(value: string) {
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
            if (this.CodeSelector && this.CodeSelector.SelectedNodeData) return true;
            else return false;
        }
        else return false;
    }
}
