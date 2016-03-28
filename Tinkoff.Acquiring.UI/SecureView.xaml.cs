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
using System.Net;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Tinkoff.Acquiring.Sdk;
using Tinkoff.Acquiring.UI.Model;

namespace Tinkoff.Acquiring.UI
{
    sealed partial class SecureView
    {
        #region Fields

        private const string SECURE_PAGE_TITLE = "SECURE_PAGE_TITLE";
        private const string SECURE_FUNC_NAME = "secureFunction";
        private const string CANCEL_ACTION = "cancel.do";
        private const string SUBMIT_3DS_AUTHORIZATION = "Submit3DSAuthorization";
        private bool processed;
        private string uri;
        private string md;
        private string paReq;
        private string termUrl;
        private string paymentId;
        private readonly AcquiringSdk sdk;

        #endregion

        #region Ctor

        public SecureView()
        {
            InitializeComponent();
            sdk = AcquiringUI.GetAcquiringSdk();
        }

        #endregion

        #region Events

        public event Action Succeeded;
        public event Action Cancelled;
        public event Action<Exception> Failed;

        #endregion

        #region Base Members

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var secureParams = e.Parameter as SecureViewParams;

            if (AcquiringUI.AreHardwareButtonsAvailable)
            {
                HardwareButtons.BackPressed += OnBackPressed;
            }

            uri = secureParams.ThreeDsData.ACSUrl;
            md = secureParams.ThreeDsData.MD;
            paReq = secureParams.ThreeDsData.PaReq;
            termUrl = string.Concat(sdk.Url, SUBMIT_3DS_AUTHORIZATION);
            paymentId = secureParams.PaymentId;
            WebView.NavigateToString(GetSecurePage());

            var inputPane = InputPane.GetForCurrentView();
            inputPane.Hiding += OnKeyboardHiding;
            inputPane.Showing += OnKeyboardShowing;
        }

        #endregion

        #region Private Members

        private void OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            ProgressRing.IsActive = true;
        }

        private async void OnNavigationCompleted(WebView webView, WebViewNavigationCompletedEventArgs args)
        {
            ProgressRing.IsActive = false;
            if (processed) return;

            if (webView.DocumentTitle == SECURE_PAGE_TITLE)
            {
                await webView.InvokeScriptAsync(SECURE_FUNC_NAME, null);
                return;
            }

            if (args.Uri == null)
                return;

            if (args.Uri.Equals(termUrl))
            {
                processed = true;
                ProgressRing.IsActive = true;
                try
                {
                    var status = await sdk.GetState(paymentId);
                    switch (status)
                    {
                        case PaymentStatus.CONFIRMED:
                        case PaymentStatus.AUTHORIZED:
                            OnSucceeded();
                            break;
                        case PaymentStatus.REJECTED:
                            OnFailed(new InvalidOperationException("Платёж отклонён банком"));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    OnFailed(ex);
                }
            }
            else if (args.Uri.ToString().Contains(CANCEL_ACTION))
            {
                OnCancelled();
            }
        }

        private void OnNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            ProgressRing.IsActive = false;
            if (processed) return;

            OnFailed(new WebException($"Navigation failed with error {e.WebErrorStatus}."));
        }


        private string GetSecurePage()
        {
            var inputFields = $"<input hidden='on' name='TermUrl' value='{termUrl}'/>" +
                              $"<input hidden='on' name='MD' value='{md}'/>" +
                              $"<input hidden='on' name='PaReq' value='{paReq}'/>";
            var htmlContent = $"<html><head><title>{SECURE_PAGE_TITLE}</title>" +
                              $"<script type='text/javascript'>function {SECURE_FUNC_NAME}(){{document.getElementById('secureId').submit();}}</script>" +
                              $"</head><body><form id='secureId' action='{uri}' method='post'>{inputFields}</form></body></html>";
            return htmlContent;
        }

        private void OnKeyboardShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            LayoutRoot.Padding = new Thickness(0, 0, 0, sender.OccludedRect.Height);
        }

        private void OnKeyboardHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            LayoutRoot.Padding = new Thickness(0);
        }

        private void Unsubscribe()
        {
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Hiding -= OnKeyboardHiding;
            inputPane.Showing -= OnKeyboardShowing;

            ProgressRing.IsActive = false;

            if (AcquiringUI.AreHardwareButtonsAvailable)
            {
                HardwareButtons.BackPressed -= OnBackPressed;
            }

            processed = true;
        }

        private void OnBackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            OnCancelled();
        }

        private void OnSucceeded()
        {
            Unsubscribe();
            Succeeded?.Invoke();
        }

        private void OnCancelled()
        {
            Unsubscribe();
            Cancelled?.Invoke();
        }

        private void OnFailed(Exception exception)
        {
            Unsubscribe();
            Failed?.Invoke(exception);
        }

        #endregion
    }
}
