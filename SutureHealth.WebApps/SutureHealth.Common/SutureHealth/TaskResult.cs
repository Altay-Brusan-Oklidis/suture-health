namespace SutureHealth
{
    public class TaskResult
    {
        public string Message { get; }
        public bool Succeeded { get; }

        private TaskResult(bool succeeded, string message = null)
        {
            Message = message;
            Succeeded = succeeded;
        }

        public static TaskResult Failed(string message = null) => new(false, message);
        public static TaskResult Success(string message = null) => new(true, message);
    }
}