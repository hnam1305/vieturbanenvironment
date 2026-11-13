using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BusinessAuthority : MonoBehaviour
{
    [SerializeField] private PlayerWallet playerWallet;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button registerBusinessButton;
    [SerializeField] private Button payReputationFineButton;
    [SerializeField, Min(1)] private int registrationFee = 20000;
    [SerializeField, Min(1)] private int reputationFine = 50000;

    private void Start()
    {
        if (registerBusinessButton != null)
        {
            registerBusinessButton.onClick.AddListener(RegisterBusiness);
        }
        if (payReputationFineButton != null)
        {
            payReputationFineButton.onClick.AddListener(PayReputationFine);
        }
        RefreshDialogue();
    }

    private void OnDestroy()
    {
        if (registerBusinessButton != null)
        {
            registerBusinessButton.onClick.RemoveListener(RegisterBusiness);
        }
        if (payReputationFineButton != null)
        {
            payReputationFineButton.onClick.RemoveListener(PayReputationFine);
        }
    }

    public void RefreshDialogue()
    {
        PlayerData playerData = GameSession.Current;
        UpdateButtonVisibility(playerData);

        if (!playerData.businessRegistered)
        {
            SetDialogueText("register your business to unlock orders and selling. fee: " + registrationFee.ToString("N0") + "đ");
            return;
        }
        if (playerData.businessSuspended)
        {
            SetDialogueText("your business is suspended. pay the reputation fine to reopen selling. fee: " + reputationFine.ToString("N0") + "đ. reputation: " + playerData.reputation);
            return;
        }

        SetDialogueText("your business is active. reputation: " + playerData.reputation);
    }

    public void RegisterBusiness()
    {
        PlayerData playerData = GameSession.Current;
        if (playerData.businessRegistered)
        {
            RefreshDialogue();
            return;
        }
        if (playerWallet == null || !playerWallet.TrySpend(registrationFee))
        {
            SetDialogueText("not enough money to register. fee: " + registrationFee.ToString("N0") + "đ");
            return;
        }

        playerData.businessRegistered = true;
        SaveSystem.SaveGame(playerData);
        UpdateButtonVisibility(playerData);
        SetDialogueText("business registered. you can now receive orders and trade.");
    }

    public void PayReputationFine()
    {
        PlayerData playerData = GameSession.Current;
        if (!playerData.businessSuspended)
        {
            RefreshDialogue();
            return;
        }
        if (playerWallet == null || !playerWallet.TrySpend(reputationFine))
        {
            SetDialogueText("not enough money to pay the reputation fine. fee: " + reputationFine.ToString("N0") + "đ");
            return;
        }

        playerData.businessSuspended = false;
        SaveSystem.SaveGame(playerData);
        UpdateButtonVisibility(playerData);
        SetDialogueText("fine paid. selling is available again. reputation remains " + playerData.reputation + ".");
    }

    private void UpdateButtonVisibility(PlayerData playerData)
    {
        if (registerBusinessButton != null)
        {
            registerBusinessButton.gameObject.SetActive(!playerData.businessRegistered);
        }
        if (payReputationFineButton != null)
        {
            payReputationFineButton.gameObject.SetActive(playerData.businessSuspended);
        }
    }

    private void SetDialogueText(string message)
    {
        if (dialogueText != null)
        {
            dialogueText.text = message;
        }
    }
}
