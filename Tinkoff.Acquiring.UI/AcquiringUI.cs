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
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Tinkoff.Acquiring.Sdk;
using Tinkoff.Acquiring.UI.Handlers;
using Tinkoff.Acquiring.UI.Model;

namespace Tinkoff.Acquiring.UI
{
    /// <summary>
    /// Класс для отображения предоставляемых SDK экранов.
    /// </summary>
    public static class AcquiringUI
    {
        #region Fields

        private static bool needRestoreStatusBar;
        private static double statusBarBackgroundOpacity;
        private static Color? statusBarForegroundColor;
        private static Color? statusBarBackgroundColor;
        private static ApplicationViewBoundsMode oldBoundsMode;

        private static string terminalKey;
        private static string password;
        private static string publicKey;

        private static bool shown;
        private static TaskCompletionSource<object> dismissTaskSource;
        private static Popup popup;

        #endregion

        #region Public Members

        /// <summary>
        /// Устанавливает ключ терминала, используемый <see cref="AcquiringSdk" />.
        /// </summary>
        /// <param name="terminalKey">Ключ терминала. Выдается после подключения к Тинькофф Эквайринг.</param>
        public static void SetTerminalKey(string terminalKey)
        {
            AcquiringUI.terminalKey = terminalKey;
        }

        /// <summary>
        /// Устанавливает пароль от терминала, используемый <see cref="AcquiringSdk" />.
        /// </summary>
        /// <param name="password">Пароль от терминала. Выдается вместе с terminalKey.</param>
        public static void SetPassword(string password)
        {
            AcquiringUI.password = password;
        }

        /// <summary>
        /// Устанавливает публичный ключ, используемый <see cref="AcquiringSdk" />.
        /// </summary>
        /// <param name="publicKey">Публичный ключ. Выдается вместе с terminalKey.</param>
        public static void SetPublicKey(string publicKey)
        {
            AcquiringUI.publicKey = publicKey;
        }

        /// <summary>
        /// Скрывает показанный экран.
        /// </summary>
        public static async Task HideAsync()
        {
            await RestoreStatusBar();
            DismissDialog();
        }


        /// <summary>
        /// Открывает экран выбора карты.
        /// </summary>
        /// <param name="customerKey">Идентификатор покупателя в системе Продавца.</param>
        /// <param name="handler">Обработчик результата выбора.</param>
        /// <param name="cardId">Карта, выбранная в данный момент.</param>
        /// <returns>
        /// The asynchronous results of the operation. Use this to determine when the async call is complete.
        /// </returns>
        public static async Task ShowCardViewAsync(string customerKey, CardPickerHandler handler, string cardId = null)
        {
            if (shown)
                throw new InvalidOperationException("The dialog is already shown.");
            await SetStatusBar();

            try
            {
                shown = true;
                Subscribe();
                dismissTaskSource = new TaskCompletionSource<object>();
                var container = new Grid();
                var frame = new Frame();
                container.Children.Add(frame);
                popup = new Popup
                {
                    Child = container,
                    IsLightDismissEnabled = false,
                    IsOpen = true
                };

                ResizeLayoutRoot();
                if (frame.Navigate(typeof (CardView), new CardViewParams {CustomerKey = customerKey, CardId = cardId}))
                {
                    var cardView = (CardView) frame.Content;
                    if (cardView != null)
                    {
                        cardView.Selected += async _ => await HideAsync();
                        cardView.Selected += handler.SelectedCallback;
                        cardView.Cancelled += async () => await HideAsync();
                        cardView.Cancelled += handler.CancelledCallback;
                        await dismissTaskSource.Task;
                    }
                }
            }
            finally
            {
                popup.IsOpen = false;
                popup.Child = null;
                popup = null;
                Unsubscribe();
                shown = false;
            }
        }

        /// <summary>
        /// Открывает экран оплаты. Позволяет посмотреть данные платежа, ввести данные карты для оплаты, проходить в случае необходимости 3DS, управлять ранее сохраненными картами.
        /// </summary>
        /// <param name="order">Параметры заказа.</param>
        /// <param name="handler">Обработчик результата платежа.</param>
        /// <returns>
        /// The asynchronous results of the operation. Use this to determine when the async call is complete.
        /// </returns>
        public static async Task ShowPaymentViewAsync(Order order, PaymentHandler handler)
        {
            if (shown)
                throw new InvalidOperationException("The dialog is already shown.");
            await SetStatusBar();

            try
            {
                shown = true;

                Subscribe();

                dismissTaskSource = new TaskCompletionSource<object>();
                var container = new Grid();
                var frame = new Frame();
                container.Children.Add(frame);
                popup = new Popup
                {
                    Child = container,
                    IsLightDismissEnabled = false,
                    IsOpen = true
                };
                ResizeLayoutRoot();
                if (frame.Navigate(typeof (PaymentView), order))
                {
                    var paymentView = (PaymentView) frame.Content;
                    if (paymentView != null)
                    {
                        paymentView.Succeeded += async _ => await HideAsync();
                        paymentView.Succeeded += handler.SucceededCallback;
                        paymentView.Cancelled += async () => await HideAsync();
                        paymentView.Cancelled += handler.CancelledCallback;
                        paymentView.Failed += async _ => await HideAsync();
                        paymentView.Failed += handler.FailedCallback;
                        await dismissTaskSource.Task;
                    }
                }
            }
            finally
            {
                popup.IsOpen = false;
                popup.Child = null;
                popup = null;
                Unsubscribe();
                shown = false;
            }
        }

