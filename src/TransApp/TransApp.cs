using System;
using System.IO;
using System.Timers;
using System.Threading;
using System.Diagnostics; // for debug atm
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

using TransApp.Formats;

namespace TransApp {

    public class TaMap : NavigationPage {

        private readonly MapContent _mapContent;

        private void OnFavoriteSwap(object sender, EventArgs args) {
            var favourites = new MyFavourites(this, _mapContent.Map);
            PushAsync(favourites.Content);
        }
        
        private void OnSettingsSwap(object sender, EventArgs args) {
            var settings = new SettingsContent();
            PushAsync(settings.Content);
        }

        private void UpdateBusLocationsThread() {
            while (true) {
                var ctime = new StopTime(DateTime.Now);
                for (var a = 0; a < _mapContent.TripPointers.Length; a++) {
                    var isDelayed = MapUtil.IsTripDelayed(_mapContent.Trips[a].Id);
                    var pos = _mapContent.Trips[a].GetBusLocation(ctime);
                    var cpointer = _mapContent.TripPointers[a];
                    if (pos == null) continue;
                    if (cpointer >= _mapContent.Map.Pins.Count) continue;
                    if (cpointer == -1) {
                        _mapContent.TripPointers[a] = _mapContent.Map.Pins.Count;
                        pos.Pin(_mapContent.Map.Pins, isDelayed ? _mapContent.Map.DelayedBusIndices : null, _mapContent.Trips[a].Id);
                    } else _mapContent.Map.Pins[cpointer].Position = new Position(pos.Latitude, pos.Longitude);
                    
                }
                if (_mapContent.Map.OnPinsUpdated != null) Device.BeginInvokeOnMainThread(() => { _mapContent.Map.OnPinsUpdated(); });
            }
        }

        public TaMap() : base(MapContent.CreateAndStash()) {
            _mapContent = MapContent.Stash;
            
            _mapContent.FavoritesButton.Clicked += OnFavoriteSwap;
            _mapContent.SettingsButton.Clicked += OnSettingsSwap;

            _mapContent.UpdateThread = new Thread(UpdateBusLocationsThread);
            _mapContent.UpdateThread.Start();
            
            SetHasNavigationBar(_mapContent.Content, false); // This has to be set twice!
        }
    }

    public class App : Application {

        public static double GetScreenWidth() {
            #if __IOS__
            return UIKit.UIScreen.MainScreen.Bounds.Width;
            #elif __ANDROID__
            return -1;
            #endif
        }

        public static double GetScreenHeight() {
            #if __IOS__
            return UIKit.UIScreen.MainScreen.Bounds.Height;
            #elif __ANDROID__
            return -1;
            #endif
        }
        
        public App() {
            // The root page of your application
            Console.WriteLine(Utility.GetUserId());
            MapUtil.UpdateDelays();
            TaMap page = new TaMap();
            NavigationPage.SetHasNavigationBar(page, false);
            MainPage = page;
        }

        protected override void OnStart() {
            // Handle when your app starts
        }

        protected override void OnSleep() {
            // Handle when your app sleeps
        }

        protected override void OnResume() {
            // Handle when your app resumes
        }
    }
}
