﻿using System;
using AutoMapper;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;
using QuickFix.Fields;

namespace Lykke.Service.FixGateway.Services.Mappings
{
    public sealed class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Side, OrderAction>().ConvertUsing((side, action) =>
            {
                switch (side.Obj)
                {
                    case Side.BUY:
                        return OrderAction.Buy;
                    case Side.SELL:
                        return OrderAction.Sell;
                    default:
                        throw new ArgumentException(nameof(side));
                }
            });

            CreateMap<OrderAction, Side>().ConvertUsing((action, side) =>
            {
                switch (action)
                {
                    case OrderAction.Buy:
                        return new Side(Side.BUY);
                    case OrderAction.Sell:
                        return new Side(Side.SELL);
                    default:
                        throw new ArgumentException(nameof(action));
                }
            });

            CreateMap<MessageStatus, OrdRejReason>().ConvertUsing((codes, reason) =>
            {
                int fixReason;
                switch (codes)
                {
                    case MessageStatus.LowBalance:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case MessageStatus.AlreadyProcessed:
                        fixReason = OrdRejReason.STALE_ORDER;
                        break;
                    case MessageStatus.UnknownAsset:
                        fixReason = OrdRejReason.UNKNOWN_SYMBOL;
                        break;
                    case MessageStatus.NoLiquidity:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case MessageStatus.NotEnoughFunds:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case MessageStatus.ReservedVolumeHigherThanBalance:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case MessageStatus.BalanceLowerThanReserved:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case MessageStatus.LeadToNegativeSpread:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case MessageStatus.Duplicate:
                        fixReason = OrdRejReason.DUPLICATE_ORDER;
                        break;
                    case MessageStatus.Runtime:
                        fixReason = OrdRejReason.OTHER;
                        break;
                    case MessageStatus.LimitOrderNotFound:
                        fixReason = OrdRejReason.UNKNOWN_ORDER;
                        break;
                    case MessageStatus.TooSmallVolume:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case MessageStatus.InvalidFee:
                        fixReason = OrdRejReason.OTHER;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(codes), codes, null);
                }

                return new OrdRejReason(fixReason);
            });
            CreateMap<OrderStatus, OrdRejReason>().ConvertUsing((codes, reason) =>
            {
                int fixReason;
                switch (codes)
                {
                    case OrderStatus.NotEnoughFunds:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case OrderStatus.UnknownAsset:
                        fixReason = OrdRejReason.UNKNOWN_SYMBOL;
                        break;
                    case OrderStatus.NoLiquidity:
                        fixReason = OrdRejReason.OTHER;
                        break;
                    case OrderStatus.ReservedVolumeGreaterThanBalance:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case OrderStatus.LeadToNegativeSpread:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case OrderStatus.TooSmallVolume:
                        fixReason = OrdRejReason.ORDER_EXCEEDS_LIMIT;
                        break;
                    case OrderStatus.InvalidFee:
                        fixReason = OrdRejReason.OTHER;
                        break;
                    default:
                        fixReason = OrdRejReason.OTHER;
                        break;
                }

                return new OrdRejReason(fixReason);
            });

            CreateMap<OrderAction, FeeCalculator.AutorestClient.Models.OrderAction>();
            CreateMap<FeeCalculator.AutorestClient.Models.OrderAction, OrderAction>();
            CreateMap<OrderAction, MatchingEngine.Connector.Abstractions.Models.OrderAction>();
            CreateMap<MatchingEngine.Connector.Abstractions.Models.OrderAction, OrderAction>();
        }
    }
}
