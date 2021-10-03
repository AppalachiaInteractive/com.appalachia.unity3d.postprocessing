using System;
using Appalachia.Core.Editing.Attributes;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Appalachia.Core.PostProcessing.AutoFocus
{
    [Serializable]
    public class DepthOfFieldActiveSettingsManager
    {
        private const string _PRF_PFX = nameof(DepthOfFieldActiveSettingsManager) + ".";
        
        public DepthOfFieldActiveSettingsManager(DepthOfFieldStateSettingCollection settingsCollection)
        {
            this.settingsCollection = settingsCollection;

            current = UnityEngine.Object.Instantiate(BlendSettingA);
            
            current.Set(settingsCollection.settings[0]);
        }

        [NonSerialized] private DepthOfField _depthOfField;
        [NonSerialized] private ComputeBuffer _autoFocusParamsBuffer;
        [NonSerialized] private ComputeBuffer _autoFocusOutputBuffer;
        [NonSerialized] private bool _resetHistory = true;
        [NonSerialized] private float[] _displayBuffer = new float[4];
        
        [FoldoutGroup("Current", Order = -9), NonSerialized, ShowInInspector, SmartLabel(Postfix = true)]
        public bool retrieveDisplayData;

        [FoldoutGroup("Current", Order = -8), ShowInInspector, SmartLabel, ReadOnly, ShowIf(nameof(retrieveDisplayData))]
        private float currentFocusDistance
        {
            get
            {
                if (_displayBuffer == null) _displayBuffer = new float[4];
                return _displayBuffer[0];
            }
        }

        [FoldoutGroup("Current", Order = -7), ShowInInspector, SmartLabel, ReadOnly, ShowIf(nameof(retrieveDisplayData))]
        private float currentFocusVelocity
        {
            get
            {
                if (_displayBuffer == null) _displayBuffer = new float[4];
                return _displayBuffer[1];
            }
        }

        [FoldoutGroup("Current", Order = -6), ShowInInspector, SmartLabel, ReadOnly, ShowIf(nameof(retrieveDisplayData))]
        private float targetFocusDistance
        {
            get
            {
                if (_displayBuffer == null) _displayBuffer = new float[4];
                return _displayBuffer[2];
            }
        }

        [FoldoutGroup("Current", Order = -5), ShowInInspector, SmartLabel, ReadOnly, ShowIf(nameof(retrieveDisplayData))]
        private float currentAperture
        {
            get
            {
                if (_displayBuffer == null) _displayBuffer = new float[4];
                return _displayBuffer[3];
            }
        }

        [BoxGroup("Influence"), PropertyRange(0f, 1f), SmartLabel]
        public float influence = 1.0f;
        
        [BoxGroup("History")]
        [HorizontalGroup("History/A", .35f)]
        [NonSerialized, ShowInInspector, ReadOnly, SmartLabel("Velocity")]
        private float _currentSmoothedPlayerVelocity;
        
        [HorizontalGroup("History/A", .65f)]
        [NonSerialized, ShowInInspector, ReadOnly, SmartLabel("Position")]
        private Vector3 _lastPosition;

        [HorizontalGroup("Target Lock/A", .05f)]
        [HideLabel, LabelWidth(0)]
        public bool targetLockEnabled;
                
        [BoxGroup("Target Lock")]
        [HorizontalGroup("Target Lock/A", .5f)]
        [SmartLabel("Target")]
        [EnableIf(nameof(targetLockEnabled))]
        public GameObject lockedTarget;
        
        [HorizontalGroup("Target Lock/A", .4f)]
        [NonSerialized, ShowInInspector, PropertyRange(0f, 1f), SmartLabel("Weight")]
        [EnableIf(nameof(targetLockEnabled))]
        private float targetLockWeight = 1.0f;

        private bool _distanceFieldsEnabled => distanceOverrideEnabled && !targetLockEnabled;
        
        [HorizontalGroup("Distance Override/A", .05f)]
        [HideLabel, LabelWidth(0)]
        [DisableIf(nameof(targetLockEnabled))]
        public bool distanceOverrideEnabled;
        
        [BoxGroup("Distance Override")]
        [HorizontalGroup("Distance Override/A", .5f)]
        [NonSerialized, ShowInInspector, PropertyRange(0f, 100f), SmartLabel("Distance")] 
        [EnableIf(nameof(_distanceFieldsEnabled))]
        private float distanceOverride;
        
        [HorizontalGroup("Distance Override/A", .4f)]
        [NonSerialized, ShowInInspector, PropertyRange(0f, 1f), SmartLabel("Weight")]
        [EnableIf(nameof(_distanceFieldsEnabled))]
        private float distanceOverrideWeight;
        
        [FoldoutGroup("Current Settings")]
        [InlineProperty, HideLabel]
        public DepthOfFieldStateSettings current;
        
        [HideInInspector]
        public DepthOfFieldStateSettingCollection settingsCollection;

        private bool PlayerVelocityUnderWalkThreshold
        {
            get
            {
                var setting = settingsCollection.settings[1];
                return _currentSmoothedPlayerVelocity < setting.velocityThreshold;
            }
        }

        private bool PlayerVelocityUnderWalkFadeThreshold
        {
            get
            {
                var setting = settingsCollection.settings[1];
                return _currentSmoothedPlayerVelocity < setting.velocityThreshold + setting.velocityFadeRange;
            }
        }

        private bool PlayerVelocityUnderRunThreshold
        {
            get
            {
                var setting = settingsCollection.settings[2];
                return _currentSmoothedPlayerVelocity < setting.velocityThreshold;
            }
        }

        private bool PlayerVelocityUnderRunFadeThreshold
        {
            get
            {
                var setting = settingsCollection.settings[2];
                return _currentSmoothedPlayerVelocity < setting.velocityThreshold + setting.velocityFadeRange;
            }
        }

        private DepthOfFieldStateSettings BlendSettingA => PlayerVelocityUnderWalkFadeThreshold
            ? settingsCollection.settings[0]
            : PlayerVelocityUnderRunFadeThreshold
                ? settingsCollection.settings[1]
                : settingsCollection.settings[2];
        
        private DepthOfFieldStateSettings BlendSettingB => PlayerVelocityUnderWalkThreshold
            ? settingsCollection.settings[0]
            : PlayerVelocityUnderRunThreshold
                ? settingsCollection.settings[1]
                : settingsCollection.settings[2];

        private static readonly ProfilerMarker _PRF_Update = new ProfilerMarker(_PRF_PFX + nameof(Update));
        public void Update(
            bool enabled,
            ComputeShader computeShader,
            CommandBuffer commandBuffer,
            float focalLength /*in meters*/,
            float filmHeight,
            Camera cam,
            bool resetHistory)
        {
            using (_PRF_Update.Auto())
            {
                if (!enabled)
                {
                    commandBuffer.DisableShaderKeyword(Uniforms.AutoFocusKeyword);
                    _resetHistory = true;
                    return;
                }

                if (resetHistory)
                {
                    _resetHistory = true;
                }

                commandBuffer.BeginSample("AutoFocus");
                commandBuffer.EnableShaderKeyword(Uniforms.AutoFocusKeyword);

                var deltaTime = Time.deltaTime;
            
                var position = cam.transform.position;
                var distance = math.distance(position, _lastPosition);

                var velocity = distance / deltaTime;

                _lastPosition = position;

                var limit = 10f;
                velocity = math.clamp(velocity, -limit, limit);

                if (float.IsNaN(_currentSmoothedPlayerVelocity))
                {
                    _currentSmoothedPlayerVelocity = 0.0f;
                }

                var step = math.clamp(settingsCollection.velocitySmoothing * (deltaTime / Time.fixedDeltaTime), 0f, 1f);
                _currentSmoothedPlayerVelocity = math.lerp(_currentSmoothedPlayerVelocity, velocity, step);
            
                var settingA = BlendSettingA;
                var settingB = BlendSettingB;

                if (current == null)
                {
                    current = UnityEngine.Object.Instantiate(BlendSettingA);
                }

                if (settingA == settingB)
                {
                    current.Set(settingA);
                }
                else
                {
                    var numerator = _currentSmoothedPlayerVelocity - settingB.velocityThreshold;
                    var denominator = settingB.velocityFadeRange;
            
                    var blendTime = math.clamp(numerator / denominator, 0.0f, 1.0f);

                    current.Lerp(settingA, settingB, blendTime);
                }
                
                InitializeBuffers();

                if (_depthOfField == null)
                {
                    var layer = cam.GetComponent<PostProcessLayer>();
                    
                    _depthOfField = layer.GetBundle<DepthOfField>().settings as DepthOfField;
                }

                ApplyToCommandBuffer(computeShader, commandBuffer, focalLength, filmHeight);
                
                commandBuffer.EndSample("AutoFocus");
            }
        }

        private static readonly ProfilerMarker _PRF_ReleaseBuffers = new ProfilerMarker(_PRF_PFX + nameof(ReleaseBuffers));
        public void ReleaseBuffers()
        {
            using (_PRF_ReleaseBuffers.Auto())
            {
                _autoFocusParamsBuffer?.Release();
                _autoFocusParamsBuffer = null;

                _autoFocusOutputBuffer?.Release();
                _autoFocusOutputBuffer = null;
            }
        }

        private static readonly ProfilerMarker _PRF_Reset = new ProfilerMarker(_PRF_PFX + nameof(Reset));
        public void Reset()
        {
            using (_PRF_Reset.Auto())
            {
                _resetHistory = true;
                _currentSmoothedPlayerVelocity = 0.0f;
                _lastPosition = Vector3.zero;
                lockedTarget = null;
                targetLockWeight = 1.0f;
                targetLockEnabled = false;
                distanceOverride = 25.0f;
                distanceOverrideWeight = 0.0f;
            }
        }

        public void RetrieveDisplayData()
        {
            if (retrieveDisplayData)
            {
                _autoFocusParamsBuffer?.GetData(_displayBuffer);
            }
        }


        private static readonly ProfilerMarker _PRF_InitializeBuffers = new ProfilerMarker(_PRF_PFX + nameof(InitializeBuffers));
        private void InitializeBuffers()
        {
            using (_PRF_InitializeBuffers.Auto())
            {
                if (_autoFocusParamsBuffer == null)
                {
                    _autoFocusParamsBuffer = new ComputeBuffer(1, 16);
                    _resetHistory = true;
                }

                if (_autoFocusOutputBuffer == null)
                {
                    _autoFocusOutputBuffer = new ComputeBuffer(1, 8);
                }

                if (_resetHistory)
                {
                    // Init the buffer to have a sensible starting point and no blinking
                    float cv = 0;
                    _autoFocusParamsBuffer.SetData(new[] {settingsCollection.focusDistance, cv, settingsCollection.focusDistance, settingsCollection.aperture});
                    _resetHistory = false;
                }
            }
        }

        private static readonly ProfilerMarker _PRF_ApplyToCommandBuffer = new ProfilerMarker(_PRF_PFX + nameof(ApplyToCommandBuffer));
        public void ApplyToCommandBuffer(ComputeShader computeShader, CommandBuffer commandBuffer, float focalLength, float filmHeight)
        {
            using (_PRF_ApplyToCommandBuffer.Auto())
            {
                var explicitDistance = 0f;
                var explicitDistanceWeight = 0.0f;
                
                if (targetLockEnabled && (lockedTarget != null))
                {
                    explicitDistance = math.distance(lockedTarget.transform.position, _lastPosition);
                    explicitDistanceWeight = targetLockWeight;
                }
                else if (distanceOverrideEnabled)
                {
                    explicitDistance = distanceOverride;
                    explicitDistanceWeight = distanceOverrideWeight;
                }

                var nearPlane = current.nearPlane;
                var farPlane = current.farPlane;

                if (nearPlane.kernelSize == KernelSize.VeryLarge || farPlane.kernelSize == KernelSize.VeryLarge)
                {
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_SMALL);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_MEDIUM);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_LARGE);
                    commandBuffer.EnableShaderKeyword(Uniforms.AUTODOF_KERNEL_VERYLARGE);
                }
                else if (nearPlane.kernelSize == KernelSize.Large || farPlane.kernelSize == KernelSize.Large)
                {
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_SMALL);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_MEDIUM);
                    commandBuffer.EnableShaderKeyword(Uniforms.AUTODOF_KERNEL_LARGE);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_VERYLARGE);
                }
                else if (nearPlane.kernelSize == KernelSize.Medium || farPlane.kernelSize == KernelSize.Medium)
                {
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_SMALL);
                    commandBuffer.EnableShaderKeyword(Uniforms.AUTODOF_KERNEL_MEDIUM);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_LARGE);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_VERYLARGE);
                }
                else if (nearPlane.kernelSize == KernelSize.Small || farPlane.kernelSize == KernelSize.Small)
                {
                    commandBuffer.EnableShaderKeyword(Uniforms.AUTODOF_KERNEL_SMALL);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_MEDIUM);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_LARGE);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_VERYLARGE);
                }
                else
                {                    
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_SMALL);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_MEDIUM);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_LARGE);
                    commandBuffer.DisableShaderKeyword(Uniforms.AUTODOF_KERNEL_VERYLARGE);
                }
                
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._Influence, influence);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FocalLength, focalLength);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FilmHeight, filmHeight);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._DeltaTime, Time.deltaTime);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FixedDeltaTime, Time.fixedDeltaTime);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._DefaultFocusDistance, current.defaultFocusDistance);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._MaxFocusDistance, current.maxFocusDistance);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._DistanceOverride, explicitDistance);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._DistanceOverrideWeight, explicitDistanceWeight);
                
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_VoteBias, nearPlane.voteBias);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_VoteSampleKernelSize, nearPlane.voteSampleKernelSize);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_VoteDepthTolerance, nearPlane.voteDepthTolerance);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_AdaptationTime, nearPlane.adaptationTime);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_AdaptationMaxSpeedTowards, nearPlane.adaptationMaxSpeedTowards);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_AdaptationMaxSpeedAway, nearPlane.adaptationMaxSpeedAway);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_AdaptationSmoothing, nearPlane.adaptationSmoothing);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_ApertureStationary, nearPlane.apertureStationary);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_ApertureStationarySmoothing, nearPlane.apertureStationarySmoothing);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_ApertureChangeVelocityThreshold, nearPlane.apertureChangeVelocityThreshold);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_ApertureMotion, nearPlane.apertureMotion);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._NearPlane_ApertureMotionSmoothing, nearPlane.apertureMotionSmoothing);
                
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_VoteBias, farPlane.voteBias);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_VoteSampleKernelSize, farPlane.voteSampleKernelSize);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_VoteDepthTolerance, farPlane.voteDepthTolerance);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_AdaptationTime, farPlane.adaptationTime);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_AdaptationMaxSpeedTowards, farPlane.adaptationMaxSpeedTowards);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_AdaptationMaxSpeedAway, farPlane.adaptationMaxSpeedAway);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_AdaptationSmoothing, farPlane.adaptationSmoothing);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_ApertureStationary, farPlane.apertureStationary);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_ApertureStationarySmoothing, farPlane.apertureStationarySmoothing);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_ApertureChangeVelocityThreshold, farPlane.apertureChangeVelocityThreshold);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_ApertureMotion, farPlane.apertureMotion);
                commandBuffer.SetComputeFloatParam(computeShader, Uniforms._FarPlane_ApertureMotionSmoothing, farPlane.apertureMotionSmoothing);
                
                commandBuffer.SetComputeBufferParam(computeShader, 0, Uniforms._AutoFocusParams, _autoFocusParamsBuffer);
                commandBuffer.SetComputeBufferParam(computeShader, 0, Uniforms._AutoFocusOutput, _autoFocusOutputBuffer);
                
                commandBuffer.DispatchCompute(computeShader, 0, 1, 1, 1);

                commandBuffer.SetGlobalBuffer(Uniforms._AutoFocusOutput, _autoFocusOutputBuffer);
            }
        }
        
        private static class Uniforms
        {
            internal const string _AutoFocusParams = "_AutoFocusParams";
            internal const string _AutoFocusOutput = "_AutoFocusOutput";
            internal const string AutoFocusKeyword = "AUTO_FOCUS";
            
            internal const string AUTODOF_KERNEL_SMALL = "AUTODOF_KERNEL_SMALL";
            internal const string AUTODOF_KERNEL_MEDIUM = "AUTODOF_KERNEL_MEDIUM";
            internal const string AUTODOF_KERNEL_LARGE = "AUTODOF_KERNEL_LARGE";
            internal const string AUTODOF_KERNEL_VERYLARGE = "AUTODOF_KERNEL_VERYLARGE";
            
            internal static readonly int _Influence = Shader.PropertyToID("_Influence");
            internal static readonly int _FocalLength = Shader.PropertyToID("_FocalLength");
            internal static readonly int _FilmHeight = Shader.PropertyToID("_FilmHeight");
            internal static readonly int _DeltaTime = Shader.PropertyToID("_DeltaTime");
            internal static readonly int _FixedDeltaTime = Shader.PropertyToID("_FixedDeltaTime");
            internal static readonly int _DefaultFocusDistance = Shader.PropertyToID("_DefaultFocusDistance");
            internal static readonly int _MaxFocusDistance = Shader.PropertyToID("_MaxFocusDistance");
            internal static readonly int _DistanceOverride = Shader.PropertyToID("_DistanceOverride");
            internal static readonly int _DistanceOverrideWeight = Shader.PropertyToID("_DistanceOverrideWeight");
            
            internal static readonly int _NearPlane_VoteBias = Shader.PropertyToID("_NearPlane_VoteBias");
            internal static readonly int _NearPlane_VoteSampleKernelSize = Shader.PropertyToID("_NearPlane_VoteSampleKernelSize");
            internal static readonly int _NearPlane_VoteDepthTolerance = Shader.PropertyToID("_NearPlane_VoteDepthTolerance");
            internal static readonly int _NearPlane_AdaptationTime = Shader.PropertyToID("_NearPlane_AdaptationTime");
            internal static readonly int _NearPlane_AdaptationMaxSpeedTowards = Shader.PropertyToID("_NearPlane_AdaptationMaxSpeedTowards");
            internal static readonly int _NearPlane_AdaptationMaxSpeedAway = Shader.PropertyToID("_NearPlane_AdaptationMaxSpeedAway");
            internal static readonly int _NearPlane_AdaptationSmoothing = Shader.PropertyToID("_NearPlane_AdaptationSmoothing");
            internal static readonly int _NearPlane_ApertureStationary = Shader.PropertyToID("_NearPlane_ApertureStationary");
            internal static readonly int _NearPlane_ApertureStationarySmoothing = Shader.PropertyToID("_NearPlane_ApertureStationarySmoothing");
            internal static readonly int _NearPlane_ApertureChangeVelocityThreshold = Shader.PropertyToID("_NearPlane_ApertureChangeVelocityThreshold");
            internal static readonly int _NearPlane_ApertureMotion = Shader.PropertyToID("_NearPlane_ApertureMotion");
            internal static readonly int _NearPlane_ApertureMotionSmoothing = Shader.PropertyToID("_NearPlane_ApertureMotionSmoothing");
            
            internal static readonly int _FarPlane_VoteBias = Shader.PropertyToID("_FarPlane_VoteBias");
            internal static readonly int _FarPlane_VoteSampleKernelSize = Shader.PropertyToID("_FarPlane_VoteSampleKernelSize");
            internal static readonly int _FarPlane_VoteDepthTolerance = Shader.PropertyToID("_FarPlane_VoteDepthTolerance");
            internal static readonly int _FarPlane_AdaptationTime = Shader.PropertyToID("_FarPlane_AdaptationTime");
            internal static readonly int _FarPlane_AdaptationMaxSpeedTowards = Shader.PropertyToID("_FarPlane_AdaptationMaxSpeedTowards");
            internal static readonly int _FarPlane_AdaptationMaxSpeedAway = Shader.PropertyToID("_FarPlane_AdaptationMaxSpeedAway");
            internal static readonly int _FarPlane_AdaptationSmoothing = Shader.PropertyToID("_FarPlane_AdaptationSmoothing");
            internal static readonly int _FarPlane_ApertureStationary = Shader.PropertyToID("_FarPlane_ApertureStationary");
            internal static readonly int _FarPlane_ApertureStationarySmoothing = Shader.PropertyToID("_FarPlane_ApertureStationarySmoothing");
            internal static readonly int _FarPlane_ApertureChangeVelocityThreshold = Shader.PropertyToID("_FarPlane_ApertureChangeVelocityThreshold");
            internal static readonly int _FarPlane_ApertureMotion = Shader.PropertyToID("_FarPlane_ApertureMotion");
            internal static readonly int _FarPlane_ApertureMotionSmoothing = Shader.PropertyToID("_FarPlane_ApertureMotionSmoothing");
        }
    }
}
