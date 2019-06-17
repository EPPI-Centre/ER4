import { Component, OnInit, OnDestroy, Input} from '@angular/core';
import { Router } from '@angular/router';
import { ItemListService,  Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute, ItemAttributeFullTextDetails } from '../services/ItemCoding.service';
import { ReviewSetsService, ReviewSet, SetAttribute} from '../services/ReviewSets.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { Subscription } from 'rxjs';
import { ComparisonsService } from '../services/comparisons.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { error } from '@angular/compiler/src/util';

@Component({
   
	selector: 'codingRecordComp',
	templateUrl: './codingRecord.component.html'  

})

export class codingRecordComp implements OnInit, OnDestroy {

    constructor(private router: Router, 
        public ItemListService: ItemListService,
		private _comparisonService: ComparisonsService,
		private _ItemCodingService: ItemCodingService,
        private _ReviewSetsService: ReviewSetsService,
        private notificationService: NotificationService
    ) { }
  
	@Input() item: Item = new Item();
	public CodingRecords: ItemSet[] = [];
	public checkboxValue: boolean = false;
	public itemsSelected: ItemSet[] = [];
	private comparison1: ItemSet | null = null;
	private comparison2: ItemSet | null = null;
	private comparison3: ItemSet | null = null;
	private ItemCodingServiceDataChanged: Subscription | null = null;

