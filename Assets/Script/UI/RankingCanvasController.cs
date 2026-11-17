using System.Linq;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;

public class RankingCanvasController : MonoBehaviour
{
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Sprite normalRow;
    [SerializeField] private Sprite goldRow;
    [SerializeField] private Sprite silverRow;
    [SerializeField] private Sprite bronzeRow;
    [SerializeField] private Sprite goldMedal;
    [SerializeField] private Sprite silverMedal;
    [SerializeField] private Sprite bronzeMedal;
    [SerializeField, Min(1)] private int maxVisiblePlayers = 100;

    private Transform myRankTransform;
    private bool refreshInProgress;

    private void Start()
    {
        FindMyRankRow();
        Refresh();
        Invoke(nameof(Refresh), 3f);
    }

    public void Refresh()
    {
        if (refreshInProgress) return;
        refreshInProgress = true;
        FirebaseCloudSaveService.LoadLeaderboard(entries =>
        {
            refreshInProgress = false;
            Render(entries);
        });
    }

    private void FindMyRankRow()
    {
        if (leaderboardContent == null)
        {
            Transform panel = transform.Find("LeaderBoardPanel");
            leaderboardContent = panel != null ? panel.Find("RankingScroll/Viewport/Content") : null;
        }
        if (leaderboardContent == null) return;

        Transform panelRoot = leaderboardContent.parent != null && leaderboardContent.parent.parent != null
            ? leaderboardContent.parent.parent.parent : null;
        if (panelRoot == null) return;
        foreach (Image image in panelRoot.GetComponentsInChildren<Image>(true))
        {
            if (image.sprite != null && image.sprite.name.ToLowerInvariant().Contains("row_me"))
            {
                myRankTransform = image.transform;
                break;
            }
        }
    }

    private void Render(FirebaseCloudSaveService.LeaderboardEntry[] entries)
    {
        if (leaderboardContent == null || rowPrefab == null) return;
        if (entries == null) entries = System.Array.Empty<FirebaseCloudSaveService.LeaderboardEntry>();

        var sorted = entries.Where(e => e != null)
            .OrderByDescending(e => e.money)
            .ThenBy(e => e.username, System.StringComparer.OrdinalIgnoreCase)
            .ToArray();

        for (int i = leaderboardContent.childCount - 1; i >= 0; i--)
            Destroy(leaderboardContent.GetChild(i).gameObject);

        string currentUid = FirebaseAuth.DefaultInstance.CurrentUser != null
            ? FirebaseAuth.DefaultInstance.CurrentUser.UserId : string.Empty;
        int myIndex = System.Array.FindIndex(sorted, e => !string.IsNullOrEmpty(currentUid) && e.uid == currentUid);
        int count = Mathf.Min(sorted.Length, maxVisiblePlayers);
        for (int i = 0; i < count; i++)
        {
            FirebaseCloudSaveService.LeaderboardEntry entry = sorted[i];
            GameObject rowObject = Instantiate(rowPrefab, leaderboardContent, false);
            rowObject.name = "LeaderboardRow_" + (i + 1);
            LeaderboardRowUI row = rowObject.GetComponent<LeaderboardRowUI>();
            if (row == null) row = rowObject.AddComponent<LeaderboardRowUI>();
            row.ConfigureSprites(goldRow, silverRow, bronzeRow, normalRow, goldMedal, silverMedal, bronzeMedal);
            row.SetData(i + 1, entry.username, entry.money);
        }

        if (myRankTransform != null && myIndex >= 0)
        {
            LeaderboardRowUI myRow = myRankTransform.GetComponent<LeaderboardRowUI>();
            if (myRow == null) myRow = myRankTransform.gameObject.AddComponent<LeaderboardRowUI>();
            myRow.ConfigureSprites(goldRow, silverRow, bronzeRow, normalRow, goldMedal, silverMedal, bronzeMedal);
            myRow.SetData(myIndex + 1, sorted[myIndex].username, sorted[myIndex].money, true);
        }
    }
}
