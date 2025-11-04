using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class QuestSystemKeyboardBinder : MonoBehaviour
{
    [SerializeField] TMP_InputField field;
    TouchScreenKeyboard kb;

    void Awake()
    {
        if (!field) field = GetComponent<TMP_InputField>();

        // Listen for selection
        field.onSelect.AddListener(OnFieldSelected);
        field.onDeselect.AddListener(OnFieldDeselected);
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (field != null)
        {
            field.onSelect.RemoveListener(OnFieldSelected);
            field.onDeselect.RemoveListener(OnFieldDeselected);
        }
    }

    void OnFieldSelected(string text)
    {
        Debug.Log($"InputField selected: {field.name}");
        Open();
    }

    void OnFieldDeselected(string text)
    {
        Debug.Log($"InputField deselected: {field.name}");
        Close();
    }

    void Open()
    {
        // CRITICAL FIX: Use NumbersAndPunctuation to allow decimal point!
        // NumberPad only allows integers (no decimal point)
        kb = TouchScreenKeyboard.Open(
            field.text,
            TouchScreenKeyboardType.NumbersAndPunctuation,  // Changed from NumberPad!
            false,  // autocorrection
            false,  // multiline
            false,  // secure
            false,  // alert
            field.placeholder?.GetComponent<TMP_Text>()?.text,
            0       // character limit
        );

        if (kb != null)
        {
            Debug.Log("Quest keyboard opened successfully");
        }
        else
        {
            Debug.LogError("Failed to open Quest keyboard!");
        }
    }

    void Close()
    {
        if (kb != null)
        {
            kb.active = false;
            Debug.Log("Quest keyboard closed");
        }
        kb = null;
    }

    void Update()
    {
        if (kb == null) return;

        // Mirror OS keyboard text into TMP without re-triggering events
        if (field.text != kb.text)
        {
            field.SetTextWithoutNotify(kb.text);
        }

        // When user finishes/cancels
        if (kb.status == TouchScreenKeyboard.Status.Canceled ||
            kb.status == TouchScreenKeyboard.Status.Done)
        {
            field.onEndEdit?.Invoke(field.text);
            Close();
        }
    }
}