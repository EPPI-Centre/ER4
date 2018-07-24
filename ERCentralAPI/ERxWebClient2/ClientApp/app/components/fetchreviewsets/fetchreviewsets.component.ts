import { Component, Inject } from '@angular/core';
import { Http, Response, Headers, RequestOptions, RequestMethod } from '@angular/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../app/revieweridentity.service';
import { Router } from '@angular/router';

@Component({
    selector: 'fetchreviewsets',
    templateUrl: './fetchreviewsets.component.html'
})
export class FetchReviewSetsComponent {
    public ReviewSets: ReviewSet[] = [];
    private headers: Headers = new Headers({ "Content-Type": 'application/x-www-form-urlencoded' });
    constructor(private router: Router,
        private http: Http,
        @Inject('BASE_URL') private baseUrl: string,
        private ReviewerIdentity: ReviewerIdentityService) {
        if (ReviewerIdentity.userId == 0 || ReviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else {
            this.GetReviewSets();
        }
        //this.nodes = this.ReviewSets;
    }
    nodes: singleNode[] = [];
    options = {};
    GetReviewSets() {
        let body = "RevId=" + this.ReviewerIdentity.reviewId;
        //let body = JSON.stringify({ 'contactId': 1 });
        let requestoptions = new RequestOptions({
            method: RequestMethod.Post,
            url: this.baseUrl + 'api/Codeset/CodesetsByReview',
            headers: this.headers,
            body: body
        });

        this.http.request(this.baseUrl + 'api/review/reviewsbycontact', requestoptions).subscribe(result => {
            this.ReviewSets = result.json() as ReviewSet[];
            this.nodes = this.ReviewSets as singleNode[];
            for (let singleNode of this.nodes) {
                console.log(singleNode.name);
            }
        }, error => console.error(error));
        //this.http.get(this.baseUrl + 'api/Codeset/CodesetsByReview').subscribe(result => {
        //    this.ReviewSets = result.json() as ReviewSet[];
        //    this.nodes = this.ReviewSets as singleNode[];
        //    for (let singleNode of this.nodes) {
        //        console.log(singleNode.name);
        //    }
        //}, error => console.error(error));
    }
    //ngOnInit() {
    //    //treeview = $.fn['treeview'] as Function;
    //    getTree();
    //    alert('building tree');
    //    ($('#tree')).treeview({ data: getTree() });
    //}
}


interface singleNode {
    id: number;
    name: string;
    children: singleNode[];
}

export class ReviewSet implements singleNode{
    set_id: number = -1;
    id: number = this.set_id;
    set_name: string = "";
    name: string = this.set_name;
    set_type: string = "";
    set_order: number = -1;
    attributes: SetAttribute[] = [];
    ShowCheckBox: boolean = false;
    children: SetAttribute[] = this.attributes;
}
export class SetAttribute implements singleNode{
    attribute_id: number = -1;
    id: number = this.attribute_id;
    attribute_name: string = "";
    name: string = this.attribute_name;
    attribute_order: number = -1;;
    attribute_type: string = "";
    attribute_set_desc: string = "";
    attribute_desc: string = "";
    showCheckBox: boolean = false;
    parent_attribute_id: number = -1;;
    attribute_type_id: number = -1;;
    attributes: SetAttribute[] = [];
    children: SetAttribute[] = this.attributes;
}
