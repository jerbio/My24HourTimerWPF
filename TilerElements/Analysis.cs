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
        public static readonly double DefaultActivationRatio = 0.30;
        double _CompletionRate = Analysis.DefaultActivationRatio;
        DateTimeOffset _LastUpdate = new DateTimeOffset();
        protected string _Id = Guid.NewGuid().ToString();
        protected TilerUser _User;
        [Key]
        public virtual string Id
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
        [ForeignKey("Id")]
        public virtual TilerUser User_DB {
            get
            {
                return _User;
            } 
            set
            {
                _User = value;
            }
        }


        public void setComplentionRate(double completionRate, DateTimeOffset currentTime)
        {
            _CompletionRate = completionRate;
            _LastUpdate = currentTime;
        }


        public void setUser(TilerUser user)
        {
            this._User = user;
        }

        public virtual TilerUser User
        {
            get
            {
                return _User;
            }
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

        public static Analysis generateAnalysisObject(TilerUser tilerUser)
        {
            Analysis retValue = new Analysis();
            return retValue;
        }

        
    }
}
