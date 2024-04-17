import { OnInit, Component, Input, OnDestroy, EventEmitter, Output } from "@angular/core";
import { Router } from "@angular/router";
import { ReviewerIdentityService } from "../services/revieweridentity.service";
import { ReviewerTermsService, ReviewerTerm } from "../services/ReviewerTerms.service";
import {  Item } from "../services/ItemList.service";
import { ItemCodingService } from "../services/ItemCoding.service";
import { ConfirmationDialogService } from "../services/confirmation-dialog.service";

@Component({
	selector: 'ReviewTermsListComp',
	templateUrl: './ReviewTermsListComp.component.html',
})

export class ReviewTermsListComp implements OnInit, OnDestroy {

	constructor(private router: Router,
		private ReviewerIdentityServ: ReviewerIdentityService,
		private ReviewTermsServ: ReviewerTermsService,
    private ItemCodingService: ItemCodingService,
    private ConfirmationDialogService: ConfirmationDialogService,
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
	public get TermsList(): ReviewerTerm[] {
		return this.ReviewTermsServ.TermsList;
	}
  public SaveChanges() {
    //will save all changed terms, but ask for confirmation is there is more than one change to save...
    const termsToSave = this.ReviewerTermsService.TermsList.filter(f=> f.CanSave == true)
    if (termsToSave.length > 1) {
      this.ConfirmationDialogService.confirm("Save multiple terms?", "This will save changes to <strong>" + termsToSave.length.toString() + " terms</strong>."
        , false, "", "Save all", "Cancel").then(
          (confirmed: any) => {
            if (confirmed == true) {
              this.ActuallySaveTheseChanges(termsToSave);
            }
          }
        ).catch();
    }
    else this.ActuallySaveTheseChanges(termsToSave);
	}
  private async ActuallySaveTheseChanges(terms: ReviewerTerm[]) {
    const lastTerm = terms[terms.length - 1];
    for (const term of terms) {
      if (term === lastTerm) await this.ReviewTermsServ.UpdateTerm(term, true);
      else await this.ReviewTermsServ.UpdateTerm(term, false);
    }
  }
	ngOnDestroy() {

		this.Clear();

	}
	public Clear() {

		this.ReviewerTermsService._ShowHideTermsList = false;
		this.ShowNewTermPanel = false;
	}
	public ShowNewTermPanel: boolean = false;
	public OpenRowPanel() {
		this.ShowNewTermPanel = !this.ShowNewTermPanel;

	}
	public Remove(term: ReviewerTerm) {
		if (term) {
			this.ReviewTermsServ.DeleteTerm(term.trainingReviewerTermId);
			if (this.item) {
				this.ItemCodingService.Fetch(this.item.itemId);
			}
		}
	}
	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
	}

	public newReviewTerm: string = '';
	public newReviewIncluded: boolean = false;

	public InsertNewRow() {

		if (this.newReviewTerm != '') {
			let newTerm: ReviewerTerm = new ReviewerTerm(this.newReviewTerm);
			newTerm.included = this.newReviewIncluded;
			this.ReviewTermsServ.CreateTerm(newTerm);
			//this.ReviewTermsServ.TermsList.push(newTerm);
			this.newReviewTerm = '';
			this.newReviewIncluded = false;
			this.ShowNewTermPanel = false;
		}
	}
}
