using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SelectParent : MonoBehaviour
{
    /// <summary/> Selects the parent of all the actively selected HoudinAllRight classed objects
    [MenuItem("Tools/Select Parents of HoudinAllRight &c")]
    public static void ParentSelect()
    {
        // Creating a list that will contain thenew selection
        List<Object> newSelection = new List<Object>();
        
        // Going through every object and checking if they have are a HoudinAllRight class and adding it's parent to the selection 
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            GameObject go = ((GameObject) Selection.objects[i]);
            
            if (go.transform.parent == null || go.transform.parent.gameObject.CompareTag("HoudinAllRight Select Ignore"))
            {
                newSelection.Add(go);
                continue;
            }
            
            if (go.TryGetComponent(out HoudinAllRight ha))
            {
                newSelection.Add(go.transform.parent.gameObject);
                continue;
            }
            
            newSelection.Add(go);
        }
        
        // Setting the new selection
        Selection.objects = newSelection.ToArray();
    }
}
