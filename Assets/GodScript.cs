using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SUGARManager = PlayGen.SUGAR.Unity.SUGARManager;

public class GodScript : MonoBehaviour
{
    public GameObject selectGroup;

    public GameObject gameUI;

    public Text candyCoinCountText;

    long candyCoinBuffer;

    public long CandyCoinDisplay
    {
        get { return long.Parse(candyCoinCountText.text); }
        set { candyCoinCountText.text = value.ToString(); }
    }

    int syncInterval = 5;
    int syncCount = 1;
    const string candyCoinKey = "CandyCoin";

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
            CandyCoinDisplay = SUGARManager.Resource.GetFromCache(candyCoinKey);
            SetupGroup();
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
        CandyCoinDisplay++;
        candyCoinBuffer++;
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

                candyCoinBuffer = 0;
            }
        }
    }
}
