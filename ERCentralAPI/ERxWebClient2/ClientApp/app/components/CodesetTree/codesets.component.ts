import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent } from 'angular-tree-component';
import { NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ArmsService } from '../services/arms.service';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';
import { frequenciesService } from '../services/frequencies.service';
import { Injectable, EventEmitter } from '@angular/core';
import { Subscription } from 'rxjs';
import { EventEmitterService } from '../services/EventEmitter.service';

@Component({
    selector: 'codesets',
    styles: [`.bt-infoBox {    
                    padding: .08rem .12rem .12rem .12rem;
                    margin-bottom: .12rem;
                    font-size: .875rem;
                    line-height: 1.2;
                    border-radius: .2rem;
                }
			.no-select{    
				-webkit-user-select: none;
				cursor:not-allowed; /*makes it even more obvious*/
				}
        `],
    templateUrl: './codesets.component.html'
})
export class CodesetTreeComponent implements OnInit, OnDestroy, AfterViewInit {
   constructor(private router: Router,
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
       private ReviewSetsService: ReviewSetsService,
       private modalService: NgbModal,
	   private armsService: ArmsService,
	   private frequenciesService: frequenciesService,
	   private _eventEmitter: EventEmitterService
    ) { }
    @ViewChild('ManualModal') private ManualModal: any;
	public showManualModal: boolean = false;
	sub: Subscription = new Subscription();

    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
		else {
			
            this.GetReviewSets();
        }
	}
	
	options: ITreeOptions = {
        childrenField: 'attributes', 
        displayField: 'name',
		allowDrag: false,
		
	}
	@ViewChild('tree') treeComponent!: TreeComponent;


	ngAfterViewInit() {

	}

	rootsCollect() {

		const treeModel: TreeModel = this.treeComponent.treeModel;
		const firstNode: any = treeModel.getFirstRoot();

		var rootsArr: Array<ITreeNode> = [];

		for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

			rootsArr[i] = this.treeComponent.treeModel.roots[i];
			console.log(rootsArr[i]);
		}

	}

	nodesNotRootsCollect(node: ITreeNode) {

		const treeModel: TreeModel = this.treeComponent.treeModel;
		const firstNode: any = treeModel.getFirstRoot();

		var childrenArr: Array<any> = [];

		for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

			var test = this.treeComponent.treeModel.roots[i];

			childrenArr[i] = test.getVisibleChildren();
			console.log(childrenArr[i]);
			
		}
	}

	onEvent($event: any) {

		alert($event);

	}
	selectAllRoots() {

		const treeModel: TreeModel = this.treeComponent.treeModel;

		const firstNode: any = treeModel.getFirstRoot();

		for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

			this.treeComponent.treeModel.roots[i].setIsActive(false, true);

				//.setIsActive(true, true);

			//if (i == 0) {
			//	console.log('A root: ' + this.treeComponent.treeModel.roots[i].doForAll(x => x.expand()))
			//}
		}
	}

	getNodeClass(node: ITreeNode): string {

		if (node.data.subTypeName == 'Screening') {
			return 'tree-node-disabled';
		}
		return 'tree-node';
	}

	test(node: singleNode) {

		node.itemSetIsLocked = true;
		alert('hello');
	}

    get nodes(): singleNode[] | null {
        //console.log('Getting codetree nodes');
        if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) 
		{
			
            return this.ReviewSetsService.ReviewSets;
        }
        else {
            return null;
        }
    }
    GetReviewSets() {
        if (this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) return;
        this.ReviewSetsService.GetReviewSets();

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
        //console.log('trying to close...')
        if (this.DeletingData) this.DeletingData.isSelected = true;
        this.DeletingEvent = undefined;
        this.DeletingData = null;
        this.showManualModal = false;
    }
    CheckBoxClickedAfterCheck(event: any, data: singleNode) {
        let evdata: CheckBoxClickedEventData = new CheckBoxClickedEventData();
        evdata.event = event;
        evdata.armId = this.armsService.SelectedArm == null ? 0 : this.armsService.SelectedArm.itemArmId;
        evdata.AttId = +data.id.replace('A', '');
        console.log('AttID: ' + evdata.AttId + ' armid = ' + evdata.armId);
        evdata.additionalText = data.additionalText;
        this.ReviewSetsService.PassItemCodingCeckboxChangedEvent(evdata);
    }
    CompleteUncomplete(event: any, AttId: string, additionalText: string, armId: number) {
        alert('Complete/uncomplete clicked - Sorry, this feature is not implemented yet.')
    }
    
    openInfoBox(data: singleNode) {
        //const tmp: any = new InfoBoxModalContent();
        let modalComp = this.modalService.open(InfoBoxModalContent);
        //console.log('ADDTXT: '+ data.additionalText);
        modalComp.componentInstance.InfoBoxTextInput = data.additionalText;
        modalComp.componentInstance.focus(this.ReviewSetsService.CanWriteCoding(data));
        //let tBox = this.renderer.selectRootElement('#InfoBoxText');
        //tBox.innerText = modalComp.componentInstance.InfoBoxTextInput;
        //console.log(tBox);
        modalComp.result.then((infoTxt) => {
            data.additionalText = infoTxt;
            if (!data.isSelected) {
                
                //console.log('InfoboxTextAdded ' + data.additionalText);
                this.CheckBoxClickedAfterCheck('InfoboxTextAdded', data);//checkbox is not ticked: we are adding this code
            }
            else {
                //console.log('InfoboxTextUpdate ' + data.additionalText);
                this.CheckBoxClickedAfterCheck('InfoboxTextUpdate', data);// checkbox is ticked: we are editing text in infobox
            }
        },
            () => {
                //alert('dismissed: ' + AttId)
            }
        );

    }
    public SelectedNodeData: singleNode | null = null;
	public SelectedCodeDescription: string = "";

	NodeSelected(node: singleNode) {

		console.log(node.name);
		this.SelectedNodeData = node;
		this._eventEmitter.sendMessage(node);
        this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
        
    }
    ngOnDestroy() {
        //this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
		this.sub.unsubscribe();
        //console.log('killing reviewSets comp');
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
    @ViewChild('InfoBoxText')
    InfoBoxText!: ElementRef;
    @Input() InfoBoxTextInput: string = "";
    private canWrite: boolean = true;
    public get IsReadOnly(): boolean {
        //console.log('Is read only???');
        return this.canWrite;
        //return this.ReviewSetsService.CanWriteCoding(new SetAttribute());//.CanWrite;
    }
    constructor(public activeModal: NgbActiveModal, private ReviewSetsService: ReviewSetsService) { }
    public focus(canWrite: boolean) {
        this.canWrite = canWrite;
        this.InfoBoxText.nativeElement.focus();
    }
}



