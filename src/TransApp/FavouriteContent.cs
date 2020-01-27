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
    
    public class TaFavFields : TaBaseFields {
        public TripFile Trips = MapUtil.GetDefaultTripFile();
        public RouteFile Routes = MapUtil.GetDefaultRouteFile();

        public DefaultMap Map;
    }
    
    public class FavouriteTripDisplay : TaFavFields {

        private static readonly FileImageSource StarAccept = (FileImageSource)ImageSource.FromFile("Star_Accept.png");
        private static readonly FileImageSource StarDisabled = (FileImageSource)ImageSource.FromFile("Star_Disabled.png");

        public readonly Trip Trip;

        private readonly Button _favouriteButton;
        private readonly Button _gotoButton;
        private readonly Label _issueLabel;

        private static readonly Color Blue = Color.FromHex("#005090");
        private static readonly Color Red = Color.FromHex("#ff0000");

        private void OnFavourite(object sender, EventArgs args) {
            MapUtil.Favorites.AddFav(Trip.Id);
            _favouriteButton.Image = StarDisabled;
            _favouriteButton.IsEnabled = false;
        }
        
        private void OnGoto(object sender, EventArgs args) {
            var vert = Trip.GetBusLocation(new StopTime(DateTime.Now));
            if (vert != null) {
                Map.MoveToRegion(MapSpan.FromCenterAndRadius(vert.AsPosition(), MapUtil.DefaultMapRadius));
                NavPage.PopToRootAsync();
            } else {
                _issueLabel.Text = "Bus is not on map.";
                _issueLabel.TextColor = Color.FromHex("#ff0000");
            }
        }

        public FavouriteTripDisplay(NavigationPage page, DefaultMap map, int tripId) {
            Map = map;
            NavPage = page;
            
            var isFavorite = MapUtil.Favorites.Contains(tripId);
            var isDelayed = MapUtil.IsTripDelayed(tripId);

            Trip = Trips.GetTripWithTripId(tripId);

            _favouriteButton = new Button {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Image = isFavorite ? StarDisabled : StarAccept,
                IsEnabled = !isFavorite,
                Margin = new Thickness(0, 40, 0, 0)
            };

            _issueLabel = new Label {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Text = isDelayed ? "Delayed by " + (MapUtil.DelayTable[tripId]/60) + " minute(s)." : "On Schedule",
                Margin = new Thickness(0, 20, 0, 0),
                TextColor = isDelayed ? Red : Blue
            };

            _gotoButton = new Button {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Text = "Go to Bus..."
            };

            _favouriteButton.Clicked += OnFavourite;
            _gotoButton.Clicked += OnGoto;

            //TODO just realised this is kinda redundant Content=Content=
            Content = new ContentPage {
                Content = new StackLayout {
                    Children = {
                        new Label {
                            Margin = new Thickness(0, 40, 0, 0),
                            HorizontalOptions = LayoutOptions.Center,
                            Text = Trip.Route.Name
                        },
                        new Label {
                            HorizontalOptions = LayoutOptions.Center,
                            Text = Trip.Id + " (" + Trip.ListStopTimeRangeAsString() + ")"
                        },
                        _favouriteButton,
                        _issueLabel,
                        _gotoButton
                    }
                }
            };
        }
    }
    
    public class FavouriteSubContent : TaFavFields {
        
        private string[] GenTableContent(int routeId) {
            var allTrips = Trips.GetTripsWithRouteId(routeId);
            var result = new string[allTrips.Count];
            for (var a = 0; a < result.Length; a++) {
                if (allTrips[a].Stops.Count > 0)
                    result[a] = allTrips[a].ListStopTimeRangeAsString() + " (" +
                                           allTrips[a].Id + ")";
            }
            return result;
        }
        
        private void ItemSelected(object sender, SelectedItemChangedEventArgs args) {
            var itemText = (string)args.SelectedItem;
            var tripDisplay = new FavouriteTripDisplay(NavPage, Map, Utility.InBraces(itemText));
            NavPage.PushAsync(tripDisplay.Content);
        }
        
        public FavouriteSubContent(NavigationPage page, DefaultMap map, int routeId) {
            Map = map;
            NavPage = page;
            
            var timeList = new ListView {
                ItemsSource = GenTableContent(routeId)
            };
            
            timeList.ItemSelected += ItemSelected;

            Content = new ContentPage {
                Content = new Grid {
                    Children = {
                        timeList
                    }
                }
            };
        }
    }
    
    public class FavouriteContent : TaFavFields {
        private string[] GenTableContent() {
            var result = new string[Routes.Count];
            for (var a = 0; a < result.Length; a++)
                result[a] = Routes[a].Name + " (" + Routes[a].ShortName + ")";
            return result;
        }
        
        private void ItemSelected(object sender, SelectedItemChangedEventArgs args) {
            var itemText = (string)args.SelectedItem;
            var subcontent = new FavouriteSubContent(NavPage, Map, Routes.GetRouteWithShortName(
                Utility.InBraces(itemText)
            ).Id);
            NavPage.PushAsync(subcontent.Content);
        }
        
        public FavouriteContent(NavigationPage page, DefaultMap map) {
            Map = map;
            NavPage = page;

            var routeList = new ListView {
                ItemsSource = GenTableContent()
            };

            routeList.ItemSelected += ItemSelected;

            Content = new ContentPage {
                Content = new Grid {
                    Children = {
                        routeList
                    }
                }
            };
        }
    }

    public class MyFavourites : TaFavFields {

        private const string AddMessage = "Add...";

        private readonly ListView _favList;
        
        private string[] GenTableContent() {
            string[] result = new string[MapUtil.Favorites.Size() + 1];
            for (int a = 0; a < MapUtil.Favorites.Size(); a++) {
                Trip ctrip = Trips.GetTripWithTripId(MapUtil.Favorites.At(a));
                result[a] = ctrip.Route.Name + " (" + ctrip.Id + ") " + ctrip.ListStopTimeRangeAsString();
            }
            result[result.Length - 1] = AddMessage;
            return result;
        }
        
        private void ItemSelected(object sender, SelectedItemChangedEventArgs args) {
            var itemText = (string)args.SelectedItem;
            if (itemText == AddMessage) {
                FavouriteContent content = new FavouriteContent(NavPage, Map);
                NavPage.PushAsync(content.Content);
            } else {
                FavouriteTripDisplay subcontent = new FavouriteTripDisplay(NavPage, Map, Utility.InBraces(itemText));
                NavPage.PushAsync(subcontent.Content);
            }
        }
        
        private void Refresh(int val) { _favList.ItemsSource = GenTableContent(); }
        
        public MyFavourites(NavigationPage page, DefaultMap map) {
            Map = map;
            NavPage = page;

            _favList = new ListView {
                ItemsSource = GenTableContent()
            };
            
            _favList.ItemSelected += ItemSelected;

            MapUtil.Favorites.OnFavouriteAdded += Refresh;
            
            Content = new ContentPage {
                Content = new Grid {
                    Children = {
                        _favList
                    }
                }
            };
        }

        ~MyFavourites() {
            MapUtil.Favorites.OnFavouriteAdded -= Refresh;
        }
    }
}
