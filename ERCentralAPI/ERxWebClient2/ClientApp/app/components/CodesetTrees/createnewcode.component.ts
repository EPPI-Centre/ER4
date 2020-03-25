import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ClassifierService } from '../services/classifier.service';
import { ReviewSetsService, kvAllowedAttributeType, SetAttribute, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { BuildModelService } from '../services/buildmodel.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy, State, process } from '@progress/kendo-data-query';
import { anyChanged } from '@progress/kendo-angular-grid/dist/es2015/utils';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Subscription } from 'rxjs';
import { OutcomesService } from '../services/outcomes.service';


@Component({
    selector: 'CreateNewCodeComp',
    templateUrl: './createnewcode.component.html',
    providers: []
})

export class CreateNewCodeComp implements OnInit, OnDestroy {
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
		private _reviewSetsService: ReviewSetsService,
		public _reviewSetsEditingService: ReviewSetsEditingService,
		public _buildModelService: BuildModelService,
		public _eventEmitterService: EventEmitterService,
		public _reviewInfoService: ReviewInfoService,
		public _reviewerIdentityServ: ReviewerIdentityService,
		private _outcomeService: OutcomesService
	) { }

    @ViewChild('CodeTypeSelectCollaborate') CodeTypeSelect: any;
    public PanelName: string = '';
    @Output() closeCreateNew = new EventEmitter();
    @Output() emitterCancel = new EventEmitter();
    @Input() IsSmall: boolean = false;

        public get AllowedChildTypes(): kvAllowedAttributeType[] {
            return this._reviewSetsService.AllowedChildTypesOfSelectedNode;
        }
    IsNewCodeNameValid() {

        return true;
        //if (this.PanelName == 'NewCodeSection') {
        //	if (this._NewCode.attribute_name.trim() != "") return true;
        //	else return false;
        //} 
    }
    IsServiceBusy(): boolean {
        if (this._reviewSetsEditingService.IsBusy || this._reviewSetsEditingService.IsBusy || this._reviewInfoService.IsBusy) return true;
        else return false;
    }
    CanWrite(): boolean {
        //console.log('CanWrite', this.ReviewerIdentityServ.HasWriteRights, this.IsServiceBusy());
        if (this._reviewerIdentityServ.HasWriteRights && !this.IsServiceBusy()) {
            //console.log('CanWrite', true);
            return true;
        }
        else {
            //console.log('CanWrite', false);
            return false;
        }
    }
    public get CurrentCodeCanHaveChildren(): boolean {
        //safety first, if anything didn't work as expexcted return false;
        if (!this.CanWrite()) return false;
        else {
            return this._reviewSetsService.CurrentCodeCanHaveChildren;
            //end of bit that goes into "ReviewSetsService.CanNodeHaveChildren(node: singleNode): boolean"
        }
    }
    private _NewReviewSet: ReviewSet = new ReviewSet();
    public get NewReviewSet(): ReviewSet {
        return this._NewReviewSet;
    }
    private _NewCode: SetAttribute = new SetAttribute();
    public get CurrentNode(): singleNode | null {
        if (!this._reviewSetsService.selectedNode) return null;
        else return this._reviewSetsService.selectedNode;
    }
    public get NewCode(): SetAttribute {
        return this._NewCode;
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
        //console.log("What the hell?", this.CodeTypeSelect, this.CodeTypeSelect.nativeElement.selectedOptions, this.CodeTypeSelect.nativeElement.selectedOptions.length);

        if (this.CodeTypeSelect && this.CodeTypeSelect.nativeElement.selectedOptions && this.CodeTypeSelect.nativeElement.selectedOptions.length > 0) {
            this._NewCode.attribute_type_id = this.CodeTypeSelect.nativeElement.selectedOptions[0].value;
			this._NewCode.attribute_type = this.CodeTypeSelect.nativeElement.selectedOptions[0].text;
			console.log('got in here', this._NewCode.attribute_type);
        }
        else {
            this._NewCode.attribute_type_id = 1;//non selectable HARDCODED WARNING!
            this._NewCode.attribute_type = "Not selectable(no checkbox)";
        }

        console.log("will create:", this._NewCode, this.CodeTypeSelect);
        this._reviewSetsEditingService.SaveNewAttribute(this._NewCode)
            .then(
                success => {
                    if (success && this.CurrentNode) {
						
						//console.log('The ones we have are: ', this._outcomeService.outcomesList);
						this.CurrentNode.attributes.push(success);
						console.log('this is the current node: ', this.CurrentNode);
                        //this._reviewSetsService.GetReviewSets();
						

                    }
                    this._NewCode = new SetAttribute();
                    this.CancelActivity(true);

                },
                error => {
                    this.CancelActivity();
                    console.log("error saving new code:", error, this._NewCode);

                })
            .catch(
                error => {
                    console.log("error(catch) saving new code:", error, this._NewCode);
                    this.CancelActivity();
                }
            );
    }
    CancelActivity(refreshTree?: boolean) {
        if (this._NewCode) {
            this._NewCode = new SetAttribute();
        }


        if (refreshTree) {
            //if (this._reviewSetsService.selectedNode) {
            //    let IsSet: boolean = this._reviewSetsService.selectedNode.nodeType == "ReviewSet";
            //    let Id: number = -1;
            //    if (IsSet) Id = (this._reviewSetsService.selectedNode as ReviewSet).set_id;
            //    else Id = (this._reviewSetsService.selectedNode as SetAttribute).attribute_id;
            //    let sub: Subscription = this._reviewSetsService.GetReviewStatsEmit.subscribe(() => {
            //        console.log("trying to reselect: ", Id);
            //        if (IsSet) this._reviewSetsService.selectedNode = this._reviewSetsService.FindSetById(Id);
            //        else this._reviewSetsService.selectedNode = this._reviewSetsService.FindAttributeById(Id);
            //        if (sub) sub.unsubscribe();
            //    }
            //        , () => { if (sub) sub.unsubscribe(); }
            //    );
            //    this._reviewSetsService.selectedNode = null;
            //    this._reviewSetsService.GetReviewSets();
            //}
            this._reviewSetsEditingService.PleaseRedrawTheTree.emit();
        }
        this.closeCreateNew.emit();
        
        this.PanelName = '';
        this.emitterCancel.emit();

    }

    public NewCodeSectionOpen() {

        if (this.PanelName == 'NewCodeSection') {
            this.PanelName = '';
        } else {
            this.PanelName = 'NewCodeSection';
        }
    }
    ngOnInit() {

        //this.ReviewSetsEditingService.FetchSetTypes();


    }
    onSubmit(): boolean {
        console.log("create new code onSubmit");
        return false;
    }
    ngOnDestroy() {

    }


    ngAfterViewInit() {

        this._reviewSetsService.AllowedChildTypesOfSelectedNode;
    }


}


export interface kvSelectFrom {
    key: number;
    value: string;
}
