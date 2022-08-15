using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;

namespace FateGames
{
    public static class HapticManager
    {
        public static void DoHaptic(float amplitude = 0.01f, float frequency = 0.01f, float duration = 0.05f)
        {
            //HapticController.Stop();
            HapticPatterns.PlayConstant(amplitude, frequency, duration);
        }

    }
}