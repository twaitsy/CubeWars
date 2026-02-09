using UnityEngine;
using UnityEngine.UI;

public class EconomyUI : MonoBehaviour
{
    [Header("Team IDs")]
    public int team1ID = 0;
    public int team2ID = 1;

    [Header("UI Text")]
    public Text team1GoldText;
    public Text team1FoodText;
    public Text team2GoldText;
    public Text team2FoodText;

    void Update()
    {
        UpdateTeamUI(team1ID, team1GoldText, team1FoodText);
        UpdateTeamUI(team2ID, team2GoldText, team2FoodText);
    }

    void UpdateTeamUI(int teamID, Text goldText, Text foodText)
    {
        int gold = TeamResources.Instance.GetResource(teamID, ResourceType.Gold);
        int food = TeamResources.Instance.GetResource(teamID, ResourceType.Food);

        goldText.text = $"Team {teamID + 1} Gold: {gold}";
        foodText.text = $"Team {teamID + 1} Food: {food}";
    }
}
