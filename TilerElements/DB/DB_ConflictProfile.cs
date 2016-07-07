using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    class DB_ConflictProfile : ConflictProfile
    {

        virtual public int Type
        {
            get
            {
                return TypeOfConflict;
            }
            set
            {
                TypeOfConflict = value;
            }
        }

        public int Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
            }
        }

        virtual public bool Flag
        {
            get
            {
                return ConflictFlag;
            }
            set
            {
                ConflictFlag = value;
            }
        }
    }
}
