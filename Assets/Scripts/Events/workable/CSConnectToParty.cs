﻿using UnityEngine;

public class CSConnectToParty : CSEvent
{
  [SerializeField] private string heroName;

  public override void OnEventAction()
  {
    if (heroName != "")
    {
      GameManager.GM.ConnectToParty(heroName);
    }
    else
      Debug.LogWarning("Не указано имя героя!");
  }
}
