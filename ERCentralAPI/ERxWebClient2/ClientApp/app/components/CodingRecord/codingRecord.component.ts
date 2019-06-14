import { Component, OnInit, OnDestroy, Input} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService,  Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute, ItemAttributeFullTextDetails } from '../services/ItemCoding.service';
import { ReviewSetsService, ReviewSet, SetAttribute} from '../services/ReviewSets.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ArmsService } from '../services/arms.service';
import { NotificationService } from '@progress/kendo-angular-notification';

@Component({
   
	selector: 'codingRecordComp',
	templateUrl: './codingRecord.component.html',
    providers: [],
    styles: [`
                button.disabled {
                    color:black; 
                    }
            `]

})

export class codingRecordComp implements OnInit, OnDestroy {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService
		, private route: ActivatedRoute,
		private _ItemCodingService: ItemCodingService,
        private _ReviewSetsService: ReviewSetsService,
        private reviewInfoService: ReviewInfoService,
        public PriorityScreeningService: PriorityScreeningService
        , private ReviewerTermsService: ReviewerTermsService,
        public ItemDocsService: ItemDocsService,
        private armservice: ArmsService,
        private notificationService: NotificationService
    ) { }
  
	@Input() item: Item = new Item();
	public CodingRecords: ItemSet[] = [];
	public checkboxValue: boolean = false;
	public itemsSelected: ItemSet[] = [];
	private comparison1: ItemSet | null = null;
	private comparison2: ItemSet | null = null;
	private comparison3: ItemSet | null = null;

    ngOnInit() {

		this._ItemCodingService.ItemCodingList.forEach(

			(x) => {

				let tmp = new ItemSet();
				tmp = x;
				this.CodingRecords.push(tmp);

			});
		//console.log(JSON.stringify(this.CodingRecords));

	}
	checkboxChange(index: number, item: CodingRecord) {

		this.itemsSelected = this.CodingRecords.filter((x) => x.isSelected == true);
		let count: number = this.itemsSelected.length;

		if (count < 2 || count > 3) {
			//alert('The number of selected coding records is incorrect');
			return;
		}

		this.checkboxValue = item.isSelected;
		//alert('index: ' + index + 'checkboxValue: ' + this.checkboxValue);


	}
	SetComparisons(): boolean {

		//let comparison1: CodingRecord = new CodingRecord();
		//let comparison2: CodingRecord = new CodingRecord();
		//let comparison3: CodingRecord = new CodingRecord();

		let isla: ItemSet[] = this.itemsSelected;
		let itemSet: ItemSet = new ItemSet();

		if (isla != null) {

			for (var i = 0; i < isla.length; i++) {

				itemSet = isla[i];	
				console.log('debug setcomparisons: ' + JSON.stringify(itemSet));
				if (itemSet.isSelected == true) {
					if (this.comparison1 == null)
						this.comparison1 = itemSet;
					else
						if (this.comparison2 == null)
							this.comparison2 = itemSet;
						else
							if (this.comparison3 == null)
								this.comparison3 = itemSet;
							else {
								alert("A maximum of three comparisons can be run at once.");
								return false;
							}
				}
			}
			if (this.comparison1 == null) {
				alert("Nothing selected to compare");
				return false;
			}
			if (this.comparison2 == null) {
				alert("You need to select at least two lines to compare");
				return false;
			}
			if (this.comparison2.setId != this.comparison1.setId) {
				alert("Selected items must be the same code set");
				return false;
			}
			if ((this.comparison3 != null) && (this.comparison3.setId != this.comparison2.setId)) {
				alert("Selected items must be the same code set");
				return false;
			}
		}
		return true;
	}
	addFullTextToComparisonReport(list: ItemAttributeFullTextDetails[]): string {
		let result: string = "";

		for (var i = 0; i < list.length; i++) {

			let ftd: ItemAttributeFullTextDetails = list[i];
			result += "<br>" + ftd.docTitle + ": ";
			if (ftd.isFromPDF) {
				result += "<span style='font-size:15px;'>" + ftd.text.replace("[¬s]", "").replace("[¬e]", "") + "</span>";
			}
			else {
				result += "<span style='font-family:Courier New; font-size:12px;'>" + ftd.text + "(from char " + ftd.textFrom.toString() + " to char " + ftd.textTo.toString()
					+ ")</span>";
			}
		}
		return result;
	}
	private CodingReportCheckChildSelected(itemSet: ItemSet,  attributeSet: SetAttribute): boolean {

		//console.log('Called this report again....');
		if (itemSet != null) {

			//console.log('list the item attributes: ' + JSON.stringify(itemSet.itemAttributesList));
			if (itemSet.itemAttributesList.filter((x) => x.attributeId == attributeSet.attribute_id) != null) {
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
			
			let roias: ReadOnlyItemAttribute[]  = comparison1.itemAttributesList.filter( (x) => x.attributeId== attributeSet.attribute_id).sort(o => o.armId);

			for (var i = 0; i < roias.length; i++) {

				let roia: ReadOnlyItemAttribute = roias[i];

				report += "<li><FONT COLOR='BLUE'>[" + comparison1.contactName + "] " +
					attributeSet.attribute_name +

					(roia.armTitle == "" ? "" : " (" + roia.armTitle + ")") +

					"<br /><i>" + roia.additionalText + "</i></font></li>";

				oneReviewerHasSelected = true;

				if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {

					let ll: ItemAttributeFullTextDetails[] = roia.itemAttributeFullTextDetails;

					ll.sort();

					report += "<FONT COLOR='BLUE'>" + this.addFullTextToComparisonReport(ll) + "</FONT>";
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
					let ll: ItemAttributeFullTextDetails[] = roia.itemAttributeFullTextDetails;
					ll.sort();
					report += "<FONT COLOR='RED'>" + this.addFullTextToComparisonReport(ll) + "</FONT>";
				}
			}

			if (comparison3 != null) {
				alert('comparison 3??');
				roias = comparison3.itemAttributesList.filter((x) => x.attributeId == attributeSet.attribute_id).sort(o => o.armId);

				for (var i = 0; i < roias.length; i++) {

					let roia: ReadOnlyItemAttribute = roias[i];

					report += "<li><FONT COLOR='GREEN'>[" + comparison3.contactName + "] " +
						attributeSet.attribute_name +
						(roia.armTitle == "" ? "" : " (" + roia.armTitle + ")") +
						"<br /><i>" + roia.additionalText + "</i></font></li>";
					oneReviewerHasSelected = true;
					if (roia.itemAttributeFullTextDetails != null && roia.itemAttributeFullTextDetails.length > 0) {
						let ll: ItemAttributeFullTextDetails[] = roia.itemAttributeFullTextDetails;
						ll.sort();
						report += "<FONT COLOR='GREEN'>" + this.addFullTextToComparisonReport(ll) + "</FONT>";
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
					console.log('children are: ' + JSON.stringify(attributeSet.attributes));
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
				alert('here?1');
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
			alert('here?2');
			report += "</ul>";
		}
		return report;
	}

	RunComparison() {

		let count: number = this.itemsSelected.length;

		if (count < 2 || count > 3) {
			alert('The number of selected coding records is incorrect');
			return;
		}
		//also need to check codesets are all the same
		for (var i = 1; i < count; i++) {
			let tmpSetName = this.itemsSelected[i].setId;
			if (tmpSetName != this.itemsSelected[i - 1].setId) {
				alert('Codesets being compared should all be the same');
				return;
			}
		}

		let isla: ItemSet[] = this.itemsSelected;

		//console.log('checking correct items were selected: ' + JSON.stringify(isla));

		//isla.AddFullTextData(e2.Object as ItemAttributesAllFullTextDetailsList);

		this.SetComparisons();

		if (this.comparison1 == null || this.comparison2 == null) {
			alert('exiting');
			return;
		}

		let firstItemSelected: ItemSet = this._ItemCodingService.ItemCodingList.filter((x) => x.setId == this.itemsSelected[0].setId)[0];
		let reviewSet: ReviewSet = this._ReviewSetsService.GetReviewSets().filter((x) => x.set_id == firstItemSelected.setId)[0];
		let report: string = '';
		if (reviewSet != null) {

			report = "<p><h1>" + reviewSet.set_name + "</h1></p><p><ul>";

			for (var i = 0; i < reviewSet.attributes.length; i++) {

				let attributeSet: SetAttribute = reviewSet.attributes[i];
				
				report += this.writeComparisonReportAttributes(this.comparison1, this.comparison2, this.comparison3, attributeSet);
			}
			report += "</ul></p>";
		}

		console.log('Here is the report: ' + report);

	}
	LiveComparison() {

	}
    BackToMain() {

        this.router.navigate(['Main']);
    }
    ngOnDestroy() {

	}
}

export class CodingRecord {

	codeSet: string = '';
	reviewer: string = '';
	completed: boolean = false;
	locked: boolean = false;
	isSelected: boolean = false;
}






