using System;
using Appalachia.Core.Volumes;
using UnityEngine;

namespace Appalachia.Core.PostProcessing.AutoFocus
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
