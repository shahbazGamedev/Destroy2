﻿using UnityEngine;

public class Soldier : Vehicle
{
    [Header("Movement")]
    public float ForwardSpeed = 5f;
    public float StrafeSpeed = 5f;

    [Header("Primary Weapon")]
    public VehicleGun PrimaryWeaponPrefab;
    public Transform[] PrimaryWeaponShootPoints;

    [Header("Corpse")]
    public Collider HeadCollider;
    public GameObject CorpsePrefab;

    private CharacterController controller;
    private VehicleGun primaryWeapon;

    // Movement
    private Vector3 move;
    private Vector3 velocity;
    private float fallSpeed;

    // Aiming
    private float pitchTarget;
    private float yawTarget;
    private Vector3 aimAt;

    // Locomotion
    private Animator meshAnimator;

    // Death
    private Vector3 killPosition;
    private Vector3 killDirection;
    private float killPower;

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        meshAnimator = GetComponent<Animator>();

        primaryWeapon = Instantiate(PrimaryWeaponPrefab).GetComponent<VehicleGun>();
        primaryWeapon.SetVelocityReference(new VelocityReference { Value = velocity });
        primaryWeapon.InitGun(PrimaryWeaponShootPoints, gameObject);
        primaryWeapon.SetClipRemaining(100);
        primaryWeapon.OnFinishReload += OnReloadPrimaryFinish;
    }

    public override void LiveUpdate()
    {
        fallSpeed += -9.81f * Time.deltaTime;
        velocity = Vector3.up * fallSpeed;
        controller.Move(velocity * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget, 0f), 25f * Time.deltaTime);
        // Locomotion
        meshAnimator.SetBool("IsAim", true);
        meshAnimator.SetFloat("Speed", move.z);
        meshAnimator.SetFloat("VerticalSpeed", move.z);
        meshAnimator.SetFloat("HorizontalSpeed", move.x);
    }

    public override void SetMove(float forward, float strafe)
    {
        move = new Vector3(strafe, 0f, forward);
        //velocity = Vector3.up*fallSpeed;
        //velocity = transform.right * move.x * StrafeSpeed + Vector3.up * fallSpeed + transform.forward * move.z * ForwardSpeed;
    }

    public override void SetAimAt(Vector3 position)
    {
        aimAt = position;
    }

    public override void SetPitchYaw(float pitch, float yaw)
    {
        pitchTarget -= pitch;
        yawTarget += yaw;
    }

    public override Vector2 GetPitchYaw()
    {
        return new Vector2(pitchTarget, yawTarget);
    }

    public override void TriggerPrimaryWeapon()
    {
        primaryWeapon.TriggerShoot(aimAt, 0f);
        if (primaryWeapon.GetClipRemaining() == 0)
        {
            primaryWeapon.ReleaseTriggerShoot();
            primaryWeapon.TriggerReload(primaryWeapon.ClipCapacity);
        }
    }

    public override void ReleasePrimaryWeapon()
    {
        primaryWeapon.ReleaseTriggerShoot();
    }

    private void OnReloadPrimaryFinish()
    {
        primaryWeapon.SetClipRemaining(primaryWeapon.ClipCapacity);
    }

    public override void Damage(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
    {
        killPosition = position;
        killDirection = direction;
        killPower = power;
        if (hitCollider == HeadCollider)
        {
            Debug.Log("HEAD DAMAGE!");
            ApplyDamage(damage * 10f, attacker);
        }
        else
        {
            base.Damage(hitCollider, position, direction, power, damage, attacker);
        }
    }

    public override void Damage(Collider hitCollider, Vector3 position, Vector3 direction, Missile missile)
    {
        killPosition = position;
        killDirection = direction;
        killPower = missile.GetPower();
        if (hitCollider == HeadCollider)
        {
            Debug.Log("HEADSHOT!");
            ApplyDamage(missile.GetDamage() * 10f, missile.GetOwner());
        }
        else
        {
            base.Damage(hitCollider, position, direction, missile);
        }
    }

    public override void Die(GameObject attacker)
    {
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var curCollider in colliders)
        {
            curCollider.enabled = false;
        }
        var corpse = (GameObject)Instantiate(CorpsePrefab, transform.position, transform.rotation);
        corpse.transform.parent = SceneManager.CorpseContainer;
        var corpseColliders = corpse.GetComponentsInChildren<Collider>();
        foreach (var corpseCollider in corpseColliders)
        {
            var killRay = new Ray(killPosition - 5f * killDirection.normalized, killDirection);
            RaycastHit killHit;
            if (corpseCollider.Raycast(killRay, out killHit, 10f))
            {
                Debug.Log("KILLED!");
                corpseCollider.attachedRigidbody.AddForceAtPosition(killDirection.normalized * killPower, killHit.point, ForceMode.Impulse);
            }
        }

        base.Die(attacker);
        Destroy(gameObject);
    }
}