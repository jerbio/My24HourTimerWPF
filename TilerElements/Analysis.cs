using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class Analysis
    {
        protected string _Id = Guid.NewGuid().ToString();
        double _CompletionRate = -1;
        double _AverageSubEventDuration = -1;
        DateTimeOffset _LastUpdate = new DateTimeOffset();

        TilerUser _User { get; set; }
        virtual public string UserId { get; set; }
        public TilerUser User
        {
            get
            {
                return _User;
            }
        }
        [ForeignKey("UserId")]
        public TilerUser User_DB
        {
            set
            {
                _User = value;
            }

            get
            {
                return _User;
            }
        }

        public void setComplentionRate(double completionRate, DateTime currentTime)
        {
            _CompletionRate = completionRate;
            _LastUpdate = currentTime;
        }

        public DateTimeOffset LastUpdate
        {
            get
            {
                return _LastUpdate;
            }
        }

        public long LastUpdate_DB
        {
            get
            {
                return _LastUpdate.ToUnixTimeMilliseconds();
            }

            set
            {
                _LastUpdate = DateTimeOffset.FromUnixTimeMilliseconds(value);
            }
        }

        public double CompletionRate
        {
            get
            {
                return _CompletionRate;
            }
        }

        public double CompletionRate_DB
        {
            get
            {
                return _CompletionRate;
            }

            set
            {
                _CompletionRate = value;
            }
        }

        public double AverageSubeventDuration_DB
        {
            get
            {
                return _AverageSubEventDuration;
            }

            set
            {
                _AverageSubEventDuration = value;
            }
        }


        [Key]
        public string Id
        {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
            }
        }
    }
}
