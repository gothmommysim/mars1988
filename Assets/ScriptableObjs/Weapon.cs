using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class Weapon : Item
{
    [Header("Weapon Attributes")]
    [Tooltip("Fire Rate in Rounds Per Minute")]
    public float fireRate = 0f;
    public float spread = 0f;
    public float recoilSpeed = 0f;
    public float recoilAngle = 0f;
    public float verticalRecoil = 0f;
    public float horizontalRecoil = 0f;
}
