import { Component, Inject, OnInit, Output, EventEmitter, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, Attribute } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent } from 'angular-tree-component';
import { NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Node } from '@angular/compiler/src/render3/r3_ast';
import { ArmsService } from '../services/arms.service';
import { TREE_ACTIONS, KEYS, IActionMapping } from 'angular-tree-component';
import { TreeNode } from '@angular/router/src/utils/tree';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';
import { ItemCodingService, ItemAttPDFCodingCrit } from '../services/ItemCoding.service';
import { ItemDocsService } from '../services/itemdocs.service';

@Component({
    selector: 'codesetTreeCoding',
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
    templateUrl: './codesetTreeCoding.component.html'
})
export class CodesetTreeCodingComponent implements OnInit, OnDestroy, AfterViewInit {
   constructor(private router: Router,
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
       private ReviewSetsService: ReviewSetsService,
       private modalService: NgbModal,
       private ItemCodingService: ItemCodingService,
       private ItemDocsService: ItemDocsService,
       private armsService: ArmsService
    ) { }
    //@ViewChild('ConfirmDeleteCoding') private ConfirmDeleteCoding: any;
    @ViewChild('ManualModal') private ManualModal: any;
    public showManualModal: boolean = false;
    @Input() InitiateFetchPDFCoding = false;
    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else {
            //console.log("Review Ticket: " + this.ReviewerIdentityServ.reviewerIdentity.ticket);
            //let modalComp = this.modalService.open(InfoBoxModalContent);
            //modalComp.close();
            //this.GetReviewSets();
        }
	}
	
	options: ITreeOptions = {
        childrenField: 'attributes', 
        displayField: 'name',
		allowDrag: false,


		
	}
	@ViewChild('tree') treeComponent!: TreeComponent;


	ngAfterViewInit() {

		//if () {
				
		//const firstNode: TreeNode<singleNode> = treeModel.getFirstRoot();
			
		//}
		//const firstNode: singleNode = treeModel.getFirstRoot();

		//firstNode.setActiveAndVisible();
	}

	rootsCollect() {

		// for now until clarification put in an array of nodes
		

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
		//this.treeComponent.treeModel.roots[0].setIsActive(true, true);
		//this.treeComponent.treeModel.roots[1].setIsActive(true, true);
		//this.treeComponent.treeModel.roots[2].setIsActive(true, true);
		//this.treeComponent.treeModel.roots[3].setIsActive(true, true);
	}

	getNodeClass(node: ITreeNode): string {

		if (node.data.subTypeName == 'Screening') {
			//console.log('node disabled: ' + node.displayField);
			return 'tree-node-disabled';
		}
		//console.log('node not disabled: ' + node.displayField);
		return 'tree-node';
	}

	test(node: singleNode) {

		node.itemSetIsLocked = true;
		alert('hello');
	}
    //nodes: singleNode[] = [];
    get nodes(): singleNode[] | null {
        //console.log('Getting codetree nodes');
        if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) 
		{
			//for (var i = 0; i < this.ReviewSetsService.ReviewSets.length; i++) {

			//	console.log('found my nodes: ' + this.ReviewSetsService.ReviewSets[i] + '\n');
			//}
			
            return this.ReviewSetsService.ReviewSets;
        }
        else {
            //console.log('NO nodes');
            return null;
        }
    }
    GetReviewSets() {
        if (this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) return;
        console.log('Get reviesets in revsets comp');
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

		//alert('in node: ' + node.name)
        this.SelectedNodeData = node;
        this.SelectedCodeDescription = node.description.replace(/\r\n/g, '<br />').replace(/\r/g, '<br />').replace(/\n/g, '<br />');
        const att = node as SetAttribute;
        if (att && node.nodeType == "SetAttribute" ) {
            this.ItemCodingService.SelectedSetAttribute = att;
            if (this.InitiateFetchPDFCoding && node.isSelected && this.ItemDocsService.CurrentDocId !== 0) {
                const ROatt = this.ItemCodingService.FindROItemAttributeByAttribute(att);
                console.log("we might need to fetch PDF coding", ROatt);
                if (ROatt) this.ItemCodingService.FetchItemAttPDFCoding(new ItemAttPDFCodingCrit(this.ItemDocsService.CurrentDocId, ROatt.itemAttributeId));
                else {
                    this.ItemCodingService.ClearItemAttPDFCoding();
                }
            }
            else {
                this.ItemCodingService.ClearItemAttPDFCoding();
            }
        } else {
            this.ItemCodingService.ClearItemAttPDFCoding();
            this.ItemCodingService.SelectedSetAttribute = null;//remove selection, PDF should not load highlights.
        }
    }
    ngOnDestroy() {
        //this.ReviewerIdentityServ.reviewerIdentity = new ReviewerIdentity();
        
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



