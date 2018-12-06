import { Component, Inject, OnInit, EventEmitter, Output, OnDestroy, Input, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription, Subject, Subscribable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute } from '../services/ItemCoding.service';
import { ReviewSetsService, ItemAttributeSaveCommand, SetAttribute } from '../services/ReviewSets.service';
import { ReviewSetsComponent, CheckBoxClickedEventData } from '../reviewsets/reviewsets.component';
import { ReviewInfo, ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ArmsService } from '../services/arms.service';
import { armsComp } from '../arms/armsComp.component';


@Component({
   
    selector: 'itemcodingFull',
    templateUrl: './codingFull.component.html',
    //providers: [ReviewerIdentityService]
    providers: [],
    styles: [`
                button.disabled {
                    color:black; 
                    }
            `]

})
export class ItemCodingFullComp implements OnInit, OnDestroy {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService
        , private route: ActivatedRoute, private ItemCodingService: ItemCodingService,
        private ReviewSetsService: ReviewSetsService,
        private reviewInfoService: ReviewInfoService,
        public PriorityScreeningService: PriorityScreeningService
        , private ReviewerTermsService: ReviewerTermsService,
        public ItemDocsService: ItemDocsService,
        private armservice: ArmsService
    ) { }
   
    @ViewChild('ArmsCmp')
    private ArmsCompRef!: any;
    @ViewChild('ItemDetailsCmp')
    private ItemDetailsCompRef!: any;

    private subItemIDinPath: Subscription | null = null;
    private subCodingCheckBoxClickedEvent: Subscription | null = null;
    private ItemCodingServiceDataChanged: Subscription | null = null;
    private ItemArmsDataChanged: Subscription | null = null;
    public itemID: number = 0;
    private itemString: string = '0';
    public item?: Item;
    public itemId = new Subject<number>();
    
    private subGotScreeningItem: Subscription | null = null;
    public IsScreening: boolean = false;
    public ShowHighlights: boolean = false;
 

    public get HasTermList(): boolean {
        if (!this.ReviewerTermsService || !this.ReviewerTermsService.TermsList || !(this.ReviewerTermsService.TermsList.length > 0)) return false;
        else return true;
    }


    public CheckBoxAutoAdvanceVal: boolean = false;
    onSubmit(f: string) {
    }
    //@Output() criteriaChange = new EventEmitter();
    //public ListSubType: string = "";

    ngOnInit() {
        //console.log('init!');
        
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            this.ArmsCompRef.armChangedEE.subscribe(() => {
                if (this.armservice.SelectedArm) this.SetArmCoding(this.armservice.SelectedArm.itemArmId);
                else this.SetArmCoding(0);
            });
            this.subItemIDinPath = this.route.params.subscribe(params => {
                this.itemString = params['itemId'];
				this.GetItem();
				console.log('coding full sajdhfkjasfdh: ' + this.itemID);
            });
            this.ItemCodingServiceDataChanged = this.ItemCodingService.DataChanged.subscribe(

                () => {
                    if (this.ItemCodingService && this.ItemCodingService.ItemCodingList && this.ItemCodingService.ItemCodingList.length > 0) {
                        console.log('data changed event caught');
                        this.SetCoding();
                    }
                }
            );
            this.subCodingCheckBoxClickedEvent = this.ReviewSetsService.ItemCodingCheckBoxClickedEvent.subscribe((data: CheckBoxClickedEventData) => this.ItemAttributeSave(data));
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.subscribe((cmdErr: any) => this.HandleItemAttributeSaveCommandError(cmdErr));
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.subscribe((cmd: ItemAttributeSaveCommand) => this.HandleItemAttributeSaveCommandDone(cmd));
        }


    }
    
    
    private GetItem() {

        this.WipeHighlights();
        if (this.itemString == 'PriorityScreening') {
            if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
            this.IsScreening = true;
            this.PriorityScreeningService.NextItem();
        }
        else {
            this.itemID = +this.itemString;
            this.item = this.ItemListService.getItem(this.itemID);

            this.IsScreening = false;
            this.GetItemCoding();
            //this.ItemListService.eventChange(this.itemID);
            console.log('fill in arms here teseroo1');

		}
		//console.log('this is item: ' + this.item);
    }

    public HasPreviousScreening(): boolean{
        //console.log('CanMoveToPInScreening' + this.PriorityScreeningService.CurrentItemIndex);
        if (this.PriorityScreeningService.CurrentItemIndex > 0) return true;
        return false;
    }
    public CanMoveToNextInScreening(): boolean {
        
        let ItemSetIndex = this.ItemCodingService.ItemCodingList.findIndex(cset =>
                cset.setId == this.reviewInfoService.ReviewInfo.screeningCodeSetId
            && (cset.contactId == this.ReviewerIdentityServ.reviewerIdentity.userId || cset.isCompleted));
        if (ItemSetIndex == -1) return false;
        else return true;
    }
    public prevScreeningItem() {
        this.WipeHighlights();
        if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
        this.IsScreening = true;
        this.PriorityScreeningService.PreviousItem();
    }
    public GotScreeningItem() {
        //console.log('got Screening Item');
        this.item = this.PriorityScreeningService.CurrentItem;
        this.itemID = this.item.itemId;
        this.GetItemCoding();
    }
    private GetItemCoding() {
        //console.log('sdjghklsdjghfjklh ' + this.itemID);
        this.ItemDocsService.FetchDocList(this.itemID);
        if (this.item) {
            this.ArmsCompRef.CurrentItem = this.item;
            this.armservice.FetchArms(this.item);
        }
        this.ItemCodingService.Fetch(this.itemID);    

    }
    SetCoding() {
        console.log('change something');
        if (this.ItemCodingService.ItemCodingList.length == 0) {
            this.ReviewSetsService.clearItemData();
            console.log('change: clearonly');
            return;
        }
        this.SetHighlights();
        this.ReviewSetsService.clearItemData();
        if (this.armservice.SelectedArm) this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, this.armservice.SelectedArm.itemArmId);
        else this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, 0);
    }
    SetArmCoding(armId: number) {
        console.log('change Arm');
        this.ReviewSetsService.clearItemData();
        this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, armId);
    }
    
    
    
    
    private _hasPrevious: boolean | null = null;
    hasPrevious(): boolean {
        
        if (!this.item || !this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.items || this.ItemListService.ItemList.items.length < 1) {
            //console.log('NO!');
            return false;
        }
        else if (this._hasPrevious === null) {
            this._hasPrevious = this.ItemListService.hasPrevious(this.item.itemId);
        }
        //console.log('has prev? ' + this._hasPrevious);
        return this._hasPrevious;
    }
    MyHumanIndex():number {
        return this.ItemListService.ItemList.items.findIndex(found => found.itemId == this.itemID) + 1;
    }
    private _hasNext: boolean | null = null;
    hasNext(): boolean {
        if (!this.item || !this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.items || this.ItemListService.ItemList.items.length < 1) {
            return false;
        }
        else if (this._hasNext === null)
        {
            this._hasNext = this.ItemListService.hasNext(this.item.itemId);
        }
        return this._hasNext;
    }
    firstItem() {
        this.WipeHighlights();
        
        if (this.item) this.goToItem(this.ItemListService.getFirst());

    }
    prevItem() {
        
        this.WipeHighlights();
        if (this.item) {
            console.log('inside previous coding item component part' + this.item.itemId);
   
            this.goToItem(this.ItemListService.getPrevious(this.item.itemId));
        }
    }
    nextItem() {
        
        this.WipeHighlights();
        if (this.item) {
            this.goToItem(this.ItemListService.getNext(this.item.itemId));
            console.log('inside next coding item component part' + this.item.itemId);
            //this.valueChange.next(this.item.itemId);
        }
    }
    lastItem() {
        this.WipeHighlights();
        if (this.item) this.goToItem(this.ItemListService.getLast());
    }
    clearItemData() {
        this._hasNext = null;
        this._hasPrevious = null;
        this.item = undefined;
        this.itemID = -1;
        this.ItemCodingService.ItemCodingList = [];
        if (this.ReviewSetsService) {
            this.ReviewSetsService.clearItemData();
        }
    }
    goToItem(item: Item) {
        this.WipeHighlights();
        this.clearItemData();
        console.log('what do you need me to do?' + item.itemId);
        this.router.navigate(['itemcoding', item.itemId]);
        this.item = item;
        if (this.item.itemId != this.itemID) {

            this.itemID = this.item.itemId;
        }
        //this.GetItemCoding();
    }
    BackToMain() {
        this.clearItemData();
        this.router.navigate(['mainFullReview']);
    }
    ItemAttributeSave(data: CheckBoxClickedEventData) {
        
        let SubSuccess: Subscription;
        let SubError: Subscription;//see https://medium.com/thecodecampus-knowledge/the-easiest-way-to-unsubscribe-from-observables-in-angular-5abde80a5ae3
        //console.log(data.AttId);
        let attribute: SetAttribute | null = this.ReviewSetsService.FindAttributeById(data.AttId);
        
        if (!attribute) {
            //problem: we don't know what to code!
            alert('Could not find the code we want to add to this item, please reload: something is not right with the data.');
            return;
        }

        let itemSet: ItemSet | null = this.ItemCodingService.FindItemSetBySetId(attribute.set_id);

        let cmd: ItemAttributeSaveCommand = new ItemAttributeSaveCommand();
        if (itemSet) {
            //we have an item set to use, so put the relevant data in cmd. Otherwise, default value is fine.
            cmd.itemSetId = itemSet.itemSetId;
        }
        cmd.itemId = this.itemID;
        cmd.additionalText = data.additionalText;
        cmd.itemArmId = data.armId;
        cmd.setId = attribute.set_id;
        //console.log(attribute.set_id);
        cmd.attributeId = data.AttId;
        cmd.revInfo = this.reviewInfoService.ReviewInfo;
        let itemAtt: ReadOnlyItemAttribute | null = null;
        if ((data.event.target && data.event.target.checked) || data.event == 'InfoboxTextAdded') {
            //add new code to item
            //console.log('cmd.saveType = "Insert"');
            cmd.saveType = "Insert";
        }
        else if (data.event == 'InfoboxTextUpdate' && itemSet) {
            //console.log('cmd.saveType = "Update"');
            cmd.saveType = "Update";
            for (let Candidate of itemSet.itemAttributesList) {
                if (Candidate.attributeId == cmd.attributeId) {
                    itemAtt = Candidate;
                    break;
                }
            }
            if (!itemAtt) {
                //problem: we can't remove an item att, if we can't find it!
                alert('Sorry: can\'t find the Attribute to update...');
                return;
            }
            cmd.itemAttributeId = itemAtt.itemAttributeId;
        }
        else if (!data.event.target.checked && itemSet) {
            //we delete the coding
            cmd.saveType = "Delete"; 
            
            for (let Candidate of itemSet.itemAttributesList) {
                if (Candidate.attributeId == cmd.attributeId) {
                    itemAtt = Candidate;
                    break;
                }
            }
            if (!itemAtt) {
                //problem: we can't remove an item att, if we can't find it!
                alert('Sorry: can\'t find the Attribute to remove...');
                return;
            }
            cmd.itemAttributeId = itemAtt.itemAttributeId;
        }
        SubError = this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.subscribe((cmdErr: any) => {
            this.ReviewSetsService.ItemCodingItemAttributeSaveCommandHandled();
            //do something if command ended with an error
            //console.log('Error handling');
            alert("Sorry, an ERROR occurred when saving your data. It's advisable to reload the page and verify that your latest change was saved.");
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.unsubscribe();
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.unsubscribe();
        });
        SubSuccess = this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.subscribe((cmdResult: ItemAttributeSaveCommand) => {
            //update local version of the coding...
            
            if (cmd.saveType == "Insert" || cmd.saveType == "Update") {
                let newItemA: ReadOnlyItemAttribute = new ReadOnlyItemAttribute();
                newItemA.additionalText = cmdResult.additionalText;
                newItemA.armId = cmdResult.itemArmId;
                newItemA.armTitle = "";
                newItemA.attributeId = cmdResult.attributeId;
                newItemA.itemAttributeId = cmdResult.itemAttributeId;
                if (itemSet) itemSet.itemAttributesList.push(newItemA);
                else {//didn't have the itemSet, so need to create it...
                    let newItemSet: ItemSet = new ItemSet();
                    newItemSet.contactId = this.ReviewerIdentityServ.reviewerIdentity.userId;
                    newItemSet.contactName = this.ReviewerIdentityServ.reviewerIdentity.name; 
                    let setDest = this.ReviewSetsService.FindSetById(cmd.setId);
                    if (setDest) {
                        newItemSet.isCompleted = setDest.codingIsFinal;
                        newItemSet.setName = setDest.set_name;
                    }
                    newItemSet.isLocked = false;
                    newItemSet.itemId = cmdResult.itemId;
                    newItemSet.itemSetId = cmdResult.itemSetId;
                    newItemSet.setId = cmdResult.setId;
                    newItemSet.itemAttributesList.push(newItemA);
                    this.ItemCodingService.ItemCodingList.push(newItemSet);
                }
            }
            else if (cmd.saveType == "Delete") {
                
                //if (itemSet) console.log(itemSet.itemAttributesList.length);
                //if (itemAtt) console.log(itemAtt.attributeId);
                if (itemSet && itemAtt) {
                    //remove the itemAttribute from itemSet
                    itemSet.itemAttributesList = itemSet.itemAttributesList.filter(obj => obj !== itemAtt);
                    if (itemSet.itemAttributesList.length == 0) {
                        //if itemset does not have item attributes, remove the itemset
                        this.ItemCodingService.ItemCodingList = this.ItemCodingService.ItemCodingList.filter(obj => itemSet && obj.itemSetId !== itemSet.itemSetId);
                    }
                    //if (itemSet) console.log(itemSet.itemAttributesList.length);
                }
            }
            
            this.SetCoding();
            console.log('set dest');
            SubSuccess.unsubscribe();
            SubError.unsubscribe();
            this.ReviewSetsService.ItemCodingItemAttributeSaveCommandHandled();
            if (cmd.saveType == "Insert" && this.CheckBoxAutoAdvanceVal) {
                //auto advance is on, we want to go to the next item
                if (!this.IsScreening && this.hasNext()) this.nextItem();
                else if (this.IsScreening) this.GetItem();//in screening mode, this uses the screening service to receive the next item
            }
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandError.unsubscribe();
            //this.ReviewSetsService.ItemCodingItemAttributeSaveCommandExecuted.unsubscribe();
        });
        //console.log("canwrite:" + this.ReviewSetsService.CanWrite);
        this.ReviewSetsService.ExecuteItemAttributeSaveCommand(cmd, this.ItemCodingService.ItemCodingList);
    }
    ItemChanged() {
        this.WipeHighlights();
        this.SetHighlights();
    }
    ngOnDestroy() {
        //console.log('killing coding comp');
        if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();
        if (this.ItemCodingServiceDataChanged) this.ItemCodingServiceDataChanged.unsubscribe();
        if (this.subCodingCheckBoxClickedEvent) this.subCodingCheckBoxClickedEvent.unsubscribe();
        if (this.subGotScreeningItem) this.subGotScreeningItem.unsubscribe();
    }
    WipeHighlights() {
        if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.WipeHighlights();
    }
    SetHighlights() {
        if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.SetHighlights();
    }
    ShowHighlightsClicked() {
        if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.ShowHighlightsClicked(); else console.log('Ouch');
    }
}






