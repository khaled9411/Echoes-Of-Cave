using UnityEngine;
using UnityEngine.Events;

public class TorchInteractable : PickableObject
{
    [Header("Torch Specific Settings")]
    [SerializeField] private float extinguishTime = 300f;
    [SerializeField] private GameObject flameEffect;
    [SerializeField] private Light torchLight;
    [SerializeField] private string relightPrompt = "Hold E to relight";

    private float currentTorchTime;
    private bool _isLit = false;

    public bool IsLit => _isLit;

    public override string InteractionPrompt
    {
        get
        {
            if (!IsLit && !IsPickedUp) return relightPrompt;
            if (IsPickedUp) return dropPrompt;
            return base.InteractionPrompt;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        currentTorchTime = extinguishTime;
        SetTorchState(true);
    }

    private void Update()
    {
        if (IsLit && IsPickedUp)
        {
            currentTorchTime -= Time.deltaTime;
            if (currentTorchTime <= 0)
            {
                SetTorchState(false);
                Debug.Log("Torch extinguished!");
            }
        }
    }

    public override bool CanInteract(GameObject interactor)
    {

        return base.CanInteract(interactor) || (!IsLit && !IsPickedUp);
    }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
    }


    public void RelightTorch()
    {
        SetTorchState(true);
        currentTorchTime = extinguishTime;
        PlayInteractionSound();
        Debug.Log("Torch relit!");
        InteractionUIManager.Instance?.HidePrompt();
    }

    private void SetTorchState(bool lit)
    {
        _isLit = lit;
        if (flameEffect != null) flameEffect.SetActive(lit);
        if (torchLight != null) torchLight.enabled = lit;
    }
}