using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.FormatProviders.Html;
using Telerik.Windows.Documents.FormatProviders;
using System.IO;
//using DevExpress.Xpf.RichEdit.Extensions;
using System.Windows.Media.Imaging;
using Telerik.Windows.Documents.FormatProviders.OpenXml.Docx;
using Telerik.Windows.Controls.RichTextBoxUI;
using Telerik.Windows.Controls.RichTextBoxUI.Menus;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.TextSearch;
using BusinessLibrary.BusinessClasses;

namespace EppiReviewer4
{
    public partial class dialogReportViewer : UserControl
    {
        public dialogReportViewer()
        {
            InitializeComponent();
            //richEdit1.ApplyTemplate();
            ContextMenu contextMenu = (ContextMenu)this.radRichTextBox1.ContextMenu;
            contextMenu.Showing += new EventHandler<ContextMenuEventArgs>(contextMenu_Showing);
            
        }

        void contextMenu_Showing(object sender, ContextMenuEventArgs e)
        {
            var groups = e.ContextMenuGroupCollection;
            for (int i = 0; i < e.ContextMenuGroupCollection.Count; i++)
            {
                if (groups[i].Type.ToString() != "ClipboardCommands")
                    groups[i].Clear();
            }
        }
        

        public void SetContent(string content)
        {
            //richEdit1.RichControl.Document.HtmlText = content;
            //richEdit1.RichControl.Document.Sections[0].Page.Landscape = true;
            HtmlFormatProvider provider = new HtmlFormatProvider();
            radRichTextBox1.Document = provider.Import(content);
            radRichTextBox1.Document.SectionDefaultPageOrientation = PageOrientation.Landscape;
        }

        public void DisplayMetaAnalysis(string maTitle, string titleText, string feResults, string reResults,
            string heterogeneity, Stream imageFe,
            Stream imageRe, Stream funnelPlot)
        {
            //richEdit1.RichControl.Document.Text = "";
            //DevExpress.XtraRichEdit.API.Native.DocumentRange dr = richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, "Meta-analysis results: " + maTitle);
            //DevExpress.XtraRichEdit.API.Native.CharacterProperties charProperties = richEdit1.RichControl.Document.BeginUpdateCharacters(dr);
            //charProperties.FontName = "Arial";
            //charProperties.FontSize = 18;
            //charProperties.ForeColor = Colors.DarkGray;
            //charProperties.Bold = true;
            //richEdit1.RichControl.Document.EndUpdateCharacters(charProperties);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertSingleLineText(richEdit1.RichControl.Document.Range.End, titleText);

            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, 
            //    heterogeneity + ".");
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, feResults);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //if (imageFe != null)
            //{
            //    richEdit1.RichControl.Document.InsertImage(richEdit1.RichControl.Document.Range.End, imageFe);
            //    richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //    richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //}
            
            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, reResults);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //if (imageRe != null)
            //{
            //    richEdit1.RichControl.Document.InsertImage(richEdit1.RichControl.Document.Range.End, imageRe);
            //    richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //}
            //if (funnelPlot != null)
            //{
            //    richEdit1.RichControl.Document.InsertImage(richEdit1.RichControl.Document.Range.End, funnelPlot);
            //}
            RadDocument document = new RadDocument();
            Section section = new Section();
            Paragraph p1 = new Paragraph();
            Span s1 = new Span("Meta-analysis results: " + maTitle);
            s1.FontFamily = new System.Windows.Media.FontFamily("Arial");
            s1.FontSize = 22;
            s1.ForeColor = Colors.DarkGray;
            s1.FontWeight = FontWeights.Bold;
            p1.Inlines.Add(s1);
            section.Blocks.Add(p1);
            Paragraph p2 = new Paragraph();
            Span s2 = new Span(titleText);
            p2.Inlines.Add(s2);
            section.Blocks.Add(p2);
            Paragraph p3 = new Paragraph();
            Span s3 = new Span(heterogeneity + ".");
            p3.Inlines.Add(s3);
            section.Blocks.Add(p3);

