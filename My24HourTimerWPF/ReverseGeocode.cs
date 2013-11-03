using System.Runtime.Serialization;

namespace My24HourTimerWPF
{
  [DataContract]
  class ReverseGeocode
  {
    [DataMember]
    public ReverseGeocodeAddress address {get; set;}

    [DataMember]
    public ReverseGeocodeLocation location { get; set; }

  }

  [DataContract]
  class ReverseGeocodeAddress
  {
    [DataMember]
    public string Address {get; set;}
  
    [DataMember]
    public string Neighborhood {get; set;}

    [DataMember]
    public string City { get; set;}
      
    [DataMember]
    public string Subregion {get; set;}

    [DataMember]
    public string Region {get; set;}

    [DataMember]
    public string Postal {get; set;}

    [DataMember]
    public string PostalExt {get; set;}

    [DataMember]
    public string CountryCode {get; set;}

    [DataMember]
    public string Loc_name {get; set;}
 }

  [DataContract]
  class ReverseGeocodeLocation
  {
    [DataMember]
    public double x {get; set;}
  
    [DataMember]
    public double y {get; set;}

    [DataMember]
    public SpatialReference spatialReference {get; set;}

  }
}
