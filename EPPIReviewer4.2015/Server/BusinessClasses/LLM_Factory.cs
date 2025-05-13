namespace BusinessLibrary.BusinessClasses
{
    public static class LLM_Factory
    {
        public static LLMRobotCommand GetRobot(RobotCoderReadOnly robot, int reviewSetId, Int64 itemId, bool onlyCodeInTheRobotName = true, bool lockTheCoding = true, bool useFullTextDocument = false)
        {
            RobotOpenAICommand res = new RobotOpenAICommand(robot, reviewSetId, itemId, onlyCodeInTheRobotName, lockTheCoding, useFullTextDocument);
            return res;
        }
        public static LLMRobotCommand GetRobot(RobotCoderReadOnly robot, int reviewSetId, Int64 itemId, bool isLastInBatch, int JobId, int robotContactId, int reviewId,
            int JobOwnerId, bool onlyCodeInTheRobotName = true, bool lockTheCoding = true, bool useFullTextDocument = false, string docsList = "")
        {
            RobotOpenAICommand res = new RobotOpenAICommand(robot, reviewSetId, itemId, isLastInBatch, JobId, robotContactId, reviewId,
            JobOwnerId, onlyCodeInTheRobotName, lockTheCoding, useFullTextDocument, docsList,
            robot.EndPoint, AzureSettings.RobotAPIKeyByRobotName(robot.RobotName));
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