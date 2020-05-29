using TMPro;
using UnityEngine;

namespace NotificationSamples.Demo
{
    public class NotificationEventItem : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI eventLabel;

        public void Show(string eventText)
        {
            eventLabel.text = eventText;
        }
    }
}
