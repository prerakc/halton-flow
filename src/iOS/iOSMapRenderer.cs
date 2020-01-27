using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

using TransApp;
using TransApp.iOS;
using TransApp.Formats;

using UIKit;
using MapKit;
using CoreGraphics;

using Xamarin.Forms.Maps.iOS;

[assembly: ExportRenderer(typeof(DefaultMap), typeof(IosDefaultMapRenderer))]
namespace TransApp.iOS {
    
    public class IosDefaultMapRenderer : MapRenderer {
        
        private class IdAnnot : MKAnnotationView {
            public readonly int TripId;
            public IdAnnot(IMKAnnotation annot, int id, int tripId) : base(annot, id.ToString()) { this.TripId = tripId; }
        }
        
        //TODO do we need the id field?
        private class IdView : UIView { public int id; }
        
        private IList<Pin> _pins;
        private List<int> _delayedBusIndices;
        
        private IdView _infoView;
        
        private static readonly UIImage BusImage = UIImage.FromFile("BusIcon_Pin.png");
        private static readonly UIImage DelayImage = UIImage.FromFile("DelayIcon_Pin.png");
        
        private int GetPin(MKPointAnnotation fromAnnotation) {
            if (fromAnnotation == null) {
                Console.WriteLine("Passed missing MKPointAnnotation!");
                return -1;
            }
            var pos = new Position(fromAnnotation.Coordinate.Latitude, fromAnnotation.Coordinate.Longitude);
            for (var a = 0; a < _pins.Count; a++)
                if (_pins[a].Position == pos) return a;
            return -1;
        }
        
        IdAnnot CreateIdAnnot(IMKAnnotation annotation, int id, int tripId) {
            var annotationView = new IdAnnot(annotation, id, tripId) {Image = BusImage};
            return annotationView;
        }
        
        private void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e) {
            var customView = (IdAnnot)e.View;
            _infoView = new IdView { id = customView.TripId };

            var trip = MapUtil.GetDefaultTripFile().GetTripWithTripId(customView.TripId);
            if (trip == null) {
                Console.WriteLine("Error! Trip not found.");
                return;
            }

            _infoView.Frame = new CGRect(0, -40, 300, 100);
            var label = new UILabel(new CGRect(0, -40, 300, 100)) {
                Text = trip.Route.Name + " (" + trip.ListStopTimeRangeAsString() + ")"
            };
            _infoView.AddSubview(label);
            _infoView.Center = new CGPoint(0, 0);
            e.View.AddSubview(_infoView);
        }
        
        private void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e) {
            if (e.View.Selected) return;
            _infoView.RemoveFromSuperview();
            _infoView.Dispose();
            _infoView = null;
        }
        
        private MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation) {
            MKAnnotationView annotationView = null;

            if (annotation is MKUserLocation)
                return null;

            var cpin = GetPin(annotation as MKPointAnnotation);
            if (cpin == -1) return CreateIdAnnot(annotation, Utility.GetUniqueName(), -1);

            var defLoc = (DefaultLocation)_pins[cpin];
            defLoc.Annotation = annotation;
            
            var isDelayed = _delayedBusIndices.Contains(cpin);
            
            annotationView = mapView.DequeueReusableAnnotation(defLoc.ViewId.ToString());
            if (annotationView == null) annotationView = CreateIdAnnot(annotation, defLoc.ViewId, defLoc.TripId);

            if (isDelayed) {
                Console.WriteLine("Used delay image.");
                annotationView.Image = DelayImage;
            }
            
            return annotationView;
        }
        
        private void UpdatePins() {
            var nativeMap = (MKMapView)Control;
            foreach (var pin in _pins) {
                var cPin = (DefaultLocation)pin;
                //TODO check if it's worth an `as` cast + a null check
                    cPin.Annotation?.SetCoordinate(
                    new CoreLocation.CLLocationCoordinate2D(cPin.Position.Latitude, cPin.Position.Longitude));
            }
            nativeMap.SetNeedsDisplay();
            SetNeedsDisplay();
        }
        
        protected override void OnElementChanged(Xamarin.Forms.Platform.iOS.ElementChangedEventArgs<View> e) {
            base.OnElementChanged(e);
            
            if (e.OldElement != null) {
                var nativeMap = (MKMapView)Control;
                if (nativeMap != null) {
                    nativeMap.RemoveAnnotations(nativeMap.Annotations);
                    nativeMap.GetViewForAnnotation = null;
                    nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                    nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;
                    ((DefaultMap)e.OldElement).OnPinsUpdated -= UpdatePins;
                }
            }

            if (e.NewElement != null) {
                var formsMap = (DefaultMap)e.NewElement;
                var nativeMap = (MKMapView)Control;
                _pins = formsMap.Pins;
                _delayedBusIndices = formsMap.DelayedBusIndices;
                nativeMap.GetViewForAnnotation = GetViewForAnnotation;
                nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;
                formsMap.OnPinsUpdated += UpdatePins;
            }
            
        }
        
    }
}