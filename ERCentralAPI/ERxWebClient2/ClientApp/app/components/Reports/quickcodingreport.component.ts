import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ItemListService } from '../services/ItemList.service';
import { ReviewSet, ReviewSetsService } from '../services/ReviewSets.service';

@Component({
    selector: 'quickcodingreport',
    templateUrl: './quickcodingreport.component.html',
})
export class QuickCodingReportComponent implements OnInit, OnDestroy {

	

    constructor(
        private ItemCodingService: ItemCodingService,
        private ItemListService: ItemListService,
        private ReviewSetsService: ReviewSetsService
    ) { }

	ngOnInit() {
	}
    public get GettingReport(): boolean {
        return this.ItemCodingService.QuickCodingReportIsRunning;
    }
    public get ReportProgress(): string {
        return this.ItemCodingService.ProgressOfQuickCodingReport;
    }
    public get ReportHTML(): string {
        return this.ItemCodingService.CodingReport;
    }
    public get CodeSets(): ReviewSet[] {
        return this.ReviewSetsService.ReviewSets;
    }
    public StartQuickReport() {
        if (!this.CanStartReport) return;
        else {
            this.ItemCodingService.FetchCodingReport(this.ItemListService.SelectedItems, this.ReviewSetsService.ReviewSets.filter(found => found.isSelected == true));
        }
    }
    public get CanStartReport(): boolean {
        if (this.HasSelectedCodesets && this.ItemListService.HasSelectedItems) return true;
        else return false;
    }
    private get HasSelectedCodesets(): boolean {
        for (let Set of this.ReviewSetsService.ReviewSets) if (Set.isSelected) return true;
        return false;
    }
    ngOnDestroy() {
        console.log("Destroy in QuickCodingReportComponent");
        this.ReviewSetsService.clearItemData();//because we are hijacking the "isSelected" field of reviewSets;
    }
}
interface SelectableReviewSet {
    isSelected: boolean;
    reviewSet: ReviewSet;
}