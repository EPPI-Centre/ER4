using BusinessLibrary.BusinessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDatabasesMVC.ViewModels
{
    public class FairTopicVM
    {
        public FairTopicVM(WebDbReviewSetsList sets, int setId, AttributeSet topic, string baseImgUrl)
        {
            ReviewSets = sets;
            PieChartSetId = setId;
            TopicAttribute = topic;
            ImagesBaseUrl = baseImgUrl;
        }
        public WebDbReviewSetsList ReviewSets { get; set; }
        public int PieChartSetId { get; set; }
        public AttributeSet TopicAttribute { get; set; }
        public string ImagesBaseUrl { get; set; }
    }
}
