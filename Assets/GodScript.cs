using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayGen.SUGAR.Unity;
using UnityEngine;
using UnityEngine.UI;
using SUGARManager = PlayGen.SUGAR.Unity.SUGARManager;

public class GodScript : MonoBehaviour
{
    public GameObject selectGroup;

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

    private void OnGroupSetup(bool success = false)
    {
        
    }
}
