using UnityEngine;
using static GameManager;

public class MenuButtonScript : MonoBehaviour
{
    public void Quit() { QuitGame(); }
    public void Restart() { RestartLevel(); }
    public void Load(int diff) { LoadLevelRelative(diff); }
}
