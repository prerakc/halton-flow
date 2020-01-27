using System;
using System.IO;
using System.Diagnostics; // for debug atm
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

using TransApp.Formats;

namespace TransApp {

    public class MapUtil {
        public static readonly Position DefaultMapLocation = new Position(43.3255, -79.7990);
        public static readonly Distance DefaultMapRadius = Distance.FromMiles(0.3);
        public static MapSpan GetDefaultMapSpan() {
            return MapSpan.FromCenterAndRadius(DefaultMapLocation, DefaultMapRadius);
        }

        public delegate void FavouriteAdded(int val);

        public class FavoriteList {
            private readonly List<int> _favs = new List<int>();

            public FavouriteAdded OnFavouriteAdded;

            public bool Contains(int val) { return _favs.Contains(val); }
            public int At(int index) { return _favs[index]; }
            public int Size() { return _favs.Count; }
            public void AddFav(int val) {
                _favs.Add(val);
                OnFavouriteAdded(val);
            }
        }

        public static readonly FavoriteList Favorites = new FavoriteList();

        public static Dictionary<int, int> DelayTable = new Dictionary<int, int>();
        public static void UpdateDelays() {
            var delayJson = Utility.RequestHttpGet(Utility.DominicFlow, "/delays");
            Console.WriteLine("delays:" + delayJson);
#if __IOS__
            DelayTable = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, int>>(delayJson);
#endif
            if (DelayTable == null) DelayTable = new Dictionary<int, int>();
        }
        
        public static bool IsTripDelayed(int tripId) {
            foreach (int a in DelayTable.Keys) if (a == tripId) return true;
            return false;
        }

        private static TripFile _defaultTripFile = null;
        private static RouteFile _defaultRouteFile = null;
        public static RouteFile GetDefaultRouteFile() {
            return _defaultRouteFile ?? (_defaultRouteFile = new RouteFile("Trips/burlington.rut"));
        }
        public static TripFile GetDefaultTripFile() {
            if (_defaultTripFile != null) return _defaultTripFile;
            
            _defaultTripFile = new TripFile("Trips/burlington.trp");
            var shpFile = new ShapeFile("Trips/burlington.shp");
            var stpFile = new StopFile("Trips/burlington.stp");
            var tmsFile = new TripTimeFile("Trips/burlington.tms");
            var rutFile = GetDefaultRouteFile();

            // Link files
            var watch = Stopwatch.StartNew(); // debug runtime watch
            _defaultTripFile.BindRoutes(rutFile);
            _defaultTripFile.BindShapes(shpFile);
            _defaultTripFile.BindTripTimes(tmsFile);
            _defaultTripFile.BindStops(stpFile);
            Console.WriteLine("Bind Time: " + watch.ElapsedMilliseconds);
            watch.Reset();

            watch.Start();
            _defaultTripFile.MixInStops();
            Console.WriteLine("Mixin Time: " + watch.ElapsedMilliseconds);

            return _defaultTripFile;
        }
    }

    public class DefaultLocation : Pin {
        public readonly int ViewId = Utility.GetUniqueName();
        public readonly int TripId;
        public DefaultLocation(int tripId) { TripId = tripId; }
#if __IOS__
        public MapKit.IMKAnnotation Annotation = null;
#endif
    }
    
    public class DefaultMap : Map {
        
        public readonly List<int> DelayedBusIndices = new List<int>();

        public Action OnPinsUpdated;
        
        public DefaultMap() : base(MapUtil.GetDefaultMapSpan()) {
            IsShowingUser = true;
            HeightRequest = 100;
            WidthRequest = 960;
            VerticalOptions = LayoutOptions.FillAndExpand;
        }
    }
}