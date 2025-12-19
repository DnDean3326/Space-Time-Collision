using UnityEngine;

[CreateAssetMenu(fileName = "Token", menuName = "Scriptable Objects/Token")]
public class TokenInfo : ScriptableObject
{
    public enum TokenType
    {
        Buff,
        Debuff,
        Ailments
    }

    public string tokenName;
    public Sprite tokenIcon;
    public TokenType tokenType;
    public float tokenValue;
    public int tokenCap;

    public string tokenDescription;
}
