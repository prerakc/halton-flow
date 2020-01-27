using System;
using System.IO;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TransApp.Formats {
    public class StopTime {
        public const int FlagSize = 9;
        public readonly int StopId;
        public readonly byte Hour, Minute, Second;
        public readonly byte PickupType, DropoffType;
        
        public bool After(StopTime time) {
            if (time.Hour > Hour) return true;
            if (time.Hour != Hour) return false;
            if (time.Minute > Minute) return true;
            if (time.Minute != Minute) return false;
            return time.Second > Second;
        }

        public bool Before(StopTime time) {
            if (time.Hour < Hour) return true;
            if (time.Hour != Hour) return false;
            if (time.Minute < Minute) return true;
            if (time.Minute != Minute) return false;
            return time.Second < Second;
        }
        
        public int InSeconds() { return Hour * 3600 + Minute * 60 + Second; }
        
        public StopTime(int stopId, byte hour, byte minute, byte second, byte pickupType, byte dropoffType) {
            StopId = stopId;
            Hour = hour; Minute = minute; Second = second;
            PickupType = pickupType; DropoffType = dropoffType;
        }
        
        public StopTime(DateTime time) {
            Hour = (byte)time.Hour; Minute = (byte)time.Minute; Second = (byte)time.Second;
        }
        
        public bool Equals(DateTime time) {
            return time.Hour == Hour && time.Minute == Minute && time.Second == Second;
        }
        public bool Equals(StopTime time) {
            return time.Hour == Hour && time.Minute == Minute && time.Second == Second;
        }

        public string ListAsString() {
            return Hour + ":" + Minute + ":" + Second;
        }
    }

    public class TripTimes : List<StopTime> {
        public const int FlagSize = 8;
        public readonly int TripId;
        public TripTimes(int tripId) { TripId = tripId; }
        public StopTime ForStop(Stop stop) {
            foreach (var time in this) if (time.StopId == stop.Id) return time;
            return null;
        }
        public bool Contains(Stop stop) { return ForStop(stop) != null; }
        public void List() {
            Console.WriteLine("TripTime for trip " + TripId);
            foreach (var time in this) {
                Console.WriteLine("\t[" + time.StopId + "] " + time.Hour + ":" + time.Minute + ":" + time.Second + "{" + time.PickupType + ", " + time.DropoffType + "}");
            }
        }
    }
    
    // For .tms
    public class TripTimeFile : List<TripTimes> {
        public TripTimeFile(string path) {
            var data = File.ReadAllBytes(path);

            var marker = 0;
            while (marker < data.Length) {
                var id = BitConverter.ToInt32(data, marker);
                var timeCount = BitConverter.ToInt32(data, marker + 4);
                var tripTimes = new TripTimes(id);
                for (var a = 0; a < timeCount; a++) {
                    var stopId = BitConverter.ToInt32(data, marker + TripTimes.FlagSize + a * StopTime.FlagSize);
                    var hour = data[marker + TripTimes.FlagSize + a * StopTime.FlagSize + 4];
                    var minute = data[marker + TripTimes.FlagSize + a * StopTime.FlagSize + 5];
                    var second = data[marker + TripTimes.FlagSize + a * StopTime.FlagSize + 6];
                    var pickupType = data[marker + TripTimes.FlagSize + a * StopTime.FlagSize + 7];
                    var dropoffType = data[marker + TripTimes.FlagSize + a * StopTime.FlagSize + 8];
                    tripTimes.Add(new StopTime(stopId, hour, minute, second, pickupType, dropoffType));
                }
                Add(tripTimes);
                marker += TripTimes.FlagSize + timeCount * StopTime.FlagSize;
            }
        }
    }
}
