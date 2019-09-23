using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerElements
{
    public class CustomErrors: Exception
    {
        public enum Errors {
            success = 0,
            cannotAuthenticate = 1,
            UnknownError = 500,
            TimelineFittabilityIssue = 40000000,
            cannotFitWithinTimeline = 40000001,
            procrastinationPastDeadline = 40000002,
            procrastinationBeforeNow = 40000003,
            eventUpdateBeforeNow = 40000005,
            restrictedTimeLineUpdateInValid = 40000006,
            TilerConfigEventError = 50000000,
            TilerConfig_Zero_SplitCount= 50000001
        };
        static Dictionary<Errors, String> errorMessage = new Dictionary<Errors, string>()
        {
            {Errors.success, "SUCCESS" },
            {Errors.UnknownError, "Unknown Error" },
            {Errors.cannotAuthenticate, "Failed to authenticate user account" },
            {Errors.cannotFitWithinTimeline, "Cannot fit the sub event within the timeline" },
            {Errors.procrastinationPastDeadline, "Procrastination of the tile will put it past the deadline of the tile" },
            {Errors.procrastinationBeforeNow, "Cannot go back in time, if you're from the future, please tell us how tiler is doing" },
            {Errors.eventUpdateBeforeNow, "The select time slot for the schedule change does is before the current time, try loading a schedule which includes the current time"},
            {Errors.TilerConfig_Zero_SplitCount, "Cannot create or modify with a final state with zero events"},
            {Errors.restrictedTimeLineUpdateInValid, "The restricted time line update is invalid"}
        };
        string ErrorMessage;
        TilerEvent TilerEvent;
        int ErrorCode;
        /* Error Code 0: No Error
         * Error Code 5: Set Sub event as Now was selected however, The sub event will exceed the bounds of the CalendarEvent
         * 10001000<=code => LoginCredential issue
         * 20000000<=Code => Log control issue
         * 30000000<=Code => Database control issue
         * 40000000<=Code => Schedule Maniputlation Error issue
         */

        public CustomErrors(int ErrorCode, string MessagEntry=null, TilerEvent tilerEvent = null)
        {
            this.ErrorCode = ErrorCode;
            ErrorMessage = MessagEntry;
            this.TilerEvent = tilerEvent;
        }

        public CustomErrors(Errors status, string MessagEntry = null, TilerEvent tilerEvent = null)
        {
            this.ErrorCode = (int)status;
            ErrorMessage = MessagEntry;
            this.TilerEvent = tilerEvent;
        }

        public CustomErrors(string MessagEntry, int ErrorCode = 0, TilerEvent tilerEvent = null)
        {
            ErrorMessage = MessagEntry;
            this.ErrorCode = ErrorCode;
            this.TilerEvent = tilerEvent;
        }

        public string Message
        {
            get
            {
                
                if(String.IsNullOrEmpty(ErrorMessage))
                {
                    return getErrorMessage(ErrorCode);
                } else
                {
                    return ErrorMessage;
                }
                
            }
        }

        public int Code
        {
            get
            {
                return ErrorCode;
            }
        }

        public static string getErrorMessage(Errors errorCode)
        {
            if(errorMessage.ContainsKey(errorCode))
            {
                return errorMessage[errorCode];
            }
            else
            {
                return errorMessage[Errors.UnknownError];
            }
            
        }

        static string getErrorMessage(int errorCode)
        {
            Errors error = (Errors)Enum.Parse(typeof(Errors), errorCode.ToString());
            return getErrorMessage(error);
        }
    }
}