using System.Runtime.Serialization;

namespace My24HourTimerWPF
{
  [DataContract]
  class Geocode
  {
    [DataMember]
    public SpatialReference spatialReference { get; set; }

    [DataMember]
    public GeocodeLocation[] locations { get; set; }
  }

  [DataContract]
  class GeocodeLocation
  {
    [DataMember]
    public string name { get; set; }

    [DataMember]
    public Extent extent { get; set; }

    [DataMember]
    public GeocodeFeature feature { get; set; }
  }

  [DataContract]
  class Extent
  {
    [DataMember]
    public double xmin { get; set; }

    [DataMember]
    public double ymin { get; set; }

    [DataMember]
    public double xmax { get; set; }

    [DataMember]
    public double ymax { get; set; }
  }

  [DataContract]
  class GeocodeFeature
  {
    [DataMember]
    public GeocodeGeometry geometry { get; set; }

    [DataMember]
    public GeocodeAttributes attributes { get; set; }
  }

  [DataContract]
  class GeocodeGeometry
  {
    [DataMember]
    public double x { get; set; }

    [DataMember]
    public double y { get; set; }
  }

  [DataContract]
  class GeocodeAttributes
  {
    [DataMember]
    public string type { get; set; }

    [DataMember]
    public string city { get; set; }

    [DataMember]
    public string region { get; set; }
  }
}
 
