using System.Collections;
using System.Collections.Generic;
using CombatMenu;
using ReDesign;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(RectTransform), typeof(Button))]
public class ActionButton : MonoBehaviour
{
    [Header("Narrow Button")] [SerializeField]
    private Vector2 narrowPosition;

    [SerializeField] private Vector2 narrowSize;
    [SerializeField] private Texture2D narrowTexture;

    [Header("Wide Button")] [SerializeField]
    private Vector2 widePosition;

    [SerializeField] private Vector2 wideSize;
    [SerializeField] private Texture2D wideTexture;
    [Header("")] [SerializeField] private ActionButton[] buttons;
    [field: SerializeField] public bool active { get; private set; } = false;

    [Header("Key binding")] [SerializeField]
    private KeyCode keyBinding;

    [SerializeField] private RectTransform bindingTextTransform;
    [SerializeField] private Vector2 narrowBindingPosition;
    [SerializeField] private Vector2 wideBindingPosition;
    [Header("Slow call")] [SerializeField] private int requiredConfirmations = 1;
    private int confirmations = 0;
    [SerializeField] private UnityEvent onConfirm = new UnityEvent();
    private RawImage image;
    public Button button;
    private RectTransform rectTransform;
    private Canvas spellMenu;
    private Canvas pauseMenu;
    private Canvas helpScreen;
    private Canvas _tutorialCanvas;

    void Awake()
    {
        // Retrieve the rect transform and button of the current object
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<RawImage>();
        button = GetComponent<Button>();

        // Find the spell menu and pause menu canvas
        spellMenu = GameObject.Find("SpellMenu").GetComponent<Canvas>();
        pauseMenu = GameObject.Find("PauseMenu").GetComponent<Canvas>();
        helpScreen = GameObject.Find("HelpScreen").GetComponent<Canvas>();


        // Add a listener for the OnClick of the button, to make the button wide
        button.onClick.AddListener(Activate);

        // Make the button wide or narrow depending on whether the button is active
        if (active)
        {
            MakeWide();
        }
    }

    void Update()
    {
        // Simulate a click event when the keybinding has been pressed and the spellmenu is closed
        if (Input.GetKeyDown(keyBinding) && !spellMenu.enabled && !pauseMenu.enabled && !helpScreen.enabled &&
            !PlayerAnimator.PerformingAction() && TurnController._turnPart == 1)
        {
            if (SceneManager.GetActiveScene().buildIndex == 2)
            {
                _tutorialCanvas = GameObject.Find("Tutorial").GetComponent<Canvas>();
                if (!_tutorialCanvas.enabled)
                {
                    button.onClick.Invoke();
                }
            }
            else
            {
                button.onClick.Invoke();
            }
        }
    }

    void MakeNarrow()
    {
        // Make this button narrow
        rectTransform.anchoredPosition = narrowPosition;
        rectTransform.sizeDelta = narrowSize;

        // Set the correct image
        image.texture = narrowTexture;
    }

    void MakeWide()
    {
        // Make this button wide
        rectTransform.anchoredPosition = widePosition;
        rectTransform.sizeDelta = wideSize;

        // Set the correct image
        image.texture = wideTexture;
    }

    public void Activate()
    {
        // Register the click event as a confirmation
        confirmations++;

        // Make the button wide
        MakeWide();

        // Make the other buttons narrow
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].Deactivate();
        }

        // If the button is not active and the player has confirmed they want to activate the action
        if (!active && confirmations >= requiredConfirmations)
        {
            // Activate the button
            active = true;

            // Reset the confirmation count
            confirmations = 0;

            // Call the slow-call functions
            onConfirm.Invoke();
        }
    }

    public void Deactivate()
    {
        // Deactivate the button
        MakeNarrow();
        active = false;
        confirmations = 0;
    }
}