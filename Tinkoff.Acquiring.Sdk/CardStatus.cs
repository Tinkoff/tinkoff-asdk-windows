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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tinkoff.Acquiring.Sdk
{
    /// <summary>
    /// Статус карты.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardStatus
    {
        /// <summary>
        /// Активна.
        /// </summary>
        [EnumMember(Value = "A")]
        ACTIVE,

        /// <summary>
        /// Неактивна.
        /// </summary>
        [EnumMember(Value = "I")]
        INACTIVE,

        /// <summary>
        /// Удалена.
        /// </summary>
        [EnumMember(Value = "D")]
        DELETED
    }
}
