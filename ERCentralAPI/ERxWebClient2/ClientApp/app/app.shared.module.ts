import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule, RouteReuseStrategy } from '@angular/router';
import { TreeModule } from 'angular-tree-component';
import { AppComponent } from './components/app/app.component';
import { HomeComponent } from './components/home/home.component';
import { CodesetTreeCodingComponent, InfoBoxModalContent } from './components/CodesetTrees/codesetTreeCoding.component';
import { FetchReadOnlyReviewsComponent } from './components/readonlyreviews/readonlyreviews.component';
import { MainComponent } from './components/main/main.component';
import { MainFullReviewComponent } from './components/main/mainfull.component';
import { ItemListComp } from './components/ItemList/itemListComp.component';
import { ItemCodingComp } from './components/coding/coding.component';
import { paginatorComp } from './components/commonComponents/paginator.component';
import { StatusBarComponent } from './components/commonComponents/statusbar.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ItemDocListComp } from './components/ItemDocumentList/itemDocListComp.component';
import { intropageComponent } from './components/intropage/intropage.component';
import { HeaderComponent } from './components/commonComponents/header.component';
import { ModalDialogComponent } from './components/ModalDialog/ModalDialog.component';
import { armsComp } from './components/ArmsAndTimePoints/armsComp.component';
import { AngularFontAwesomeModule } from 'angular-font-awesome';
import { DataTablesModule } from 'angular-datatables';
import { ItemCodingFullComp } from './components/coding/codingFull.component';
import { itemDetailsComp } from './components/itemDetails/itemDetails.component';
import { ReviewStatisticsComp } from './components/reviewStatistics/reviewstatistics.component';
import { itemDetailsPaginatorComp } from './components/ItemDetailsPaginator/itemDetailsPaginator.component';
import { frequenciesResultsComp } from './components/Frequencies/frequenciesResults.component';
import { EventEmitterService } from './components/services/EventEmitter.service';
import { CrossTabsComp } from './components/CrossTabs/crosstab.component';
import { ChartsModule } from 'ng2-charts'
import { SearchComp } from './components/Search/SearchComp.component';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { GridModule, GridComponent } from '@progress/kendo-angular-grid';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { NotificationModule } from '@progress/kendo-angular-notification';
import { ChartModule } from '@progress/kendo-angular-charts';
import { ToolBarModule } from '@progress/kendo-angular-toolbar';
import { DialogsModule } from '@progress/kendo-angular-dialog';
import { UploadModule } from '@progress/kendo-angular-upload';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { frequenciesComp } from './components/Frequencies/frequencies.component';
import { CustomRouteReuseStrategy } from './components/helpers/CustomRouteReuseStrategy';
import { ImportReferencesFileComponent } from './components/Sources/importreferencesfile.component';
import { ROSourcesListComponent } from './components/Sources/ROSourcesList.component';
import { DatePickerModule } from '@progress/kendo-angular-dateinputs';
import { SourcesComponent } from './components/Sources/sources.component';
import { PubMedComponent } from './components/Sources/PubMed.component';
import { ReviewSetsEditorComponent } from './components/CodesetTrees/reviewSetsEditor.component';
import { CodesetTreeMainComponent } from './components/CodesetTrees/codesetTreeMain.component';
import { CodesetTreeEditComponent } from './components/CodesetTrees/codesetTreeEdit.component';
import { BuildModelComponent } from './components/BuildModel/buildmodel.component';
import { codesetSelectorComponent } from './components/CodesetTrees/codesetSelector.component';
import { ImportCodesetsWizardComponent } from './components/CodesetTrees/importCodesetsWizard.component';
import { codesetTree4CopyComponent } from './components/CodesetTrees/codesetTree4Copy.component';
import { ConfirmationDialogComponent } from './components/ConfirmationDialog/confirmation-dialog.component';
import { QuickCodingReportComponent } from './components/Reports/quickcodingreport.component';
import { NewReviewComponent } from './components/Review/newreview.component';
import 'hammerjs';
import { RunLingo3G } from './components/CodesetTrees/runlingo3g.component';
import { armDetailsComp } from './components/ArmsAndTimePoints/armDetailsComp.component';
import { CodesetTree4QuickQuestionReportComponent } from './components/CodesetTrees/codesetTree4QuickQuestionReport.component';
import { WorkAllocationComp } from './components/WorkAllocations/WorkAllocationComp.component';
import { SiteAdminComponent } from './components/SiteAdmin/siteadmin.component';
import { WorkAllocationContactListComp } from './components/WorkAllocations/WorkAllocationContactListComp.component';
import { SiteAdminEntryComponent } from './components/SiteAdmin/siteadminEntry.component';
import { editItemDetailsComp } from './components/itemDetails/editItemDetails.component';
import { CreateNewCodeComp } from './components/CodesetTrees/createnewcode.component';
import { ComparisonComp } from './components/Comparison/createnewcomparison.component';
import { ComparisonStatsComp } from './components/Comparison/comparisonstatistics.component';
import { ComparisonReconciliationComp } from './components/Comparison/comparisonreconciliation.component';
import { ArchieCallBackComponent } from './components/home/ArchieCallBack.component ';
import { WebViewerComponent } from './components/PDFTron/webviewer.component';
import { PdfTronContainer } from './components/PDFTron/pdftroncontainer.component';
import { codingRecordComp } from './components/CodingRecord/codingRecord.component';
import { LiveComparisonComp } from './components/CodingRecord/LiveComparison.component';
import { timePointsComp } from './components/ArmsAndTimePoints/timePointsComp.component';
import { OutcomesComponent } from './components/Outcomes/outcomes.component'
import { EditCodeComp } from './components/CodesetTrees/editcode.component';
import { SingleCodesetTreeCodingComponent } from './components/CodesetTrees/SingleCodesetTreeCoding.component';
import { TextSelectDirective } from './components/helpers/text-select.directive';
import { ReviewTermsListComp } from './components/ReviewTermsList/ReviewTermsListComp.component';
import { configurablereportComp } from './components/Reports/configurablereport.component';
import { DuplicatesComponent } from './components/Duplicates/duplicates.component';
import { codesetTree4Move } from './components/CodesetTrees/codesetTree4Move.component';
import { BasicMAGComp } from './components/MAG/BasicMAGComp.component';
import { AdvancedMAGFeaturesComponent } from './components/MAG/AdvancedMAGFeatures.component';
import { MAGpaginatorComp } from './components/commonComponents/MAGpaginator.component';
import { MAGBrowser } from './components/MAG/MAGBrowser.component';
import { MAGBrowserHistory } from './components/MAG/MAGBrowserHistory.component';
import { MAGAdminComp } from './components/MAG/MAGAdmin.component';
import { MatchingMAGItemsComponent } from './components/MAG/MatchingMAGItems.component';
import { WorkAllocationWizardComp } from './components/WorkAllocations/WorkAllocationWizardComp.component';
import { microsoftAcademicComp } from './components/MAG/microsoftAcademic.component';
import { MAGHeaderBar2Comp } from './components/commonComponents/MAGHeaderBar2.component';
import { ScreeningSetupComp } from './components/WorkAllocations/ScreeningSetup.component';
import { DatePipe } from '@angular/common';
import { MAGSearchComponent } from './components/MAG/MAGSearch.component';
import { WebDBsComponent } from './components/Review/WebDBs.component';
import { TruncatePipe } from './components/MAG/TruncatePipe.component';
import { MAGSearchDetailsComponent } from './components/MAG/MAGSearchDetails.component'; 
import { WebDbCcodesetTreeComponent } from './components/CodesetTrees/WebDbCcodesetTree.component';
import { CKEditorModule } from 'ckeditor4-angular';
import { MAGKeepUpToDate } from './components/MAG/MAGKeepUpToDate.component';
import { MAGComp } from './components/MAG/MAG.component';
import { ItemLinksComp } from './components/ArmsAndTimePoints/ItemLinks.component';
import { SetupConfigurableReports } from './components/Reports/SetupConfigurableReports.component';
import { FreqXtabMapsComp } from './components/Frequencies/FreqXtabMaps.component';
import { ReconcilingCodesetTreeComponent } from './components/CodesetTrees/ReconcilingCodesetTree.component';

