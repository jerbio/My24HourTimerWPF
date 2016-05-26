using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using TilerElements;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    public class DB_UserActivity : UserActivity
    {
        public DB_UserActivity(ReferenceNow triggerTime, ActivityType type):base(triggerTime, type)
        {

        }

        public DB_UserActivity(ReferenceNow referenceNow, ActivityType type, IEnumerable<String> ids = null): base(referenceNow.constNow, type, ids)
        {

        }

        public DB_UserActivity(DateTimeOffset triggerTime, ActivityType type, IEnumerable<String> ids = null) : base(triggerTime, type, ids)
        {

        }
        public DB_UserActivity(DateTimeOffset triggerTime, ActivityType type) : base(triggerTime, type) { }

        public DB_UserActivity(UserActivity activity) : base(activity.ActivityTriggerTime, activity.TriggerType, activity.eventIds) { }

        public DB_UserActivity():base(null, ActivityType.None) { }

        public string ToXML()
        {
            XmlDocument xmlDoc = new XmlDocument();   //Represents an XML document, 
                                                      // Initializes a new instance of the XmlDocument class.          
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
            // Creates a stream whose backing store is memory. 
            using (MemoryStream xmlStream = new MemoryStream())
            {
                xmlSerializer.Serialize(xmlStream, this);
                xmlStream.Position = 0;
                //Loads the XML document from the specified string.
                xmlDoc.Load(xmlStream);
                return xmlDoc.DocumentElement.InnerXml;
            }
        }

        public static DB_UserActivity LoadFromXMLString(string xmlText)
        {
            var stringReader = new System.IO.StringReader(xmlText);
            var serializer = new XmlSerializer(typeof(DB_UserActivity));
            return serializer.Deserialize(stringReader) as DB_UserActivity;
        }

        [XmlElement("TriggerTimeForEvent")]
        public string TriggerTimeToString
        {
            get { return TriggerTimeForEvent.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"); }
            set { TriggerTimeForEvent = DateTimeOffset.Parse(value); }
        }


        [XmlElement("Type")]
        public string TriggerTypeToString
        {
            get { return Type.ToString(); }
            set { Type = (ActivityType) Enum.Parse(typeof(ActivityType), value); }
        }

        [XmlElement("Ids")]
        public string affectedIds
        {
            get { return String.Join(",", updatedIds); }
            set { updatedIds = value.Split(',').ToList(); }
        }
    }
}
