import { Component, Inject, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { result } from 'underscore';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewService, Contact } from '../services/review.service';


@Component({
	selector: 'EditAccountComp',
	templateUrl: './editAccount.component.html',
	providers: []
})

export class EditAccountComponent implements OnInit {
    constructor(
        private userAccountService: ReviewerIdentityService,
        private AccountManagerService: ReviewService
    ) {
    }

    ngOnInit() {
        //if (this.userAccountService.reviewerIdentity.userId != null) {
        //    this.userAccountService.GetUserAccount(this.userAccountService.reviewerIdentity.userId);
        //}
    }

    /*IsAccountNameValid(): number {
        // zero if it's fine, 1 if empty, 2 if name-clash (we don't want 2 sources with the same name)
        //if (this.WizPhase != 2) return 1;
        if (this._CurrentAccount == null) return 1;
        else {
            return 0
            //return this.AccountManagerService.IsSourceNameValid(this._CurrentSource.source_Name, this._CurrentSource.source_ID);
        };
    }*/

    public CurrentAccount: Contact | null = null;
    //public _CurrentAccount: Contact | null = null;
    //get CurrentAccount(): Contact | null {
    //    return this._CurrentAccount;
    //}

	//@Output() onCloseClick = new EventEmitter();
    public isExpanded: boolean = false;
    public oldPassword: string = '';
    public confirmEmail: string = '';
    public newPassword: string = '';
    public confirmNewPassword: string = '';

    CanWrite(): boolean {
        //console.log("create rev check:", this._reviewerIdentityServ.reviewerIdentity);

            return true;
        
    }

    onSubmit(): boolean {
        console.log("Contact onSubmit");
        return false;
    }


    async Expand() {      
        if (this.userAccountService.reviewerIdentity.userId != null) {
            await this.AccountManagerService.GetUserAccount(this.userAccountService.reviewerIdentity.userId);
            //this.CurrentAccount = this.AccountManagerService.CurrentAccountDetail;
            
            this.CurrentAccount = JSON.parse(JSON.stringify(this.AccountManagerService.CurrentAccountDetail));

            if (this.CurrentAccount != null) {
                //this.confirmEmail = this.CurrentAccount.Email // make user fill it in
            }
            this.newPassword = "";
            this.confirmNewPassword = "";

            this.isExpanded = true;
        }
       
    }

    CloseEditAccount() {
        this.isExpanded = false;
	}


    SaveAccount() {
        if (this.CurrentAccount) {
            this.AccountManagerService.UpdateAccount(this.userAccountService.reviewerIdentity.userId, this.CurrentAccount.contactName,
                this.CurrentAccount.username, this.CurrentAccount.email, this.oldPassword, this.newPassword);
        }
	}

    private passw = "^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$";

    CheckPassword() {     
        if (this.newPassword.length == 0) {
            return true // hide warning
        }
        else {
            if (this.newPassword.match(this.passw)) {
                return true;
            }
            else {
                return false;
            }
        }
    }

    OldPasswordNeeded() {
        if ((this.newPassword.length == 0) && (this.confirmNewPassword.length == 0)) {
            return true // hide warning
        }
        else if ((this.newPassword.length > 0) && (this.confirmNewPassword.length > 0) && this.oldPassword.length > 0) {
            return true;
        }
        else {
            return false;
        }
    }


}
