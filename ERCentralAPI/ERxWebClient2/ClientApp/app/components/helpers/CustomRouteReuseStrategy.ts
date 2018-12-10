/**
 * reuse-strategy.ts
 * by corbfon 1/6/17
 * See: https://stackoverflow.com/questions/41280471/how-to-implement-routereusestrategy-shoulddetach-for-specific-routes-in-angular
 * modified By Sergio Graziosi from 26/11/2018...
 */

import { ActivatedRouteSnapshot, RouteReuseStrategy, DetachedRouteHandle } from '@angular/router';

/** Interface for object which can store both: 
 * An ActivatedRouteSnapshot, which is useful for determining whether or not you should attach a route (see this.shouldAttach)
 * A DetachedRouteHandle, which is offered up by this.retrieve, in the case that you do want to attach the stored route
 */
interface RouteStorageObject {
    snapshot: ActivatedRouteSnapshot;
    handle: DetachedRouteHandle;
}

export class CustomRouteReuseStrategy implements RouteReuseStrategy {

    /** 
     * Object which will store RouteStorageObjects indexed by keys
     * The keys will all be a path (as in route.routeConfig.path)
     * This allows us to see if we've got a route stored for the requested path
     */
    private storedRoutes: { [key: string]: RouteStorageObject } = {};

    //these are pages that will always mean the user is going to log on a review in order to reach mainfull.
    //thus, we never store mainfull when going to one of these. This is to avoid leaking components that report "shouldDetach == true"
    private KillDestinations: string[] = ["main", "home", "intropage"];

    //IMPORTANT! We statically "keep" only these routes, all the others are not recyled...
    private routesToCache: string[] = ["mainFullReview"];
    private GoingTo: string = "";
    /** 
     * Determines whether or not the current route should be reused
     * @param future The route the user is going to, as triggered by the router
     * @param curr The route the user is currently on
     * @returns boolean basically indicating true if the user intends to leave the current route
     */
    //shouldReuseRoute(future: ActivatedRouteSnapshot, curr: ActivatedRouteSnapshot): boolean {
    //EEEEK!!? the line above causes the log message to report the current page as the future one, so I'll swap the paramaters around...
    shouldReuseRoute(curr: ActivatedRouteSnapshot, future: ActivatedRouteSnapshot): boolean {
        console.log("shouldReuseRoute:", "future", future.routeConfig, "current", curr.routeConfig, "return: ", future.routeConfig === curr.routeConfig);
        if (future && future.routeConfig && future.routeConfig.path) {
            console.log("shouldReuseRoute, GOINGTO:" + future.routeConfig.path);
            this.GoingTo = future.routeConfig.path;
        }
        //if (curr.routeConfig === null && future.routeConfig === null) {
        //    console.log("Null case, ret: false");
        //    return false;
        //}
        //if (curr.routeConfig && future.routeConfig
        //    && curr.routeConfig.path == "mainFullReview"
        //    && (future.routeConfig.path == "main" || future.routeConfig.path == "home" || future.routeConfig.path == "readonlyreviews")) {
        //    console.log("I'll return something special because of where we're going... this is an experiment!")
        //    return true;
        //}
        return future.routeConfig === curr.routeConfig;
    }


    /** 
     * Decides when the route should be stored
     * If the route should be stored, I believe the boolean is indicating to a controller whether or not to fire this.store
     * _When_ it is called though does not particularly matter, just know that this determines whether or not we store the route
     * An idea of what to do here: check the route.routeConfig.path to see if it is a path you would like to store
     * @param route This is, at least as I understand it, the route that the user is currently on, and we would like to know if we want to store it
     * @returns boolean indicating that we want to (true) or do not want to (false) store that route
     */
    shouldDetach(route: ActivatedRouteSnapshot): boolean {
        //Modified by SG
        //IMPORTANT! Do not mark current page as in need of detaching when we are going to a page in KillDestinations.
        //when reaching these pages, we'll have to "logintoreview" in order to return to mainfull, so we don't want to store current mainfull.
        //however, if this method returns true, ngOnDestroy isn't called and we leak: instance remains alive (memory leak) and worse, we leak subscriptions!
        if (this.KillDestinations.indexOf(this.GoingTo) > -1) return false;
        if (route.routeConfig && route.routeConfig.path) {
            if (this.routesToCache.indexOf(route.routeConfig.path) > -1) {
                console.log("shouldDetach, will return true!!!!!!!!!!!!!!!!!", route);
                return true;
            }
        }
        console.log("shouldDetach, will return false.", route);
        return false;
        //let detach: boolean = true;
        //console.log("detaching", route, "return: ", detach);
        //return detach;
    }

    /**
     * Constructs object of type `RouteStorageObject` to store, and then stores it for later attachment
     * @param route This is stored for later comparison to requested routes, see `this.shouldAttach`
     * @param handle Later to be retrieved by this.retrieve, and offered up to whatever controller is using this class
     */
    store(route: ActivatedRouteSnapshot, handle: DetachedRouteHandle): void {
        let storedRoute: RouteStorageObject = {
            snapshot: route,
            handle: handle
        };

        console.log( "store component for reuse:", storedRoute, "into: ", this.storedRoutes );
        // routes are stored by path - the key is the path name, and the handle is stored under it so that you can only ever have one object stored for a single path
        if (route.routeConfig && route.routeConfig.path) this.storedRoutes[route.routeConfig.path] = storedRoute;
    }

