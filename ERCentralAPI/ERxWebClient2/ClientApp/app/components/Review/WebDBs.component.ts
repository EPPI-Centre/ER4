import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { WebDBService, iWebDB } from '../services/WebDB.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { ReviewSetsService } from '../services/ReviewSets.service';

@Component({
	selector: 'WebDBsComp',
	templateUrl: './WebDBs.component.html',
	providers: []
})

export class WebDBsComponent implements OnInit, OnDestroy {
	constructor(private router: Router,
		@Inject('BASE_URL') private _baseUrl: string,
		private WebDBService: WebDBService,
		private ReviewerIdentityService: ReviewerIdentityService,
		private ReviewSetsService: ReviewSetsService,
		private ModalService: ModalService
	)
	{ }

	//@Output() onCloseClick = new EventEmitter();
	//public isExpanded: boolean = false;
	public EditingDB: iWebDB | null = null;
	public get WebDbs(): iWebDB[] {
		return this.WebDBService.WebDBs;
	}
	public get CurrentDB(): iWebDB | null {
		return this.WebDBService.CurrentDB;
	}
	public get IsBusy(): boolean {
		return (this.WebDBService.IsBusy
			|| this.ReviewSetsService.IsBusy);
	}
	public FilterName(AttId: number): string {
		if (AttId == 0) return "No filter";
		else {
			let att = this.ReviewSetsService.FindAttributeById(AttId);
			if (att) return att.attribute_name;
			else return "Unknown Code (ID: " + AttId.toString() + ")";
        }
    }
	ngOnInit() {
		if (this.WebDBService.WebDBs.length == 0) this.WebDBService.Fetch();
	}
    CanWrite(): boolean {
        //console.log("create rev check:", this._reviewerIdentityServ.reviewerIdentity);
		return this.ReviewerIdentityService.HasWriteRights;
    }
	Edit(item: iWebDB | null) {
		if (!item) {
			item = {
				webDBId: 0,
				webDBName: '',
				webDBDescription: '',
				attributeIdFilter: 0,
				isOpen: true,
				userName: '',
				password: '',
				createdBy: '',
				editedBy: ''
			};
        }
		this.EditingDB = this.WebDBService.CloneWebDBforEdit(item);
        //this.isExpanded = true;
    }
	CancelEdit() {
		this.EditingDB = null;
        //this.isExpanded = false;
	}
	
	BackToMain() {
		this.router.navigate(['Main']);
	}
	ngOnDestroy() {
		this.WebDBService.Clear();
	}
	ngAfterViewInit() {

	}
}
