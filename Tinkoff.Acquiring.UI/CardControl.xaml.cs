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
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Tinkoff.Acquiring.UI.Extensions;
using Tinkoff.Acquiring.UI.Model;

namespace Tinkoff.Acquiring.UI
{
    sealed partial class CardControl
    {
        #region Fields

        private bool modelJustSet;
        private bool numberJustSet;
        private bool dateJustSet;
        private bool dateChanged;
        private bool isSecurityCodeEmpty;
        private CancellationTokenSource cancellationTokenSource;
        private ControlState controlState = ControlState.Initial;
        private PaymentSystem paymentSystem = PaymentSystem.Undefined;

        #endregion

        #region Ctor

        public CardControl()
        {
            InitializeComponent();
            
            BorderThickness = new Thickness(2);
            BorderBrush = Resources["TextBoxBorderThemeBrush"] as Brush;
            ErrorBrush = Resources["ErrorBrush"] as Brush;
            FocusedForeground = Resources["HighlightBrush"] as Brush;
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CardModelProperty = DependencyProperty.Register(
            "CardModel",
            typeof(ICardModel),
            typeof(CardControl),
            new PropertyMetadata(new DefaultCardModel(), CardModelChangedCallback));

        public static readonly DependencyProperty BrandLogoProperty = DependencyProperty.Register(
            "BrandLogo",
            typeof(string),
            typeof(CardControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CardNumberPlaceholderProperty = DependencyProperty.Register(
            "CardNumberPlaceholder",
            typeof(string),
            typeof(CardControl),
            new PropertyMetadata("Номер карты"));

        public static readonly DependencyProperty ExpiryDatePlaceholderProperty = DependencyProperty.Register(
            "ExpiryDatePlaceholder",
            typeof(string),
            typeof(CardControl),
            new PropertyMetadata("ММ/ГГ"));

        public static readonly DependencyProperty CvcPlaceholderProperty = DependencyProperty.Register(
            "CvcPlaceholder",
            typeof(string),
            typeof(CardControl),
            new PropertyMetadata("CVC"));

        public static readonly DependencyProperty FocusedForegroundProperty = DependencyProperty.Register(
            "FocusedForeground",
            typeof(Brush),
            typeof(CardControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty UnfocusedForegroundProperty = DependencyProperty.Register(
            "UnfocusedForeground",
            typeof(Brush),
            typeof(CardControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ErrorBrushProperty = DependencyProperty.Register(
            "ErrorBrush",
            typeof(Brush),
            typeof(CardControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.Register(
            "IsFocused",
            typeof(bool),
            typeof(CardControl),
            new PropertyMetadata(false));

        #endregion

        #region Properties

        public ICardModel CardModel
        {
            get { return (ICardModel) GetValue(CardModelProperty); }
            set { SetValue(CardModelProperty, value); }
        }

        public string BrandLogo
        {
            get { return (string) GetValue(BrandLogoProperty); }
            set { SetValue(BrandLogoProperty, value); }
        }

        public string CardNumberPlaceholder
        {
            get { return (string) GetValue(CardNumberPlaceholderProperty); }
            set { SetValue(CardNumberPlaceholderProperty, value); }
        }

        public string ExpiryDatePlaceholder
        {
            get { return (string) GetValue(ExpiryDatePlaceholderProperty); }
            set { SetValue(ExpiryDatePlaceholderProperty, value); }
        }

        public string CvcPlaceholder
        {
            get { return (string) GetValue(CvcPlaceholderProperty); }
            set { SetValue(CvcPlaceholderProperty, value); }
        }

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

        public Brush ErrorBrush
        {
            get { return (Brush) GetValue(ErrorBrushProperty); }
            set { SetValue(ErrorBrushProperty, value); }
        }

        public bool IsFocused
        {
            get { return (bool) GetValue(IsFocusedProperty); }
            set { SetValue(IsFocusedProperty, value); }
        }

        #endregion

        #region Base Members

        protected override async void OnGotFocus(RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
            IsFocused = true;
            VisualStateManager.GoToState(this, "FocusedState", false);
            if (modelJustSet)
            {
                if (controlState == ControlState.NumberInput || controlState == ControlState.Initial)
                {
                    await SetNextStateAsync();
                }
                else
                {
                    SetFocus(controlState);
                }
                modelJustSet = false;
            }
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            Task.Delay(TimeSpan.FromMilliseconds(100), cancellationTokenSource.Token)
                .ContinueWith(async _ =>
                {
                    IsFocused = false;
                    if (CardModel.Number.IsValid)
                    {
                        if (controlState == ControlState.NumberInput)
                        {
                            HideAdditionalButtons();
                            controlState = ControlState.Unfocused;
                            await CollapseNumberInputField();
                        }

                        VisualStateManager.GoToState(this, !CardModel.IsValid ? "InvalidState" : "DefaultState", false);
                    }
                    else if (!string.IsNullOrEmpty(CardNumberInputTextBox.Text))
                    {
                        VisualStateManager.GoToState(this, "InvalidState", false);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "DefaultState", false);
                    }
                }, cancellationTokenSource.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.FromCurrentSynchronizationContext());
            base.OnLostFocus(e);
        }

        #endregion

        #region Event Handlers

        private async void NextStateButton_OnClick(object sender, RoutedEventArgs e)
        {
            await SetNextStateAsync();
        }

        private void DeleteNumberButton_OnClick(object sender, RoutedEventArgs e)
        {
            ResetCardModel();
        }

        private void CardNumberInputTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (controlState != ControlState.NumberInput)
            {
                SetState(ControlState.NumberInput);
            }
            VisualStateManager.GoToState(this, "NotValidatedState", false);
        }

        private void CardNumberInputTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (CardNumberInputTextBox.Text.Length != 0 && !CardModel.Number.IsValid)
            {
                VisualStateManager.GoToState(this, "CardNumberInvalidState", false);
            }
        }

        private async void CardNumberInputTextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (numberJustSet)
            {
                numberJustSet = false;
                return;
            }

            var selectionStart = sender.SelectionStart;

            if (CardModel.Number.IsReadOnly)
            {
                sender.Text = CardModel.Number.FormattedData;
                sender.SelectionStart = selectionStart;
                return;
            }

            var data = sender.Text;
            var isDeleteAction = data.Length < CardModel.Number.FormattedData.Length;
            try
            {
                CardModel.Number.Data = data;

                EnsurePaymentSystemState();
            }
            catch (ArgumentException)
            {
                if (data.Replace(" ", string.Empty).Length > CardModel.Number.MaxLength)
                {
                    RemoveLastAddedChar(sender, selectionStart);
                }
                else
                {
                    VisualStateManager.GoToState(this, "CardNumberInvalidState", false);
                }
                return;
            }

            VisualStateManager.GoToState(this, "NotValidatedState", false);
            sender.Text = CardModel.Number.FormattedData;
            if (isDeleteAction)
            {
                sender.SelectionStart = selectionStart;
            }
            else
            {
                sender.SelectionStart = selectionStart + (sender.Text.Length - data.Length);
            }

            HideAdditionalButtons();
            if (CardModel.Number.IsValid)
            {
                await SetNextStateAsync();
            }
            else if (!string.IsNullOrEmpty(sender.Text))
            {
                ShowDeleteButton();
            }
        }

        private void DateTextBox_OnKeyDown(object sender, KeyRoutedEventArgs args)
        {
            if (!dateChanged && (args.Key == VirtualKey.Delete || args.Key == VirtualKey.Back))
            {
                SetState(ControlState.NumberInput);
            }
            dateChanged = false;
        }

        private void DateTextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (dateJustSet)
            {
                dateJustSet = false;
                return;
            }

            var selectionStart = sender.SelectionStart;

            if (CardModel.ExpiryDate.IsReadOnly)
            {
                sender.Text = CardModel.ExpiryDate.FormattedData;
                sender.SelectionStart = selectionStart;
                return;
            }

            dateChanged = true;
            var data = sender.Text;
            var isDeleteAction = data.Length < CardModel.ExpiryDate.FormattedData.Length;
            try
            {
                CardModel.ExpiryDate.Data = sender.Text;
            }
            catch (ArgumentException)
            {
            }

            sender.Text = CardModel.ExpiryDate.FormattedData;
            if (isDeleteAction)
            {
                sender.SelectionStart = selectionStart;
            }
            else
            {
                sender.SelectionStart = selectionStart + (sender.Text.Length - data.Length);
            }

            if (CardModel.ExpiryDate.IsValid)
            {
                SetState(ControlState.SecurityCodeInput);
            }
        }

        private void SecurityCodeTextBox_OnKeyDown(object sender, KeyRoutedEventArgs args)
        {
            if (SecurityCodePasswordBox.Password.Length == 0)
            {
                if (isSecurityCodeEmpty && (args.Key == VirtualKey.Delete || args.Key == VirtualKey.Back))
                {
                    SetState(ControlState.DateInput);
                }
                isSecurityCodeEmpty = true;
            }
        }

        private void SecurityCodePasswordBox_OnPasswordChanged(object sender, RoutedEventArgs args)
        {
            try
            {
                CardModel.SecurityCode.Data = SecurityCodePasswordBox.Password;
            }
            catch (ArgumentException)
            {
                
            }

            SecurityCodePasswordBox.Password = CardModel.SecurityCode.FormattedData;
            isSecurityCodeEmpty = string.IsNullOrEmpty(SecurityCodePasswordBox.Password);

            if (CardModel.SecurityCode.IsValid)
            {
                SetState(ControlState.Complete);
            }
        }

        #endregion

        #region Dependency Properties Callbacks

        private static async void CardModelChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (CardControl) obj;
            await control.SetCardModel((ICardModel) args.NewValue);
        }

        #endregion

        #region Private Members       

        private void EnsurePaymentSystemState()
        {
            paymentSystem = CardModel.PaymentSystem;
            string state;
            switch (paymentSystem)
            {
                case PaymentSystem.Visa:
                    state = "VisaState";
                    break;
                case PaymentSystem.MasterCard:
                    state = "MastercardState";
                    break;
                case PaymentSystem.Maestro:
                    state = "MaestroState";
                    break;
                default:
                    state = "DefaultPaymentSystemState";
                    break;
            }
            VisualStateManager.GoToState(this, state, true);
        }

        private static void RemoveLastAddedChar(TextBox textBox, int selectionStart)
        {
            var invalidSymbolIndex = selectionStart - 1;
            textBox.Text = textBox.Text.Remove(invalidSymbolIndex);
            textBox.SelectionStart = invalidSymbolIndex;
        }

        private void ResetCardModel()
        {
            CardModel = new DefaultCardModel();
        }

        private async Task SetCardModel(ICardModel model)
        {
            if (model is DefaultCardModel)
            {
                CardNumberInputTextBox.Text = string.Empty;
                DateTextBox.Text = string.Empty;
                SecurityCodePasswordBox.Password = string.Empty;
                SetState(ControlState.Initial);
                HideAdditionalButtons();
                EnsurePaymentSystemState();
            }
            else if (model is SavedCardModel)
            {
                modelJustSet = true;
                numberJustSet = true;
                CardNumberInputTextBox.Text = model.Number.FormattedData;
                dateJustSet = true;
                DateTextBox.Text = model.ExpiryDate.FormattedData;
                SecurityCodePasswordBox.Password = model.SecurityCode.FormattedData;
                EnsurePaymentSystemState();
                numberJustSet = dateJustSet = false;
                await SetNextStateAsync();
            }
        }

        private void SetFocus(ControlState state)
        {
            if (!IsFocused) return;

            switch (state)
            {
                case ControlState.Initial:
                case ControlState.NumberInput:
                    CardNumberInputTextBox.SelectionStart = CardNumberInputTextBox.Text.Length;
                    CardNumberInputTextBox.Focus(FocusState.Programmatic);
                    break;
                case ControlState.DateInput:
                    DateTextBox.SelectionStart = DateTextBox.Text.Length;
                    DateTextBox.Focus(FocusState.Programmatic);
                    break;
                case ControlState.SecurityCodeInput:
                    SecurityCodePasswordBox.Focus(FocusState.Programmatic);
                    break;
            }
        }

        private void SetState(ControlState state)
        {
            if (state == ControlState.NumberInput && controlState != state)
            {
                NumberInputRestoreStoryboard.BeginAsync();
                DataInputRestoreTimeline.To = DataInputGrid.ActualWidth;
                DataInputRestoreStoryboard.BeginAsync();

                if (controlState != ControlState.Initial)
                {
                    ShowNextStateButton();
                }
            }

            SetFocus(state);
            controlState = state;
        }

        private async Task SetNextStateAsync()
        {
            SetState(CardModel.ExpiryDate.IsValid ? ControlState.SecurityCodeInput : ControlState.DateInput);
            HideAdditionalButtons();
            await CollapseNumberInputField();
        }

        private void ShowNextStateButton()
        {
            VisualStateManager.GoToState(this, "NextStateButtonVisibleState", true);
        }

        private void HideAdditionalButtons()
        {
            VisualStateManager.GoToState(this, nameof(AdditionalButtonsInvisibleState), true);
        }

        private void ShowDeleteButton()
        {
            VisualStateManager.GoToState(this, "DeleteNumberButtonVisibleState", true);
        }

        private Task CollapseNumberInputField()
        {
            var textLength = CardNumberInputTextBox.Text.Length;
            var rectFromCharacterIndex = CardNumberInputTextBox.GetRectFromCharacterIndex(textLength - 5, true);
            var numberInputFieldOffset = -rectFromCharacterIndex.X;
            NumberInputSlideTimeline.To = numberInputFieldOffset;
            var numberFieldAnimation = NumberInputSlideStoryboard.BeginAsync();
            var dataFieldAnimation = DataInputSlideStoryboard.BeginAsync();
            return Task.WhenAll(numberFieldAnimation, dataFieldAnimation);
        }

        private enum ControlState
        {
            Initial,
            NumberInput,
            DateInput,
            SecurityCodeInput,
            Unfocused,
            Complete
        }

        #endregion
    }
}