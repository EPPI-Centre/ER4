import { Injectable } from '@angular/core';


@Injectable({
    providedIn: 'root',
})

export class ZoteroClasses  {
    constructor(
    ) {
    }
}


export class GroupSelf {
    href: string = '';
    type: string = '';
}

export class GroupAlternate {
    href: string = '';
    type: string = '';
}

export class GroupLinks {
    self: GroupSelf = new GroupSelf();
    alternate: GroupAlternate = new GroupAlternate();
}

export class GroupMeta {
    created: string = '';
    lastModified: string = '';
    numItems: number = 0;
}

export class GroupData {
    id: number = 0;
    version: number = 0;
    name: string = '';
    owner: number = 0;
    type: string = '';
    description: string = '';
    url: string = '';
    libraryEditing: string = '';
    libraryReading: string = '';
    fileEditing: string = '';
    groupBeingSynced: boolean = false;
}

export class Group {
    id: number = 0;
    version: number = 0;
    links: GroupLinks = new GroupLinks();
    meta: GroupMeta = new GroupMeta();
    data: GroupData = new GroupData();
    groupBeingSynced: number = 0;
}


export interface User {
    library: boolean;
    files: boolean;
    notes: boolean;
    write: boolean;
}

export interface AllPermissions {
    library: boolean;
    write: boolean;
}

export interface PerGroupPermissions {
    [key: string]: AllPermissions
}


export interface Groups {
    all: AllPermissions | any;
}


export interface Access {
    user: User;
    groups: Groups| PerGroupPermissions;
}

export interface UserKey {
    key: string;
    userID: number;
    username: string;
    access: Access;
}

export interface UserSubscription {



}


export interface IERWebObjects {
    itemID: number;
    itemReviewID: number;
    itemDocumentID: number;
}


export interface IERWebZoteroObjects {
    itemID: number;
    itemReviewID: number;
    itemDocumentID: number;
    itemkey: string;
    version: string;
}

export interface IZoteroReviewItem {
    zotero_item_review_ID: number;
    itemKey: string;
    libraryID: string;
    iteM_REVIEW_ID: number;
    version: string;
    lAST_MODIFIED: string;
}

export interface IERWebANDZoteroReviewItem {
    zotero_item_review_ID: number;
    itemKey: string;
    libraryID: string;
    iteM_REVIEW_ID: number;
    version: string;
    lAST_MODIFIED: string;
    itemID: number;
    shortTitle: string;
    typeName: string;
    title: string;
}

export interface IUserGroup {
    items: GroupItem[];
}

export class GroupItem {
    groupID: string = '';
    version: string = '';
    sync: string = '';
}

// need to look at a generic collection to capture any type of item
export class TypeCollection {
    key: string = '';
    version: number = 0;
    library: any;
    links: any;
    meta: any;
    data: CollectionData = new CollectionData();
    //syncState: boolean = false;
}

export class Book {
    key: string = '';
    version: number = 0;
    itemType: string = '';
    title: string = '';
    creators: CreatorsItem[] = [];
    abstractNote: string = '';
    series: string = '';
    seriesNumber: string = '';
    volume: string = '';
    numberOfVolumes: string = '';
    edition: string = '';
    place: string = '';
    publisher: string = '';
    date: string = '';
    numPages: string = '';
    language: string = '';
    ISBN: string = '';
    shortTitle: string = '';
    url: string = '';
    accessDate: string = '';
    archive: string = '';
    archiveLocation: string = '';
    libraryCatalog: string = '';
    callNumber: string = '';
    rights: string = '';
    extra: string = '';
    tags: string[] = [];
    collections: string[] = [];
    relations: any;
    dateAdded: string = '';
    dateModified: string = '';
}

export class BookRecieved {
    key: string = '';
    version: number = 0;
    itemType: string = '';
    title: string = '';
    creators: any[] = [];
    abstractNote: string = '';
    series: string = '';
    seriesNumber: string = '';
    volume: string = '';
    numberOfVolumes: string = '';
    edition: string = '';
    place: string = '';
    publisher: string = '';
    date: string = '';
    numPages: string = '';
    language: string = '';
    ISBN: string = '';
    shortTitle: string = '';
    url: string = '';
    accessDate: string = '';
    archive: string = '';
    archiveLocation: string = '';
    libraryCatalog: string = '';
    callNumber: string = '';
    rights: string = '';
    extra: string = '';
    tags: any[] = [];
    collections: any[] = [];
    relations: any;
    dateAdded: string = '';
    dateModified: string = ''; //31
}

export class CreatorsItem {
    creatorType: string = '';
    firstName: string = '';
    lastName: string = '';
}

export class Collection {
    key: string = '';
    version: number = 0;
    library: any; //= new Library;
    links: Links = new Links();
    meta: Meta = new Meta(); // TODO check
    data: CollectionData = new CollectionData();
}

//TODO change meta to the below.
export class Meta {
    created: string = '';
    lastModified: string = '';
    numItems: number = 0;
    numChildren: number = 0;
}

//createdByUser: Object { id: 8317548, username: "pat1odriscoll", name: "", … }
//creatorSummary: "jghjgfhj"
//numChildren: 1
//    <prototype>: Object { … }
//​
//version: 468

export class CollectionData {

    key: string = '';
    version: number = 0;
    itemType: string = '';
    title: string = '';
    creators: any[] = [];
    abstractNote: string = '';
    series: string = '';
    seriesNumber: string = '';
    volume: string = '';
    date: string = '';
    language: string = '';
    shortTitle: string = '';
    url: string = '';
    accessDate: string = '';
    archive: string = '';
    archiveLocation: string = '';
    libraryCatalog: string = '';
    callNumber: string = '';
    rights: string = '';
    extra: string = '';
    tags: any;
    collections: any[] = [];
    relations: any;
    dateAdded: string = '';
    dateModified: string = '';
    parentItem: string = '';
}

export class Library {
    type: string = '';
    id: number = 0;
    name: string = '';
    links: Links = new Links();
}

export class Links {
    alternate: Alternate = new Alternate();
    self: Self = new Self();
    attachment: Attachment = new Attachment();
}

export class Alternate {
    href: string = '';
    type: string = '';
}

export class Self {
    href: string = '';
    type: string = '';
}

export class Up {
    href: string = '';
    type: string = '';
}

export class Attachment {
    href: string = '';
    type: string = '';
    attachmentSize: number = 0;
    attachmentType: string = '';
}

export class ZoteroReviewCollection {
    collectionKey: string = '';
    libraryID: string = '';
    apiKey: string = '';
    revieW_ID: string = '';
    userId: string = '';
    parentCollection: string = '';
    collectionName: string = '';
    version: string = '';
    dateCreated: string = '';
    groupBeingSynced: boolean = false;
}

export class ZoteroReviewCollectionList {

    ZoteroReviewCollectionList: ZoteroReviewCollection[] = [];
}

export enum SyncState {
    doesNotExist,
    behind,
    upToDate,
    ahead,
    attachmentDoesNotExist
}

export interface IObjectSyncState {
    objectKey: string;
    syncState: SyncState;
}

export interface IObjectsInERWebNotInZotero {
    itemId: number;
    shortTitle: string;
    itemReviewId: number;
    typeName: string;
    documentAttached: boolean;
}
