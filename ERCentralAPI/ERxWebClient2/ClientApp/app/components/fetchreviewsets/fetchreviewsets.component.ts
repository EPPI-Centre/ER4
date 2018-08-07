import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';

@Component({
    selector: 'fetchreviewsets',
    templateUrl: './fetchreviewsets.component.html'
})
export class FetchReviewSetsComponent implements OnInit {
    public ReviewSets: ReviewSet[] = [];
    
    constructor(private router: Router,
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService) {
    }
    ngOnInit() {
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
            this.router.navigate(['home']);
        }
        else {
            this.GetReviewSets();
        }
    }

    nodes: singleNode[] = [];
    options = {};
    GetReviewSets() {
        let body = "RevId=" + this.ReviewerIdentityServ.reviewerIdentity.reviewId;
        
        this._httpC.post<ReviewSet[]>(this._baseUrl + 'api/Codeset/CodesetsByReview',
            body).subscribe(
            result => {
                this.ReviewSets = result;
                this.nodes = this.ReviewSets as singleNode[];
                for (let singleNode of this.nodes) {
                    console.log(singleNode.name);
                }
            }, error => {
                console.error(error);
                this.router.navigate(['readonlyreviews']);
            }
            );
    }
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
