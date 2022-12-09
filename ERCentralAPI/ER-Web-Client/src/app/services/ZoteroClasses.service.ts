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
    groupBeingSynced: boolean = false;
}

export interface ApiKeyInfo {
  zoteroConnectionId: number;
  libraryId: string;
  rEVIEW_ID: number;
  erUserId: number;
  zoteroUserId: number;
  status: string;
}

export class ZoteroItem {
  constructor(izjo: iZoteroJobject) {
    const t = izjo.data;
    this.key = t.key;
    this.title = t.title;
    this.shortTitle = t.shortTitle;
    this.parentTitle = t.publicationTitle;
    this.dateModified = t.dateModified;
    this.itemType = t.itemType;
    this.version = t.version;
  }
  public ToZoteroERWebReviewItem(): ZoteroERWebReviewItem {
    let res = new ZoteroERWebReviewItem(null, this);
    return res;
  }
  public get HasAttachments(): boolean {
    if (this.attachments.length == 0) return false;
    else return true;
  }
  public get HasAttachmentsToPull(): boolean {
    const ind = this.attachments.findIndex(f => f.syncState == SyncState.canPull);
    if (ind == -1) return false;
    else return true;
  }
  public FindAttachmentByZoteroKey(key: string): ZoteroAttachment | null {
    const ind = this.attachments.findIndex(f => f.key == key);
    if (ind == -1) return null;
    else return this.attachments[ind];
  }
  key: string = "";
  title: string = "";
  shortTitle: string = "";
  parentTitle: string = "";
  dateModified: string = "";
  itemType: string = "";
  itemId: number = 0;
  attachments: ZoteroAttachment[] = [];
  version: number = 0;
  syncState: SyncState = SyncState.notSet;
  ClientSelected: boolean = false;
}
export class ZoteroAttachment {
  constructor(izjo: iZoteroJobject) {
    this.key = izjo.data.key;
    this.filename = izjo.data.title;
    this.dateModified = izjo.data.dateModified;
  }
  key: string = "";
  filename: string = "";
  dateModified: string = "";
  syncState: SyncState = SyncState.notSet;
}

export interface iZoteroItemsResult {
  zoteroItems: iZoteroJobject[];
  pairedItems: iZoteroERWebReviewItem[];
}
export interface iZoteroJobject {
  key: string;
  version: number;
  library: iZoteroLibrary;
  links: iZoteroLinks;
  meta: iZoteroMeta;
  data: iCollectionData;
}

export interface iZoteroLibrary {
  type: string;
  id: number;
  name: string;
  links: iZoteroLinks;
}

export interface iZoteroMeta {
  created: string;
  lastModified: string;
  numItems: number;
  numChildren: number;
}

export interface iZoteroLinks {
  alternate: iZoteroTypeRefPair;
  self: iZoteroTypeRefPair;
  attachment: iZoteroAttachment;
}
export interface iZoteroTypeRefPair {
  href: string;
  type: string;
}
export interface iZoteroAttachment extends iZoteroTypeRefPair {
  attachmentSize: number;
  attachmentType: string;
}

export interface iCollectionData {

  key: string;
  version: number;
  itemType: string;
  title: string;
  creators: any[];
  abstractNote: string;
  series: string;
  seriesNumber: string;
  volume: string;
  date: string;
  language: string;
  shortTitle: string;
  publicationTitle: string;
  url: string;
  accessDate: string;
  archive: string;
  archiveLocation: string;
  libraryCatalog: string;
  callNumber: string;
  rights: string;
  extra: string;
  tags: any;
  collections: any[];
  relations: any;
  dateAdded: string;
  dateModified: string;
  parentItem: string;
}


export interface iZoteroERWebReviewItem {
  itemID: number;
  iteM_REVIEW_ID: number;
  itemKey: string;
  lasT_MODIFIED: string;
  pdfList: iZoteroERWebItemDoc[];
  shortTitle: string;
  syncState: SyncState;
  title: string;
  version: number;
}
export interface iZoteroERWebItemDoc {
  docZoteroKey: string;
  documenT_TITLE: string;
  itemDocumentId: number;
  syncState: SyncState;
}

export enum SyncState {
  notSet,
  upToDate,
  canPush,
  canPull
}

export class ZoteroERWebReviewItem {
  constructor(data: iZoteroERWebReviewItem | null , data2: ZoteroItem | null = null) {
    if (data != null) {
      this.itemID = data.itemID;
      this.iteM_REVIEW_ID = data.iteM_REVIEW_ID;
      this.itemKey = data.itemKey;
      this.lasT_MODIFIED = data.lasT_MODIFIED;
      this.shortTitle = data.shortTitle;
      this.syncState = data.syncState;
      this.title = data.title;
      this.version = data.version;
      this.pdfList = [];
      for (let ip of data.pdfList) {
        let pdf = new ZoteroERWebItemDoc(ip);
        this.pdfList.push(pdf);
      }
    } else if (data2 != null) {
      this.itemID = data2.itemId;
      this.iteM_REVIEW_ID = -1;
      this.itemKey = data2.key;
      this.lasT_MODIFIED = data2.dateModified;
      this.shortTitle = data2.shortTitle;
      this.syncState = data2.syncState;
      this.title = data2.title;
      this.version = -1;
      this.pdfList = [];
      for (let ip of data2.attachments) {
        let pdf = new ZoteroERWebItemDoc(null, ip);
        this.pdfList.push(pdf);
      }
    } else {
      this.itemID = -1;
      this.iteM_REVIEW_ID = -1;
      this.itemKey = "";
      this.lasT_MODIFIED = "";
      this.shortTitle = "";
      this.syncState = SyncState.notSet;
      this.title = "";
      this.version = -1;
      this.pdfList = [];
    }
  }
  public get HasPdf(): boolean {
    if (this.pdfList.length == 0) return false;
    else return true;
  }
  public get HasPdfToPush(): boolean {
    const ind = this.pdfList.findIndex(f => f.syncState == SyncState.canPush);
    if (ind == -1) return false;
    else return true;
  }
  itemID: number;
  iteM_REVIEW_ID: number;
  itemKey: string;
  lasT_MODIFIED: string;
  pdfList: ZoteroERWebItemDoc[];
  shortTitle: string;
  version: number;
  syncState: SyncState;
  title: string;
  ClientSelected: boolean = false;
}

export class ZoteroERWebItemDoc {
  constructor(data: iZoteroERWebItemDoc | null, data2: ZoteroAttachment | null = null) {
    if (data != null) {
      this.documenT_TITLE = data.documenT_TITLE;
      this.docZoteroKey = data.docZoteroKey;
      this.itemDocumentId = data.itemDocumentId;
      this.syncState = data.syncState;
    } else if (data2 != null) {
      this.documenT_TITLE = data2.filename;
      this.docZoteroKey = data2.key;
      this.itemDocumentId = -1;
      this.syncState = data2.syncState;
    } else {
      this.documenT_TITLE = "";
      this.docZoteroKey = "";
      this.itemDocumentId = -1;
      this.syncState = SyncState.notSet;
    }
  }
  docZoteroKey: string;
  documenT_TITLE: string;
  itemDocumentId: number;
  syncState: SyncState;
}
