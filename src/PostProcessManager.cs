using System.Linq;
using Appalachia.Utility.Constants;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

#if UNITY_EDITOR

#endif

namespace Appalachia.PostProcessing
{
    public static class PostProcessManager
    {
        private static PostProcessLayer _layer;
        private static PostProcessDebug _debug;
        private static PostProcessTimeOfDay _timeOfDay;

        public static bool lightMeter
        {
            get => (_debug != null) && _debug.lightMeter;
            set
            {
                if (_debug != null)
                {
                    _debug.lightMeter = value;
                }
            }
        }

        public static bool histogram
        {
            get => (_debug != null) && _debug.histogram;
            set
            {
                if (_debug != null)
                {
                    _debug.histogram = value;
                }
            }
        }

        public static bool waveform
        {
            get => (_debug != null) && _debug.waveform;
            set
            {
                if (_debug != null)
                {
                    _debug.waveform = value;
                }
            }
        }

        public static bool vectorscope
        {
            get => (_debug != null) && _debug.vectorscope;
            set
            {
                if (_debug != null)
                {
                    _debug.vectorscope = value;
                }
            }
        }

        public static DebugOverlay debugOverlay
        {
            get => _debug == null ? DebugOverlay.None : _debug.debugOverlay;
            set
            {
                if (_debug != null)
                {
                    _debug.debugOverlay = value;
                }
            }
        }

        private static void Initialize()
        {
            if (_layer == null)
            {
                _layer = Camera.main.GetComponent<PostProcessLayer>();
            }

            if (_layer == null)
            {
                return;
            }

            if (_debug == null)
            {
                _debug = Object.FindObjectsOfType<PostProcessDebug>()
                               .FirstOrDefault(ppd => ppd.postProcessLayer == _layer);
            }

            if (_debug == null)
            {
                return;
            }

            if (_timeOfDay == null)
            {
                _timeOfDay = Object.FindObjectOfType<PostProcessTimeOfDay>();
            }

            if (_timeOfDay == null)
            {
            }
        }

        public static void SetMonitorHeight(int height)
        {
            Initialize();

            _layer.debugLayer.histogram.height = height;
            _layer.debugLayer.waveform.height = height;
            _layer.debugLayer.vectorscope.size = height;
            _layer.debugLayer.lightMeter.height = height;
        }

        public static void SetMonitorWidth(int width)
        {
            Initialize();

            _layer.debugLayer.histogram.width = width;
            _layer.debugLayer.lightMeter.width = width;
        }
#if UNITY_EDITOR
        /*[MenuItem("Tools/Game View Auto Update"+SHC.CTRL_ALT_SHFT_G, true)]
        public static bool ToggleGameViewAutoUpdateValidate()
        {
            var gameView = EditorWindow.
            Menu.SetChecked("Tools/Enviro/Time/Paused", GameView.GetWindow<GameView>());
            return true;
        }

        [MenuItem("Tools/Game View Auto Update"+SHC.CTRL_ALT_SHFT_G, priority = 1050)]
        public static void ToggleGameViewAutoUpdate()
        {
            SetPaused(!_paused);
        }*/

        [MenuItem(
            "Tools/Post Processing/Debug Overlays/None" + SHC.CTRL_ALT_SHFT_F1,
            priority = 40
        )]
        public static void PostProcessingOverlay_None()
        {
            debugOverlay = DebugOverlay.None;
        }

        [MenuItem(
            "Tools/Post Processing/Debug Overlays/Depth" + SHC.CTRL_ALT_SHFT_F2,
            priority = 41
        )]
        public static void PostProcessingOverlay_Depth()
        {
            debugOverlay = DebugOverlay.Depth;
        }

        [MenuItem(
            "Tools/Post Processing/Debug Overlays/Normals" + SHC.CTRL_ALT_SHFT_F3,
            priority = 42
        )]
        public static void PostProcessingOverlay_Normals()
        {
            debugOverlay = DebugOverlay.Normals;
        }

