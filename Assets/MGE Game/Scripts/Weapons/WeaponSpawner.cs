using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class WeaponSpawner : NetworkBehaviour
{
    [HideInInspector]
    public NetworkObject cameraEyes;

    [FishNet.CodeGenerating.ExcludeSerialization]

    [SerializeField] private GameObject[] weaponPrefabs;

    private BaseWeaponBehaviour[] weaponBehaviours = new BaseWeaponBehaviour[2];
    private NetworkObject[] weaponNetworkObjects = new NetworkObject[2];

    private int currentWeaponNum = 0;

    public override void OnStartClient() // Functions such as equipping are done on the client side, not server
    {
        if (!IsOwner)
            return;

        RequestSpawn(cameraEyes);

        base.OnStartClient();
    }

    public void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && weaponBehaviours[0] != null && weaponBehaviours[0].IsInitialized && !weaponBehaviours[0].IsEquipped)
        {
            EquipWeapon(0);
            UnequipWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && weaponBehaviours[0] != null && weaponBehaviours[1].IsInitialized && !weaponBehaviours[1].IsEquipped)
        {
            EquipWeapon(1);
            UnequipWeapon(0);
        }
    }

    [ServerRpc]
    public void RequestSpawn(NetworkObject root)
    {
        NetworkObject[] outWeapons = new NetworkObject[weaponPrefabs.Length];

        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            var weaponPrefab = weaponPrefabs[i];

            GameObject spawnedGO = Instantiate(weaponPrefab);

            NetworkObject spawnedNO = spawnedGO.GetComponent<NetworkObject>();
            spawnedNO.SetParent(root);
            ServerManager.Spawn(spawnedNO, Owner);

            outWeapons[i] = spawnedNO;
        }

        RecieveWeaponsTargetRpc(root.Owner, outWeapons);
    }

    [TargetRpc]
    public void RecieveWeaponsTargetRpc(NetworkConnection conn, NetworkObject[] weapons)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weaponNetworkObjects[i] = weapons[i];

            weaponBehaviours[i] = weapons[i].GetComponent<BaseWeaponBehaviour>();

            if (i == 0)
                weaponBehaviours[i].Equip();
        }
    }



    public void EquipWeapon(int weaponNum)
    {
        var weaponBehaviour = weaponBehaviours[weaponNum];

        weaponBehaviour.Equip();
        currentWeaponNum = weaponNum;
    }

    public void UnequipWeapon(int weaponNum)
    {
        var weaponBehaviour = weaponBehaviours[weaponNum];
        weaponBehaviour.Unequip();
    }
}