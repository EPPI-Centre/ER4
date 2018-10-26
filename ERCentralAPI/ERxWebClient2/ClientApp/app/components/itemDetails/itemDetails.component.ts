import { Component, Inject, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { Item } from '../services/ItemList.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';



@Component({
    selector: 'itemDetailsComp',
    templateUrl: './itemDetails.component.html',
    providers: [],
    styles: []
})
export class itemDetailsComp implements OnInit {

    constructor(private ReviewerTermsService: ReviewerTermsService,
        public ItemDocsService: ItemDocsService
    ) {}

    @Input() item: Item | undefined;
    @Input() ShowHighlights: boolean = false;
    public HAbstract: string = "";
    public HTitle: string = "";
	ngOnInit() {

    }

    public WipeHighlights() {
        this.HAbstract = "";
        this.HTitle = "";
    }
    ShowHighlightsClicked() {
        if (this.item && this.ShowHighlights && this.HAbstract == '' && !(this.item.abstract == '')) {
            this.SetHighlights();
        }
    }
    public SetHighlights() {
        if (this.item && this.ReviewerTermsService && this.ReviewerTermsService.TermsList.length > 0) {
            this.HTitle = this.item.title;
            this.HAbstract = this.item.abstract;
            for (let term of this.ReviewerTermsService.TermsList) {
                if (term.reviewerTerm && term.reviewerTerm.length > 0) {
                   
                    let lFirst = term.reviewerTerm.substr(0, 1);
                    lFirst = lFirst.toLowerCase();
                    let uFirst = lFirst.toUpperCase();
                    let lTerm = lFirst + term.reviewerTerm.substr(1);
                    let uTerm = uFirst + term.reviewerTerm.substr(1);
                  
                    let reg = new RegExp(lTerm, "g");
                    let reg2 = new RegExp(uTerm, "g");
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
        }
    }
    toHTML(text: string): string {
        return text.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
    }
}






