using HUDIndicator;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{

    [SerializeField] private PickableObject pickable;
    [SerializeField] private IndicatorRenderer indicatorRendererForPickable;
    [SerializeField] private IndicatorRenderer indicatorRendererForPuzzel;

    private bool isClossedAllIndictors = false;

    // Update is called once per frame
    void Update()
    {
        if (pickable != null && !isClossedAllIndictors)
        {
            indicatorRendererForPickable.visible = !pickable.IsCurrentlyPickedUp;
            indicatorRendererForPuzzel.visible = pickable.IsCurrentlyPickedUp;
        }
    }

    public void ClosseAllIndictors()
    {
        isClossedAllIndictors = true;
        indicatorRendererForPickable.visible = false;
        indicatorRendererForPuzzel.visible = false;
    }
}
