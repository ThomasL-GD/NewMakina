using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(HoudinAllRight))]
[CanEditMultipleObjects]
public class HoudinAllRightInspector : Editor
{
    // The instances of all the selected HoudinAllRight classes
    private HoudinAllRight[] m_instances;
        
    /// <summary/> Called when the script is added onto the object and enabled
    public void OnEnable() => ResetArray();

    /// <summary/> Called every frame the GUI is visible
    public override void OnInspectorGUI()
    {
        // Cycling through all the istances of the first indexed gameObject of m_instances
        for (int i = 0; i < m_instances[0].m_children.Length; i++)
        {
            // Removing the noise of the object name
            string buttonName = m_instances[0].m_children[i].name;
            buttonName = buttonName.Replace("M_", "");
            buttonName = buttonName.Replace("P_", "");
            buttonName = buttonName.Replace('_', ' ');
            
            // Creating buttons for each child of the first indexed gameObject of m_instances
            if (GUILayout.Button(buttonName))
            {
                // For each instance of Houdinallright in the m_instances
                foreach (HoudinAllRight instance in m_instances)
                {
                    // Disable all the "unwanted" children
                    for (int j = 0; j < instance.m_children.Length; j++)
                        if (j != i) SafeSetActive(false, instance, j);

                    //Enable the wanted child
                    SafeSetActive(true, instance, i);
                }
            }
        }
        
        // Adding a rotate 90 and a rotate -90 Button
        GUILayout.Space(20);
        if (GUILayout.Button("Rotate 90")) foreach (HoudinAllRight instance in m_instances) instance.Rotate(90);
        if (GUILayout.Button("Rotate -90")) foreach (HoudinAllRight instance in m_instances) instance.Rotate(-90);
        
        //Adding a refresh button
        GUILayout.Space(20);
        if (GUILayout.Button("Refresh")) ResetArray();
        
        //Adding a refresh button
        GUILayout.Space(20);
        if (GUILayout.Button("Reset Edtor Only Children"))
        {
            foreach (HoudinAllRight script in m_instances)
            {
                script.ResetEditorOnly();
            }
        }
    }
    

    /// <summary/> Sets the children of a HoudinAllRight class to active or inactive while avoiding null index issues
    /// <param name="p_active"> the vakue to which the gameObject will be set (active or inactive) </param>
    /// <param name="instance"> the HoudinAllRight instance itself </param>
    /// <param name="p_index"> the gameObject index in the HoudinAllRight class </param>
    private void SafeSetActive(bool p_active, HoudinAllRight instance, int p_index)
    {
        
        if (instance.m_children[p_index] != null)
        {
            // Adding the modification to the CTRL Z List
            Undo.RecordObject(instance.m_children[p_index], "Change GameObject with HoudinAllRight");
            instance.m_children[p_index].SetActive(p_active);
            instance.m_children[p_index].tag = p_active?"Untagged":"EditorOnly";
        }
        else
        {
            instance.Refresh();
            // Adding the modification to the CTRL Z List
            Undo.RecordObject(instance.m_children[p_index], "Change GameObject with HoudinAllRight");
            if (instance.m_children[p_index] != null)
            {
                instance.m_children[p_index].SetActive(p_active);
                instance.m_children[p_index].tag = p_active?"Untagged":"EditorOnly";
            }
        }
    }
    
    /// <summary/> Resets the Array of HoudinAllRight Selected scripts
    private void ResetArray()
    {
        // Creating the list that will contain all the selected HoudinAllRight classes
        List<HoudinAllRight> list = new List<HoudinAllRight>();
        
        // fetching all the selected HoudinAllRight classes
        foreach (object target in targets)
            list.Add(target as HoudinAllRight);
        
        // Converting the list to array and setting it as the m_instances value 
        m_instances = list.ToArray();
        
        // Refreshing all the instances
        foreach (HoudinAllRight instance in m_instances) instance.Refresh();
    }

    
}
