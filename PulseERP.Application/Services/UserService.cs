using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.DTOs.Users.Commands;
using PulseERP.Abstractions.Common.DTOs.Users.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.VO;

namespace PulseERP.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserService(
        IUserRepository userRepository,
        IDateTimeProvider dateTimeProvider,
        IPasswordService passwordService,
        IMapper mapper,
        ILogger<UserService> logger
    )
    {
        _userRepository = userRepository;
        _dateTimeProvider = dateTimeProvider;
        _passwordService = passwordService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<UserSummary>> GetAllAsync(UserFilter userFilter)
    {
        var result = await _userRepository.GetAllAsync(userFilter);

        var summaries = _mapper.Map<List<UserSummary>>(result.Items);

        return new PagedResult<UserSummary>()
        {
            Items = summaries,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalItems = summaries.Count(),
        };
    }

    public async Task<UserDetails> GetByIdAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id, true)
            ?? throw new NotFoundException("User", id);

        var nowUtc = _dateTimeProvider.UtcNow;
        user.CheckPasswordExpiration(nowUtc);

        return _mapper.Map<UserDetails>(user);
    }

    public async Task<UserInfo> CreateAsync(CreateUserCommand cmd)
    {
        EmailAddress emailVO = new EmailAddress(cmd.Email);
        Phone phoneNumberVO = new Phone(cmd.PhoneNumber);
        Password passwordVO = new Password(cmd.Password);
        string passwordHash = passwordVO.HashedValue;

        var user = new User(cmd.FirstName, cmd.LastName, emailVO, phoneNumberVO, passwordHash);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserInfo>(user);
    }

    public async Task<UserDetails> UpdateAsync(Guid id, UpdateUserCommand cmd)
    {
        var user =
            await _userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        if (!string.IsNullOrWhiteSpace(cmd.FirstName) || !string.IsNullOrWhiteSpace(cmd.LastName))
            user.UpdateName(cmd.FirstName, cmd.LastName);

        if (!string.IsNullOrWhiteSpace(cmd.Email))
        {
            var newEmailVO = new EmailAddress(cmd.Email);
            user.UpdateEmail(newEmailVO);
        }

        if (!string.IsNullOrWhiteSpace(cmd.Phone))
        {
            var newPhoneVO = new Phone(cmd.Phone);
            user.UpdatePhone(newPhoneVO);
        }

        if (!string.IsNullOrWhiteSpace(cmd.Role))
        {
            var newRoleVO = new Role(cmd.Role);
            user.ChangeRole(newRoleVO);
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserDetails>(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ActivateUserAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        if (user.IsDeleted)
        {
            throw new InvalidOperationException(
                $"Impossible d'activer l'utilisateur ({id}) : il est marqué comme sup​primé."
            );
        }

        if (user.IsActive)
        {
            throw new InvalidOperationException($"L'utilisateur ({id}) est déjà actif.");
        }

        user.MarkAsActivate();

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task DeactivateUserAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        if (user.IsDeleted)
        {
            throw new InvalidOperationException(
                $"Impossible de désactiver l'utilisateur ({id}) : il est marqué comme sup​primé."
            );
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException($"L'utilisateur ({id}) est déjà désactivé.");
        }

        user.MarkAsDeactivate();

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ResetLockoutAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        user.ResetLockout();

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task RestoreUserAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id, bypassCache: true)
            ?? throw new NotFoundException("User", id);

        user.MarkAsRestored();

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }
}
