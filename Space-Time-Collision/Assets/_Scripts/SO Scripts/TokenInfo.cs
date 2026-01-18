using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Token", menuName = "Scriptable Objects/Token")]
public class Token : ScriptableObject
{
    public enum TokenType
    {
        Buff,
        Debuff,
        Ailments
    }

    public string tokenName;
    public string displayName;
    public Sprite tokenIcon;
    public TokenType tokenType;
    public float tokenValue;
    public int tokenCap;
    public List<String> tokenInverses;
    public bool isUnique;

    [TextArea(10,20)]
    public string tokenDescription;
}