            Paragraph p4 = new Paragraph();
            Span s4 = new Span(feResults);
            p4.Inlines.Add(s4);
            section.Blocks.Add(p4);

            if (imageFe != null && imageFe.Length > 0)
            {
                ImageInline imageInline = new ImageInline(imageFe);
                Paragraph p5 = new Paragraph();
                p5.Inlines.Add(imageInline);
                section.Blocks.Add(p5);
            }

            Paragraph p6 = new Paragraph();
            Span s6 = new Span(reResults);
            p6.Inlines.Add(s6);
            section.Blocks.Add(p6);

            if (imageRe != null && imageRe.Length > 0)
            {
                ImageInline imageInline = new ImageInline(imageRe);
                Paragraph p7 = new Paragraph();
                p7.Inlines.Add(imageInline);
                section.Blocks.Add(p7);
            }
            if (funnelPlot != null && funnelPlot.Length > 0)
            {
                ImageInline imageInline = new ImageInline(funnelPlot);
                Paragraph p8 = new Paragraph();
                p8.Inlines.Add(imageInline);
                section.Blocks.Add(p8);
            }
            
            document.Sections.Add(section);
            radRichTextBox1.Document = document;
        }

        public void DisplaySubGroupAnalysis(string maTitle1, string maTitle2, string titleText1, string titleText2,
            string feResults, string reResults, string feDifference, string reDifference, string heterogeneity,
            Stream imageFe,
            Stream imageRe
            )
        {
            //richEdit1.RichControl.Document.Text = "";
            //DevExpress.XtraRichEdit.API.Native.DocumentRange dr = richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, "Sub-group analysis");
            //DevExpress.XtraRichEdit.API.Native.CharacterProperties charProperties = richEdit1.RichControl.Document.BeginUpdateCharacters(dr);
            //charProperties.FontName = "Arial";
            //charProperties.FontSize = 18;
            //charProperties.ForeColor = Colors.DarkGray;
            //charProperties.Bold = true;
            //richEdit1.RichControl.Document.EndUpdateCharacters(charProperties);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);

            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //dr = richEdit1.RichControl.Document.InsertSingleLineText(richEdit1.RichControl.Document.Range.End, "Subgroup 1: " + maTitle1);
            //charProperties = richEdit1.RichControl.Document.BeginUpdateCharacters(dr);
            //charProperties.FontName = "Arial";
            //charProperties.Bold = true;
            //richEdit1.RichControl.Document.EndUpdateCharacters(charProperties);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, titleText1);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);

            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //dr = richEdit1.RichControl.Document.InsertSingleLineText(richEdit1.RichControl.Document.Range.End, "Subgroup 2: " + maTitle2);
            //charProperties = richEdit1.RichControl.Document.BeginUpdateCharacters(dr);
            //charProperties.FontName = "Arial";
            //charProperties.Bold = true;
            //richEdit1.RichControl.Document.EndUpdateCharacters(charProperties);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, titleText2);

            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End,
            //    heterogeneity + ".");
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);

            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, feResults);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, feDifference);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //if (imageFe != null)
            //{
            //    richEdit1.RichControl.Document.InsertImage(richEdit1.RichControl.Document.Range.End, imageFe);
            //    richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //    richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //}

            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, reResults);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //richEdit1.RichControl.Document.InsertText(richEdit1.RichControl.Document.Range.End, reDifference);
            //richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //if (imageRe != null)
            //{
            //    richEdit1.RichControl.Document.InsertImage(richEdit1.RichControl.Document.Range.End, imageRe);
            //    richEdit1.RichControl.Document.InsertParagraph(richEdit1.RichControl.Document.Range.End);
            //}
            RadDocument document = new RadDocument();
            Section section = new Section();
            Paragraph p1 = new Paragraph();
            Span s1 = new Span("Sub-group analysis");
            s1.FontFamily = new System.Windows.Media.FontFamily("Arial");
            s1.FontSize = 22;
            s1.ForeColor = Colors.DarkGray;
            s1.FontWeight = FontWeights.Bold;
            p1.Inlines.Add(s1);
            section.Blocks.Add(p1);
            
            Paragraph p2 = new Paragraph();
            Span s2 = new Span("Subgroup 1: " + maTitle1);
            s2.FontWeight = FontWeights.Bold;
            p2.Inlines.Add(s2);
            section.Blocks.Add(p2);

            Paragraph p2b = new Paragraph();
            Span s2b = new Span("Subgroup 2: " + maTitle2);
            s2b.FontWeight = FontWeights.Bold;
            p2b.Inlines.Add(s2b);
            section.Blocks.Add(p2b);
            
            Paragraph p3 = new Paragraph();
            Span s3 = new Span(titleText1);
            p3.Inlines.Add(s3);
            section.Blocks.Add(p3);
            
            Paragraph p3b = new Paragraph();
            Span s3b = new Span(titleText2);
            p3b.Inlines.Add(s3b);
            section.Blocks.Add(p3b);

            Paragraph p3c = new Paragraph();
            Span s3c = new Span(heterogeneity + ".");
            p3c.Inlines.Add(s3c);
            section.Blocks.Add(p3c);

            Paragraph p4 = new Paragraph();
            Span s4 = new Span(feResults);
            p4.Inlines.Add(s4);
            section.Blocks.Add(p4);

            Paragraph p4b = new Paragraph();
            Span s4b = new Span(feDifference);
            p4b.Inlines.Add(s4b);
            section.Blocks.Add(p4b);

            if (imageFe != null && imageFe.Length > 0)
            {
                ImageInline imageInline = new ImageInline(imageFe);
                Paragraph p5 = new Paragraph();
                p5.Inlines.Add(imageInline);
                section.Blocks.Add(p5);
            }

            Paragraph p6 = new Paragraph();
            Span s6 = new Span(reResults);
            p6.Inlines.Add(s6);
            section.Blocks.Add(p6);

            Paragraph p6b = new Paragraph();
            Span s6b = new Span(reDifference);
            p6b.Inlines.Add(s6b);
            section.Blocks.Add(p6b);

            if (imageRe != null && imageRe.Length > 0)
            {
                ImageInline imageInline = new ImageInline(imageRe);
                Paragraph p7 = new Paragraph();
                p7.Inlines.Add(imageInline);
                section.Blocks.Add(p7);
            }
            

            document.Sections.Add(section);
            radRichTextBox1.Document = document;
        }
        public void DisplayRMetaAnalysis(MetaAnalysisRunInRCommand cmd)
        { 
            RadDocument document = new RadDocument();
            //document.ParagraphDefaultSpacingAfter = 0; // James added, as the editor is interpreting Environment.NewLine as a paragrpah separator
            // document.ParagraphDefaultSpacingBefore = 0; // using code below now
            Section section = new Section();

            Paragraph TitleP = new Paragraph();
            Span Title = new Span(cmd.MetaAnalaysisObject.Title);
            Title.FontSize = 20;
            Title.FontWeight = FontWeights.Bold;
            Color mycolor = new Color();
            mycolor.A = 255;
            mycolor.B = 181;
            mycolor.G = 116;
            mycolor.R = 46;
            Title.ForeColor = mycolor;
            TitleP.Inlines.Add(Title);
            section.Blocks.Add(TitleP);

            List<Paragraph> pars = new List<Paragraph>();
            for (int i = 0; i < cmd.ResultsLabels.Count; i++)
            {
                Paragraph p1 = new Paragraph();
                p1.FontSize =  i == 0 ? 11 : 10;
                Span s1 = new Span(cmd.ResultsLabels[i]);
                s1.FontWeight = FontWeights.Bold;
                p1.Inlines.Add(s1);
                pars.Add(p1);
                Paragraph p2 = new Paragraph();
                p2.FontSize = i == 0 ? 11 : 10;
                //p2.LineSpacing = 0.5; // doesn't look nice when exported
                Span s2 = new Span(cmd.ResultsText[i]);
                p2.Inlines.Add(s2);
                s2.FontFamily = new System.Windows.Media.FontFamily("Courier New");
                pars.Add(p2);
            }
            foreach (Paragraph par in pars)
            {
                //p1.Inlines.Add(span);
                section.Blocks.Add(par);
            }
            //section.Blocks.Add(p1);
            if (cmd.GraphsList != null && cmd.GraphsList.Count > 0)
            {
                for (int i = 0; i < cmd.GraphsList.Count; i++)
                {
                    MemoryStream imageRe = new MemoryStream(cmd.GraphsList[i]);
                    if (imageRe != null && imageRe.Length > 0)
                    {
                        Paragraph imTitle = new Paragraph();
                        Span s1 = new Span(cmd.GraphsTitles[i]);
                        s1.FontWeight = FontWeights.Bold;
                        imTitle.Inlines.Add(s1);
                        section.Blocks.Add(imTitle);
                        ImageInline imageInline = new ImageInline(imageRe);
                        Paragraph p7 = new Paragraph();
                        p7.Inlines.Add(imageInline);
                        section.Blocks.Add(p7);
                    }
                }
            }
            Paragraph codeTitleP = new Paragraph();
            codeTitleP.FontSize = 12;
            Span codeTitle = new Span("R-Code (Metafor):");
            codeTitle.FontFamily = new System.Windows.Media.FontFamily("Arial");
            codeTitle.FontSize = 12;
            codeTitle.FontWeight = FontWeights.Bold;
            codeTitleP.Inlines.Add(codeTitle);
            section.Blocks.Add(codeTitleP);

            Paragraph codeBodyP = new Paragraph();
            Span codeBody = new Span(cmd.RCode);
            codeBody.FontSize = 11;
            codeBody.FontFamily = new System.Windows.Media.FontFamily("Courier New");
            codeBodyP.Inlines.Add(codeBody);
            section.Blocks.Add(codeBodyP);

            Paragraph codeCitP = new Paragraph();
            Span citation1 = new Span("These results are provided by the Metafor Package for R, please include the following citation when publishing the above." + Environment.NewLine);
            citation1.FontSize = 11;
            codeCitP.Inlines.Add(citation1);
            Span citation2 = new Span("Wolfgang Viechtbauer (2010). Conducting meta-analyses in R with the metafor package. ");
            citation2.FontSize = 11;
            codeCitP.Inlines.Add(citation2);
            Span citation3 = new Span("Journal of Statistical Software");
            citation3.FontSize = 11;
            citation3.FontStyle = FontStyles.Italic;
            codeCitP.Inlines.Add(citation3);
            Span citation4 = new Span(", 36(3), 1-48." + Environment.NewLine);
            citation4.FontSize = 11;
            codeCitP.Inlines.Add(citation4);
            section.Blocks.Add(codeCitP);
            document.Sections.Add(section);
            radRichTextBox1.Document = document;

            // James added to change the paragraph breaks to line breaks
            radRichTextBox1.Document.Selection.Clear();
            DocumentTextSearch search = new DocumentTextSearch(radRichTextBox1.Document);
            foreach (var textRange in search.FindAll(Environment.NewLine))
            {
                if (textRange != null)
                {
                    radRichTextBox1.Document.Selection.AddSelectionStart(textRange.StartPosition);
                    this.radRichTextBox1.Document.Selection.AddSelectionEnd(textRange.EndPosition);
                    radRichTextBox1.InsertLineBreak();
                }
            }
        }
        public void ExportToDocx(object sender, RoutedEventArgs e)
        {
            RadDocument document = radRichTextBox1.Document;
            DocxFormatProvider provider = new DocxFormatProvider();
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".docx";
            saveDialog.Filter = "Documents|*.docx";
            bool? dialogResult = saveDialog.ShowDialog();
            if (dialogResult == true)
            {
                using (Stream output = saveDialog.OpenFile())
                {
                    provider.Export(document, output);
                    MessageBox.Show("Saved Successfuly!");
                }
            }
        }
        private void btnFindTxt_Click(object sender, RoutedEventArgs e)
        {
            if (tbFindTxt.Text.Length == 0) return;
            //rich.Document.Selection.Clear(); // this clears the selection before processing
            DocumentTextSearch search = new DocumentTextSearch(radRichTextBox1.Document);
            IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange> result = search.FindAll(System.Text.RegularExpressions.Regex.Escape(tbFindTxt.Text));
            tbFindTxt.DataContext = result;
            //IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange>

            foreach (Telerik.Windows.Documents.TextSearch.TextRange tr in result)
            {
                if (radRichTextBox1.Document.CaretPosition < tr.StartPosition)
                {
                    radRichTextBox1.Document.Selection.Clear();
                    radRichTextBox1.Document.CaretPosition.MoveToPosition(tr.EndPosition);
                    radRichTextBox1.Document.Selection.AddSelectionStart(tr.StartPosition);
                    radRichTextBox1.Document.Selection.AddSelectionEnd(tr.EndPosition);
                    btnPrevTxt.Visibility = System.Windows.Visibility.Visible;
                    btnNextTxt.Visibility = System.Windows.Visibility.Visible;
                    return;
                }
            }
            RadWindow.Alert("Not found");
        }

        private void tbFindTxt_KeyUp(object sender, KeyEventArgs e)
        {
            tbFindTxt.DataContext = null;
            btnPrevTxt.Visibility = System.Windows.Visibility.Collapsed;
            btnNextTxt.Visibility = System.Windows.Visibility.Collapsed;
            if (tbFindTxt.Text.Length < 1)
            {
                return;
            }
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btnFindTxt_Click(sender, new RoutedEventArgs());
            }
        }


        private void btnPrevTxt_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange> result = tbFindTxt.DataContext as IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange>;
            if (result == null)
            {
                btnPrevTxt.Visibility = System.Windows.Visibility.Collapsed;
                btnNextTxt.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            List<Telerik.Windows.Documents.TextSearch.TextRange> li = result as List<Telerik.Windows.Documents.TextSearch.TextRange>;
            int i = 0;
            while (i < li.Count)
            {
                Telerik.Windows.Documents.TextSearch.TextRange tr = li[i];
                if (radRichTextBox1.Document.CaretPosition <= tr.EndPosition)
                {
                    if (i == 0)
                    {
                        RadWindow.Alert("Reached the Start of the" + Environment.NewLine + "Document");
                        return;
                    }
                    tr = li[i - 1];
                    radRichTextBox1.Document.Selection.Clear();
                    radRichTextBox1.Document.CaretPosition.MoveToPosition(tr.EndPosition);
                    radRichTextBox1.Document.Selection.AddSelectionStart(tr.StartPosition);
                    radRichTextBox1.Document.Selection.AddSelectionEnd(tr.EndPosition);
                    btnPrevTxt.Visibility = System.Windows.Visibility.Visible;
                    btnNextTxt.Visibility = System.Windows.Visibility.Visible;
                    return;
                }
                else i++;
            }
            RadWindow.Alert("Not found");
        }

        private void btnNextTxt_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange> result = tbFindTxt.DataContext as IEnumerable<Telerik.Windows.Documents.TextSearch.TextRange>;
            if (result == null)
            {
                btnPrevTxt.Visibility = System.Windows.Visibility.Collapsed;
                btnNextTxt.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            foreach (Telerik.Windows.Documents.TextSearch.TextRange tr in result)
            {
                if (radRichTextBox1.Document.CaretPosition < tr.StartPosition)
                {
                    radRichTextBox1.Document.Selection.Clear();
                    radRichTextBox1.Document.CaretPosition.MoveToPosition(tr.EndPosition);
                    radRichTextBox1.Document.Selection.AddSelectionStart(tr.StartPosition);
                    radRichTextBox1.Document.Selection.AddSelectionEnd(tr.EndPosition);
                    btnPrevTxt.Visibility = System.Windows.Visibility.Visible;
                    btnNextTxt.Visibility = System.Windows.Visibility.Visible;
                    
                    return;
                }
            }
            RadWindow.Alert("Reached the End of the" + Environment.NewLine + "Document");
        }

    }
}