        [MenuItem(
            "Tools/Post Processing/Debug Overlays/Motion Vectors" + SHC.CTRL_ALT_SHFT_F4,
            priority = 43
        )]
        public static void PostProcessingOverlay_MotionVectors()
        {
            debugOverlay = DebugOverlay.MotionVectors;
        }

        [MenuItem("Tools/Post Processing/Debug Overlays/NAN Tracker", priority = 44)]
        public static void PostProcessingOverlay_NANTracker()
        {
            debugOverlay = DebugOverlay.NANTracker;
        }

        [MenuItem("Tools/Post Processing/Debug Overlays/Color Blindness Simulation", priority = 45)]
        public static void PostProcessingOverlay_ColorBlindnessSimulation()
        {
            debugOverlay = DebugOverlay.ColorBlindnessSimulation;
        }

        [MenuItem(
            "Tools/Post Processing/Debug Overlays/Ambient Occlusion" + SHC.CTRL_ALT_SHFT_F5,
            priority = 46
        )]
        public static void PostProcessingOverlay_AmbientOcclusion()
        {
            debugOverlay = DebugOverlay.AmbientOcclusion;
        }

        [MenuItem(
            "Tools/Post Processing/Debug Overlays/Bloom Buffer" + SHC.CTRL_ALT_SHFT_F6,
            priority = 47
        )]
        public static void PostProcessingOverlay_BloomBuffer()
        {
            debugOverlay = DebugOverlay.BloomBuffer;
        }

        [MenuItem(
            "Tools/Post Processing/Debug Overlays/Bloom Threshold" + SHC.CTRL_ALT_SHFT_F7,
            priority = 48
        )]
        public static void PostProcessingOverlay_BloomThreshold()
        {
            debugOverlay = DebugOverlay.BloomThreshold;
        }

        [MenuItem(
            "Tools/Post Processing/Debug Overlays/Dept hOf Field" + SHC.CTRL_ALT_SHFT_F8,
            priority = 49
        )]
        public static void PostProcessingOverlay_DepthOfField()
        {
            debugOverlay = DebugOverlay.DepthOfField;
        }

        [MenuItem("Tools/Post Processing/Monitors/Light Meter" + SHC.CTRL_ALT_SHFT_F9, true)]
        public static bool TogglePostProcessingMonitorLightMeterValidate()
        {
            Initialize();
            Menu.SetChecked("Tools/Post Processing/Monitors/Light Meter", lightMeter);
            return _debug != null;
        }

        [MenuItem(
            "Tools/Post Processing/Monitors/Light Meter" + SHC.CTRL_ALT_SHFT_F9,
            priority = 50
        )]
        public static void TogglePostProcessingMonitorLightMeter()
        {
            lightMeter = !lightMeter;
        }

        [MenuItem("Tools/Post Processing/Monitors/Histogram" + SHC.CTRL_ALT_SHFT_F10, true)]
        public static bool TogglePostProcessingMonitorHistogramValidate()
        {
            Initialize();
            Menu.SetChecked("Tools/Post Processing/Monitors/Histogram", histogram);
            return _debug != null;
        }

        [MenuItem(
            "Tools/Post Processing/Monitors/Histogram" + SHC.CTRL_ALT_SHFT_F10,
            priority = 50
        )]
        public static void TogglePostProcessingMonitorHistogram()
        {
            histogram = !histogram;
        }

        [MenuItem("Tools/Post Processing/Monitors/Waveform" + SHC.CTRL_ALT_SHFT_F11, true)]
        public static bool TogglePostProcessingMonitorWaveformValidate()
        {
            Initialize();
            Menu.SetChecked("Tools/Post Processing/Monitors/Waveform", waveform);
            return _debug != null;
        }

        [MenuItem("Tools/Post Processing/Monitors/Waveform" + SHC.CTRL_ALT_SHFT_F11, priority = 50)]
        public static void TogglePostProcessingMonitorWaveform()
        {
            waveform = !waveform;
        }

