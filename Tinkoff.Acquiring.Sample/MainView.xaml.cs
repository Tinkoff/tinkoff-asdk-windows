#region License

// Copyright © 2016 Tinkoff Bank
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//     http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Tinkoff.Acquiring.Sample.Models;

namespace Tinkoff.Acquiring.Sample
{
    public sealed partial class MainView
    {
        private Color? oldStatusBarBackground;
        private double oldStatusBarOpacity;
        private Color? oldStatusBarForeground;

        public MainView()
        {
            InitializeComponent();
            var content = GetFromResource();
            ListView.ItemsSource = Serializer.Deserialize<Item[]>(content);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.SaveStatusBarColors(ref oldStatusBarBackground, ref oldStatusBarOpacity, ref oldStatusBarForeground);
            this.RecolorStatusBar((Color)Application.Current.Resources["Cerulean"], 1, Colors.White);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.RecolorStatusBar(oldStatusBarBackground, oldStatusBarOpacity, oldStatusBarForeground);
        }

        private void OnBasketClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BasketView));
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutView));
        }

        private void OnMoreButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                var id = button.Tag.ToString();
                if (string.IsNullOrEmpty(id)) return;
                var book = ListView.Items.OfType<Item>().FirstOrDefault(item => item.Id == id);
                if (book == null) return;
                Frame.Navigate(typeof (DetailView), book);
            }
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(DetailView), e.ClickedItem);
        }

        private string GetFromResource()
        {
            try
            {
                var assembly = GetType().GetTypeInfo().Assembly;
                var resource = assembly.GetManifestResourceNames().Single(name => name.Contains("books.json"));
                using (var stream = assembly.GetManifestResourceStream(resource))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
