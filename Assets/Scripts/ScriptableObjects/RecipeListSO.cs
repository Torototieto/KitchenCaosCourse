using System.Collections.Generic;
using UnityEngine;

// Commented out so we cannot create new RecipeListSO objects on the editor
// [CreateAssetMenu()]
public class RecipeListSO : ScriptableObject
{
    public List<RecipeSO> recipeSOList;
}
