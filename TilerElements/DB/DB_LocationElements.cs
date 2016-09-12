using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using TilerFront;
namespace TilerElements.DB
{
    public class DB_LocationElements:Location_Elements
    {
        
        public DB_LocationElements():base()
        {
            updateNameHash();
        }

        public DB_LocationElements(string Address, string tag = "", string ID = ""):base(Address, tag, ID)
        {
            updateNameHash();
        }
        protected string _NameHash { get; set; }
        /// <summary>
        /// user id for which this location is associated with
        /// </summary>
        [Index(name: "UserId_CacheNameHash", IsUnique = true, Order = 0)]
        [Required]
        public string CreatorId { get; set; }
        protected TilerUser _User { get; set; }
        [ForeignKey("CreatorId")]
        public virtual TilerUser User
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
            }
        }
        /// <summary>
        /// Address Line 1
        /// </summary>
        public string Address1 { get; set; }
        /// <summary>
        /// Address Line 1
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// City or town of address
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// State or province of the address
        /// </summary>
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }

        override public string Id
        {
            get
            {
                return LocationID.ToString();
            }
            protected set
            {
                LocationID =Guid.Parse( value).ToString();
            }
        }

        public double Longitude {
            get
            {
                return yValue;
            }
            set
            {
                yValue = value;
            }
        }

        public double Latitude
        {
            get
            {
                return xValue;
            }
            set
            {
                xValue = value;
            }
        }

        public bool IsDefault
        {
            get
            {
                return isDefault;
            }
            set
            {
                DefaultFlag = value;
            }
        }

        public bool isNullLocation
        {
            get
            {
                return NullLocation;
            }
            set
            {
                NullLocation = value;
            }
        }
        [Index(name: "UserId_CacheNameHash", IsUnique = true, Order = 1)]
        [Required]
        public string NameHash
        {
            get
            {
                return _NameHash;
            }
            set
            {
                _NameHash = value;
            }
        }

        
        public string Name
        {
            get
            {
                return TaggedDescription;
            }
            set
            {
                TaggedDescription = value;
                updateNameHash();
            }
        }

        protected void updateNameHash()
        {
            if(!string.IsNullOrEmpty(this.Name)){
                NameHash = Utility.CalculateMD5Hash(this.Name);
            }
            
        }

        public override bool Validate()
        {
            var retValue = base.Validate();
            updateNameHash();
            return retValue;
        }
        /// <summary>
        /// this updates  this location element, with every element in newLocation,e xcepth the Id
        /// </summary>
        /// <param name="newLocation"></param>
        public override void updateThis(Location_Elements newLocation)
        {
            this.TaggedAddress = newLocation.Address;
            this.Name = newLocation.Address;
            this.DefaultFlag = newLocation.isDefault;
            this.NullLocation = newLocation.isNull;
            this.xValue = newLocation.XCoordinate;
            this.yValue = newLocation.YCoordinate;
        }

        public string FullAddress
        {
            get
            {
                return TaggedAddress;
            }
            set
            {
                TaggedAddress = value;
            }
        }

        static public DB_LocationElements ConvertToPersistable(Location_Elements location, string UserId)
        {
            DB_LocationElements retValue = (location as DB_LocationElements) ?? new DB_LocationElements()
            {
                FullAddress = location.Address,
                TaggedDescription = location.Description,
                Id = location.Id,
                isNullLocation = location.isNull,
                xValue = location.XCoordinate,
                yValue = location.YCoordinate,
                DefaultFlag = location.isDefault,
                Name = location.Description,
                CreatorId = UserId
            };
            retValue.updateNameHash();
            return retValue;
        }
    }
}
