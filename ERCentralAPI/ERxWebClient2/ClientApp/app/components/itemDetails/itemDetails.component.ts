import { Component, Inject, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Item, ItemListService, iAdditionalItemDetails } from '../services/ItemList.service';
import { ReviewerTermsService, ReviewerTerm } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ModalService } from '../services/modal.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { TextSelectEvent } from "../helpers/text-select.directive";
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';

// COPYRIGHTS BELONG TO THE FOLLOWING FOR ABILITY TO SELECT TEXT AND CAPTURE EVENT
// https://www.bennadel.com/blog/3439-creating-a-medium-inspired-text-selection-directive-in-angular-5-2-10.htm

@Component({
    selector: 'itemDetailsComp',
    templateUrl: './itemDetails.component.html',
    providers: [],
    styles: []
})
export class itemDetailsComp implements OnInit {

    constructor(
        private router: Router,
        private ReviewerTermsService: ReviewerTermsService,
        public ItemDocsService: ItemDocsService,
        private PriorityScreeningService: PriorityScreeningService,
        private ItemListService: ItemListService,
		private ModalService: ModalService,
		private ItemCodingService: ItemCodingService,
		private ReviewerIdentityServ: ReviewerIdentityService
    ) {}

    @Input() item: Item | undefined;
    @Input() ShowHighlights: boolean = false;
    @Input() CanEdit: boolean = false;
    @Input() IsScreening: boolean = false;
	@Input() ShowDocViewButton: boolean = true;
	@Input() Context: string = "CodingFull";
    public HAbstract: string = "";
    public HTitle: string = "";
    public showOptionalFields = false;

    public get CurrentItemAdditionalData(): iAdditionalItemDetails | null {
        if (this.IsScreening) {
            return this.PriorityScreeningService.CurrentItemAdditionalData;
        }
        else {
            return this.ItemListService.CurrentItemAdditionalData;
        }
	}
	public hostRectangle!: SelectionRectangle | null;

	private selectedText!: string;

	ngOnInit() {

		console.log(this.item);
		this.hostRectangle = null;
		this.selectedText = "";

	}

	public renderRectangles(event: TextSelectEvent): void {

		console.groupEnd();

		if (event.hostRectangle) {

			this.hostRectangle = event.hostRectangle;
			this.selectedText = event.text;

		} else {

			this.hostRectangle = null;
			this.selectedText = "";

		}
	}

    public WipeHighlights() {
        this.HAbstract = "";
        this.HTitle = "";
    }
	ShowHighlightsClicked() {

        if (this.item && this.ShowHighlights && this.HAbstract == '' && !(this.item.abstract == '')) {
            this.SetHighlights();
		}
		this.ShowHighlights = !this.ShowHighlights;
	}
	ItemChanged() {
		alert('item changed!!');
    }
    EditItem() {
        if (this.item) {
            this.router.navigate(['EditItem', this.item.itemId], { queryParams: { return: 'itemcoding/' + this.item.itemId.toString() } });
        }
    }
    
    public FindReferenceOnMicrosoftAcademic(item: Item) {
        if (item != null) {
            let searchString: string = "\"" + item.title + "\" " + item.authors;
            window.open("https://academic.microsoft.com/search?q=" +
                encodeURIComponent(searchString));
        }
    }
    public FindReferenceOnGoogle(item: Item) {
        if (item != null) {
            let searchString: string = "\"" + item.title + "\" " + item.authors;
            window.open("https://www.google.com/search?q=" + searchString);
        }
    }

    public FindReferenceOnGoogleScholar(item: Item) {
        if (item != null) {
            let searchString: string = "\"" + item.title + "\" " + item.authors;
            window.open("https://scholar.google.com/scholar?q=" + searchString);
        }
	}
	public RemoveTerm() {

		if (this.selectedText != null && this.selectedText != ''
			&& this.ReviewerTermsService.TermsList.length > 0) {
		
			var findTerm = this.ReviewerTermsService.TermsList
				.find(x => x.reviewerTerm == this.selectedText);

			if (findTerm) {
				this.ReviewerTermsService.DeleteTerm(findTerm.trainingReviewerTermId);
			}
			this.RefreshHighlights();
			this.selectedText = '';
		}

	}
	public AddRelevantTerm(addRemoveBtn: boolean) {

		if (this.selectedText != null && this.selectedText != '') {
			
			let s: string = this.selectedText.trim().toLowerCase();
			if (s == null || s.length == 0) return;
			if (s.length > 50) return;
			let terms: string[] = s.split(" ", 50);
			for (var i = 0; i < terms.length; i++) {

				var term = terms[i];
				console.log(term + '\n');
				let cTrt: ReviewerTerm | null = this.FindTerm(term);
				//console.log('CTrt not null: ', cTrt);
				if (cTrt == null) {
					let trt: ReviewerTerm = {} as ReviewerTerm;
					trt.reviewerTerm = term;
					trt.included = addRemoveBtn;
					trt.itemTermDictionaryId = 0;
					trt.trainingReviewerTermId = 0;
					trt.term = term;
						
					if (this.ReviewerTermsService.TermsList != null) {
						this.ReviewerTermsService.TermsList.push(trt as ReviewerTerm);
						this.ReviewerTermsService.CreateTerm(trt);
					}
				}	
				else {//term is already there, see if we need to flip the Included flag
					if (
						
						(cTrt.included && !addRemoveBtn)//adding as negative, but it's already there as positive
						||
						(!cTrt.included && addRemoveBtn)
					) {
						cTrt.included = !cTrt.included;
						this.ReviewerTermsService.UpdateTerm(cTrt);
					}
				}
				this.RefreshHighlights();
				this.selectedText = '';
			}
		}
	}

