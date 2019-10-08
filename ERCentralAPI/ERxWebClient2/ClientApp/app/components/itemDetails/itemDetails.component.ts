import { Component, Inject, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Event } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { Item, ItemListService, iAdditionalItemDetails } from '../services/ItemList.service';
import { ReviewerTermsService, ReviewerTerm } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ModalService } from '../services/modal.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { TextSelectEvent } from "../helpers/text-select.directive";



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
        private ModalService: ModalService
    ) {}

    @Input() item: Item | undefined;
    @Input() ShowHighlights: boolean = false;
    @Input() CanEdit: boolean = false;
    @Input() IsScreening: boolean = false;
    @Input() ShowDocViewButton: boolean = true;
    public HAbstract: string = "";
    public HTitle: string = "";
    public showOptionalFields = false;

	private eventsTest: Subject<void> = new Subject<void>();
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


		this.hostRectangle = null;
		this.selectedText = "";

	}

	// I render the rectangles emitted by the [textSelect] directive.
	public renderRectangles(event: TextSelectEvent): void {

		//console.group("Text Select Event");
		console.log("Text:", event.text);

		//console.log("Viewport Rectangle:", event.viewportRectangle);
		//console.log("Host Rectangle:", event.hostRectangle);
		console.groupEnd();

		// If a new selection has been created, the viewport and host rectangles will
		// exist. Or, if a selection is being removed, the rectangles will be null.
		if (event.hostRectangle) {

			this.hostRectangle = event.hostRectangle;
			this.selectedText = event.text;

		} else {

			this.hostRectangle = null;
			this.selectedText = "";

		}
	}


	// I share the selected text with friends :)
	public shareSelection(): void {

		console.group("Shared Text");
		console.log(this.selectedText);
		console.groupEnd();

		if (document != null ) {
			if (document.getSelection() != null) {
				var selection = document.getSelection(); 
				if (selection != null) {
					selection.removeAllRanges();
				}
			}
		}
		// Now that we've shared the text, let's clear the current selection.

		// CAUTION: In modern browsers, the above call triggers a "selectionchange"
		// event, which implicitly calls our renderRectangles() callback. However,
		// in IE, the above call doesn't appear to trigger the "selectionchange"
		// event. As such, we need to remove the host rectangle explicitly.
		this.hostRectangle = null;
		this.selectedText = "";
	}
	//======================================================
	Changed() {
	//	alert('item changed');
	//	//this.eventsTest.next();
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
	private selectedRange: string = '';
	public RemoveTerm() {

		var findTerm = this.ReviewerTermsService.TermsList
			.find(x => x.reviewerTerm == this.selectedText);

		console.log('findTerm: ',findTerm);
		if (findTerm) {
			this.ReviewerTermsService.DeleteTerm(findTerm.trainingReviewerTermId);
		}

	}
	public AddRelevantTerm(addRemoveBtn: boolean) {
		if (this.selectedText != null) {
			
			let s: string = this.selectedText.trim().toLowerCase();
			if (s == null || s.length == 0) return;
			if (s.length > 50) return;
			let terms: string[] = s.split(" ", 50);
			console.log('terms: ' + terms);			//removing empty from the abvoe array could happen here if there are any present
			for (var i = 0; i < terms.length; i++) {

				var term = terms[i];
				console.log(term + '\n');
				//checks whether term is in the list already...
				let cTrt: ReviewerTerm | null = this.FindTerm(term);

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
					}	//TODO
					else {//term is already there, see if we need to flip the Included flag

						if (
							(cTrt.included || !cTrt.included)//adding as negative, but it's already there as positive
						) {
							cTrt.included = !cTrt.included;
							//api call when everything above is correct
							//cTrt.BeginSave(true);
						}
					}

				this.ItemListService.getItem(this.ItemListService.currentItem.itemId);
				this.RefreshHighlights();
			}
		}
	}

	public FindTerm(term: string): ReviewerTerm | null {

		// TODO
		return null;
	}

	public RefreshHighlights() {

		this.ReviewerTermsService.Fetch();
//		this.SetHighlights();

	}

    public SetHighlights() {
        if (this.item && this.ReviewerTermsService && this.ReviewerTermsService.TermsList.length > 0) {
            this.HTitle = this.item.title;
			this.HAbstract = this.item.abstract;
			console.log('set highlights called: ' + this.HAbstract);
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






