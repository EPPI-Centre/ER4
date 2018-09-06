import { Component, Inject, OnInit, Output, EventEmitter, Input, ViewChild, ChangeDetectorRef } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet } from '../services/ReviewSets.service';
import { ITreeOptions } from 'angular-tree-component';
import { NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Node } from '@angular/compiler/src/render3/r3_ast';


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
       private modalService: NgbModal,
       private cd: ChangeDetectorRef
    ) { }
    @ViewChild('ConfirmDeleteCoding') private ConfirmDeleteCoding: any;
    @ViewChild('ManualModal') private ManualModal: any;
    public showManualModal: boolean = false;
    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else {
            //console.log("Review Ticket: " + this.ReviewerIdentityServ.reviewerIdentity.ticket);
            //let modalComp = this.modalService.open(InfoBoxModalContent);
            //modalComp.close();
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
    CheckBoxClicked(event: any, data: singleNode, ) {
        let checkPassed: boolean = true;
        if (event.target) checkPassed = event.target.checked;//if we ticked the checkbox, it's OK to carry on, otherwise we need to check
        if (!checkPassed) {
            //event.srcElement.blur();
            console.log('checking...');
            //deleting the codeset: need to confirm
            this.DeletingData = data;
            this.DeletingEvent = event;
            //all this seems necessary because I could not suppress the error discussed here:
            //https://blog.angularindepth.com/everything-you-need-to-know-about-the-expressionchangedafterithasbeencheckederror-error-e3fd9ce7dbb4
            this.showManualModal = true;
        }
        else this.CheckBoxClickedAfterCheck(event, data);
    }
    private DeletingEvent: any;
    private DeletingData: singleNode | null = null;
    DeleteCodingConfirmed() {
        if (this.DeletingData) {
            this.DeletingData.isSelected = false;
            this.CheckBoxClickedAfterCheck(this.DeletingEvent, this.DeletingData);
        }
        this.DeletingEvent = undefined;
        this.DeletingData = null;
        this.showManualModal = false;
    }
    DeleteCodingCancelled() {
        console.log('trying to close...')
        if (this.DeletingData) this.DeletingData.isSelected = true;
        this.DeletingEvent = undefined;
        this.DeletingData = null;
        this.showManualModal = false;
    }
    CheckBoxClickedAfterCheck(event: any, data: singleNode) {
        let evdata: CheckBoxClickedEventData = new CheckBoxClickedEventData();
        evdata.event = event;
        evdata.armId = data.armId;
        evdata.AttId = +data.id.replace('A', '');
        console.log('AttID: ' + evdata.AttId);
        evdata.additionalText = data.additionalText;
        this.ReviewSetsService.PassItemCodingCeckboxChangedEvent(evdata);
    }
    CompleteUncomplete(event: any, AttId: string, additionalText: string, armId: number) {
        alert('Complete/uncomplete clicked - Sorry, this feature is not implemented yet.')
    }
    
    openInfoBox(data: singleNode) {
        //const tmp: any = new InfoBoxModalContent();
        let modalComp = this.modalService.open(InfoBoxModalContent);

        console.log('ADDTXT: '+ data.additionalText);
        modalComp.componentInstance.InfoBoxTextInput = data.additionalText;
        modalComp.result.then((infoTxt) => {
            data.additionalText = infoTxt;
            if (!data.isSelected) {
                
                console.log('InfoboxTextAdded ' + data.additionalText);
                this.CheckBoxClickedAfterCheck('InfoboxTextAdded', data);//checkbox is not ticked: we are adding this code
            }
            else {
                console.log('InfoboxTextUpdate ' + data.additionalText);
                this.CheckBoxClickedAfterCheck('InfoboxTextUpdate', data);// checkbox is ticked: we are editing text in infobox
            }
        },
            () => {
                //alert('dismissed: ' + AttId)
            }
        );

    }
}


//another class!!!
export class CheckBoxClickedEventData {
    event: any | null = null;
    AttId: number = 0;
    additionalText: string = "";
    armId: number = 0;
}


//EVEN more: small separate component for the infobox modal.
@Component({
    selector: 'ngbd-InfoBoxModal-content',
    templateUrl: './InfoBoxModal.component.html'
})
export class InfoBoxModalContent {
    @Input() InfoBoxTextInput: string = "";

    constructor(public activeModal: NgbActiveModal) { }
}



