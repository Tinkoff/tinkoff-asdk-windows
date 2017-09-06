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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Tinkoff.Acquiring.Sdk;
using Tinkoff.Acquiring.UI.Model;

namespace Tinkoff.Acquiring.UI
{
    sealed partial class PaymentView : INotifyPropertyChanged
    {
        #region Fields

        private bool cardIsLoaded;
        private string cardId;
        private Order order;
        private ICardModel cardModel;
        private DisplayOrientations autoRotationsPreferences;

        private static readonly Regex EmailRegex = new Regex("^(([^<>()[\\]\\.,;:\\s@\"]+(\\.[^<>()[\\]\\.,;:\\s@\"]+)*)|(\".+\"))@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\])|(([a-zA-Z\\-0-9]+\\.)+[a-zA-Z]{2,}))$");

        #endregion

        #region Ctor

        public PaymentView()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            CardModel = new DefaultCardModel();
        }

        #endregion

        #region Events

        public event Action<string> Succeeded;
        public event Action Cancelled;
        public event Action<Exception> Failed;

        #endregion

        #region Properties

        private bool IsEmailValid => string.IsNullOrEmpty(Email.Text) || Encoding.UTF8.GetByteCount(Email.Text) == Email.Text.Length && EmailRegex.IsMatch(Email.Text);

        public ICardModel CardModel
        {
            get { return cardModel; }
            set
            {
                if (ReferenceEquals(cardModel, value)) return;

                cardModel = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Base Members

        // TODO: check mode and cache solutions
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            order = e.Parameter as Order;
            if (order == null)
            {
                // TODO: message
                OnFailed(new ArgumentException());
                return;
            }

            if (AcquiringUI.IsStatusBarAvailable)
            {
                var statusBar = StatusBar.GetForCurrentView();
                TitlePanel.Padding = new Thickness(0, statusBar.OccludedRect.Height, 0, 0);
            }

            SetNecessarySettings();

            ProductTitle.Text = order.Title;
            ProductDescription.Text = order.Description;
            var numberFormatInfo = NumberFormatInfo.GetInstance(new CultureInfo("ru"));
            ProductAmount.Text = string.Format(numberFormatInfo, "{0:C2}", order.Amount / 100);
            if (!string.IsNullOrEmpty(order.Email))
            {
                Email.Text = order.Email;
            }
        }

        #endregion

        #region Private Members

        private async void PaymentView_OnLoading(FrameworkElement sender, object args)
        {
            if (cardIsLoaded)
            {
                cardIsLoaded = false;
            }
            else
            {
                try
                {
                    var cardManager = new CardManager();
                    var cards = await cardManager.GetAllCards(order.CustomerKey);
                    if (cards.Count() != 0)
                    {
                        var card = (order.CardId != null
                            ? await cardManager.GetCardById(order.CustomerKey, order.CardId)
                            : null) ?? cards.First();

                        cardId = card.CardId;
                        CardModel = new SavedCardModel(card.Pan, "**/**");
                    }
                }
                catch
                {
                }
            }
            CardControl.Focus(FocusState.Programmatic);
        }

        private async void OnPayClick(object sender, RoutedEventArgs e)
        {
            if (!CardModel.IsValid || !IsEmailValid)
                return;

            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }
            ProgressBar.Visibility = Visibility.Visible;

            try
            {
                var sdk = AcquiringUI.GetAcquiringSdk();
                var paymentId = await sdk.Init(order.Amount, order.OrderId, order.CustomerKey, receipt: order.Receipt);
                if (string.IsNullOrEmpty(paymentId))
                    return;

                var cardData = cardModel is SavedCardModel
                    ? (CardData) new SavedCardData {Id = cardId, SecureCode = cardModel.SecurityCode.Data}
                    : new DefaultCardData
                    {
                        Pan = cardModel.Number.Data,
                        ExpiryDate = cardModel.ExpiryDate.Data,
                        SecureCode = cardModel.SecurityCode.Data
                    };

                var response = await sdk.FinishAuthorize(paymentId, false, cardData, Email.Text == string.Empty ? null : Email.Text);

                if (response.IsThreeDsNeed)
                {
                    if (Frame.Navigate(typeof (SecureView), new SecureViewParams {ThreeDsData = response, PaymentId = paymentId}))
                    {
                        cardIsLoaded = true;
                        SetPreviousSettings();
                        var secureView = (SecureView) Frame.Content;
                        if (secureView == null)
                            return;

                        secureView.Succeeded += () => OnSucceeded(paymentId);
                        secureView.Cancelled += async () =>
                        {
                            await ShowMessageDialogAsync("Оплата была отменена Вами.", "Оплата отменена");
                            OnCancelled();
                        };
                        secureView.Failed += async exception =>
                        {
                            if (exception is WebException)
                            {
                                if (!Frame.CanGoBack)
                                    return;

                                Frame.GoBack();
                                await ShowMessageDialogAsync("Возникли проблемы с интернетом. Проверьте соединение и повторите запрос.", "Проблемы с соединением");
                            }
                            else if (exception is InvalidOperationException)
                            {
                                if (!Frame.CanGoBack)
                                    return;

                                Frame.GoBack();
                                await ShowMessageDialogAsync("Платёж отклонён банком. Убедитесь, что на карте достаточно средств и попробуйте снова.", "Платёж отклонён");
                            }
                            else
                            {
                                OnFailed(exception);
                            }
                        };
                    }
                }
                else
                {
                    OnSucceeded(paymentId);
                }
            }
            catch (AcquiringApiException ex)
            {
                OnFailed(ex);
            }
            catch (AcquiringSdkException)
            {
                await ShowMessageDialogAsync("Возникли проблемы с интернетом. Проверьте соединение и повторите запрос.", "Проблемы с соединением");
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                if (button != null)
                    button.IsEnabled = true;
            }
        }

