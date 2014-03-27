﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Google.Maps.Geocoding;
using System.Drawing;


namespace My24HourTimerWPF
{
    public class Location
    {
        string _token = string.Empty;
        enum requestType
        {
            authenticate,
            geocode,
            reverseGeocode,
            batchGeocode,
            route
        };

        double xValue;
        double yValue;
        string TaggedDescription;
        string TaggedAddress;

        public Location()
        {
            xValue = double.MaxValue;
            yValue = double.MaxValue;
        }

        public Location(double MyxValue, double MyyValue)
        {
            xValue = MyxValue;
            yValue = MyyValue;
        }

        public Location(double MyxValue, double MyyValue,string AddressEntry, string AddressDescription)
        {
            xValue = MyxValue;
            yValue = MyyValue;
            TaggedAddress = AddressEntry;
            TaggedDescription = AddressDescription;
        }
        
        public Location(string Address)
        {
            Address=Address.Trim();
            if (string.IsNullOrEmpty(Address))
            {
                xValue = double.MaxValue;
                yValue = double.MaxValue;
            }

            else
            {
                var request = new GeocodingRequest();
                request.Address = Address;
                request.Sensor = false;
                var response = new GeocodingService().GetResponse(request);
                var result = response.Results.First();
                TaggedDescription = Address;
                TaggedAddress = result.FormattedAddress;
                xValue=Convert.ToDouble(result.Geometry.Location.Latitude);
                yValue = Convert.ToDouble(result.Geometry.Location.Longitude);
                MessageBox.Show("Found Location At: " + result.FormattedAddress + " Latitude: " + xValue + " Longitude: " + yValue); 



                /*
                string url = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/find?text=";
                string[] Searches = { "coffee", "market", "library", "park" };
                //Address = "boulder co " + Searches[(searchindex++) % 3];
                {
                    url += Address.Replace(" ", "+");
                    url = url.Replace(",", "%26") + "&f=pjson";
                }




                //Address = Address;
                string json = HttpGetWebRequest(url);
                //My24HourTimerWPF.Form1.Deserialize(My24HourTimerWPF.Form1.requestType.geocode, json);
                Geocode MyGeoCode = Deserialize(json);
                xValue = MyGeoCode.locations[0].feature.geometry.x;
                yValue = MyGeoCode.locations[0].feature.geometry.y;
                //write the response json to the UI
                json.Replace("{", "{ \r\n");
                json.Replace(",", ", \r\n");
                json.Replace("}", "} \r\n");*/
            }
        }


        #region Functions
        string getStringWebLocation24(float xLocation24, float yLocation24)
        {
            return "Hello";
        }
        float[,] getGPSWebLocation24(string Address)
        {
            
            return null;
        }

        static public double calculateDistance(Location Location24A, Location Location24B)
        {
            //note .... this function does not take into consideration the calendar event. So if there are two locations of the same calendarevent they will get scheduled right next to each other
            double deltaX=Location24A.xValue-Location24B.xValue;
            double deltaY = Location24A.yValue - Location24B.yValue;
            double sqrValue = (Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            double retValue=Math.Sqrt(sqrValue);
            if (double.IsPositiveInfinity(retValue))
                        {
                            ;
                        }
            return retValue;
        }


        static public double calculateDistance(List<Location> ListOfLocations)
        {
            int i = 0;
            int j = i + 1;
            double retValue = 0;
            while (j < ListOfLocations.Count)
            {
                retValue += calculateDistance(ListOfLocations[i], ListOfLocations[j]);
                i++; j++;
            }
            return retValue;
        }
        private string HttpGetWebRequest(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "GET";

            HttpWebResponse httpResponse = null;
            try
            {
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch { return null; }

            string JSON = string.Empty;

            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                JSON = reader.ReadToEnd();

            return JSON;
        }

        private Geocode Deserialize( string json)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            
            
           
                Geocode geocode = javaScriptSerializer.Deserialize<Geocode>(json);
                return geocode;
            
        }

        public Location CreateCopy()
        {
            Location this_cpy = new Location();
            this_cpy._token = this._token;
            this_cpy.TaggedAddress = this.TaggedAddress;
            this_cpy.TaggedDescription = this.TaggedDescription;
            this_cpy.xValue = this.xValue;
            this_cpy.yValue = this.yValue;
            return this_cpy;
        }

        #endregion 


        #region Properties

        public string Description
        {
            get 
            {
                return TaggedDescription;
            }
        }

        public string Address
        {
            get
            {
                return TaggedAddress;
            }
        }

        public double XCoordinate
        {
            get
            { return xValue; }
            
        }


        public double YCoordinate
        {
            get
            {
                return yValue;
            }
        }
        #endregion
    }

    
}
