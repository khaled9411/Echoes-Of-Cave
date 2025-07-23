using UnityEngine;

public class CollectibleInteractable : BaseInteractable
{
    [Header("Collectible Settings")]
    [SerializeField] private string itemName = "Coin";
    [SerializeField] private int quantity = 1;
    [SerializeField] private bool destroyOnCollect = true;

    public override void Interact(GameObject interactor)
    {
        var inventory = interactor.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddItem(itemName, quantity);
            PlayInteractionSound();

            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                isInteractable = false;
                gameObject.SetActive(false);
            }
        }
    }
}
