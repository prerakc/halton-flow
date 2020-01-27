using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

using Xamarin.Forms.Maps;

namespace TransApp {
    public class Utility {
        public const string DominicPhone = "000000000";
        public const string DominicFlow = "http://flow.dmfj.io/api";
        public static readonly string DominicJson = File.ReadAllText("Dominic.json");
        
        private const string UserFile = "userfile.dat";
        
        private static int _lastName;
        public static int GetUniqueName() { unchecked { return _lastName++; } }
        
        public static int InBraces(string text) {
            int firstBrace = text.IndexOf('('), secondBrace = text.IndexOf(')');
            return int.Parse(text.Substring(firstBrace + 1, secondBrace - firstBrace - 1));
        }

        private static int _defaultUserId = -1;
        public static int GetUserId() {
            if (_defaultUserId != -1) return _defaultUserId;
            //TODO make local id work
            var userfile = new FileInfo(UserFile);
            if (userfile.Exists) {
                Console.WriteLine("Read ID");
                var data = File.ReadAllBytes(UserFile);
                _defaultUserId = BitConverter.ToInt32(data, 0);
            } else {
                Console.WriteLine("Get ID");
                var id = RequestHttpGet(DominicFlow, "/users?phone=" + Utility.DominicPhone);
                _defaultUserId = id == "" ? 1 : int.Parse(id);
            }
            return _defaultUserId;
        }
        
        public static string RequestHttpGet(string address, string append) {
            try {
                var request = WebRequest.Create(address + append);
                var requestStream = request.GetResponse().GetResponseStream();
                if (requestStream == null) {
                    Console.WriteLine("Could not get request stream.");
                    return "";
                }
                var dataStream = new StreamReader(requestStream);
                return dataStream.ReadToEnd();
            } catch (Exception e) {
                Console.WriteLine("Internet Error: " + e.Message);
                return "";
            }
        }
        
        // May want to use this in gathering Android files
        public static void ListFiles() {
            var info = new DirectoryInfo(".");

            var dirs = info.EnumerateDirectories();
            var files = info.EnumerateFiles();

            Console.WriteLine("--- DIRECTORIES ---");
            foreach (var dir in dirs) Console.WriteLine(dir.Name);
            Console.WriteLine("--- FILES ---");
            foreach (var fil in files) Console.WriteLine(fil.Name);
        }
        
        public class Vertex : ICloneable {

            public double Latitude, Longitude;
            public Vertex(double latitude, double longitude) {
                Latitude = latitude;
                Longitude = longitude;
            }

            public void Pin(IList<Pin> pins, IList<int> indices, int tripId) {
                var loc = new DefaultLocation(tripId) {
                    Position = new Position(Latitude, Longitude),
                    Label = ""
                };
                indices?.Add(pins.Count);
                pins.Add(loc);
            }
            
            public object Clone() { return new Vertex(Latitude, Longitude); }

            public override string ToString() { return "{" + Latitude + ", " + Longitude + "}"; }
            
            public virtual void List() { Console.WriteLine(ToString()); }

            public Position AsPosition() { return new Position(Latitude, Longitude); }
            
            public Vertex VertexOnLine(Vertex oppositeVertex, double point) {
                var vert = (Vertex)Clone();
                vert.Latitude += point * (oppositeVertex.Latitude - Latitude);
                vert.Longitude += point * (oppositeVertex.Longitude - Longitude);
                return vert;
            }

            public int IndexOfNearestVertex(List<Vertex> vertices) {
                var bidDistance = double.MaxValue;
                var bidLocation = -1;
                for (var a = 0; a < vertices.Count; a++) {
                    var cdistance = Math.Sqrt(Math.Pow(vertices[a].Latitude - Latitude, 2) + Math.Pow(vertices[a].Longitude - Longitude, 2));
                    if (!(cdistance < bidDistance)) continue;
                    bidDistance = cdistance;
                    bidLocation = a;
                }
                return bidLocation;
            }
        }
    }
}