using System;

namespace Audiox.Runtime.Tools
{
    public static class ExtraMethods
    {
        public static void Swap<T>(ref T left, ref T right)
        {
            (left, right) = (right, left);
        }

        public static string FormatTimeSpan(TimeSpan newTime, bool formatMilliseconds = false)
        {
            string formattedTime;
            if (newTime.Hours > 0)
            {
                if (formatMilliseconds)
                {
                    formattedTime = string.Format("{0}:{1:D2}:{2:D2}", newTime.Hours, newTime.Minutes, newTime.Seconds);
                }
                else
                {
                    formattedTime = string.Format("{0}:{1:D2}:{2:D2}:{3:D2}", newTime.Hours, newTime.Minutes, newTime.Seconds, newTime.Milliseconds);
                }
            }
            else
            {
                if (formatMilliseconds)
                {
                    formattedTime = string.Format("{0}:{1:D2}:{2:D2}", newTime.Minutes, newTime.Seconds, newTime.Milliseconds);
                }
                else
                {
                    formattedTime = string.Format("{0}:{1:D2}", newTime.Minutes, newTime.Seconds);
                }
            }
            return formattedTime;
        }
    }
}