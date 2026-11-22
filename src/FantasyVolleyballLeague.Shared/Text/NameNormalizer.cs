using System.Globalization;
using System.Text;

namespace FantasyVolleyballLeague.Shared.Text
{
    /// <summary>
    /// The one rule that turns a name into its searchable form: the <c>NormalizedName</c> columns, the
    /// <c>?q=</c> search words and the importer's name comparison all use it, so both sides of a match are
    /// normalized the same way.
    /// Lower case, no diacritics (letters without a Unicode decomposition are mapped: <c>ł</c> to <c>l</c>,
    /// <c>ß</c> to <c>ss</c>), apostrophes and dots removed (<c>O'Brien</c> gives <c>obrien</c>,
    /// <c>S.S.A.</c> gives <c>ssa</c>), every other character that is not a letter or a digit separates words
    /// </summary>
    public static class NameNormalizer
    {
        public static string Normalize(string text)
        {
            return string.Join(' ', Tokens(text));
        }

        public static string[] Tokens(string text)
        {
            var tokens = new List<string>();
            var token = new StringBuilder();

            foreach (var character in text.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (IsDroppedPunctuation(character))
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    token.Append(ReplaceSpecialLetter(char.ToLowerInvariant(character)));
                    continue;
                }

                FlushToken(token, tokens);
            }

            FlushToken(token, tokens);
            return [.. tokens];
        }

        private static void FlushToken(StringBuilder token, List<string> tokens)
        {
            if (token.Length == 0)
            {
                return;
            }

            tokens.Add(token.ToString());
            token.Clear();
        }

        private static bool IsDroppedPunctuation(char character)
        {
            return character is '\'' or '’' or '‘' or '`' or '´' or '.';
        }

        // Letters that have no decomposed form, so FormD does not strip them.
        private static string ReplaceSpecialLetter(char character)
        {
            return character switch
            {
                'ł' => "l",
                'đ' => "d",
                'ø' => "o",
                'ß' => "ss",
                'æ' => "ae",
                'œ' => "oe",
                'þ' => "th",
                'ð' => "d",
                'ı' => "i",
                _ => character.ToString()
            };
        }
    }
}
