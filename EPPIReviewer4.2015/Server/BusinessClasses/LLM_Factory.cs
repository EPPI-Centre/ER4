using Csla;
using Csla.Serialization;
using System;

namespace BusinessLibrary.BusinessClasses
{
#if CSLA_NETCORE
    public static class LLM_Factory
    {
        public static LLMRobotCommand GetRobot(RobotCoderReadOnly robot, int reviewSetId, Int64 itemId, bool onlyCodeInTheRobotName = true, 
            bool lockTheCoding = true, bool useFullTextDocument = false, int creditId = 0)
        {
            RobotOpenAICommand res = new RobotOpenAICommand(robot, reviewSetId, itemId, onlyCodeInTheRobotName, lockTheCoding, useFullTextDocument, creditId);
            return res;
        }

        public static LLMRobotCommand GetRobot(RobotCoderReadOnly robot, int reviewSetId, Int64 itemId, bool isLastInBatch, int JobId, int robotContactId, int reviewId,
            int JobOwnerId, bool onlyCodeInTheRobotName = true, bool lockTheCoding = true, bool useFullTextDocument = false, string docsList = "", ReviewSet? reviewSetForPrompts = null, string cachedPrompt = ""
            , int creditId = 0)
        {
            RobotOpenAICommand res = new RobotOpenAICommand(robot, reviewSetId, itemId, isLastInBatch, JobId, robotContactId, reviewId,
            JobOwnerId, onlyCodeInTheRobotName, lockTheCoding, useFullTextDocument, docsList,
            robot.EndPoint, AzureSettings.RobotAPIKeyByRobotName(robot.RobotName), creditId);
            if (reviewSetForPrompts != null) res.ReviewSetForPrompts = reviewSetForPrompts;
            if (cachedPrompt != "") res.CachedPrompt = cachedPrompt;
            return res;
        }
    }
#endif
    public interface ILLMRobot
    {
        RobotCoderReadOnly RobotCoder { get; }
        string ReturnMessage { get; }
    }
    [Serializable]
    public abstract class LLMRobotCommand: LongLastingFireAndForgetCommand<LLMRobotCommand>, ILLMRobot
    {
        public static readonly PropertyInfo<RobotCoderReadOnly> RobotCoderProperty = RegisterProperty<RobotCoderReadOnly>(new PropertyInfo<RobotCoderReadOnly>("RobotCoder", "RobotCoder"));
        public RobotCoderReadOnly RobotCoder
        {
            get { return ReadProperty(RobotCoderProperty); }
            set { LoadProperty(RobotCoderProperty, value); }
        }

        public static readonly PropertyInfo<ReviewSet> ReviewSetForPromptsProperty = RegisterProperty<ReviewSet>(new PropertyInfo<ReviewSet>("ReviewSetForPrompts", "ReviewSetForPrompts", null));
        public ReviewSet ReviewSetForPrompts
        {
            get { return ReadProperty(ReviewSetForPromptsProperty); }
            set { LoadProperty(ReviewSetForPromptsProperty, value); }
        }
        public static readonly PropertyInfo<string> CachedPromptProperty = RegisterProperty<string>(new PropertyInfo<string>("CachedPrompt", "CachedPrompt", ""));
        public string CachedPrompt
        {
            get { return ReadProperty(CachedPromptProperty); }
            set { LoadProperty(CachedPromptProperty, value); }
        }
        public static readonly PropertyInfo<int> CreditIdProperty = RegisterProperty<int>(new PropertyInfo<int>("CreditId", "CreditId"));
        public int CreditId
        {
            get { return ReadProperty(CreditIdProperty); }
            set { LoadProperty(CreditIdProperty, value); }
        }
        protected string _message = "";
        public string ReturnMessage
        {
            get { return _message; }
        }
    }
}