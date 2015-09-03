﻿using UnityEngine;

public abstract class Vehicle : Killable
{
    [Header("Vehicle")]
    public float CameraDistance;

    public Vector3 CameraOffset;

    public delegate void OnVehicleDestroyedEvent();
    public event OnVehicleDestroyedEvent OnVehicleDestroyed;

    public abstract void Initialize();

    public abstract void SetMove(float forward, float strafe);

    public abstract void SetPitchYaw(float pitch, float yaw);

    public abstract Vector2 GetPitchYaw();

    public abstract void SetRun(bool value);

    public abstract bool IsRun();

    public abstract void SetAimAt(Vector3 position);

    public abstract void TriggerPrimaryWeapon();

    public abstract void ReleasePrimaryWeapon();

    private void OnDestroy()
    {
        if (OnVehicleDestroyed != null)
        {
            OnVehicleDestroyed();
        }
    }
}