using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.VO;

namespace PulseERP.Infrastructure.Identity
{
    /// <summary>
    /// Gère les opérations sécurisées sur les mots de passe (hash, vérification, changement, réinitialisation,
    /// jetons de réinitialisation). Utilise le Value Object <see cref="Password"/> pour valider la complexité
    /// des nouveaux mots de passe. Pour vérifier un mot de passe existant, on refait exactement le PBKDF2
    /// utilisé par le VO. On lance uniquement des exceptions concrètes de votre dossier Errors.
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly IEmailSenderService _emailService;
        private readonly ITokenGeneratorService _tokenGenerator;
        private readonly ITokenHasherService _tokenHasher;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(
            IUserRepository userRepository,
            ITokenRepository tokenRepository,
            IEmailSenderService emailService,
            IDateTimeProvider dateTimeProvider,
            ITokenGeneratorService tokenGenerator,
            ITokenHasherService tokenHasher,
            ILogger<PasswordService> logger
        )
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _emailService = emailService;
            _dateTimeProvider = dateTimeProvider;
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
            _logger = logger;
        }

        #region Hash & Vérification

        /// <summary>
        /// Hash un mot de passe brut en utilisant le VO <see cref="Password"/> (qui vérifie la complexité
        /// et produit un hash PBKDF2 sous la forme "Base64(salt):Base64(hash)").
        /// </summary>
        /// <param name="rawPassword">Le mot de passe en clair à hasher.</param>
        /// <returns>Le hash PBKDF2 (format "salt:hash").</returns>
        /// <exception cref="DomainValidationException">
        /// Si <paramref name="rawPassword"/> est nul, vide ou ne respecte pas les règles de complexité du VO.
        /// </exception>
        public string HashPassword(string rawPassword)
        {
            _logger.LogDebug("HashPassword DÉBUT à {TimeLocal}.", _dateTimeProvider.NowLocal);

            // Le constructeur du VO vérifie la complexité et calcule le hash.
            var vo = new Password(rawPassword);
            var hashed = vo.HashedValue;

            _logger.LogDebug("HashPassword FIN à {TimeLocal}.", _dateTimeProvider.NowLocal);
            return hashed;
        }

        /// <summary>
        /// Vérifie qu'un mot de passe brut correspond au hash PBKDF2 existant en base.
        /// On extrait le sel + hash du format "Base64Salt:Base64Hash", puis on refait un PBKDF2
        /// (100 000 itérations, HMACSHA256, longueur 32 octets) pour comparer en temps constant.
        /// </summary>
        /// <param name="rawPassword">Le mot de passe en clair candidat.</param>
        /// <param name="storedHash">Le hash stocké en base (format "salt:hash").</param>
        /// <returns>True si ça correspond, false sinon.</returns>
        public bool VerifyPassword(string rawPassword, string storedHash)
        {
            _logger.LogDebug("VerifyPassword DÉBUT à {TimeLocal}.", _dateTimeProvider.NowLocal);

            if (string.IsNullOrWhiteSpace(rawPassword) || string.IsNullOrWhiteSpace(storedHash))
            {
                _logger.LogWarning("VerifyPassword : mot de passe ou hash stocké vide.");
                return false;
            }

            var parts = storedHash.Split(':');
            if (parts.Length != 2)
            {
                _logger.LogWarning("VerifyPassword : format du hash stocké invalide.");
                return false;
            }

            byte[] saltBytes;
            byte[] expectedHashBytes;
            try
            {
                saltBytes = Convert.FromBase64String(parts[0]);
                expectedHashBytes = Convert.FromBase64String(parts[1]);
            }
            catch (FormatException)
            {
                _logger.LogWarning("VerifyPassword : échec du Base64 decoding.");
                return false;
            }

            // On reproduit exactement les paramètres PBKDF2 de Password.ComputeHash :
            //   • Sel de 16 octets (extrait ci‐dessus)
            //   • 100 000 itérations
            //   • Hash de 32 octets
            //   • HMACSHA256
            using var derive = new Rfc2898DeriveBytes(
                rawPassword,
                saltBytes,
                100_000,
                HashAlgorithmName.SHA256
            );
            var actualHashBytes = derive.GetBytes(32);

            var verified = CryptographicOperations.FixedTimeEquals(
                expectedHashBytes,
                actualHashBytes
            );
            _logger.LogDebug(
                "VerifyPassword FIN à {TimeLocal}. Résultat={Result}.",
                _dateTimeProvider.NowLocal,
                verified
            );
            return verified;
        }

        #endregion

        #region Changement / Réinitialisation sans jeton

        /// <summary>
        /// Change le mot de passe d'un utilisateur après avoir vérifié l'ancien.
        /// Lance :
        ///  • NotFoundException si l'utilisateur n'existe pas.
        ///  • ValidationException si le mot de passe actuel est incorrect.
        ///  • DomainValidationException si le nouveau mot de passe ne respecte pas la complexité.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur dont on change le mot de passe.</param>
        /// <param name="currentRawPassword">Mot de passe actuel (en clair).</param>
        /// <param name="newRawPassword">Nouveau mot de passe (en clair).</param>
        public async Task ChangePasswordAsync(
            Guid userId,
            string currentRawPassword,
            string newRawPassword
        )
        {
            var nowLocal = _dateTimeProvider.NowLocal;
            _logger.LogDebug(
                "ChangePasswordAsync DÉBUT : UserId={UserId} à {TimeLocal}. Vérification de l'ancien mot de passe.",
                userId,
                nowLocal
            );

            // 1. On charge l'utilisateur
            var user =
                await _userRepository.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(User), userId);

            // 2. On vérifie l'ancien mot de passe
            if (!VerifyPassword(currentRawPassword, user.PasswordHash))
            {
                _logger.LogWarning(
                    "ChangePasswordAsync : mot de passe actuel incorrect pour UserId={UserId} à {TimeLocal}.",
                    userId,
                    nowLocal
                );

                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        {
                            nameof(currentRawPassword),
                            new[] { "Le mot de passe actuel est incorrect." }
                        },
                    }
                );
            }

            // 3. On valide & hash le nouveau mot de passe via VO (lance DomainValidationException si échec)
            Password newVo;
            try
            {
                newVo = new Password(newRawPassword);
            }
            catch (DomainValidationException ex)
            {
                _logger.LogWarning(
                    "ChangePasswordAsync : validation du nouveau mot de passe échouée pour UserId={UserId} : {Error} à {TimeLocal}.",
                    userId,
                    ex.Message,
                    nowLocal
                );
                throw; // on relance DomainValidationException
            }

            // 4. On appelle le domaine pour mettre à jour le hash (enqueue UserPasswordChangedEvent)
            user.UpdatePassword(newVo.HashedValue);
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation(
                "ChangePasswordAsync SUCCÈS : UserId={UserId} à {TimeLocal}.",
                userId,
                _dateTimeProvider.NowLocal
            );
        }

        /// <summary>
        /// Réinitialise sans vérification l'utilisateur spécifié (opération admin).
        /// Lance :
        ///  • NotFoundException si l'utilisateur n'existe pas.
        ///  • DomainValidationException si le nouveau mot de passe ne respecte pas la complexité.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur à réinitialiser.</param>
        /// <param name="newRawPassword">Le nouveau mot de passe (en clair).</param>
        public async Task ResetPasswordAsync(Guid userId, string newRawPassword)
        {
            var nowLocal = _dateTimeProvider.NowLocal;
            _logger.LogDebug(
                "ResetPasswordAsync DÉBUT : UserId={UserId} à {TimeLocal}.",
                userId,
                nowLocal
            );

            var user =
                await _userRepository.FindByIdAsync(userId)
                ?? throw new NotFoundException(nameof(User), userId);

            // Valider & hash du nouveau mot de passe
            Password newVo;
            try
            {
                newVo = new Password(newRawPassword);
            }
            catch (DomainValidationException ex)
            {
                _logger.LogWarning(
                    "ResetPasswordAsync : validation du mot de passe échouée pour UserId={UserId} : {Error} à {TimeLocal}.",
                    userId,
                    ex.Message,
                    nowLocal
                );
                throw; // rethrow
            }

            // Mise à jour domaine
            user.UpdatePassword(newVo.HashedValue);
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation(
                "ResetPasswordAsync SUCCÈS : UserId={UserId} à {TimeLocal}.",
                userId,
                _dateTimeProvider.NowLocal
            );
        }

        /// <summary>
        /// Force la réinitialisation du mot de passe et oblige un changement au prochain login.
        /// Lance :
        ///  • NotFoundException si l'email n'existe pas.
        ///  • DomainValidationException si le nouveau mot de passe ne respecte pas la complexité.
        /// </summary>
        /// <param name="emailAddress">Email de l'utilisateur à réinitialiser.</param>
        /// <param name="newRawPassword">Le nouveau mot de passe (en clair).</param>
        public async Task ForcePasswordResetAsync(EmailAddress emailAddress, string newRawPassword)
        {
            var nowLocal = _dateTimeProvider.NowLocal;
            _logger.LogDebug(
                "ForcePasswordResetAsync DÉBUT : Email={Email} à {TimeLocal}.",
                emailAddress.Value,
                nowLocal
            );

            var user =
                await _userRepository.FindByEmailAsync(emailAddress)
                ?? throw new NotFoundException(nameof(User), emailAddress.Value);

            // Valider & hash du nouveau mot de passe
            Password newVo;
            try
            {
                newVo = new Password(newRawPassword);
            }
            catch (DomainValidationException ex)
            {
                _logger.LogWarning(
                    "ForcePasswordResetAsync : validation du mot de passe échouée pour Email={Email} : {Error} à {TimeLocal}.",
                    emailAddress.Value,
                    ex.Message,
                    nowLocal
                );
                throw; // rethrow
            }

            // Mise à jour domaine + forcer changement
            user.UpdatePassword(newVo.HashedValue); // enqueues UserPasswordChangedEvent
            user.ForcePasswordReset(); // enqueues UserPasswordResetForcedEvent

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogWarning(
                "ForcePasswordResetAsync SUCCÈS : admin-forcé pour UserId={UserId}, Email={Email} à {TimeLocal}.",
                user.Id,
                emailAddress.Value,
                _dateTimeProvider.NowLocal
            );
        }

        #endregion

        #region Flux « Mot de passe oublié » (Password Reset Token)

        /// <summary>
        /// Démarre la procédure de réinitialisation de mot de passe : génère un jeton, le stocke,
        /// puis envoie un e-mail contenant ce jeton. Lance <see cref="NotFoundException"/> si l’e-mail introuvable.
        /// </summary>
        /// <param name="emailAddress">L’adresse e-mail de l’utilisateur qui a oublié son mot de passe.</param>
        public async Task RequestPasswordResetAsync(EmailAddress emailAddress)
        {
            var nowLocal = _dateTimeProvider.NowLocal;
            _logger.LogDebug(
                "RequestPasswordResetAsync DÉBUT : Email={Email} à {TimeLocal}.",
                emailAddress.Value,
                nowLocal
            );

            var user =
                await _userRepository.FindByEmailAsync(emailAddress)
                ?? throw new NotFoundException(nameof(User), emailAddress.Value);

            // 1. Générer un jeton aléatoire
            var rawToken = _tokenGenerator.GenerateToken();

            // 2. Hasher ce jeton avant de le stocker
            var tokenHash = _tokenHasher.Hash(rawToken);

            // 3. Expiration à +1h
            var expiresUtc = _dateTimeProvider.UtcNow.AddHours(1);

            // 4. Créer et persister un RefreshToken de type PasswordReset
            var resetTokenEntity = RefreshToken.Create(
                _dateTimeProvider,
                user.Id,
                tokenHash,
                TokenType.PasswordReset,
                expiresUtc
            );
            await _tokenRepository.AddAsync(resetTokenEntity);

            // 5. Construire l’URL de réinitialisation et envoyer l’email
            var resetUrl =
                $"https://app.pulseerp.com/reset-password?token={Uri.EscapeDataString(rawToken)}";
            await _emailService.SendPasswordResetEmailAsync(
                user.Email.Value,
                $"{user.FirstName} {user.LastName}",
                resetUrl,
                expiresUtc
            );

            _logger.LogInformation(
                "RequestPasswordResetAsync : jeton généré et envoyé à {Email} à {TimeLocal}.",
                emailAddress.Value,
                _dateTimeProvider.NowLocal
            );
        }

        /// <summary>
        /// Termine la réinitialisation à l’aide d’un jeton valide et d’un nouveau mot de passe.
        /// Lance :
        ///  • <see cref="DomainException"/> si le jeton est invalide ou expiré,
        ///  • <see cref="NotFoundException"/> si l’utilisateur n’existe pas,
        ///  • <see cref="DomainValidationException"/> si le mot de passe ne respecte pas la complexité.
        /// </summary>
        /// <param name="resetToken">Le jeton envoyé par e-mail (en clair).</param>
        /// <param name="newRawPassword">Le nouveau mot de passe (en clair).</param>
        public async Task ResetPasswordWithTokenAsync(string resetToken, string newRawPassword)
        {
            var nowLocal = _dateTimeProvider.NowLocal;
            _logger.LogDebug("ResetPasswordWithTokenAsync DÉBUT à {TimeLocal}.", nowLocal);

            // 1. Hash du jeton reçu pour trouver l’entité correspondante
            var tokenHash = _tokenHasher.Hash(resetToken);

            // 2. Recherche de l’entité en base
            var entity = await _tokenRepository.GetByTokenAndTypeAsync(
                tokenHash,
                TokenType.PasswordReset
            );
            if (entity == null)
            {
                _logger.LogWarning(
                    "ResetPasswordWithTokenAsync ÉCHEC : jeton invalide ou expiré à {TimeLocal}.",
                    _dateTimeProvider.NowLocal
                );
                throw new BadRequestException("Jeton de réinitialisation invalide ou expiré.");
            }

            // 3. Chargement de l’utilisateur
            var user =
                await _userRepository.FindByIdAsync(entity.UserId)
                ?? throw new NotFoundException(nameof(User), entity.UserId);

            // 4. Révocation de tous les jets de réinitialisation pour cet utilisateur
            entity.Revoke(_dateTimeProvider.UtcNow);
            await _tokenRepository.RevokeAllByUserIdAndTypeAsync(
                entity.UserId,
                TokenType.PasswordReset
            );

            // 5. Validation & hash du nouveau mot de passe
            Password newVo;
            try
            {
                newVo = new Password(newRawPassword);
            }
            catch (DomainValidationException ex)
            {
                _logger.LogWarning(
                    "ResetPasswordWithTokenAsync : validation du mot de passe échouée : {Error} à {TimeLocal}.",
                    ex.Message,
                    nowLocal
                );
                throw; // rethrow pour propager DomainValidationException
            }

            // 6. Mise à jour du domaine
            user.UpdatePassword(newVo.HashedValue);
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation(
                "ResetPasswordWithTokenAsync SUCCÈS : UserId={UserId} à {TimeLocal}.",
                entity.UserId,
                _dateTimeProvider.NowLocal
            );
        }

        #endregion
    }
}


