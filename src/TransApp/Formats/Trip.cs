using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TransApp.Formats {
    // Made this file because TripFile.cs was getting too big!
    public class Trip {

        public const int FlagSize = 21;
        public readonly int Id, ServiceId, ShapeId, BlockId, RouteId;
        public readonly byte Direction;

        // Bound information
        private Shape _shape = null;
        private TripTimes _tripTimes = null;
        
        public Route Route;
        public List<FullStop> Stops = new List<FullStop>();

        public Utility.Vertex GetBusLocation(StopTime time) {
            var ctimeSeconds = time.InSeconds() - (MapUtil.IsTripDelayed(Id) ? MapUtil.DelayTable[Id] : 0);
            for (var a = 0; a < Stops.Count - 1; a++) {
                FullStop thisStop = Stops[a], nextStop = Stops[a + 1];
                if (!thisStop.StopTime.After(time) || !nextStop.StopTime.Before(time)) continue;
                var progress = (double)(ctimeSeconds - thisStop.StopTime.InSeconds()) / (double)(nextStop.StopTime.InSeconds() - thisStop.StopTime.InSeconds());
                for (var b = 0; b < thisStop.DirectionsToNext.Count - 1; b++) {
                    ShapeVertex thisVertex = thisStop.DirectionsToNext[b], nextVertex = thisStop.DirectionsToNext[b + 1];
                    if (progress >= thisVertex.Distance && progress <= nextVertex.Distance) {
                        return thisVertex.VertexOnLine(
                            nextVertex, (double)(progress - thisVertex.Distance) / (double)(nextVertex.Distance - thisVertex.Distance));
                    }
                }
            }
            return null;
        }
        
        public bool BindRoute(RouteFile routes) {
            foreach (var rut in routes) if (rut.Id == RouteId) { Route = rut; return true; }
            return false;
        }
        
        public bool BindShape(ShapeFile shapes) {
            foreach (var shp in shapes) if (shp.Id == ShapeId) { _shape = shp; return true; }
            return false;
        }

        public bool BindTripTime(TripTimeFile times) {
            foreach (var tms in times) if (tms.TripId == Id) { _tripTimes = tms; return true; }
            return false;
        }

        public bool BindStop(StopFile fstops) {
            if (_tripTimes == null) return false;
            var result = false;
            foreach (var stop in fstops) {
                var stopTime = _tripTimes.ForStop(stop);
                if (stopTime != null) { Stops.Add(new FullStop(stop, stopTime)); result = true; }
            }
            _tripTimes = null;
            return result;
        }

        public bool MixInStop() {
            if (_shape == null) return false;
            foreach (var stop in Stops)
                stop.DirectionsIndex = stop.IndexOfNearestVertex(_shape.AsVertexList());
            // Do we need to sort this, or is it already sorted for us?
            Stops.Sort((x, y) => x.DirectionsIndex - y.DirectionsIndex);
            var recordingIndex = 0;
            for (var a = 0; a < _shape.Count; a++) {
                if (recordingIndex + 1 < Stops.Count && Stops[recordingIndex + 1].DirectionsIndex <= a) recordingIndex++;
                Stops[recordingIndex].DirectionsToNext.Add(_shape[a]);
            }
            foreach (var stop in Stops) {
                if (stop.DirectionsToNext.Count < 1) continue;
                float firstDistance = stop.FirstDirection().Distance, lastDistance = stop.LastDirection().Distance;
                foreach (var vert in stop.DirectionsToNext)
                    vert.Distance = (vert.Distance - firstDistance) / (lastDistance - firstDistance);
            }
            _shape = null;
            // I might skip the next task :P
            return true; //TODO get feedback of operation
        }

        public void List() {
            Console.WriteLine("Trip " + Id + " [shapeId: " + ShapeId + ", serviceId: " + ServiceId + ", blockId: " + BlockId + "] " + (Direction == 1 ? "<--" : "-->"));
            foreach (var stop in Stops) {
                Console.WriteLine("\tStop " + stop.Id + "\n\tDirections to next stop:");
                foreach (var vert in stop.DirectionsToNext) Console.WriteLine("\t\t{" + vert.Latitude + ", " + vert.Longitude + "}");
            }
        }
        
        public string ListStopTimeRangeAsString() {
            return Stops[0].StopTime.ListAsString() + " - " + Stops[Stops.Count - 1].StopTime.ListAsString();
        }

        public Trip(int tripId, int serviceId, int shapeId, int blockId, int routeId, byte direction) {
            Id = tripId; ServiceId = serviceId; ShapeId = shapeId; BlockId = blockId; RouteId = routeId;
            Direction = direction;
        }
    }
}
