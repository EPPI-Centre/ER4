import { Component, OnInit, Input, ViewChild, Inject } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import {  Item, ItemListService } from '../services/ItemList.service';
import { _localeFactory } from '@angular/core/src/application_module';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ArmTimepointLinkListService, ItemTimepointDeleteWarningCommandJSON, iTimePoint, TimePoint, iItemLink } from '../services/ArmTimepointLinkList.service';
import { NgModel, NgForm } from '@angular/forms';

import { Helpers } from '../helpers/HelperMethods';

@Component({
	selector: 'ItemLinksComp',
	templateUrl: './ItemLinks.component.html',
    providers: []
})

export class ItemLinksComp  implements OnInit {

	constructor(
		private ArmTimepointLinkListService: ArmTimepointLinkListService,
		private confirmationDialogService: ConfirmationDialogService,
		private itemListService: ItemListService,
		private ReviewerIdentityServ: ReviewerIdentityService,
		@Inject('BASE_URL') private _baseUrl: string,
	) {
		
	}
	private _item: Item | undefined = undefined;
	@Input() public set item(it: Item | undefined) {
		this.Clear();
		this._item = it;
	}
	public get item(): Item | undefined {
		return this._item;
    }
	public EditingLink: iItemLink | null = null;
	public ItemIdTofind: number = 0;
	public CantSaveLinkMessage: string = "";
	public ShowItemLinks: boolean = true;
	private _LinkedItemDetails: Item | null = null;
	ngOnInit() {

		
	}

	public get HasWriteRights(): boolean {
		return this.ReviewerIdentityServ.HasWriteRights;
	}
	public get ItemLinks(): iItemLink[] {
		return this.ArmTimepointLinkListService.links;
	}
	public get ShowHideBtnText(): string {
		if (this.ShowItemLinks) return "Collapse";
		else return "Expand";
	}
	public get LinkedItemDetails(): Item | null {
		if (this._LinkedItemDetails != null) {
			const idetails = this._LinkedItemDetails;
			const i = this.ItemLinks.findIndex(f => f.itemIdSecondary == idetails.itemId);
			if (i == -1) {
				this._LinkedItemDetails = null;
            }
		}
		return this._LinkedItemDetails;
	}
	public get DescriptionForItemDetails(): string {
		if (this.LinkedItemDetails == null) return "";
		else {
			const itm: Item = this.LinkedItemDetails;
			const i = this.ItemLinks.findIndex(f => f.itemIdSecondary == itm.itemId);
			if (i != -1) return this.ItemLinks[i].description;
			else return "";
		}
    }
	public CreateLink() {
		this.ShowItemLinks = true;
		let newLink: iItemLink = {
			itemIdPrimary: 0
			, itemIdSecondary: 0
			, itemLinkId: 0
			, description: ''
			, shortTitle: ''
			, title: ''
		}
		this.EditingLink = newLink;
	}
	public CreateLinkReport() {

		if (this._item != null) {

			let report: string = "<h3>Linked reference report</h3>";
			report += "<table border='1' cellspacing='0' cellpadding='2'>";
			report += "<tr>"
			report += "<td><b>Master<br>EPPI ID</b></td>";
			report += "<td><b>Master<br>Short title</b></td>";
			report += "<td><b>Master title</b></td>";
			report += "<td><b>Linked EPPI ID <br>& Short title</b></td>";
			report += "<td><b>Linked Item title</b></td>";
			report += "<td><b>Link description</b></td>";
			report += "</tr>"
			report += "<tr>"

			report += "<td>" + this._item.itemId + "</td>"
			report += "<td>" + this._item.shortTitle + "</td>"
			report += "<td>" + this._item.title + "</td>"

			for (var i = 0; i < this.ItemLinks.length; i++) {
				let currentItem: iItemLink = this.ItemLinks[i];
				if (i == 0) {
					if (currentItem.itemIdPrimary == currentItem.itemIdSecondary) {
						// the first row is the master item so get the rest from next row
						i += 1;
						let currentItem: iItemLink = this.ItemLinks[1];
						report += "<td>" + currentItem.itemIdSecondary + ": " + currentItem.shortTitle + "</td>"
						report += "<td>" + currentItem.title + "</td>"
						report += "<td>" + currentItem.description + "</td>"
						report += "</tr>"
					}
					else {
						report += "<td>" + currentItem.itemIdSecondary + ": " + currentItem.shortTitle + "</td>"
						report += "<td>" + currentItem.title + "</td>"
						report += "<td>" + currentItem.description + "</td>"
						report += "</tr>"
					}
				}				
				else {
					// subsequent links
					report += "<tr>"
					report += "<td></td>"
					report += "<td></td>"
					report += "<td></td>"
					report += "<td>" + currentItem.itemIdSecondary + ": " + currentItem.shortTitle + "</td>"
					report += "<td>" + currentItem.title + "</td>"
					report += "<td>" + currentItem.description + "</td>"
					report += "</tr>"
				}

			}
			report += "</table>"
			Helpers.OpenInNewWindow(report, this._baseUrl);
		}
	}

