using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.AI;

public class FriendDistance : MonoBehaviour
{
    [Header("Distance UI Settings")]
    [SerializeField] public TextMeshProUGUI distanceText;
    [Header("Seconds UI Settings")]
    [SerializeField] public TextMeshProUGUI secondsText;
    [Header("References")]
    [SerializeField] private NavMeshFriends navMeshFriendsScript;

    NavMeshAgent agent;
    
    public float seconds = 0.0f;
    public float secondsToYard = 0.0f;
    public float distance = 0.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        distanceText.enabled = true;
        secondsText.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        TimerText();
    }

    public void TimerText()
    {
        seconds += Time.deltaTime; 
        secondsToYard += Time.deltaTime;
        secondsText.text = $"Seconds: {Mathf.CeilToInt(seconds)} ";
        if (secondsToYard >= 0.5f)
        {
            distance += 1.0f; 
            distanceText.text = $"Yards: {distance}";
            secondsToYard = 0.0f; 
        }
    }
}
