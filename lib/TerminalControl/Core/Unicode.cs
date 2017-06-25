/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Unicode.cs,v 1.3 2012/01/29 14:42:05 kzmi Exp $
 */
using System;

namespace Poderosa.Document {

    /// <summary>
    /// <ja>特別な処理を行うため、特定の文字を私用領域(U+E000-U+F8FF)にマップする。</ja>
    /// <en>This class maps some characters to the private use area (U+E000-U+F8FF) for handling them specially.</en>
    /// </summary>
    public static class Unicode {

        // Areas:
        // U+E080 - U+E0FF : Half-width characters in U+0080 - U+00FF (CJK font)
        // U+E180 - U+E1FF : Full-width characters in U+0080 - U+00FF (CJK font)
        // U+E200 - U+E2FF : Half-width characters in U+2500 - U+25FF (CJK font)
        // U+E300 - U+E3FF : Full-width characters in U+2500 - U+25FF (CJK font)
        // U+E400 - U+E6FF : Full-width characters in U+0200 - U+04FF (CJK font)

        // Tables consist of the following values.
        //
        // 0 : Don't map to private area. Character is displayed as half-width using latin font.
        // 1 : Map to private area. Character is displayed as half-width using CJK font.
        // 2 : Map to private area. Character is displayed as full-width using CJK font.
        //
        // Symbols or letters that are contained in CJK character set (JIS X 0201/0208, GB2312, Big5, KS X 1001)
        // except ASCII characters are treated as full-width characters.

        private static readonly byte[] WIDTH_MAP_0000_00FF = {
            // C0 Controls and Basic Latin (U+0000 - U+007F)
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0000-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0010-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0020-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0030-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0040-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0050-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0060-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0070-0F
            // C1 Controls and Latin-1 Supplement (U+0080 - U+00FF)
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //0080-8F
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //0090-9F
            1, 2, 1, 1, 2, 1, 1, 2, 2, 1, 2, 1, 1, 2, 2, 2, //00A0-AF
            2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, //00B0-BF
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, //00C0-CF
            2, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 2, 2, //00D0-DF
            1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, //00E0-EF
            2, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 2, 1, //00F0-FF
        };

        private static readonly byte[] WIDTH_MAP_0200_04FF = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0200-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0210-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0220-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0230-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0240-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0250-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0260-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0270-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0280-8F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0290-9F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //02A0-AF
            // Spacing Modifier Letters (U+02B0 - U+02FF)
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //02B0-BF
            0, 0, 0, 0, 0, 0, 0, 2, 0, 2, 2, 2, 0, 2, 0, 0, //02C0-CF
            2, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 2, 0, 0, //02D0-DF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //02E0-EF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //02F0-FF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0300-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0310-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0320-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0330-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0340-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0350-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0360-0F
            // Greek and Coptic (U+0370 - U+03FF)
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0370-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0380-8F
            0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //0390-9F
            2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, //03A0-AF
            0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //03B0-BF
            2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, //03C0-BF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //03D0-BF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //03E0-EF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //03F0-FF
            // Cyrillic (U+0400 - U+04FF)
            0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0400-0F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //0410-0F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //0420-0F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //0430-0F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //0440-0F
            0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0450-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0460-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0470-0F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0480-8F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0490-9F
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //04A0-AF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //04B0-BF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //04C0-BF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //04D0-BF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //04E0-EF
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //04F0-FF
        };

        private static readonly byte[] WIDTH_MAP_2500_25FF = {
            // Box Drawing (U+2500 - U+257F)
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //2500-0F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //2510-1F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //2520-2F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //2530-3F
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, //2540-4F
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, //2550-5F
            1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 2, 2, //2560-6F
            2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //2570-7F
            // Block Elements (U+2580 - U+259F)
            1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, //2580-8F
            1, 1, 2, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //2590-9F
            // Geometric Shapes (U+25A0 - U+25FF)
            2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, //25A0-AF
            1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 1, 1, 2, 2, 1, 1, //25B0-BF
            2, 2, 1, 1, 1, 1, 2, 2, 2, 1, 1, 2, 1, 1, 2, 2, //25C0-CF
            2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //25D0-DF
            1, 1, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, //25E0-EF
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1  //25F0-FF
        };

