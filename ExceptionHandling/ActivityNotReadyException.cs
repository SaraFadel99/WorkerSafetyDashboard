namespace WorkerSafetyDashboard.ExceptionHandling
{
    public class ActivityNotReadyException : Exception
    {
        public ActivityNotReadyException(string activityId)
            : base($"Activity {activityId} not yet queryable") { }
    }

    public class TaskFailedException : Exception
    {
        public TaskFailedException(string message) : base(message) { }
    }

    public class TaskTimeoutException : Exception
    {
        public TaskTimeoutException(string message) : base(message) { }
    }
}
