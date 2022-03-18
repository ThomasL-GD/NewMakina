using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
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
            // Cycling through all the instances of the first indexed gameObject of m_instances
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
                        instance.HoudinoEnable(i);
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
}
