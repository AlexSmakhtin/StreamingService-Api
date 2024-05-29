using Application.DTOs;
using System.Security.Claims;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("api/subscriptions")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;

    public SubscriptionController(
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        ITransactionRepository transactionRepository)
    {
        ArgumentNullException.ThrowIfNull(subscriptionRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(transactionRepository);

        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<SubscriptionToBuyResponse>>> GetSubscriptions(CancellationToken ct)
    {
        var subs = await _subscriptionRepository.GetAll(ct);

        return subs
            .Select(sub => new SubscriptionToBuyResponse()
            {
                Id = sub.Id,
                Cost = sub.Cost,
                Duration = sub.DurationInDays,
                Name = sub.Name
            }).ToList();
    }

    [HttpPost("buy")]
    [Authorize]
    public async Task<IActionResult> BuySubscription([FromQuery] Guid subscriptionId, CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UserNotFoundException("Token does not contain the userId");
        var guid = Guid.Parse(strId);
        var user = await _userRepository.GetById(guid, ct);
        var subscription = await _subscriptionRepository.GetById(subscriptionId, ct);
        try
        {
            var lastTransaction = await _transactionRepository.GetActiveSubscription(user.Id, ct);
            var transaction = new Transaction(lastTransaction.ExpirationDate.ToUniversalTime())
            {
                User = user,
                Subscription = subscription
            };
            await _transactionRepository.Add(transaction, ct);

            return Created();
        }
        catch (InvalidOperationException)
        {
            var transaction = new Transaction(DateTime.Now.ToUniversalTime())
            {
                User = user,
                Subscription = subscription
            };
            await _transactionRepository.Add(transaction, ct);
            return Created();
        }
    }
}