﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlchemyAPI;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace TilerElements
{
    public class Classification:IUndoable
    {
        Vicinity _Placement = Vicinity.None;
        EnergyDifferential _Succubus = EnergyDifferential.None;
        Leisure _LeisureType = Leisure.None;
        bool _Initialized = true;

        public Vicinity _UndoPlacement;
        public EnergyDifferential _UndoSuccubus;
        public Leisure _UndoLeisureType;
        public bool _UndoInitialized;
        protected string _UndoId = "";

        protected string _Id { get; set; }
        protected TilerEvent _AssociatedEvent { get; set; }
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
            RetValue.Id = this.Id;
            return RetValue;
        }
        
        async internal Task InitializeClassification(string NameOfEvent)
        {
            AlchemyAPI.AlchemyAPI AlchemyObj = new AlchemyAPI.AlchemyAPI();
            AlchemyObj.SetAPIKey("c73e93af01a6cb7728a6b90887c266a8881b7665");
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

        public void undoUpdate(Undo undo)
        {
            throw new NotImplementedException();
        }

        public void undo(string undoId)
        {
            if(undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoPlacement, ref _Placement);
                Utility.Swap(ref _UndoSuccubus, ref _Succubus);
                Utility.Swap(ref _UndoLeisureType, ref _LeisureType);
                Utility.Swap(ref _UndoInitialized, ref _Initialized);
            }
        }

        public void redo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoPlacement, ref _Placement);
                Utility.Swap(ref _UndoSuccubus, ref _Succubus);
                Utility.Swap(ref _UndoLeisureType, ref _LeisureType);
                Utility.Swap(ref _UndoInitialized, ref _Initialized);
            }
        }
        #region Properties

        public bool Initialized
        {
            get
            {
                return _Initialized;
            }
            set
            {
                _Initialized = value;
            }
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

        [ForeignKey("Id")]
        public TilerEvent AssociatedEvent
        {
            get
            {
                return _AssociatedEvent;
            }
            set
            {
                _AssociatedEvent = value;
            }
        }

        public string Placement
        {
            get
            {
                return _Placement.ToString();
            }
            set
            {
                Enum.TryParse(value, out _Placement);
            }
        }

        public string Succubus
        {
            get
            {
                return _Succubus.ToString();
            }
            set
            {
                Enum.TryParse(value, out _Succubus);
            }
        }

        public string LeisureType
        {
            get
            {
                return _LeisureType.ToString();
            }
            set
            {
                Enum.TryParse(value, out _LeisureType);
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;

        public virtual string UndoId
        {
            get
            {
                return _UndoId;
            }
            set
            {
                _UndoId = value;
            }
        }
        #endregion

    }

    public enum Vicinity { Indoor, None, Outdoor }
    public enum EnergyDifferential { Lethargic, None, Active }
    public enum Leisure { Casual, None, Business }
}
