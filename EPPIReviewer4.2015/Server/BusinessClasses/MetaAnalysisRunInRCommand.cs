using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using System.Text.RegularExpressions;


#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Xml.Linq;
using DeployR;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

#endif


namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MetaAnalysisRunInRCommand : CommandBase<MetaAnalysisRunInRCommand>
    {
//#if SILVERLIGHT
        
        public MetaAnalysisRunInRCommand() 
        {
            EffectSizes = new MobileList<double>();
            ConfIntervals = new MobileList<double>();
            StudyLabels = new MobileList<string>();
            ResultsLabels = new MobileList<string>();
            GraphsList = new MobileList<byte[]>();
            GraphsTitles = new MobileList<string>();
        }
//#else
//        public MetaAnalysisRunInRCommand()
//        {
//            //EffectSizes = new MobileList<double>();
//            //ConfnItervals = new MobileList<double>();
//            //StudyLabels = new MobileList<string>();
//            //ResultsText = new MobileList<string>();
//            GraphsList = new MobileList<byte[]>();
//            //GraphsTitles = new MobileList<string>();
//        }
//#endif
        public static readonly PropertyInfo<MetaAnalysis> MetaAnalaysisObjectProperty = RegisterProperty<MetaAnalysis>(new PropertyInfo<MetaAnalysis>("MetaAnalaysisObject", "MetaAnalaysisObject"));
        public MetaAnalysis MetaAnalaysisObject
        {
            get { return ReadProperty(MetaAnalaysisObjectProperty); }
            set { LoadProperty(MetaAnalaysisObjectProperty, value); }
        }

        public static readonly PropertyInfo<MobileList<double>> EffectSizesProperty = RegisterProperty<MobileList<double>>(new PropertyInfo<MobileList<double>>("EffectSizes", "EffectSizes"));
        public MobileList<double> EffectSizes
        {
            get { return ReadProperty(EffectSizesProperty); }
            set { LoadProperty(EffectSizesProperty, value); }
        }
        public static readonly PropertyInfo<MobileList<double>> ConfIntervalsProperty = RegisterProperty<MobileList<double>>(new PropertyInfo<MobileList<double>>("ConfIntervals", "ConfIntervals"));
        public MobileList<double> ConfIntervals
        {
            get { return ReadProperty(ConfIntervalsProperty); }
            set { LoadProperty(ConfIntervalsProperty, value); }
        }
        public static readonly PropertyInfo<MobileList<string>> StudyLabelsProperty = RegisterProperty<MobileList<string>>(new PropertyInfo<MobileList<string>>("StudyLabels", "StudyLabels"));
        public MobileList<string> StudyLabels
        {
            get { return ReadProperty(StudyLabelsProperty); }
            set { LoadProperty(StudyLabelsProperty, value); }
        }
        public static readonly PropertyInfo<string> RCodeProperty = RegisterProperty<string>(new PropertyInfo<string>("RCode", "RCode"));
        public string RCode
        {
            get { return ReadProperty(RCodeProperty); }
            set { LoadProperty(RCodeProperty, value); }
        }
        public static readonly PropertyInfo<MobileList<string>> ResultsTextProperty = RegisterProperty<MobileList<string>>(new PropertyInfo<MobileList<string>>("ResultsText", "ResultsText"));
        public MobileList<string> ResultsText
        {
            get { return ReadProperty(ResultsTextProperty); }
            set { LoadProperty(ResultsTextProperty, value); }
        }
        public static readonly PropertyInfo<MobileList<string>> ResultsLabelsProperty = RegisterProperty<MobileList<string>>(new PropertyInfo<MobileList<string>>("ResultsLabels", "ResultsLabels"));
        public MobileList<string> ResultsLabels
        {
            get { return ReadProperty(ResultsLabelsProperty); }
            set { LoadProperty(ResultsLabelsProperty, value); }
        }
        public static readonly PropertyInfo<string> OptionsProperty = RegisterProperty<string>(new PropertyInfo<string>("Options", "Options"));
        public string Options
        {
            get { return ReadProperty(OptionsProperty); }
            set { LoadProperty(OptionsProperty, value); }
        }
        public static readonly PropertyInfo<MobileList<byte[]>> GraphsListProperty = RegisterProperty<MobileList<byte[]>>(new PropertyInfo<MobileList<byte[]>>("GraphsList", "GraphsList"));
        public MobileList<byte[]> GraphsList
        {
            get { return ReadProperty(GraphsListProperty); }
            set { LoadProperty(GraphsListProperty, value); }
        }
        public static readonly PropertyInfo<MobileList<string>> GraphsTitlesProperty = RegisterProperty<MobileList<string>>(new PropertyInfo<MobileList<string>>("GraphsTitles", "GraphsTitles"));
       
        public MobileList<string> GraphsTitles
        {
            get { return ReadProperty(GraphsTitlesProperty); }
            set { LoadProperty(GraphsTitlesProperty, value); }
        }
#if !SILVERLIGHT


        protected string GetRServerAddress()
        {
            if (Dns.GetHostName() == "ssru_Elephant")
            {
                return "http://R-EPI2.cloudapp.net/deployr"; 
            }
            else
            {
                return "http://r-epi-private:7400/deployr";
            }
        }

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //first block logon and prepare the input data

            RClient rClient = RClientFactory.createClient(GetRServerAddress());
            
            RAuthentication authToken = new RBasicAuthentication("***REMOVED***", "***REMOVED***");
            RUser rUser = rClient.login(authToken);

            List<Double?> numVect = new List<Double?>();
            List<Double?> numVect2 = new List<Double?>();
            List<String> NameVec = new List<String>();

            RNumericVector rNumVector = RDataFactory.createNumericVector("yi", numVect);
            RNumericVector rNumVector2 = RDataFactory.createNumericVector("sei", numVect2); // James changed on 23/09/2015
            RStringVector rNDateVect = RDataFactory.createStringVector("studylabs", NameVec); // nb - JT changed this name from author

            //PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(Outcome)).Find(MetaAnalaysisObject.SortedBy, false);
            IEnumerable<Outcome> Outcomes;
            switch (this.MetaAnalaysisObject.SortDirection)
            {
                case "Ascending":
                    Outcomes =
                from outcome in this.MetaAnalaysisObject.Outcomes
                where outcome.IsSelected == true
                orderby outcome.GetType().GetProperty(this.MetaAnalaysisObject.SortedBy).GetValue(outcome, null) ascending
                select outcome;
                    break;

                case "Descending":
                    Outcomes =
                from outcome in this.MetaAnalaysisObject.Outcomes
                where outcome.IsSelected == true
                orderby outcome.GetType().GetProperty(this.MetaAnalaysisObject.SortedBy).GetValue(outcome, null) descending
                select outcome;
                    break;

                default:
                    Outcomes =
                from outcome in this.MetaAnalaysisObject.Outcomes
                where outcome.IsSelected == true
                select outcome;
                    break;
            }

            foreach (Outcome outc in Outcomes)
            {
                if (outc.IsSelected)
                {
                    NameVec.Add(outc.ShortTitle);
                    numVect.Add(outc.GetEffectSizeDisplaying(this.MetaAnalaysisObject.MetaAnalysisTypeId));
                    numVect2.Add(outc.SEES);
                }
            }
            List<RData> dfVector = new List<RData>();
            dfVector.Add(rNumVector);
            dfVector.Add(rNumVector2);
            dfVector.Add(rNDateVect);
            if (this.MetaAnalaysisObject.AnalysisType == 0) // i.e. s standard meta-analysis with poossible moderators
            {
                foreach (MetaAnalysisModerator mam in MetaAnalaysisObject.MetaAnalysisModerators)
                {
                    if (mam.IsSelected)
                    {
                        if (mam.IsFactor || !mam.IsFactor) // i.e. we need logic for non-factors, but everything is a factor at the moment
                        {
                            List<String> vals = new List<String>();
                            foreach (Outcome outc in Outcomes)
                            {
                                if (outc.IsSelected)
                                {
                                    //string s = outc.GetType().GetProperty(GetObjFieldName(mam)).GetValue(outc, null).ToString();
                                    string s = outc.GetType().GetProperty(mam.FieldName).GetValue(outc, null).ToString();
                                    vals.Add(Regex.Replace(s, @"[^A-Za-z0-9]+", ""));
                                }
                            }
                            RStringVector rsv = RDataFactory.createStringVector(Regex.Replace(mam.Name, @"[^A-Za-z0-9]+", ""), vals);
                            dfVector.Add(rsv);
                        }
                        else
                        {
                            //RNumericVector rnv = RDataFactory.createNumericVector(Regex.Replace(mam.Name, @"[^A-Za-z0-9]+", ""), new List<Double?>());
                            List<String> vals = new List<String>();
                            RStringVector rsv = RDataFactory.createStringVector(Regex.Replace(mam.Name, @"[^A-Za-z0-9]+", ""), vals);
                            foreach (Outcome outc in this.MetaAnalaysisObject.Outcomes)
                            {
                                if (outc.IsSelected)
                                {

                                }
                            }
                        }
                    }
                }
            }

            if (this.MetaAnalaysisObject.AnalysisType == 1) // i.e. a network meta-analysis where we always want to add the intervention and control columns
            {
                foreach (MetaAnalysisModerator mam in this.MetaAnalaysisObject.MetaAnalysisModerators)
                {
                    if (mam.FieldName == "InterventionText" || mam.FieldName == "ControlText")
                    {
                        List<String> vals = new List<String>();
                        foreach (Outcome outc in Outcomes)
                        {
                            if (outc.IsSelected)
                            {
                                //string s = outc.GetType().GetProperty(GetObjFieldName(mam)).GetValue(outc, null).ToString();
                                string s = outc.GetType().GetProperty(mam.FieldName).GetValue(outc, null).ToString();
                                vals.Add(Regex.Replace(s, @"[^A-Za-z0-9]+", ""));
                            }
                        }
                        RStringVector rsv = RDataFactory.createStringVector(Regex.Replace(mam.Name, @"[^A-Za-z0-9]+", ""), vals);
                        dfVector.Add(rsv);
                    }
                }
            }

            RDataFrame rDataFrame = RDataFactory.createDataFrame("dat", dfVector); // nb - JT changed this name from dat.bcg
            ProjectCreationOptions options = new ProjectCreationOptions();
            options.rinputs.Add(rDataFrame);
            RProject blac = rUser.createProject(options);
            //end of first block

            //SECOND block prepare code: James sets this bit up
            //step 1 is to decide how to represent the options and add the corresponding property (or properties) to this object,
            //step 2 in dialogMetaAnalysisSetup is to populate the "options" properties according to user choice.
            //step 3 is back here: write the correct R-Code based on the properties
            //that's it, nothing else is required ;-)
            string Rcode = "";
            switch (this.MetaAnalaysisObject.AnalysisType)
            {
                case 0: Rcode = GetMetaforScript(this.MetaAnalaysisObject, numVect.Count); break;
                case 1: Rcode = GetNetmetaScript(this.MetaAnalaysisObject, numVect.Count); break;
                default: break;
            }
            
            //end of second block

            //third block, prepare output
            ProjectExecutionOptions ExecOptions = new ProjectExecutionOptions();
            ExecOptions.consoleoff = false;
            ExecOptions.echooff = true;


            foreach (string section in ResultsLabels)
            {//this adds named objects that will be fetched as results, MUST match the R code that is being prepared above
                ExecOptions.routputs.Add(section.Replace(" ", "_"));
            }
            //end of third block

            //fourth block talk to R
            try
            {
                RProjectExecution exec = blac.executeCode(Rcode, ExecOptions);

                //now grab results
                if (ResultsText == null) ResultsText = new MobileList<string>();
                foreach (string section in ResultsLabels)
                {//this fetches named objects from results, MUST match the R code that is being prepared above


                    RStringVector txtMainRes = blac.getObject(section.Replace(" ", "_")) as RStringVector;
                    string currResults = "";
                    foreach (string line in txtMainRes.Value as List<string>)
                    {
                        //if (line.Trim().Length > 0) //James commented out, as the empty lines help make the output more readable
                        {
                            currResults += line.Trim() + Environment.NewLine;
                        }
                    }
                    ResultsText.Add(currResults);
                }

                List<RProjectFile> files = blac.listFiles();
                if (files != null && files.Count > 0)
                {
                    GraphsList = new MobileList<byte[]>();//we always populate this on server side
                    if (GraphsTitles == null)
                    {// it's possible we could populate this on the client side / not use the data anyway
                        //could be useful to populate on client side, filling the titles of the expected plots, and thus specifying how many plots we expect
                        GraphsTitles = new MobileList<string>();
                    }
                    foreach (string title in GraphsTitles)
                    {
                        foreach (RProjectFile file in files)
                        {
                            //string tmp = System.Web.HttpUtility.UrlPathEncode(title.Replace(' ', '_'));
                            if (file.about().url.Contains(title.Replace(' ', '_').Replace("(", "").Replace(")", "")))
                            {
                                using (WebClient wc = new WebClient())
                                {
                                    //byte[] data = wc.DownloadData(file.about().url.Replace(":7400", ""));//was used when passing through "Classic" azure endpoints
                                    byte[] data = wc.DownloadData(file.about().url);
                                    GraphsList.Add(data);
                                }
                                break;
                            }
                        }
                    }
                    //for (int i = 0; i < GraphsTitles.Count; i++)
                    //{
                    //    RProjectFile plot = files[i];
                    //    using (WebClient wc = new WebClient())
                    //    {
                    //        byte[] data = wc.DownloadData(plot.about().url.Replace(":7400", ""));
                    //        GraphsList.Add(data);
                    //    }
                    //}
                }
            }
            finally
            {
                RCode = Rcode;
                blac.close();
            }
            //end of 4th block and all
        }

        private string GetObjFieldName(MetaAnalysisModerator mam)
        {
            if (mam.Name == "Intervention") return "InterventionText";
            if (mam.Name == "Control") return "ControlText";
            if (mam.Name == "Outcome") return "OutcomeText";

            return "";
        }

        
        private string GetMetaforScript(MetaAnalysis ma, int OutcomesCount)
        {
            RCode = "if (!require(metafor))"
                            + Environment.NewLine + "{"
                            + Environment.NewLine + "library(metafor, lib.loc=\".\", verbose=TRUE)"
                            + Environment.NewLine + "}";

            string model = ", method='" + ma.GetStatisticalModelText() + "'";
            string verboseOutput = ", verbose=" + ma.Verbose.ToString();
            string measure = ", measure='" + ma.GetMetaAnalysisTypeText() + "'";
            string sigLevel = ", level=" + ma.SignificanceLevel.ToString();
            string decPlace = ", digits=" + ma.DecPlaces.ToString();

            //unified graphtitle names, to match the correct titles and plots
            string TitleFP = "Forest plot"
                , TitleFuPSE = "Funnel plot (Standard Error)"
                , TitleFuPSV = "Funnel plot (Sampling Variance)"
                , TitleFuPISE = "Funnel plot (Inverse Standard Error)"
                , TitleFuPISV = "Funnel plot (Inverse Sampling Variance)"
                , TitleRad = "Radial (Galbraith) Plot"
                , TitleNormQQ = "Normal QQ Plot (for selected statistical model)"
                , TitleBox = "Boxplot of effect size estimates"
                , TitleTaF = "Trim and fill";
            

            string knha = ma.KNHA ? ", knha=TRUE" : "";

            string slab = ", slab=studylabs";
            string mods = "";


            foreach (MetaAnalysisModerator mam in ma.MetaAnalysisModerators)
            {
                if (mam.IsSelected)
                {
                    if (mam.IsFactor)
                    {
                        if (mods == "")
                        {
                            mods = "relevel(factor(" + Regex.Replace(mam.Name, @"[^A-Za-z0-9]+", "") + "), ref='" +
                                Regex.Replace(mam.Reference, @"[^A-Za-z0-9]+", "") + "')";
                        }
                        else
                        {
                            mods += " + relevel(factor(" + Regex.Replace(mam.Name, @"[^A-Za-z0-9]+", "") + "), ref='" +
                                Regex.Replace(mam.Reference, @"[^A-Za-z0-9]+", "") + "')";
                        }
                    }
                    else
                    {
                        if (mods == "")
                        {
                            mods = Regex.Replace(mam.Name, @"[^A-Za-z0-9]+", "");
                        }
                        else
                        {
                            mods += " + " + Regex.Replace(mam.Name, @"[^A-Za-z0-9]+", "");
                        }
                    }
                }
            }
            
            

            if (mods != "")
            {
                mods = ", mods = ~ " + mods;
            }
            
            if (ma.GetMetaAnalysisTypeText() == "OR" || ma.GetMetaAnalysisTypeText() == "RR")
            {
                // take log of yi 
                RCode += Environment.NewLine + "res <- rma(log(yi), sei*sei, data=dat" + mods +
                            measure + knha + slab +
                            sigLevel + decPlace + verboseOutput +
                            model + ")" 
                            + Environment.NewLine + "Main_Summary <- capture.output(res)";
            }
            else
            {
                // just the same as before
                RCode += Environment.NewLine + "res <- rma(yi, sei*sei, data=dat" + mods +
                            measure + knha + slab +
                            sigLevel + decPlace + verboseOutput +
                            model + ")" 
                            + Environment.NewLine + "Main_Summary <- capture.output(res)";
            }
            ResultsLabels.Add("Main Summary");

            if (ma.FitStats)
            {
                RCode += Environment.NewLine + "Fit_Statistics <- capture.output(fitstats(res))";
                ResultsLabels.Add("Fit Statistics");
            }
            if (ma.Confint)
            {
                RCode += Environment.NewLine + "Confidence_Intervals <- capture.output(confint(res))";
                ResultsLabels.Add("Confidence Intervals");
            }
            if (ma.Egger)
            {
                RCode += Environment.NewLine + "Egger_regression_test_for_funnel_plot_asymmetry <- capture.output(regtest(res))";
                ResultsLabels.Add("Egger regression test for funnel plot asymmetry");
            }
            if (ma.RankCorr)
            {
                RCode += Environment.NewLine + "Rank_correlation_test_for_funnel_plot_asymmetry <- capture.output(ranktest(res))";
                ResultsLabels.Add("Rank correlation test for funnel plot asymmetry");
            }


            GraphsTitles.Add(TitleFP);//"Forest plot");
            if (ma.ShowFunnel)
            {
                GraphsTitles.Add(TitleFuPSE);// "Funnel plot (Standard Error)");
                GraphsTitles.Add(TitleFuPSV);//"Funnel plot (Sampling Variance)");
                GraphsTitles.Add(TitleFuPISE);//"Funnel plot (Inverse Standard Error)");
                GraphsTitles.Add(TitleFuPISV);//"Funnel plot (Inverse Sampling Variance)");
            }

            if (mods == "")
            {
                GraphsTitles.Add(TitleRad);//"Radial (Galbraith) Plot");
            }

            if (ma.ShowBoxplot)
            {
                GraphsTitles.Add(TitleNormQQ);//"Normal QQ Plot (for selected statistical model)");
                GraphsTitles.Add(TitleBox);//"Boxplot of effect size estimates");
            }
            
            string TrimFill = "";
            if (ma.TrimFill)
            {
                TrimFill = Environment.NewLine + "res.tf <- trimfill(res)"
                     + Environment.NewLine + "Trim_and_fill <- capture.output(trimfill(res.tf))"
                     + Environment.NewLine + "png(filename=\"" + TitleTaF.Replace(' ', '_').Replace("(", "").Replace(")", "") + "\", height = 480, width = 480, units = \"px\")"
                     + Environment.NewLine + "funnel(res.tf)"
                     + Environment.NewLine + "dev.off()";
                ResultsLabels.Add("Trim and fill");
                GraphsTitles.Add(TitleTaF);//"Trim and fill");
            }

            // Create a forest plot
            string annotate = ma.ShowAnnotations ? "" : ", annotate=FALSE";
            string annotateWeights = ma.ShowAnnotationWeights ? ", showweights=TRUE" : "";
            string addfit = ma.FittedVals ? "" : ", addfit=FALSE";
            string addcred = ma.CredInt ? ", addcred=TRUE" : "";
            
            string orderBy = "";
            /*
            if (cbOrderByValue.IsChecked.Value)
            {
                orderBy = (cbOrderByValueSelection.SelectedItem as ComboBoxItem).Tag.ToString();
                if (orderBy == "PRESENTED")
                    orderBy = "";
                else
                    orderBy = ", order='" + orderBy + "'";
            }
            else
            {
                orderBy = ", order=order(dat$" + (cbOrderByFieldSelection.SelectedItem as ComboBoxItem).Content.ToString() + ")";
            }
            */
            RCode += TrimFill;
            string xAxisTitle = ma.XAxisTitle == "" ? "" : ",xlab='" + ma.XAxisTitle + "'";
            string summaryEstimate = ma.SummaryEstimateTitle == "" ? "" : ",mlab='" + ma.SummaryEstimateTitle + "'";

            if (ma.ShowBoxplot)
            {
                RCode += Environment.NewLine + "png(filename=\"" + TitleBox.Replace(' ', '_').Replace("(", "").Replace(")", "") + "\", height = 480, width = 480, units = \"px\")"
                    + Environment.NewLine + "boxplot(dat$yi)"
                    + Environment.NewLine + "dev.off()";
                RCode += Environment.NewLine + "png(filename=\"" + TitleNormQQ.Replace(' ', '_').Replace("(", "").Replace(")", "") + "\", height = 480, width = 480, units = \"px\")"
                    + Environment.NewLine + "qqnorm(res)"
                    + Environment.NewLine + "dev.off()";
            }
            if (mods == "")
            {
                RCode += Environment.NewLine + "png(filename=\"" + TitleRad.Replace(' ', '_').Replace("(", "").Replace(")", "") + "\", height = 480, width = 480, units = \"px\")"
                    + Environment.NewLine + "radial(res)"
                    + Environment.NewLine + "dev.off()";
            }
            if (ma.ShowFunnel)
            {
                RCode += Environment.NewLine + "png(filename=\"" + TitleFuPISV.Replace(' ', '_').Replace("(", "").Replace(")", "") + "\", height = 480, width = 480, units = \"px\")" 
                    + Environment.NewLine + "funnel(res, yaxis='vinv', main='Inverse Sampling Variance')"
                    + Environment.NewLine + "dev.off()";
                RCode += Environment.NewLine + "png(filename=\"" + TitleFuPISE.Replace(' ', '_').Replace("(", "").Replace(")", "") + "\", height = 480, width = 480, units = \"px\")"
                    + Environment.NewLine + "funnel(res, yaxis='seinv', main='Inverse Standard Error')"
                    + Environment.NewLine + "dev.off()";
                RCode += Environment.NewLine + "png(filename=\"" + TitleFuPSV.Replace(' ', '_').Replace("(", "").Replace(")", "") + "\", height = 480, width = 480, units = \"px\")"
                    + Environment.NewLine + "funnel(res, yaxis='vi', main='Sampling Variance')"
                    + Environment.NewLine + "dev.off()";
                RCode += Environment.NewLine + "png(filename=\"" + TitleFuPSE.Replace(' ', '_').Replace("(", "").Replace(")", "") + "\", height = 480, width = 480, units = \"px\")"
                    + Environment.NewLine + "funnel(res, main='Standard Error')"
                    + Environment.NewLine + "dev.off()";
            }
            string atransf = "";
            if (ma.GetMetaAnalysisTypeText() == "OR" || ma.GetMetaAnalysisTypeText() == "RR")
            {
                atransf = ", atransf=exp";
            }
            int heightPixels = 112 + 50 + OutcomesCount * 20;
            //double heightInces = (double)heightPixels / 96;
            //double widthInces = (double)640 / 72;
            //double halfW = widthInces / 2;
            //double spacer = halfW / 3;
            //double yOffset = heightInces - (double)69 / 96;
            RCode += Environment.NewLine + "png(filename=\"" + TitleFP.Replace(' ', '_').Replace("(", "").Replace(")", "") + "\", height = " + heightPixels + ", width = 800, units = \"px\")"
                    + Environment.NewLine + "forest(res" + annotate + annotateWeights + addfit + addcred + 
                //xlim + alim + clim + ylim + at + steps +
                    orderBy +
                    xAxisTitle + summaryEstimate + atransf + ")"
                    + Environment.NewLine + "x1 <- par(\"usr\")[1]"
                    + Environment.NewLine + "x2 <- par(\"usr\")[2]"
                    + Environment.NewLine + "totX <- x2 - x1"
                    + Environment.NewLine + "eigthX <- totX/8"
                    + Environment.NewLine + "text(x1 + eigthX , " + (OutcomesCount + 1.7).ToString() + ", \"Studies\", adj = 1, font=2, cex=1)"
                    + Environment.NewLine + "text(x2 - eigthX , " + (OutcomesCount + 1.7).ToString() + ", \"Values\", adj = 0, font=2, cex=1)"
                    + Environment.NewLine + "title(\"" + ma.Title + "\")"
                    + Environment.NewLine + "dev.off()";
            return RCode;
        }

        private string GetNetmetaScript(MetaAnalysis ma, int OutcomesCount)
        {
            string SmallValues = ", small.values='bad'";
            if (!ma.LargeValuesGood)
            {
                SmallValues = ", small.values='good'";
            }
            
            RCode = "if (!require(netmeta))"
                            + Environment.NewLine + "{"
                            + Environment.NewLine + "library(netmeta, lib.loc=\".\", verbose=TRUE)"
                            + Environment.NewLine + "}";

            string model = ma.NMAStatisticalModel == 1 ? ", comb.random=TRUE" : "";
            RCode += Environment.NewLine + "net1 <- netmeta(yi, sei, Intervention, Comparison, studylabs, data=dat" + model + 
                (ma.GetMetaAnalysisTypeText() == "OR" && ma.Exponentiated == false ? "" : ", sm='" + ma.GetMetaAnalysisTypeText() + "'") + ")";
            RCode += Environment.NewLine + "Main_Results <- capture.output(summary(net1))";
            ResultsLabels.Add("Main Results");

            RCode += Environment.NewLine + "Intervention_Ranking <- capture.output(netrank(net1" + SmallValues + "))";
            ResultsLabels.Add("Intervention Ranking");

            RCode += Environment.NewLine + "nm1 <- netmeasures(net1)"
                + Environment.NewLine + "Network_Characteristics <- capture.output(nm1)";
            ResultsLabels.Add("Network Characteristics");

            GraphsTitles.Add("Network Graph");
            RCode += Environment.NewLine + "png(filename=\"" + "Network_Graph" + "\", height = 431, width = 804, units = \"px\")"
                    + Environment.NewLine + "netgraph(net1)"
                    + Environment.NewLine + "dev.off()";

            GraphsTitles.Add("Forest Plot");
            RCode += Environment.NewLine + "png(filename=\"" + "Forest_Plot" + "\", height = 431, width = 804, units = \"px\")"
                    + Environment.NewLine + "forest(net1, ref='" + ma.NMAReference + "')"
                    + Environment.NewLine + "dev.off()";

            GraphsTitles.Add("Heat Plot");
            RCode += Environment.NewLine + "png(filename=\"" + "Heat_Plot" + "\", height = 431, width = 804, units = \"px\")"
                    + Environment.NewLine + "netheat(net1" + model + ")"
                    + Environment.NewLine + "dev.off()";

            GraphsTitles.Add("Minimal parallelism versus mean path length");
            RCode += Environment.NewLine + "png(filename=\"" + "Minimal_parallelism_versus_mean_path_length" + "\", height = 431, width = 804, units = \"px\")"
                    + Environment.NewLine + "plot(nm1$meanpath, nm1$minpar, pch='', xlab='Mean path length', ylab='Minimal parallelism')"
                    + Environment.NewLine + "text(nm1$meanpath, nm1$minpar, names(nm1$meanpath), cex=0.8)"
                    + Environment.NewLine + "dev.off()";

            return RCode;
        }

        

#endif
    }
}
