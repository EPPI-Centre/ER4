# Copy Review Data Readme
This console app (.Net8 console App) is a slow-growing collection of utilities that can facilitate copying data across (and perhaps within?) reviews.

"Coding data" in EPPI Reviewer is hard to Copy because it is hierarchical and interconnected: for example, one cannot add an outcome measure record without first having outcome codes in a coding tool and without having the relevant item coded with some code in the same coding tool. But the same outcome measure might also require the existence of arms and timepoints for the same item.

The same applies to PDF coding, whereby it can't be added without having the relative coding tool, item with its PDF document uploaded. 

Overall, each "kind of record" that needs to be copied depends on a number of prerequitsites that all need to be fulfilled in the correct order, making the work of copying data a complicated task.

The interconnectedness and dependency network is vast and constantly changing, making the task of copying ALL coding data for a given set of items and coding tools a complicated affair. And one that cannot be automated "once and for all", as new functionalities may change or extend what needs copying and/or the dependency relations.

Finally, only two "core entities" have records that contain data that can be used to track what is a "copy" of what. These two are Items(references) which have the `OLD_ITEM_ID` field, and nodes in coding tools (including the coding tool itself), which have the `ORIGINAL_SET_ID` and `ORIGINAL_ATTRIBUTE_ID` fields.

All of these fields can be used to trace "is a copy of" relations and thus ultimately "match" entities in two reviews. However, even this task can be and usually is complicated, as it often involves chains, where a given coding tool has been copied from ReviewA to ReviewB, and then to ReviewC, thus if one needs to copy coding data from ReviewA to ReviewC, to find out which code corresponds to what, the route including ReviewB needs to be discovered and used.

Items can have the same "complication", but will more often be affected also by deduplication whereby the "master item" (representing a group) isn't the one that has the `old_item_id` value that points to the source item in the "copying from" review.

For these reasons, requests to copy data are currently handled on a per-request basis and require a lot of manual work, both for "mapping" 1:1 relations between already existing records in source and destination, then to create new records in the correct order (due to dependencies) and finally to correctly match new records (the copies) with existing ones, for the purpose of identifying what-depends-on-what correctly.

In this context, rather than attempting to write a one-stop solution to the problem of data-copying, it seems more reasonable to write "snippets" of useful functionalities, which can automate one or the other mini-task. In time the collection might grow and approach "completeness", and or might link together multiple snippets automating multiple sub-steps in one go.

## Implemented functionalities

As of Oct 2024, this utility contains only one functionality, the one used to match Codes (records in `TB_ATTRIBUTE` and `TB_ATTRIBUTE_SET`) between two reviews. This assumes that one or more coding tools have been "Imported" from Review1, to Review2, [...] to ReviewN and that the task is to copy data from Review1 to ReviewN. This assumes a linear "ancestry" relation from source (Review1) to destination (ReviewN) and has no restriction on how many steps the chain of relations involves; the case where coding tools in both source and destination derive from the same "template" ancestor is not yet supported.

## Development approach
The plan is to extend this app "as and when": for each "copy data" task, the idea is to write one more function that will facilitate the task at hand. Given enough "copy data" tasks, this utility might grow and become a "one stop facility" for some instances.

Otherwise, if the copy data tasks will dry up, not too much effort will ever be spent on this app, reducing waste.

Given all of this features in this utility should be kept "atomic" with sharp distinctions of tasks, making each distinct task easier to maintain. Focus should thus be on maintainability rather than on the efficiency/speed of the code.