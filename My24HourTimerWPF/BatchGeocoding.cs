using System.Runtime.Serialization;

namespace My24HourTimerWPF
{
  [DataContract]
  class BatchGeocodingResponse
  {
    [DataMember]
    public SpatialReference spatialReference { get; set; }

    [DataMember]
    public Addresses[] locations { get; set; }
  }

  [DataContract]
  class SpatialReference
  {
    [DataMember]
    public long wkid { get; set; }

    [DataMember]
    public long latestWkid { get; set; }
  }

  [DataContract]
  class Addresses
  {
    [DataMember]
    public string address { get; set; }

    [DataMember]
    public LocationMap location { get; set; }

    [DataMember]
    public int score { get; set; }

    [DataMember]
    public BatchGeocoding attributes { get; set; }
  }

  [DataContract]
  class LocationMap
  {
    [DataMember]
    public double x { get; set; }

    [DataMember]
    public double y { get; set; }
  }

  [DataContract]
  class BatchGeocoding
  {
    [DataMember]
    public int ResultID { get; set; }

    [DataMember]
    public string Loc_name { get; set; }

    [DataMember]
    public int Score { get; set; }

    [DataMember]
    public string Match_addr { get; set; }

    [DataMember]
    public string Addr_type { get; set; }

    [DataMember]
    public string PlaceName { get; set; }

    [DataMember]
    public string Rank { get; set; }

    [DataMember]
    public string AddBldg { get; set; }


    [DataMember]
    public string AddNum { get; set; }

    [DataMember]
    public string AddNumFrom { get; set; }

    [DataMember]
    public string AddNumTo { get; set; }

    [DataMember]
    public string Side { get; set; }

    [DataMember]
    public string StPreDir { get; set; }

    [DataMember]
    public string StPreType { get; set; }

    [DataMember]
    public string StName { get; set; }

    [DataMember]
    public string StType { get; set; }

    [DataMember]
    public string StDir { get; set; }

    [DataMember]
    public string Nbrhd { get; set; }

    [DataMember]
    public string City { get; set; }

    [DataMember]
    public string Subregion { get; set; }

    [DataMember]
    public string Region { get; set; }

    [DataMember]
    public string Postal { get; set; }

    [DataMember]
    public string PostalExt { get; set; }

    [DataMember]
    public string Country { get; set; }

    [DataMember]
    public string LangCode { get; set; }

    [DataMember]
    public int Distance { get; set; }

    [DataMember]
    public double X { get; set; }

    [DataMember]
    public double Y { get; set; }

    [DataMember]
    public double DisplayX { get; set; }

    [DataMember]
    public double DisplayY { get; set; }

    [DataMember]
    public double Xmin { get; set; }

    [DataMember]
    public double Xmax { get; set; }

    [DataMember]
    public double Ymin { get; set; }

    [DataMember]
    public double Ymax { get; set; }

    [DataMember]
    public string Status { get; set; }
  }
}
