/*
using System;
using Appalachia.Core.Volumes;
using Appalachia.Core.Volumes.PropertyMaster;
using UnityEngine;

namespace Appalachia.Core.PostProcessing.AutoFocus
{

    [Serializable]
    [CreateAssetMenu(menuName = "Internal/Runtime/PostProcessing/AutoFocus", order = 0)]
    public class AutoFocusProperties<T> : PropertyVolumeComponent<T>
        where T : PropertyVolumeComponent<T>
    {
        public ExposedAutoFocusReferenceParameter target =
            new ExposedAutoFocusReferenceParameter(default);

        public ClampedFloatParameter influence = new ClampedFloatParameter(1f, 0f, 1f);

        public ClampedFloatParameter standingStickiness = new ClampedFloatParameter(0.9f, 0f, 1f);
        public ClampedFloatParameter standingDefaultFocusDistance = new ClampedFloatParameter(5.0f, 0.1f, 20.0f);
        public ClampedFloatParameter standingApertureStationary = new ClampedFloatParameter(1.0f, 0.05f, 32f);
        public ClampedFloatParameter standingApertureMotion = new ClampedFloatParameter(3.0f, 0.05f, 32f);
        public ClampedFloatParameter standingAdaptationTime = new ClampedFloatParameter(.75f, 0.01f, 3f);
        public ClampedFloatParameter standingAdaptationSmoothing = new ClampedFloatParameter(.2f, 0.01f, 1f);
        public ClampedFloatParameter standingMaxAdaptionSpeedTowards = new ClampedFloatParameter(1.0f, 0.01f, 100.0f);
        public ClampedFloatParameter standingMaxAdaptionSpeedAway = new ClampedFloatParameter(1.0f, 0.01f, 100.0f);
        public ClampedFloatParameter standingMaxFocusDistance = new ClampedFloatParameter(100.0f, 20.0f, 100.0f);
        public ClampedFloatParameter standingVelocitySmoothing = new ClampedFloatParameter(.3f, 0.01f, 1.0f);

        public ClampedFloatParameter walkingStickiness = new ClampedFloatParameter(0.5f, 0f, 1f);
        public ClampedFloatParameter walkingDefaultFocusDistance = new ClampedFloatParameter(7f, 0.1f, 20.0f);
        public ClampedFloatParameter walkingApertureStationary = new ClampedFloatParameter(4.0f, 0.05f, 32f);
        public ClampedFloatParameter walkingApertureMotion = new ClampedFloatParameter(6.0f, 0.05f, 32f);
        public ClampedFloatParameter walkingVelocityThreshold = new ClampedFloatParameter(0.1f, 0.1f, 6f);
        public ClampedFloatParameter walkingAdaptationTime = new ClampedFloatParameter(.4f, 0.01f, 3f);
        public ClampedFloatParameter walkingAdaptationSmoothing = new ClampedFloatParameter(.2f, 0.01f, 1f);
        public ClampedFloatParameter walkingMaxAdaptionSpeedTowards = new ClampedFloatParameter(2.0f, 0.01f, 100.0f);
        public ClampedFloatParameter walkingMaxAdaptionSpeedAway = new ClampedFloatParameter(2.0f, 0.01f, 100.0f);
        public ClampedFloatParameter walkingMaxFocusDistance = new ClampedFloatParameter(75.0f, 20.0f, 100.0f);
        public ClampedFloatParameter walkingVelocitySmoothing = new ClampedFloatParameter(.2f, 0.01f, 1.0f);

        public ClampedFloatParameter runningStickiness = new ClampedFloatParameter(0.2f, 0f, 1f);
        public ClampedFloatParameter runningDefaultFocusDistance = new ClampedFloatParameter(10f, 0.1f, 20.0f);
        public ClampedFloatParameter runningApertureStationary = new ClampedFloatParameter(20.0f, 0.05f, 32f);
        public ClampedFloatParameter runningApertureMotion = new ClampedFloatParameter(20.0f, 0.05f, 32f);
        public ClampedFloatParameter runningVelocityThreshold = new ClampedFloatParameter(2.5f, 0.1f, 6f);
        public ClampedFloatParameter runningAdaptationTime = new ClampedFloatParameter(.4f, 0.2f, 3f);
        public ClampedFloatParameter runningAdaptationSmoothing = new ClampedFloatParameter(.2f, 0.01f, 1f);
        public ClampedFloatParameter runningMaxAdaptionSpeedTowards = new ClampedFloatParameter(4.0f, 0.01f, 100.0f);
        public ClampedFloatParameter runningMaxAdaptionSpeedAway = new ClampedFloatParameter(4.0f, 0.01f, 100.0f);
        public ClampedFloatParameter runningMaxFocusDistance = new ClampedFloatParameter(50.0f, 20.0f, 100.0f);
        public ClampedFloatParameter runningVelocitySmoothing = new ClampedFloatParameter(.1f, 0.01f, 1.0f);

        public override void OverrideProperties(PropertyMaster master)
        {
            var autoFocus = target.value.Resolve(master);

            if (!autoFocus)
            {
                return;
            }

            var infl = autoFocus.influence;

            Override(influence, ref infl);
            autoFocus.influence = infl;


            Override(standingStickiness, ref autoFocus.standing.stickiness);
            Override(standingDefaultFocusDistance, ref autoFocus.standing.defaultFocusDistance);
            Override(standingApertureStationary, ref autoFocus.standing.apertureStationary);
            Override(standingApertureMotion, ref autoFocus.standing.apertureMotion);
            Override(standingAdaptationTime, ref autoFocus.standing.adaptationTime);
            Override(standingAdaptationSmoothing, ref autoFocus.standing.adaptationSmoothing);
            Override(standingMaxAdaptionSpeedTowards, ref autoFocus.standing.maxAdaptionSpeedTowards);
            Override(standingMaxAdaptionSpeedAway, ref autoFocus.standing.maxAdaptionSpeedAway);
            Override(standingMaxFocusDistance, ref autoFocus.standing.maxFocusDistance);
            Override(standingVelocitySmoothing, ref autoFocus.standing.velocitySmoothing);

            Override(walkingStickiness, ref autoFocus.walking.stickiness);
            Override(walkingDefaultFocusDistance, ref autoFocus.walking.defaultFocusDistance);
            Override(walkingApertureStationary, ref autoFocus.walking.apertureStationary);
            Override(walkingApertureMotion, ref autoFocus.walking.apertureMotion);
            Override(walkingVelocityThreshold, ref autoFocus.walking.velocityThreshold);
            Override(walkingAdaptationTime, ref autoFocus.walking.adaptationTime);
            Override(walkingAdaptationSmoothing, ref autoFocus.walking.adaptationSmoothing);
            Override(walkingMaxAdaptionSpeedTowards, ref autoFocus.walking.maxAdaptionSpeedTowards);
            Override(walkingMaxAdaptionSpeedAway, ref autoFocus.walking.maxAdaptionSpeedAway);
            Override(walkingMaxFocusDistance, ref autoFocus.walking.maxFocusDistance);
            Override(walkingVelocitySmoothing, ref autoFocus.walking.velocitySmoothing);

            Override(runningStickiness, ref autoFocus.running.stickiness);
            Override(runningDefaultFocusDistance, ref autoFocus.running.defaultFocusDistance);
            Override(runningApertureStationary, ref autoFocus.running.apertureStationary);
            Override(runningApertureMotion, ref autoFocus.running.apertureMotion);
            Override(runningVelocityThreshold, ref autoFocus.running.velocityThreshold);
            Override(runningAdaptationTime, ref autoFocus.running.adaptationTime);
            Override(runningAdaptationSmoothing, ref autoFocus.running.adaptationSmoothing);
            Override(runningMaxAdaptionSpeedTowards, ref autoFocus.running.maxAdaptionSpeedTowards);
            Override(runningMaxAdaptionSpeedAway, ref autoFocus.running.maxAdaptionSpeedAway);
            Override(runningMaxFocusDistance, ref autoFocus.running.maxFocusDistance);
            Override(runningVelocitySmoothing, ref autoFocus.running.velocitySmoothing);
        }
    }
}
*/


