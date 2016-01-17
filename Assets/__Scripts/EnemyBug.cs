using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EnemyBug : PT_MonoBehaviour, Enemy
{
    [SerializeField]
    private float _touchDamage = 1;
    public float touchDamage
    {
        get { return (_touchDamage); }
        set { _touchDamage = value; }
    }
    // The pos Property is already implemented in PT_MonoBehaviour
    public string typeString
    {
        get { return (roomXMLString); }
        set { roomXMLString = value; }
    }
    public string roomXMLString;
    public float speed = 0.5f;    public float health = 10; //**
    public float damageScale = 0.8f; //**
    public float damageScaleDuration = 0.25f; //**
    public bool ________________;

    private float damageScaleStartTime;
    private float _maxHealth; //**
    public Vector3 walkTarget;
    public bool walking;
    public Transform characterTrans;
    public Dictionary<ElementType, float> damageDict;
    // ^ NOTE: Dictionaries do not appear in the Unity Inspector
    void Awake()
    {
        characterTrans = transform.Find("CharacterTrans");
        _maxHealth = health; // Used to put a top cap on healing //**
        ResetDamageDict();
    }

    // Resets the values for the damageDict
    void ResetDamageDict()
    {
        if (damageDict == null)
        {
            damageDict = new Dictionary<ElementType, float>();
        }
        damageDict.Clear();
        damageDict.Add(ElementType.earth, 0);
        damageDict.Add(ElementType.water, 0);
        damageDict.Add(ElementType.air, 0);
        damageDict.Add(ElementType.fire, 0);
        damageDict.Add(ElementType.aether, 0);
        damageDict.Add(ElementType.none, 0);
    }

    void Update()
    {
        WalkTo(Mage.S.pos);
    }
    // ---------------- Walking Code ----------------
    // All of this walking code is copied directly from Mage
    // Walk to a specific position. The position.z is always 0
    public void WalkTo(Vector3 xTarget)
    {
        walkTarget = xTarget; // Set the point to walk to
        walkTarget.z = 0; // Force z=0
        walking = true; // Now the EnemyBug is walking
        Face(walkTarget); // Look in the direction of the walkTarget
    }
    public void Face(Vector3 poi)
    { // Face towards a point of interest
        Vector3 delta = poi - pos; // Find vector to the point of interest
                                   // Use Atan2 to get the rotation around Z that points the X-axis of
                                   // EnemyBug:CharacterTrans towards poi
        float rZ = Mathf.Rad2Deg * Mathf.Atan2(delta.y, delta.x);
        // Set the rotation of characterTrans (doesn't actually rotate Enemy)
        characterTrans.rotation = Quaternion.Euler(0, 0, rZ);
    }
    public void StopWalking()
    { // Stops the EnemyBug from walking
        walking = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
    void FixedUpdate()
    { // Happens every physics step (i.e., 50 times/second)
        if (walking)
        { // If EnemyBug is walking
            if ((walkTarget - pos).magnitude < speed * Time.fixedDeltaTime)
            {
                // If EnemyBug is very close to walkTarget, just stop there
                pos = walkTarget;
                StopWalking();
            }
            else
            {
                // Otherwise, move towards walkTarget
                GetComponent<Rigidbody>().velocity = (walkTarget - pos).normalized * speed;
            }
        }
        else
        {
            // If not walking, velocity should be zero
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    // Damage this instance. By default, the damage is instant, but it can also
    // be treated as damage over time, where the amt value would be the amount
    // of damage done every second.
    // NOTE: This same code can be used to heal the instance
    public void Damage(float amt, bool damageOverTime = false)
    {
        // If it's DOT, then only damage the fractional amount for this frame
        if (damageOverTime)
        {
            amt *= Time.deltaTime;
        }
        health -= amt;
        health = Mathf.Min(_maxHealth, health); // Limit health if healing
        if (health <= 0)
        {
            Die();
        }
    }
    // Making Die() a separate function allows us to add things later like
    // different death animations, dropping something for the player, etc.
    public void Die()
    {
        Destroy(gameObject);
    }
    // Damage this instance. By default, the damage is instant, but it can also
    // be treated as damage over time, where the amt value would be the amount
    // of damage done every second.
    // NOTE: This same code can be used to heal the instance
    public void Damage(float amt, ElementType eT, bool damageOverTime = false)
    {
        // If it's DOT, then only damage the fractional amount for this frame
        if (damageOverTime)
        {
            amt *= Time.deltaTime;
        }
        // Treat different damage types differently (most are default)
        switch (eT)
        {
            case ElementType.fire:
                // Only the max damage from one fire source affects this instance

                damageDict[eT] = Mathf.Max(amt, damageDict[eT]);
                break;
            case ElementType.air:
                // air doesn't damage EnemyBugs, so do nothing
                break;
            default:
                // By default, damage is added to the other damage by same element
                damageDict[eT] += amt;
                break;
        }
    }
    // LateUpdate() is automatically called by Unity every frame. Once all the
    // Updates() on all instances have been called, then LateUpdate() is called
    // on all instances.
    void LateUpdate()
    {
        // Apply damage from the different element types
        // Iteration through a Dictionary uses a KeyValuePair
        // entry.Key is the ElementType, while entry.Value is the float
        float dmg = 0;
        foreach (KeyValuePair<ElementType, float> entry in damageDict)
        {
            dmg += entry.Value;
        }

        if (dmg > 0)
        { // If this took damage... //**
          // and if it is at full scale now (& not already damage scaling)... //**
            if (characterTrans.localScale == Vector3.one)
            { //**
              // start the damage scale animation //**
                damageScaleStartTime = Time.time; //**
            }
        }

        // The damage scale animation //**
        float damU = (Time.time - damageScaleStartTime) / damageScaleDuration; //**
        damU = Mathf.Min(1, damU); // Limit the max localScale to 1 //**
        float scl = (1 - damU) * damageScale + damU * 1; //**
        characterTrans.localScale = scl * Vector3.one; //**

        health -= dmg;
        health = Mathf.Min(_maxHealth, health); // Limit health if healing
        ResetDamageDict(); // Prepare for next frame
        if (health <= 0)
        {
            Die();
        }
    }
}