	CancelEditing() {
		this.CantSaveLinkMessage = "";
		this.EditingLink = null;
    }
	public EditLink(link: iItemLink) {
		this.ShowItemLinks = true;
		let copied: iItemLink = {
			itemIdPrimary: link.itemIdPrimary
			, itemIdSecondary: link.itemIdSecondary
			, itemLinkId: link.itemLinkId
			, description: link.description
			, shortTitle: link.shortTitle
			, title: link.title
		}
		this.EditingLink = copied;
	}

	public async FindItemById() {
		if (!this.ItemIdTofind || this.ItemIdTofind < 1 ) return;
		else {
			this.CantSaveLinkMessage = "";
			let itm: Item | null = await this.itemListService.FetchSingleItem(this.ItemIdTofind);
			if (itm && this.item) {
				let newL: iItemLink = {
					itemIdPrimary: this.item.itemId
					, itemIdSecondary: itm.itemId
					, itemLinkId: 0
					, description: (this.EditingLink != null && this.EditingLink.description != "") ? this.EditingLink.description : ""
					, shortTitle: itm.shortTitle
					, title: itm.title
				}
				this.EditingLink = newL;
			}
			if (this.EditingLink == null || this.EditingLink.itemIdSecondary == 0) {
				this.CantSaveLinkMessage = "Item with ID " + this.ItemIdTofind.toString() + " was not found in this review";
			}
			else if (this.ItemLinks.findIndex(f => this.EditingLink != null && f.itemIdSecondary == this.EditingLink.itemIdSecondary) != -1) {
				this.CantSaveLinkMessage = "Item " + this.EditingLink.itemIdSecondary.toString() + " is already linked.";
            }
        }
    }
	public get CanSaveLink(): boolean {
		if (!this.HasWriteRights || this.EditingLink == null
			|| this.EditingLink.itemIdPrimary < 1 || this.EditingLink.itemIdSecondary < 1
			|| this.EditingLink.description.trim().length < 3) {
			//console.log("R1");
			return false;
		}
		else if (this.EditingLink.itemLinkId == 0
			&& this.ItemLinks.findIndex(f => this.EditingLink != null && f.itemIdSecondary == this.EditingLink.itemIdSecondary) != -1 //this is a new link, but we're trying to link to an already-linked item
		) {
			//console.log("R2");
			return false;
		}
		else return true;
    }
	public async  SaveLink(link: iItemLink) {
		if (!this.CanSaveLink || this.EditingLink == null) return;
		else {
			if (this.EditingLink.itemLinkId == 0) {
				if (await this.ArmTimepointLinkListService.CreateItemLink(this.EditingLink)) {
					this.EditingLink = null;
				}
			} else {
				//must be that we're editing an existing link...
				if (await this.ArmTimepointLinkListService.UpdateItemLink(this.EditingLink)) {
					this.EditingLink = null;
				}
				
            }
        } 
	}

	public DeleteLink(link: iItemLink) {
		if (!this.HasWriteRights || link.itemLinkId < 1) return;
		else {
			if (this.EditingLink != null && this.EditingLink.itemLinkId == link.itemLinkId) {
				//we're thinking also of editing this same link, so we're going to stop editing it, just in case...
				this.EditingLink = null;
            }
			const msg = "Are you sure you want to delete this link? <br />"
				+ "<div class='m-1 rounded border-info border p-1'> ID: <strong>" + link.itemIdSecondary + "</strong><br />"
				+ "Short Title: <strong>" + link.shortTitle + "</strong><br /></div>";
			this.confirmationDialogService.confirm("Delete Item Link?", msg, false, "", "Yes, delete", "Cancel").then(
				(confirmed: any) => {
					console.log('User confirmed:');
					if (confirmed) {
						this.ArmTimepointLinkListService.DeleteItemLink(link);
					} 
				}
			).catch(() => { });
			
        }
    }

	public async ViewLinkedItemDetails(link: iItemLink) {
		if (link.itemIdSecondary > 0) {
			this._LinkedItemDetails = await this.itemListService.FetchSingleItem(link.itemIdSecondary);
		}
    }
	public CloseLinkedItemDetails() {
		this._LinkedItemDetails = null;
    }
	Clear() {
		this.CancelEditing();
		this.ItemIdTofind = 0;
	}

	
}

