using BepInEx;
using EnemiesReturns;
using EngiMechSpiders.Modules;
using EntityStates;
using EntityStates.Engi.EngiWeapon;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.AddressableAssets;
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace EngiMechSpiders
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.ItemAPI.PluginGUID)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(EnemiesReturnsPlugin.GUID)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.Moffein.EngiMechSpiders", "EngiMechSpiders", "1.0.0")]
    public class EngiMechSpiderPlugin : BaseUnityPlugin
    {
        internal static PluginInfo pluginInfo;
        public static SkillDef MechSpiderSkillDef;
        public static SkillDef MechSpiderScepterSkillDef;
        public static ItemDef MechSpiderStatItem;
        public static ItemDef MechSpiderScepterItem;
        private static BodyIndex MechSpiderBodyIndex;
        private static MasterCatalog.MasterIndex MechSpiderMasterIndex;

        private void Awake()
        {
            pluginInfo = base.Info;
            CreateSkillDef();
            SetupScepter();
            CreateStatItem();
            RoR2Application.onLoad += OnLoad;
            Tokens.LoadLanguage();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EngiMechSpiders.mechspiderskillbundle"))
            {
                var assetBundle = AssetBundle.LoadFromStream(stream);
                MechSpiderSkillDef.icon = assetBundle.LoadAsset<Sprite>("skillMechSpider");
                if (MechSpiderScepterSkillDef) MechSpiderScepterSkillDef.icon = assetBundle.LoadAsset<Sprite>("skillMechSpiderScepter");
            }
        }

        private void OnLoad()
        {
            MechSpiderBodyIndex = BodyCatalog.FindBodyIndex("MechanicalSpiderBody");
            MechSpiderMasterIndex = MasterCatalog.FindMasterIndex("MechanicalSpiderMaster");
            PlaceMechSpider.masterPrefab = MasterCatalog.FindMasterPrefab("MechanicalSpiderMaster");
        }

        private void CreateSkillDef()
        {
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.skillName = "ENGI_SPECIAL_MECHSPIDER_NAME";
            skillDef.skillNameToken = "ENGI_SPECIAL_MECHSPIDER_NAME";
            skillDef.skillDescriptionToken = "ENGI_SPECIAL_MECHSPIDER_DESCRIPTION";
            skillDef.keywordTokens = new string[] { };
            (skillDef as ScriptableObject).name = skillDef.skillName;
            skillDef.activationState = new SerializableEntityStateType(typeof(PlaceMechSpider));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 2;
            skillDef.baseRechargeInterval = 30f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = false;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 0;

            SkillFamily skillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Engi/EngiBodySpecialFamily.asset").WaitForCompletion();
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };

            ContentAddition.AddSkillDef(skillDef);
            ContentAddition.AddEntityState(typeof(PlaceMechSpider), out bool wasAdded);

            MechSpiderSkillDef = skillDef;
        }

        private void SetupScepter()
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter")) return;

            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.skillName = "ENGI_SPECIAL_MECHSPIDER_SCEPTER_NAME";
            skillDef.skillNameToken = "ENGI_SPECIAL_MECHSPIDER_SCEPTER_NAME";
            skillDef.skillDescriptionToken = "ENGI_SPECIAL_MECHSPIDER_SCEPTER_DESCRIPTION";
            skillDef.keywordTokens = new string[] { };
            (skillDef as ScriptableObject).name = skillDef.skillName;
            skillDef.activationState = new SerializableEntityStateType(typeof(PlaceMechSpider));
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 2;
            skillDef.baseRechargeInterval = 30f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill;
            skillDef.isCombatSkill = false;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 0;
            ContentAddition.AddSkillDef(skillDef);
            MechSpiderScepterSkillDef = skillDef;

            CreateScepterItem();
            RegisterScepter();
        }

        private void CreateScepterItem()
        {
            MechSpiderScepterItem = ScriptableObject.CreateInstance<ItemDef>();
            MechSpiderScepterItem.canRemove = false;
            MechSpiderScepterItem.name = "MoffeinEngiMechSpiderScepterItem";
            MechSpiderScepterItem.deprecatedTier = ItemTier.NoTier;
            MechSpiderScepterItem.descriptionToken = "Stat modifier for Mech Spiders summoned by Engi with Scepter.";
            MechSpiderScepterItem.nameToken = "MoffeinEngiMechSpiderScepterItem";
            MechSpiderScepterItem.pickupToken = "Stat modifier for Mech Spiders summoned by Engi with Scepter.";
            MechSpiderScepterItem.hidden = true;
            MechSpiderScepterItem.pickupIconSprite = null;
            MechSpiderScepterItem.tags = new[]
            {
                ItemTag.WorldUnique,
                ItemTag.BrotherBlacklist,
                ItemTag.CannotSteal,
                ItemTag.CannotDuplicate,
                ItemTag.AIBlacklist
            };
            ItemDisplayRule[] idr = new ItemDisplayRule[0];
            ItemAPI.Add(new CustomItem(MechSpiderScepterItem, idr));
            RecalculateStatsAPI.GetStatCoefficients += MechSpiderScepterItem_Stats;
        }

        private void MechSpiderScepterItem_Stats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory && sender.inventory.GetItemCount(MechSpiderScepterItem) > 0)
            {
                args.damageMultAdd += 0.3f;
                args.healthMultAdd += 0.3f;
                args.regenMultAdd += 0.3f;
                args.armorAdd += 30f;
                args.moveSpeedMultAdd += 0.3f;
                args.attackSpeedMultAdd += 0.3f;
                args.critAdd += 0.3f;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void RegisterScepter()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(MechSpiderScepterSkillDef, "EngiBody", MechSpiderSkillDef);
        }

        private void CreateStatItem()
        {
            MechSpiderStatItem = ScriptableObject.CreateInstance<ItemDef>();
            MechSpiderStatItem.canRemove = false;
            MechSpiderStatItem.name = "MoffeinEngiMechSpiderStatItem";
            MechSpiderStatItem.deprecatedTier = ItemTier.NoTier;
            MechSpiderStatItem.descriptionToken = "Stat modifier for Mech Spiders summoned by Engi.";
            MechSpiderStatItem.nameToken = "MoffeinEngiMechSpiderStatItem";
            MechSpiderStatItem.pickupToken = "Stat modifier for Mech Spiders summoned by Engi.";
            MechSpiderStatItem.hidden = true;
            MechSpiderStatItem.pickupIconSprite = null;
            MechSpiderStatItem.tags = new[]
            {
                ItemTag.WorldUnique,
                ItemTag.BrotherBlacklist,
                ItemTag.CannotSteal,
                ItemTag.CannotDuplicate,
                ItemTag.AIBlacklist
            };
            ItemDisplayRule[] idr = new ItemDisplayRule[0];
            ItemAPI.Add(new CustomItem(MechSpiderStatItem, idr));

            RecalculateStatsAPI.GetStatCoefficients += MechSpiderStatItem_Stats;
            RoR2.CharacterBody.onBodyInventoryChangedGlobal += FixDeathState;

            On.RoR2.CharacterMaster.AddDeployable += CharacterMaster_AddDeployable;
        }

        private void FixDeathState(CharacterBody body)
        {
            if (body.bodyIndex != MechSpiderBodyIndex || body.inventory.GetItemCount(MechSpiderStatItem) <= 0) return;
            CharacterDeathBehavior cdb = body.GetComponent<CharacterDeathBehavior>();
            if (cdb && cdb.deathState.stateType != typeof(EnemiesReturns.ModdedEntityStates.MechanicalSpider.Death.DeathNormal)){
                cdb.deathState = new SerializableEntityStateType(typeof(EnemiesReturns.ModdedEntityStates.MechanicalSpider.Death.DeathNormal));
            }
        }

        private void MechSpiderStatItem_Stats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory && sender.inventory.GetItemCount(MechSpiderStatItem) > 0)
            {
                args.baseRegenAdd += 1f;
                args.levelRegenAdd += 0.2f;

                args.baseDamageAdd += 18f;
                args.levelDamageAdd += 3.6f;

                //These don't sprint, so up the speed from 9m/s to 10.15m/s to match player base sprint speed.
                args.baseMoveSpeedAdd += 1.15f;

                sender.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            }
        }

        private void CharacterMaster_AddDeployable(On.RoR2.CharacterMaster.orig_AddDeployable orig, CharacterMaster self, Deployable deployable, DeployableSlot slot)
        {
            orig(self, deployable, slot);

            if (slot == DeployableSlot.EngiTurret && deployable)
            {
                CharacterMaster deployableMaster = deployable.GetComponent<CharacterMaster>();
                if (deployableMaster)
                {
                    CharacterBody body = deployableMaster.GetBody();
                    if (body && body.bodyIndex == MechSpiderBodyIndex)
                    {
                        //Jank
                        CharacterBody droneVersion = EnemiesReturns.Enemies.MechanicalSpider.MechanicalSpiderDroneBody.BodyPrefab.GetComponent<CharacterBody>();
                        body.portraitIcon = droneVersion.portraitIcon;

                        Inventory inv = deployableMaster.inventory;
                        if (inv)
                        {
                            if (inv.GetItemCount(MechSpiderStatItem) <= 0) inv.GiveItem(MechSpiderStatItem);
                            if (inv.GetItemCount(RoR2Content.Items.MinionLeash) <= 0) inv.GiveItem(RoR2Content.Items.MinionLeash);

                            //Check if owner has scepter
                            //Too lazy to code something unique
                            if (deployable.ownerMaster && deployable.ownerMaster.inventory && MechSpiderScepterItem != null)
                            {
                                ItemIndex scepterIndex = ItemCatalog.FindItemIndex("ITEM_ANCIENT_SCEPTER");
                                if (scepterIndex != ItemIndex.None && deployable.ownerMaster.inventory.GetItemCount(scepterIndex) > 0)
                                {
                                    inv.GiveItem(MechSpiderScepterItem);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
