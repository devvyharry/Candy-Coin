using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayGen.SUGAR.Common;
using UnityEngine;
using UnityEngine.UI;
using SUGARManager = PlayGen.SUGAR.Unity.SUGARManager;

public class GodScript : MonoBehaviour
{
    public GameObject selectGroup;

    public GameObject gameUI;

    public Text candyCoinCountText;

    long candyCoinBuffer;
    long candyCoinComitted;
    
    int syncInterval = 5;
    int syncCount = 1;
    const string candyCoinKey = "CandyCoin";

    const string minerKey = "Miner";
    long minerCount;
    int minerCostMultiplier = 10;
    public Text minerCostDisplay;
    public Button hireMinerButton;

    // Use this for initialization
    void Start ()
	{
	    SUGARManager.Account.DisplayLogInPanel(OnLoggedIn);
    }

    void OnLoggedIn(bool success)
    {
        if (!success)
        {
            Debug.LogError("Failed to login");
        }
        else
        {
            candyCoinComitted = SUGARManager.Resource.GetFromCache(candyCoinKey);
            UpdateCandyCoinDisplay();
            SUGARManager.GameData.GetCumulative(minerKey, EvaluationDataType.Long, response =>
            {
                UpdateMinerCost();

                if (response != null)
                {
                    var minerCount = int.Parse(response.Value);
                    for (var i = 0; i < minerCount; i++)
                    {
                        AddMiner();
                    }
                }

                SetupGroup();
            });
        }
    }

    void SetupGroup()
    {
        if (SUGARManager.CurrentGroup == null)
        {
            selectGroup.SetActive(true);
        }
        else
        {
            OnGroupSetup();
        }
    }

    public void SelectGroup(Text buttonText)
    {
        selectGroup.SetActive(false);

        var selected = buttonText.text;

        var groups = SUGARManager.Client.Group.Get(selected);

        if (!groups.Any())
        {
            Debug.LogError($"No groups with name: {selected}");
        }
        else
        {
            var group = groups.First();
            SUGARManager.UserGroup.AddGroup(group.Id, false, true, OnGroupSetup);
        }
    }

    void OnGroupSetup(bool success = true)
    {
        if (!success)
        {
            Debug.LogError("Failed to setup group.");
        }
        else
        {
            gameUI.SetActive(true);
        }
    }

    public void AddCandyCoin()
    {
        candyCoinBuffer++;
        UpdateCandyCoinDisplay();
    }

    public void HireMiner()
    {
        hireMinerButton.enabled = false;

        var minerCost = Convert.ToInt64(Math.Pow(minerCostMultiplier, minerCount + 1));
        if (candyCoinComitted + candyCoinBuffer >= minerCost)
        {
            var committedDiff = candyCoinComitted;
            candyCoinComitted -= minerCost;
            if (candyCoinComitted < 0)
            {
                candyCoinBuffer += candyCoinComitted;
                candyCoinComitted = 0;
            }

            UpdateCandyCoinDisplay();

            SUGARManager.Resource.Add(candyCoinKey, - (committedDiff - candyCoinComitted), (success, value) =>
            {
                if (!success)
                {
                    Debug.LogError("Failed to debit candy coin for miner.");
                }
                else
                {
                    SUGARManager.GameData.Send(minerKey, 1, gdSuccess =>
                    {
                        if (gdSuccess)
                        {
                            AddMiner();
                        }
                    });
                }

                hireMinerButton.enabled = true;
            });
        }
        else
        {
            hireMinerButton.enabled = true;
        }
    }

    void AddMiner()
    {
        minerCount++;
        UpdateMinerCost();
        Debug.Log("Added miner");
        
    }

    void UpdateMinerCost()
    {
        var minerCost = Convert.ToInt64(Math.Pow(minerCostMultiplier, minerCount + 1));
        minerCostDisplay.text = $"Hire miner for: C {minerCost}.";
    }

    void UpdateCandyCoinDisplay()
    {
        candyCoinCountText.text = (candyCoinComitted + candyCoinBuffer).ToString();
    }

    void Update()
    {
        if (Time.time > syncInterval * syncCount)
        {
            syncCount++;

            if (candyCoinBuffer != 0)
            {
                SUGARManager.Resource.Add(candyCoinKey, candyCoinBuffer, (success, newValue) =>
                {
                    if (!success)
                    {
                        Debug.LogError($"Failed to update resource: {candyCoinBuffer}");
                    }
                });

                candyCoinComitted += candyCoinBuffer;
                candyCoinBuffer = 0;
            }
        }
    }
}
