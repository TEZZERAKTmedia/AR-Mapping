using UnityEngine;

public class ToggleCoachingUI : MonoBehaviour
{
    [SerializeField]
    private GameObject coachingUI;

    private bool isVisible = false;

    public void ToggleUI()
    {
        isVisible = !isVisible;
        coachingUI.SetActive(isVisible);
    }
}
