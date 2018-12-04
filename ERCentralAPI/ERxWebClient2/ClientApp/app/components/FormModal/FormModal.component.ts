//import { Component, Input } from '@angular/core';
//import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';

//@Component({
//	selector: 'ngbdModalContent',
//	template: `
//    <div class="modal-header">
//      <h4 class="modal-title">{{testVarForMemory}}</h4>
//      <button type="button" class="close" aria-label="Close" (click)="activeModal.dismiss('Cross click')">
//        <span aria-hidden="true">&times;</span>
//      </button>
//    </div>
//    <div class="modal-body">
//      <p>Hello, {{name}}!</p>
//    </div>
//    <div class="modal-footer">
//      <button type="button" class="btn btn-outline-dark" (click)="activeModal.close('Close click')">Close</button>
//    </div>
//  `
//})
//export class NgbdModalContent {
//	@Input() name: string | undefined;

//	constructor(public activeModal: NgbActiveModal) { }
//}

//@Component({
//	selector: 'ngbdModalComponent',
//	templateUrl: './NgbdModal.component.html'
//})
//export class NgbdModalComponent {
//	constructor(private modalService: NgbModal) { }

//	open() {
//		const modalRef = this.modalService.open(NgbdModalContent);
//		modalRef.componentInstance.name = 'World';
//	}
//}

//import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms';
//import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
//import { Component, Input } from '@angular/core';
//import { SearchCodeCommand, searchService } from '../services/search.service';
//import { ReviewSetsService, ReviewSet } from '../services/ReviewSets.service';

//@Component({
//	selector: 'ngbdModalContent',
//	template: `
//    <div class="modal-header bg-primary">
//    <h3 id="modal-basic-title"><b>Searches</b></h3>
//    <button type="button" class="close" aria-label="Close" (click)="activeModal.dismiss('Cross click')">
//        <span aria-hidden="true">&times;</span>
//    </button>
//</div>
//<div class="modal-body">
//	<div class="container-fluid">
//		<div class="row">
//			<div class="col-md-4 font-italic"><h5> Find Documents: {{searchInclOrExcl}} </h5></div>
//			<div class="col-md-8">
//				<div ngbDropdown class="d-inline-block col-md-12">
//					<button class="btn btn-secondary col-md-12" id="dropdownBasic1" ngbDropdownToggle>{{selectedSearchDropDown}}</button>
//					<div ngbDropdownMenu aria-labelledby="dropdownBasic1">
//						<button (click)="nextDropDownList(1, 'With this code')" class="dropdown-item">With this code</button>
//						<button (click)="nextDropDownList(2, 'Without this code')" class="dropdown-item">Without this code</button>
//						<button (click)="nextDropDownList(3, 'With these internal IDs (comma separated)')" class="dropdown-item">With these internal IDs (comma separated)</button>
//						<button (click)="nextDropDownList(4, 'With these imported IDs (comma separated)')" class="dropdown-item">With these imported IDs (comma separated)</button>
//						<button (click)="nextDropDownList(5, 'That have at least one code from this set')" class="dropdown-item">That have at least one code from this set</button>
//						<button (click)="nextDropDownList(6, 'That dont have any codes from this set')" class="dropdown-item">That don't have any codes from this set</button>
//						<button (click)="nextDropDownList(7,'Containing this text')" class="dropdown-item">Containing this text</button>
//						<button (click)="nextDropDownList(8,'With at least one document uploaded')" class="dropdown-item">With at least one document uploaded</button>
//						<button (click)="nextDropDownList(9, 'Without any documents uploaded')" class="dropdown-item">Without any documents uploaded</button>
//						<button (click)="nextDropDownList(10,'Without an abstract')" class="dropdown-item">Without an abstract</button>
//					</div>
//				</div>

//				<div *ngIf="(selectedSearchDropDown == 'With this code' )
//						||   (selectedSearchDropDown == 'Without this code' )">
//					<codesets class="col-md-8"></codesets>
//				</div>

//				<div *ngIf="(selectedSearchDropDown == 'That have at least one code from this set'
//						||		selectedSearchDropDown == 'That dont have any codes from this set') "
//						ngbDropdown class="d-inline-block col-md-12">
//					<button class="btn btn-secondary col-md-12" id="dropdownBasic2" ngbDropdownToggle>{{selectedSearchCodeSetDropDown}}</button>
//					<div ngbDropdownMenu aria-labelledby="dropdownBasic2">
//						<button (click)="setSearchCodeSetDropDown(item)" *ngFor="let item of CodeSets;" class="dropdown-item">{{item}}</button>
//					</div>
//				</div>
//			</div>
//			<div *ngIf="showTextBox" class="d-inline-block col-md-12 m-3">
			
