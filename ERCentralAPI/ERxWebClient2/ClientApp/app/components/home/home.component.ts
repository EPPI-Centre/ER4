import { Component, Inject } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../app/revieweridentity.service';

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
     ,providers: []

    //providers: [ReviewerIdentityService]
})
export class HomeComponent {
    constructor(private router: Router, private ReviewerIdentity: ReviewerIdentityService) { }//, @Inject(ReviewerIdentityService) private ReviewerIdentity: ReviewerIdentityService) { }
    onSubmit(f: string) {
        this.ReviewerIdentity.ContactId = +f;
        this.router.navigate(['readonlyreviews'])
        console.log(f);  
        //console.log(this.ReviewerIdentity.ContactId); 
        //this.ReviewerIdentity.Report();
    }
}
