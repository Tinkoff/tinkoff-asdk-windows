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
using System.IO;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Tinkoff.Acquiring.Sdk;

namespace Tinkoff.Acquiring.UI
{
    /// <summary>
    /// Реализация механизма создания ключей, использующая файл public.pem.
    /// </summary>
    public class DefaultKeyCreator : IKeyCreator
    {
        private const string Filename = "public.pem";

        /// <summary>
        /// Создаёт ключ.
        /// </summary>
        /// <returns>Созданный ключ.</returns>
        public CryptographicKey Create()
        {
            return CreateAsync().Result;
        }

        private static async Task<CryptographicKey> CreateAsync()
        {
            var path = $@"{AppContext.BaseDirectory}\Assets\{Filename}";
            var file = await StorageFile.GetFileFromPathAsync(path).AsTask().ConfigureAwait(false);

            using (var stream = await file.OpenReadAsync().AsTask().ConfigureAwait(false))
            using (var reader = new StreamReader(stream.AsStream()))
            {
                var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                var stringKey = content
                    .Replace("-----BEGIN PUBLIC KEY-----", string.Empty)
                    .Replace("-----END PUBLIC KEY-----", string.Empty)
                    .Trim();
                return new StringKeyCreator(stringKey).Create();
            }
        }
    }
}