// using Microsoft.Extensions.Logging;
// using PulseERP.Abstractions.Security.Interfaces;
// using PulseERP.Domain.Entities;
// using PulseERP.Domain.Enums.Token;
// using PulseERP.Domain.Errors;
// using PulseERP.Domain.Interfaces;
// using PulseERP.Domain.Security.Interfaces;
// using PulseERP.Domain.VO;

// namespace PulseERP.Infrastructure.Identity;

// /// <summary>
// /// Handles secure password operations (hash, verify, change, reset, and password reset tokens).
// /// Uses Value Objects for all input. All logs are context-rich and safe.
// /// </summary>
// public class PasswordService : IPasswordService
// {
//     private readonly IUserRepository _userRepository;
//     private readonly ITokenRepository _tokenRepository;
//     private readonly IEmailSenderService _emailService;
//     private readonly ITokenGeneratorService _tokenGenerator;
//     private readonly ITokenHasherService _tokenHasher;
//     private readonly IDateTimeProvider _dateTimeProvider;
//     private readonly ILogger<PasswordService> _logger;

//     /// <summary>
//     /// Initializes a new instance of <see cref="PasswordService"/>.
//     /// </summary>
//     public PasswordService(
//         IUserRepository userRepository,
//         ITokenRepository tokenRepository,
//         IEmailSenderService emailService,
//         IDateTimeProvider dateTimeProvider,
//         ITokenGeneratorService tokenGenerator,
//         ITokenHasherService tokenHasher,
//         ILogger<PasswordService> logger
//     )
//     {
//         _userRepository = userRepository;
//         _tokenRepository = tokenRepository;
//         _emailService = emailService;
//         _dateTimeProvider = dateTimeProvider;
//         _tokenGenerator = tokenGenerator;
//         _tokenHasher = tokenHasher;
//         _logger = logger;
//     }

