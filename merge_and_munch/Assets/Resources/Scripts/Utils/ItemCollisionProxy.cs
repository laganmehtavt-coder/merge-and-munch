    using UnityEngine;
    public class ItemCollisionProxy : MonoBehaviour {
        public Item parentItem;
        private void OnCollisionEnter2D(Collision2D collision) {
            if (parentItem != null)
                parentItem.ProxyCollisionEnter(collision);
        }
    }