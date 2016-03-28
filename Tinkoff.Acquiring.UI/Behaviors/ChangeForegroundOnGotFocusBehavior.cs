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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Tinkoff.Acquiring.UI.Behaviors
{
    class ChangeForegroundOnGotFocusBehavior : Behavior<Control>
    {
        #region Dependency Properties

        public static readonly DependencyProperty FocusedForegroundProperty = DependencyProperty.Register(
            "FocusedForeground",
            typeof (Brush),
            typeof (ChangeForegroundOnGotFocusBehavior),
            new PropertyMetadata(DependencyProperty.UnsetValue, OnFocusedForegroundChangedCallback));

        public static readonly DependencyProperty UnfocusedForegroundProperty = DependencyProperty.Register(
            "UnfocusedForeground",
            typeof (Brush),
            typeof (ChangeForegroundOnGotFocusBehavior),
            new PropertyMetadata(DependencyProperty.UnsetValue, OnUnfocusedForegroundChangedCallback));

        #endregion

        #region Properties

        public Brush FocusedForeground
        {
            get { return (Brush) GetValue(FocusedForegroundProperty); }
            set { SetValue(FocusedForegroundProperty, value); }
        }

        public Brush UnfocusedForeground
        {
            get { return (Brush) GetValue(UnfocusedForegroundProperty); }
            set { SetValue(UnfocusedForegroundProperty, value); }
        }

        #endregion

        #region Overrides of Behavior

        protected override void OnAttached()
        {
            AssociatedObject.GotFocus += OnAssociatedObjectGotFocus;
            AssociatedObject.LostFocus += OnAssociatedObjectLostFocus;
        }

        protected override void OnDetached()
        {
            AssociatedObject.GotFocus -= OnAssociatedObjectGotFocus;
            AssociatedObject.LostFocus -= OnAssociatedObjectLostFocus;
        }

        #endregion

        #region Private Members

        private static void OnFocusedForegroundChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((ChangeForegroundOnGotFocusBehavior) obj).UpdateForeground();
        }

        private static void OnUnfocusedForegroundChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((ChangeForegroundOnGotFocusBehavior) obj).UpdateForeground();
        }

        private void UpdateForeground()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.Foreground = AssociatedObject.FocusState == FocusState.Unfocused
                    ? UnfocusedForeground
                    : FocusedForeground;
            }
        }

        private void OnAssociatedObjectGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.Foreground = FocusedForeground;
        }

        private void OnAssociatedObjectLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.Foreground = UnfocusedForeground;
        }

        #endregion
    }
}