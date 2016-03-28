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

using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace Tinkoff.Acquiring.Sdk
{
    class CryptoUtils
    {
        public static string Sha256(string value)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            return CryptographicBuffer.EncodeToHexString(provider.HashData(buffer));
        }

        public static string EncryptRsa(string value, CryptographicKey key)
        {
            var plainBuffer = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);
            var encryptedBuffer = CryptographicEngine.Encrypt(key, plainBuffer, null);
            return CryptographicBuffer.EncodeToBase64String(encryptedBuffer);
        }
    }
}
