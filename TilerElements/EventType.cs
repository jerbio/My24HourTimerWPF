using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlchemyAPI;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace TilerElements
{
    public class Classification
    {
        Vicinity Placement = Vicinity.None;
        EnergyDifferential Succubus = EnergyDifferential.None;
        Leisure LeisureType = Leisure.None;
        bool Initialized = true;
        public Classification(Vicinity LocationPlacement, EnergyDifferential StrengthDelta, Leisure RelaxationData, bool InitializedData)
        {
            LeisureType = RelaxationData;
            Succubus = StrengthDelta;
            Placement = LocationPlacement;
            Initialized = InitializedData;
        }

        public Classification()
        {

        }
        public Classification createCopy()
        {
            Classification RetValue = new Classification();
            RetValue.Initialized = this.Initialized;
            RetValue.LeisureType = this.LeisureType;
            RetValue.Placement = this.Placement;
            RetValue.Succubus = this.Succubus;
            return RetValue;
        }
        
        async internal Task InitializeClassification(string NameOfEvent)
        {
            AlchemyAPI.AlchemyAPI AlchemyObj = new AlchemyAPI.AlchemyAPI();
            AlchemyObj.SetAPIKey("c73e93af01a6cb7728a6b90887c266a8881b7665");
            string textTaxonomy = "";
            string sentimentAnalysis = "";
            XmlDocument textTaxonomyDoc = new XmlDocument();
            XmlDocument textSetimentDoc = new XmlDocument();
            try
            {
                textTaxonomy = AlchemyObj.TextGetRankedTaxonomy(NameOfEvent);
                sentimentAnalysis = AlchemyObj.TextGetTextSentiment(NameOfEvent);
                textTaxonomyDoc.LoadXml(textTaxonomy);
                textSetimentDoc.LoadXml(sentimentAnalysis);
            }
            catch
            {

            }
        }
    }

    public enum Vicinity { Indoor, None, Outdoor }
    public enum EnergyDifferential { Lethargic, None, Active }
    public enum Leisure { Casual, None, Business }
}
