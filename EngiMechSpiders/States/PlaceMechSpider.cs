using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Engi.EngiWeapon
{
    public class PlaceMechSpider : PlaceTurret
    {
        public static GameObject masterPrefab;
        public override void OnEnter()
        {
            blueprintPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiWalkerTurretBlueprints.prefab").WaitForCompletion();
            wristDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiTurretWristDisplay.prefab").WaitForCompletion();
            turretMasterPrefab = masterPrefab;
            base.OnEnter();
        }
    }
}
