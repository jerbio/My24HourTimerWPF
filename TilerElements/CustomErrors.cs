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
            BeginningOfTimeError = 2,
            UnknownError = 500,
            UserEmailNotMatchingSubEvent = 10000002,
            TimelineFittabilityIssue = 40000000,
            cannotFitWithinTimeline = 40000001,
            procrastinationPastDeadline = 40000002,
            procrastinationBeforeNow = 40000003,
            procrastinationAllSubeventsCannotFitDeadline = 40000004,
            eventUpdateBeforeNow = 40000005,
            restrictedTimeLineUpdateInValid = 40000006,
            TilerConfigEventError = 40000007,
            TilerConfig_Zero_SplitCount= 40000008,
            TilerConfig_Repeat_Rigid = 40000009,
            Invalid_Event_OR_Tiler_ID = 40000010,
            Tile_Or_Event_ID_Cannot_Be_Found = 40000011,
            Repeated_Tile_Is_Not_Current_Tile = 40000012,
            TilerConfig_Repeat_Third_Party = 400000013,
            TilerConfig_Repeat_Procrastinate_All = 400000014,
            Creation_Config_RepeatEnd_Earlier_Than_RepeatStart = 400000015,
            Creation_Config_End_Earlier_Than_Start = 400000016,
            Procrastinate_All_Cannot_Clear_Past = 400000017,
            Resume_Event_Paused_Event_Id_is_Null = 400000018,
            Resume_Event_Cannot_Resume_Not_Paused_SubEvent = 400000019,
            SetAsNow_Cannot_Set_In_Active_Tile_AsNow = 400000020,
            Pause_Event_There_Is_No_Current_To_Pause = 400000021,
            Pause_Event_Cannot_Pause_ProcrastinateAllEvent = 400000022,
            Resume_Event_Cannot_Outside_Deadline_Of_CalendarEvent = 400000023,
            restrictionProfileNonvaiable = 400000024,
            Recurring_Tile_Extension_Has_To_Be_Greater_Than_Current_Timeline= 400000025,
            Preview_Calendar_Type_Not_Supported = 50000001,
            Preview_Calendar_Not_Enough_Data_For_Preview = 50000002
        };
        static Dictionary<Errors, String> errorMessage = new Dictionary<Errors, string>()
        {
            {Errors.success, "SUCCESS" },
            {Errors.UnknownError, "Unknown Error" },
            {Errors.BeginningOfTimeError, "Invalid time provided, did you fail to set the time to a valid data" },
            {Errors.cannotAuthenticate, "Failed to authenticate user account" },
            {Errors.cannotFitWithinTimeline, "Cannot fit the sub event within the timeline" },
            {Errors.procrastinationPastDeadline, "Procrastination of the tile will put it past the deadline of the tile" },
            {Errors.procrastinationAllSubeventsCannotFitDeadline, "After Procrastination of the tile all subevents won't be able to fit within the newly readjusted timeline post procrastination " },
            {Errors.Procrastinate_All_Cannot_Clear_Past, "You cannot clear time chunk in the past, try updating an event in the past" },
            {Errors.Resume_Event_Paused_Event_Id_is_Null, "Cannot resume events because paused event id is null" },
            {Errors.Resume_Event_Cannot_Resume_Not_Paused_SubEvent, "Cannot resume tile that is not paused" },
            {Errors.Resume_Event_Cannot_Outside_Deadline_Of_CalendarEvent, "Cannot resume tile because it is outside calendar range" },
            {Errors.SetAsNow_Cannot_Set_In_Active_Tile_AsNow, "Cannot set an inactive subevent as now, is it disabled or completed?" },
            {Errors.Pause_Event_There_Is_No_Current_To_Pause, "There is no current tile to be paused, a tile can only be paused if it is interferring with the current time" },
            {Errors.Pause_Event_Cannot_Pause_ProcrastinateAllEvent, "Cannot pause a procrastinate all tile" },
            {Errors.procrastinationBeforeNow, "Cannot go back in time, if you're from the future, please tell us how tiler is doing" },
            {Errors.eventUpdateBeforeNow, "The select time slot for the schedule change does is before the current time, try loading a schedule which includes the current time"},
            {Errors.TilerConfig_Zero_SplitCount, "Cannot create or modify with a final state with zero events"},
            {Errors.restrictedTimeLineUpdateInValid, "The restricted time line update is invalid"},
            {Errors.TilerConfig_Repeat_Rigid, "Cannot repeat a rigid event" },
            {Errors.Invalid_Event_OR_Tiler_ID, "Invalid tiler or event id provided, check if the ID is in the right format" },
            {Errors.Tile_Or_Event_ID_Cannot_Be_Found, "Tile or Event with the provided ID cannot be found" },
            {Errors.Repeated_Tile_Is_Not_Current_Tile, "Cannot repeat Tile that is not current active tile" },
            {Errors.TilerConfig_Repeat_Third_Party, "Cannot repeat Third party event" },
            {Errors.TilerConfig_Repeat_Procrastinate_All, "Cannot repeat Procrastinate All Tile" },
            {Errors.Creation_Config_RepeatEnd_Earlier_Than_RepeatStart, "Repetition end time has to be later than repetition start " },
            {Errors.Creation_Config_End_Earlier_Than_Start, "End time has to be later than start " },
            {Errors.Preview_Calendar_Type_Not_Supported, "Selected Calendar is not supported for manipulation" },
            {Errors.Preview_Calendar_Not_Enough_Data_For_Preview, "Tiler cannot confidently make prediction" },
            {Errors.UserEmailNotMatchingSubEvent, "Event ids do not match the event assosciated emails. Check if there is an email for each event id" },
            {Errors.restrictionProfileNonvaiable, "The timeline for tile is non viable check to see that you're scheduling the tile within a time frame that can contain tile"}

        };
        string ErrorMessage;
        TilerEvent TilerEvent;
        int ErrorCode;
        /* Error Code 0: No Error
         * Error Code 5: Set Sub event as Now was selected however, The sub event will exceed the bounds of the CalendarEvent
         * 10000000<=code => Invalid Request Made
         * 20000000<=Code => Log control issue
         * 30000000<=Code => Database control issue
         * 40000000<=Code => Schedule Maniputlation Error issue
         * 50000000<=Code => Prediction Maniputlation Error issue
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

        public override string Message
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

        public static string getErrorMessage(int errorCode)
        {
            Errors error = (Errors)Enum.Parse(typeof(Errors), errorCode.ToString());
            return getErrorMessage(error);
        }

        public static bool isValidErrorCode(int errorCode)
        {
            bool retValue = Enum.IsDefined(typeof(Errors), errorCode);
            return retValue;
        }


        public static bool errorCodeHasMessage(CustomErrors errorCode)
        {
            bool retValue = Enum.IsDefined(typeof(Errors), errorCode);
            return retValue;
        }

        public static bool errorCodeHasMessage(int errorCode)
        {
            bool retValue = isValidErrorCode(errorCode) && errorMessage.ContainsKey((Utility.ParseEnum<Errors>(errorCode.ToString()) ));
            return retValue;
        }

        
    }
}