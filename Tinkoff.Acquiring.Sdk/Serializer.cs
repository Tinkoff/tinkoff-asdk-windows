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

using JetBrains.Annotations;
using Newtonsoft.Json;
using Tinkoff.Acquiring.Sdk.Responses;

namespace Tinkoff.Acquiring.Sdk
{
    static class Serializer
    {
        [CanBeNull]
        public static T Deserialize<T>(string content) where T : AcquiringResponse
        {
            try
            {
                var response = JsonConvert.DeserializeObject<T>(content);
                response.RawData = new RawData(content);

                return response;
            }
            catch
            {
                return null;
            }
        }

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
