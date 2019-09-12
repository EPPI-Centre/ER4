import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ClassifierService } from '../services/classifier.service';
import { ReviewSetsService, kvAllowedAttributeType, SetAttribute, ReviewSet, singleNode } from '../services/ReviewSets.service';
import { BuildModelService } from '../services/buildmodel.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewSetsEditingService } from '../services/ReviewSetsEditing.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Subscription } from 'rxjs';


@Component({
    selector: 'EditCodeComp',
    templateUrl: './editcode.component.html',
    providers: []
})

export class EditCodeComp implements OnInit, OnDestroy {
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewSetsService: ReviewSetsService,
        private ReviewSetsEditingService: ReviewSetsEditingService,
        private _buildModelService: BuildModelService,
        private _eventEmitterService: EventEmitterService,
        private ReviewInfoService: ReviewInfoService,
        private ReviewerIdentityServ: ReviewerIdentityService
	) { }

    
    @Input() IsSmall: boolean = false;
    @Input() UpdatingCode: SetAttribute | null = null;
    @Output() emitterCancel = new EventEmitter();
    ShowDeleteCode: boolean = false;
    //public get UpdatingCode2(): SetAttribute | null {
    //    console.log("UpdatingCode2");
    //    return this.UpdatingCode;
    //}

    ngOnInit() {
    }

    private _appliedCodes: number = -1;
    public get appliedCodes(): number {
        return this._appliedCodes;
    }
    public get AllowedChildTypes(): kvAllowedAttributeType[] {
        return this.ReviewSetsService.AllowedChildTypesOfSelectedNode;
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
    IsNewCodeNameValid() {
        if (!this.ReviewSetsService.selectedNode || this.ReviewSetsService.selectedNode.nodeType == "ReviewSet") return false;//might happen when in itemdetails page
        if (this.UpdatingCode && this.UpdatingCode.attribute_name.trim() != "") return true;
        else return false;
    }
    IsServiceBusy(): boolean {
        if (this.ReviewSetsService.IsBusy || this.ReviewSetsEditingService.IsBusy || this.ReviewInfoService.IsBusy) return true;
        else return false;
    }
    CancelActivity(refreshtree?: boolean) {
        if (refreshtree != null) this.emitterCancel.emit(refreshtree);
        else this.emitterCancel.emit();
    }

    AttributeTypeChanged(typeId: number) {
        console.log('selected att type:' + typeId);
        if (this.UpdatingCode && this.UpdatingCode.nodeType == "SetAttribute") {
            this.UpdatingCode.attribute_type_id = typeId;
            let foundT = this.AllowedChildTypes.find(found => found.key == typeId);
            if (foundT) this.UpdatingCode.attribute_type = foundT.value;
        }
    }

    UpdateCode() {
        if (!this.UpdatingCode || this.UpdatingCode.nodeType != "SetAttribute") {
            this.CancelActivity();
            return;//fail silently, should be ok as it should never happen...
        }
        let Att = this.UpdatingCode;
        this.ReviewSetsEditingService.UpdateAttribute(Att)
            .then(
                success => {
                    //alert("did it");
                    if (success) {
                        this.ReviewSetsService.selectedNode = Att;
                        let OldAtt = this.ReviewSetsService.FindAttributeById(Att.attribute_id);
                        if (OldAtt) {
                            if (OldAtt.parent_attribute_id == 0) {
                                let Set = this.ReviewSetsService.FindSetById(Att.set_id);
                                if (Set) {
                                    let index = Set.attributes.findIndex(Old => OldAtt === Old);
                                    if (index != -1) {
                                        console.log("replacing updated Att...");
                                        Set.attributes.splice(index, 1, Att);
                                    }
                                }
                            }
                            else {
                                let Par = this.ReviewSetsService.FindAttributeById(Att.parent_attribute_id);
                                if (Par) {
                                    let index = Par.attributes.findIndex(Old => OldAtt === Old);
                                    if (index != -1) Par.attributes.splice(index, 1, Att);
                                }
                            }
                            //if (this.treeEditorComponent) this.treeEditorComponent.RefreshLocalTree();
                        }
                        //eventual error messages have been shown by the service...
                        this.CancelActivity(true);
                    }
                },
                error => {
                    this.CancelActivity(true);
                    console.log("error updating code:", error, Att);

                })
            .catch(
                error => {
                    console.log("error(catch) updating code:", error, Att);
                    this.CancelActivity(true);
                }
            );
    }


    ShowDeleteCodesetClicked() {
        //console.log('0');
        if (!this.UpdatingCode) return;
        this._appliedCodes = -1;
        this.ShowDeleteCode = true;
        this.ReviewSetsEditingService.AttributeOrSetDeleteCheck(0, this.UpdatingCode.attributeSetId).then(
            success => {
                //alert("did it");
                this._appliedCodes = success;
                //return result;
            },
            error => {
                //alert("Sorry, creating the new codeset failed.");
                //this.modalService.GenericErrorMessage(ErrMsg);
            });
    }
    HideDeleteCodeset() {
        this.ShowDeleteCode = false;
        this._appliedCodes = -1;
    }
    DoDeleteCode() {
        if (!this.UpdatingCode) return;

        console.log("will delete:", this.UpdatingCode);
        this.ReviewSetsEditingService.SetAttributeDelete(this.UpdatingCode)
            .then(
                success => {
                    //alert("did it");
                    this.ReviewSetsService.selectedNode = null;
                    this.ReviewSetsService.GetReviewSets();
                    this.CancelActivity(true);
                },
                error => {
                    this.ReviewSetsService.selectedNode = null;
                    this.ReviewSetsService.GetReviewSets();
                    this.CancelActivity(true);
                });
    }

    ngOnDestroy() {

    }
    

}

