using System;
using Appalachia.Core.Volumes.Parameters;
using UnityEngine;

namespace Appalachia.PostProcessing.AutoFocus
{
    [Serializable]
    public sealed class ExposedAutoFocusReferenceParameter : VolumeParameter<ExposedReference<DepthOfFieldAutoFocus>>
    {
        public ExposedAutoFocusReferenceParameter(
            ExposedReference<DepthOfFieldAutoFocus> value,
            bool overrideState = false) : base(value, overrideState)
        {
        }
    }
}
