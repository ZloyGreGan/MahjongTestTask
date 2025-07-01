using UnityEngine;

namespace Game.Scripts.Common.MVP
{
    public interface IView
    {
        public void Init(APresenter presenter);
        bool IsActive { get; }
        public MonoBehaviour monoBehaviour { get; }
        void SetActive(bool value);
    }
}
