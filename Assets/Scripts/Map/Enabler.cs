using UnityEngine;
using System;
using UnityEditor;


public class Enabler : Trigger
{
    public bool inverted;

    public override void Interact(bool activate)
    {
        gameObject.SetActive(inverted ? !activate : activate);
    }
}
