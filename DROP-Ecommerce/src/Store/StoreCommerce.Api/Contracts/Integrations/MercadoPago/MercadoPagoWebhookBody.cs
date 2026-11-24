using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

namespace StoreCommerce.Api.Controllers;

public sealed record MercadoPagoWebhookBody(string? Type, MercadoPagoWebhookData? Data);
