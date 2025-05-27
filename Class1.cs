using System.Collections.Generic;
using System.Diagnostics;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Linq;
using Il2CppInterop.Runtime.Injection;
using BepInEx.Logging;
using Utils;
using Il2CppSystem.Collections.Generic;
using System.Reflection;
using Il2CppSystem;
using SystemIntPtr = System.IntPtr;
using Il2CppIntPtr = Il2CppSystem.IntPtr;
using Il2CppInterop.Runtime;
using IntPtr = System.IntPtr;
using CodeStage.AntiCheat.ObscuredTypes;
using static MirrorDungeonSelectThemeUIPanel.UIResources;
using UnityEngine.Networking;

namespace BaseMod
{

    public static class GlobalAppearance
    {

        public static string[] AppearanceOptions = new string[]
        {
        "10301_Donquixote_BaseAppearance",
        "10302_Donquixote_WAppearance",
        "10303_Donquixote_ShiAssoAppearance",
        "10304_Donquixote_NCorpAppearance",
        "10305_Donquixote_CinqAppearance",
        "10306_Donquixote_MiddleFingerAppearance",
        "10307_Donquixote_MeatLanternAppearance",
        "10308_Donquixote_SwordGroupAppearance",
        "10309_Donquixote_TCorpAppearance",
        "10310_Donquixote_DarkSanchoAppearance",
        "10311_Donquixote_Cinq_EastAppearance"
        };
        private static System.Collections.Generic.List<string> currentRoundOrder =
        new System.Collections.Generic.List<string>();
        private static int currentIndex;
        private static bool isFirstRound = true;
        private static System.Random random = new System.Random();
        private static int lastRound = -1;

        public static string CurrentAppearanceId { get; private set; }

        // 新增回合状态监控
        public static void UpdateRoundState(int currentRound)
        {
            // 检测到回合1且不是连续回合时触发重置
            if (currentRound == 1 && lastRound != 1)
            {
                Main.SharedLog.LogWarning("Reset!!!");
                ResetBattleState();
            }
            lastRound = 0;
        }

        private static void ResetBattleState()
        {
            isFirstRound = true;
            currentRoundOrder.Clear();
            currentIndex = 0;
            GenerateNewRound();
        }

        public static string GetNextAppearance()
        {
            if (currentIndex >= currentRoundOrder.Count)
            {
                GenerateNewRound();
            }

            CurrentAppearanceId = currentRoundOrder[currentIndex];
            currentIndex++;

            return CurrentAppearanceId;
        }

        private static void GenerateNewRound()
        {
            var tempList = new System.Collections.Generic.List<string>(AppearanceOptions);

            if (isFirstRound)
            {
                // 首轮固定第一个元素
                var fixedItem = tempList[0];
                var shuffled = tempList.Skip(1).ToList();
                Shuffle(shuffled);

                currentRoundOrder = new System.Collections.Generic.List<string> { fixedItem };
                currentRoundOrder.AddRange(shuffled);

                isFirstRound = false;
            }
            else
            {
                // 后续轮次完全随机
                Shuffle(tempList);
                currentRoundOrder = tempList;
            }

            currentIndex = 0;
        }

