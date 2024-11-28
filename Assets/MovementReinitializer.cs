using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using PlanetRunner;

public class MovementReinitializer : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "main")
        {
            StartCoroutine(ReinitializeMovement());
        }
    }

    private IEnumerator ReinitializeMovement()
    {
        // Wait for everything to be properly initialized
        yield return new WaitForSeconds(0.2f);

        // Find the player controller
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found!");
            yield break;
        }

        // Find the movement buttons
        GameObject leftButton = GameObject.Find("MoveLeftButton");
        GameObject rightButton = GameObject.Find("MoveRightButton");

        if (leftButton != null && rightButton != null)
        {
            // Clear and reset event triggers
            foreach (var button in new[] { leftButton, rightButton })
            {
                // Remove existing components
                EventTrigger oldTrigger = button.GetComponent<EventTrigger>();
                if (oldTrigger != null)
                {
                    Destroy(oldTrigger);
                }

                // Add fresh EventTrigger
                EventTrigger trigger = button.AddComponent<EventTrigger>();

                // Setup events based on which button it is
                if (button == leftButton)
                {
                    AddEventTriggerEntry(trigger, EventTriggerType.PointerDown, (data) => { playerController.OnMoveLeftButtonDown(); });
                    AddEventTriggerEntry(trigger, EventTriggerType.PointerUp, (data) => { playerController.OnMoveLeftButtonUp(); });
                    AddEventTriggerEntry(trigger, EventTriggerType.PointerExit, (data) => { playerController.OnMoveLeftButtonUp(); });
                }
                else
                {
                    AddEventTriggerEntry(trigger, EventTriggerType.PointerDown, (data) => { playerController.OnMoveRightButtonDown(); });
                    AddEventTriggerEntry(trigger, EventTriggerType.PointerUp, (data) => { playerController.OnMoveRightButtonUp(); });
                    AddEventTriggerEntry(trigger, EventTriggerType.PointerExit, (data) => { playerController.OnMoveRightButtonUp(); });
                }
            }

            Debug.Log("Movement controls reinitialized");
        }
        else
        {
            Debug.LogError("Movement buttons not found!");
        }
    }

    private void AddEventTriggerEntry(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }
}