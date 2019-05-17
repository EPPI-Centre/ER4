import { Component, Inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ReviewerIdentityService, ReviewerIdentity, iCreateER4ContactViaArchieCommandJSON } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { NotificationService } from '@progress/kendo-angular-notification';
import { isString } from '@ng-bootstrap/ng-bootstrap/util/util';

@Component({
    selector: 'ArchieCallBack',
    templateUrl: './ArchieCallBack.component.html'
})
export class ArchieCallBackComponent implements OnInit {
    constructor(private router: Router,
        private ReviewerIdentityServ: ReviewerIdentityService,
        @Inject('BASE_URL') private _baseUrl: string,
        private NotificationService: NotificationService,
        private route: ActivatedRoute
    ) {
        //this.ReviewerIdentityServ.LoginFailed.subscribe(() => this.LoginFailed());
    }
    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            console.log("Params:", params);
            this.State = params['state'];
            this.Code = params['code'];
            if (params['error']) this.Error = params['error'];
            if (this.Error != "") this.Phase = "Error";
            else {
                this.ReviewerIdentityServ.LoginViaArchieReq(this.Code, this.State).then(res => {
                    console.log("Back into callback:", res);
                    if (res == undefined) {
                        console.log("Back into LoginViaArchieReq callback:", 1);
                        this.Phase = "Error";
                        this.Error = "Authentication failed in an unexpected way, please try again. If the problem persists, please contact EPPISupport@ucl.ac.uk."
                    }
                    else if (res.reviewId == -1 && res.userId == -1 && res.name.startsWith("{ERROR: In ")) {
                        //this is an error managed on the API call handler, condition above is the signal for it
                        console.log("Back into LoginViaArchieReq callback:", 2, res.name, res.ticket);
                        this.Error = "Authentication failed with an error. " + res.name.substr(1, res.name.length - 2) + ". Error details are: " + res.ticket;
                        this.Error += " Please try again. If the error persists, please contact EPPISupport@ucl.ac.uk.";
                        this.Phase = "Error";
                    }
                    else if ((res as ReviewerIdentity) && (res as ReviewerIdentity).name == "{UnidentifiedArchieUser}") {
                        //we need to link the Archie user to an existing or new ER user
                        console.log("Back into LoginViaArchieReq callback:", 3);
                        this.Phase = "LinkOrCreateAccount";
                    }
                    else  {
                        //oh? Should not happen...
                        console.log("Back into LoginViaArchieReq callback:", 4);
                        this.Phase = "Error";
                        this.Error = "Authentication failed in an unexpected way, please try again. If the problem persists, please contact EPPISupport@ucl.ac.uk."
                    }
                    //no more elses: if authentication worked and is done, ReviewerIdentityServ brings the user on...
                });
            }
        });
    }
    public Phase = "Start";
    public State = "";
    public Code = "";
    public Error = "";
    public ShowLoginFailed: boolean = false;
    public ShowUsernameRequired: boolean = false;
    public ShowPasswordRequired: boolean = false;
    public CreateAccountCommand: iCreateER4ContactViaArchieCommandJSON = this.newCreateAccountCommand();
    public ConfirmEmail: string = "";
    public ConfirmPw: string = "";
    public ErrorrMsgCreateAccount: string = "";
    private newCreateAccountCommand(): iCreateER4ContactViaArchieCommandJSON {
        let res = {
            code: "",
            status: "",
            username: "",
            email: "",
            fullname: "",
            password: "",
            sendNewsletter: false,
            createExampleReview: false,
            result: ""
        };
        this.ConfirmEmail = "";
        this.ConfirmPw = "";
        return res;
    }
    //public get FirstNameIsValid() {

    //}
    public FieldNeedsRevision(fieldN: string): boolean {
        console.log("FieldNeedsRevision", fieldN, this.ErrorrMsgCreateAccount.indexOf(fieldN) != -1, this.ErrorrMsgCreateAccount);
        return this.ErrorrMsgCreateAccount.indexOf(fieldN) != -1;
    }
    GoToLinkAccount() {
        this.ShowLoginFailed = false;
        this.ShowUsernameRequired = false;
        this.ShowPasswordRequired = false;
        this.Phase = "LinkAccount";
    }
    GoToCreateNewAccount() {
        this.ShowLoginFailed = false;
        this.ShowUsernameRequired = false;
        this.ShowPasswordRequired = false;
        this.Phase = "CreateNewAccount";
    }
    BackToMainChoice() {
        this.ShowLoginFailed = false;
        this.ShowUsernameRequired = false;
        this.ShowPasswordRequired = false;
        this.Phase = "LinkOrCreateAccount";
    }
    BackHome() {
        this.router.navigate(['home']);
    }
    onLogin(un: string, pw: string) {
        this.ShowLoginFailed = false;
        this.ShowUsernameRequired = false;
        this.ShowPasswordRequired = false;
        if (un.length < 2) {
            this.ShowUsernameRequired = true;
            return;
        }
        if (pw.length < 6) {
            this.ShowPasswordRequired = true;
            return;
        }
        this.ReviewerIdentityServ.LinkToArchieAccount(this.Code, this.State, un, pw).then(res => {
            console.log("Back into callback (linking):", res);
            if (res == undefined || (res.reviewerIdentity == null && res.error == "")) {
                console.log("Back into callback:", 1);
                this.Phase = "Error";
                this.Error = "Authentication failed in an unexpected way, please try again. If the problem persists, please contact EPPI-Support."
            }
            else if (res.error != "") {
                //we need to link the Archie user to an existing or new ER user
                console.log("Back into callback error:", 2, res.error);
                if (res.error == "Login Failed") {
                    this.ShowLoginFailed = true;
                }
                else {
                    this.Phase = "Error";
                    this.Error = "Authentication failed, the error details is: " + res.error + ". Please try again. If the problem persists please contact EPPI-Support.";
                }
            }
            else {
                this.NotificationService.show({
                    content: "Your EPPI-Reviewer and Archie Accounts have been Linked!",
                    animation: { type: 'slide', duration: 400 },
                    position: { horizontal: 'center', vertical: 'top' },
                    type: { style: "success", icon: true },
                    closable: true
                });
                this.router.navigate(['intropage']);
            }
        });
    }
    CheckAndCreateNewAccount() {
        this.ErrorrMsgCreateAccount = "";
        console.log("CheckAndCreateNewAccount");
        //fill in missing fields:
        this.CreateAccountCommand.code = this.Code;
        this.CreateAccountCommand.createExampleReview = false;
        this.CreateAccountCommand.sendNewsletter = false;
        this.CreateAccountCommand.status = this.State;
        //trim user-defined fields
        this.CreateAccountCommand.email = this.CreateAccountCommand.email.trim();
        this.CreateAccountCommand.fullname = this.CreateAccountCommand.fullname.trim();
        this.CreateAccountCommand.password = this.CreateAccountCommand.password.trim();
        this.ConfirmEmail = this.ConfirmEmail.trim();
        this.ConfirmPw = this.ConfirmPw.trim();
        this.CreateAccountCommand.username = this.CreateAccountCommand.username.trim();

        if (this.CreateAccountCommand.fullname.length < 2) {
            console.log("don't like this FN: ", this.CreateAccountCommand.fullname);
            this.ErrorrMsgCreateAccount = ".1";
            //tboxFirstname.Background = scb;
        }
        
        //if (this.Lastname.trim().length < 1) {
        //    console.log("don't like this ln: ", this.Lastname);
        //    this.ErrorrMsgCreateAccount += ".2";
        //    //tboxLastname.Background = scb;
        //}
        if (this.CreateAccountCommand.username.length < 4) {
            console.log("don't like this UN: ", this.CreateAccountCommand.username);
            this.ErrorrMsgCreateAccount += ".3";
        }
        
        if (this.CreateAccountCommand.email.length < 1 //too short
            ||
            ((this.CreateAccountCommand.email.indexOf("@") < 2) ||
            (this.CreateAccountCommand.email.indexOf("@") >= this.CreateAccountCommand.email.length - 1)
            )//@ symbol absent or too close to the string edges
            ||
            ((this.CreateAccountCommand.email.lastIndexOf(".") < 2) ||
            (this.CreateAccountCommand.email.lastIndexOf(".") >= this.CreateAccountCommand.email.length - 1)
            )//. symbol absent or too close to the string edges
        ) {

            console.log("don't like this EMAIL: ", this.CreateAccountCommand.email);
            this.ErrorrMsgCreateAccount += ".4";
        }
        else if (this.CreateAccountCommand.email != this.ConfirmEmail) {
            console.log("don't like this EMAIL 2: ", this.CreateAccountCommand.email, this.ConfirmEmail);
            this.ErrorrMsgCreateAccount += ".4";
        }
        let indInSt = this.CreateAccountCommand.password.search(/^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$/);//regex for password pattern, straight from ER4.
        if (indInSt != -1 || this.CreateAccountCommand.password.length < 8 || this.CreateAccountCommand.password != this.ConfirmPw) {
            console.log("don't like this PW: ", this.CreateAccountCommand.password, this.ConfirmPw);
            this.ErrorrMsgCreateAccount += ".5";
        }
        if (this.ErrorrMsgCreateAccount != "") {
            //one of the tests failed, so we won't send anything to API...
            return;
        }
        //if we got here, all client side checks worked out.
        this.ReviewerIdentityServ.CreateERAccountFromArchie(this.CreateAccountCommand).then(res => this.HandleCreateERAccountFromArchieResult(res));
    }
    HandleCreateERAccountFromArchieResult(res: iCreateER4ContactViaArchieCommandJSON) {
        this.ErrorrMsgCreateAccount = "";
        if (res.result == "Done") {
            //all is well, let's move on...
            //cheat! to avoid adding another login API endpoint (find RI via oAuth code/status but using them within the DB), 
            //this.ReviewerIdentityServ.LoginViaArchieReq checks if user is partially authenticated and a CochraneUser,
            //if so it will send the RI with the request, and the Controller will react accordingly
            this.ReviewerIdentityServ.LoginViaArchieReq(this.Code, this.State);
        }
        else if (res.result == "Name is missing or too short") {
            this.ErrorrMsgCreateAccount = ".1";
        }
        else if (res.result == "Username is missing or too short") {
            this.ErrorrMsgCreateAccount = ".3";
        }
        else if (res.result == "Email is missing or invalid") {
            this.ErrorrMsgCreateAccount = ".4";
        }
        else if (res.result == "Password is missing or invalid") {
            this.ErrorrMsgCreateAccount = ".5";
        }
        else if (res.result == "Username is already in use.") {
            this.ErrorrMsgCreateAccount = ".3";
            this.NotificationService.show({
                content: "Your chosen EPPI-Reviewer username is already in use!",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "error", icon: true },
                closable: true
            });
        }
        else if (res.result == "Email is already in use. Please select another or link to the (already) existing account.") {
            this.ErrorrMsgCreateAccount = ".4";
            this.NotificationService.show({
                content: res.result,
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "error", icon: true },
                closable: true
            });
        }
        else if (res.result == "Failed to create account, please contact EPPISupport@ioe.ac.uk") {
            this.NotificationService.show({
                content: "Failed to create your EPPI-Reviewer account, please contact EPPISupport@ucl.ac.uk!",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "error", icon: true },
                closable: true
            });
            this.BackHome();
        }
        else if (res.result == "Account not created: failed to link to your Archie Identity") {
            this.NotificationService.show({
                content: res.result + ". Please contact EPPISupport@ucl.ac.uk!",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "error", icon: true },
                closable: true
            });
            this.BackHome();
        }
        else if (res.result == "Account not created: failed to generate the activation link") {
            this.NotificationService.show({
                content: res.result + ". Please contact EPPISupport@ucl.ac.uk!",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "error", icon: true },
                closable: true
            });
            this.BackHome();
        }
        else if (res.result == "No response from 'CreateERAccountFromArchie' API call.") {
            this.NotificationService.show({
                content: "The attempt did not complete in the allocated time (no response). Please try again.",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "error", icon: true },
                closable: true
            });
        }
        else if (res.result == "Login Failed" || res.result == "Not Authorised") {
            this.NotificationService.show({
                content: "Account not created: could not authenticate your request. If the problem persists, please contact EPPISupport@ucl.ac.uk!",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "error", icon: true },
                closable: true
            });
            this.BackHome();
        }
        else if (res.result.startsWith("Unexpected error")) {
            this.NotificationService.show({
                content: res.result + ". Please contact EPPISupport@ucl.ac.uk!",
                animation: { type: 'slide', duration: 400 },
                position: { horizontal: 'center', vertical: 'top' },
                type: { style: "error", icon: true },
                closable: true
            });
            this.BackHome();
        }
    }
}

