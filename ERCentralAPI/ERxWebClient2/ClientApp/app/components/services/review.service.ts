import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewInfo, ReviewInfoService } from './ReviewInfo.service';
import { ReviewerIdentityService } from './revieweridentity.service';

@Injectable({

	providedIn: 'root',

})

export class ReviewService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
        ) {
        super();
    }



	CreateReview(RevName: string, ContactId: string): Promise<number> {

		//hardcode until this works

		this._BusyMethods.push("CreateReview");

		let body = JSON.stringify({ reviewName: RevName, userId: ContactId });
        return this._httpC.post<number>(this._baseUrl + 'api/Review/CreateReview', body
        ).toPromise<number>().then(
            (result) => {
                this.RemoveBusy("CreateReview");
                return result;
            },
            (rejected) => {
                this.RemoveBusy("CreateReview");
                this.modalService.GenericErrorMessage("Sorry could not create new review. If the problem persists, please contact EPPI-Support.");
                return 0;
            }
        ).catch((error) => {
            this.RemoveBusy("CreateReview");
            this.modalService.GenericErrorMessage("Sorry could not create new review. If the problem persists, please contact EPPI-Support.");
            return 0;
        });

	
	}

    private _Account: Contact | null = null;
    public get CurrentAccountDetail(): Contact | null {
        return this._Account;
    }

    public GetAccountData(Id: number): Promise<Contact | boolean> {
        this._BusyMethods.push("GetAccount");
        let ErrMsg = "Something went wrong when getting the account data. \r\n If the problem persists, please contact EPPISupport.";
        let body = JSON.stringify({ Value: Id });
        return this._httpC.post<Contact>(this._baseUrl + 'api/AccountManager/GetUserAccountDetails',
            body).toPromise()
            .then(
                (result) => {
                    this.RemoveBusy("GetAccount");
                    return result;
                }
                , (error) => {
                    this.modalService.GenericError(error);
                    this.RemoveBusy("GetAccount");
                    return false;
                }
            ).catch((caught) => {
                this.modalService.GenericError(caught);
                this.RemoveBusy("GetAccount");
                return false;
            });
    }


    public async GetUserAccount(ContactId: number) {
        let res = await this.GetAccountData(ContactId);
        if (res != false) {
            if (res != true) {
                this._Account = res;
            }
        }
    }
    
    private _AccountFullDetails: ContactFull = { contactId: 0, ContactName: "", username: "", email: "", OldPassword: "", NewPassword: ""};


    public async UpdateAccount(contactId: number, ContactName: string, Username: string, Email: string, OldPassword: string, NewPassword: string) {

        // put values into ContactFull and pass that through
            this._AccountFullDetails.contactId = contactId;
            this._AccountFullDetails.ContactName = ContactName;
            this._AccountFullDetails.username = Username;
            this._AccountFullDetails.email = Email;
            this._AccountFullDetails.OldPassword = OldPassword;
            this._AccountFullDetails.NewPassword = NewPassword;

            let res = await this.UpdateAccountFull(this._AccountFullDetails);

            //this._Account = res;

            // res
            // 0 - everything OK
            // 1 - email already in use
            // 2 - username already in use
            // 3 - oldPassword is not correct

        //}
    }
    
    public UpdateAccountFull(fullAccountDetails: ContactFull): Promise<ContactFull> {

        this._BusyMethods.push("UpdateAccount");
        let ErrMsg = "Something went wrong when updating the account. \r\n If the problem persists, please contact EPPISupport.";

        return this._httpC.post<ContactFull>(this._baseUrl + 'api/Contact/UpdateAccount',

            fullAccountDetails).toPromise()
            .then(
                (result) => {
                    if (!result) this.modalService.GenericErrorMessage(ErrMsg);
                    this.RemoveBusy("UpdateAccount");
                    return result;
                }
                , (error) => {
                    this.modalService.GenericErrorMessage(ErrMsg);
                    this.RemoveBusy("CreateArm");
                    return error;
                }
            )
            .catch(
                (error) => {
                    this.modalService.GenericErrorMessage(ErrMsg);
                    this.RemoveBusy("UpdateAccount");
                    return error;
                }
            );

    }
    




	ngOnInit() {

	}
}


export interface Contact {
    contactName: string;
    username: string;
    email: string;
    ContactId: number;
}



export interface ContactFull {
    contactId: number;
    ContactName: string;
    username: string;
    email: string;
    OldPassword: string;
    NewPassword: string;

    
}



export class Review {

	contactId: number = 0;
	reviewId: number = 0;
	reviewName: string = '';
}

