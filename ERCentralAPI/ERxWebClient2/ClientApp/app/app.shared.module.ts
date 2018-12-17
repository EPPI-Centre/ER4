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
import { WorkAllocationContactListComp } from './components/WorkAllocationContactList/workAllocationContactListComp.component';
import { ItemListComp } from './components/ItemList/itemListComp.component';
import { ItemCodingComp } from './components/coding/coding.component';
import { paginatorComp } from './components/commonComponents/paginator.component';
import { StatusBarComponent } from './components/commonComponents/statusbar.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ItemDocListComp } from './components/ItemDocumentList/itemDocListComp.component';
import { intropageComponent } from './components/intropage/intropage.component';
import { HeaderComponent } from './components/commonComponents/header.component';
import { ModalDialogComponent } from './components/ModalDialog/ModalDialog.component';
import { armsComp } from './components/arms/armsComp.component';
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
import { DialogsModule  } from '@progress/kendo-angular-dialog';
import { frequenciesComp } from './components/Frequencies/frequencies.component';
import { CustomRouteReuseStrategy } from './components/helpers/CustomRouteReuseStrategy';
import { ImportReferencesFileComponent } from './components/Sources/importreferencesfile.component';
import { ROSourcesListComponent } from './components/Sources/ROSourcesList.component';
import { DatePickerModule } from '@progress/kendo-angular-dateinputs';
import { SourcesComponent } from './components/Sources/sources.component';
import { PubMedComponent } from './components/Sources/PubMed.component';
import { ReviewSetsEditorComponent } from './components/CodesetTrees/reviewSetsEditor.component';
import { CodesetTreeMainComponent } from './components/CodesetTrees/codesetTreeMain.component';


@NgModule({
    declarations: [
		AppComponent,
        CodesetTreeMainComponent,
		SearchComp,
        frequenciesComp,
        frequenciesResultsComp,
        ReviewSetsEditorComponent,
		CrossTabsComp,
		ReviewStatisticsComp,
		itemDetailsPaginatorComp,
        CodesetTreeCodingComponent,
        armsComp,
        FetchReadOnlyReviewsComponent,
        HomeComponent,
        WorkAllocationContactListComp,
        ItemListComp,
        ItemCodingComp,
        ItemCodingFullComp,
        itemDetailsComp,
        paginatorComp,
		StatusBarComponent,
		InfoBoxModalContent,
        ItemDocListComp,
        SourcesComponent,
        ImportReferencesFileComponent,
        PubMedComponent,
        intropageComponent,
        ModalDialogComponent,
        HeaderComponent,
        ROSourcesListComponent,
        MainFullReviewComponent,
        MainComponent
	],
    providers: [
        EventEmitterService,
		{ provide: RouteReuseStrategy, useClass: CustomRouteReuseStrategy }
    ],
	entryComponents: [InfoBoxModalContent, ModalDialogComponent],
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
        NotificationModule,
        DatePickerModule,
        LayoutModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'readonlyreviews', component: FetchReadOnlyReviewsComponent },
            { path: 'Main', component: MainFullReviewComponent }, 
            { path: 'MainCodingOnly', component: MainComponent }, 
            { path: 'sources', component: SourcesComponent },
            { path: 'itemcodingOnly/:itemId', component: ItemCodingComp },
            { path: 'itemcoding/:itemId', component: ItemCodingFullComp },
            { path: 'WorkAllocationContactListComp', component: WorkAllocationContactListComp },
            { path: 'EditCodeSets', component: ReviewSetsEditorComponent },
            { path: 'intropage', component: intropageComponent },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModuleShared {

}
