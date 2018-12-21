import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of, race } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { OK } from 'http-status-codes';
import { error } from '@angular/compiler/src/util';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ModalService } from './modal.service';
import { iSetType, ReviewSetsService, ReviewSet, iReviewSet, SetAttribute, iAttributeSet } from './ReviewSets.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({
    providedIn: 'root',
})

export class ReviewSetsEditingService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient, private ReviewerIdentityService: ReviewerIdentityService,
        private modalService: ModalService,
        private ReviewSetsService: ReviewSetsService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }

    private _SetTypes: iSetType[] = [];
    public get SetTypes(): iSetType[]
    {
        return this._SetTypes;
    }
    public FetchSetTypes() {
        this._BusyMethods.push("FetchSetTypes");
        this._httpC.get<iSetType[]>(this._baseUrl + 'api/Codeset/SetTypes').subscribe(
            (res) => {
                this._SetTypes = res;
            }
            , error => { this.modalService.GenericError(error); }
            , () => {
                this.RemoveBusy("FetchSetTypes");
            }
        );
    }
    public SaveReviewSet(rs: ReviewSet) {
        this._BusyMethods.push("SaveReviewSet");
        let rsC: ReviewSetUpdateCommand = {
            ReviewSetId: rs.reviewSetId,
            SetId: rs.set_id,
            AllowCodingEdits: rs.allowEditingCodeset,
            CodingIsFinal: rs.codingIsFinal,
            SetName: rs.set_name,
            setOrder: rs.order,
            setDescription: rs.description,
            SetTypeId: rs.setType ? rs.setType.setTypeId : -1
        }
        //console.log("saving reviewSet via command", rs, rsC);
        this._httpC.post<ReviewSetUpdateCommand>(this._baseUrl + 'api/Codeset/SaveReviewSet', rsC).subscribe(
            data => {
                this.RemoveBusy("SaveReviewSet");
                //this.ItemCodingItemAttributeSaveCommandExecuted.emit(data);
                //this._IsBusy = false;
            }, error => {
                this.RemoveBusy("SaveReviewSet");
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
            }, () => { }
        );
    }
    public SaveNewReviewSet(rs: ReviewSet): Promise<iReviewSet | null> {
        this._BusyMethods.push("SaveNewReviewSet");
        let ErrMsg = "Something went wrong: it appears that the codeset was not saved correctly. \r\n Reloading the review is probably wise. \r\n If the problem persists, please contact EPPISupport.";
        let rsC: ReviewSetUpdateCommand = {
            ReviewSetId: rs.reviewSetId,
            SetId: rs.set_id,
            AllowCodingEdits: rs.allowEditingCodeset,
            CodingIsFinal: rs.codingIsFinal,
            SetName: rs.set_name,
            setOrder: rs.order,
            setDescription: rs.description,
            SetTypeId: rs.setType ? rs.setType.setTypeId : -1
        }
        //console.log("saving reviewSet via command", rs, rsC);
        return this._httpC.post<iReviewSet>(this._baseUrl + 'api/Codeset/ReviewSetCreate', rsC).toPromise()
            .then((res) => { this.RemoveBusy("SaveNewReviewSet"); return res; },
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
    public AttributeOrSetDeleteCheck(SetId: number, AttributeSetId:number): Promise<number> {//get how many items have coding in a codeset or section therein
        this._BusyMethods.push("AttributeOrSetDeleteCheck");
        let ErrMsg = "Something went wrong: could not check how many items would be affected. \r\n If the problem persists, please contact EPPISupport.";
        let body: AttributeOrSetDeleteCheckCommandJSON = {
            attributeSetId: AttributeSetId,
            setId: SetId
        };
        return this._httpC.post<number>(this._baseUrl + 'api/Codeset/AttributeOrSetDeleteCheck', body).toPromise()
            .then(
                (result) => {
                    //console.log("ReviewSetCheckCodingStatus", result);
                    this.RemoveBusy("AttributeOrSetDeleteCheck");
                    return result;
                }
                , (error) => {
                    console.log("ReviewSetCheckCodingStatus Err", error);
                    this.RemoveBusy("AttributeOrSetDeleteCheck");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return -1;
                }
            )
            .catch(
                (error) => {
                    console.log("ReviewSetCheckCodingStatus catch", error);
                    this.RemoveBusy("AttributeOrSetDeleteCheck");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return -1;
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
    
    public MoveSetAttribute(attributeSetId: number,
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
        this._httpC.post<ReviewSetUpdateCommand>(this._baseUrl + 'api/Codeset/AttributeSetMove', rsC).subscribe(
            data => {
                this.RemoveBusy("MoveSetAttribute");
                //this.ItemCodingItemAttributeSaveCommandExecuted.emit(data);
                //this._IsBusy = false;
            }, error => {
                this.RemoveBusy("MoveSetAttribute");
                this.modalService.GenericErrorMessage("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
            }, () => {
                
            }
        );
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
        this._BusyMethods.push("SaveNewAttribute");
        let ErrMsg = "Something went wrong: it appears that the Code was not saved correctly. \r\n Reloading the review is probably wise. \r\n If the problem persists, please contact EPPISupport.";
        if (Att.set_id < 1) {
            //bad! can't do this...
            this.modalService.GenericErrorMessage(ErrMsg);
            this.RemoveBusy("SaveNewAttribute"); 
            return new Promise<null>((resolve, reject) => { setTimeout(reject, 1, null) });
        }
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
        //console.log("saving reviewSet via command", rs, rsC);
        return this._httpC.post<boolean>(this._baseUrl + 'api/Codeset/AttributeUpdate', Data).toPromise()
            .then((res) => {
                this.RemoveBusy("UpdateAttribute");
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
    public SetAttributeDelete(Att: SetAttribute): Promise<AttributeDeleteCommand> {
        this._BusyMethods.push("ReviewSetDelete");
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
                    this.RemoveBusy("ReviewSetDelete");
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
}

export interface AttributeSetMoveCommand {
    FromId: number;
    ToId: number;
    AttributeSetId: number;
    attributeOrder: number;
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
}