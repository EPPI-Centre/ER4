using System;
using System.Collections.Generic;
using System.Windows.Media;
using Telerik.Windows.Documents.Fixed.Model;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Fixed.Text;
using Telerik.Windows.Documents.Fixed.Selection;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows;
using BusinessLibrary.BusinessClasses;


namespace EppiReviewer4
{
    public class Highlights : Dictionary<RadFixedPage, PathGeometry>
    { //index of pages, each with it's own PageSelections
    }

    public class NotesCs : Dictionary<RadFixedPage, NotesColumn>
    {//index of pages, each with it's column of annotations.
    }
    public class PDFCodingHelper
    {
        
        public static void AddSel(PathGeometry pg, int start, int end, string Txt, RadFixedPage pdfPage, RadPdfViewer pdfV, ref ItemAttributePDF iaPDF)
        {

            //find intersections with existing selections in page
            Dictionary<int, string> interSects = EvaluateIntersects(start, end, ref iaPDF);

            if (interSects.Count == 1)
            {
                foreach (KeyValuePair<int, string> el in interSects)
                {//selected text is all part of existing selections -> do nothing
                    if (el.Value == "contains")
                    {
                        pdfV.Document.Selection.Clear(); //remove selection so that the highlight shows immediately
                        return;
                    }
                }
            }
            int MinS = start, MaxE = end;
            bool done = false;
            //no intersections
            if (interSects.Count == 0)
            {//recalculate geometry, set shapeTxt, add to _list (remove '¬' from selectedTxt)
                selectOnPage(pdfV, pdfPage, start, end);
                pg = pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                //while (pg.Figures.Count > 0)
                //{
                //    PathFigure pf = pg.Figures[0];
                //    pg.Figures.RemoveAt(0);
                //    _Shape.Figures.Add(pf);
                //}
                //_ShapeTxt = _Shape.ToString();
                //if (_ShapeTxt.Length >= 2 && _ShapeTxt.Substring(0, 2) != "F1")
                //{
                //    _ShapeTxt = "F1" + _ShapeTxt;
                //    _Shape = makeGeom(_ShapeTxt);
                //}
                iaPDF.inPageSelections.Add(new InPageSelections(start, end, Txt));
                //if (_Page == null) _Page = pdfPage.PageNo;
                //isDirty = true; _newShape = true;
                done = true;
            }
            else //there are intersections
            {// selected text touches (but is not entirely contained) in one or more existing selections (ExSs)->

                foreach (KeyValuePair<int, string> el in interSects)
                { //find minStart and maxEnd
                    InPageSelections elSel = iaPDF.inPageSelections[el.Key];

                    if (elSel.Start < MinS) MinS = elSel.Start;
                    if (elSel.End > MaxE) MaxE = elSel.End;
                }
                List<int> lint = new List<int>();
                lint.AddRange(interSects.Keys);
                for (int i = lint.Count - 1; i >= 0; i--)
                { //remove stale selections object el in interSects.Keys

                    InPageSelections elSel = iaPDF.inPageSelections[lint[i]];
                    iaPDF.inPageSelections.Remove(elSel);//remove old sel
                }
            }
            iaPDF.Shape = new PathGeometry { FillRule = FillRule.Nonzero };
            //iaPDF.ShapeTxt = "";
            for (int i = 0; i < iaPDF.inPageSelections.Count; i++)
            {
                InPageSelections ppas = iaPDF.inPageSelections[i];
                if (!done && ppas.Start > MinS && ppas.End > MaxE) //add new selection
                {
                    selectOnPage(pdfV, pdfPage, MinS, MaxE);
                    pg = pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                    while (pg.Figures.Count > 0)
                    {
                        PathFigure pf = pg.Figures[0];
                        pg.Figures.RemoveAt(0);
                        iaPDF.Shape.Figures.Add(pf);
                    }
                    iaPDF.inPageSelections.Add(new InPageSelections(MinS, MaxE, pdfV.Document.Selection.GetSelectedText().Replace('¬', '?')));
                    iaPDF.inPageSelections.Sort();
                    i++;
                    done = true;
                }
                selectOnPage(pdfV, pdfPage, ppas.Start, ppas.End);
                pg = pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                while (pg.Figures.Count > 0)
                {
                    PathFigure pf = pg.Figures[0];
                    pg.Figures.RemoveAt(0);
                    iaPDF.Shape.Figures.Add(pf);
                }
            }
            if (!done)
            {// the new sel was not added (is after all remaining ones) add it now
                selectOnPage(pdfV, pdfPage, MinS, MaxE);
                pg = pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                while (pg.Figures.Count > 0)
                {
                    PathFigure pf = pg.Figures[0];
                    pg.Figures.RemoveAt(0);
                    iaPDF.Shape.Figures.Add(pf);
                }
                iaPDF.inPageSelections.Add(new InPageSelections(MinS, MaxE, pdfV.Document.Selection.GetSelectedText().Replace('¬', '?')));
            }
            if (iaPDF.inPageSelections.Count == 0) //we didn't add anything on the previous for cycle because nothing was there to cycle through
            {//add the only new selection
                pdfV.Document.Selection.Clear();
                pg = pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                selectOnPage(pdfV, pdfPage, MinS, MaxE);
                pg = pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                while (pg.Figures.Count > 0)
                {
                    PathFigure pf = pg.Figures[0];
                    pg.Figures.RemoveAt(0);
                    iaPDF.Shape.Figures.Add(pf);
                }
                iaPDF.inPageSelections.Add(new InPageSelections(MinS, MaxE, pdfV.Document.Selection.GetSelectedText().Replace('¬', '?')));
            }

            iaPDF.ShapeTxt = iaPDF.Shape.ToString();
            if (iaPDF.ShapeTxt.Length >= 2 && iaPDF.ShapeTxt.Substring(0, 2) != "F1")
            {
                iaPDF.ShapeTxt = "F1" + iaPDF.ShapeTxt;
                iaPDF.Shape = ItemAttributePDF.makeGeom(iaPDF.ShapeTxt);
            }
            //selectOnPage(pdfV, pdfPage, MinS, MaxE); //reinstate original selection
            iaPDF.inPageSelections.Sort();
            
            //isDirty = true; _newShape = true;

            if (iaPDF.inPageSelections.Count == 0)
            {
                iaPDF.Delete();
            }

            pdfV.Document.Selection.Clear(); //remove selection so that the highlight shows immediately
            return;

        }
        public static void RemoveSel(PathGeometry pg, int start, int end, string Txt, RadFixedPage pdfPage, RadPdfViewer pdfV, ref ItemAttributePDF iaPDF)
        {
            //find intersections with existing selections in page
            Dictionary<int, string> interSects = EvaluateIntersects(start, end, ref iaPDF);
            //no intersections -> do nothing
            if (interSects.Count == 0) return;

            //there are intersections (ExSs)
            List<InPageSelections> toDel = new List<InPageSelections>();
            List<InPageSelections> toAdd = new List<InPageSelections>();
            //for each ExSs, populate 2 lists: sel to be deleted, sel to be added, the first contains sels that need to be deleted, and sels that have been modified
            //the second contains brand new sels and new version of modified sels.
            foreach (KeyValuePair<int, string> el in interSects)
            {
                toDel.Add(iaPDF.inPageSelections[el.Key]); //in all cases, current sel has to be renovated.
                //if (el.Value == "contained") //if ExS is contained in current sel, delete it 
                //{ //this is superflous As we alway delete the current sel

                //}
                if (el.Value == "contains")
                {//we need two new selections
                    InPageSelections curr = iaPDF.inPageSelections[el.Key];
                    int Nstart = curr.Start;
                    int Nend = start;
                    selectOnPage(pdfV, pdfPage, Nstart, Nend);
                    if (pdfV.Document.Selection.GetSelectedText().Length != 0)//check that current sel doesn't start where E sel starts -> omit 0 length new selection
                    {
                        toAdd.Add(new InPageSelections(Nstart, Nend, pdfV.Document.Selection.GetSelectedText()));
                    }
                    Nstart = end;
                    Nend = curr.End;
                    selectOnPage(pdfV, pdfPage, Nstart, Nend);
                    if (pdfV.Document.Selection.GetSelectedText().Length != 0)//check that current sel doesn't end where E sel ends -> omit 0 length new selection
                    {
                        toAdd.Add(new InPageSelections(Nstart, Nend, pdfV.Document.Selection.GetSelectedText()));
                    }
                }
                else if (el.Value == "overlap")
                {
                    if (iaPDF.inPageSelections[el.Key].Start == start && iaPDF.inPageSelections[el.Key].End == end)//they coincide, so no other change is needed
                    { }
                    else if (iaPDF.inPageSelections[el.Key].Start < start && iaPDF.inPageSelections[el.Key].End <= end)//keep the first part
                    {
                        InPageSelections curr = iaPDF.inPageSelections[el.Key];
                        int Nstart = curr.Start;
                        int Nend = start;
                        selectOnPage(pdfV, pdfPage, Nstart, Nend);
                        toAdd.Add(new InPageSelections(Nstart, Nend, pdfV.Document.Selection.GetSelectedText()));
                    }
                    else if (iaPDF.inPageSelections[el.Key].Start >= start && iaPDF.inPageSelections[el.Key].End > end)//keep the trailing part
                    {
                        InPageSelections curr = iaPDF.inPageSelections[el.Key];
                        int Nstart = end;
                        int Nend = curr.End;
                        selectOnPage(pdfV, pdfPage, Nstart, Nend);
                        toAdd.Add(new InPageSelections(Nstart, Nend, pdfV.Document.Selection.GetSelectedText()));
                    }
                }
            }
            foreach (InPageSelections dellin in toDel)
            {
                iaPDF.inPageSelections.Remove(dellin);//remove stale
            }
            iaPDF.inPageSelections.AddRange(toAdd);//and new/updated
            iaPDF.inPageSelections.Sort();//make sure they are in the right order


            //recalculate shape:
            iaPDF.Shape = new PathGeometry { FillRule = FillRule.Nonzero };
            
            foreach (InPageSelections sel in iaPDF.inPageSelections)
            {
                selectOnPage(pdfV, pdfPage, sel.Start, sel.End);
                pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                pg = pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                while (pg.Figures.Count > 0)
                {
                    PathFigure pf = pg.Figures[0];
                    pg.Figures.RemoveAt(0);
                    iaPDF.Shape.Figures.Add(pf);
                }
            }
            iaPDF.ShapeTxt = iaPDF.Shape.ToString();
            if (iaPDF.ShapeTxt.Length >= 2 && iaPDF.ShapeTxt.Substring(0, 2) != "F1")
            {
                iaPDF.ShapeTxt = "F1" + iaPDF.ShapeTxt;
                iaPDF.Shape = ItemAttributePDF.makeGeom(iaPDF.ShapeTxt);
            }
            pdfV.Document.Selection.Clear();//remove all traces

            if (iaPDF.inPageSelections.Count == 0)
            {
                iaPDF.Delete();
            }
        }
        public static void selectOnPage(RadPdfViewer pdfV, RadFixedPage pdfPage, int start, int end)
        {
            pdfV.Document.Selection.Clear();
            pdfV.Document.Selection.SetSelectionStart(new Telerik.Windows.Documents.Fixed.Text.TextPosition(pdfPage, start));
            try
            {
                pdfV.Document.Selection.SetSelectionEnd(new Telerik.Windows.Documents.Fixed.Text.TextPosition(pdfPage, end));
            }
            catch
            {
                int i = 0, counter = 0;
                Telerik.Windows.Documents.Fixed.Text.TextPosition tp = new Telerik.Windows.Documents.Fixed.Text.TextPosition(pdfPage, start);
                while (i <= end && tp.Page == pdfPage && counter < 5000)
                {
                    counter++;
                    tp.MoveToNextWord();
                    i = tp.Index;
                }
                if (tp.Page != pdfPage)
                {
                    tp.MoveToPreviousWord();
                    if (tp.Page != pdfPage) tp.MoveToPreviousWord();
                    
                }
                pdfV.Document.Selection.SetSelectionEnd(tp);
            }
        }
        public static void RebuildShape(RadPdfViewer pdfV, RadFixedPage pdfPage, ref ItemAttributePDF iaPDF)
        {
            
            PathGeometry pg = new PathGeometry { FillRule = FillRule.Nonzero };
            foreach (InPageSelections sel in iaPDF.inPageSelections)
            {
                selectOnPage(pdfV, pdfPage, sel.Start, sel.End);
                pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                pg = pdfV.Document.Selection.GetSelectionGeometry(pdfPage);
                while (pg.Figures.Count > 0)
                {
                    PathFigure pf = pg.Figures[0];
                    pg.Figures.RemoveAt(0);
                    iaPDF.Shape.Figures.Add(pf);
                }
            }
            pdfV.Document.Selection.Clear();
            iaPDF.ShapeTxt = iaPDF.Shape.ToString();
            if (iaPDF.ShapeTxt.IndexOf("F1") != 0) iaPDF.ShapeTxt = "F1" + iaPDF.ShapeTxt;
        }

        private static Dictionary<int, string> EvaluateIntersects(int start, int end, ref ItemAttributePDF iaPDF)
        {
            Dictionary<int, string> res = new Dictionary<int, string>();
            bool inSt;
            bool inEnd;
            for (int i = 0; i < iaPDF.inPageSelections.Count; i++)
            {
                inSt = false;
                inEnd = false;
                InPageSelections ppas = iaPDF.inPageSelections[i];
                if (start <= ppas.Start && end >= ppas.End) //current sel includes E sel
                {
                    res.Add(i, "contained");
                    continue;
                }
                if (start >= ppas.Start && start <= ppas.End) { inSt = true; }//current sel starts within E sel
                if (end <= ppas.End && end >= ppas.Start) { inEnd = true; }//current sel ends within E sel 
                if (inSt && inEnd) //current sel is part of E sel
                {
                    res.Add(i, "contains");
                    break; //because the current sel can be contained in one E sel only;
                }
                else if ((inSt && !inEnd) || (!inSt && inEnd))// (current sel starts within E sel and end is outside or end is in start is out: proper intersection
                {
                    res.Add(i, "overlap");
                    continue;
                }
            }
            return res;
        }
    }
}
