using UnityEngine;
using UnityEngine.UI;

public class GateController : MonoBehaviour
{
    [Header("Gate References")]
    public AncientGateSystem[] gates;

    [Header("Input Settings")]
    public KeyCode activateKey = KeyCode.F;
    public KeyCode openAllKey = KeyCode.O;
    public KeyCode closeAllKey = KeyCode.C;

    [Header("Proximity Settings")]
    public bool openOnProximity = false;
    [Range(1f, 20f)]
    public float activationDistance = 5f;
    public string playerTag = "Player";

    [Header("UI")]
    public Text interactionText;
    public string interactionMessage = "Press F to open gate";

    [Header("Debug")]
    public bool showDebugMessages = true;

    private Transform player;
    private bool playerInRange = false;
    private int currentGateIndex = 0;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        SetupGateEvents();

        if (showDebugMessages)
        {
            Debug.Log($"Gate Controller activated - Number of gates: {gates.Length}");
        }
    }

    void Update()
    {
        CheckProximity();
        HandleInput();
    }

    void SetupGateEvents()
    {
        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null)
            {
                int gateIndex = i;

                gates[i].OnGateStartOpening += () => OnGateEvent($"Gate {gateIndex} started opening");
                gates[i].OnGateFullyOpened += () => OnGateEvent($"Gate {gateIndex} fully opened");
                gates[i].OnGateStartClosing += () => OnGateEvent($"Gate {gateIndex} started closing");
                gates[i].OnGateFullyClosed += () => OnGateEvent($"Gate {gateIndex} fully closed");
            }
        }
    }

    void CheckProximity()
    {
        if (player == null || gates.Length == 0) return;

        bool wasInRange = playerInRange;
        playerInRange = false;

        float closestDistance = float.MaxValue;
        int closestGateIndex = -1;

        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null)
            {
                float distance = Vector3.Distance(player.position, gates[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestGateIndex = i;
                }
            }
        }

        if (closestDistance <= activationDistance && closestGateIndex != -1)
        {
            playerInRange = true;
            currentGateIndex = closestGateIndex;

            if (openOnProximity && !wasInRange && !gates[currentGateIndex].IsOpen())
            {
                gates[currentGateIndex].OpenGate();
                if (showDebugMessages)
                    Debug.Log($"Auto-opening gate {currentGateIndex}");
            }
        }

        UpdateUI();
    }

    void HandleInput()
    {
        if (gates.Length == 0) return;

        if (Input.GetKeyDown(activateKey))
        {
            if (playerInRange && currentGateIndex < gates.Length)
            {
                gates[currentGateIndex].ToggleGate();
            }
            else if (!playerInRange)
            {
                gates[0].ToggleGate();
            }
        }

        if (Input.GetKeyDown(openAllKey))
        {
            OpenAllGates();
        }

        if (Input.GetKeyDown(closeAllKey))
        {
            CloseAllGates();
        }

        for (int i = 0; i < Mathf.Min(gates.Length, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                gates[i].ToggleGate();
                if (showDebugMessages)
                    Debug.Log($"Manually toggling gate {i}");
            }
        }
    }

    void UpdateUI()
    {
        if (interactionText == null) return;

        if (playerInRange && currentGateIndex < gates.Length)
        {
            interactionText.gameObject.SetActive(true);

            string message = interactionMessage;
            if (gates[currentGateIndex].IsOpen())
            {
                message = message.Replace("open", "close");
            }

            interactionText.text = message;
        }
        else
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    public void OpenAllGates()
    {
        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null && !gates[i].IsOpen())
            {
                StartCoroutine(DelayedGateAction(gates[i], true, i * 0.5f));
            }
        }

        if (showDebugMessages)
            Debug.Log("Opening all gates");
    }

    public void CloseAllGates()
    {
        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null && gates[i].IsOpen())
            {
                StartCoroutine(DelayedGateAction(gates[i], false, i * 0.3f));
            }
        }

        if (showDebugMessages)
            Debug.Log("Closing all gates");
    }

    System.Collections.IEnumerator DelayedGateAction(AncientGateSystem gate, bool open, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (open)
            gate.OpenGate();
        else
            gate.CloseGate();
    }

    public void ActivateGate(int gateIndex)
    {
        if (gateIndex >= 0 && gateIndex < gates.Length && gates[gateIndex] != null)
        {
            gates[gateIndex].ToggleGate();
            if (showDebugMessages)
                Debug.Log($"Activated gate number {gateIndex}");
        }
    }

    public void OpenGate(int gateIndex)
    {
        if (gateIndex >= 0 && gateIndex < gates.Length && gates[gateIndex] != null)
        {
            gates[gateIndex].OpenGate();
        }
    }

    public void CloseGate(int gateIndex)
    {
        if (gateIndex >= 0 && gateIndex < gates.Length && gates[gateIndex] != null)
        {
            gates[gateIndex].CloseGate();
        }
    }

    public void AddGate(AncientGateSystem newGate)
    {
        if (newGate == null) return;

        AncientGateSystem[] newArray = new AncientGateSystem[gates.Length + 1];
        for (int i = 0; i < gates.Length; i++)
        {
            newArray[i] = gates[i];
        }
        newArray[gates.Length] = newGate;
        gates = newArray;

        SetupGateEvents();
    }

    void OnGateEvent(string message)
    {
        if (showDebugMessages)
        {
            Debug.Log($"[Gate Event] {message}");
        }
    }

    public AncientGateSystem GetClosestGate()
    {
        if (player == null || gates.Length == 0) return null;

        AncientGateSystem closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null)
            {
                float distance = Vector3.Distance(player.position, gates[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = gates[i];
                }
            }
        }

        return closest;
    }

    void OnDrawGizmosSelected()
    {
        if (gates == null) return;

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] != null)
            {
                Gizmos.DrawSphere(gates[i].transform.position, activationDistance);
            }
        }
    }
}