//					<div>
//						<b class="col-md-4 font-italic">Comma-Separated list of IDs:</b>
//						<input [(ngModel)]="commaIDs"
//								class="col-md-8 bg-light border-dark form-control pull-right"
//								type="text" name="CommaIDs"
//								placeholder="IDs"><br>
//					</div>
//			</div>
//			<div class="col-md-12">

//					<div class="col-md-4"></div>
//					<div *ngIf="(selectedSearchDropDown == 'Containing this text')"
//							class="col-md-8  pull-right">
//						<div>
//							<div>
//								<input [(ngModel)]="searchText"
//										class="col-md-11 bg-light border-dark form-control m-3"
//										type="text" name="SearchText"
//										placeholder="search text"><br>
//							</div>
//							<h3 class="col-md-12 font-italic text-center">IN</h3>
//							<div ngbDropdown class="d-inline-block col-md-12">
//								<button class="btn btn-secondary col-md-12" id="dropdownBasic2" ngbDropdownToggle>{{selectedSearchTextDropDown}}</button>
//								<div ngbDropdownMenu aria-labelledby="dropdownBasic2">
//									<button (click)="setSearchTextDropDown('TitleAbstract')" class="dropdown-item">Title and abstract</button>
//									<button (click)="setSearchTextDropDown('Title')" class="dropdown-item">Title only</button>
//									<button (click)="setSearchTextDropDown('Abstract')" class="dropdown-item">Abstract only</button>
//									<button (click)="setSearchTextDropDown('AdditionalText')" class="dropdown-item">Additional text</button>
//									<button (click)="setSearchTextDropDown('UploadedDocs')" class="dropdown-item">Uploaded documents</button>
//									<button (click)="setSearchTextDropDown('Authors')" class="dropdown-item">Authors</button>
//									<button (click)="setSearchTextDropDown('PubYear')" class="dropdown-item">Publication year</button>
//								</div>
//							</div>
//						</div>
//					</div>
//				</div>
//		</div>
//	</div>
//<div class="row">
//	<div class="col-md-4"></div>
//	<div class="col-md-12">
//		<table class="table table-responsive table-striped table-light card-body">
//			<tr>
//				<td>
//					<input  name="searchInclOrExclRB" type="radio" value="true" [(ngModel)]="searchInclOrExcl"><br />

//					<b>&nbsp; Included </b>
//				</td>
//				<td>
//					<input name="searchInclOrExclRB" type="radio" value="false" [(ngModel)]="searchInclOrExcl"><br />

//					<b>&nbsp; Excluded</b>
//				</td>
//			</tr>
//		</table>
//	</div>
//</div>
//</div>
//<div class="modal-footer bg-primary">
//	<button type="button" class="btn btn-dark" (click)="_activeModal.dismiss('Cancel click')">&nbsp;&nbsp;Cancel&nbsp;&nbsp;</button>
//	<button type="button" class="btn btn-secondary"
//			(click)="callSearches(selectedSearchDropDown, selectedSearchTextDropDown, searchInclOrExcl)"> Search
//	</button>
//</div>


//  `
//})
//export class NgbdModalContent {

//	@Input() name: string | undefined;
//	public searchInclOrExcl: boolean | undefined;


//	constructor(private  _activeModal: NgbActiveModal,
//		 private _reviewSetsService: ReviewSetsService,
//		private _searchService: searchService) {

//	}

//	private canWrite: boolean = true;
//	public dropDownList: any = null;
//	public showTextBox: boolean = false;
//	public selectedSearchDropDown: string = '';
//	public selectedSearchTextDropDown: string = '';
//	public selectedSearchCodeSetDropDown: string = '';
//	public CodeSets: any[] = [];


//	_setID: number = 0;
//	_searchText: string = '';
//	_title: string = '';
//	_answers: string = '';
//	_included: boolean = true;
//	_withCodes: boolean = false;;
//	_searchId: number = 0;
//	_IDs: string = '';

//	public get IsReadOnly(): boolean {
//		return this.canWrite;
//	}


//	ngOnInit() {
//		this.searchInclOrExcl = true;
//	}

//	public cmdSearches: SearchCodeCommand = {
//		_setID: 0,
//		_searchText: '',
//		_IDs: '',
//		_title: '',
//		_answers: '',
//		_included: false,
//		_withCodes: false,
//		_searchId: 0
//	};

