using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponsList", menuName = "Weapons/WeaponsList", order = 1)]
public class GameWeaponsList : ScriptableObject
{
    public List<WeaponData> gameWeaponsList;
}