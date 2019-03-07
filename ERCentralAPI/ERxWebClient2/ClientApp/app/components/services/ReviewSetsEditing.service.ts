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
import { Search } from './search.service';
import { WorkAllocation } from './WorkAllocationList.service';

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
	private _SearchVisualiseData!: Observable<any>;

	public get SearchVisualiseData(): Observable<any> {
		return this._SearchVisualiseData;
	}
	public set SearchVisualiseData(searches: Observable<any>) {
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
                    console.log("AttributeOrSetDeleteCheck Err", error);
                    this.RemoveBusy("AttributeOrSetDeleteCheck");
                    this.modalService.GenericErrorMessage(ErrMsg);
                    return -1;
                }
            )
            .catch(
                (error) => {
                    console.log("AttributeOrSetDeleteCheck catch", error);
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

	public CreateVisualiseData(searchId: number): Observable<Search> {

		this._BusyMethods.push("CreateVisualiseData");
		let body = JSON.stringify({ searchId: searchId });

		this._httpC.post<Observable<Search>>(this._baseUrl + 'api/SearchList/CreateVisualiseData', body)
			.subscribe(result => {

				this.SearchVisualiseData = result;
				this.RemoveBusy("CreateVisualiseData");
				return result;

			},
				error => {
					this.RemoveBusy("CreateVisualiseData");
					this.modalService.GenericError(error);
					return 
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
                    console.log("I am returning this:", result);
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