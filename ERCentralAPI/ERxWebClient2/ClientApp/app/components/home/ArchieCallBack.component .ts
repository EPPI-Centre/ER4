import { Component, Inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ReviewerIdentityService, ReviewerIdentity } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';

@Component({
    selector: 'ArchieCallBack',
    templateUrl: './ArchieCallBack.component.html'
})
export class ArchieCallBackComponent implements OnInit {
    constructor(private router: Router,
        private ReviewerIdentityServ: ReviewerIdentityService,
        @Inject('BASE_URL') private _baseUrl: string,
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
                this.ReviewerIdentityServ.LoginViaArchieReq(this.Code, this.State);
            }
        });
    }
    public Phase = "Start";
    public State = "";
    public Code = "";
    public Error = "";
    
}

