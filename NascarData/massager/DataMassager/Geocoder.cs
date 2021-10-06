using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using DataMassager.BingMaps;

namespace DataMassager
{
    public class Geocoder
    {
        public void Go()
        {
            var reverseGeocodeRequest = new ReverseGeocodeRequest
            {
                Credentials =
                    new Credentials { ApplicationId = "Ajlvx1MRWtrOm8sYYJFUhGqyi4-PY6XCUPpmDtsdzreRbocbV3XSQRi4A-FfVXsF" },
                Location = new Location { Latitude = 42.068144d, Longitude = -84.242730d }
            };

            var svc = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            var geocodeResponse = svc.ReverseGeocode(reverseGeocodeRequest);

            Console.WriteLine(geocodeResponse.Results.Length > 0
                ? geocodeResponse.Results[0].DisplayName
                : "No Results found");
        }

        public string GetPoi(decimal lat, decimal lon)
        {
            return GetPoi(lat.ToString(CultureInfo.InvariantCulture), lon.ToString(CultureInfo.InvariantCulture));
        }

        public string GetPoi(string lat, string lon)
        {

            var uri = $"http://spatial.virtualearth.net/REST/v1/data/f22876ec257b474b82fe2ffcb8393150/NavteqNA/NavteqPOIs?spatialFilter=nearby({lat},{lon},15)&$filter=EntityTypeID%20eq%20'7940'&$select=EntityID,DisplayName,Latitude,Longitude,__Distance&$top=3&key=Ajlvx1MRWtrOm8sYYJFUhGqyi4-PY6XCUPpmDtsdzreRbocbV3XSQRi4A-FfVXsF";
            var rawXml = new WebClient().DownloadString(uri);
            var xdoc = XDocument.Parse(rawXml);
            var ok = xdoc.Descendants(XName.Get("DisplayName", "http://schemas.microsoft.com/ado/2007/08/dataservices")).ToList();
            return ok.Count() == 1 ? ok.Single().Value : ok.First().Value;

            var entries = xdoc.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom"))
                    .Elements(XName.Get("content", "http://www.w3.org/2005/Atom"))
                    .Elements(XName.Get("properties", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"))
                    .Elements(XName.Get("DisplayName", "http://schemas.microsoft.com/ado/2007/08/dataservices"));

        }
    }
}