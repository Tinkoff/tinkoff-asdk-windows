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
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Acquiring.Sdk;

namespace Tinkoff.Acquiring.UI
{
    class CardManager
    {
        #region Fields

        private static readonly IDictionary<string, Card[]> cache = new Dictionary<string, Card[]>();

        #endregion

        #region Public Members

        public async Task<IEnumerable<Card>> GetAllCards(string customerKey)
        {
            Card[] cards = null;
            if (cache.ContainsKey(customerKey))
                cards = cache[customerKey];
            if (cards == null)
            {
                cards = await AcquiringUI.GetAcquiringSdk().GetCardList(customerKey);
                cache[customerKey] = cards;
            }
            return cards;
        }

        public async Task<Card> GetCardById(string customerKey, string cardId)
        {
            Card[] cards = null;
            if (cache.ContainsKey(customerKey))
                cards = cache[customerKey];
            if (cards == null)
            {
                cards = await AcquiringUI.GetAcquiringSdk().GetCardList(customerKey);
                cache[customerKey] = cards;
            }
            return cards.FirstOrDefault(c => c.CardId == cardId);
        }

        public async Task<bool> RemoveCardById(string customerKey, string cardId)
        {
            var response = await AcquiringUI.GetAcquiringSdk().RemoveCard(customerKey, cardId);
            if (response && cache.ContainsKey(customerKey))
            {
                var cards = cache[customerKey];
                cache[customerKey] = cards.Where(c => c.CardId != cardId).ToArray();
            }
            return response;
        }

        public void Clear(string customerKey)
        {
            cache.Remove(customerKey);
        }

        #endregion
    }
}
