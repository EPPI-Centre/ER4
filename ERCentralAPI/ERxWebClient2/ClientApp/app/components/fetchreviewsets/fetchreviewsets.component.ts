import { Component, Inject, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet } from '../services/ReviewSets.service';
import { ITreeOptions } from 'angular-tree-component';
import { NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
    selector: 'reviewsets',
    styles: [`
                .ScreeningSet { color: #00BB00;} 
                .AdminSet { color: #0000DD;} 
                .ExcludeCode { color: #FF7777;} 
                .bt-infoBox {    
                    padding: .08rem .12rem .12rem .12rem;
                    margin-bottom: .12rem;
                    font-size: .875rem;
                    line-height: 1.2;
                    border-radius: .2rem;
                }
        `],
    templateUrl: './fetchreviewsets.component.html'
})
export class ReviewSetsComponent implements OnInit {
   constructor(private router: Router,
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
       private ReviewSetsService: ReviewSetsService,
       private modalService: NgbModal
    ) { }

    

    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else {
            //console.log("Review Ticket: " + this.ReviewerIdentityServ.reviewerIdentity.ticket);
            this.GetReviewSets();
        }
    }
    options: ITreeOptions = {
        childrenField: 'attributes', 
        displayField: 'name',
        allowDrag: false,
    }
    //nodes: singleNode[] = [];
    get nodes(): singleNode[] | null {
        //console.log('Getting codetree nodes');
        if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) 
        {
            //console.log('found my nodes');
            return this.ReviewSetsService.ReviewSets;
        }
        else {
            //console.log('NO nodes');
            return null;
        }
    }
    GetReviewSets() {
        if (this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) return;
        this.ReviewSetsService.GetReviewSets();
            //.subscribe(
            //result => {
            //    this.ReviewSetsService.ReviewSets = result;
            //    this.nodes = this.ReviewSetsService.ReviewSets;// as singleNode[];
            //}, error => {
            //    console.error(error);
            //    this.router.navigate(['main']);
            //}
            //);
    }
    CheckBoxClicked(event: any, AttId: string, additionalText: string, armId: number) {
        
        let evdata: CheckBoxClickedEventData = new CheckBoxClickedEventData();
        evdata.event = event;
        evdata.armId = armId;
        evdata.AttId = +AttId.replace('A', '');
        evdata.additionalText = additionalText;
        this.ReviewSetsService.PassItemCodingCeckboxChangedEvent(evdata);
    }
    //'InfoboxTextAdded'
    openInfoBox(AttId: string, additionalText: string, armId: number, isAlreadyCoded: boolean) {
        //const tmp: any = new InfoBoxModalContent();
        let modalComp = this.modalService.open(InfoBoxModalContent);
        console.log('ADDTXT: '+ additionalText);
        modalComp.componentInstance.InfoBoxTextInput = additionalText;
        modalComp.result.then((infoTxt) => {
            //alert('closed: ' + AttId + ' Content: ' + infoTxt);
            if (!isAlreadyCoded) this.CheckBoxClicked('InfoboxTextAdded', AttId, infoTxt, armId);
            else {
                //this.CheckBoxClicked('InfoboxTextAdded', AttId, infoTxt, armId);
                alert('To Do: implement Update command');
            }
        },
            () => { alert('dismissed: ' + AttId) }
        );

    }
}
export class CheckBoxClickedEventData {
    event: any | null = null;
    AttId: number = 0;
    additionalText: string = "";
    armId: number = 0;
}
@Component({
    selector: 'ngbd-InfoBoxModal-content',
    templateUrl: './InfoBoxModal.component.html'
})
export class InfoBoxModalContent {
    @Input() InfoBoxTextInput: string = "";

    constructor(public activeModal: NgbActiveModal) { }
}