//     /// <summary>
//     /// Hashes the provided password using a secure algorithm.
//     /// </summary>
//     public string HashPassword(string password)
//     {
//         _logger.LogDebug("Hashing password at {TimeLocal}.", _dateTimeProvider.NowLocal);
//         return BCrypt.Net.BCrypt.HashPassword(password);
//     }

//     /// <summary>
//     /// Verifies the provided password against a hashed password.
//     /// </summary>
//     public bool VerifyPassword(Password password, string hashedPassword)
//     {
//         _logger.LogDebug("Verifying password hash at {TimeLocal}.", _dateTimeProvider.NowLocal);
//         return BCrypt.Net.BCrypt.Verify(password.ToString(), hashedPassword);
//     }

//     /// <summary>
//     /// Changes a user's password after verifying the current one.
//     /// Throws if the current password does not match.
//     /// </summary>
//     public async Task ChangePasswordAsync(
//         Guid userId,
//         Password currentPassword,
//         Password newPassword
//     )
//     {
//         var user = await _userRepository.FindByIdAsync(userId) ?? throw new("User not found.");

//         if (!VerifyPassword(currentPassword, user.PasswordHash))
//         {
//             _logger.LogWarning(
//                 "Failed password change: current password incorrect for user {UserId} at {TimeLocal}.",
//                 userId,
//                 _dateTimeProvider.NowLocal
//             );
//             throw new DomainException("Current password is incorrect.");
//         }

