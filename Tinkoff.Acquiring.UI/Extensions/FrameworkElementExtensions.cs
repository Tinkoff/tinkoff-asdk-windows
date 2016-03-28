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

using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Tinkoff.Acquiring.UI.Extensions
{
    public static class FrameworkElementExtensions
    {
        #region ClipToBounds

        public static readonly DependencyProperty ClipToBoundsProperty = DependencyProperty.RegisterAttached(
            "ClipToBounds",
            typeof (bool),
            typeof (FrameworkElementExtensions),
            new PropertyMetadata(false, OnClipToBoundsChanged));

        public static bool GetClipToBounds(DependencyObject obj)
        {
            return (bool) obj.GetValue(ClipToBoundsProperty);
        }

        public static void SetClipToBounds(DependencyObject obj, bool value)
        {
            obj.SetValue(ClipToBoundsProperty, value);
        }

        private static void OnClipToBoundsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var newClipToBounds = (bool) obj.GetValue(ClipToBoundsProperty);

            SetClipToBoundsHandler(obj, newClipToBounds ? new ClipToBoundsHandler() : null);
        }

        #endregion

        #region ClipToBoundsHandler

        public static readonly DependencyProperty ClipToBoundsHandlerProperty =
            DependencyProperty.RegisterAttached(
                "ClipToBoundsHandler",
                typeof (ClipToBoundsHandler),
                typeof (FrameworkElementExtensions),
                new PropertyMetadata(null, OnClipToBoundsHandlerChanged));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ClipToBoundsHandler GetClipToBoundsHandler(DependencyObject obj)
        {
            return (ClipToBoundsHandler) obj.GetValue(ClipToBoundsHandlerProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetClipToBoundsHandler(DependencyObject obj, ClipToBoundsHandler value)
        {
            obj.SetValue(ClipToBoundsHandlerProperty, value);
        }

        private static void OnClipToBoundsHandlerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var oldClipToBoundsHandler = (ClipToBoundsHandler) args.OldValue;
            var newClipToBoundsHandler = (ClipToBoundsHandler) obj.GetValue(ClipToBoundsHandlerProperty);

            oldClipToBoundsHandler?.Detach();
            newClipToBoundsHandler?.Attach((FrameworkElement) obj);
        }

        #endregion
    }

    public class ClipToBoundsHandler
    {
        private FrameworkElement element;

        public void Attach(FrameworkElement frameworkElement)
        {
            element = frameworkElement;
            UpdateClipGeometry();
            frameworkElement.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (element == null) return;

            UpdateClipGeometry();
        }

        private void UpdateClipGeometry()
        {
            element.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, element.ActualWidth, element.ActualHeight)
            };
        }

        public void Detach()
        {
            if (element == null) return;

            element.SizeChanged -= OnSizeChanged;
            element = null;
        }
    }
}