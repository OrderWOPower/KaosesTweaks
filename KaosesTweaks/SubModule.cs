﻿using HarmonyLib;
using KaosesTweaks.Behaviors;
using KaosesTweaks.Event;
using KaosesTweaks.Common;
using KaosesTweaks.Models;
using KaosesTweaks.Settings;
using KaosesTweaks.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Core.ItemObject;
using KaosesTweaks.BTTweaks;
using System.Text;
using System.Linq;

namespace KaosesTweaks
{
    public class SubModule : MBSubModuleBase
    {

        /* Another chance at marriage */

        public static Dictionary<Hero, CampaignTime> LastAttempts;
        public static readonly FastInvokeHandler RemoveUnneededPersuasionAttemptsHandler =
        HarmonyLib.MethodInvoker.GetHandler(AccessTools.Method(typeof(RomanceCampaignBehavior), "RemoveUnneededPersuasionAttempts"));
        private Harmony _harmony;
        /* Another chance at marriage */




        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            ConfigLoader.LoadConfig();
            bool modUsesHarmoney = Statics.UsesHarmony;
            if (modUsesHarmoney)
            {
                if (Kaoses.IsHarmonyLoaded())
                {
                    IM.DisplayModLoadedMessage();
                }
                else { IM.DisplayModHarmonyErrorMessage(); }
            }
            else { IM.DisplayModLoadedMessage(); }

        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            try
            {
                _harmony ??= new Harmony(Statics.HarmonyId);
                _harmony.PatchAll();
            }
            catch (Exception ex)
            {
                //Handle exceptions
                Logging.Lm("Error with harmony patch");
                Logging.Lm(ex.ToString());
                MessageBox.Show($"Error Initialising Bannerlord Tweaks:\n\n{ex.ToStringFull()}");
            }
        }



        public override void OnGameInitializationFinished(Game game)
        {
            // Called 4th after choosing (Resume Game, Campaign, Custom Battle) from the main menu.
            base.OnGameInitializationFinished(game);
            Campaign gameType = game.GameType as Campaign;
            if (!(gameType is Campaign))
            {
                return;
            }

            if (gameType != null)
            {
                MBReadOnlyList<ItemObject> ItemsList = gameType.Items;
                new KaosesItemTweaks(gameType.Items);
            }

            //~ BT

/*
            if (Campaign.Current != null && BannerlordTweaksSettings.Instance is { } settings && (settings.EnableMissingHeroFix && settings.PrisonerImprisonmentTweakEnabled))
            {

                try
                {
                    CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, delegate
                    {
                        PrisonerImprisonmentTweak.DailyTick();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Initialising Missing Hero Fix:\n\n{ex.ToStringFull()}");
                }
            }*/

            //~ BT

        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);


            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarter;

                #pragma warning disable CS8604 // Possible null reference argument.
                AddModels(campaignGameStarter);
                #pragma warning restore CS8604 // Possible null reference argument.


                PlayerBattleEndEventListener playerBattleEndEventListener = new PlayerBattleEndEventListener();
                CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(playerBattleEndEventListener, new Action<MapEvent>(playerBattleEndEventListener.IncreaseLocalRelationsAfterBanditFight));


                //~ BT

                try
                {
                    campaignGameStarter.AddBehavior(new ChangeSettlementCulture());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Initialising Culture Changer:\n\n{ex.ToStringFull()}");
                }

                //~BT

