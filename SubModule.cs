using HarmonyLib;
using MCM.Abstractions;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.PerCampaign;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.KeepYourDaughters
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            var harmony = new Harmony("bannerlord.keepyourdaughters");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(DefaultMarriageModel), nameof(DefaultMarriageModel.GetClanAfterMarriage))]
        class Patch01
        {
            static void Postfix(ref Clan __result, Hero firstHero, Hero secondHero)
            {
                if (firstHero.Clan?.Leader == firstHero || secondHero.Clan?.Leader == secondHero)
                    return;

                if (firstHero.Clan != Hero.MainHero.Clan && secondHero.Clan != Hero.MainHero.Clan)
                    return;

                if (firstHero.Clan == Hero.MainHero.Clan && secondHero.Clan == Hero.MainHero.Clan)
                    return;

                Hero clanHero = firstHero.Clan == Hero.MainHero.Clan ? firstHero : secondHero;
                Hero giveHero = firstHero.Clan != Hero.MainHero.Clan ? firstHero : secondHero;

                if (Settings.Instance == null)
                    return;

                bool keep = (clanHero.IsFemale && Settings.Instance.KeepDaughters) || (!clanHero.IsFemale && !Settings.Instance.LoseSons);

                if (keep)
                    __result = clanHero.Clan;
                else
                    __result = giveHero.Clan;
            }
        }
    }

    internal sealed class Settings : AttributePerCampaignSettings<Settings>
    {
        public override string Id => "KeepYourDaughters";
        public override string FolderName => "KeepYourDaughters";
        public override string DisplayName => $"Keep Your Daughters {typeof(Settings).Assembly.GetName().Version.ToString(3)}";

        [SettingPropertyBool("Player Clan Keeps Daughters", RequireRestart = false, Order = 1)]
        [SettingPropertyGroup("Marriage Outcomes", GroupOrder = 1)]
        public bool KeepDaughters { get; set; } = true;

        [SettingPropertyBool("Player Clan Loses Sons", RequireRestart = false, Order = 2)]
        [SettingPropertyGroup("Marriage Outcomes", GroupOrder = 1)]
        public bool LoseSons { get; set; } = false;

        public override IEnumerable<ISettingsPreset> GetBuiltInPresets()
        {
            foreach (var preset in base.GetBuiltInPresets())
            {
                yield return preset;
            }

            yield return new MemorySettingsPreset(Id, "balanced", "Balanced", () => new Settings
            {
                KeepDaughters = true,
                LoseSons = true
            });

            yield return new MemorySettingsPreset(Id, "off", "Off", () => new Settings
            {
                KeepDaughters = false,
                LoseSons = false
            });
        }
    }
}