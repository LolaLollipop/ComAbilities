﻿using Exiled.API.Interfaces;

namespace RueI
{
    /// <summary>
    /// Provides helpful functions for dealing with elements and hints.
    /// </summary>
    public static class Ruetility
    {
        /// <summary>
        /// Cleans a string by wrapping it in noparses, and removes any noparse closer tags existing in it already.
        /// </summary>
        /// <param name="text">The string to clean.</param>
        /// <returns>The cleaned string.</returns>
        public static string GetCleanText(string text)
        {
            string cleanText = text.Replace("</noparse>", "</nopa​rse>"); // zero width space is inserted
            return $"<noparse>{cleanText}</noparse>";
        }

        /// <summary>
        /// Converts a scaled position from 0-1000 into functional pixels (offset from baseline).
        /// </summary>
        /// <returns>The converted value.</returns>
        public static float ScaledPositionToFunctional(float position) => (position * -2.14f) + 755f;

        /// <summary>
        /// Converts a functional position into a scaled position.
        /// </summary>
        /// <returns>The converted value.</returns>
        public static float FunctionalToScaledPosition(float position) => (position - 755f) * 2.14f;
    }
}
