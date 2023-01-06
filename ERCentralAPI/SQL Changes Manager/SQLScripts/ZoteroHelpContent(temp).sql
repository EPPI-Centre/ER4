USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'ZoteroSetup'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('ZoteroSetup', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
<p>The <strong>Zotero</strong> pages allow to exchange references data with a Zotero <strong>Group Library</strong>.</p>

      <div class="container-fluid">
        <div class="row">
          <div class="col-12 col-sm-6"><b>Overview</b>
          <br>
          <img class="img w-100" src="assets/Images/zotero_schema.gif" ></div>

          <div class="col-12 col-sm-6">
            <p>Within EPPI-Reviewer, each <strong>Review</strong> can be paired with one and only one Zotero <strong>Group
            Library</strong>.
            <br>
            This pairing is done (starting from within EPPI-Reviewer) by asking Zotero to provide an Access <strong>Key</strong> to
            a Group Library. Whoever obtains the Key will be considered as the <strong>Key Owner</strong>.
            <br>
            Having a <strong>Key</strong>, it becomes possible to <strong>Push</strong> review items into the Group Library and/or
            to Pull <strong>Library References</strong> into the Review.
            <br>
            EPPI-Reviewer will keep track of the status of pulled/pushed references/items and allow to pull/push them again,
            depending on which version is &quot;newer&quot;.</p>

            <p><strong>Review Members</strong> and <strong>Group Members</strong> do not need to include the same people, however,
            the Key Owner needs to have access to both the Review and the Group Library.</p>
          </div>
        </div>

        <div class="row border-top mt-1 border-info">
          <p><strong>Setup:</strong>
          <br>
          To get started, you&#39;ll need to <strong>pair your review with a Zotero Group Library</strong>. The setup page contains
          in-line detailed instructions about the whole setup process.
          <br>
          Since some of the steps are to be carried on the <strong>Zotero pages</strong>, you can use the &quot; <strong>Show me
          (new tab)</strong>&quot; buttons to open the instructions in a separate page, and thus have them visible while setting up
          things on the Zotero end.</p>
        </div>

        <div class="row border-top mt-1 border-info">
          <p><strong>Maintenance:</strong>
          <br>
          Once an Access Key is obtained, and the pairing within Review and Group Library is established, this page shows
          <strong>basic information about the pairing/key</strong>.
          <br>
          Since the Access Key is owned and controlled by one (and only one) specific user, if the current user is not the Key
          Owner, this page will mention who the owner is, as only the Key Owner can manage or delete the key.
          <br>
          If the current user is the Key Owner, they will be offered the chance to Delete the Key, and/or do other changes if/when
          they are possible.
          <br>
          Deleting the key removes the pairing(/link) between Review and Group Library. This could be useful to re-purpose the
          Group Library to support a different review, and/or to change the Key Owner, so to side-step storage quota limitations.
          <br>
          The page will also contain a link to the Zotero &quot;storage quota&quot; <a href=
          "https://www.zotero.org/settings/storage" target="_blank">page</a>, useful to check if they are running out of
          &quot;space&quot; on the Zotero (cloud storage) side.</p>
        </div>

        <div class="row border-top mt-1 border-info">
          <p><strong>Troubleshooting:</strong>
          <br>
          EPPI-Reviewer has limited access to the Zotero end, and there are numerous things that users could do on the Zotero side,
          which could break or compromise the Review/Library pairing. For example, a user could <strong>Delete</strong> the paired
          Group Library, or even revoke/delete/edit permissions on the Access Key.</p>

          <div class="bg-light text-dark p-1 rounded border border-danger m-2">
            <strong class="text-warning">&#9888;</strong><strong> Please Note:</strong> the Zotero API used to exchange data is known to be <em>ever changing</em>. This
            (unfortunately) means that changes in the Zotero API might produce new, unforeseen problems <em>at any time</em>; if
            this happens, we would react and fix the problem at our end as quickly as possible. However, we cannot offer guarantees
            about how long it will take. As a consequence, by using these functionalities, <strong>you implicitly accept
            the</strong> (very small) <strong>risk of facing unforeseen interruptions in your workflow, which may last for days or
            more</strong>.<strong class="text-warning">&#9888;</strong>
          </div>

          <p>For a full discussion of troubleshooting procedures, please see <a href="https://eppi.ioe.ac.uk/cms/er4" target=
          "_blank">this page</a> in the EPPI-Reviewer gateway.</p>
        </div>
        <br>
      </div>
    </div>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'ZoteroSetup'

GO

USE [ReviewerAdmin]
GO

select * from TB_ONLINE_HELP
where CONTEXT = 'ZoteroSync'
if @@ROWCOUNT = 0
begin
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	VALUES ('ZoteroSync', 'tmp_place_holder')
end
GO


declare @Content nvarchar(max) = '
    <div class="container-fluid">
        <div class="row">
          <div class="bg-light text-dark p-1 rounded border border-danger m-2">
            <strong class="text-warning">&#9888;</strong> <strong>Please Note:</strong> the Zotero API used to exchange data is known to be <em>ever changing</em>. This
            (unfortunately) means that changes in the Zotero API might produce new, unforeseen problems <em>at any time</em>; if
            this happens, we would react and fix the problem at our end as quickly as possible. However, we cannot offer guarantees
            about how long it will take. As a consequence, by using these functionalities, <strong>you implicitly accept
            the</strong> (very small) <strong>risk of facing unforeseen interruptions in your workflow, which may last for days or
            more</strong>.<strong class="text-warning">&#9888;</strong>
          </div>
        </div>

        <div class="row">
          <div class="col-12 col-sm-6"><strong>Overview (Sync)</strong>
          <br>
          <img class="img w-100" src="assets/Images/zotero_schema.gif"></div>

          <div class="col-12 col-sm-6">
            <p>The current <strong>Review</strong> is paired with one Zotero <strong>Group Library</strong> via an Access
            <strong>Key</strong> owned by a Review Member.
            <br>
            You can <strong>Push</strong> review items into the Group Library and/or <strong>Pull Library References</strong> into
            the Review.</p>

            <div class="bg-light text-dark p-1 rounded border border-danger m-2">
              <strong class="text-warning">&#9888;</strong> Items pushed into the Zotero Group Library will contribute to the <a href=
              "https://www.zotero.org/settings/storage" target="_blank">Storage Quota</a> of the Access <strong>Key</strong> owner,
              and <strong>only</strong> to their quota.<strong class="text-warning">&#9888;</strong>
            </div>

            <p>You can list &quot;<strong>Items with this code</strong>&quot; to identify Items to push, while the list on the
            right always lists <strong>All References</strong> present on the Zotero Group Library.</p>

            <p>EPPI-Reviewer keeps track of the status of pulled/pushed references/items and allows to pull/push them again
            (updating the record on the destination), depending on which version is &quot;newer&quot;, &quot;can push/pull&quot;
            and &quot;up to date&quot; icons are shown for each Item and Reference.
            <br>
            <strong>Note:</strong> EPPI-Reviewer does <strong>not continuously monitor the status of Items/References</strong>;
            thus, in some cases, you may want to press the &quot;Refresh&quot; button, to ensure the &quot;can push/pull&quot; and
            &quot;up to date&quot; icons reflect the latest changes.</p>
          </div>
        </div>

        <div class=" border-top mt-1 border-info">
          <p><strong>Typical usage:</strong>
          <br>
          The functions available in this page have been designed <strong>explicitly</strong> to support <strong>two
          specific</strong> use-cases. EPPI-Reviewer will not try to stop using these functionalities in new/more/different ways,
          but you may encounter unforeseen obstacles, if you will try to do so.
          <br>
          The <strong>supported</strong> use cases are:</p>

          <ol>
            <li>Use Zotero to &quot;<strong>find the full-text documents</strong>&quot; after screening on Title and Abstract.</li>

            <li>Use Zotero to <strong>&quot;Cite While You Write&quot; (CWYW)</strong> to produce the final review report (and/or
            other reports).</li>
          </ol>

          <p>You can find a short description of both supported workflows below, followed by some troubleshooting hints.</p>
        </div>

        <div class="border-top mt-1 border-info">
          <p><strong>Using Zotero to &quot;find the full-text documents&quot;:</strong></p>

          <ol>
            <li>Start by making sure your Group Library contains no references. This is useful to ensure you won&#39;t risk
            importing unneeded references later on.</li>

            <li>In this (EPPI-Reviewer) page, list &quot;Items with this code&quot;, picking the code(s) that contain the included
            items after screening on title and abstract.</li>

            <li>Push all these items into the Group Library.</li>

            <li>In Zotero, use the built-in &quot;Find available PDF&quot; function for all references in the library.</li>

            <li>(Optional) Use other strategies to locate the full-text documents, and attach them to your references in
            Zotero.</li>

            <li>(Optional) You may also <em>edit</em> your references in Zotero, at this point. If you do so, your changes will be
            &quot;re-imported&quot; into EPPI-Reviewer at the next step.</li>

            <li>Visit this page again, and pull all references back into EPPI-Reviewer. This will <em>Update</em> the references
            you pushed in step 3, by adding to them all the PDFs you attached in steps 4 and 5, and updating the item records
            themselves, if step 6 applies.</li>
          </ol>
        </div>

        <div class="border-top mt-1 border-info">
          <p><strong>Using Zotero to &quot;Cite While You Write&quot; (CWYW):</strong></p>

          <ol>
            <li>Start by making sure your Group Library contains no references, or only the references you know are relevant to
            your report, but are not included in the review. This is useful to ensure you won&#39;t risk having irrelevant
            references in your library.</li>

            <li>In this (EPPI-Reviewer) page, list &quot;Items with this code&quot;, picking the code(s) that contain the included
            items after screening on <strong>Full Text</strong>.</li>

            <li>Push all these items into the Group Library.</li>

            <li>(Optional) You can &quot;list items with any code&quot; of course, in case you want to push and CWYW different sets
            of references. You can also choose to push individual items, by ticking the apposite checkboxes.</li>

            <li>(Optional) If the report will be written collaboratively, ensure your collaborators have access to the Group
            Library.</li>

            <li>(Optional) You can otherwise use the group library as a &quot;stepping stone&quot; and move/copy the references
            into whichever Zotero library (shared or personal) would suit your needs.</li>

            <li>You can now CWYW while having access to all Included/Relevant studies in your review.</li>
          </ol>
        </div>

        <div class="border-top mt-1 border-info">
          <p><strong>Troubleshooting:</strong>
          <br>
          The code that drives data exchanges between Zotero and EPPI-Reviewer is remarkably complex, mostly because of the need to
          &quot;translate&quot; references&#39; data from one &quot;language&quot; to the other, which is made complex because the
          two &quot;languages&quot; are indeed, very different.
          <br>
          For this reason, it is possible that problems (anticipated or not) will occur, and/or that the EPPI-Reviewer code
          harbours disruptive bugs. In general, we <em>anticipate</em> <strong>two distinct families</strong> of issues, when
          pulling/pushing references/items: <strong>errors preventing push or pull operations</strong> and
          <strong>&quot;translation&quot; errors</strong>, where data goes missing (or deteriorates) when pushing, pulling or
          cycling through both actions.</p>

          <p><strong>Explicit errors when pushing or pulling:</strong>
          <br>
          We made considerable efforts to ensure that, whenever possible, data exchanges do not &quot;fail in block&quot;: when
          trying to pull or push many references, ideally, a problem occurring with one specific reference <em>should not</em> foil
          the whole attempt. Concurrently, we also wrote the error-handling code so to report <em>individual</em> failures.
          <br>
          In practice, this means that, if a pull or push operation encountered an error, and if it was the kind of error that does
          not compromise the whole operation, EPPI-Reviewer will show an error mentioning the unique identifiers (Item/document ID
          and/or Zotero Key) of the records involved. Concurrently, the rest of the operation would have completed, thus resulting
          in the expected changes of &quot;can pull/push, up to date&quot; status for the references that could be pulled/pushed.
          <br>
          If this happens, please take a note of the unique identifiers mentioned in the error. Try again, and if all else fails,
          please contact EPPI-Support.
          <br>
          In some cases, the whole operation mail fail, and an &quot;overall&quot; message (which doesn&#39;t mention any unique
          identifiers) will be shown. Once again, if all &quot;retry&quot; attempts do fail, please contact EPPI-Support for
          assistance.</p>

          <p><strong>Data &quot;translation&quot; errors:</strong>
          <br>
          This kind of problem occurs when re-shaping data from the &quot;language&quot; of one system to the other is either
          inevitably &quot;lossy&quot; or is done in a less than ideal way. At the root, this class of errors happens because there
          never is a one-to-one perfect correspondence of data-fields between the two systems. For example, books and books
          sections do not have a DOI field in Zotero. In addition, the &quot;types&quot; of references (Journal article, book,
          conference proceeding, etc.) supported by the two systems also do not neatly match, complicating things further.
          <br>
          Thus, when designing these functionalities, we had to make hundreds of &quot;judgement calls&quot; and then run hundreds
          of different tests to check that our &quot;translation system&quot; works well enough.
          <br>
          As a result, it is possible and <em>predictable</em> that some of our judgement calls will not be ideal for all use-cases
          and that, given the size of the task, we might also have made some actual mistakes, in some place.
          <br>
          In practice, if you should notice that data goes missing (or otherwise degrades) upon pushing or pulling, you should
          contact EPPI-Support, preferably already mentioning exactly what doesn&#39;t work in detail. In an ideal world, you would
          send us a RIS file with one or more examples of references that fail to push/pull data across well enough.
          <br>
          It&#39;s also important to note here that, unlike the first &quot;family&quot; of problems, it is very unlikely, but not
          impossible, that &quot;translation problems&quot; will generate explicit error messages. Thus, you will need to actually
          inspect references on either system, and consciously <em>notice</em> the problem.</p>

          <p>For a full discussion of troubleshooting procedures, please see <a href="https://eppi.ioe.ac.uk/cms/er4" target=
          "_blank">this page</a> in the EPPI-Reviewer gateway.</p>
        </div>
        <br>
      </div>

'
UPDATE TB_ONLINE_HELP
SET [HELP_HTML] = @Content
	WHERE [CONTEXT] = 'ZoteroSync'

GO