@NgModule({
    declarations: [
		AppComponent,
		SearchComp,
        BasicMAGComp,
        frequenciesComp,
        frequenciesResultsComp,
        ReviewSetsEditorComponent,
		CrossTabsComp,
		ReviewStatisticsComp,
		itemDetailsPaginatorComp,
        CodesetTreeMainComponent,
		CodesetTreeCodingComponent,
        CodesetTreeEditComponent,
        ImportCodesetsWizardComponent,
        codesetTree4CopyComponent,
		codesetSelectorComponent,
		armsComp,
		armDetailsComp,
        FetchReadOnlyReviewsComponent,
        HomeComponent,
        ArchieCallBackComponent, 
        ItemListComp,
        ItemCodingComp,
        ItemCodingFullComp,
        itemDetailsComp,
        editItemDetailsComp,
        paginatorComp,
		StatusBarComponent,
		InfoBoxModalContent,
        ItemDocListComp,
        LiveComparisonComp,
		SourcesComponent,
		BuildModelComponent,
        ImportReferencesFileComponent,
        PubMedComponent,
        intropageComponent,
        ModalDialogComponent,
        HeaderComponent,
        ROSourcesListComponent,
        MainFullReviewComponent,
		MainComponent,
        ConfirmationDialogComponent,
        QuickCodingReportComponent,
        CodesetTree4QuickQuestionReportComponent,
        NewReviewComponent,
		RunLingo3G,
        WorkAllocationComp,
        WorkAllocationContactListComp,
        SiteAdminComponent,
		SiteAdminEntryComponent,
		CreateNewCodeComp,
		ComparisonComp,
		ComparisonStatsComp,
		ComparisonReconciliationComp,
		timePointsComp,
        PdfTronContainer,
        WebViewerComponent,
		codingRecordComp,
        OutcomesComponent,
		EditCodeComp,
		SingleCodesetTreeCodingComponent,
		TextSelectDirective,
		ReviewTermsListComp,
        configurablereportComp,
        DuplicatesComponent,
        codesetTree4Move,
        AdvancedMAGFeaturesComponent,
        MAGpaginatorComp,
        MAGBrowser,
        MAGBrowserHistory,
        MAGAdminComp,
        MatchingMAGItemsComponent,
        MAGAdminComp,
        MAGKeepUpToDate,
        WorkAllocationWizardComp,
        microsoftAcademicComp,
        MAGHeaderBar2Comp,
        MAGComp,
        ScreeningSetupComp,
        MAGSearchComponent,
        TruncatePipe,
        MAGSearchDetailsComponent,
        WebDBsComponent,
        WebDbCcodesetTreeComponent,
        ItemLinksComp,
        SetupConfigurableReports,
        FreqXtabMapsComp,
        ReconcilingCodesetTreeComponent
	],
    providers: [
        DatePipe, 
        EventEmitterService,
        {
            provide: RouteReuseStrategy, useClass: CustomRouteReuseStrategy
        }
    ],
	entryComponents: [InfoBoxModalContent, ModalDialogComponent, ConfirmationDialogComponent],
	imports: [
        AngularFontAwesomeModule,
		DataTablesModule,
		CommonModule,
        NgbModule,
        HttpModule,
		FormsModule,
		BrowserModule,
		BrowserAnimationsModule,
		ChartsModule,
        ReactiveFormsModule,
		TreeModule,
        GridModule,
        ChartModule,
        DialogsModule,
        ToolBarModule,
        InputsModule,
		UploadModule,
		ButtonsModule,
        NotificationModule,
        DatePickerModule,
        LayoutModule,
        CKEditorModule, 
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'ArchieCallBack', component: ArchieCallBackComponent },
            { path: 'readonlyreviews', component: FetchReadOnlyReviewsComponent },
            { path: 'Main', component: MainFullReviewComponent }, 
            { path: 'MainCodingOnly', component: MainComponent }, 
			{ path: 'sources', component: SourcesComponent },
            { path: 'BuildModel', component: BuildModelComponent },
            { path: 'BasicMAGFeatures', component: BasicMAGComp },
            { path: 'ItemList', component: ItemListComp },
            { path: 'MAGAdmin', component: MAGAdminComp },
            { path: 'MAG', component: MAGComp },
            { path: 'MAG/:paperId', component: MAGComp },
            { path: 'MAGBrowser', component: MAGBrowser },
            { path: 'MagSearch', component: MAGSearchComponent },
            { path: 'MAGKeepUpToDate', component: MAGKeepUpToDate }, 
            { path: 'microsoftAcademic', component: microsoftAcademicComp },
            { path: 'MAGBrowserHistory', component: MAGBrowserHistory },
            { path: 'MatchingMAGItems', component: MatchingMAGItemsComponent},
            { path: 'AdvancedMAGFeatures', component: AdvancedMAGFeaturesComponent },
            { path: 'itemcodingOnly/:itemId', component: ItemCodingComp },
            { path: 'itemcoding/:itemId', component: ItemCodingFullComp },
            { path: 'EditItem/:itemId', component: editItemDetailsComp },
            { path: 'EditItem', component: editItemDetailsComp },
			{ path: 'EditCodeSets', component: ReviewSetsEditorComponent },
			{ path: 'Reconciliation', component: ComparisonReconciliationComp},
            { path: 'ImportCodesets', component: ImportCodesetsWizardComponent },
            { path: 'intropage', component: intropageComponent },
            { path: 'Duplicates', component: DuplicatesComponent },
            { path: 'SiteAdmin', component: SiteAdminComponent }, 
            { path: 'WebDBs', component: WebDBsComponent },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModuleShared {

}
