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
using Windows.Web.Http;
using Tinkoff.Acquiring.Sdk.Requests;
using Tinkoff.Acquiring.Sdk.Responses;

namespace Tinkoff.Acquiring.Sdk
{
    class AcquiringApi
    {
        #region Fields

        private readonly Journal journal;

        #endregion

        #region Ctor

        public AcquiringApi(string url, Journal journal)
        {
            Url = new Uri(url);
            this.journal = journal;
        }

        #endregion

        #region Properties

        public Uri Url { get; set; }

        #endregion

        #region Internal Members

        internal Task<InitResponse> Init(InitRequest request)
        {
            return SendAsync<InitResponse>(Url, new HttpFormUrlEncodedContent(request.ToDictionary()));
        }

        internal Task<FinishAuthorizeResponse> FinishAuthorize(FinishAuthorizeRequest request)
        {
            return SendAsync<FinishAuthorizeResponse>(Url, new HttpFormUrlEncodedContent(request.ToDictionary()));
        }

        internal Task<ChargeResponse> Charge(ChargeRequest request)
        {
            return SendAsync<ChargeResponse>(Url, new HttpFormUrlEncodedContent(request.ToDictionary()));
        }

        internal Task<GetStateResponse> GetState(GetStateRequest request)
        {
            return SendAsync<GetStateResponse>(Url, new HttpFormUrlEncodedContent(request.ToDictionary()));
        }

        internal async Task<GetCardListResponse> GetCardList(GetCardListRequest request)
        {
            var response = await SendAsync<GetCardListResponse>(Url, new HttpFormUrlEncodedContent(request.ToDictionary()));
            if (string.IsNullOrEmpty(response.ErrorCode))
            {
                response.Success = true;
                response.Cards = response.RawData.To<Card[]>();
            }
            return response;
        }

        internal Task<RemoveCardResponse> RemoveCard(RemoveCardRequest request)
        {
            return SendAsync<RemoveCardResponse>(Url, new HttpFormUrlEncodedContent(request.ToDictionary()));
        }

        #endregion

        #region Private Members

        private async Task<T> SendAsync<T>(Uri uri, IHttpContent content) where T : AcquiringResponse
        {
            journal.Log($"=== Sending POST request to {uri}");
            journal.Log($"===== Parameters: {content}");

            var response = await HttpService.PostAsync(uri, content).ConfigureAwait(false);
            var value = await response.ReadAsStringAsync();

            journal.Log($"=== Got server response: {value}");

            return Serializer.Deserialize<T>(value);
        }

        #endregion
    }
}
