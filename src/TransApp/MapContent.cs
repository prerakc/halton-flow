#define MAPCONENT_TAG_RANDOM_BUS

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

    public class TaBaseFields {
        protected NavigationPage NavPage;
        public ContentPage Content;
    }

    public class TaMapFields : TaBaseFields {
        public DefaultMap Map;
        public Button FavoritesButton;
        public Button SettingsButton;

        public TripFile Trips;
        public int[] TripPointers;

        public Thread UpdateThread;
    }

    public class MapContent : TaMapFields {

        public static MapContent Stash;

        private readonly bool _taggedRandomBus
        #if !MAPCONENT_TAG_RANDOM_BUS
            = true
        #endif
        ;
        
        private static void UpdateServer(int val) {
            Console.WriteLine("Favourite add " + val + " result: " + Utility.RequestHttpGet(Utility.DominicFlow, "/favourites?user=" + Utility.GetUserId() + "&trip=" + val));
        }

        private MapContent() {
            Trips = MapUtil.GetDefaultTripFile();
            TripPointers = new int[Trips.Count];
            
            Console.WriteLine("Est. Screen size: " + App.GetScreenWidth() + ", " + App.GetScreenHeight());

            Map = new DefaultMap();
            FavoritesButton = new Button {
                HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.End,
                WidthRequest = 50, HeightRequest = 50,
                Image = (FileImageSource)(ImageSource.FromFile("Star_Button.png")),
                BackgroundColor = new Color(0, 1, 0),
                Margin = new Thickness(10, 0, 0, 10),
                #if __IOS__
                CornerRadius = 25,
                #endif
            };

            SettingsButton = new Button {
                HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End,
                WidthRequest = 50, HeightRequest = 50,
                Image = (FileImageSource)(ImageSource.FromFile("Settings_Button.png")),
                BackgroundColor = new Color(0, 1, 0),
                Margin = new Thickness(0, 0, 10, 10),
                #if __IOS__
                CornerRadius = 25,
                #endif
            };
            
            var rightNow = new StopTime(DateTime.Now);

            for (var a = 0; a < Trips.Count; a++) {
                var isDelayed = MapUtil.IsTripDelayed(Trips[a].Id);
                Utility.Vertex vert = Trips[a].GetBusLocation(rightNow);
                TripPointers[a] = Map.Pins.Count;
                if (vert == null) continue;
                vert.Pin(Map.Pins, (!_taggedRandomBus || isDelayed) ? Map.DelayedBusIndices : null, Trips[a].Id);
                _taggedRandomBus = true;
            }
            
            MapUtil.Favorites.OnFavouriteAdded += UpdateServer;

            Content = new ContentPage {
                Content = new Grid {
                    Children = {
                        Map,
                        FavoritesButton,
                        SettingsButton
                    }
                }
            };
        }
        
        public static ContentPage CreateAndStash() {
            var mainContent = new MapContent();
            Stash = mainContent;
            return mainContent.Content;
        }
    }
}
