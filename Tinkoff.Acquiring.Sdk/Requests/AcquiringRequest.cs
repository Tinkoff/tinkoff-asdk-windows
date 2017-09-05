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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tinkoff.Acquiring.Sdk.Requests
{
    /// <summary>
    /// Представляет настройки запроса.
    /// </summary>
    abstract class AcquiringRequest
    {
        #region Properties

        /// <summary>
        /// Возвращает имя опреации.
        /// </summary>
        [JsonIgnore]
        public abstract string Operation { get; }

        /// <summary>
        /// Возвращает идентификатор терминала, выдается Продавцу Банком.
        /// </summary>
        [JsonProperty(nameof(TerminalKey))]
        public string TerminalKey { get; set; }

        /// <summary>
        /// Возвращает подпись запроса.
        /// </summary>
        [JsonProperty(nameof(Token))]
        public string Token { get; set; }

        #endregion

        #region Methods

        public virtual IDictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                {Fields.TERMINALKEY, TerminalKey},
                {Fields.TOKEN, Token}
            };
        }

        #endregion
    }
}
