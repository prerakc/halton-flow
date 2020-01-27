//#define TRIPFILE_DO_REMOVE_DUPLICATES

using System;
using System.IO;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TransApp.Formats {

    public class TripFile : List<Trip> {
        
        public void BindRoutes(RouteFile routes) {
            foreach (var trip in this) if (!trip.BindRoute(routes)) Console.WriteLine("[BIND_ROUTE_ISSUE]");
        }

        public void BindShapes(ShapeFile shapes) {
            foreach (var trip in this) if (!trip.BindShape(shapes)) Console.WriteLine("[BIND_SHAPE_ISSUE]");
        }

        public void BindTripTimes(TripTimeFile times) {
            foreach (var trip in this) if (!trip.BindTripTime(times)) Console.WriteLine("[BIND_TIMES_ISSUE]");
        }
        
        public void BindStops(StopFile fstops) {
            foreach (var trip in this) if (!trip.BindStop(fstops)) Console.WriteLine("[BIND_STOPS_ISSUE]");
        }

        public void MixInStops() {
            foreach (var trip in this) if (!trip.MixInStop()) Console.WriteLine("[BIND_MIXIN_ISSUE]");
        }
        
        public Trip GetTripWithTripId(int tripId) {
            foreach (var trip in this) if (trip.Id == tripId) return trip;
            return null;
        }
        
        public List<Trip> GetTripsWithRouteId(int routeId) {
            var allTrips = new List<Trip>();
            foreach (var trip in this) {
                if (trip.RouteId == routeId) {
                    #if TRIPFILE_DO_REMOVE_DUPLICATES
                    StopTime thisStoptime = trip.Stops[0].StopTime, lastStoptime = trip.Stops[trip.Stops.Count - 1].StopTime;
                    var noDuplicates = true;
                    foreach (var strip in allTrips) {
                        if (!strip.Stops[0].StopTime.Equals(thisStoptime) ||
                            !strip.Stops[strip.Stops.Count - 1].StopTime.Equals(lastStoptime)) continue;
                        noDuplicates = false; break;
                    }
                    if (noDuplicates)
                    #endif
                        allTrips.Add(trip);
                }
            }
            allTrips.Sort((tripa, tripb) => tripa.Stops[0].StopTime.InSeconds() - tripb.Stops[0].StopTime.InSeconds());
            return allTrips;
        }
        
        public void List() {
            foreach (var trip in this) trip.List();
        }

        public TripFile(string path) {
            var data = File.ReadAllBytes(path);

            var marker = 0;
            while (marker < data.Length) {
                var tripId = BitConverter.ToInt32(data, marker);
                var serviceId = BitConverter.ToInt32(data, marker + 4);
                var shapeId = BitConverter.ToInt32(data, marker + 8);
                var blockId = BitConverter.ToInt32(data, marker + 12);
                var routeId = BitConverter.ToInt32(data, marker + 16);
                var direction = data[marker + 20];
                Add(new Trip(tripId, serviceId, shapeId, blockId, routeId, direction));
                marker += Trip.FlagSize;
            }
        }
    }
}
