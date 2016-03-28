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

using Windows.Security.Cryptography.Core;

namespace Tinkoff.Acquiring.Sdk
{
    /// <summary>
    /// Данные сохранённой карты.
    /// </summary>
    public class SavedCardData : CardData
    {
        #region Properties

        /// <summary>
        /// Идентификатор карты.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Защитный код.
        /// </summary>
        public string SecureCode { get; set; }

        #endregion

        #region Internal Members

        internal override string Encode(CryptographicKey publicKey)
        {
            return CryptoUtils.EncryptRsa(string.Format("CardId={0};CVV={1}", Id, SecureCode), publicKey);
        }

        #endregion
    }
}