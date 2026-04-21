using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Hooks the main menu buttons to the scene loader.
    public class MenuButtons : MonoBehaviour
    {
        [Header("Wiring")]
        public Button testButton;
        public Button levelsButton;

        void Start()
        {
            if (testButton != null) testButton.onClick.AddListener(SceneLoader.LoadTest);
            if (levelsButton != null) levelsButton.onClick.AddListener(SceneLoader.LoadLevels);
        }
    }
}
