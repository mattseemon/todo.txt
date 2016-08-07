using System;

namespace Seemon.Todo.Models
{
    [Serializable]
    internal class TaskException : Exception
    {
        public TaskException(string message) 
            : base(message)
        {
        }

        public TaskException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}