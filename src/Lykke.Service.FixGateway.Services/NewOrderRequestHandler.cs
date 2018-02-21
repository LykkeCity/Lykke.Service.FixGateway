using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.Assets.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;
using QuickFix.Fields;
using QuickFix.FIX44;
using ILog = Common.Log.ILog;
using Message = QuickFix.Message;
using OrderAction = Lykke.Service.FixGateway.Core.Domain.OrderAction;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class NewOrderRequestHandler : IRequestHandler<NewOrderSingle>
    {
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;
        private readonly ILog _log;
        private readonly IFeeCalculatorClient _feeCalculatorClient;
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly IMapper _mapper;
        private readonly Credentials _credentials;
        private readonly FeeSettings _feeSettings;
        private readonly SessionState _sessionState;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly OrderCharacteristicsValidator _orderCharacteristicsValidator;
        private readonly CancellationTokenSource _tokenSource;



        public NewOrderRequestHandler(
            ILog log,
            IFeeCalculatorClient feeCalculatorClient,
            IMatchingEngineClient matchingEngineClient,
            IClientOrderIdProvider clientOrderIdProvider,
            IAssetsServiceWithCache assetsService,
            IMapper mapper,
            Credentials credentials,
            FeeSettings feeSettings,
            SessionState sessionState,
            IFixMessagesSender fixMessagesSender)
        {
            _log = log;
            _feeCalculatorClient = feeCalculatorClient;
            _matchingEngineClient = matchingEngineClient;
            _clientOrderIdProvider = clientOrderIdProvider;
            _assetsServiceWithCache = assetsService;
            _mapper = mapper;
            _credentials = credentials;
            _feeSettings = feeSettings;
            _sessionState = sessionState;
            _fixMessagesSender = fixMessagesSender;
            _orderCharacteristicsValidator = new OrderCharacteristicsValidator(clientOrderIdProvider, assetsService, credentials);
            _tokenSource = new CancellationTokenSource();

        }



        public void Handle(NewOrderSingle request)
        {
            Task.Factory.StartNew(HandleRequestAsync, request, _tokenSource.Token).Unwrap().GetAwaiter().GetResult();
        }

        private async Task HandleRequestAsync(object input)
        {
            var request = (NewOrderSingle)input;
            var newOrderId = GenerateOrderId();
            try
            {
                if (!await ValidateRequestAsync(newOrderId, request))
                {
                    return;
                }

                await _clientOrderIdProvider.RegisterNewOrderAsync(newOrderId, request.ClOrdID.Obj);

                if (IsMarketOrder(request))
                {
                    await HandleMarketOrderAsync(newOrderId, request);
                }
                else
                {
                    await HandleLimitOrderAsync(newOrderId, request);
                }
            }
            catch (Exception ex)
            {
                await _log.WriteWarningAsync(nameof(HandleRequestAsync), $"NewOrederRequest. Id {newOrderId}", "", ex);
                var reject = CreateRejectResponse(newOrderId, request, OrdRejReason.OTHER);
                Send(reject);
            }
        }

        private static bool IsMarketOrder(NewOrderSingle request)
        {
            return request.OrdType.Obj == OrdType.MARKET;
        }

        private Guid GenerateOrderId()
        {
            return Guid.NewGuid();
        }

        private async Task HandleLimitOrderAsync(Guid newOrderId, NewOrderSingle request)
        {
            var orderAction = _mapper.Map<OrderAction>(request.Side);
            var volumeSign = orderAction == OrderAction.Buy ? 1 : -1;
            var orderModel = new LimitOrderModel
            {
                Id = newOrderId.ToString("D"),
                ClientId = request.Account.Obj,
                AssetPairId = request.Symbol.Obj,
                Volume = volumeSign * (double)request.OrderQty.Obj,
                OrderAction = _mapper.Map<MatchingEngine.Connector.Abstractions.Models.OrderAction>(orderAction),
                Fee = await GetLimitOrderFeeAsync(request.Symbol.Obj, orderAction),
                Price = (double)request.Price.Obj,
                CancelPreviousOrders = false
            };

            var meResponse = await _matchingEngineClient.PlaceLimitOrderAsync(orderModel);
            await CheckResponseAndThrowIfNullAsync(meResponse);


            SendResponse(newOrderId, request, meResponse.Status);

        }

        private void SendResponse(Guid newOrderId, NewOrderSingle request, MeStatusCodes meStatus)
        {
            var status = (MessageStatus)meStatus;
            //Send only if received OK. Other messages we will receive via RabbitMq
            switch (status)
            {
                case MessageStatus.Ok:
                    var ack = CreteAckResponse(newOrderId, request);
                    Send(ack);
                    break;
                case MessageStatus.Duplicate:
                case MessageStatus.Runtime:
                    var rejectReason = _mapper.Map<OrdRejReason>(status);
                    var reject = CreateRejectResponse(newOrderId, request, rejectReason, status.ToString());
                    Send(reject);
                    break;
            }
        }

        private async Task HandleMarketOrderAsync(Guid newOrderId, NewOrderSingle request)
        {
            var orderAction = _mapper.Map<OrderAction>(request.Side);
            var volumeSign = orderAction == OrderAction.Buy ? 1 : -1;

            var orderModel = new MarketOrderModel
            {
                Id = newOrderId.ToString("D"),
                ClientId = request.Account.Obj,
                AssetPairId = request.Symbol.Obj,
                Straight = true,
                Volume = volumeSign * (double)request.OrderQty.Obj,
                OrderAction = _mapper.Map<MatchingEngine.Connector.Abstractions.Models.OrderAction>(orderAction),
                Fee = await GetMarketOrderFeeAsync(request.Symbol.Obj, orderAction),
                ReservedLimitVolume = null
            };

            var meResponse = await _matchingEngineClient.HandleMarketOrderAsync(orderModel);
            await CheckResponseAndThrowIfNullAsync(meResponse);


            SendResponse(newOrderId, request, meResponse.Status);
        }

        private async Task CheckResponseAndThrowIfNullAsync(object response)
        {
            if (response == null)
            {
                var exception = new InvalidOperationException("ME not available");
                await _log.WriteErrorAsync(nameof(NewOrderRequestHandler), nameof(CheckResponseAndThrowIfNullAsync), exception);
                throw exception;
            }
        }

        private async Task<MarketOrderFeeModel> GetMarketOrderFeeAsync(string assetPairId, OrderAction orderAction)
        {
            var assetPair = await _assetsServiceWithCache.TryGetAssetPairAsync(assetPairId);
            var fee = await _feeCalculatorClient.GetMarketOrderAssetFee(_credentials.ClientId.ToString("D"), assetPairId, assetPair?.BaseAssetId, _mapper.Map<FeeCalculator.AutorestClient.Models.OrderAction>(orderAction));

            return new MarketOrderFeeModel
            {
                Size = (double)fee.Amount,
                SourceClientId = _credentials.ClientId.ToString("D"),
                TargetClientId = _feeSettings.TargetClientId.Hft,
                Type = (int)MarketOrderFeeType.CLIENT_FEE
            };
        }

        private async Task<LimitOrderFeeModel> GetLimitOrderFeeAsync(string assetPairId, OrderAction orderAction)
        {
            var assetPair = await _assetsServiceWithCache.TryGetAssetPairAsync(assetPairId);
            var fee = await _feeCalculatorClient.GetLimitOrderFees(_credentials.ClientId.ToString("D"), assetPairId, assetPair?.BaseAssetId, _mapper.Map<FeeCalculator.AutorestClient.Models.OrderAction>(orderAction));

            return new LimitOrderFeeModel
            {
                MakerSize = (double)fee.MakerFeeSize,
                TakerSize = (double)fee.TakerFeeSize,
                SourceClientId = _credentials.ClientId.ToString("D"),
                TargetClientId = _feeSettings.TargetClientId.Hft,
                Type = (int)LimitOrderFeeType.CLIENT_FEE
            };
        }


        private async Task<bool> ValidateRequestAsync(Guid newOrderId, NewOrderSingle request)
        {
            var rejectReason = await _orderCharacteristicsValidator.ValidateRequestAsync(request);
            if (rejectReason != null)
            {
                var reject = CreateRejectResponse(newOrderId, request, rejectReason);
                Send(reject);
                return false;
            }
            return true;
        }


        private void Send(Message message)
        {
            _fixMessagesSender.Send(message, _sessionState.SessionID);
        }

        private Message CreteAckResponse(Guid newOrderId, NewOrderSingle request)
        {
            var ack = new ExecutionReport
            {
                OrderID = new OrderID(newOrderId.ToString("D")),
                ClOrdID = new ClOrdID(request.ClOrdID.Obj),
                ExecID = new ExecID(_sessionState.NextOrderReportId.ToString()),
                OrdStatus = new OrdStatus(OrdStatus.PENDING_NEW),
                ExecType = new ExecType(ExecType.PENDING_NEW),
                Symbol = new Symbol(request.Symbol.Obj),
                OrderQty = new OrderQty(request.OrderQty.Obj),
                OrdType = new OrdType(request.OrdType.Obj),
                TimeInForce = new TimeInForce(request.TimeInForce.Obj),
                LeavesQty = new LeavesQty(0),
                CumQty = new CumQty(0),
                AvgPx = new AvgPx(0),
                Side = new Side(request.Side.Obj)
            };

            return ack;
        }

        private Message CreateRejectResponse(Guid newOrderId, NewOrderSingle request, OrdRejReason rejectReason, string rejectDescription = null)
        {
            return CreateRejectResponse(newOrderId, request, rejectReason.Obj, rejectDescription);
        }

        private Message CreateRejectResponse(Guid newOrderId, NewOrderSingle request, int rejectReason, string rejectDescription = null)
        {
            var reject = new ExecutionReport
            {
                OrderID = new OrderID(newOrderId.ToString("D")),
                ClOrdID = new ClOrdID(request.ClOrdID.Obj),
                ExecID = new ExecID(_sessionState.NextOrderReportId.ToString()),
                OrdStatus = new OrdStatus(OrdStatus.REJECTED),
                ExecType = new ExecType(ExecType.REJECTED),
                Symbol = new Symbol(request.Symbol.Obj),
                OrderQty = new OrderQty(request.OrderQty.Obj),
                OrdType = new OrdType(request.OrdType.Obj),
                TimeInForce = new TimeInForce(request.TimeInForce.Obj),
                LeavesQty = new LeavesQty(0),
                CumQty = new CumQty(0),
                AvgPx = new AvgPx(0),
                Side = new Side(request.Side.Obj),
                OrdRejReason = new OrdRejReason(rejectReason)
            };
            if (rejectDescription != null)
            {
                reject.Text = new Text(rejectDescription);
            }
            return reject;
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
