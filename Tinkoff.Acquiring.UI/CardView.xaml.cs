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
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Tinkoff.Acquiring.Sdk;
using Tinkoff.Acquiring.UI.Model;

namespace Tinkoff.Acquiring.UI
{
    sealed partial class CardView
    {
        #region Fields
        
        private string customerKey;
        private string cardId;

        #endregion

        #region Ctor

        public CardView()
        {
            Cards = new ObservableCollection<CardItem>();
            InitializeComponent();
        }

        #endregion

        #region Events

        public event Action<Card> Selected;
        public event Action Cancelled;

        #endregion

        #region Properties

        private ObservableCollection<CardItem> Cards { get; }

        #endregion

        #region Base Members

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = e.Parameter as CardViewParams;
            customerKey = parameter.CustomerKey;
            cardId = parameter.CardId;

            if (AcquiringUI.IsStatusBarAvailable)
            {
                var statusBar = StatusBar.GetForCurrentView();
                TitlePanel.Padding = new Thickness(0, statusBar.OccludedRect.Height, 0, 0);
            }

            if (AcquiringUI.AreHardwareButtonsAvailable)
            {
                HardwareButtons.BackPressed += OnBackPressed;
            }
        }

        #endregion

        #region Private Members

        private async void CardView_OnLoading(FrameworkElement sender, object args)
        {
            ProgressBar.Visibility = Visibility.Visible;
            try
            {
                ListView.Header = "Обновление списка карт";
                var cardManager = new CardManager();
                var cards = await cardManager.GetAllCards(customerKey);
                foreach (var card in cards)
                {
                    Cards.Add(new CardItem
                    {
                        Card = card,
                        Selected = card.CardId == cardId
                    });
                }
            }
            catch
            {
                var messageDialog = new MessageDialog("Возникли проблемы с интернетом. Проверьте соединение и повторите запрос.", "Проблемы с соединением")
                {
                    CancelCommandIndex = 0,
                    Commands = { new UICommand("Ок") }
                };
                await messageDialog.ShowAsync();
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ListView.Header = Cards.Any() ? string.Empty : "Нет сохраненных карт";
            }
        }

        private void OnNewCardClick(object sender, RoutedEventArgs e)
        {
            OnSelected(null);
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            OnSelected((e.ClickedItem as CardItem)?.Card);
        }

        private void OnItemHolding(object sender, HoldingRoutedEventArgs e)
        {
            ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void OnItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowAttachedFlyout(sender as FrameworkElement);
        }

        private async void OnRemoveItemClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var card = item?.DataContext as CardItem;
            if (card == null) return;

            ProgressBar.Visibility = Visibility.Visible;
            try
            {
                var cardManager = new CardManager();
                var result = await cardManager.RemoveCardById(customerKey, card.Card.CardId);
                if (result)
                    Cards.Remove(card);
            }
            catch
            {
                var messageDialog = new MessageDialog("Не удалось удалить карту. Попробуйте снова.", "Карта не удалена");
                await messageDialog.ShowAsync();
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ListView.Header = Cards.Any() ? string.Empty : "Нет сохраненных карт";
            }
        }

        private void ShowAttachedFlyout(FrameworkElement element)
        {
            if (element == null) return;

            var flyoutBase = FlyoutBase.GetAttachedFlyout(element);
            flyoutBase?.ShowAt(element);
        }

        private void OnBackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            OnCancelled();
        }

        private void OnSelected(Card card)
        {
            if (AcquiringUI.AreHardwareButtonsAvailable)
            {
                HardwareButtons.BackPressed -= OnBackPressed;
            }
            Selected?.Invoke(card);
        }

        private void OnCancelled()
        {
            if (AcquiringUI.AreHardwareButtonsAvailable)
            {
                HardwareButtons.BackPressed -= OnBackPressed;
            }
            Cancelled?.Invoke();
        }

        private class CardItem
        {
            public Card Card { get; set; }
            public bool Selected { get; set; }
        }

        #endregion
    }
}