	public FindTerm(term: string): ReviewerTerm | null {

		var ind = this.ReviewerTermsService.TermsList.findIndex(x => x.reviewerTerm == term);
		if (ind != -1) {
			return this.ReviewerTermsService.TermsList[ind];
		} else {
			return null;
		}
	}
	
	public ShowHideTermsList() {

		this.ReviewerTermsService._ShowHideTermsList = !this.ReviewerTermsService._ShowHideTermsList;
	}

	public RefreshHighlights() {
		if (this.item) {
			this.ItemCodingService.Fetch(this.item.itemId);
		}
	}

	public SetHighlights() {
		if (this.item) {
			this.HTitle = this.item.title;
			this.HAbstract = this.item.abstract;
			if (this.ReviewerTermsService && this.ReviewerTermsService.TermsList.length > 0) {
				//console.log('set highlights called: ' + this.HAbstract);
				for (let term of this.ReviewerTermsService.TermsList) {
					//console.log('something to do with the terms list here: ' + this.ReviewerTermsService.TermsList);
					try {
						if (term.reviewerTerm && term.reviewerTerm.length > 0) {
							let lFirst = term.reviewerTerm.substr(0, 1);
							lFirst = lFirst.toLowerCase();
							let uFirst = lFirst.toUpperCase();
							let lTerm = lFirst + term.reviewerTerm.substr(1);
							let uTerm = uFirst + term.reviewerTerm.substr(1);

							let reg = new RegExp(this.cleanSpecialRegexChars(lTerm), "g");
							let reg2 = new RegExp(this.cleanSpecialRegexChars(uTerm), "g");
							if (term.included) {
								this.HTitle = this.HTitle.replace(reg, "<span class='RelevantTerm'>" + lTerm + "</span>");
								this.HTitle = this.HTitle.replace(reg2, "<span class='RelevantTerm'>" + uTerm + "</span>");
								this.HAbstract = this.HAbstract.replace(reg, "<span class='RelevantTerm'>" + lTerm + "</span>");
								this.HAbstract = this.HAbstract.replace(reg2, "<span class='RelevantTerm'>" + uTerm + "</span>");
							}
							else {
								this.HTitle = this.HTitle.replace(reg, "<span class='IrrelevantTerm'>" + lTerm + "</span>");
								this.HTitle = this.HTitle.replace(reg2, "<span class='IrrelevantTerm'>" + uTerm + "</span>");
								this.HAbstract = this.HAbstract.replace(reg, "<span class='IrrelevantTerm'>" + lTerm + "</span>");
								this.HAbstract = this.HAbstract.replace(reg2, "<span class='IrrelevantTerm'>" + uTerm + "</span>");
							}
						}
					}
					catch (error) {
						console.log(error);
						this.ModalService.GenericErrorMessage("Sorry, the terms-highlighting system has encountered a problem. Please inform EPPI-Support.");
					}
				}
			}
		}
	}
	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
	}
    private cleanSpecialRegexChars(input: string): string {
        //need to replace these: [\^$.|?*+(){}
        let result = input.replace(/\\/g, "\\\\");
        result = result.replace(/\[/g, "\\[");
        result = result.replace(/\^/g, "\\^");
        result = result.replace(/\$/g, "\\$");
        result = result.replace(/\./g, "\\.");
        result = result.replace(/\|/g, "\\|");
        result = result.replace(/\?/g, "\\?");
        result = result.replace(/\*/g, "\\*");
        result = result.replace(/\+/g, "\\+");
        result = result.replace(/\(/g, "\\(");
        result = result.replace(/\)/g, "\\)");
        result = result.replace(/\{/g, "\\{");
        result = result.replace(/\}/g, "\\}");
        //console.log(input, result);
        return result;
    }
    toHTML(text: string): string {
        return text.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
    }
    public FieldsByType(typeId: number) {
        return Helpers.FieldsByPubType(typeId);
    }
}

export class TrainingReviewerTerm {
	public reviewId: number=0;
	public reviewerTerm: string='';
	public included: boolean = false;
	public term: string='';
}

interface SelectionRectangle {
	left: number;
	top: number;
	width: number;
	height: number;
}






