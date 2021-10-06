using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Nascar.Sv.UniversalLive.SocketWorker;

namespace Nascar.Sv.UniversalLive
{
    public sealed partial class MainPage : Page, IObserver<string>
    {
        private Geopoint _root;

        public ObservableCollection<string> Data { get; set; }

        public MainPage()
        {
            InitializeComponent();
            Data = new ObservableCollection<string>();
            Go();
        }

        public void Go()
        {
            //Task.Run(() => { });
            var e = new CarStreamReader();
            e.Subscribe(this);
            e.Start("10.69.162.183", 23);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(string value)
        {
            var stuff = value.Split(';');
            var car = stuff[1];
            var lat = double.Parse(stuff[5]);
            var lon = double.Parse(stuff[6]);
            var alt = double.Parse(stuff[7]);
            var e = new Geopoint(new BasicGeoposition() { Altitude = alt, Latitude = lat, Longitude = lon });
            if (_root == null)
            {
                _root = e;
                Map.Center = e;
                //Map.ZoomLevel = 16;
            }

            var existingIcons = Map.MapElements.OfType<MapIcon>().Where(x => x.Title == car).ToList();
            if (existingIcons.Any())
            {
                existingIcons.ForEach(x => Map.MapElements.Remove(x));
            }

            Map.MapElements.Add(new MapIcon()
            {
                CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                Location = e,
                Title = car
            });
        }
    }
}