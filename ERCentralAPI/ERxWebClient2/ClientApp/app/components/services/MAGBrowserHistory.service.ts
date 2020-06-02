import { Injectable } from "@angular/core";
import { Router,  NavigationEnd } from '@angular/router';
import { filter } from "rxjs/operators";
import { Subscription } from "rxjs";

@Injectable({

    providedIn: 'root',

})

export class MAGBrowserHistoryService {
    private history: NavigationEnd[] = [];
    public MAGSubscription: Subscription = new Subscription();
    constructor(

        private router: Router

    ) { }
    public loadRouting(): void {

        this.MAGSubscription = this.router.events
            .pipe(filter(event => event instanceof NavigationEnd))

            .subscribe(
                    ( url: any) =>
                    {
                        var test = url as NavigationEnd;
                        this.history = [...this.history, test];
                    });
    }
    public getHistory(): NavigationEnd[] {
        return this.history;

    }
    public ClearHistory() {

        this.history = [];
    }
    public getPreviousUrl(): NavigationEnd {

        return this.history[this.history.length - 2] || '/index';
    }
    public UnsubscribeMAGHistory() {

        this.MAGSubscription.unsubscribe();
    }
}