                /* Another chance at marriage */
                LastAttempts = new Dictionary<Hero, CampaignTime>();
                /* Another chance at marriage */
                campaignGameStarter.CampaignBehaviors.Add(new AnotherChanceBehavior());
                /* Another chance at marriage */


            }
        }


        public override bool DoLoading(Game game)
        {
            if (Campaign.Current != null && MCMSettings.Instance is { } settings)
            {
                if (settings.PrisonerImprisonmentTweakEnabled)
                    PrisonerImprisonmentTweak.Apply(Campaign.Current);
                if (settings.DailyTroopExperienceTweakEnabled)
                    DailyTroopExperienceTweak.Apply(Campaign.Current);
                // 1.5.7.2 - Disable until we understand main quest changes.
                //if (settings.TweakedConspiracyQuestTimerEnabled)
                //    BTConspiracyQuestTimerTweak.Apply(Campaign.Current);
            }
            return base.DoLoading(game);
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        public override void OnGameEnd(Game game)
        {

            /* Another chance at marriage */
            _harmony?.UnpatchAll(Statics.HarmonyId);
            /* Another chance at marriage */
        }


        //~ BT

        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            if (mission == null) return;
            base.OnMissionBehaviourInitialize(mission);
        }

        private void AddModels(CampaignGameStarter campaignGameStarter)
        {

            if (campaignGameStarter != null && MCMSettings.Instance is { } settings)
            {
                if (settings.MCMClanModifiers)
                {
                    campaignGameStarter.AddModel(new KaosesClanTierModel());
                }
                if (settings.MCMArmy)
                {
                    campaignGameStarter.AddModel(new KaosesArmyManagementCalculationModel());
                }
                if (settings.MCMBattleRewardModifiers)
                {
                    campaignGameStarter.AddModel(new KaosesBattleRewardModel());
                }
                if (settings.MCMCharacterDevlopmentModifiers)
                {
                    campaignGameStarter.AddModel(new KaosesCharacterDevelopmentModel());
                }
                if (settings.MCMPregnancyModifiers)
                {
                    campaignGameStarter.AddModel(new KaosesPregnancyModel());
                }
                if (settings.MCMSmithingModifiers)
                {
                    campaignGameStarter.AddModel(new KaosesSmithingModel());
                }
                if (settings.MCMSmithingModifiers)
                {
                    campaignGameStarter.AddModel(new KaosesMobilePartyFoodConsumptionModel());
                }
                if (settings.DifficultyTweakEnabled)
                {
                    campaignGameStarter.AddModel(new BTDifficultyModel());
                }
                if (settings.SettlementMilitiaEliteSpawnRateBonusEnabled)
                {
                    campaignGameStarter.AddModel(new BTSettlementMilitiaModel());
                }
                if (settings.AgeTweaksEnabled)
                {
                    BTAgeModel model = new();
                    List<string> configErrors = model.GetConfigErrors().ToList();
                    
                                        if (configErrors.Any())
                                        {
                                            StringBuilder sb = new();
                                            sb.AppendLine("There is a configuration error in the \'Age\' tweaks from Bannerlord Tweaks.");
                                            sb.AppendLine("Please check the below errors and fix the age settings in the settings menu:");
                                            sb.AppendLine();
                                            foreach (var e in configErrors)
                                                sb.AppendLine(e);
                                            sb.AppendLine();
                                            sb.AppendLine("The age tweaks will not be applied until these errors have been resolved.");
                                            sb.Append("Note that this is only a warning message and not a crash.");

                                            MessageBox.Show(sb.ToString(), "Configuration Error in Bannerlord Tweaks");
                                        }
                    else
                    {
                        campaignGameStarter.AddModel(new BTAgeModel());
                    }

                }
                if (settings.SiegeTweaksEnabled)
                {
                    campaignGameStarter.AddModel(new BTSiegeEventModel());
                }
                if (settings.MaxWorkshopCountTweakEnabled || settings.WorkshopBuyingCostTweakEnabled || settings.WorkshopEffectivnessEnabled)
                {
                    campaignGameStarter.AddModel(new BTWorkshopModel());
                }
                if (settings.MCMWorkShopModifiers)
                {
                    //campaignGameStarter.AddModel(new KaosesWorkshopModel());
                }









                //if (settings.TroopExperienceTweakEnabled || settings.ArenaHeroExperienceMultiplierEnabled || settings.TournamentHeroExperienceMultiplierEnabled)
                //campaignGameStarter.AddModel(new TweakedCombatXpModel());
                //


                //if (settings.PartiesLimitTweakEnabled || settings.CompanionLimitTweakEnabled || settings.BalancingPartyLimitTweaksEnabled)
                //gameStarter.AddModel(new TweakedClanTierModel());


                //if (settings.MCMPregnancyModifiers)
                //campaignGameStarter.AddModel(new TweakedPregnancyModel());
                //if (settings.AttributeFocusPointTweakEnabled)
                //campaignGameStarter.AddModel(new TweakedCharacterDevelopmentModel());
            }
        }



        protected override void OnApplicationTick(float dt)
        {
/*
            if (Campaign.Current != null && BannerlordTweaksSettings.Instance is { } settings2 && settings2.CampaignSpeed != 4)
            {
                Campaign.Current.SpeedUpMultiplier = settings2.CampaignSpeed;
            }*/
        }
        //~ BT




    }
}