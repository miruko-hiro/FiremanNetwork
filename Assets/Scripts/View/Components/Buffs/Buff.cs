using System;
using UnityEngine;

namespace View.Components.Buffs
{
    public abstract class Buff: MonoBehaviour
    {
        public abstract event Action<Buff, IBuffTarget> OnTargetPickedUpBuffEvent;
        public abstract float TimeOfAction { get; }
    }
}