using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;
using Lykke.Service.FixGateway.Services.Extensions;
using QuickFix.Fields;
using QuickFix.FIX44;
using ILog = Common.Log.ILog;
using Message = QuickFix.Message;
using OrderAction = Lykke.Service.FixGateway.Core.Domain.OrderAction;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class NewOrderRequestHandler : INewOrderRequestHandler
    {
        private readonly IAssetsService _assetsService;
        private readonly ILog _log;
        private readonly IFeeCalculatorClient _feeCalculatorClient;
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly IMapper _mapper;
        private readonly Credentials _credentials;
        private readonly FeeSettings _feeSettings;
        private readonly SessionState _sessionState;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly IFixNewOrderRequestValidator _newOrderRequestValidator;
        private readonly CancellationTokenSource _tokenSource;



        public NewOrderRequestHandler(
            ILog log,
            IFeeCalculatorClient feeCalculatorClient,
            IMatchingEngineClient matchingEngineClient,
            IClientOrderIdProvider clientOrderIdProvider,
            IAssetsService assetsService,
            IMapper mapper,
            Credentials credentials,
            FeeSettings feeSettings,
            SessionState sessionState,
            IFixMessagesSender fixMessagesSender,
            IFixNewOrderRequestValidator newOrderRequestValidator)
        {
            _log = log;
            _feeCalculatorClient = feeCalculatorClient;
            _matchingEngineClient = matchingEngineClient;
            _clientOrderIdProvider = clientOrderIdProvider;
            _assetsService = assetsService;
            _mapper = mapper;
            _credentials = credentials;
            _feeSettings = feeSettings;
            _sessionState = sessionState;
            _fixMessagesSender = fixMessagesSender;
            _newOrderRequestValidator = newOrderRequestValidator;

            _tokenSource = new CancellationTokenSource();

        }



        public Task Handle(NewOrderSingle request)
        {
            return HandleRequestAsync(request);
        }

        private async Task HandleRequestAsync(NewOrderSingle request)
        {
            var newOrderId = GenerateOrderId();
            try
            {
                if (!await _newOrderRequestValidator.ValidateAsync(request))
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
                var errorCode = Guid.NewGuid();
                _log.WriteWarning(nameof(HandleRequestAsync), "", $"NewOrederRequest. Id: {newOrderId}. NewOrderSingle.ClOrdID: {request.ClOrdID}. Error code: {errorCode}", ex);
                var reject = new Reject().CreateReject(request, errorCode);
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
                Id = newOrderId.ToString(),
                ClientId = request.Account.Obj,
                AssetPairId = request.Symbol.Obj,
                Volume = volumeSign * (double)request.OrderQty.Obj,
                OrderAction = _mapper.Map<MatchingEngine.Connector.Abstractions.Models.OrderAction>(orderAction),
                Fee = await GetLimitOrderFeeAsync(request.Symbol.Obj, orderAction),
                Price = (double)request.Price.Obj,
                CancelPreviousOrders = false
            };

            var ack = CreteAckResponse(newOrderId, request);
            _sessionState.RegisterRequest(newOrderId.ToString(), $"Did not receive a response from ME for limit order with ID: {newOrderId}");
            Send(ack);

            using (var timeout = new CancellationTokenSource(Const.MeRequestTimeout))
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tokenSource.Token, timeout.Token))
            {
                var meResponse = await _matchingEngineClient.PlaceLimitOrderAsync(orderModel, linkedTokenSource.Token);
                await CheckResponseAndThrowIfNullAsync(meResponse);
                SendResponse(newOrderId, request, meResponse.Status);
                _sessionState.ConfirmRequest(newOrderId.ToString());
            }


        }

        private void SendResponse(Guid newOrderId, NewOrderSingle request, MeStatusCodes meStatus)
        {
            var status = (MessageStatus)meStatus;
            //Send only if received OK. Other messages we will receive via RabbitMq
            switch (status)
            {
                case MessageStatus.Duplicate:
                case MessageStatus.Runtime:
                    var rejectReason = _mapper.Map<OrdRejReason>(status);
                    var reject = new ExecutionReport().CreateReject(newOrderId, request, _sessionState.NextOrderReportId.ToString(), rejectReason.Obj, status.ToString());
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
                Id = newOrderId.ToString(),
                ClientId = request.Account.Obj,
                AssetPairId = request.Symbol.Obj,
                Straight = true,
                Volume = volumeSign * (double)request.OrderQty.Obj,
                OrderAction = _mapper.Map<MatchingEngine.Connector.Abstractions.Models.OrderAction>(orderAction),
                Fee = await GetMarketOrderFeeAsync(request.Symbol.Obj, orderAction),
                ReservedLimitVolume = null
            };

            var ack = CreteAckResponse(newOrderId, request);
            _sessionState.RegisterRequest(newOrderId.ToString(), $"Did not receive a response from ME for market order with ID: {newOrderId}");
            Send(ack);

            using (var timeout = new CancellationTokenSource(Const.MeRequestTimeout))
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tokenSource.Token, timeout.Token))
            {
                var meResponse = await _matchingEngineClient.HandleMarketOrderAsync(orderModel, linkedTokenSource.Token);
                await CheckResponseAndThrowIfNullAsync(meResponse);
                SendResponse(newOrderId, request, meResponse.Status);
                _sessionState.ConfirmRequest(newOrderId.ToString());
            }
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
            var assetPair = await _assetsService.TryGetAssetPairAsync(assetPairId);
            var fee = await _feeCalculatorClient.GetMarketOrderAssetFee(_credentials.ClientId.ToString(), assetPairId, assetPair?.BaseAssetId, _mapper.Map<FeeCalculator.AutorestClient.Models.OrderAction>(orderAction));

            return new MarketOrderFeeModel
            {
                Size = (double)fee.Amount,
                SourceClientId = _credentials.ClientId.ToString(),
                TargetClientId = _feeSettings.TargetClientId.Hft,
                Type = fee.Amount == 0 ? (int)MarketOrderFeeType.NO_FEE : (int)MarketOrderFeeType.CLIENT_FEE
            };
        }

        private async Task<LimitOrderFeeModel> GetLimitOrderFeeAsync(string assetPairId, OrderAction orderAction)
        {
            var assetPair = await _assetsService.TryGetAssetPairAsync(assetPairId);
            var fee = await _feeCalculatorClient.GetLimitOrderFees(_credentials.ClientId.ToString(), assetPairId, assetPair?.BaseAssetId, _mapper.Map<FeeCalculator.AutorestClient.Models.OrderAction>(orderAction));

            return new LimitOrderFeeModel
            {
                MakerSize = (double)fee.MakerFeeSize,
                TakerSize = (double)fee.TakerFeeSize,
                SourceClientId = _credentials.ClientId.ToString(),
                TargetClientId = _feeSettings.TargetClientId.Hft,
                Type = fee.MakerFeeSize == 0 && fee.TakerFeeSize == 0 ? (int)LimitOrderFeeType.NO_FEE : (int)LimitOrderFeeType.CLIENT_FEE
            };
        }




        private void Send(Message message)
        {
            _fixMessagesSender.Send(message);
        }

        private Message CreteAckResponse(Guid newOrderId, NewOrderSingle request)
        {
            var ack = new ExecutionReport
            {
                OrderID = new OrderID(newOrderId.ToString()),
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



        public void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
