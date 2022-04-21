import { Component, Inject, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { result } from 'underscore';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewService, Contact } from '../services/review.service';
import { NotificationService } from '@progress/kendo-angular-notification';


@Component({
	selector: 'EditAccountComp',
	templateUrl: './editAccount.component.html',
	providers: []
})

export class EditAccountComponent implements OnInit {
    constructor(
        private userAccountService: ReviewerIdentityService,
        private AccountManagerService: ReviewService,
        private notificationService: NotificationService,
        private ReviewerIdentityServ: ReviewerIdentityService
    ) {
    }

    ngOnInit() {
    }


    public CurrentAccount: Contact | null = null;
    public isExpanded: boolean = false;
    public oldPassword: string = '';
    public confirmEmail: string = '';
    public newPassword: string = '';
    public confirmNewPassword: string = '';
    public ShowPasswords: boolean = false;

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
            
            this.CurrentAccount = JSON.parse(JSON.stringify(this.AccountManagerService.CurrentAccountDetail));

            if (this.CurrentAccount != null) {
                this.confirmEmail = this.CurrentAccount.email;
            }
            else {
                this.confirmEmail = "";
            }
            this.oldPassword = "";
            this.newPassword = "";
            this.confirmNewPassword = "";

            this.isExpanded = true;
        }    
    }

    CloseEditAccount() {
        this.ShowPasswords = false;
        this.oldPassword = "";
        this.newPassword = "";
        this.confirmNewPassword = "";
        this.isExpanded = false;
	}


    async SaveAccount() {
        if (this.CurrentAccount && this.CheckPassword) { // I changed how CheckPassword works by using get
            let result = await this.AccountManagerService.UpdateAccount(this.userAccountService.reviewerIdentity.userId, this.CurrentAccount.contactName,
                this.CurrentAccount.username, this.CurrentAccount.email, this.oldPassword, this.newPassword);
            if (result == true) {
                // close div and put up a message saying the account was updated 
                this.isExpanded = false;
                this.showAccountUpdatedNotification();
                this.ReviewerIdentityServ.reviewerIdentity.name = this.CurrentAccount.contactName;
            }
        }
    }

    private showAccountUpdatedNotification(): void {
        let contentSt: string = "Accounts details updated";
        this.notificationService.show({
            content: contentSt,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "success", icon: true },
            closable: true
        });
    }

    private passw = "^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$";

    get CheckPassword(): boolean { 
        if (this.newPassword.length == 0) {
            return true // hide warning
        }
        else {
            if ( this.newPassword.match(this.passw)) {
                return true;
            }
            else {
                return false;
            }
        }
    }



    get OldPasswordNeeded(): boolean { //warning is hidden when this returns "true"
        if (this.oldPassword.length == 0
            && (this.newPassword.length > 0 || this.confirmNewPassword.length > 0)
            ) return false; //show the warning
        else return true; //hide the wening

        //if ((this.newPassword.length == 0) && (this.confirmNewPassword.length == 0)) {
        //    return true; // hide warning
        //}
        //else if ((this.newPassword.length > 0) && (this.confirmNewPassword.length > 0) && this.oldPassword.length > 0) {
        //    return true;
        //}
        //else {
        //    return false;
        //}
    }

    //get IsolatedPassword(): boolean { // disable save button if only the oldPassword box has data
    //    if ((this.newPassword.length == 0) && (this.confirmNewPassword.length == 0) && this.oldPassword.length > 0) {
    //        return true;
    //    }
    //    else {
    //        return false;
    //    }
    //}


}
