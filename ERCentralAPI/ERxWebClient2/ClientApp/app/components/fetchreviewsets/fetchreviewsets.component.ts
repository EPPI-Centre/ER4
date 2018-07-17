
import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { forEach } from '@angular/router/src/utils/collection';
//import 'angular-tree-component/dist/angular-tree-component.css';
//declare var $: any; 
//import $ from "jquery";
//declare var $: $;
//declare var jQuery: JQuery;
//declare var $.treeview: any;// = $.fn['treeview'];
//import '../../../../node_modules/bootstrap-treeview/src/js/bootstrap-treeview.js';
//import * as  $ from 'jquery';
//declare var jQuery: JQuery;
//import *  as any from '../../../../node_modules/bootstrap-treeview/src/js/bootstrap-treeview.js';

@Component({
    selector: 'fetchreviewsets',
    templateUrl: './fetchreviewsets.component.html'
})
export class FetchReviewSetsComponent {
    public ReviewSets: ReviewSet[];
    constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
        http.get(baseUrl + 'api/Codeset/CodesetsByReview').subscribe(result => {
            this.ReviewSets = result.json() as ReviewSet[];
            this.nodes = this.ReviewSets as singleNode[];
            for (let singleNode of this.nodes)
            {
                console.log(singleNode.name);
            }
        }, error => console.error(error));
        //this.nodes = this.ReviewSets;
    }
    nodes: singleNode[];
    //nodes = this.ReviewSets;
    //nodes = [
    //    {
    //        id: 1,
    //        name: 'root11',
    //        children: [
    //            { id: 2, name: 'child1' },
    //            { id: 3, name: 'child2' }
    //        ]
    //    },
    //    {
    //        id: 4,
    //        name: 'root2',
    //        children: [
    //            { id: 5, name: 'child2.1' },
    //            {
    //                id: 6,
    //                name: 'child2.2',
    //                children: [
    //                    { id: 7, name: 'subsub' }
    //                ]
    //            }
    //        ]
    //    }
    //];
    options = {};
    //ngOnInit() {
    //    //treeview = $.fn['treeview'] as Function;
    //    getTree();
    //    alert('building tree');
    //    ($('#tree')).treeview({ data: getTree() });
    //}
}
function getTree() {
    var tree = [
        {
            text: "Parent 1",
            nodes: [
                {
                    text: "Child 1",
                    nodes: [
                        {
                            text: "Grandchild 1"
                        },
                        {
                            text: "Grandchild 2"
                        }
                    ]
                },
                {
                    text: "Child 2"
                }
            ]
        },
        {
            text: "Parent 2"
        },
        {
            text: "Parent 3"
        },
        {
            text: "Parent 4"
        },
        {
            text: "Parent 5"
        }
    ];
    return tree;
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
