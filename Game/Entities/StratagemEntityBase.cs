﻿using K8055Velleman.Game.Systems;
using K8055Velleman.Game.Interfaces;
using K8055Velleman.Lib.CustomControls;
using System.Drawing;

namespace K8055Velleman.Game.Entities;

internal abstract class StratagemEntityBase : StaticEntity
{
    private int _actionSpeed = 1000;
    internal int Level = 1;
    internal abstract int UiID { get; }
    internal abstract bool Unlockable { get; }
    internal abstract int UnkockPrice { get; }
    internal abstract Color Color { get; }
    internal abstract string Name { get; }
    internal abstract int MaxLevel { get; }
    internal abstract int StartActionSpeed { get; }
    internal int ActionSpeed { get { return _actionSpeed; } set { _actionSpeed = value; timer.Interval = value; } }

    private PausableTimer timer;
    private bool action = false;

    internal override void OnCreate(EntitySystem entitySystem)
    {
        MainPanel = new()
        {
            Name = Name,
            BackColor = Color,
        };
        base.OnCreate(entitySystem);
        _actionSpeed = StartActionSpeed;
        //timer = new System.Timers.Timer(actionSpeed);
        //timer.Elapsed += (e, h) => { action = true; };
        //timer.AutoReset = true;
        timer = new(_actionSpeed);
        timer.Elapsed += (s, e) => { action = true; };
        DisableStratagem();
    }

    internal override void OnUpdate()
    {
        if(action)
        {
            action = false;
            Action();
        }
    }

    internal abstract void Action();

    internal virtual bool Upgrade(Upgrades upgrade) 
    {
        if (upgrade == Upgrades.ActionSpeed)
        {
            if (ActionSpeed <= 500) return false;
            ActionSpeed -= UpgradesValue.ActionSpeed;
        }
        Level++;
        return true;
    }

    internal void EnableStratagem()
    {
        enabled = true;
        timer.Enabled = true;
    }

    internal void DisableStratagem()
    {
        enabled = false;
        timer.Enabled = false;
    }

    internal void PauseStratagem()
    {
        enabled = false;
        timer.Pause();
    }

    internal void ResumeStratagem()
    {
        enabled = true;
        timer.Resume();
    }
}