//	public withCode: boolean = false;
//	public attributeNames: string = '';
//	public commaIDs: string = '';
//	public searchText: string = '';

//	callSearches(selectedSearchDropDown: string, selectedSearchTextDropDown: string, searchBool: boolean) {

//		this.selectedSearchTextDropDown = selectedSearchTextDropDown;
//		let searchTitle: string = '';
//		let firstNum: boolean = selectedSearchDropDown.search('With this code') != -1;
//		let secNum: boolean = selectedSearchDropDown.search('Without this code') != -1
//		this.cmdSearches._included = Boolean(searchBool);
//		this.cmdSearches._withCodes = this.withCode;
//		this.cmdSearches._searchId = 0;

//		if (firstNum == true || secNum == true) {

//			if (firstNum) {

//				this.withCode = true;
//			} else {

//				this.withCode = false;
//			}

//			if (this._reviewSetsService.selectedNode != undefined) {

//				let tmpID: number = this._reviewSetsService.selectedNode.attributeSetId;
//				this.attributeNames = this._reviewSetsService.selectedNode.name;
//				this.cmdSearches._answers = String(tmpID);
//				alert(this._reviewSetsService.selectedNode);

//				searchTitle = this.withCode == true ?
//					"Coded with: " + this.attributeNames : "Not coded with: " + this.attributeNames;


//				this.cmdSearches._title = searchTitle;
//				this.cmdSearches._withCodes = this.withCode;

//				this._searchService.CreateSearch(this.cmdSearches, 'SearchCodes');

//			}
//		}

//		if (selectedSearchDropDown == 'With these internal IDs (comma separated)') {

//			this.cmdSearches._IDs = this.commaIDs;
//			this.cmdSearches._title = this.commaIDs;
//			this._searchService.CreateSearch(this.cmdSearches, 'SearchIDs');


//		}
//		if (selectedSearchDropDown == 'With these imported IDs (comma separated)') {

//			this.cmdSearches._IDs = this.commaIDs;
//			this.cmdSearches._title = this.commaIDs;

//			this._searchService.CreateSearch(this.cmdSearches, 'SearchImportedIDs');


//		}
//		if (selectedSearchDropDown == 'Containing this text') {

//			this.cmdSearches._title = this.searchText;
//			this.cmdSearches._included = Boolean(searchBool);
//			this._searchService.CreateSearch(this.cmdSearches, 'SearchText');
//		}
//		if (selectedSearchDropDown == 'That have at least one code from this set') {

//			this.cmdSearches._withCodes = true;
//			this.cmdSearches._title = this.selectedSearchCodeSetDropDown;

//			this._searchService.CreateSearch(this.cmdSearches, 'SearchCodeSetCheck');

//		}
//		if (selectedSearchDropDown == 'That dont have any codes from this set') {

//			this.cmdSearches._withCodes = false;
//			this.cmdSearches._title = this.selectedSearchCodeSetDropDown;
//			this._searchService.CreateSearch(this.cmdSearches, 'SearchCodeSetCheck');

//		}
//		if (selectedSearchDropDown == 'Without an abstract') {

//			alert(selectedSearchDropDown);
//			this.cmdSearches._title = searchTitle;

//			this._searchService.CreateSearch(this.cmdSearches, 'SearchNoAbstract');

//		}

//		if (selectedSearchDropDown == 'Without any documents uploaded') {

//			alert(selectedSearchDropDown);
//			this.cmdSearches._title = 'Without any documents uploaded';

//			this._searchService.CreateSearch(this.cmdSearches, 'SearchNoFiles');

//		}
//		if (selectedSearchDropDown == 'With at least one document uploaded') {

//			this.cmdSearches._title = 'With at least one document uploaded.';
//			this._searchService.CreateSearch(this.cmdSearches, 'SearchOneFile');

//		}
//		alert('got to end of return of BO from CSLA');
//		this._activeModal.close(this.cmdSearches);

//	}

//	public nextDropDownList(num: number, val: string) {

//		this.showTextBox = false;
//		this.selectedSearchDropDown = val;

//		switch (num) {

//			case 1: {
//				this.dropDownList = this._reviewSetsService.ReviewSets;
//				break;
//			}
//			case 2: {
//				this.dropDownList = this._reviewSetsService.ReviewSets;
//				break;
//			}
//			case 3: {
//				this.showTextBox = true;
//				break;
//			}
//			case 4: {
//				this.showTextBox = true;
//				break;
//			}
//			case 5: {

