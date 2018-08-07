import { Component, Inject } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
     ,providers: []

    //providers: [ReviewerIdentityService]
})
export class HomeComponent {
    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        ) {}
    
    onLogin(u: string, p:string) {
        //this.ReviewerIdentityServ.Login(u, p);

        this.ReviewerIdentityServ.LoginReq(u, p).subscribe(ri => {
            this.ReviewerIdentityServ.reviewerIdentity = ri;
            console.log('home login: ' + this.ReviewerIdentityServ.reviewerIdentity.userId);
            if (this.ReviewerIdentityServ.reviewerIdentity.userId > 0) {
                this.ReviewerIdentityServ.Save();
                this.router.navigate(['readonlyreviews']);
            }
        })
        
    };
    
}
