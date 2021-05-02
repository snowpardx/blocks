using System;

namespace CommonTypes
{
    public class HandledException: Exception
    {
        public Exception innerException;
        public HandledException(Exception e)
        {
            innerException = e;
        }
    }
}