//         await UpdateUserPasswordAsync(user, newPassword, requirePasswordChange: false);
//         _logger.LogInformation(
//             "Password changed for user {UserId} at {TimeLocal}.",
//             userId,
//             _dateTimeProvider.NowLocal
//         );
//     }

//     /// <summary>
//     /// Directly resets a user's password (admin or internal process).
//     /// Does not require the old password.
//     /// </summary>
//     public async Task ResetPasswordAsync(Guid userId, Password newPassword)
//     {
//         var user =
//             await _userRepository.FindByIdAsync(userId)
//             ?? throw new DomainException("User not found.");

//         await UpdateUserPasswordAsync(user, newPassword, requirePasswordChange: false);
//         _logger.LogInformation(
//             "Password reset for user {UserId} at {TimeLocal}.",
//             userId,
//             _dateTimeProvider.NowLocal
//         );
//     }

//     /// <summary>
//     /// Forces a user's password to be reset and requires them to change it at next login.
//     /// </summary>
//     public async Task ForcePasswordResetAsync(EmailAddress email, Password newPassword)
//     {
//         var user =
//             await _userRepository.FindByEmailAsync(email.Value)
//             ?? throw new DomainException("Unknown email.");

//         await UpdateUserPasswordAsync(user, newPassword, requirePasswordChange: true);

