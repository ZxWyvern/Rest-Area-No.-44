using TMPro;
using UnityEngine;

namespace Game.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractionPromptPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _promptText;

        public void Show(string prompt)
        {
            if (_root != null && !_root.activeSelf)
            {
                _root.SetActive(true);
            }

            if (_promptText != null)
            {
                _promptText.text = prompt;
            }
        }

        public void Hide()
        {
            if (_root != null && _root.activeSelf)
            {
                _root.SetActive(false);
            }
        }
    }
}
