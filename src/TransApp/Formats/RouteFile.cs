using System;
using System.IO;
using System.Collections.Generic;

namespace TransApp.Formats {
    public class Route {
        public readonly int Id, ShortName;
        public readonly string Name;
        
        public Route(int id, int shortName, string name) {
            Id = id; ShortName = shortName; Name = name;
        }
    }
    
    public class RouteFile : List<Route> {
        
        public Route GetRouteWithShortName(int shortName) {
            foreach (var route in this) if (route.ShortName == shortName) return route;
            return null;
        }
        
        public RouteFile(string path) {
            var data = File.ReadAllBytes(path);
            
            var marker = 0;

            while (marker < data.Length) {
                var id = BitConverter.ToInt32(data, marker);
                var shortName = BitConverter.ToInt32(data, marker + 4);
                var nameLength = BitConverter.ToInt32(data, marker + 8);
                var name = "";
                for (var a = 0; a < nameLength; a++)
                    name += (char)data[marker + 12 + a];
                Add(new Route(id, shortName, name));
                marker += 12 + nameLength;
            }
        }
    }
}
