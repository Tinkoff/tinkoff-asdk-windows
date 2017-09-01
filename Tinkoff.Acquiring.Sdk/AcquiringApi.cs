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
using Windows.Storage.Streams;
using Windows.Web.Http;
using JetBrains.Annotations;
using Tinkoff.Acquiring.Sdk.Requests;
using Tinkoff.Acquiring.Sdk.Responses;

namespace Tinkoff.Acquiring.Sdk
{
    class AcquiringApi
    {
        #region Fields

        [NotNull] private readonly Uri url;
        [NotNull] private readonly Journal journal;

        #endregion

        #region Ctor

        public AcquiringApi([NotNull] string url, [NotNull] Journal journal)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (journal == null) throw new ArgumentNullException(nameof(journal));

            this.url = new Uri(url);
            this.journal = journal;
        }

        #endregion

        #region Internal Members

        [ItemCanBeNull]
        internal async Task<InitResponse> Init(InitRequest request)
        {
            return await SendAsync<InitResponse>(request).ConfigureAwait(false);
        }

        [ItemCanBeNull]
        internal async Task<FinishAuthorizeResponse> FinishAuthorize(FinishAuthorizeRequest request)
        {
            return await SendAsync<FinishAuthorizeResponse>(request).ConfigureAwait(false);
        }

        [ItemCanBeNull]
        internal async Task<ChargeResponse> Charge(ChargeRequest request)
        {
            return await SendAsync<ChargeResponse>(request).ConfigureAwait(false);
        }

        [ItemCanBeNull]
        internal async Task<GetStateResponse> GetState(GetStateRequest request)
        {
            return await SendAsync<GetStateResponse>(request).ConfigureAwait(false);
        }

        [ItemCanBeNull]
        internal async Task<GetCardListResponse> GetCardList(GetCardListRequest request)
        {
            var response = await SendAsync<GetCardListResponse>(request).ConfigureAwait(false);
            if (response != null && string.IsNullOrEmpty(response.ErrorCode))
            {
                response.Success = true;
            }

            return response;
        }

        [ItemCanBeNull]
        internal async Task<RemoveCardResponse> RemoveCard(RemoveCardRequest request)
        {
            return await SendAsync<RemoveCardResponse>(request).ConfigureAwait(false);
        }

        #endregion

        #region Private Members

        [ItemCanBeNull]
        private async Task<T> SendAsync<T>(AcquiringRequest request) where T : AcquiringResponse
        {
            var uri = new Uri(url, request.Operation);

            return await SendAsync<T>(uri, GetContentFromRequest(request)).ConfigureAwait(false);
        }

        [ItemCanBeNull]
        private async Task<T> SendAsync<T>(Uri uri, IHttpContent content) where T : AcquiringResponse
        {
            journal.Log($"=== Sending POST request to {uri}");
            journal.Log($"===== Parameters: {content}");

            var response = await HttpService.PostAsync(uri, content).ConfigureAwait(false);
            var value = await response.ReadAsStringAsync();

            journal.Log($"=== Got server response: {value}");

            return Serializer.Deserialize<T>(value);
        }

        [NotNull]
        private static IHttpContent GetContentFromRequest(AcquiringRequest request)
        {
            return new HttpStringContent(Serializer.Serialize(request), UnicodeEncoding.Utf8, "application/json");
        }

        #endregion
    }
}
