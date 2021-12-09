using System.Runtime.CompilerServices;
using UnityEngine;

namespace Synchronizers {
    public abstract class Synchronizer/*<T>*/ : MonoBehaviour /*where T : Synchronizer<T> */{
        
        //TODO repair this coz it ain't working properly and is f*cking useful (and fancy ( ͡° ͜ʖ ͡°) )
        /*private static int howManyMe = 0;
        public static T halfSingleton = null;

        protected void Awake() {
            Debug.Log("I wanna check something out", this);
            howManyMe++;
            halfSingleton = this;
            
            if (howManyMe != 1) {
                Debug.LogError("                                                         c=====e\nH\n____________                                         _,,_H__\n(__((__((___()                                       //|     |\n(__((__((___()()_____________________________________// |ACME |\n(__((__((___()()()------------------------------------'  |_____|", this);
                this.enabled = false;
            }
        }*/
    }
}
