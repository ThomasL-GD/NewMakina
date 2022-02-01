using UnityEngine;

namespace Synchronizers {
    /// <summary>
    /// A singleton class which will be used to synchronize the Clients and the server
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Synchronizer<T> : MonoBehaviour where T : Component {

        protected static T m_instance;
        
        public static T Instance
        {
            get
            {
                //Est-ce que l'objet existe déjà comme singleton
                if (m_instance != null) return m_instance;
                
                m_instance = FindObjectOfType<T>();
                
                int amountOfInstances = FindObjectsOfType<T>().Length;
                if (amountOfInstances > 1) Debug.LogWarning(amountOfInstances + $" Synchronizer Class Detected. Only {m_instance.name} will taken into account.");
                
                if (m_instance != null) return m_instance;
                
                Debug.LogError("trying to call a synchronizer that isn't instantiated in the project");
                return null;
            }
        }
    }
}