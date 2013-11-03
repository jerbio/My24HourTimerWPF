using System;
using System.Runtime.Serialization;

namespace My24HourTimerWPF
{
  [DataContract]
  class RouteDataContract
  {
    [DataMember]
    public object[] messages { get; set; }

    [DataMember]
    public RouteContract routes { get; set; }
  }

  [DataContract]
  class RouteContract
  {
    [DataMember]
    public FieldAliases fieldAliases { get; set; }

    [DataMember]
    public string geometryType { get; set; }

    [DataMember]
    public SpatialReference spatialReference { get; set; }

    [DataMember]
    public Feature[] features { get; set; }
  }

  [DataContract]
  class FieldAliases
  {
    [DataMember]
    public string ObjectID { get; set; }

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string FirstStopID { get; set; }

    [DataMember]
    public string LastStopID { get; set; }

    [DataMember]
    public string StopCount { get; set; }

    [DataMember]
    public string Total_TravelTime { get; set; }

    [DataMember]
    public string Total_Kilometers { get; set; }

    [DataMember]
    public string Total_Miles { get; set; }

    [DataMember]
    public string Shape_Length { get; set; }
  }

  [DataContract]
  class Feature
  {
    [DataMember]
    public RouteAttributes attributes { get; set; }

    [DataMember]
    public Geometry geometry { get; set; }
  }

  [DataContract]
  class RouteAttributes
  {
    [DataMember]
    public string ObjectID { get; set; }

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string FirstStopID { get; set; }

    [DataMember]
    public string LastStopID { get; set; }

    [DataMember]
    public string StopCount { get; set; }

    [DataMember]
    public string Total_TravelTime { get; set; }

    [DataMember]
    public string Total_Kilometers { get; set; }

    [DataMember]
    public string Total_Miles { get; set; }

    [DataMember]
    public string Shape_Length { get; set; }
  }

  [DataContract]
  class Geometry
  {
    [DataMember]
    public object[] paths { get; set; }
  }

}

//{
// "messages": [
  
// ],
// "routes": {
//  "fieldAliases": {
//   "ObjectID": "ObjectID",
//   "Name": "Name",
//   "FirstStopID": "FirstStopID",
//   "LastStopID": "LastStopID",
//   "StopCount": "StopCount",
//   "Total_TravelTime": "Total_TravelTime",
//   "Total_Kilometers": "Total_Kilometers",
//   "Total_Miles": "Total_Miles",
//   "Shape_Length": "Shape_Length"
//  },
//  "geometryType": "esriGeometryPolyline",
//  "spatialReference": {
//   "wkid": 4326,
//   "latestWkid": 4326
//  },
//  "features": [
//   {
//    "attributes": {
//     "ObjectID": 1,
//     "Name": "Location 1 - Location 2",
//     "FirstStopID": 1,
//     "LastStopID": 2,
//     "StopCount": 2,
//     "Total_TravelTime": 3.4970763159708733,
//     "Total_Kilometers": 1.8624238672796565,
//     "Total_Miles": 1.1572565388628264,
//     "Shape_Length": 0.019773795438324304
//    },
//    "geometry": {
//     "paths": [
//      [
//       [
//        -117.17597000021533,
//        34.065258746294148
//       ],
//       [
//        -117.17597000021533,
//        34.066400000359806
//       ],
//       [
//        -117.17484999982759,
//        34.066389999898604
//       ],
//       [
//        -117.17377999994699,
//        34.066409999921632
//       ],
//       [
//        -117.1726599995593,
//        34.066400000359806
//       ],
//       [
//        -117.17160999970167,
//        34.066389999898604
//       ],
//       [
//        -117.17100000034969,
//        34.066389999898604
//       ],
//       [
//        -117.16944000035568,
//        34.066380000336778
//       ],
//       [
//        -117.16792000040766,
//        34.066389999898604
//       ],
//       [
//        -117.16725999964922,
//        34.066400000359806
//       ],
//       [
//        -117.16041000032124,
//        34.066380000336778
//       ],
//       [
//        -117.16041999988306,
//        34.065500000224858
//       ],
//       [
//        -117.15822803549543,
//        34.065489229044715
//       ]
//      ]
//     ]
//    }
//   }
//  ]
// },
// "directions": [
//  {
//   "routeId": 1,
//   "routeName": "Location 1 - Location 2",
//   "summary": {
//    "totalLength": 1.8624238672796563,
//    "totalTime": 3.4970763197634369,
//    "totalDriveTime": 3.4970763159708733,
//    "envelope": {
//     "xmin": -117.17600939861484,
//     "ymin": 34.065258746294148,
//     "xmax": -117.15822597874586,
//     "ymax": 34.066409999921632,
//     "spatialReference": {
//      "wkid": 4326,
//      "latestWkid": 4326
//     }
//    }
//   },
//   "features": [
//    {
//     "attributes": {
//      "length": 0,
//      "time": 0,
//      "text": "Start at Location 1",
//      "ETA": -2209161600000,
//      "maneuverType": "esriDMTDepart"
//     },
//     "compressedGeometry": "+1m91-66oi9+1pp4d+0+0"
//    },
//    {
//     "attributes": {
//      "length": 0.12592628377348575,
//      "time": 0.40596725715004073,
//      "text": "Go north on OXFORD DR toward E BROCKTON AVE",
//      "ETA": -2209161600000,
//      "maneuverType": "esriDMTStraight"
//     },
//     "compressedGeometry": "+1m91-66oi9+1pp4d+0+20"
//    },
//    {
//     "attributes": {
//      "length": 1.4365290639930641,
//      "time": 2.4580856047183905,
//      "text": "Turn right on E BROCKTON AVE",
//      "ETA": -2209161600000,
//      "maneuverType": "esriDMTTurnRight"
//     },
//     "compressedGeometry": "+1m91-66oi9+1pp6d+r1-1"
//    },
//    {
//     "attributes": {
//      "length": 0.097616343929840768,
//      "time": 0.18755897857205023,
//      "text": "Turn right on N GROVE ST",
//      "ETA": -2209161600000,
//      "maneuverType": "esriDMTTurnRight"
//     },
//     "compressedGeometry": "+1m91-66nn8+1pp6c-1-1h"
//    },
//    {
//     "attributes": {
//      "length": 0.20235217558326568,
//      "time": 0.4454644755303922,
//      "text": "Turn left on CAMPUS AVE",
//      "ETA": -2209161600000,
//      "maneuverType": "esriDMTTurnLeft"
//     },
//     "compressedGeometry": "+1m91-66nn9+1pp4r+3q-1"
//    },
//    {
//     "attributes": {
//      "length": 0,
//      "time": 0,
//      "text": "Finish at Location 2, on the left",
//      "ETA": -2209161600000,
//      "maneuverType": "esriDMTStop"
//     },
//     "compressedGeometry": "+1m91-66njf+1pp4q+0+0"
//    }
//   ]
//  }
// ]
//}
