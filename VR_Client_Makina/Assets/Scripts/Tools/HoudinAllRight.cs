using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[SelectionBase]
public class HoudinAllRight : MonoBehaviour
{
    public GameObject[] m_children;

    /// <summary>
    /// Refreshing the list of m_children Array
    /// </summary>
    public void Refresh()
    {
        // Creating the list that will contain all the children gameobjects
        List<GameObject> children= new List<GameObject>();

        // Fetching all the children and adding them to the list
        for (int i = 0; i < transform.childCount; i++) children.Add( transform.GetChild(i).gameObject);

        // Converting the list to array and setting it as the m_children value
        m_children = children.ToArray();
    }

    /// <summary>
    /// A function that rotates the game object p_value degrees on the y axis
    /// </summary>
    /// <param name="p_value"> the degrees to rotate on the y axis </param>
    public void Rotate(float p_value)
    {
        
        // Adding the modification to the CTRL Z List
        #if UNITY_EDITOR
        Undo.RecordObject(gameObject.transform, "Rotae GameObject with HoudinAllRight");
        #endif  
        transform.Rotate(new Vector3(0f, p_value, 0f));
    }
}
