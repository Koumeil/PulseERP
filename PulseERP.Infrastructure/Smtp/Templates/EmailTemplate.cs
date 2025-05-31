using System.Globalization;
using MimeKit;

namespace PulseERP.Infrastructure.Smtp.Template
{
    internal static class EmailTemplates
    {
        // Format français « vendredi 30 mai 2025 à 17 h 30 »
        private static string FormatFrenchDate(DateTime dt)
        {
            var fr = CultureInfo.GetCultureInfo("fr-BE");
            return dt.ToString("dddd d MMMM yyyy 'à' H 'h' mm", fr);
        }

        /// <summary>
        /// Compte verrouillé
        /// </summary>
        public static BodyBuilder BuildAccountLocked(DateTime lockoutEnd, string userFullName)
        {
            var friendlyDate = FormatFrenchDate(lockoutEnd);

            var html =
                $@"
<!DOCTYPE html>
<html lang=""fr"">
<head><meta charset=""UTF-8""/><style>
  body {{ margin:0; padding:0; font-family:'Segoe UI',sans-serif; background:#f0f4f8; }}
  .container {{ width:100%; max-width:600px; margin:40px auto; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
  .header {{ background:#c62828; color:#fff; padding:20px; text-align:center; }}
  .header h1 {{ margin:0; font-size:24px; }}
  .content {{ padding:30px; color:#333; line-height:1.6; }}
  .content p {{ margin-bottom:16px; }}
  .btn {{ display:inline-block; padding:14px 28px; background:#1976d2; color:#fff; text-decoration:none; border-radius:4px; font-weight:600; }}
  .footer {{ background:#f7f9fc; padding:20px; text-align:center; font-size:13px; color:#777; }}
</style></head>
<body>
  <div class=""container"">
    <div class=""header""><h1>🔒 Compte verrouillé</h1></div>
    <div class=""content"">
      <p>Bonjour <strong>{userFullName}</strong>,</p>
      <p>Par mesure de sécurité, votre compte Pulse ERP est verrouillé jusqu’au <strong>{friendlyDate}</strong>.</p>
      <p>Si vous n’êtes pas à l’origine de ces tentatives, cliquez ci-dessous pour réinitialiser votre mot de passe :</p>
      <p style=""text-align:center;"">
        <a href=""https://app.pulseepr.com/mot-de-passe-oublie"" class=""btn"">Réinitialiser le mot de passe</a>
      </p>
      <p>Merci de votre confiance,</p>
      <p><em>L’équipe Pulse ERP</em></p>
    </div>
    <div class=""footer"">Ce message a été généré automatiquement – merci de ne pas répondre.</div>
  </div>
</body>
</html>
";

            var text =
                $@"
Bonjour {userFullName},

Votre compte Pulse ERP est verrouillé jusqu’au {friendlyDate}.

Si ce n’est pas vous, réinitialisez votre mot de passe :
https://app.pulseepr.com/mot-de-passe-oublie

Merci de votre confiance,
L’équipe Pulse ERP
";

            return new BodyBuilder { HtmlBody = html, TextBody = text };
        }

        /// <summary>
        /// Email de bienvenue
        /// </summary>
        public static BodyBuilder BuildWelcome(string userFullName, string loginUrl)
        {
            var html =
                $@"
<!DOCTYPE html>
<html lang=""fr"">
<head><meta charset=""UTF-8""/><style>
  body {{ margin:0; padding:0; font-family:'Segoe UI',sans-serif; background:#eef7f3; }}
  .container {{ width:100%; max-width:600px; margin:40px auto; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
  .header {{ background:#2e7d32; color:#fff; padding:20px; text-align:center; }}
  .header h1 {{ margin:0; font-size:24px; }}
  .content {{ padding:30px; color:#333; line-height:1.6; }}
  .content p {{ margin-bottom:16px; }}
  .btn {{ display:inline-block; padding:14px 28px; background:#43a047; color:#fff; text-decoration:none; border-radius:4px; font-weight:600; }}
  .footer {{ background:#f1f8e9; padding:20px; text-align:center; font-size:13px; color:#555; }}
</style></head>
<body>
  <div class=""container"">
    <div class=""header""><h1>🎉 Bienvenue, {userFullName} !</h1></div>
    <div class=""content"">
      <p>Votre compte Pulse ERP a été créé avec succès.</p>
      <p>Pour commencer, cliquez sur le bouton ci-dessous :</p>
      <p style=""text-align:center;"">
        <a href=""{loginUrl}"" class=""btn"">Accéder à mon compte</a>
      </p>
      <p>Nous sommes ravis de vous compter parmi nous !</p>
      <p><em>L’équipe Pulse ERP</em></p>
    </div>
    <div class=""footer"">Email automatique – merci de ne pas répondre.</div>
  </div>
</body>
</html>
";

            var text =
                $@"
Bonjour {userFullName},

Votre compte Pulse ERP a été créé avec succès.

Connectez-vous dès maintenant :
{loginUrl}

Nous sommes ravis de vous compter parmi nous !
– L’équipe Pulse ERP
";

            return new BodyBuilder { HtmlBody = html, TextBody = text };
        }

        /// <summary>
        /// Réinitialisation du mot de passe
        /// </summary>
        public static BodyBuilder BuildPasswordReset(
            string resetUrl,
            DateTime expiresAtUtc,
            string userFullName
        )
        {
            var expiry = FormatFrenchDate(expiresAtUtc);

            var html =
                $@"
<!DOCTYPE html>
<html lang=""fr"">
<head><meta charset=""UTF-8""/><style>
  body {{ margin:0; padding:0; font-family:'Segoe UI',sans-serif; background:#f0f0f5; }}
  .container {{ width:100%; max-width:600px; margin:40px auto; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
  .header {{ background:#1e88e5; color:#fff; padding:20px; text-align:center; }}
  .header h1 {{ margin:0; font-size:24px; }}
  .content {{ padding:30px; color:#333; line-height:1.6; }}
  .content p {{ margin-bottom:16px; }}
  .btn {{ display:inline-block; padding:14px 28px; background:#0288d1; color:#fff; text-decoration:none; border-radius:4px; font-weight:600; }}
  .footer {{ background:#e3f2fd; padding:20px; text-align:center; font-size:13px; color:#555; }}
</style></head>
<body>
  <div class=""container"">
    <div class=""header""><h1>🔑 Réinitialisation du mot de passe</h1></div>
    <div class=""content"">
      <p>Bonjour <strong>{userFullName}</strong>,</p>
      <p>Nous avons reçu une demande de réinitialisation de votre mot de passe.</p>
      <p>Cliquez sur le bouton ci-dessous pour en choisir un nouveau (valable jusqu’au <strong>{expiry}</strong>) :</p>
      <p style=""text-align:center;"">
        <a href=""{resetUrl}"" class=""btn"">Changer mon mot de passe</a>
      </p>
      <p>Si vous n’avez pas fait cette demande, ignorez cet email.</p>
      <p><em>L’équipe Pulse ERP</em></p>
    </div>
    <div class=""footer"">Email automatique – ne pas répondre.</div>
  </div>
</body>
</html>
";

            var text =
                $@"
Bonjour {userFullName},

Nous avons reçu une demande de réinitialisation de votre mot de passe.
Utilisez ce lien (valable jusqu’au {expiry}) :

{resetUrl}

Si vous n’êtes pas à l’origine, ignorez cet email.
– L’équipe Pulse ERP
";

            return new BodyBuilder { HtmlBody = html, TextBody = text };
        }

        /// <summary>
        /// Confirmation de changement de mot de passe
        /// </summary>
        public static BodyBuilder BuildPasswordChanged(string userFullName)
        {
            var date = FormatFrenchDate(DateTime.UtcNow);

            var html =
                $@"
<!DOCTYPE html>
<html lang=""fr"">
<head><meta charset=""UTF-8""/><style>
  body {{ margin:0; padding:0; font-family:'Segoe UI',sans-serif; background:#f9f5f0; }}
  .container {{ width:100%; max-width:600px; margin:40px auto; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.1); }}
  .header {{ background:#6a1b9a; color:#fff; padding:20px; text-align:center; }}
  .header h1 {{ margin:0; font-size:24px; }}
  .content {{ padding:30px; color:#333; line-height:1.6; }}
  .content p {{ margin-bottom:16px; }}
  .footer {{ background:#f3e5f5; padding:20px; text-align:center; font-size:13px; color:#555; }}
</style></head>
<body>
  <div class=""container"">
    <div class=""header""><h1>✅ Mot de passe changé</h1></div>
    <div class=""content"">
      <p>Bonjour <strong>{userFullName}</strong>,</p>
      <p>Votre mot de passe a bien été mis à jour le <strong>{date}</strong>.</p>
      <p>Si vous n’êtes pas à l’origine de ce changement, contactez immédiatement notre support :</p>
      <p style=""text-align:center;"">
        <a href=""https://support.pulseepr.com"" class=""btn"" style=""background:#6a1b9a;"">Contacter le support</a>
      </p>
      <p>Merci de votre confiance,</p>
      <p><em>L’équipe Pulse ERP</em></p>
    </div>
    <div class=""footer"">Email automatique – ne pas répondre.</div>
  </div>
</body>
</html>
";

            var text =
                $@"
Bonjour {userFullName},

Votre mot de passe a été mis à jour le {date}.

Si ce n’était pas vous, contactez notre support :
https://support.pulseepr.com

– L’équipe Pulse ERP
";

            return new BodyBuilder { HtmlBody = html, TextBody = text };
        }
    }
}
