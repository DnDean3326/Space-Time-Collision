using UnityEngine;

[CreateAssetMenu(fileName = "TalismanInfo", menuName = "Scriptable Objects/TalismanInfo")]
public class TalismanInfo : ScriptableObject
{
    public string talismanName;
    public Sprite talismanIcon;
    public int talismanPrice;

    [Header("Description")]
    [TextArea(10,20)]
    public string talismanDescription;
}
