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
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace Tinkoff.Acquiring.Sdk
{
    /// <summary>
    /// 
    /// </summary>
    static class HttpService
    {
        #region Public Members

        /// <summary>
        /// Выполняет асинхронный POST-запрос.
        /// </summary>
        /// <param name="uri">URI запроса.</param>
        /// <param name="token">Токен, который может быть использован для запроса отмены асинхронной операции.</param>
        /// <returns>Объект, представляющий асинхронный операцию.</returns>
        public static Task<IHttpContent> PostAsync(Uri uri, CancellationToken token = default(CancellationToken))
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                return SendRequestAsync(request, token);
            }
        }

        /// <summary>
        /// Выполняет асинхронный POST-запрос.
        /// </summary>
        /// <param name="uri">URI запроса.</param>
        /// <param name="content">Содержимое HTTP-запроса.</param>
        /// <param name="token">Токен, который может быть использован для запроса отмены асинхронной операции.</param>
        /// <returns>Объект, представляющий асинхронный операцию.</returns>
        public static Task<IHttpContent> PostAsync(Uri uri, IHttpContent content, CancellationToken token = default(CancellationToken))
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                request.Content = content;
                return SendRequestAsync(request, token);
            }
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Выполняет асинхронный запрос.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="token">Токен, который может быть использован для запроса отмены асинхронной операции.</param>
        /// <returns>Объект, представляющий асинхронный операцию.</returns>
        private static async Task<IHttpContent> SendRequestAsync(HttpRequestMessage request, CancellationToken token)
        {
            using (var response = await SendRequestAsync(request).AsTask(token).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return response.Content;
            }
        }

        /// <summary>
        /// Выполняет асинхронный запрос.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <returns>Объект, представляющий асинхронный операцию.</returns>
        private static IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
        {
            using (var protocolFilter = new HttpBaseProtocolFilter())
            {
                using (var client = new HttpClient(protocolFilter))
                {
                    protocolFilter.AutomaticDecompression = true;
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new HttpContentCodingWithQualityHeaderValue("gzip"));
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new HttpContentCodingWithQualityHeaderValue("deflate"));
                    return client.SendRequestAsync(request);
                }
            }
        }

        #endregion
    }
}
