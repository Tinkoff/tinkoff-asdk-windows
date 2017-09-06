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
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using JetBrains.Annotations;
using Tinkoff.Acquiring.Sdk.Builders;

namespace Tinkoff.Acquiring.Sdk
{
    /// <summary>
    /// Класс позволяет конфигурировать SDK и осуществлять взаимодействие с Тинькофф Эквайринг API. Методы, осуществляющие
    /// обращение к API, возвращают результат в случае успешного выполнения запроса или бросают исключение
    /// <see cref="AcquiringSdkException" />.
    /// </summary>
    public class AcquiringSdk
    {
        #region Fields

        private const string API_URL_RELEASE = "https://securepay.tinkoff.ru/v2/";
        private const string API_URL_DEBUG = "https://rest-api-test.tcsbank.ru/v2/";
        private readonly string terminalKey;
        private readonly string password;
        private readonly CryptographicKey publicKey;
        private readonly Journal journal = new Journal();

        #endregion

        #region Ctor

        /// <summary>
        /// Создает новый экземпляр SDK.
        /// </summary>
        /// <param name="terminalKey">Ключ терминала. Выдается после подключения к Тинькофф Эквайринг.</param>
        /// <param name="password">Пароль от терминала. Выдается вместе с terminalKey.</param>
        /// <param name="publicKey">Публичный ключ. Выдается вместе с terminalKey.</param>
        public AcquiringSdk(string terminalKey, string password, CryptographicKey publicKey)
        {
            this.terminalKey = terminalKey;
            this.password = password;
            this.publicKey = publicKey;
        }

