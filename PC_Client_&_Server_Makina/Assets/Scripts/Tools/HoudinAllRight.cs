using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [SelectionBase]
    public class HoudinAllRight : MonoBehaviour
    {
        public GameObject[] m_children;

        /// <summary/>
        /// Refreshing the list of m_children Array
        public void Refresh()
        {
            //gameObject.AddComponent<SelectionBaseAttribute>();
            // Creating the list that will contain all the children gameobjects
            List<GameObject> children= new List<GameObject>();

            // Fetching all the children and adding them to the list
            for (int i = 0; i < transform.childCount; i++) children.Add( transform.GetChild(i).gameObject);

            // Converting the list to array and setting it as the m_children value
            m_children = children.ToArray();
        }

        /// <summary/>
        /// A function that rotates the game object p_value degrees on the y axis
        /// <param name="p_value"> the degrees to rotate on the y axis </param>
        public void Rotate(float p_value)
        {
        
            // Adding the modification to the CTRL Z List
#if UNITY_EDITOR
            Undo.RecordObject(gameObject.transform, "Rotate GameObject with HoudinAllRight");
#endif  
            transform.Rotate(new Vector3(0f, p_value, 0f));
        }
    
        /// <summary/> A function that sets all the dialed children to EditorOnly
        public void ResetEditorOnly() {
            foreach (GameObject child in m_children) child.tag = child.activeSelf?"Untagged":"EditorOnly";
        }
        
        /// <summary/> Sets the children of a HoudinAllRight class to active or inactive while avoiding null index issues
        /// <param name="p_active"> the vakue to which the gameObject will be set (active or inactive) </param>
        /// <param name="instance"> the HoudinAllRight instance itself </param>
        /// <param name="p_index"> the gameObject index in the HoudinAllRight class </param>
        public void SafeSetActive(bool p_active, int p_index)
        {
        
            if (m_children[p_index] != null)
            {
            #if UNITY_EDITOR
                // Adding the modification to the CTRL Z List
                Undo.RecordObject(m_children[p_index], "Change GameObject with HoudinAllRight");
            #endif
                m_children[p_index].SetActive(p_active);
                m_children[p_index].tag = p_active?"Untagged":"EditorOnly";
            }
            else
            {
                Refresh();
                #if UNITY_EDITOR
                // Adding the modification to the CTRL Z List
                Undo.RecordObject(m_children[p_index], "Change GameObject with HoudinAllRight");
                #endif
                if (m_children[p_index] != null)
                {
                    m_children[p_index].SetActive(p_active);
                    m_children[p_index].tag = p_active?"Untagged":"EditorOnly";
                }
            }
        }

        public void HoudinoEnable(int p_index)
        {
            
            // For each instance of Houdinallright in the m_instances
            for (int i = 0; i < m_children.Length; i++)
                if (i != p_index) SafeSetActive(false, i);

            //Enable the wanted child
            SafeSetActive(true, p_index);
        }
    }
}