using System.Security;
using UnityEngine;

// It is not MonoBeahviour, it is ScriptableObject
[CreateAssetMenu()]
public class KitchenObjectSO : ScriptableObject 
{
    public Transform prefab;
    public Sprite sprite;
    public string objectName;
}
