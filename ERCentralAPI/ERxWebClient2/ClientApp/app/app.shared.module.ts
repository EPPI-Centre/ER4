import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { TreeModule } from 'angular-tree-component';

import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { FetchDataComponent } from './components/fetchdata/fetchdata.component';
import { CounterComponent } from './components/counter/counter.component';
import { FetchReviewSetsComponent } from './components/fetchreviewsets/fetchreviewsets.component';
import { FetchReadOnlyReviewsComponent } from './components/readonlyreviews/readonlyreviews.component';
import { MainComponent } from './components/main/main.component';
import { WorkAllocationContactListComp } from './components/WorkAllocationContactList/workAllocationContactListComp.component';
import { ItemListComp } from './components/ItemList/itemListComp.component';


@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        CounterComponent,
        FetchDataComponent,
        FetchReviewSetsComponent,
        FetchReadOnlyReviewsComponent,
        HomeComponent,
        WorkAllocationContactListComp,
        ItemListComp,
        MainComponent
    ],
    imports: [
        CommonModule,
        HttpModule,
        FormsModule,
        TreeModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'counter', component: CounterComponent },
            { path: 'fetch-data', component: FetchDataComponent },
            { path: 'fetch-reviewsets', component: FetchReviewSetsComponent },
            { path: 'readonlyreviews', component: FetchReadOnlyReviewsComponent },
            { path: 'main', component: MainComponent }, 
            { path: 'WorkAllocationContactListComp', component: WorkAllocationContactListComp },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModuleShared {
}
