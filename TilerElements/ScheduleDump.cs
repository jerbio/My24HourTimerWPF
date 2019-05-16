using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TilerElements
{
    public class ScheduleDump
    {
        protected string _Id = Guid.NewGuid().ToString();
        protected string _UserId;
        protected string _ScheduleXmlString { get; set; }
        public DateTimeOffset DateOfCreation { get; set; } = DateTimeOffset.UtcNow;
        [Required]
        public DateTimeOffset StartOfDay { get; set; }
        /// <summary>
        /// This is the time derived from the reference now object
        /// </summary>
        [Required]
        public DateTimeOffset ReferenceNow { get; set; }
        protected string _Notes { get; set; }
        protected TilerUser _User { get; set; }

        public ScheduleDump()
        {

        }
        public ScheduleDump (string xmlString)
        {
            _ScheduleXmlString = xmlString;
        }


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

        public string Notes
        {
            get
            {
                return _Notes;
            }
            set
            {
                _Notes = value;
            }
        }
        [Required]
        public string UserId
        {
            get
            {
                return _UserId;
            }
            set
            {
                _UserId = value;
            }
        }

        [Required, ForeignKey("UserId")]
        public TilerUser User {
            get {
                return _User;
            } set {
                _User = value;
            }
        }

        public void updaeteId()
        {
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash);
                _Id = hash;
            }
        }
        public string ScheduleXmlString
        {
            get
            {
                return _ScheduleXmlString;
            }
            set
            {
                _ScheduleXmlString = value;
                //updaeteId();
            }
        }
        [NotMapped]
        public XmlDocument XmlDoc
        {
            set
            {
                _ScheduleXmlString = value.InnerXml;
                string timeInString = value.SelectSingleNode("/ScheduleLog/referenceDay").InnerText;
                StartOfDay = DateTimeOffset.Parse(timeInString);
                XmlNode lastScheduleUpdateNode = value.SelectSingleNode("/ScheduleLog/lastUpdated");
                if(lastScheduleUpdateNode != null) {
                    string lastScheduleUpdate = value.SelectSingleNode("/ScheduleLog/lastUpdated").InnerText;
                    this.ReferenceNow = DateTimeOffset.Parse(lastScheduleUpdate);
                } else
                {
                    StartOfDay = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
                }
            }
            get
            {
                XmlDocument retValue = new XmlDocument();
                retValue.LoadXml(ScheduleXmlString);
                return retValue;
            }
        }

        string GetMd5Hash(MD5 md5Hash)
        {
            string input = ScheduleXmlString;

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }


    }
}
