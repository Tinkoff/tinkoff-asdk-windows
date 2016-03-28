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

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Tinkoff.Acquiring.Sample
{
    public sealed partial class ConfirmationView
    {
        private Color? oldStatusBarBackground;
        private double oldStatusBarOpacity;
        private Color? oldStatusBarForeground;

        public ConfirmationView()
        {
            InitializeComponent();
        }

        public decimal Price { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                Price = (decimal) e.Parameter;
            }

            this.SaveStatusBarColors(ref oldStatusBarBackground, ref oldStatusBarOpacity, ref oldStatusBarForeground);
            this.RecolorStatusBar((Color)Application.Current.Resources["Cerulean"], 1, Colors.White);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.RecolorStatusBar(oldStatusBarBackground, oldStatusBarOpacity, oldStatusBarForeground);
        }
    }
}