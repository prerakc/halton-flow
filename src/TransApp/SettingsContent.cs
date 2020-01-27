using System;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TransApp {
    public class SettingsContent : TaBaseFields {
        
        public SettingsContent() {

            var userId = new Label {
                Text = "User Id: " + Utility.GetUserId(),
                FontSize = 48,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var demoPhoneNumber = new Label {
                Text = "Demo Phone: " + Utility.DominicPhone,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 40, 0, 0)
            };
            
            Content = new ContentPage {
                Content = new StackLayout {
                    Children = {
                        userId,
                        demoPhoneNumber,
                    }
                }
            };
        }
    }
}
