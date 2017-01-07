using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerElements
{
    public class CustomErrors: Exception
    {
        string ErrorMessage;
        int ErrorCode;
        /* Error Code 0: No Error
         * Error Code 5: Set Sub event as Now was selected however, The sub event will exceed the bounds of the CalendarEvent
         * 10001000<=code => LoginCredential issue
         * 20000000<=Code => Log control issue
         * 30000000<=Code => Database control issue
         * 40000000<=Code => Schedule Maniputlation Error issue
         */

        public CustomErrors(string MessagEntry, int ErrorCode = 0)
        {
            ErrorMessage = MessagEntry;
            this.ErrorCode = ErrorCode;
        }

        public string Message
        {
            get
            {
                return ErrorMessage;
            }
        }

        public int Code
        {
            get
            {
                return ErrorCode;
            }
        }
    }
}