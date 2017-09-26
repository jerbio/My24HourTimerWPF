using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
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
            ITextAnalyticsAPI client = new TextAnalyticsAPI();
            client.AzureRegion = AzureRegions.Westus;
            client.SubscriptionKey = "38669a98f64148c0948129f762b86153";

            LanguageBatchResult result = client.DetectLanguage(
                    new BatchInput(
                        new List<Input>()
                        {
                          new Input("1", "This is a document written in English."),
                          new Input("2", NameOfEvent),
                          new Input("3", "这是一个用中文写的文件")
                        }));
            foreach (var document in result.Documents)
            {
                Console.WriteLine("Document ID: {0} , Language: {1}", document.Id, document.DetectedLanguages[0].Name);
            }

            // Getting key-phrases
            Console.WriteLine("\n\n===== KEY-PHRASE EXTRACTION ======");

            KeyPhraseBatchResult result2 = client.KeyPhrases(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput("ja", "1", "猫は幸せ"),
                          new MultiLanguageInput("de", "2", "Fahrt nach Stuttgart und dann zum Hotel zu Fu."),
                          new MultiLanguageInput("en", "3", "My cat is stiff as a rock."),
                          new MultiLanguageInput("es", "4", "A mi me encanta el fútbol!")
                        }));


            // Printing keyphrases
            foreach (var document in result2.Documents)
            {
                Console.WriteLine("Document ID: {0} ", document.Id);

                Console.WriteLine("\t Key phrases:");

                foreach (string keyphrase in document.KeyPhrases)
                {
                    Console.WriteLine("\t\t" + keyphrase);
                }
            }

            // Extracting sentiment
            Console.WriteLine("\n\n===== SENTIMENT ANALYSIS ======");

            SentimentBatchResult result3 = client.Sentiment(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput("en", "0", NameOfEvent)
                        }));


            // Printing sentiment results
            foreach (var document in result3.Documents)
            {
                Console.WriteLine("Document ID: {0} , Sentiment Score: {1:0.00}", document.Id, document.Score);
            }
        }
    }

    public enum Vicinity { Indoor, None, Outdoor }
    public enum EnergyDifferential { Lethargic, None, Active }
    public enum Leisure { Casual, None, Business }
}
