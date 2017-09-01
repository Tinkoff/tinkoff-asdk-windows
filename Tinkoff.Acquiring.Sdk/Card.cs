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

namespace Tinkoff.Acquiring.Sdk
{
    /// <summary>
    /// Класс привязанной карты.
    /// </summary>
    public sealed class Card
    {
        #region Properties

        /// <summary>
        /// Возвращает номер карты.
        /// </summary>
        [JsonProperty(nameof(Pan))]
        public string Pan { get; internal set; }


        /// <summary>
        /// Возвращает идентификатор карты в системе Банка.
        /// </summary>
        [JsonProperty(nameof(CardId))]
        public string CardId { get; internal set; }


        /// <summary>
        /// Возвращает идентификатор рекуррентного платежа.
        /// </summary>
        [JsonProperty(nameof(RebillId))]
        public string RebillId { get; internal set; }


        /// <summary>
        /// Возвращает статус карты.
        /// </summary>
        [JsonProperty(nameof(Status))]
        public CardStatus Status { get; internal set; }


        #endregion
    }
}
