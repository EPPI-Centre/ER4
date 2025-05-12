namespace BusinessLibrary.BusinessClasses
{
    public static class LLM_Factory
    {
        public static LLMRobotCommand GetRobot(string robotName, int reviewSetId, Int64 itemId, Int64 itemDocumentId, bool onlyCodeInTheRobotName = true, bool lockTheCoding = true, bool useFullTextDocument = false)
        {
            RobotOpenAICommand res = new RobotOpenAICommand(robotName, reviewSetId, itemId, itemDocumentId, onlyCodeInTheRobotName, lockTheCoding, useFullTextDocument);
            return res;
        }
        public static LLMRobotCommand GetRobot(string robotName, int reviewSetId, Int64 itemId, Int64 itemDocumentId, bool isLastInBatch, int JobId, int robotContactId, int reviewId,
            int JobOwnerId, bool onlyCodeInTheRobotName = true, bool lockTheCoding = true, bool useFullTextDocument = false, string docsList = "",
            string ExplicitEndpoint = "", string ExplicitEndpointKey = "")
        {
            RobotOpenAICommand res = new RobotOpenAICommand(robotName, reviewSetId, itemId, itemDocumentId, isLastInBatch, JobId, robotContactId, reviewId,
            JobOwnerId, onlyCodeInTheRobotName, lockTheCoding, useFullTextDocument, docsList,
            ExplicitEndpoint, ExplicitEndpointKey);
            return res;
        }
    }
    public interface ILLMRobot
    {
        string ReturnMessage { get; }
    }
    [Serializable]
    public abstract class LLMRobotCommand: LongLastingFireAndForgetCommand, ILLMRobot
    {
        protected string _message = "";
        public string ReturnMessage
        {
            get { return _message; }
        }
    }
}