        /// <summary>
        /// Maps an original character code to an private character code.
        /// </summary>
        /// <param name="c">Original character code.</param>
        /// <returns>Private character code.</returns>
        public static char ToPrivate(char c) {
            byte upperByte = (byte)((ushort)c >> 8);

            switch(upperByte) {
                case 0x00:
                    switch (WIDTH_MAP_0000_00FF[c]) {
                        case 1:
                            return (char)(0xe000 + c);  // U+E0XX --> Display as a half-width character (CJK font)
                        case 2:
                            return (char)(0xe100 + c);  // U+E1XX --> Display as a full-width character (CJK font)
                        default:
                            return c;   // U+00XX --> Display as a half-width character (Latin font)
                    }

                case 0x02:
                case 0x03:
                case 0x04:
                    switch (WIDTH_MAP_0200_04FF[c - 0x0200]) {
                        case 2:
                            return (char)(c + (0xe400 - 0x0200));  // U+E4XX,U+E5XX,U+E6XX --> Display as a full-width character (CJK font)
                        default:
                            return c;   // U+02XX --> Display as a half-width character (Latin font)
                    }

                case 0x25:
                    switch (WIDTH_MAP_2500_25FF[c - 0x2500]) {
                        case 1:
                            return (char)(c + (0xe200 - 0x2500)); // U+E2XX --> Display as half-width character (CJK font)
                        case 2:
                            return (char)(c + (0xe300 - 0x2500)); // U+E3XX --> Display as full-width character (CJK font)
                        default:
                            return c;   // U+25XX --> Display as half-width character (Latin font)
                    }

                default:
                    return c;
            }
        }

        /// <summary>
        /// Reverts an private character code to an original character code.
        /// </summary>
        /// <param name="c">Character code (private or non-private)</param>
        /// <returns>Original character code</returns>
        public static char ToOriginal(char c) {
            byte upperByte = (byte)((ushort)c >> 8);

            switch(upperByte) {
                case 0xe0:  // U+E000 - U+E0FF
                    return (char)(c - 0xe000);  // U+0000 - U+00FF
                case 0xe1:  // U+E100 - U+E1FF
                    return (char)(c - 0xe100);  // U+0000 - U+00FF
                case 0xe2:  // U+E200 - U+E2FF
                    return (char)(c - (0xe200 - 0x2500));  // U+2500 - U+25FF
                case 0xe3:  // U+E300 - U+E3FF
                    return (char)(c - (0xe300 - 0x2500));  // U+2500 - U+25FF
                case 0xe4:  // U+E400 - U+E4FF
                case 0xe5:  // U+E500 - U+E5FF
                case 0xe6:  // U+E600 - U+E6FF
                    return (char)(c - (0xe400 - 0x0200));  // U+0200 - U+04FF
                default:
                    return c;
            }
        }

        /// <summary>
        /// Gets number of columns used to display a character.
        /// </summary>
        /// <param name="ch">Private character code.</param>
        /// <returns>1 (half-width character) or 2 (full-width character)</returns>
        public static int GetCharacterWidth(char ch) {
            CharGroup charGroup = GetCharGroup(ch);
            return CharGroupUtil.GetColumnsPerCharacter(charGroup);
        }

        /// <summary>
        /// Gets CharGroup of a character.
        /// </summary>
        /// <param name="ch">Private character code.</param>
        /// <returns>CharGroup value</returns>
        public static CharGroup GetCharGroup(char ch) {
            if (ch < 0x2000)
                return CharGroup.LatinHankaku;

            byte upperByte = (byte)((ushort)ch >> 8);

            switch (upperByte) {
                case 0x20:
                    if (ch == '\u2017') // for OEM850
                        return CharGroup.LatinHankaku;
                    break;
                case 0x25:  // 0x2500 <= ch <= 0x25ff (Box Drawing | Block Elements | Geometric Shapes)
                    return CharGroup.LatinHankaku;
                case 0xe0:
                case 0xe2:  // Half-width caharacters (private character code)
                    return CharGroup.CJKHankaku;
                case 0xe1:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:  // Full-width caharacters (private character code)
                    return CharGroup.CJKZenkaku;
                case 0xff:
                    if (0xFF61 <= ch && ch <= 0xFFDC) // FF61-FF64:Halfwidth CJK punctuation FF65-FF9F:Halfwidth Katakana FFA0-FFDC:Halfwidth Hangul
                        return CharGroup.CJKHankaku;
                    else if (0xFFE8 <= ch && ch <= 0xFFEE) // Halfwidth Symbol
                        return CharGroup.CJKHankaku;
                    else
                        return CharGroup.CJKZenkaku;
            }

            return CharGroup.CJKZenkaku;
        }

        /// <summary>
        /// Gets whether a character is a control character.
        /// </summary>
        /// <param name="ch">Private character code.</param>
        /// <returns>True if a character is a control character.</returns>
        public static bool IsControlCharacter(char ch) {
            return ch < 0x20 || ch == 0x7f || (0x80 <= ch && ch <= 0x9f);
        }

        /// <summary>
        /// Gets whether a character is in Unicode's private-use area.
        /// </summary>
        /// <param name="ch">A character</param>
        /// <returns>True if a character is in Unicode's private-use area.</returns>
        public static bool IsPrivateUseArea(char ch) {
            return 0xe000 <= ch && ch <= 0xf8ff;
        }
    }

}
