using UnityEngine;
using Yarn.Unity;

public sealed class MerchantYarnCommands : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private CanvasGroup linePresenterCanvasGroup;

    private void Awake()
    {
        if (linePresenterCanvasGroup == null && dialogueRunner != null)
            linePresenterCanvasGroup = dialogueRunner.GetComponentInChildren<CanvasGroup>(true);

        SetShopOpen(false);
    }

    [YarnCommand("open_shop_panel")]
    public void OpenShopPanel()
    {
        if (shopPanel == null)
        {
            Debug.LogWarning("Cannot open the merchant shop because Shop Panel is not assigned.", this);
            return;
        }

        SetShopOpen(true);
    }

    public async void StartMerchantNode(string nodeName)
    {
        if (dialogueRunner == null)
        {
            Debug.LogWarning("Cannot start merchant dialogue because DialogueRunner is not assigned.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(nodeName))
        {
            Debug.LogWarning("Cannot start merchant dialogue because the Yarn node name is empty.", this);
            return;
        }

        SetShopOpen(false);

        await dialogueRunner.StartDialogue(nodeName);

        SetShopOpen(true);
    }

    private void SetShopOpen(bool isOpen)
    {
        if (shopPanel != null)
            shopPanel.SetActive(isOpen);

        if (linePresenterCanvasGroup != null)
            linePresenterCanvasGroup.blocksRaycasts = !isOpen;
    }
}