        [MenuItem("Tools/Post Processing/Monitors/Vectorscope" + SHC.CTRL_ALT_SHFT_F12, true)]
        public static bool TogglePostProcessingMonitorVectorscopeValidate()
        {
            Initialize();
            Menu.SetChecked("Tools/Post Processing/Monitors/Vectorscope", vectorscope);
            return _debug != null;
        }

        [MenuItem(
            "Tools/Post Processing/Monitors/Vectorscope" + SHC.CTRL_ALT_SHFT_F12,
            priority = 50
        )]
        public static void TogglePostProcessingMonitorVectorscope()
        {
            vectorscope = !vectorscope;
        }

        [MenuItem("Tools/Post Processing/Monitors/Height/32", priority = 40)]
        public static void PPM_Height_032()
        {
            SetMonitorHeight(32);
        }

        [MenuItem("Tools/Post Processing/Monitors/Height/48", priority = 40)]
        public static void PPM_Height_048()
        {
            SetMonitorHeight(48);
        }

        [MenuItem("Tools/Post Processing/Monitors/Height/64", priority = 40)]
        public static void PPM_Height_064()
        {
            SetMonitorHeight(64);
        }

        [MenuItem("Tools/Post Processing/Monitors/Height/96", priority = 40)]
        public static void PPM_Height_096()
        {
            SetMonitorHeight(96);
        }

        [MenuItem("Tools/Post Processing/Monitors/Height/128", priority = 40)]
        public static void PPM_Height_128()
        {
            SetMonitorHeight(128);
        }

        [MenuItem("Tools/Post Processing/Monitors/Height/192", priority = 40)]
        public static void PPM_Height_192()
        {
            SetMonitorHeight(192);
        }

        [MenuItem("Tools/Post Processing/Monitors/Height/256", priority = 40)]
        public static void PPM_Height_256()
        {
            SetMonitorHeight(256);
        }

        [MenuItem("Tools/Post Processing/Monitors/Height/384", priority = 40)]
        public static void PPM_Height_384()
        {
            SetMonitorHeight(384);
        }

        [MenuItem("Tools/Post Processing/Monitors/Height/512", priority = 40)]
        public static void PPM_Height_512()
        {
            SetMonitorHeight(512);
        }

        [MenuItem("Tools/Post Processing/Monitors/Width/32", priority = 40)]
        public static void PPM_Width_032()
        {
            SetMonitorWidth(32);
        }

        [MenuItem("Tools/Post Processing/Monitors/Width/48", priority = 40)]
        public static void PPM_Width_048()
        {
            SetMonitorWidth(48);
        }

        [MenuItem("Tools/Post Processing/Monitors/Width/64", priority = 40)]
        public static void PPM_Width_064()
        {
            SetMonitorWidth(64);
        }

        [MenuItem("Tools/Post Processing/Monitors/Width/96", priority = 40)]
        public static void PPM_Width_096()
        {
            SetMonitorWidth(96);
        }

        [MenuItem("Tools/Post Processing/Monitors/Width/128", priority = 40)]
        public static void PPM_Width_128()
        {
            SetMonitorWidth(128);
        }

        [MenuItem("Tools/Post Processing/Monitors/Width/192", priority = 40)]
        public static void PPM_Width_192()
        {
            SetMonitorWidth(192);
        }

        [MenuItem("Tools/Post Processing/Monitors/Width/256", priority = 40)]
        public static void PPM_Width_256()
        {
            SetMonitorWidth(256);
        }

        [MenuItem("Tools/Post Processing/Monitors/Width/384", priority = 40)]
        public static void PPM_Width_384()
        {
            SetMonitorWidth(384);
        }

        [MenuItem("Tools/Post Processing/Monitors/Width/512", priority = 40)]
        public static void PPM_Width_512()
        {
            SetMonitorWidth(512);
        }

#endif
    }
}
