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

using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace Tinkoff.Acquiring.Sample
{
    static class PageExtensions
    {
        public static void RecolorStatusBar(this Page @this, Color? backgroundColor, double opacity, Color? foregroundColor)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = backgroundColor;
                statusBar.BackgroundOpacity = opacity;
                statusBar.ForegroundColor = foregroundColor;
            }
        }

        public static void SaveStatusBarColors(this Page @this, ref Color? backgroundColor, ref double opacity, ref Color? foregroundColor)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                backgroundColor = statusBar.BackgroundColor;
                opacity = statusBar.BackgroundOpacity;
                foregroundColor = statusBar.ForegroundColor;
            }
        }
    }
}