        /// <summary>
        /// Открывает экран 3D-Secure. Позволяет проходить 3DS.
        /// </summary>
        /// <param name="handler">Обработчик результата проверки 3DS.</param>
        /// <returns>
        /// The asynchronous results of the operation. Use this to determine when the async call is complete.
        /// </returns>
        public static async Task ShowSecureViewAsync(ThreeDsHandler handler)
        {
            if (shown)
                throw new InvalidOperationException("The dialog is already shown.");
            await SetStatusBar();

            try
            {
                shown = true;

                Subscribe();

                dismissTaskSource = new TaskCompletionSource<object>();
                var container = new Grid();
                var frame = new Frame();
                container.Children.Add(frame);
                popup = new Popup
                {
                    Child = container,
                    IsLightDismissEnabled = false,
                    IsOpen = true
                };

                ResizeLayoutRoot();
                if (frame.Navigate(typeof (SecureView)))
                {
                    var secureView = (SecureView) frame.Content;
                    if (secureView != null)
                    {
                        secureView.Succeeded += async () => await HideAsync();
                        secureView.Succeeded += handler.SucceededCallback;
                        secureView.Cancelled += async () => await HideAsync();
                        secureView.Cancelled += handler.CancelledCallback;
                        secureView.Failed += async _ => await HideAsync();
                        secureView.Failed += handler.FailedCallback;
                        await dismissTaskSource.Task;
                    }
                }
            }
            finally
            {
                popup.IsOpen = false;
                popup.Child = null;
                popup = null;
                Unsubscribe();
                shown = false;
            }

        }

        #endregion

        #region Internal Members

        internal static bool AreHardwareButtonsAvailable => ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");

        internal static bool IsStatusBarAvailable => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        internal static AcquiringSdk GetAcquiringSdk()
        {
            return new AcquiringSdk(terminalKey, password, new StringKeyCreator(publicKey)) {IsDebug = true};
        }

        #endregion

        #region Private Members

        private static async Task SetStatusBar()
        {
            if (IsStatusBarAvailable)
            {
                var statusBar = StatusBar.GetForCurrentView();
                needRestoreStatusBar = statusBar.OccludedRect.IsEmpty;

                statusBarBackgroundOpacity = statusBar.BackgroundOpacity;
                statusBarBackgroundColor = statusBar.BackgroundColor;
                statusBarForegroundColor = statusBar.ForegroundColor;

                statusBar.BackgroundOpacity = 0;
                statusBar.BackgroundColor = Color.FromArgb(255, 52, 71, 87);
                statusBar.ForegroundColor = Colors.White;

                if (needRestoreStatusBar)
                    await statusBar.ShowAsync();

                oldBoundsMode = ApplicationView.GetForCurrentView().DesiredBoundsMode;
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }
        }

        private static async Task RestoreStatusBar()
        {
            if (IsStatusBarAvailable)
            {
                var statusBar = StatusBar.GetForCurrentView();

                statusBar.BackgroundOpacity = statusBarBackgroundOpacity;
                statusBar.BackgroundColor = statusBarBackgroundColor;
                statusBar.ForegroundColor = statusBarForegroundColor;

                if (needRestoreStatusBar)
                    await statusBar.HideAsync();

                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(oldBoundsMode);
            }
        }

        private static void OnSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            ResizeLayoutRoot();
        }

        private static void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
                DismissDialog();
        }

        private static void ResizeLayoutRoot()
        {
            var grid = popup?.Child as Grid;
            if (grid == null) return;
            grid.Width = Window.Current.Bounds.Width;
            grid.Height = Window.Current.Bounds.Height;
        }

        private static void DismissDialog()
        {
            dismissTaskSource?.TrySetResult(null);
        }

        private static void Subscribe()
        {
            Window.Current.SizeChanged += OnSizeChanged;
            Window.Current.Content.KeyUp += OnKeyUp;
        }

        private static void Unsubscribe()
        {
            Window.Current.SizeChanged -= OnSizeChanged;
            Window.Current.Content.KeyUp -= OnKeyUp;
        }

        #endregion
    }
}