//				this.CodeSets = this._reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
//					.map(
//						(y: ReviewSet) => {
//							return y.name;
//						}
//					);
//				this.dropDownList = this._reviewSetsService.ReviewSets;
//				break;
//			}
//			case 6: {
//				this.CodeSets = this._reviewSetsService.ReviewSets.filter(x => x.nodeType == 'ReviewSet')
//					.map(
//						(y: ReviewSet) => {
//							return y.name;
//						}
//					);
//				this.dropDownList = this._reviewSetsService.ReviewSets;
//				break;
//			}
//			case 7: {
//				break;
//			}
//			case 8: {
//				break;
//			}
//			case 9: {
//				break;
//			}
//			case 10: {
//				break;
//			}
//			default: {
//				break;
//			}
//		}
//	}

//	public setSearchCodeSetDropDown(codeSetName: string) {

//		this.selectedSearchCodeSetDropDown = this._reviewSetsService.ReviewSets.filter(x => x.name == codeSetName)
//			.map(
//				(y: ReviewSet) => {

//					this.cmdSearches._setID = y.set_id;
//					return y.name;
//				}
//			)[0];
//	}

//	public setSearchTextDropDown(heading: string) {

//		this.selectedSearchTextDropDown = heading;
//		this.cmdSearches._searchText = heading;
//	}

//	public focus(canWrite: boolean) {
//		this.canWrite = canWrite;
//	}

//}



//import { Component, Output, EventEmitter, Input } from '@angular/core';
//import { FormGroup, FormBuilder, FormControl, Validators } from '@angular/forms';
//import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

//@Component({
//	selector: 'FormModal',
//	templateUrl: './FormModal.component.html'
//})

//export class FormModalComponent {

//	@Input() id!: number;
//	myForm!: FormGroup;

//	constructor(
//		public activeModal: NgbActiveModal,
//		private formBuilder: FormBuilder
//	) {
//		this.createForm();
//	}
//	private createForm() {
//		this.myForm = this.formBuilder.group({

//			id : this.id ,
//			username: '',
//			password: ''
//		});
//	}

//	private submitForm() {
//		this.activeModal.close(this.myForm.value);
//	}
//}



	//openFormModal() {
	//	const modalRef = this.modalService.open(NgbdModalContent);
	//	modalRef.componentInstance.id = 10; // should be the id

	//	modalRef.result.then((result) => {
	//		console.log(result);
	//	}).catch((error) => {
	//		console.log(error);
	//	});
	//}

	
	//open() {

	//	const modalRef = this.modalService.open(NgbdModalContent);
	//	modalRef.componentInstance.id = 10; // should be the id

	//	modalRef.result.then(

	//		//called if search button is pressed
	//		(result) => {

	//			//console.log('tester123: ' + modalRef.componentInstance._searchService.SearchList());
	//			console.log('pressed search okay: ' + JSON.stringify(result));

	//		},
	//		//called if modal is cancelled
	//		(result) => {

	//			console.log('cancelled: ' + JSON.stringify(result));
	//		}
	//	);
	//}
//}

//import { Component, ElementRef, Input, OnInit, OnDestroy } from '@angular/core';
//import * as $ from 'jquery';

//import { ModalService } from '../services/modal.service';

//@Component({
//	moduleId: module.id.toString(),
//	selector: 'modal',
//	template: '<ng-content></ng-content>'
//})

//export class ModalComponent implements OnInit, OnDestroy {
//	@Input() id!: string;
//	private element: JQuery;

//	constructor(private modalService: ModalService, private el: ElementRef) {
//		this.element = $(el.nativeElement);
//	}

//	ngOnInit(): void {
//		let modal = this;

//		// ensure id attribute exists
//		if (!this.id) {
//			console.error('modal must have an id');
//			return;
//		}

//		// move element to bottom of page (just before </body>) so it can be displayed above everything else
//		this.element.appendTo('body');

//		// close modal on background click
//		this.element.on('click', function (e: any) {
//			var target = $(e.target);
//			if (!target.closest('.modal-body').length) {
//				modal.close();
//			}
//		});

//		// add self (this modal instance) to the modal service so it's accessible from controllers
//		this.modalService.add(this);
//	}

//	// remove self from modal service when directive is destroyed
//	ngOnDestroy(): void {
//		this.modalService.remove(this.id);
//		this.element.remove();
//	}

//	// open modal
//	open(): void {
//		this.element.show();
//		$('body').addClass('modal-open');
//	}

//	// close modal
//	close(): void {
//		this.element.hide();
//		$('body').removeClass('modal-open');
//	}
//}