using UnityEngine.SceneManagement;

namespace Slash
{
    // Named scene constants plus small helpers for the menu buttons.
    public static class SceneLoader
    {
        public const string MenuScene = "MainMenu";
        public const string TestScene = "Test";
        public const string LevelsScene = "Levels";

        public static void LoadMenu() { SceneManager.LoadScene(MenuScene); }
        public static void LoadTest() { SceneManager.LoadScene(TestScene); }
        public static void LoadLevels() { SceneManager.LoadScene(LevelsScene); }
    }
}
