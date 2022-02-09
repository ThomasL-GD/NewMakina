using UnityEngine;

namespace GD2
{
    /// <summary>
    /// Une classe qui permet de créer des singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// Les variable modifiables dans un Serialize field
        /// </summary>
        
        //Bool : est-ce que l'objet sera ajouté dans la catégories à ne pas décharger lors d'un changement de scene
        [SerializeField][Tooltip("est-ce que l'objet sera déchargé lors d'un changement de scene")] private bool m_dontDestroyOnLoad;
    
        //String : nom que prendras le game object si vous voulez le changer
        [SerializeField][Tooltip("nom que prendras la game object si vous voulez le changer (garder vide si vous ne voulez pas le changer)")] private string m_name;
        
        //L'instance qui assurera que l'objet resteras un singleton
        protected static T m_instance;
        public static T Instance
        {
            get
            {
                //Est-ce que l'objet existe déjà comme singleton
                if (m_instance != null) return m_instance;

                //Est-ce que l'object existe déjà dans la scene et affectation au singleton
                m_instance = FindObjectOfType<T>();

                if (m_instance == null)
                {
                    //Si il n'y a aucune instance de ce script dans la scene créer un nouveau singleton
                    CreateSingleton();
                }

                //Message d'avertissement si l'utilisateur a plusieurs instances d'un singleton dans la scene
                int amountOfInstances = FindObjectsOfType<T>().Length;
                if (amountOfInstances > 1)
                    Debug.LogWarning(amountOfInstances + $" SingletonClass Detected. Only {m_instance.name} will taken into account.");
                
                //Lancement de l'initialize
                (m_instance as Singleton<T>)?.Initialize();
                return m_instance;
                
            }
        
        }

        /// <summary>
        /// Création d'un singleton
        /// </summary>
        static void CreateSingleton()
        {
            GameObject singletonObject = new GameObject();

            m_instance = singletonObject.AddComponent<T>();
        }

        /// <summary>
        /// Intialisation du singleton
        /// </summary>
        protected virtual void Initialize()
        {
            //Vérification des conditions des variables serializées
            
            if (m_dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            if (!string.IsNullOrWhiteSpace(m_name))
                m_instance.gameObject.name = m_name;
        }
    }
}