        private void OnSelectCardClick(object sender, RoutedEventArgs e)
        {
            if (Frame.Navigate(typeof (CardView), new CardViewParams {CustomerKey = order.CustomerKey, CardId = cardId}))
            {
                SetPreviousSettings();
                cardIsLoaded = true;
                var cardView = (CardView) Frame.Content;
                if (cardView == null)
                    return;

                cardView.Selected += card =>
                {
                    Frame.GoBack();
                    cardIsLoaded = true;
                    if (card != null)
                    {
                        CardModel = new SavedCardModel(card.Pan, "**/**");
                        cardId = card.CardId;
                    }
                    else
                    {
                        CardModel = new DefaultCardModel();
                        cardId = null;
                    }
                };
                cardView.Cancelled += () => { Frame.GoBack(); };
            }
        }

        private void Email_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            OnPropertyChanged(nameof(IsEmailValid));
        }

        private void OnBackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            OnCancelled();
        }

        private void OnKeyboardShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            ContentViewer.Padding = new Thickness(0, 0, 0, sender.OccludedRect.Height);
            args.EnsuredFocusedElementInView = true;
        }

        private void OnKeyboardHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            ContentViewer.Padding = new Thickness(0);
        }

        private void SetNecessarySettings()
        {
            if (AcquiringUI.AreHardwareButtonsAvailable)
            {
                HardwareButtons.BackPressed += OnBackPressed;
            }
            autoRotationsPreferences = DisplayProperties.AutoRotationPreferences;
            DisplayProperties.AutoRotationPreferences = DisplayOrientations.Portrait;

            var inputPane = InputPane.GetForCurrentView();
            inputPane.Hiding += OnKeyboardHiding;
            inputPane.Showing += OnKeyboardShowing;
        }

        private void SetPreviousSettings()
        {
            if (AcquiringUI.AreHardwareButtonsAvailable)
            {
                HardwareButtons.BackPressed -= OnBackPressed;
            }
            DisplayProperties.AutoRotationPreferences = autoRotationsPreferences;

            var inputPane = InputPane.GetForCurrentView();
            inputPane.Hiding -= OnKeyboardHiding;
            inputPane.Showing -= OnKeyboardShowing;
        }

        private static async Task ShowMessageDialogAsync(string content, string title)
        {
            var messageDialog = new MessageDialog(content, title);
            await messageDialog.ShowAsync();
        }

        private void OnSucceeded(string paymentId)
        {
            if (paymentId == null)
            {
                OnFailed(new ArgumentNullException(nameof(paymentId)));
                return;
            }

            new CardManager().Clear(order.CustomerKey);
            SetPreviousSettings();
            Succeeded?.Invoke(paymentId);
        }

        private void OnCancelled()
        {
            SetPreviousSettings();
            Cancelled?.Invoke();
        }

        private void OnFailed(Exception exception)
        {
            SetPreviousSettings();
            Failed?.Invoke(exception);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null, [CallerMemberName] string caller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? caller));
        }

        #endregion
    }
}