    /**
     * Determines whether or not there is a stored route and, if there is, whether or not it should be rendered in place of requested route
     * @param route The route the user requested
     * @returns boolean indicating whether or not to render the stored route
     */
    shouldAttach(route: ActivatedRouteSnapshot): boolean {
        if (route.routeConfig && route.routeConfig.path) {
            // this will be true if the route has been stored before
            let canAttach: boolean = !!route.routeConfig && !!this.storedRoutes[route.routeConfig.path];
            //Modified BY SG
            //Assumptions:
            //1. we reuse routes only on few big pages.
            //2. if we stored a page, we'll ALWAYS reuse it.
            return canAttach;


            //// this decides whether the route already stored should be rendered in place of the requested route, and is the return value
            //// at this point we already know that the paths match because the storedResults key is the route.routeConfig.path
            //// so, if the route.params and route.queryParams also match, then we should reuse the component
            //if (canAttach) {
            //    let willAttach: boolean = true;
            //    console.log("param comparison:");
            //    console.log(this.compareObjects(route.params, this.storedRoutes[route.routeConfig.path].snapshot.params));
            //    console.log("query param comparison");
            //    console.log(this.compareObjects(route.queryParams, this.storedRoutes[route.routeConfig.path].snapshot.queryParams));

            //    let paramsMatch: boolean = this.compareObjects(route.params, this.storedRoutes[route.routeConfig.path].snapshot.params);
            //    let queryParamsMatch: boolean = this.compareObjects(route.queryParams, this.storedRoutes[route.routeConfig.path].snapshot.queryParams);

            //    console.log("deciding to attach...", route, "does it match?", this.storedRoutes[route.routeConfig.path].snapshot, "return: ", paramsMatch && queryParamsMatch);
            //    return paramsMatch && queryParamsMatch;
            //} else {
            //    return false;
            //}
        } else {
            return false;
        }
    }

    /** 
     * Finds the locally stored instance of the requested route, if it exists, and returns it
     * @param route New route the user has requested
     * @returns DetachedRouteHandle object which can be used to render the component
     */
    retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle | null {

        // return null if the path does not have a routerConfig OR if there is no stored route for that routerConfig
        if (!route.routeConfig || !route.routeConfig.path || !this.storedRoutes[route.routeConfig.path]) return null;
        console.log("retrieving", "return: ", this.storedRoutes[route.routeConfig.path]);

        /** returns handle when the route.routeConfig.path is already stored */
        //debugger;
        //let requestedPath = route.routeConfig.path;
        return this.storedRoutes[route.routeConfig.path].handle;
    }

    //SG Added: method to clear current saved pages...
    public Clear() {
        console.log('clearing cached component instances.', this.storedRoutes);
        //while (this.storedRoutes != {}) {
        //    this.storedRoutes[0]
        //}
        //for (var comK in this.storedRoutes) {
        //    let el = this.storedRoutes[comK];
        //    console.log("trying to destroy this: ", el.snapshot);
        //    //if (el.snapshot && el.snapshot.component && !(typeof el.snapshot.component == 'string') ) {
        //    //    if ('ngOnDestroy' in el.snapshot.component && ) el.snapshot.component.ngOnDestroy();
        //    //}
        //    if (el.snapshot && el.snapshot.component && !(typeof el.snapshot.component == 'string')) {
        //        //if ('ngOnDestroy' in el.snapshot.component ) {
        //        console.log("trying to destroy this: ", el.snapshot);
        //            el.snapshot.component = null;
        //        //el.handle = new DetachedRouteHandle();
        //        //el =  { };
        //        //}
        //    }
        //}
        this.storedRoutes = {};
    }

    /** 
     * This nasty bugger finds out whether the objects are _traditionally_ equal to each other, like you might assume someone else would have put this function in vanilla JS already
     * One thing to note is that it uses coercive comparison (==) on properties which both objects have, not strict comparison (===)
     * @param base The base object which you would like to compare another object to
     * @param compare The object to compare to base
     * @returns boolean indicating whether or not the objects have all the same properties and those properties are ==
     */
    //By SG: not used as of 26/11/2018, but kept as might be handy someday.
    private compareObjects(base: any, compare: any): boolean {

        // loop through all properties in base object
        for (let baseProperty in base) {

            // determine if comparrison object has that property, if not: return false
            if (compare.hasOwnProperty(baseProperty)) {
                switch(typeof base[baseProperty]) {
                    // if one is object and other is not: return false
                    // if they are both objects, recursively call this comparison function
                    case 'object':
                        if ( typeof compare[baseProperty] !== 'object' || !this.compareObjects(base[baseProperty], compare[baseProperty]) ) { return false; } break;
                    // if one is function and other is not: return false
                    // if both are functions, compare function.toString() results
                    case 'function':
                        if ( typeof compare[baseProperty] !== 'function' || base[baseProperty].toString() !== compare[baseProperty].toString() ) { return false; } break;
                    // otherwise, see if they are equal using coercive comparison
                    default:
                        if ( base[baseProperty] != compare[baseProperty] ) { return false; }
                }
            } else {
                return false;
            }
        }

        // returns true only after false HAS NOT BEEN returned through all loops
        return true;
    }
}