//         _logger.LogWarning(
//             "Password forcibly reset by admin for user {UserId} ({Email}) at {TimeLocal}.",
//             user.Id,
//             email.Value,
//             _dateTimeProvider.NowLocal
//         );
//     }

//     /// <summary>
//     /// Initiates a password reset flow by generating a reset token and sending it via email.
//     /// </summary>/// <summary>
//     /// Initiates a password reset flow: generates a reset token, stores it, and sends an email with the reset link.
//     /// </summary>
//     public async Task RequestPasswordResetAsync(EmailAddress email)
//     {
//         var user =
//             await _userRepository.FindByEmailAsync(email)
//             ?? throw new DomainException("Unknown email.");

//         var rawToken = _tokenGenerator.GenerateToken();
//         var tokenHash = _tokenHasher.Hash(rawToken);
//         var expiresUtc = _dateTimeProvider.UtcNow.AddHours(1);

//         var resetTokenEntity = RefreshToken.Create(
//             _dateTimeProvider,
//             user.Id,
//             tokenHash,
//             TokenType.PasswordReset,
//             expiresUtc
//         );

//         await _tokenRepository.AddAsync(resetTokenEntity);

//         // Génère l'URL de reset (à adapter selon ta conf front)
//         var resetUrl =
//             $"https://app.pulseerp.com/reset-password?token={Uri.EscapeDataString(rawToken)}";

