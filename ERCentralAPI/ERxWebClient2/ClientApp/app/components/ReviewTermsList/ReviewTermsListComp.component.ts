import { OnInit, Component, Input } from "@angular/core";
import { Router } from "@angular/router";
import { ReviewerIdentityService } from "../services/revieweridentity.service";
import { ReviewerTermsService, ReviewerTerm } from "../services/ReviewerTerms.service";

import { ItemListService, Item } from "../services/ItemList.service";
import { ItemCodingService } from "../services/ItemCoding.service";

@Component({
	selector: 'ReviewTermsListComp',
	templateUrl: './ReviewTermsListComp.component.html',
})

export class ReviewTermsListComp implements OnInit {

	constructor(private router: Router,
		private ReviewerIdentityServ: ReviewerIdentityService,
		private ReviewTermsServ: ReviewerTermsService,
		private ItemListServ: ItemListService,
		private ItemCodingService: ItemCodingService,
		private ReviewerTermsService: ReviewerTermsService

	) {

	}
	@Input() item: Item | undefined;
	ngOnInit() {

		if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
			this.router.navigate(['home']);
		}
		else {
		}
	}
	public Update(term: ReviewerTerm) {
		if (term) {
			this.ReviewTermsServ.UpdateTerm(term);
			this.ReviewerTermsService.Fetch();
			if (this.item) {
				this.ItemCodingService.Fetch(this.item.itemId);
			}

		}
	}
	public Remove(term: ReviewerTerm) {
		if (term) {
			this.ReviewTermsServ.DeleteTerm(term.trainingReviewerTermId);
			this.ReviewerTermsService.Fetch();
			if (this.item) {
				console.log('got in here', this.item);
				this.ItemCodingService.Fetch(this.item.itemId);
			}
		}
	}
}
 