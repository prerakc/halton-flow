using System;
using System.IO;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TransApp.Formats {
    
    public class ShapeVertex : Utility.Vertex {
        public const int FlagSize = 20;
        
        // Is adjusted/calculated at setup
        public float Distance;
        public ShapeVertex(double latitude, double longitude, float distance = -1)
            : base(latitude, longitude) {
            Distance = distance;
        }
    }

    public class Shape : List<ShapeVertex> {
        public const int FlagSize = 16;

        public readonly int Id;
        public readonly double Distance;

        public Shape(int id, double distance) { Id = id; Distance = distance; }

        public void List() {
            Console.WriteLine("Trip " + Id + "{" + Distance + "}");
            foreach (var vert in this) Console.WriteLine("{" + vert.Latitude + ", " + vert.Longitude + "} ---" + vert.Distance);
        }
        
        public List<Utility.Vertex> AsVertexList() {
            var result = new List<Utility.Vertex>(Count);
            foreach (var vert in this) result.Add(vert);
            return result;
        }
    }
    
    // For .shp
    public class ShapeFile : List<Shape> {
        public ShapeFile(string path) {
            var data = File.ReadAllBytes(path);

            var marker = 0;
            while (marker < data.Length) {
                var id = BitConverter.ToInt32(data, marker);
                var vertexCount = BitConverter.ToInt32(data, marker + 4);
                var length = BitConverter.ToDouble(data, marker + 8);
                var trip = new Shape(id, length);
                for (var a = 0; a < vertexCount; a++) {
                    var lat = BitConverter.ToDouble(data, marker + Shape.FlagSize + a * ShapeVertex.FlagSize);
                    var lon = BitConverter.ToDouble(data, marker + Shape.FlagSize + a * ShapeVertex.FlagSize + 8);
                    var dist = BitConverter.ToSingle(data, marker + Shape.FlagSize + a * ShapeVertex.FlagSize + 16);
                    trip.Add(new ShapeVertex(lat, lon, dist));
                }
                marker += Shape.FlagSize + vertexCount * ShapeVertex.FlagSize;
                Add(trip);
            }
        }
    }
    
}
