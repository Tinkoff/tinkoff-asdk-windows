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

using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Tinkoff.Acquiring.Sdk;

namespace Tinkoff.Acquiring.Sample
{
    public sealed partial class AboutView
    {
        private Color? oldStatusBarBackground;
        private double oldStatusBarOpacity;
        private Color? oldStatusBarForeground;

        public AboutView()
        {
            this.InitializeComponent();
            VersionTextBlock.Text = GetAssemblyVersion(typeof(AcquiringSdk).GetTypeInfo().Assembly);
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

        private static string GetAssemblyVersion(Assembly assembly)
        {
            var version = assembly.GetName().Version;
            return $"SDK v {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
