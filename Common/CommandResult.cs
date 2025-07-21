namespace SteadyGrowth.Web.Common
{
    public class CommandResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        private CommandResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static CommandResult Success(string message = "Operation successful.")
        {
            return new CommandResult(true, message);
        }

        public static CommandResult Fail(string message = "Operation failed.")
        {
            return new CommandResult(false, message);
        }
    }
}
