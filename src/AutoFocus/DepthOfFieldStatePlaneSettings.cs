using System;
using Appalachia.Editing.Attributes;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine.Rendering.PostProcessing;

namespace Appalachia.PostProcessing.AutoFocus
{
    [Serializable]
    public class DepthOfFieldStatePlaneSettings
    {
        [BoxGroup("Sampling")]
        [HorizontalGroup("Sampling/A", .5f)]
        [PropertyTooltip("Large kernel size could possible be more accurate, but is slower.")]
        [SmartLabel]
        public KernelSize kernelSize = KernelSize.Small;
        
        [HorizontalGroup("Sampling/A", .5f)]
        [PropertyTooltip(
            "Stickier auto focus is more stable (less switching back and forth as tiny grass blades cross the camera), but requires looking at a bigger uniform-ish area to switch focus to it."
        )]
        [PropertyRange(0f, 1f), SmartLabel("Votes")]
        public float voteBias = .8f;

        [HorizontalGroup("Sampling/B", .5f)]
        [PropertyTooltip(
            "Stickier auto focus is more stable (less switching back and forth as tiny grass blades cross the camera), but requires looking at a bigger uniform-ish area to switch focus to it."
        )]
        [PropertyRange(0f, .2f), SmartLabel("Size")]
        public float voteSampleKernelSize = .02f;

        [HorizontalGroup("Sampling/B", .5f)]
        [PropertyTooltip(
            "Stickier auto focus is more stable (less switching back and forth as tiny grass blades cross the camera), but requires looking at a bigger uniform-ish area to switch focus to it."
        )]
        [PropertyRange(0f, .2f), SmartLabel("Tolerance")]
        public float voteDepthTolerance = 0.02f;
        
        [HorizontalGroup("Adaptation Speed/A", .5f)]
        [PropertyRange(0.01f, 2.0f), SmartLabel("Time")]
        public float adaptationTime = .1f;
        
        [HorizontalGroup("Adaptation Speed/A", .5f)]
        [PropertyRange(0.01f, 1.0f), SmartLabel("Smoothing")]
        public float adaptationSmoothing = .2f;

        [BoxGroup("Adaptation Speed")]
        [HorizontalGroup("Adaptation Speed/B", .5f)]
        [PropertyRange(0.01f, 100.0f), SmartLabel("Towards")]
        public float adaptationMaxSpeedTowards = 4.0f;

        [HorizontalGroup("Adaptation Speed/B", .5f)]
        [PropertyRange(0.01f, 100.0f), SmartLabel("Away")]
        public float adaptationMaxSpeedAway = 4.0f;
        
        [BoxGroup("Aperture")]
        [HorizontalGroup("Aperture/Stationary", .6f)]
        [PropertyRange(0.05f, 128f), SmartLabel("Stationary")]
        public float apertureStationary = 2.0f;
        
        [HorizontalGroup("Aperture/Stationary", .4f)]
        [PropertyTooltip("Rate that aperture moves towards stationary lower bounds.")]
        [PropertyRange(0.001f, 1.0f), SmartLabel("Smoothing")]
        public float apertureStationarySmoothing = .01f;

        [BoxGroup("Aperture")]
        [PropertyTooltip(
            "Focus distance delta in meters which will not be considered a change between stationary/motion."
        )]
        [PropertyRange(0.01f, 3.0f), SmartLabel("Velocity Threshold")]
        public float apertureChangeVelocityThreshold = .1f;
        
        [HorizontalGroup("Aperture/Motion", .6f)]
        [PropertyRange(0.05f, 128f), SmartLabel("In Motion")]
        public float apertureMotion = 2.0f;

        [HorizontalGroup("Aperture/Motion", .4f)]
        [PropertyTooltip("Rate that aperture moves towards motion upper bounds.")]
        [PropertyRange(0.001f, 1.0f), SmartLabel("Smoothing")]
        public float apertureMotionSmoothing = .2f;

        /*
            this.adaptationTime
            this.adaptationSmoothing
            this.apertureChangeVelocityThreshold
            this.apertureMotion
            this.apertureMotionSmoothing
            this.apertureStationary
            this.apertureStationarySmoothing
            this.maxAdaptionSpeedAway
            this.maxAdaptionSpeedTowards
            this.samples
            this.voteBias
            this.voteDepthTolerance
            this.voteSampleKernelSize
         */

        public void Set(DepthOfFieldStatePlaneSettings a)
        {
            this.adaptationTime = a.adaptationTime;
            this.apertureChangeVelocityThreshold = a.apertureChangeVelocityThreshold;
            this.apertureMotion = a.apertureMotion;
            this.apertureMotionSmoothing = a.apertureMotionSmoothing;
            this.apertureStationary = a.apertureStationary;
            this.apertureStationarySmoothing = a.apertureStationarySmoothing;
            this.adaptationMaxSpeedAway = a.adaptationMaxSpeedAway;
            this.adaptationMaxSpeedTowards = a.adaptationMaxSpeedTowards;
            this.kernelSize = a.kernelSize;
            this.voteBias = a.voteBias;
            this.voteDepthTolerance = a.voteDepthTolerance;
            this.voteSampleKernelSize = a.voteSampleKernelSize;
        }

        public void Lerp(DepthOfFieldStatePlaneSettings a, DepthOfFieldStatePlaneSettings b, float t)
        {
            adaptationTime = math.lerp(a.adaptationTime, b.adaptationTime, t);
            adaptationSmoothing = math.lerp(a.adaptationSmoothing,                         b.adaptationSmoothing,             t);
            apertureChangeVelocityThreshold = math.lerp(a.apertureChangeVelocityThreshold, b.apertureChangeVelocityThreshold, t);
            apertureMotion = math.lerp(a.apertureMotion,                                   b.apertureMotion,                  t);
            apertureMotionSmoothing = math.lerp(a.apertureMotionSmoothing,                 b.apertureMotionSmoothing,         t);
            apertureStationary = math.lerp(a.apertureStationary,                           b.apertureStationary,              t);
            apertureStationarySmoothing = math.lerp(a.apertureStationarySmoothing,         b.apertureStationarySmoothing,     t);
            adaptationMaxSpeedAway = math.lerp(a.adaptationMaxSpeedAway,                       b.adaptationMaxSpeedAway,            t);
            adaptationMaxSpeedTowards = math.lerp(a.adaptationMaxSpeedTowards,                 b.adaptationMaxSpeedTowards,         t);
            kernelSize = b.kernelSize;
            voteBias = math.lerp(a.voteBias, b.voteBias, t);
            voteDepthTolerance = math.lerp(a.voteDepthTolerance,     b.voteDepthTolerance,   t);
            voteSampleKernelSize = math.lerp(a.voteSampleKernelSize, b.voteSampleKernelSize, t);
        }
    }
}
