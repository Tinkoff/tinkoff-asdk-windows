using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tinkoff.Acquiring.Sdk.Responses
{
    sealed class GetCardListResponseConverter : JsonConverter
    {
        #region Overrides of JsonConverter

        public override bool CanWrite => false;

        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            var cards = token.ToObject<Card[]>();

            return new GetCardListResponse
            {
                Cards = cards ?? Array.Empty<Card>()
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(GetCardListResponse));
        }

        #endregion
    }
}