    ngOnInit() {

		this.ItemCodingServiceDataChanged = this._ItemCodingService.DataChanged.subscribe(

			() => {
				this.CodingRecords = [];
				this._ItemCodingService.ItemCodingList.forEach(

					(x) => {

						let tmp = new ItemSet();
						tmp = x;
						this.CodingRecords.push(tmp);

					});
			}
		);
		

	}
	checkboxChange(index: number, item: CodingRecord) {

		this.itemsSelected = this.CodingRecords.filter((x) => x.isSelected == true);
		let count: number = this.itemsSelected.length;

		if (count < 2 || count > 3) {
			return;
		}

		this.checkboxValue = item.isSelected;
	}
	SetComparisons(): boolean {

		this.comparison1 = null;
		this.comparison2 = null;
		this.comparison3 = null;

		let isla: ItemSet[] = this.itemsSelected;
		let itemSet: ItemSet = new ItemSet();

		if (isla != null) {

			for (var i = 0; i < isla.length; i++) {

				itemSet = isla[i];	
                if (itemSet.isSelected == true) {
                    if (this.comparison1 == null) {
                        this.comparison1 = itemSet;
                    }
                    else {
                        if (this.comparison2 == null)
                            this.comparison2 = itemSet;
                        else {
                            if (this.comparison3 == null)
                                this.comparison3 = itemSet;
                            else {
                                this.ShowErrorMsg("Comparisons can handle no more than three codings.");
                                return false;
                            }
                        }
                    }
                }
			}
			if (this.comparison1 == null) {
                this.ShowErrorMsg("Nothing selected to compare");
				return false;
			}
			if (this.comparison2 == null) {
                this.ShowErrorMsg("You need to select at least two elements to compare");
				return false;
			}
			if (this.comparison2.setId != this.comparison1.setId) {
                this.ShowErrorMsg("Selected items must refer to the same Coding Tool");
				return false;
			}
			if ((this.comparison3 != null) && (this.comparison3.setId != this.comparison2.setId)) {
                this.ShowErrorMsg("Selected items must be the same Coding Tool");
				return false;
			}
		}
		return true;
    }
    private ShowErrorMsg(message: string) {
        this.notificationService.show({
            content: message,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: 'error', icon: true },
            closable: true
        });
    }
	
	private CodingReportCheckChildSelected(itemSet: ItemSet,  attributeSet: SetAttribute): boolean {

		if (itemSet != null) {
			var listAttributes = itemSet.itemAttributesList.filter((x) => x.attributeId == attributeSet.attribute_id);
			
			if (listAttributes && listAttributes.length > 0) {
				return true;
			}

			for (var i = 0; i < attributeSet.attributes.length; i++) {

				let child: SetAttribute = attributeSet.attributes[i];
				if (this.CodingReportCheckChildSelected(itemSet, child) == true) {
					return true;
				}
			}
		}
		return false;
	}
	private writeComparisonReportAttributes(comparison1: ItemSet, comparison2: ItemSet, comparison3: ItemSet | null, attributeSet: SetAttribute): string {

		let report: string = "";

		if (attributeSet.attribute_type_id > 1) {

			let oneReviewerHasSelected: boolean = false;
			let roias: ReadOnlyItemAttribute[];
			var listAttributes = comparison1.itemAttributesList.filter((x) => x.attributeId == attributeSet.attribute_id).sort(o => o.armId);
			if (listAttributes && listAttributes.length > 0) {
				roias = listAttributes;
				for (var i = 0; i < roias.length; i++) {

                    let roia: ReadOnlyItemAttribute = roias[i];
                    
				report += "<li><FONT COLOR='BLUE'>[" + comparison1.contactName + "] " +
					attributeSet.attribute_name +

					(roia.armTitle == "" ? "" : " (" + roia.armTitle + ")") +

					"<br /><i>" + roia.additionalText + "</i></font></li>";

                    oneReviewerHasSelected = true;
                    if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {
                        report += "<FONT COLOR='BLUE'>" + this._ItemCodingService.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails) + "</FONT>";
                    }
                }

			}
			roias = comparison2.itemAttributesList.filter((x) => x.attributeId == attributeSet.attribute_id).sort(o => o.armId);
			
			for (var i = 0; i < roias.length; i++) {

				let roia: ReadOnlyItemAttribute = roias[i];
			
				report += "<li><FONT COLOR='RED'>[" + comparison2.contactName + "] " +
					attributeSet.attribute_name +
					(roia.armTitle == "" ? "" : " (" + roia.armTitle + ")") +
					"<br /><i>" + roia.additionalText + "</i></font></li>";
				oneReviewerHasSelected = true;
				if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {
					
                    report += "<FONT COLOR='RED'>" + this._ItemCodingService.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails) + "</FONT>";
				}
			}

			if (comparison3 != null) {
				roias = comparison3.itemAttributesList.filter((x) => x.attributeId == attributeSet.attribute_id).sort(o => o.armId);

				for (var i = 0; i < roias.length; i++) {

					let roia: ReadOnlyItemAttribute = roias[i];

					report += "<li><FONT COLOR='GREEN'>[" + comparison3.contactName + "] " +
						attributeSet.attribute_name +
						(roia.armTitle == "" ? "" : " (" + roia.armTitle + ")") +
						"<br /><i>" + roia.additionalText + "</i></font></li>";
					oneReviewerHasSelected = true;
					if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {
                        report += "<FONT COLOR='GREEN'>" + this._ItemCodingService.addFullTextToComparisonReport(roia.itemAttributeFullTextDetails) + "</FONT>";
					}
				}

			}

			if (oneReviewerHasSelected == false) {

				if ((this.CodingReportCheckChildSelected(comparison1, attributeSet) == true) ||
					(this.CodingReportCheckChildSelected(comparison2, attributeSet) == true) ||
				(this.CodingReportCheckChildSelected(comparison3 != null? comparison3: new ItemSet(), attributeSet) == true)) // ie an attribute below this is selected, even though this one isn't
				{
					report += "<li>" + attributeSet.attribute_name + "</li>";
					report += "<ul>";
					
					for (var i = 0; i < attributeSet.attributes.length; i++) {

						let child: SetAttribute = attributeSet.attributes[i];
						report += this.writeComparisonReportAttributes(comparison1, comparison2, comparison3, child);
					}
					report += "</ul>";
				}
			}
			else {

				report += "<ul>";
			
				for (var i = 0; i < attributeSet.attributes.length; i++) {

					let child: SetAttribute = attributeSet.attributes[i];
					report += this.writeComparisonReportAttributes(comparison1, comparison2, comparison3, child);
				}
				report += "</ul>";
			}
		}
		else {

			report += "<li>" + attributeSet.attribute_name + "</li>";
			report += "<ul>";
			
			for (var i = 0; i < attributeSet.attributes.length; i++) {

				let child: SetAttribute = attributeSet.attributes[i];
				report += this.writeComparisonReportAttributes(comparison1, comparison2, comparison3, child);
			}
			report += "</ul>";
		}
		return report;
	}

	AddFullTextData(FTDL: ItemAttributeFullTextDetails[]) {
		//adds a list of full text details into the right itemSet and ItemAttribute if possible;

		for (var i = 0; i < FTDL.length; i++) {

			let ftd: ItemAttributeFullTextDetails = FTDL[i];
						
			let set: ItemSet = this.itemsSelected.filter((x) => x.itemSetId = ftd.itemSetId)[0];
			
			if (set == null) continue;
			let roia: ReadOnlyItemAttribute = set.itemAttributesList.filter( (x) => x.itemAttributeId ==ftd.itemAttributeId)[0];
			if (roia == null) continue;
			let oldElement: ItemAttributeFullTextDetails = roia.itemAttributeFullTextDetails.filter( (x) => x.isFromPDF == ftd.isFromPDF && x.itemAttributeTextId== ftd.itemAttributeTextId)[0];
			if (oldElement != null) {// to make sure we don't add the same "line" if it's already there
				let index = roia.itemAttributeFullTextDetails.findIndex((x) => x == oldElement);
				if (index > 0) {
					roia.itemAttributeFullTextDetails.splice(index, 1);
				}
			}
			roia.itemAttributeFullTextDetails.push(ftd);
			console.log('hello' + ftd);
		}

	}

	RunComparison() {

		let count: number = this.itemsSelected.length;
        if (!this.SetComparisons()) {
            return;
        }

        this._ItemCodingService.FetchAllFullTextData(this.item.itemId).then(

            (GetFTWorked: boolean) => {
                if (!GetFTWorked) {
                    return;
                }
                else {
                    let firstItemSetSelected = this._ItemCodingService.ItemCodingList.find((x) => x.setId == this.itemsSelected[0].setId);
                    if (firstItemSetSelected != undefined && this.comparison1 && this.comparison2) {
                        let reviewSet = this._ReviewSetsService.ReviewSets.find((x) => firstItemSetSelected != undefined && x.set_id == firstItemSetSelected.setId);
                        if (reviewSet == undefined) return;
                        else {
                            let report = "<p><h1>" + reviewSet.set_name + "</h1></p><p><ul>";

                            for (var i = 0; i < reviewSet.attributes.length; i++) {

                                let attributeSet: SetAttribute = reviewSet.attributes[i];

                                report += this.writeComparisonReportAttributes(this.comparison1, this.comparison2, this.comparison3, attributeSet);
                            }
                            report += "</ul></p>";
                            this._comparisonService.OpenInNewWindow(report);
                        }
                    }
                    
                }
				//console.log('blah blah: ' + JSON.stringify(fullText));


				//let isla: ItemSet[] = this.itemsSelected;
				//for (var i = 0; i < isla.length; i++) {

				//	this.AddFullTextData(fullText);
				//}
		
				//this.SetComparisons();

				//if (this.comparison1 == null || this.comparison2 == null) {
				//	alert('exiting');
				//	return;
				//}

				
				//let report: string = '';
				//if (reviewSet != null) {

				//	report = "<p><h1>" + reviewSet.set_name + "</h1></p><p><ul>";

				//	for (var i = 0; i < reviewSet.attributes.length; i++) {

				//		let attributeSet: SetAttribute = reviewSet.attributes[i];

				//		report += this.writeComparisonReportAttributes(this.comparison1, this.comparison2, this.comparison3, attributeSet);
				//	}
				//	report += "</ul></p>";
				//}
				//// need to open a new window with this html like previously
				//this._comparisonService.OpenInNewWindow(report);

			});
	}
	LiveComparison() {

	}
    BackToMain() {

        this.router.navigate(['Main']);
    }
	ngOnDestroy() {

		if (this.ItemCodingServiceDataChanged) {

			this.ItemCodingServiceDataChanged.unsubscribe();
		}

	}
}

export class CodingRecord {

	codeSet: string = '';
	reviewer: string = '';
	completed: boolean = false;
	locked: boolean = false;
	isSelected: boolean = false;
}