//         await _emailService.SendPasswordResetEmailAsync(
//             user.Email.Value,
//             $"{user.FirstName} {user.LastName}",
//             resetUrl,
//             expiresUtc
//         );

//         _logger.LogInformation(
//             "Password reset token generated and sent to {Email} at {TimeLocal}.",
//             email.Value,
//             _dateTimeProvider.NowLocal
//         );
//     }

//     /// <summary>
//     /// Resets a user's password using a valid password reset token.
//     /// </summary>
//     public async Task ResetPasswordWithTokenAsync(string resetToken, Password newPassword)
//     {
//         var tokenHash = _tokenHasher.Hash(resetToken);
//         var entity = await _tokenRepository.GetByTokenAndTypeAsync(
//             tokenHash,
//             TokenType.PasswordReset
//         );

//         if (entity == null)
//         {
//             _logger.LogWarning(
//                 "Password reset with token failed (invalid or expired) at {TimeLocal}.",
//                 _dateTimeProvider.NowLocal
//             );
//             throw new DomainException("Invalid or expired reset token.");
//         }

//         var user =
//             await _userRepository.FindByIdAsync(entity.UserId)
//             ?? throw new DomainException("User not found.");

//         entity.Revoke(_dateTimeProvider.UtcNow);
//         await _tokenRepository.RevokeAllByUserIdAndTypeAsync(
//             entity.UserId,
//             TokenType.PasswordReset
//         );

//         await UpdateUserPasswordAsync(user, newPassword, requirePasswordChange: false);

//         _logger.LogInformation(
//             "Password reset by token for user {UserId} at {TimeLocal}.",
//             entity.UserId,
//             _dateTimeProvider.NowLocal
//         );
//     }

//     /// <summary>
//     /// Updates a user's password, handles flag for requiring password change.
//     /// </summary>
//     /// <param name="user">User entity to update.</param>
//     /// <param name="newPassword">New password (Value Object).</param>
//     /// <param name="requirePasswordChange">If true, will require change on next login.</param>
//     private async Task UpdateUserPasswordAsync(
//         User user,
//         Password newPassword,
//         bool requirePasswordChange
//     )
//     {
//         user.UpdatePassword(HashPassword(newPassword.Value));
//         if (requirePasswordChange)
//             user.RequirePasswordReset();
//         await _userRepository.UpdateAsync(user);
//     }
// }