        private static void Shuffle<T>(System.Collections.Generic.IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
    internal class ChangeAppearanceTemp : MonoBehaviour
    {
        [HarmonyPatch]
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<ChangeAppearanceTemp>();
            harmony.PatchAll(typeof(ChangeAppearanceTemp));
        }

        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.ChangeAppearance),
        new[] { typeof(string), typeof(bool) })] 
        [HarmonyPrefix]
        private static bool BattleUnitView_ChangeAppearance(
        BattleUnitView __instance,
        ref string appearanceId,
        bool isMain)
        {
            // 记录原始ID
            Main.SharedLog.LogWarning(string.Format("sourse appearanceID: {0},__instance.unitModel.GetUnitID();{1}", appearanceId, __instance.unitModel.GetUnitID()));
            //BattleUnitModel unit = __instance.GetCurrentDuelViewer
            if (__instance.unitModel.GetUnitID() == 10301)
            {
                appearanceId = GlobalAppearance.CurrentAppearanceId;
            }
            // --- 强制修改外观ID ---
            
            Main.SharedLog.LogWarning(string.Format("new appearanceID: {0}", appearanceId));

            return true;
        }

        [HarmonyPatch(typeof(SkillModel), "OnEndTurn")]
        [HarmonyPostfix]
        private static void Postfix_SkillModel_OnEndTurn(BattleActionModel action, BATTLE_EVENT_TIMING timing, SkillModel __instance)
        {
            //long num = __instance.Pointer.ToInt64();
            //Main.SharedLog.LogWarning(string.Format("SkillModel _instance:{0} BattleActionModel action:{1},BATTLE_EVENT_TIMING timing:{2}", __instance.GetInformationID(), action.GetMainTarget().GetUnitID(), timing));
        }
        [HarmonyPatch(typeof(BattleUnitModel), "OnEndTurn")]
        [HarmonyPostfix]
        private static void Postfix_BattleUnitModel_OnEndTurn(BattleUnitModel __instance, BattleActionModel action, BATTLE_EVENT_TIMING timing)
        {
            //Main.SharedLog.LogWarning(string.Format("BattleUnitModel _instance:{0} BattleActionModel action:{1},BATTLE_EVENT_TIMING timing:{2}", __instance.GetUnitID(), action.GetMainTarget().GetUnitID(), timing));
        }
        [HarmonyPatch(typeof(PassiveDetail), "OnRoundStart_After_Event")]
        [HarmonyPostfix]
        private static void Postfix_PassiveDetail_OnRoundStart_After_Event(BATTLE_EVENT_TIMING timing, PassiveDetail __instance)
        {
            

            Main.SharedLog.LogWarning(string.Format("PassiveDetail_OnRoundStart_After_Event:{0}", timing));
            Il2CppSystem.Collections.Generic.List<BattleUnitModel> list = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
            Il2CppSystem.Collections.Generic.List<BattleUnitModel> enemylist = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();
            BattleObjectManager battleObjectManager = Singleton<SinManager>.Instance._battleObjectManager;
            BattleUnitModel Don = null;
            foreach (BattleUnitModel battleUnitModel in battleObjectManager.GetModelList())
            {
                list.Add(battleUnitModel);
                
                if (battleUnitModel.GetUnitID() == 10301)
                {
                    Don = battleUnitModel;
                }
            }
            foreach (BattleUnitModel model in list)
            {
                if (model._passiveDetail.Equals(__instance)&& Don != null)
                {
                    int currentRound = Singleton<StageController>.Instance.GetCurrentRound();
                    
                    Main.SharedLog.LogWarning(string.Format("Unit ID:{0},Unit data{1},Don faction{2}", model.GetUnitID(), model.Faction, Don.Faction));
                    foreach (BattleUnitModel battleUnitModel in list)
                    {
                        if (battleUnitModel.Faction != Don.Faction)
                        {
                            enemylist.Add(battleUnitModel);
                        }
                    }
                    


                }
                    
            }
            PassiveAbility dummyPassiveAbility = null;
            foreach (PassiveModel passiveModel in Don.UnitDataModel.PassiveList)
            {
                dummyPassiveAbility = passiveModel._script;
            }
            if (dummyPassiveAbility != null && Don != null && Don._passiveDetail.Equals(__instance))
            {
                string newAppearance = GlobalAppearance.CurrentAppearanceId;
                
                switch (newAppearance)
                {
                    case "10301_Donquixote_BaseAppearance":
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.ParryingResultUp, 3, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.AttackDmgUp, 2, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        
                        break;

                    case "10302_Donquixote_WAppearance":
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Charge, 0, 9, 0, BATTLE_EVENT_TIMING.NONE, null);
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.SlashDamageUp, 2, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.SlashResultUp, 1, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        break;

                    case "10303_Donquixote_ShiAssoAppearance":
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Breath, 10, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        //dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Agility, 3, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        break;
                        
                    case "10304_Donquixote_NCorpAppearance":
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Assemble, 2, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        if (enemylist.Count > 0)
                        {
                            System.Random random = new System.Random();
                            for (int i = 0; i < 3; i++)
                            {
                                int randomIndex = random.Next(enemylist.Count);
                                BattleUnitModel selectedTarget = enemylist[randomIndex];
                                dummyPassiveAbility.GiveBuff_Self(selectedTarget, BUFF_UNIQUE_KEYWORD.NailPersonality, 1, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                            }
                        }
                        
                        break;

                    case "10305_Donquixote_CinqAppearance":
                        
                        //dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Agility, 6, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        if (enemylist.Count > 0)
                        {
                            System.Random random = new System.Random();
                            for (int i = 0; i < 1; i++)
                            {
                                int randomIndex = random.Next(enemylist.Count);
                                BattleUnitModel selectedTarget = enemylist[randomIndex];
                                dummyPassiveAbility.GiveBuff_Self(selectedTarget, BUFF_UNIQUE_KEYWORD.DuelDeclaration_DonQuixote, 1, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                            }
                        }
                        break;

                    case "10306_Donquixote_MiddleFingerAppearance":
                        
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.VioletResultUp, 2, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.VioletDamageUp, 2, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        if (enemylist.Count > 0)
                        {
                            System.Random random = new System.Random();

                            // 创建索引列表用于随机选择
                            var indexes = new Il2CppSystem.Collections.Generic.List<int>();
                            for (int i = 0; i < enemylist.Count; i++)
                            {
                                indexes.Add(i);
                            }

                            // Fisher-Yates洗牌算法
                            for (int i = indexes.Count - 1; i > 0; i--)
                            {
                                int j = random.Next(i + 1);
                                (indexes[i], indexes[j]) = (indexes[j], indexes[i]);
                            }

                            // 计算实际施加次数（最多2次）
                            int applyCount = Mathf.Min(2, enemylist.Count);

                            // 使用洗牌后的索引施加buff
                            for (int i = 0; i < applyCount; i++)
                            {
                                BattleUnitModel selectedTarget = enemylist[indexes[i]];
                                dummyPassiveAbility.GiveBuff_Self(
                                    selectedTarget,
                                    BUFF_UNIQUE_KEYWORD.RetaliationBook,
                                    1,  // stack
                                    0,  // turn
                                    0,  // param4
                                    BATTLE_EVENT_TIMING.NONE,
                                    null
                                );
                            }
                        }
                        // 中指形态逻辑
                        break;

                    case "10307_Donquixote_MeatLanternAppearance":
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.ParryingResultUp, 1, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.DefenseUp, 6, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        if (enemylist.Count > 0)
                        {
                            System.Random random = new System.Random();

                            // 创建索引列表用于随机选择
                            var indexes = new Il2CppSystem.Collections.Generic.List<int>();
                            for (int i = 0; i < enemylist.Count; i++)
                            {
                                indexes.Add(i);
                            }

                            // Fisher-Yates洗牌算法
                            for (int i = indexes.Count - 1; i > 0; i--)
                            {
                                int j = random.Next(i + 1);
                                (indexes[i], indexes[j]) = (indexes[j], indexes[i]);
                            }

                            // 计算实际施加次数（最多3次）
                            int applyCount = Mathf.Min(3, enemylist.Count);

                            // 使用洗牌后的索引施加buff
                            for (int i = 0; i < applyCount; i++)
                            {
                                BattleUnitModel selectedTarget = enemylist[indexes[i]];
                                dummyPassiveAbility.GiveBuff_Self(
                                    selectedTarget,
                                    BUFF_UNIQUE_KEYWORD.Burst,
                                    6,  // stack
                                    0,  // turn
                                    0,  // param4
                                    BATTLE_EVENT_TIMING.NONE,
                                    null
                                );
                            }
                        }
                        break;

                    case "10308_Donquixote_SwordGroupAppearance":
        
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.SlashDamageUp, 2, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Breath, 0, 10, 0, BATTLE_EVENT_TIMING.NONE, null); 
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.BladeResultUpTier1, 1, 0, 0, BATTLE_EVENT_TIMING.NONE, null); 
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.SlashResultUp, 1, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        // 剑契形态逻辑
                        break;

                    case "10309_Donquixote_TCorpAppearance":
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.DefenseUp, 6, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Protection, 1, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Vibration, 0, 10, 0, BATTLE_EVENT_TIMING.NONE, null);
                        if (enemylist.Count > 0)
                        {
                            System.Random random = new System.Random();

                            // 创建索引列表用于随机选择
                            var indexes = new Il2CppSystem.Collections.Generic.List<int>();
                            for (int i = 0; i < enemylist.Count; i++)
                            {
                                indexes.Add(i);
                            }

                            // Fisher-Yates洗牌算法
                            for (int i = indexes.Count - 1; i > 0; i--)
                            {
                                int j = random.Next(i + 1);
                                (indexes[i], indexes[j]) = (indexes[j], indexes[i]);
                            }

                            // 计算实际施加次数（最多3次）
                            int applyCount = Mathf.Min(2, enemylist.Count);

                            // 使用洗牌后的索引施加buff
                            for (int i = 0; i < applyCount; i++)
                            {
                                BattleUnitModel selectedTarget = enemylist[indexes[i]];
                                dummyPassiveAbility.GiveBuff_Self(
                                    selectedTarget,
                                    BUFF_UNIQUE_KEYWORD.Vibration,
                                    6,  // stack
                                    0,  // turn
                                    0,  // param4
                                    BATTLE_EVENT_TIMING.NONE,
                                    null
                                );
                            }
                        }
                        // T公司形态逻辑
                        break;

                    case "10310_Donquixote_DarkSanchoAppearance":
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.BloodArmorPersonalityFirst, 10, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                        if (enemylist.Count > 0)
                        {
                            System.Random random = new System.Random();

                            // 创建索引列表用于随机选择
                            var indexes = new Il2CppSystem.Collections.Generic.List<int>();
                            for (int i = 0; i < enemylist.Count; i++)
                            {
                                indexes.Add(i);
                            }

                            // Fisher-Yates洗牌算法
                            for (int i = indexes.Count - 1; i > 0; i--)
                            {
                                int j = random.Next(i + 1);
                                (indexes[i], indexes[j]) = (indexes[j], indexes[i]);
                            }

                            // 计算实际施加次数（最多3次）
                            int applyCount = Mathf.Min(2, enemylist.Count);

                            // 使用洗牌后的索引施加buff
                            for (int i = 0; i < applyCount; i++)
                            {
                                BattleUnitModel selectedTarget = enemylist[indexes[i]];
                                dummyPassiveAbility.GiveBuff_Self(
                                    selectedTarget,
                                    BUFF_UNIQUE_KEYWORD.Laceration,
                                    6,  // stack
                                    0,  // turn
                                    0,  // param4
                                    BATTLE_EVENT_TIMING.NONE,
                                    null
                                );
                            }
                        }
                        break;

                    case "10311_Donquixote_Cinq_EastAppearance":
                        dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Breath, 0, 5, 0, BATTLE_EVENT_TIMING.NONE, null);
                        //dummyPassiveAbility.GiveBuff_Self(Don, BUFF_UNIQUE_KEYWORD.Agility, 3, 0, 0, BATTLE_EVENT_TIMING.NONE, null);

                        if (enemylist.Count > 0)
                        {
                            System.Random random = new System.Random();
                            for (int i = 0; i < 3; i++)
                            {
                                int randomIndex = random.Next(enemylist.Count);
                                BattleUnitModel selectedTarget = enemylist[randomIndex];
                                dummyPassiveAbility.GiveBuff_Self(selectedTarget, BUFF_UNIQUE_KEYWORD.DianxueDonQuixote, 1, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                            }
                        }
                        break;

                    default:
                        // 未知形态处理
                        break;
                }
                


            }



            return;
        }
        [HarmonyPatch(typeof(BattleUnitModel), "OnRoundStart_Before")]
        [HarmonyPrefix]
        private static void OnRoundStart_Before(BattleUnitModel __instance)
        {


            if (__instance.GetUnitID() == 10301)
            {
                string newAppearance = GlobalAppearance.GetNextAppearance();
                Main.SharedLog.LogWarning(string.Format("changing!!!"));
                Main.SharedLog.LogWarning(string.Format("current Appearance:{0}", GlobalAppearance.CurrentAppearanceId));
                SingletonBehavior<BattleObjectManager>.Instance.GetView(__instance).ChangeAppearance(newAppearance, true);

                Il2CppSystem.Collections.Generic.List<int> defenseIdList = __instance.UnitDataModel._defenseSkillIDList;
                System.Text.StringBuilder idBuilder = new System.Text.StringBuilder();

                // 使用索引判断替代状态标记
                for (int i = 0; i < defenseIdList.Count; i++)
                {
                    idBuilder.Append(defenseIdList[i]);
                    if (i < defenseIdList.Count - 1)
                    {
                        idBuilder.Append(", ");
                    }
                }
                Main.SharedLog.LogWarning(string.Format("unitKeywordList:{0}", idBuilder));
                __instance.UnitDataModel._defenseSkillIDList.Clear();
                __instance.UnitDataModel._defenseSkillIDList.Add(int.Parse(GlobalAppearance.CurrentAppearanceId.Substring(0, 5)) * 100 + 4);
                defenseIdList = __instance.UnitDataModel._defenseSkillIDList;
                idBuilder = new System.Text.StringBuilder();

                // 使用索引判断替代状态标记
                for (int i = 0; i < defenseIdList.Count; i++)
                {
                    idBuilder.Append(defenseIdList[i]);
                    if (i < defenseIdList.Count - 1)
                    {
                        idBuilder.Append(", ");
                    }
                }
                Main.SharedLog.LogWarning(string.Format("adder unitKeywordList:{0}", idBuilder));
                __instance.UnitDataModel.PassiveList.Clear();
                __instance.GetPassiveList().Clear();
                __instance.AddPassive(int.Parse(GlobalAppearance.CurrentAppearanceId.Substring(0, 5)) * 100 + 1);
                if (int.Parse(GlobalAppearance.CurrentAppearanceId.Substring(0, 5)) == 10310)
                {
                    __instance.AddPassive(1031002);
                    __instance.AddPassive(1031003);
                    __instance.AddPassive(1031004);
                }
                if (int.Parse(GlobalAppearance.CurrentAppearanceId.Substring(0, 5)) == 10306)
                {
                    __instance.GetPassiveList().Clear();
                    __instance.AddPassive(1030611);
                }
                if (int.Parse(GlobalAppearance.CurrentAppearanceId.Substring(0, 5)) == 10307)
                {
                    __instance.GetPassiveList().Clear();
                    __instance.AddPassive(1030711);
                }
                if (int.Parse(GlobalAppearance.CurrentAppearanceId.Substring(0, 5)) == 10308)
                {
                    __instance.GetPassiveList().Clear();
                    __instance.AddPassive(1030811);
                }
                if (int.Parse(GlobalAppearance.CurrentAppearanceId.Substring(0, 5)) == 10311)
                {
                    __instance.GetPassiveList().Clear();
                    __instance.AddPassive(1031111);
                }


                PassiveAbility dummyPassiveAbility = null;
                foreach (PassiveModel passiveModel in __instance.UnitDataModel.PassiveList)
                {
                    dummyPassiveAbility = passiveModel._script;
                }
                if (dummyPassiveAbility != null && __instance != null)
                {
                    switch (newAppearance)
                    {
                        case "10303_Donquixote_ShiAssoAppearance":
                            dummyPassiveAbility.GiveBuff_Self(__instance, BUFF_UNIQUE_KEYWORD.Agility, 3, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                            break;
                        case "10305_Donquixote_CinqAppearance":

                            dummyPassiveAbility.GiveBuff_Self(__instance, BUFF_UNIQUE_KEYWORD.Agility, 6, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                            break;
                        case "10311_Donquixote_Cinq_EastAppearance":
                            dummyPassiveAbility.GiveBuff_Self(__instance, BUFF_UNIQUE_KEYWORD.Agility, 3, 0, 0, BATTLE_EVENT_TIMING.NONE, null);
                            break;
                        default:
                            // 未知形态处理
                            break;
                    }




                }
            }
        }
        [HarmonyPatch(typeof(StageController), "StartRoundAfterAbnormalityChoice_Init")]
        [HarmonyPostfix]
        private static void Postfix_StageController_StartRoundAfterAbnormalityChoice_Init()
        {
            Il2CppSystem.Collections.Generic.List<BattleUnitModel> list = new Il2CppSystem.Collections.Generic.List<BattleUnitModel>();

            BattleObjectManager battleObjectManager = Singleton<SinManager>.Instance._battleObjectManager;
            foreach (BattleUnitModel battleUnitModel in battleObjectManager.GetModelList())
            {
                list.Add(battleUnitModel);
            }
            foreach (BattleUnitModel model in list)
            {
                if (model.GetUnitID() == 10301)
                {
                    // 提取当前外观ID的前5位数字
                    int appearanceBase = int.Parse(GlobalAppearance.CurrentAppearanceId.Substring(0, 5));

                    Il2CppSystem.Collections.Generic.List<SinActionModel> sinActionList3 = model.GetSinActionList();
                    foreach (SinActionModel sinActionModel3 in sinActionList3)
                    {
                        foreach (SkillModel skillModel in sinActionModel3.UnitModel.GetSkillList())
                        {
                            int id = skillModel.GetID();
                            // 获取技能ID的最后两位
                            int skillSuffix = id % 100;
                            // 组合成7位数字
                            int combinedID = appearanceBase * 100 + skillSuffix;

                            sinActionModel3.ReplaceSkillAtoB(id, combinedID);
                        }
                    }
                }
            }
            return;
        }
        [HarmonyPatch(typeof(BattleUnitModel), "Init")]
        [HarmonyPostfix]
        private static void Postfix_BattleUnitModel_Init(BattleUnitModel __instance, UnitDataModel unitmodel, int instanceID)
        {
            Main.SharedLog.LogWarning(string.Format("BattleUnitModel init ID:{0}",__instance.GetUnitID()));
            if (__instance.GetUnitID() == 10301)
            {
                GlobalAppearance.UpdateRoundState(1);
            }
        }



        [HarmonyPatch(typeof(SinActionModel_Player), "EquipDefenseSin")]
        [HarmonyPostfix]
        private static void Prefix_UnEquipDefenseSin(SinActionModel_Player __instance, UnitSinModel __state)
        {
            Main.SharedLog.LogWarning(string.Format("before chaning defence skill!!!{0}",__instance._replacedSinByDefenseSkill.SkillID));
            // 获取SkillID字段的私有引用
            var skillIdField = typeof(UnitSinModel).GetField(
                "SkillID",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // 强制修改值
            skillIdField?.SetValue(__instance._replacedSinByDefenseSkill, 1031004);
            Main.SharedLog.LogWarning(string.Format("after chaning defence skill!!!{0}", __instance._replacedSinByDefenseSkill.SkillID));

            //UnitSinModel replacedSinByDefenseSkill = __instance._replacedSinByDefenseSkill;
            //__instance._replacedSinByDefenseSkill = new UnitSinModel(1031004, __instance.UnitModel, __instance, false);
            //__instance.SelectSin(new UnitSinModel(1031004, __instance.UnitModel, __instance, false));
        }
        




    }

        


    [BepInPlugin(Guid, Name, Version)]
    public class Main : BasePlugin
    {
        public const string Guid = Author + "." + Name;
        public const string Name = "KaleidoDonQuixote";
        public const string Version = "0.0.1";
        public const string Author = "Tintagedfish";
        public static ManualLogSource SharedLog;

        public override void Load()
        {
            // 打印日志信息
            SharedLog = Log;
            Harmony harmony = new Harmony("KaleidoDonQuixote");
            Log.LogInfo("Hello LimbusConpanay!!!This is KaleidoDonQuixoten MOD!!!");
            Log.LogWarning("This is Warning!!!From KaleidoDonQuixote MOD.");
            Harmony.CreateAndPatchAll(typeof(Main));
            ChangeAppearanceTemp.Setup(harmony);
        }
        [HarmonyPostfix, HarmonyPatch(typeof(BattleEffectManager), "OnDamageEffect_Base")]
        public static void BattleEffectManager_OnDamageEffect_Base(BattleEffectManager __instance, float damage, ATK_BEHAVIOUR attackType, string appearanceId, HashSet<UNIT_KEYWORD> keyword)
        {
            //string logMessage = $"dealDamage {damage} to {appearanceId}####attackType:{attackType}";
            //System.Diagnostics.Debug.WriteLine($"####{logMessage}####");
        }
    }
}