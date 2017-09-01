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

using Newtonsoft.Json;

namespace Tinkoff.Acquiring.Sdk.Responses
{
    /// <summary>
    /// Класс ответа API.
    /// </summary>
    public abstract class AcquiringResponse
    {
        #region Properties

        /// <summary>
        /// Возвращает идентификатор терминала, выдается Продавцу Банком.
        /// </summary>
        [JsonProperty(nameof(TerminalKey))]
        public string TerminalKey { get; internal set; }

        /// <summary>
        /// Возвращает успешность операции.
        /// </summary>
        [JsonProperty(nameof(Success))]
        public bool Success { get; internal set; }

        /// <summary>
        /// Возвращает код ошибки.
        /// </summary>
        [JsonProperty(nameof(ErrorCode))]
        public string ErrorCode { get; internal set; }

        /// <summary>
        /// Возвращает краткое описание ошибки.
        /// </summary>
        [JsonProperty(nameof(Message))]
        public string Message { get; internal set; }

        /// <summary>
        /// Возвращает подробное описание ошибки.
        /// </summary>
        [JsonProperty(nameof(Details))]
        public string Details { get; internal set; }

        /// <summary>
        /// Содержит необработанные данные.
        /// </summary>
        [JsonIgnore]
        internal RawData RawData { get; set; }

        #endregion
    }
}