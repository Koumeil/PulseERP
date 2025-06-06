namespace PulseERP.Application.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using PulseERP.Domain.VO;

public static class CurrencyHelper
{
    /// <summary>
    /// Renvoie une instance de <see cref="Currency"/> (3 lettres, ex. "EUR" ou "USD")
    /// en fonction de la région (locale) du système.
    /// </summary>
    public static Currency GetCurrencyByCurrentRegion()
    {
        // Liste ISO 2 lettres des pays utilisant l’euro
        var euroCountries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AT",
            "BE",
            "CY",
            "EE",
            "FI",
            "FR",
            "DE",
            "GR",
            "IE",
            "IT",
            "LV",
            "LT",
            "LU",
            "MT",
            "NL",
            "PT",
            "SK",
            "SI",
            "ES",
        };

        RegionInfo regionInfo;
        try
        {
            // CultureInfo.CurrentCulture.Name -> "fr-FR", "en-US", "de-DE", etc.
            regionInfo = new RegionInfo(CultureInfo.CurrentCulture.Name);
        }
        catch
        {
            // Si la culture n'est pas au format attendu, on choisit par défaut USD
            return new Currency("USD");
        }

        // Si le pays (code ISO 2 lettres) fait partie de la zone euro
        if (euroCountries.Contains(regionInfo.TwoLetterISORegionName))
        {
            return new Currency("EUR");
        }

        // Si c’est spécifiquement les États-Unis
        if (regionInfo.TwoLetterISORegionName.Equals("US", StringComparison.OrdinalIgnoreCase))
        {
            return new Currency("USD");
        }

        // Sinon, on peut ajouter d’autres cas (par ex. "GBP" pour GB, "CHF" pour CH, etc.)
        // Par défaut, on retourne USD
        return new Currency("USD");
    }
}
