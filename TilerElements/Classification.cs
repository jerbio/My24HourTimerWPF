using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlchemyAPI;
using System.Threading.Tasks;

namespace TilerElements
{
    public class Classification
    {
        protected string ID = Guid.NewGuid().ToString();   
        protected Vicinity _Placement = Vicinity.None;
        protected EnergyDifferential _Succubus = EnergyDifferential.None;
        protected Leisure _LeisureType = Leisure.None;
        protected bool _Initialized = true;
        protected ClassificationInterpreter Deduction;
#region Constructor
        public Classification(Vicinity LocationPlacement, EnergyDifferential StrengthDelta, Leisure RelaxationData, bool InitializedData)
        {
            _LeisureType = RelaxationData;
            _Succubus = StrengthDelta;
            _Placement = LocationPlacement;
            _Initialized = InitializedData;
        }

        public Classification()
        {

        }
        public Classification createCopy()
        {
            Classification RetValue = new Classification();
            RetValue._Initialized = this._Initialized;
            RetValue._LeisureType = this._LeisureType;
            RetValue._Placement = this._Placement;
            RetValue._Succubus = this._Succubus;
            return RetValue;
        }
#endregion

#region Functions
        async internal Task InitializeClassification(string NameOfEvent)
        {
            AlchemyAPI.AlchemyAPI AlchemyObj = new AlchemyAPI.AlchemyAPI();
            AlchemyObj.SetAPIKey("c73e93af01a6cb7728a6b90887c266a8881b7665");
            var strings = new string [] { "", "" };
            Deduction = new ClassificationInterpreter(strings);
            string xml = "";
            try
            {
                xml = AlchemyObj.TextGetRankedTaxonomy(NameOfEvent);
            }
            catch
            {

            }
            if(string.IsNullOrEmpty(xml))
            {

            }
            else
            {

            }
            //Console.WriteLine(xml);
        }
#endregion

        #region Properties
        public Vicinity LocationPreference
        {
            get
            {
                return _Placement;
            }
        }

        public  EnergyDifferential FatigueDelta
        {
            get
            {
                return _Succubus;
            }
        }

        public Leisure SeriousNess
        {
            get
            {
                return _LeisureType;
            }
        }

        public bool isInitialized
        {
            get
            {
                return _Initialized;
            }
        }
        #endregion

        #region Interior Class
        protected class ClassificationInterpreter
        {
            protected Vicinity LocationDeduction = Vicinity.None;
            EnergyDifferential SuccubusDeduction= EnergyDifferential.None;
            Leisure LesiureTypeDeduction = Leisure.None;

            public ClassificationInterpreter(string [] Categorization)
            {

            }

            protected void Evaluatecategorization ()
            {

            }
        }
        #endregion
    }

    public enum Vicinity { Indoor, None, Outdoor }
    public enum EnergyDifferential { Lethargic, None, Active }
    public enum Leisure { Casual, None, Business }
}
