import { OnInit, Component } from "@angular/core";
import { Router } from "@angular/router";
import { ReviewerIdentityService } from "../services/revieweridentity.service";
import { ReviewerTermsService, ReviewerTerm } from "../services/ReviewerTerms.service";
import { FormArray, FormGroup, Validators, FormControl } from "@angular/forms";


@Component({
	selector: 'ReviewTermsListComp',
	templateUrl: './ReviewTermsListComp.component.html',
})

export class ReviewTermsListComp implements OnInit {

	constructor(private router: Router,
		private ReviewerIdentityServ: ReviewerIdentityService,
		private ReviewTermsServ: ReviewerTermsService

	) {

	}
	ngOnInit() {

		if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
			this.router.navigate(['home']);
		}
		else {

		}
	}
	// add update and delete method calls to the api here
	// call teh refresh of the item after
	// 		if (this.item) {
			//this.ItemCodingService.Fetch(this.item.itemId);
		//}

}
 