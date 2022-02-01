using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Menu {

    public class MakiButton : MonoBehaviour {

        public delegate void ClickDelegator();
        public ClickDelegator OnClick;

        public void OnPointerClick(PointerEventData p_eventData) {
            if (p_eventData.button != PointerEventData.InputButton.Left)
                return;

            if(OnClick == null) Debug.LogWarning("You pressed a button but there is nothing triggered in the delegate...", this);
            OnClick?.Invoke();
        }

    }

}
