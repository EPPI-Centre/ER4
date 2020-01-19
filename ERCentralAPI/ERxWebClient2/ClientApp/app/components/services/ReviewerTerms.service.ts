import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({
    providedIn: 'root',
})

export class ReviewerTermsService extends BusyAwareService {
	   
    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
		super();
    }
	@Output() setHighlights: EventEmitter<boolean> = new EventEmitter();
    private _TermsList: ReviewerTerm[] = [];
    public get TermsList(): ReviewerTerm[] {
        return this._TermsList;
	}
	public _ShowHideTermsList: boolean = false;

    public Fetch() {

		this._BusyMethods.push("Fetch");
        return this._httpC.get<ReviewerTerm[]>(this._baseUrl + 'api/ReviewerTermList/Fetch').subscribe(result => {
			this._TermsList = result;
			this.setHighlights.emit();
			this.RemoveBusy("Fetch");
        },
			error => {
				this.modalService.GenericError(error);
				this.RemoveBusy("Fetch");
			},
			() => {
				this.RemoveBusy("Fetch");
			}
        );
	}

	public CreateTerm(trt: ReviewerTerm) {

		this._BusyMethods.push("CreateTerm");
		let body = JSON.stringify( trt );
		return this._httpC.post<ReviewerTerm>(this._baseUrl + 'api/ReviewerTermList/CreateReviewerTerm',
		body)
			.subscribe(result => {
				this._TermsList.push(result)
				this.Fetch();
				this.RemoveBusy("CreateTerm");

		},
			error => {
				this.modalService.GenericError(error);
				this.RemoveBusy("CreateTerm");
			},
			() => {
				this.RemoveBusy("CreateTerm");
			}
		);
	}

	public DeleteTerm(termId: number) {

		this._BusyMethods.push("DeleteTerm");
		let body = JSON.stringify({ Value: termId });
		return this._httpC.post<ReviewerTerm>(this._baseUrl + 'api/ReviewerTermList/DeleteReviewerTerm',
			body)
			.subscribe(result => {
			
				let ind: number = this._TermsList.findIndex(x => x.trainingReviewerTermId == result.trainingReviewerTermId);
				this._TermsList.splice(ind, 1);
				this.Fetch();
				this.RemoveBusy("DeleteTerm");

			},
			error => {

					this.modalService.GenericError(error);
					this.RemoveBusy("DeleteTerm");
			}, () => {
				this.RemoveBusy("DeleteTerm");
			}
		);
	}

	public UpdateTerm(term: ReviewerTerm) {

		this._BusyMethods.push("UpdateTerm");

		let body = JSON.stringify(term);
		return this._httpC.post<ReviewerTerm>(this._baseUrl + 'api/ReviewerTermList/UpdateReviewerTerm',
			body)
			.subscribe(result => {

				let ind: number = this._TermsList.findIndex(x => x.trainingReviewerTermId == term.trainingReviewerTermId);
				this._TermsList[ind] = result;
				this.Fetch();
				this.RemoveBusy("UpdateTerm");
				//return this.TermsList;
			},
				error => {

					this.modalService.GenericError(error);
					this.RemoveBusy("UpdateTerm");
			}, () => {
					this.RemoveBusy("UpdateTerm");
			}
		);
	}
}
export interface ReviewerTerm {
    trainingReviewerTermId: number;
    itemTermDictionaryId: number;
    reviewerTerm: string;
    included: boolean;
    term: string;
}