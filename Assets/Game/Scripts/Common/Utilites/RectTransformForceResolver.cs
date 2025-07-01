using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Common.Utilites
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformForceResolver : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

        private void OnValidate()
        {
            _rectTransform ??= GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        private void OnEnable()
        {
            RebuildLayoutNextFrame().Forget();
        }

        private async UniTaskVoid RebuildLayoutNextFrame()
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }
    }

}