        /// <summary>
        /// Создает новый экземпляр SDK.
        /// </summary>
        /// <param name="terminalKey">Ключ терминала. Выдается после подключения к Тинькофф Эквайринг.</param>
        /// <param name="password">Пароль от терминала. Выдается вместе с terminalKey.</param>
        /// <param name="keyCreator"></param>
        public AcquiringSdk(string terminalKey, string password, [NotNull] IKeyCreator keyCreator)
        {
            if (keyCreator == null) throw new ArgumentNullException(nameof(keyCreator));

            this.terminalKey = terminalKey;
            this.password = password;
            publicKey = keyCreator.Create();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Позволяет переключать SDK с тестового режима и обратно. В тестовом режиме деньги с карты не списываются, при этом в
        /// лог пишется отладочная информация: запросы и ответы API, ошибки валидации и т.д.
        /// </summary>
        /// <remarks>По умолчанию включено.</remarks>
        public bool IsDebug
        {
            get { return journal.IsDebug; }
            set { journal.IsDebug = value; }
        }

        /// <summary>
        /// Возвращает URL адрес API.
        /// </summary>
        public string Url => IsDebug ? API_URL_DEBUG : API_URL_RELEASE;

        #endregion

        #region Public Members

        /// <summary>
        /// Инициирует платежную сессию.
        /// </summary>
        /// <param name="amount">Сумма в копейках.</param>
        /// <param name="orderId">Номер заказа в системе Продавца.</param>
        /// <param name="customerKey">
        /// Идентификатор покупателя в системе Продавца. Если передается и Банком разрешена
        /// автоматическая привязка карт к терминалу, то для данного покупателя будет осуществлена привязка карты.
        /// </param>
        /// <param name="description">Краткое описание.</param>
        /// <param name="payForm">Название шаблона формы оплаты продавца.</param>
        /// <param name="recurrent">Регистрирует платеж как рекуррентный.</param>
        /// <param name="receipt">JSON объект с данными чека.</param>
        /// <param name="data">
        /// JSON объект содержащий дополнительные параметры в виде "ключ":"значение".
        /// Данные параметры будут переданы на страницу оплаты (в случае ее кастомизации).
        /// Максимальная длина для каждого передаваемого параметра: Ключ – 20 знаков, Значение – 100 знаков. 
        /// Максимальное количество пар «ключ-значение» не может превышать 20.
        /// </param>
        /// <returns>Уникальный идентификатор транзакции в системе Банка.</returns>
        public async Task<string> Init(decimal amount, string orderId, string customerKey, string description = null, string payForm = null, bool recurrent = false, string receipt = null, IReadOnlyList<KeyValuePair<string, string>> data = null)
        {
            var builder = new InitRequestBuilder(password, terminalKey, journal)
                .SetAmount(amount)
                .SetOrderId(orderId)
                .SetCustomerKey(customerKey)
                .SetDescription(description)
                .SetPayForm(payForm)
                .SetRecurrent(recurrent);

            if (receipt != null)
                builder.SetReceipt(receipt);

            if (data != null)
                builder.AddData(data);

            var request = builder.Build();
            try
            {
                var response = await GetApi().Init(request);
                if (response != null && response.Success) return response.PaymentId;

                throw new AcquiringApiException(response);
            }
            catch (AcquiringApiException ex)
            {
                journal.Log(ex);
                throw;
            }
            catch (Exception ex)
            {
                journal.Log(ex);
                throw new AcquiringSdkException(ex.Message);
            }
        }

        /// <summary>
        /// Подтверждает инициированный платеж передачей карточных данных.
        /// </summary>
        /// <param name="paymentId">Уникальный идентификатор транзакции в системе Банка.</param>
        /// <param name="sendEmail">Параметр, который определяет отравлять email с квитанцией или нет.</param>
        /// <param name="cardData">Данные карты.</param>
        /// <param name="infoEmail">Email на который будет отправлена квитанция об оплате.</param>
        /// <returns>
        /// Объект ThreeDsData. Если терминал требует прохождения, свойство IsThreeDsNeeded будет установлено в true.
        /// </returns>
        public async Task<ThreeDsData> FinishAuthorize(string paymentId, bool sendEmail, CardData cardData, string infoEmail)
        {
            var request = new FinishAuthorizeRequestBuilder(password, terminalKey, journal)
                .SetPaymentId(paymentId)
                .SetSendEmail(sendEmail)
                .SetCardData(cardData.Encode(publicKey))
                .SetInfoEmail(infoEmail)
                .Build();

            try
            {
                var response = await GetApi().FinishAuthorize(request);
                if (response == null || !response.Success) throw new AcquiringApiException(response);

                return response.Status == PaymentStatus.DS_CHECKING
                    ? new ThreeDsData {IsThreeDsNeed = true, ACSUrl = response.ACSUrl, MD = response.MD, PaReq = response.PaReq}
                    : new ThreeDsData {IsThreeDsNeed = false};
            }
            catch (AcquiringApiException ex)
            {
                journal.Log(ex);
                throw;
            }
            catch (Exception ex)
            {
                journal.Log(ex);
                throw new AcquiringSdkException(ex.Message);
            }
        }

        /// <summary>
        /// <para>
        ///     Осуществляет рекуррентный (повторный) платеж — безакцептное списание денежных средств со счета банковской карты
        ///     Покупателя. Для возможности его использования Покупатель должен совершить хотя бы один платеж в пользу
        ///     Продавца, который должен быть указан как рекуррентный (см. параметр recurrent в методе <see cref="Init" />),
        ///     фактически являющийся первичным.
        /// </para>
        /// <list type="number">
        ///     <listheader>
        ///         <description>
        ///             Другими словами, для использования рекуррентных платежей необходима следующая
        ///             последовательность действий:
        ///         </description>
        ///     </listheader>
        ///     <item>
        ///         <description>
        ///             Совершить родительский платеж путем вызова <see cref="Init" /> с указанием дополнительного параметра
        ///             Recurrent=Y.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             2. Получить RebillId, предварительно вызвав метод <see cref="GetCardList" />
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             3. Спустя некоторое время для совершения рекуррентного платежа необходимо вызвать метод
        ///             <see cref="Init" /> со стандартным набором параметров (параметр Recurrent здесь не нужен).
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             4. Получить в ответ на <see cref="Init" /> параметр PaymentId.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             5. Вызвать метод <see cref="Charge" /> с параметром <paramref name="rebillId" /> полученным в п.2 и
        ///             параметром <paramref name="paymentId" /> полученным в п.4.
        ///         </description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="paymentId">Уникальный идентификатор транзакции в системе Банка.</param>
        /// <param name="rebillId">Идентификатор рекуррентного платежа. См. метод <see cref="GetCardList" />.</param>
        /// <returns>Информация о платеже.</returns>
        public async Task<PaymentInfo> Charge(string paymentId, string rebillId)
        {
            var request = new ChargeRequestBuilder(password, terminalKey, journal)
                .SetPaymentId(paymentId)
                .SetRebillId(rebillId)
                .Build();
            try
            {
                var response = await GetApi().Charge(request);
                if (response != null && response.Success)
                {
                    return new PaymentInfo
                    {
                        OrderId = response.OrderId,
                        PaymentId = response.PaymentId,
                        Status = response.Status
                    };
                }

                throw new AcquiringApiException(response);
            }
            catch (AcquiringApiException ex)
            {
                journal.Log(ex);
                throw;
            }
            catch (Exception ex)
            {
                journal.Log(ex);
                throw new AcquiringSdkException(ex.Message);
            }
        }

        /// <summary>
        /// Возвращает статус платежа.
        /// </summary>
        /// <param name="paymentId">Уникальный идентификатор транзакции в системе Банка.</param>
        /// <returns>Статус платежа.</returns>
        public async Task<PaymentStatus> GetState(string paymentId)
        {
            var request = new GetStateRequestBuilder(password, terminalKey, journal)
                .SetPaymentId(paymentId)
                .Build();
            try
            {
                var response = await GetApi().GetState(request);
                if (response != null && response.Success) return response.Status;

                throw new AcquiringApiException(response);
            }
            catch (AcquiringApiException ex)
            {
                journal.Log(ex);
                throw;
            }
            catch (Exception ex)
            {
                journal.Log(ex);
                throw new AcquiringSdkException(ex.Message);
            }
        }

        /// <summary>
        /// Возвращает список привязанных карт.
        /// </summary>
        /// <param name="customerKey">Идентификатор покупателя в системе Продавца.</param>
        /// <returns>Список сохраненных карт.</returns>
        public async Task<Card[]> GetCardList(string customerKey)
        {
            var request = new GetCardListRequestBuilder(password, terminalKey, journal)
                .SetCustomerKey(customerKey)
                .Build();
            try
            {
                var response = await GetApi().GetCardList(request);
                if (response != null && response.Success) return response.Cards;

                throw new AcquiringApiException(response);
            }
            catch (AcquiringApiException ex)
            {
                journal.Log(ex);
                throw;
            }
            catch (Exception ex)
            {
                journal.Log(ex);
                throw new AcquiringSdkException(ex.Message);
            }
        }

        /// <summary>
        /// Удаляет привязанную карту.
        /// </summary>
        /// <param name="customerKey">Идентификатор покупателя в системе Продавца.</param>
        /// <param name="cardId">Идентификатор карты в системе Банка.</param>
        /// <returns>Признак, удалена ли карта.</returns>
        public async Task<bool> RemoveCard(string customerKey, string cardId)
        {
            var request = new RemoveCardRequestBuilder(password, terminalKey, journal)
                .SetCustomerKey(customerKey)
                .SetCardId(cardId)
                .Build();
            try
            {
                var response = await GetApi().RemoveCard(request);
                if (response != null && response.Success) return true;

                throw new AcquiringApiException(response);
            }
            catch (AcquiringApiException ex)
            {
                journal.Log(ex);
                throw;
            }
            catch (Exception ex)
            {
                journal.Log(ex);
                throw new AcquiringSdkException(ex.Message);
            }
        }

        /// <summary>
        /// Позволяет использовать свой механизм управления логами.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        public void SetLogger([NotNull] ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            journal.SetLogger(logger);
        }

        #endregion

        #region Private Members

        private AcquiringApi GetApi()
        {
            return IsDebug
                ? new AcquiringApi(API_URL_DEBUG, journal)
                : new AcquiringApi(API_URL_RELEASE, journal);
        }

        #endregion
    }
}