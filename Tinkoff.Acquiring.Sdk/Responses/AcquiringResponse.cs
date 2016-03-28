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

using System.Runtime.Serialization;

namespace Tinkoff.Acquiring.Sdk.Responses
{
    /// <summary>
    /// Класс ответа API.
    /// </summary>
    [DataContract]
    public abstract class AcquiringResponse
    {
        /// <summary>
        /// Возвращает идентификатор терминала, выдается Продавцу Банком.
        /// </summary>
        [DataMember]
        public string TerminalKey { get; internal set; }

        /// <summary>
        /// Возвращает успешность операции.
        /// </summary>
        [DataMember]
        public bool Success { get; internal set; }

        /// <summary>
        /// Возвращает код ошибки.
        /// </summary>
        [DataMember]
        public string ErrorCode { get; internal set; }

        /// <summary>
        /// Возвращает краткое описание ошибки.
        /// </summary>
        [DataMember]
        public string Message { get; internal set; }

        /// <summary>
        /// Возвращает подробное описание ошибки.
        /// </summary>
        [DataMember]
        public string Details { get; internal set; }

        /// <summary>
        /// Содержит необработанные данные.
        /// </summary>
        [IgnoreDataMember]
        internal RawData RawData { get; set; }
    }
}
