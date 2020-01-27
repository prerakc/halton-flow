using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TransApp.Formats {
    
    public class Stop : Utility.Vertex {
        public readonly int Id;
        public readonly string Name;
        
        public int Pointer;
        
        public Stop(string name, int id, double latitude, double longitude) : base(latitude, longitude) {
            Name = name; Id = id;
        }

        public override void List() {
            Console.Write("[" + Id + "] " + Name);
            base.List();
        }
    }
    
    public class FullStop : Stop {
        public readonly StopTime StopTime;
        public int DirectionsIndex;
        public readonly List<ShapeVertex> DirectionsToNext = new List<ShapeVertex>();
        
        public ShapeVertex FirstDirection() { return DirectionsToNext[0]; }
        public ShapeVertex LastDirection() { return DirectionsToNext[DirectionsToNext.Count - 1]; }
        
        public FullStop(Stop original, StopTime time)
            : base(original.Name, original.Id, original.Latitude, original.Longitude) {
            Pointer = original.Pointer;
            StopTime = time;
        }
    }
    
    
    // For .stp
    public class StopFile : List<Stop> {

        public StopFile(string path) {
            var data = File.ReadAllBytes(path);

            var marker = 0;

            while (marker < data.Length) {
                var id = BitConverter.ToInt32(data, marker);
                var latitude = BitConverter.ToDouble(data, marker + 4);
                var longitude = BitConverter.ToDouble(data, marker + 12);
                var nameLength = BitConverter.ToInt32(data, marker + 20);
                var nameBuilder = new StringBuilder();
                for (var a = 0; a < nameLength; a++)
                    nameBuilder.Append((char)data[marker + 24 + a]);
                Add(new Stop(nameBuilder.ToString(), id, latitude, longitude));
                marker += 24 + nameLength;
            }
        }
    }
}
