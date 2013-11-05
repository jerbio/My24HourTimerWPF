using System;
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
using System.Drawing;


namespace My24HourTimerWPF
{
    public class Location24
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
        string Address;

        public Location24()
        {
            xValue = 0;
            yValue = 0;
        }

        public Location24(float MyxValue, float MyyValue)
        {
            xValue = MyxValue;
            yValue = MyyValue;
        }
        static int searchindex = 0;
        public Location24(string Address)
        {
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
            yValue= MyGeoCode.locations[0].feature.geometry.y;
            //write the response json to the UI
            json.Replace("{", "{ \r\n");
            json.Replace(",", ", \r\n");
            json.Replace("}", "} \r\n");
            //txtGeocodeResult.Text = json.Replace("{", "{\r\n");
        }

        string getStringWebLocation24(float xLocation24, float yLocation24)
        {
            return "Hello";
        }
        float[,] getGPSWebLocation24(string Address)
        {
            
            return null;
        }

        static public double calculateDistance(Location24 Location24A, Location24 Location24B)
        {
            double deltaX=Location24A.xValue-Location24B.xValue;
            double deltaY = Location24A.yValue - Location24B.yValue;
            double sqrValue = (Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            return Math.Sqrt(sqrValue);
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

    }

    
}
