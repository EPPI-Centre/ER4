import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewInfo, ReviewInfoService } from './ReviewInfo.service';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ConfigService } from './config.service';


@Injectable({

	providedIn: 'root',

})

export class ReviewService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
      private modalService: ModalService,
    configService: ConfigService
        //@Inject('BASE_URL') private _baseUrl: string
        ) {
      super(configService);
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
    

    public async UpdateAccount(contactId: number, ContactName: string, Username: string,
        Email: string, OldPassword: string, NewPassword: string): Promise<boolean> {

        let _AccountFullDetails: ContactFull = { contactId: 0, ContactName: "", username: "", email: "", OldPassword: "", NewPassword: "" };
        // put values into ContactFull and pass that through
        _AccountFullDetails.contactId = contactId;
        _AccountFullDetails.ContactName = ContactName;
        _AccountFullDetails.username = Username;
        _AccountFullDetails.email = Email;
        if (NewPassword.trim().length > 0) {
            _AccountFullDetails.OldPassword = OldPassword;
            _AccountFullDetails.NewPassword = NewPassword;
        }
        else {
            _AccountFullDetails.OldPassword = "";
            _AccountFullDetails.NewPassword = "";
        }
        let res = await this.UpdateAccountFull(_AccountFullDetails);
        // res = 0 - everything OK
        // res = 1 - email already in use
        // res = 2 - username already in use
        // res = 3 - oldPassword is not correct
        // res = 4 - API call failed, error has been shown already

        if (res == 0) {
            return true;
        }
        else {
            if (res == 1) {
                this.modalService.GenericErrorMessage("This <b>email</b> is already in use.<br>If you do not think this is correct please contact eppisupport@ucl.ac.uk");
            } else if (res == 2) {
                this.modalService.GenericErrorMessage("This <b>Username</b> is already in use.<br>Please try a different username.");
            }
            else if (res == 3) {
                this.modalService.GenericErrorMessage("Your <b>Exising password</b> is not correct.");
            }
            return false;
        }
    }


    public UpdateAccountFull(fullAccountDetails: ContactFull): Promise<number> { // could make this async directly
        this._BusyMethods.push("UpdateAccount");
        let body = JSON.stringify(fullAccountDetails);

        return this._httpC.post<number>(this._baseUrl + 'api/AccountManager/UpdateAccount',
            body).toPromise()
            .then(
                (result) => {
                    this.RemoveBusy("UpdateAccount");
                    return result;
            }, error => {
                this.modalService.GenericError(error);
                this.RemoveBusy("UpdateAccount");
                return 4;
            }
        );
    }


    public async UpdateReviewName(reviewName: string): Promise<boolean> {
        this._BusyMethods.push("UpdateReviewName");

        let _ReviewName = { Value: reviewName };        
        let body = JSON.stringify(_ReviewName);

        return this._httpC.post<boolean>(this._baseUrl + 'api/AccountManager/UpdateReviewName',
            body).toPromise()
            .then(
                (result) => {
                    this.RemoveBusy("UpdateReviewName");
                    return true;
                }, error => {
                    this.modalService.GenericError(error);
                    this.RemoveBusy("UpdateReviewName");
                    return false;
                }
            );
    }


    public RemoveReviewer(reviewerID: number): Promise<boolean> { // could make this async directly
      this._BusyMethods.push("RemoveReviewer");
      //let body = JSON.stringify({ contactID: reviewerID });

      let _reviewerID = { Value: reviewerID };
      let body = JSON.stringify(_reviewerID);

      return this._httpC.post<boolean>(this._baseUrl + 'api/AccountManager/RemoveReviewer',
        body).toPromise()
        .then(
          (result) => {
            this.RemoveBusy("RemoveReviewer");
            return true;
          }, error => {
            this.modalService.GenericError(error);
            this.RemoveBusy("RemoveReviewer");
            return false;
          }
        );
    }


    public UpdateReviewerRole(role: string, reviewerID: number): Promise<boolean> { // could make this async directly
      this._BusyMethods.push("UpdateReviewerRole");
      let body = JSON.stringify({ role: role, contactID: reviewerID });

      return this._httpC.post<boolean>(this._baseUrl + 'api/AccountManager/UpdateReviewerRole',
        body).toPromise()
        .then(
          (result) => {
            this.RemoveBusy("UpdateReviewerRole");
            return true;
          }, error => {
            this.modalService.GenericError(error);
            this.RemoveBusy("UpdateReviewerRole");
            return false;
          }
        );
    }

    public async InviteReviewer(reviewerEmail: string): Promise<boolean> {
      let res = await this.AddNewReviewer(reviewerEmail);
      // res = 0 - everything OK
      // res = 1 - email not found
      // res = 2 - there is more than 1 account with this email address
      // res = 3 - API call failed, error has been shown already

      if (res == 0) {
        return true;
      }
      else {
        if (res == 1) {
          this.modalService.GenericErrorMessage("This email was not found in the database.<br>" +
            "Are sure an EPPI-Reviewer account exists with this email address?<br>" +
            "New accounts can be created in the ACCOUNT MANAGER that can be found at " +
            "<a href=\"https://eppi.ioe.ac.uk/cms/er4 \"target=\"_blank\">https://eppi.ioe.ac.uk/cms/er4</a>");
        } else if (res == 2) {
          this.modalService.GenericErrorMessage("There is more than 1 account with this email address.<br>Please contact EPPISupport@ucl.ac.uk for assistance.");
        }
        return false;
      }
    }


    public AddNewReviewer(reviewerEmail: string): Promise<number> {
      this._BusyMethods.push("AddUpdateAccount");

      let _ReviewerEmail = { Value: reviewerEmail };
      let body = JSON.stringify(_ReviewerEmail);

      return this._httpC.post<number>(this._baseUrl + 'api/AccountManager/AddReviewer',
        body).toPromise()
        .then(
          (result) => {
            this.RemoveBusy("UpdateAccount");
            return result;
          }, error => {
            this.modalService.GenericError(error);
            this.RemoveBusy("UpdateAccount");
            return 3;
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

    expiry: string;
    role: string;
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




