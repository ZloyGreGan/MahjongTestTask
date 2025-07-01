using Game.Scripts.Common.MVP;
using Game.Scripts.Mahjong.MainMenu.MVP.Presenter;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Mahjong.MainMenu.MVP.View
{
    public interface IMainMenuView : IView { }
    
    public class MainMenuView : MonoBehaviour, IMainMenuView
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _exitButton;
        
        public bool IsActive => gameObject.activeSelf;
        public MonoBehaviour monoBehaviour => this;

        private MainMenuPresenter _presenter;
        
        public void Init(APresenter presenter)
        {
            _presenter = (MainMenuPresenter) presenter;
        }

        private void OnEnable()
        {
            _playButton.onClick.AddListener(_presenter.PlayGame);
            _exitButton.onClick.AddListener(_presenter.ExitGame);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveAllListeners();
            _exitButton.onClick.RemoveAllListeners();
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}