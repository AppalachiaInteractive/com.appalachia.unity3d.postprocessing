using System;
using System.Collections.Generic;
using Appalachia.Base.Scriptables;
using Appalachia.Core.Comparisons;
using Appalachia.Editing.Attributes;
using Appalachia.Utility.Enums;
using Sirenix.OdinInspector;

namespace Appalachia.PostProcessing.AutoFocus
{
    [Serializable]
    public class DepthOfFieldStateSettingCollection : SelfNamingSavingAndIdentifyingScriptableObject
        <DepthOfFieldStateSettingCollection>
    {
        [FoldoutGroup("Defaults")]
        [PropertyRange(0.01f, 1.0f)]
        [SmartLabel]
        public float velocitySmoothing = .2f;

        [FoldoutGroup("Defaults")]
        [PropertyRange(0.1f, 50.0f)]
        [SmartLabel]
        public float focusDistance = 15f;

        [FoldoutGroup("Defaults")]
        [PropertyRange(0.0f, 128.0f)]
        [SmartLabel]
        public float aperture = 16f;

        [InlineEditor] public List<DepthOfFieldStateSettings> settings;

        private IComparer<DepthOfFieldStateSettings> _comparison;

        protected override void OnEnable()
        {
            var values = EnumValueManager.GetAllValues<DepthOfFieldState>();

            if (settings == null)
            {
                settings = new List<DepthOfFieldStateSettings>();
                SetDirty();
            }

            while (settings.Count < values.Length)
            {
                var targetname = $"{name}_{values[settings.Count]}";

                settings.Add(DepthOfFieldStateSettings.LoadOrCreateNew(targetname));
                SetDirty();
            }

            while (settings.Count > values.Length)
            {
                settings.RemoveAt(settings.Count - 1);
                SetDirty();
            }

            if (_comparison == null)
            {
                _comparison =
                    new ComparisonWrapper<DepthOfFieldStateSettings>(
                        (a, b) => a.state.CompareTo(b.state)
                    );
            }

            settings.Sort(_comparison);

            for (var i = 0; i < settings.Count; i++)
            {
                var targetName = $"{name}_{values[i]}";

                if (settings[i].name != targetName)
                {
                    settings[i].Rename(targetName);
                }
            }

            for (var i = 0; i < settings.Count; i++)
            {
                var setting = settings[i];

                if (setting.nearPlane == null)
                {
                    setting.nearPlane = new DepthOfFieldStatePlaneSettings();
                    setting.SetDirty();
                }

                if (setting.farPlane == null)
                {
                    setting.farPlane = new DepthOfFieldStatePlaneSettings();
                    setting.SetDirty();
                }
            }
        }

        [Button]
        private void Refresh()
        {
            OnEnable();
        }
    }
}
