import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { TreeModule } from 'angular-tree-component';
import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { CounterComponent } from './components/counter/counter.component';
import { ReviewSetsComponent, InfoBoxModalContent } from './components/reviewsets/reviewsets.component';
import { FetchReadOnlyReviewsComponent } from './components/readonlyreviews/readonlyreviews.component';
import { MainComponent } from './components/main/main.component';
import { WorkAllocationContactListComp } from './components/WorkAllocationContactList/workAllocationContactListComp.component';
import { ItemListComp } from './components/ItemList/itemListComp.component';
import { ItemCodingComp } from './components/coding/coding.component';
import { paginatorComp } from './components/paginator/paginator.component';
import { StatusBarComponent } from './components/StatusBar/statusbar.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ItemDocListComp } from './components/ItemDocumentList/itemDocListComp.component';
import { intropageComponent } from './components/intropage/intropage.component';
import { FilterPipe } from './filters/filter.pipe';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        CounterComponent,
        ReviewSetsComponent,
        FetchReadOnlyReviewsComponent,
        HomeComponent,
        WorkAllocationContactListComp,
        ItemListComp,
        ItemCodingComp,
        paginatorComp,
        StatusBarComponent,
        InfoBoxModalContent,
        ItemDocListComp,
        intropageComponent,
        FilterPipe ,
        MainComponent
    ],
    entryComponents: [InfoBoxModalContent],
    imports: [
        CommonModule,
        NgbModule,
        HttpModule,
        FormsModule,
        ReactiveFormsModule,
        TreeModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'counter', component: CounterComponent },
            { path: 'reviewsets', component: ReviewSetsComponent },
            { path: 'readonlyreviews', component: FetchReadOnlyReviewsComponent },
            { path: 'main', component: MainComponent }, 
            { path: 'itemcoding/:itemId', component: ItemCodingComp },
            { path: 'WorkAllocationContactListComp', component: WorkAllocationContactListComp },//intropage
            { path: 'intropage', component: intropageComponent },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModuleShared {
}
